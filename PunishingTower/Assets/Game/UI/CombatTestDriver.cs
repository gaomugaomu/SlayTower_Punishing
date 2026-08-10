using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Core;
using PunishingTower.Data;
using PunishingTower.Enemy;
using PunishingTower.Relic;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for M4:
    /// battle driven by BattleManager + TurnManager state machine,
    /// squad (constructs) + commander, multi-enemy, selection switching (A/D constructs, W/S enemies),
    /// basic attack by selected construct, ultimate (R) with per-construct energy,
    /// leave/return demo for dynamic squad membership.
    /// </summary>
    public class CombatTestDriver : MonoBehaviour
    {
        [Header("Commander")]
        [SerializeField] private int commanderMaxHp = 100;
        [SerializeField] private int commanderMaxSerum = 3;

        [Header("Enemy - Infected Mechanical Unit")]
        [SerializeField] private int enemyMaxHp = 50;
        [SerializeField] private int enemyAttack = 10;
        [SerializeField] private int enemyDefenseShield = 8;

        [Header("Enemy - Defensive Machine")]
        [SerializeField] private int enemy2MaxHp = 70;
        [SerializeField] private int enemy2Attack = 7;
        [SerializeField] private int enemy2DefenseShield = 12;

        [Header("Boss - Corrupted Prototype (doc 208)")]
        [SerializeField] private int bossMaxHp = 300;
        [SerializeField] private int bossAttack = 15;
        [SerializeField] private int bossDefenseShield = 20;
        [SerializeField] private int bossCorruptInterval = 3;

        [Header("Player")]
        [SerializeField] private int actionPointsPerTurn = 3;

        private CommanderState commander;
        private BattleContext battle;
        private BattleManager manager;
        private SquadRuntime squad;
        private OrbPool orbPool;
        private OrbSkillController skillController;
        private CorruptionModifier corruptionModifier;
        private bool bossMode;
        private RelicSystem relicSystem;
        private RelicData greyRavenBadge;
        private RelicData selectedRelic;
        private readonly List<RelicData> relicPool = new List<RelicData>();
        private List<RewardEntry> victoryRewards;
        private readonly HashSet<int> claimedRewards = new HashSet<int>();
        private readonly HashSet<int> declinedRewards = new HashSet<int>();
        private readonly List<EnemyAiController> aiControllers = new List<EnemyAiController>();
        private readonly List<EnemyIntent> intents = new List<EnemyIntent>();
        private bool battleOver;
        private string lastMessage = string.Empty;
        private Vector2 scrollPos;

        private const KeyCode SelectNextConstructKey = KeyCode.D;
        private const KeyCode SelectPrevConstructKey = KeyCode.A;
        private const KeyCode SelectNextEnemyKey = KeyCode.S;
        private const KeyCode SelectPrevEnemyKey = KeyCode.W;
        private const KeyCode UltimateKey = KeyCode.R;
        private const KeyCode AttackKey = KeyCode.J;
        private const KeyCode EndTurnKey = KeyCode.E;
        private const KeyCode SerumKey = KeyCode.Q;
        private const KeyCode RelicKey = KeyCode.F;
        private const KeyCode LeaveKey = KeyCode.T;

        private void Start()
        {
            StartNewBattle();
        }

        private void StartNewBattle()
        {
            commander = new CommanderState(commanderMaxHp, commanderMaxSerum);
            battle = new BattleContext { Commander = commander };

            squad = new SquadRuntime(new[]
            {
                CreateConstruct("lucia", "露西亚", ConstructType.Attack, 6, 1, 100, 10,
                    SkillAssetFactory.LuciaRed(), SkillAssetFactory.LuciaBlue(), SkillAssetFactory.LuciaYellow(),
                    CorePassiveType.ThreeMatchNextRedBonus, 6),
                CreateConstruct("lee", "里", ConstructType.Attack, 5, 1, 100, 10,
                    SkillAssetFactory.LeeRed(), SkillAssetFactory.LeeBlue(), SkillAssetFactory.LeeYellow(),
                    CorePassiveType.None, 0),
                CreateConstruct("liv", "丽芙", ConstructType.Support, 3, 1, 100, 10,
                    SkillAssetFactory.LivRed(), SkillAssetFactory.LivBlue(), SkillAssetFactory.LivYellow(),
                    CorePassiveType.None, 0)
            });

            orbPool = BuildOrbPool();
            skillController = new OrbSkillController(orbPool);
            orbPool.Draw(8);
            Debug.Log($"[CombatTest] 初始手牌 {orbPool.HandCount} 球");

            // Grey Raven starting relic (doc 204): +1 energy on three match, team wide.
            // Equipped onto the squad leader (Lucia) by default; owner can be switched in the UI.
            if (relicSystem != null)
            {
                relicSystem.Shutdown();
            }
            relicSystem = new RelicSystem();
            relicSystem.Initialize(squad, battle);
            greyRavenBadge = RelicAssetFactory.GreyRavenBadge();
            relicPool.Clear();
            relicPool.Add(greyRavenBadge);
            relicPool.Add(CreateDefensiveCoreRelic());
            relicSystem.EquipRelic(squad.Members[0], greyRavenBadge);
            selectedRelic = greyRavenBadge;
            victoryRewards = null;

            if (corruptionModifier != null)
            {
                corruptionModifier.Shutdown();
                corruptionModifier = null;
            }

            if (bossMode)
            {
                // Boss battle: single Corrupted Prototype with orb corruption (doc 208).
                battle.AddEnemy(new EnemyState("boss", "Corrupted Prototype 腐化原型体", bossMaxHp, bossAttack, bossDefenseShield));
                corruptionModifier = new CorruptionModifier(orbPool, bossCorruptInterval);
            }
            else
            {
                battle.AddEnemy(new EnemyState("enemy_1", "Infected Mechanical Unit", enemyMaxHp, enemyAttack, enemyDefenseShield));
                battle.AddEnemy(new EnemyState("enemy_2", "Defensive Machine", enemy2MaxHp, enemy2Attack, enemy2DefenseShield));
            }

            aiControllers.Clear();
            intents.Clear();
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                aiControllers.Add(new EnemyAiController());
                intents.Add(null);
            }

            manager = new BattleManager(battle, ExecuteEnemyTurn, actionPointsPerTurn);
            battleOver = false;

            manager.StartBattle();
            PrepareIntents();
            manager.RevealIntents();
            manager.BeginPlayerTurn();

            Debug.Log($"[CombatTest] === 新战斗开始 Round {manager.Turns.Round} === {(bossMode ? "BOSS战: 腐化原型体" : "灰鸦小队")} | 敌人 {battle.Enemies.Count} | 指挥官 HP {commander.Hp}");
            LogTurnStart();
        }

        private static ConstructData CreateConstruct(string id, string displayName, ConstructType type,
            int attack, int energyGain, int maxEnergy, int ultDamage,
            SkillData redSkill, SkillData blueSkill, SkillData yellowSkill,
            CorePassiveType corePassive, int corePassiveValue)
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, displayName);
            data.AssignCombatStats(type, attack, energyGain, maxEnergy, ultDamage);
            data.AssignSkills(redSkill, blueSkill, yellowSkill);
            data.AssignCorePassive(corePassive, corePassiveValue);
#endif
            return data;
        }

        /// <summary>Initial pool: 5 red / 5 yellow / 5 blue, shuffled (doc 205).</summary>
        private static OrbPool BuildOrbPool()
        {
            var defs = new List<OrbData>();
            for (int i = 0; i < 5; i++) { defs.Add(CreateOrbData("red_" + i, OrbColor.Red)); }
            for (int i = 0; i < 5; i++) { defs.Add(CreateOrbData("yellow_" + i, OrbColor.Yellow)); }
            for (int i = 0; i < 5; i++) { defs.Add(CreateOrbData("blue_" + i, OrbColor.Blue)); }
            var pool = new OrbPool(defs);
            pool.ShuffleDrawPile();
            return pool;
        }

        private static OrbData CreateOrbData(string id, OrbColor color)
        {
            var data = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, id);
            data.AssignColor(color);
#endif
            return data;
        }

        /// <summary>Demo relic: turn-start shield, owner only (shows the relic-pool selection flow).</summary>
        private static RelicData CreateDefensiveCoreRelic()
        {
            var relic = ScriptableObject.CreateInstance<RelicData>();
#if UNITY_EDITOR
            relic.AssignIdentity("defensive_core", "防御核心");
            relic.AssignRelic(RelicTrigger.TurnStart, EffectType.Shield, 3, "每回合开始:装备者获得 3 点护盾", false);
#endif
            return relic;
        }

        // ---- Public state (read-only, for UI binding and automated tests) ----

        public bool IsBattleOver => battleOver;
        public int Round => manager.Turns.Round;
        public int CurrentActionPoints => manager.ActionPoints.ActionPoints;
        public int MaxActionPoints => manager.ActionPoints.MaxActionPoints;
        public int CommanderHp => commander.Hp;
        public int CommanderInfection => commander.Infection;
        public int SerumCount => commander.SerumCount;
        public int SquadCount => squad.Count;
        public int EnemyCount => battle.Enemies.Count;
        public int HandCount => orbPool.HandCount;
        public int DiscardCount => orbPool.DiscardCount;
        public string LastMessage => lastMessage;

        public EnemyState GetEnemy(int index) => battle.Enemies[index];
        public ConstructState GetConstruct(int index) => squad.Members[index];
        public string SelectedEnemyName => battle.SelectedEnemy != null ? battle.SelectedEnemy.DisplayName : string.Empty;
        public string SelectedConstructName => squad.Current != null ? squad.Current.DisplayName : string.Empty;
        public int GetSelectedConstructIndex() => squad.CurrentIndex;
        public int? GetCommanderShield() => commander != null ? commander.Shield : (int?)null;
        public List<OrbInstance> GetVisibleOrbs(int rowSize) => orbPool.GetVisibleRow(rowSize);

        public IReadOnlyList<RewardEntry> VictoryRewards => victoryRewards;

        // ---- Public actions (UI buttons / keyboard / tests) ----

        public void ActionBasicAttack() => BasicAttack();
        public void ActionUltimate() => UseUltimate();
        public void ActionUseSerum() => UseSerum();
        public void ActionEndTurn() => EndTurn();
        public void ActionRestart() => StartNewBattle();
        public void ActionToggleLeave() => ToggleConstructLeave();
        public void ActionSelectNextConstruct() => SelectNextConstruct();
        public void ActionSelectPrevConstruct() => SelectPrevConstruct();
        public void ActionSelectNextEnemy() => SelectNextEnemy();
        public void ActionSelectPrevEnemy() => SelectPrevEnemy();
        public void ActionPlayOrb(int slot) => PlayOrb(slot);

        /// <summary>Decides the intent of every alive enemy for the upcoming enemy turn.</summary>
        private void PrepareIntents()
        {
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                EnemyState enemy = battle.Enemies[i];
                intents[i] = enemy.IsDefeated
                    ? null
                    : aiControllers[i].DecideIntent(enemy, enemy.AttackDamage, enemy.DefenseShield);
            }
        }

        /// <summary>Injected into BattleManager: infection penalty then every enemy acts in order.</summary>
        private void ExecuteEnemyTurn(BattleContext ctx)
        {
            int penalty = commander.ApplyTurnStartPenalty();
            if (penalty > 0)
            {
                Debug.Log($"[CombatTest] 感染惩罚: -{penalty} HP | HP {commander.Hp}");
            }
            if (commander.IsDefeated)
            {
                manager.EndBattle(false);
                return;
            }

            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                EnemyState enemy = battle.Enemies[i];
                if (enemy.IsDefeated || intents[i] == null)
                {
                    continue;
                }
                string result = EnemyAiController.ExecuteIntent(enemy, intents[i], commander);
                Debug.Log($"[CombatTest] 敌人行动: {result} | 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
                lastMessage = result;
                if (commander.IsDefeated)
                {
                    manager.EndBattle(false);
                    return;
                }
            }
        }

        private void LogTurnStart()
        {
            Debug.Log($"[CombatTest] Round {manager.Turns.Round} 玩家回合 | AP {manager.ActionPoints.ActionPoints}/{manager.ActionPoints.MaxActionPoints} | 选中: {squad.Current.DisplayName} -> {battle.SelectedEnemy.DisplayName}");
        }

        private void Update()
        {
            if (battleOver)
            {
                return;
            }

            if (Input.GetKeyDown(SelectNextConstructKey)) { SelectNextConstruct(); }
            else if (Input.GetKeyDown(SelectPrevConstructKey)) { SelectPrevConstruct(); }
            else if (Input.GetKeyDown(SelectNextEnemyKey)) { SelectNextEnemy(); }
            else if (Input.GetKeyDown(SelectPrevEnemyKey)) { SelectPrevEnemy(); }
            else if (Input.GetKeyDown(UltimateKey)) { UseUltimate(); }
            else if (Input.GetKeyDown(AttackKey) || Input.GetKeyDown(KeyCode.Space)) { BasicAttack(); }
            else if (Input.GetKeyDown(EndTurnKey) || Input.GetKeyDown(KeyCode.Return)) { EndTurn(); }
            else if (Input.GetKeyDown(SerumKey)) { UseSerum(); }
            else if (Input.GetKeyDown(RelicKey)) { CycleRelicOwner(); }
            else if (Input.GetKeyDown(LeaveKey)) { ToggleConstructLeave(); }
        }

        private void SelectNextConstruct()
        {
            squad.SelectNext();
            Debug.Log($"[CombatTest] 切换到构造体: {squad.Current.DisplayName}");
            lastMessage = $"选中构造体: {squad.Current.DisplayName}";
        }

        private void SelectPrevConstruct()
        {
            squad.SelectPrevious();
            Debug.Log($"[CombatTest] 切换到构造体: {squad.Current.DisplayName}");
            lastMessage = $"选中构造体: {squad.Current.DisplayName}";
        }

        private void SelectNextEnemy()
        {
            battle.SelectNextEnemy();
            Debug.Log($"[CombatTest] 切换攻击目标: {battle.SelectedEnemy.DisplayName}");
            lastMessage = $"选中敌人: {battle.SelectedEnemy.DisplayName}";
        }

        private void SelectPrevEnemy()
        {
            battle.SelectPreviousEnemy();
            Debug.Log($"[CombatTest] 切换攻击目标: {battle.SelectedEnemy.DisplayName}");
            lastMessage = $"选中敌人: {battle.SelectedEnemy.DisplayName}";
        }

        /// <summary>
        /// Plays the visible orb at the given slot (0 = rightmost/newest) with the currently
        /// selected construct: resolves the play group, executes the orb skill, discards the orbs.
        /// </summary>
        private void PlayOrb(int slot)
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (squad.Current == null)
            {
                lastMessage = "无有效我方单位";
                return;
            }

            List<OrbInstance> visible = orbPool.GetVisibleRow(OrbTestDriver.VisibleRowSize);
            int visibleIndex = visible.Count - 1 - slot;
            if (visibleIndex < 0 || visibleIndex >= visible.Count)
            {
                return;
            }

            OrbInstance orb = visible[visibleIndex];
            if (orb.Locked)
            {
                Debug.Log($"[CombatTest] {OrbLabel(orb)} 是锁球,无法消球");
                lastMessage = "锁球无法消球";
                return;
            }

            var row = new List<PunishingTower.Core.ISignalOrb>(visible);
            OrbPlayGroup group = ThreeMatchDetector.ResolvePlay(row, visibleIndex);
            if (group == null)
            {
                lastMessage = "无法消球";
                return;
            }

            if (!manager.ActionPoints.CanSpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {manager.ActionPoints.ActionPoints})";
                Debug.Log($"[CombatTest] 消球失败: AP 不足 (剩余 {manager.ActionPoints.ActionPoints})");
                return;
            }

            string effectText = skillController.TryPlayGroup(group, squad.Current, battle, manager.ActionPoints);
            string colorName = ColorName(group.Color);
            lastMessage = $"{squad.Current.DisplayName} {group.MatchCount}消[{colorName}]: {effectText}";
            Debug.Log($"[CombatTest] {squad.Current.DisplayName} {group.MatchCount}消[{colorName}] | {effectText} | AP {manager.ActionPoints.ActionPoints}");

            CheckVictory();
        }

        private void BasicAttack()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (squad.Current == null)
            {
                lastMessage = "无有效我方单位";
                return;
            }

            // 当前目标已死亡时,自动切换到下一个存活敌人;没有存活目标则无法攻击。
            if (battle.SelectedEnemy == null || battle.SelectedEnemy.IsDefeated)
            {
                if (battle.AliveEnemyCount == 0)
                {
                    lastMessage = "没有可攻击的存活敌人";
                    Debug.Log("[CombatTest] 没有可攻击的存活敌人");
                    return;
                }
                battle.SelectNextEnemy();
                Debug.Log($"[CombatTest] 原目标已死亡,自动切换到 {battle.SelectedEnemy.DisplayName}");
            }

            if (!manager.ActionPoints.TrySpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {manager.ActionPoints.ActionPoints})";
                Debug.Log($"[CombatTest] 普攻失败: AP 不足 (剩余 {manager.ActionPoints.ActionPoints})");
                return;
            }

            ConstructState actor = squad.Current;
            EnemyState target = battle.SelectedEnemy;
            int damage = actor.BasicAttackDamage;
            int dealt = target.TakeDamage(damage);
            actor.AddEnergy(actor.BasicAttackEnergyGain);

            Debug.Log($"[CombatTest] {actor.DisplayName} 普攻 {target.DisplayName}: {dealt} 伤害 | 敌人 HP {target.Hp} 盾 {target.Shield} | {actor.DisplayName} 能量 {actor.Energy}/{actor.EnergyMax} | AP {manager.ActionPoints.ActionPoints}");
            lastMessage = $"{actor.DisplayName} 普攻 {dealt} 伤";

            // 击杀后自动选中下一个存活目标,便于连续输出。
            if (target.IsDefeated && !battle.AllEnemiesDefeated)
            {
                battle.SelectNextEnemy();
                Debug.Log($"[CombatTest] 目标被击杀,自动选中下一个目标: {battle.SelectedEnemy.DisplayName}");
                lastMessage += $" | 目标击杀,自动选中 {battle.SelectedEnemy.DisplayName}";
            }

            CheckVictory();
        }

        private void UseUltimate()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (squad.Current == null)
            {
                return;
            }

            ConstructState actor = squad.Current;
            if (!actor.IsEnergyFull)
            {
                lastMessage = $"{actor.DisplayName} 能量未满 ({actor.Energy}/{actor.EnergyMax}),无法释放大招";
                Debug.Log($"[CombatTest] 大招失败: {actor.DisplayName} 能量 {actor.Energy}/{actor.EnergyMax}");
                return;
            }
            if (!manager.ActionPoints.TrySpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {manager.ActionPoints.ActionPoints})";
                Debug.Log($"[CombatTest] 大招失败: AP 不足 (剩余 {manager.ActionPoints.ActionPoints})");
                return;
            }

            actor.TryConsumeUltimateEnergy();

            int totalDamage = 0;
            foreach (EnemyState enemy in battle.Enemies)
            {
                if (!enemy.IsDefeated)
                {
                    totalDamage += enemy.TakeDamage(actor.UltimateDamage);
                }
            }

            Debug.Log($"*** 大招释放 *** {actor.DisplayName} 对全体敌人造成 {totalDamage} 总伤害 | AP {manager.ActionPoints.ActionPoints}");
            lastMessage = $"{actor.DisplayName} 大招: 全体 {totalDamage} 伤";

            CheckVictory();
        }

        private void UseSerum()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (!commander.UseSerum())
            {
                lastMessage = "血清用完了";
                Debug.Log("[CombatTest] 血清用完了");
                return;
            }
            Debug.Log($"[CombatTest] 使用血清: 感染 {commander.Infection} 剩余血清 {commander.SerumCount}");
            lastMessage = $"血清使用: 感染 -> {commander.Infection}";
        }

        private void EndTurn()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }

            Debug.Log($"[CombatTest] --- 玩家回合结束 (Phase: {manager.Turns.Phase}) ---");
            manager.EndPlayerTurn();

            if (manager.IsOver)
            {
                battleOver = true;
                Debug.Log(manager.GetBattleState().Phase == BattlePhase.Victory
                    ? "*** 胜利 *** 所有敌人被击败!"
                    : $"*** 失败 *** 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
                lastMessage = manager.GetBattleState().Phase == BattlePhase.Victory ? "*** 胜利 ***" : "*** 失败 ***";
                return;
            }

            PrepareIntents();
            manager.RevealIntents();
            manager.BeginPlayerTurn();
            LogTurnStart();
        }

        private void ToggleConstructLeave()
        {
            if (battleOver || squad.Count == 0)
            {
                return;
            }
            ConstructState current = squad.Current;
            if (current.IsActive)
            {
                current.SetFlag(ConstructStateFlag.Unavailable);
                Debug.Log($"[CombatTest] {current.DisplayName} 暂时离队 (Unavailable)");
                lastMessage = $"{current.DisplayName} 离队";
                squad.SelectNext();
                Debug.Log($"[CombatTest] 当前选中切到: {squad.Current.DisplayName}");
            }
            else
            {
                current.SetFlag(ConstructStateFlag.Active);
                Debug.Log($"[CombatTest] {current.DisplayName} 归队 (Active)");
                lastMessage = $"{current.DisplayName} 归队";
            }
        }

        private void CheckVictory()
        {
            if (battle.AllEnemiesDefeated)
            {
                battleOver = true;
                manager.EndBattle(true);
                victoryRewards = RewardGenerator.GenerateNormalBattle(new System.Random());
                claimedRewards.Clear();
                declinedRewards.Clear();
                string rewardText = string.Join(" | ", victoryRewards.ConvertAll(r => r.ToString()));
                Debug.Log($"*** 胜利 *** 所有敌人被击败! 奖励: {rewardText} (请在下方逐项选择拿取/放弃)");
                lastMessage = $"*** 胜利 *** 请选择奖励";
            }
        }

        /// <summary>Applies the claimed reward: orb joins the deck, black cards and relics are recorded.</summary>
        private void ClaimReward(int index)
        {
            if (victoryRewards == null || index < 0 || index >= victoryRewards.Count)
            {
                return;
            }
            if (claimedRewards.Contains(index) || declinedRewards.Contains(index))
            {
                return;
            }

            RewardEntry reward = victoryRewards[index];
            claimedRewards.Add(index);

            switch (reward.type)
            {
                case RewardType.Orb:
                {
                    OrbData data = CreateOrbData("reward_orb_" + orbPool.TotalCount, reward.orbColor);
                    orbPool.AddOrb(data);
                    Debug.Log($"[CombatTest] 拿取奖励: {reward} -> 已加入牌组, 当前牌组 {orbPool.TotalCount} 球");
                    break;
                }
                case RewardType.Relic:
                    Debug.Log($"[CombatTest] 拿取奖励: {reward} -> 获得遗物(待实现遗物池)");
                    break;
                default:
                    Debug.Log($"[CombatTest] 拿取奖励: {reward}");
                    break;
            }
        }

        private void DeclineReward(int index)
        {
            if (victoryRewards == null || index < 0 || index >= victoryRewards.Count)
            {
                return;
            }
            if (claimedRewards.Contains(index) || declinedRewards.Contains(index))
            {
                return;
            }

            declinedRewards.Add(index);
            Debug.Log($"[CombatTest] 放弃奖励: {victoryRewards[index]}");
        }

        /// <summary>Counts locked (corrupted) orbs across draw/hand/discard.</summary>
        public int CountLockedOrbs()
        {
            int count = 0;
            foreach (OrbInstance orb in orbPool.DrawPile) { if (orb.Locked) count++; }
            foreach (OrbInstance orb in orbPool.Hand) { if (orb.Locked) count++; }
            foreach (OrbInstance orb in orbPool.Discard) { if (orb.Locked) count++; }
            return count;
        }

        /// <summary>Cycles the Grey Raven Badge to the next construct in the squad (key F).</summary>
        public void CycleRelicOwner()
        {
            if (greyRavenBadge == null || squad == null || squad.Count == 0)
            {
                return;
            }

            ConstructState currentOwner = relicSystem.GetRelicOwner(greyRavenBadge);
            int index = 0;
            for (int i = 0; i < squad.Members.Count; i++)
            {
                if (squad.Members[i] == currentOwner)
                {
                    index = i;
                    break;
                }
            }
            int next = (index + 1) % squad.Count;
            relicSystem.EquipRelic(squad.Members[next], greyRavenBadge);
            Debug.Log($"[CombatTest] 灰鸦徽章 装备到 {squad.Members[next].DisplayName}");
            lastMessage = $"灰鸦徽章 -> {squad.Members[next].DisplayName}";
        }

        private static string OrbLabel(OrbInstance orb)
        {
            string colorName = ColorName(orb.Color);
            string suffix = orb.Locked ? " [LOCKED]" : string.Empty;
            return $"[{colorName}]{suffix}";
        }

        private static string ColorName(int color)
        {
            switch (color)
            {
                case (int)OrbColor.Red: return "红";
                case (int)OrbColor.Yellow: return "黄";
                case (int)OrbColor.Blue: return "蓝";
                case (int)OrbColor.White: return "白";
                default: return "?";
            }
        }

        /// <summary>Toggles Boss mode and restarts the battle.</summary>
        public void ToggleBossMode()
        {
            bossMode = !bossMode;
            Debug.Log($"[CombatTest] Boss 模式: {(bossMode ? "开启 (腐化原型体)" : "关闭 (普通双敌人)")}");
            StartNewBattle();
        }
    }
}