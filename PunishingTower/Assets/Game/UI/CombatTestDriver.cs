using System.Collections.Generic;
using PunishingTower.Combat;
using PunishingTower.Construct;
using PunishingTower.Data;
using PunishingTower.Enemy;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for M3.5:
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

        [Header("Player")]
        [SerializeField] private int actionPointsPerTurn = 3;

        private CommanderState commander;
        private BattleContext battle;
        private ActionPointSystem ap;
        private SquadRuntime squad;
        private readonly List<EnemyAiController> aiControllers = new List<EnemyAiController>();
        private readonly List<EnemyIntent> intents = new List<EnemyIntent>();
        private int round;
        private bool battleOver;
        private string lastMessage = string.Empty;

        private const KeyCode SelectNextConstructKey = KeyCode.D;
        private const KeyCode SelectPrevConstructKey = KeyCode.A;
        private const KeyCode SelectNextEnemyKey = KeyCode.S;
        private const KeyCode SelectPrevEnemyKey = KeyCode.W;
        private const KeyCode UltimateKey = KeyCode.R;

        private void Start()
        {
            StartNewBattle();
        }

        private void StartNewBattle()
        {
            commander = new CommanderState(commanderMaxHp, commanderMaxSerum);
            battle = new BattleContext { Commander = commander };
            ap = new ActionPointSystem(actionPointsPerTurn);

            squad = new SquadRuntime(new[]
            {
                CreateConstruct("lucia", "露西亚", ConstructType.Attack, 6, 1, 100, 40),
                CreateConstruct("lee", "里", ConstructType.Attack, 5, 1, 100, 35),
                CreateConstruct("liv", "丽芙", ConstructType.Support, 3, 1, 100, 25)
            });

            battle.AddEnemy(new EnemyState("enemy_1", "Infected Mechanical Unit", enemyMaxHp));
            battle.AddEnemy(new EnemyState("enemy_2", "Defensive Machine", enemy2MaxHp));

            aiControllers.Clear();
            aiControllers.Add(new EnemyAiController());
            aiControllers.Add(new EnemyAiController());
            intents.Clear();
            intents.Add(null);
            intents.Add(null);

            round = 1;
            battleOver = false;

            StartPlayerTurn();
            Debug.Log($"[CombatTest] === 新战斗开始 Round {round} === 灰鸦小队 {squad.Count} 人 | 敌人 {battle.Enemies.Count} | 指挥官 HP {commander.Hp}");
        }

        private static ConstructData CreateConstruct(string id, string displayName, ConstructType type,
            int attack, int energyGain, int maxEnergy, int ultDamage)
        {
            var data = ScriptableObject.CreateInstance<ConstructData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, displayName);
            data.AssignCombatStats(type, attack, energyGain, maxEnergy, ultDamage);
#endif
            return data;
        }

        private void StartPlayerTurn()
        {
            ap.BeginTurn();
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                intents[i] = aiControllers[i].DecideIntent(battle.Enemies[i], i == 0 ? enemyAttack : enemy2Attack,
                    i == 0 ? enemyDefenseShield : enemy2DefenseShield);
            }
            Debug.Log($"[CombatTest] Round {round} 玩家回合 | AP {ap.ActionPoints}/{ap.MaxActionPoints} | 选中: {squad.Current.DisplayName} -> {battle.SelectedEnemy.DisplayName}");
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

        private void BasicAttack()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (squad.Current == null || battle.SelectedEnemy == null)
            {
                lastMessage = "无有效目标";
                return;
            }
            if (!ap.TrySpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {ap.ActionPoints})";
                Debug.Log($"[CombatTest] 普攻失败: AP 不足 (剩余 {ap.ActionPoints})");
                return;
            }

            ConstructState actor = squad.Current;
            EnemyState target = battle.SelectedEnemy;
            int damage = actor.BasicAttackDamage;
            int dealt = target.TakeDamage(damage);
            actor.AddEnergy(actor.BasicAttackEnergyGain);

            Debug.Log($"[CombatTest] {actor.DisplayName} 普攻 {target.DisplayName}: {dealt} 伤害 | 敌人 HP {target.Hp} 盾 {target.Shield} | {actor.DisplayName} 能量 {actor.Energy}/{actor.EnergyMax} | AP {ap.ActionPoints}");
            lastMessage = $"{actor.DisplayName} 普攻 {dealt} 伤";

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
            if (!ap.TrySpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {ap.ActionPoints})";
                Debug.Log($"[CombatTest] 大招失败: AP 不足 (剩余 {ap.ActionPoints})");
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

            Debug.Log($"*** 大招释放 *** {actor.DisplayName} 对全体敌人造成 {totalDamage} 总伤害 | AP {ap.ActionPoints}");
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

            ap.EndTurn();
            Debug.Log($"[CombatTest] --- 玩家回合结束, 敌人回合 ---");

            int penalty = commander.ApplyTurnStartPenalty();
            if (penalty > 0)
            {
                Debug.Log($"[CombatTest] 感染惩罚: -{penalty} HP | HP {commander.Hp}");
                if (commander.IsDefeated)
                {
                    Defeat();
                    return;
                }
            }

            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                EnemyState enemy = battle.Enemies[i];
                if (enemy.IsDefeated)
                {
                    continue;
                }
                string result = EnemyAiController.ExecuteIntent(enemy, intents[i], commander);
                Debug.Log($"[CombatTest] 敌人行动: {result} | 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
                lastMessage = result;
                if (commander.IsDefeated)
                {
                    Defeat();
                    return;
                }
            }

            round++;
            StartPlayerTurn();
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
                Debug.Log("*** 胜利 *** 所有敌人被击败!");
                lastMessage = "*** 胜利 ***";
            }
        }

        private void Defeat()
        {
            battleOver = true;
            Debug.Log($"*** 失败 *** 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
            lastMessage = "*** 失败 ***";
        }

        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(20, 20, 760, 640), GUI.skin.box);

            GUILayout.Label("=== Combat Test M3.5 (编队 + 多敌人) ===", GUI.skin.label);
            GUILayout.Space(6);

            GUILayout.Label("--- Squad 灰鸦小队 (A/D 切换, 选中高亮) ---", GUI.skin.label);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < squad.Count; i++)
            {
                ConstructState member = squad.Members[i];
                string flagText = member.IsActive ? "" : (member.Flag == ConstructStateFlag.Unavailable ? " [离队]" : " [恢复中]");
                string energyBar = EnergyBar(member);
                string label = $"[{i}] {member.DisplayName}{flagText}\n能量 {energyBar}";
                if (i == squad.CurrentIndex)
                {
                    GUILayout.Label(label, SelectedStyle(), GUILayout.Width(200), GUILayout.Height(70));
                }
                else
                {
                    GUILayout.Label(label, NormalStyle(), GUILayout.Width(200), GUILayout.Height(70));
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label($"--- Commander 指挥官 ---", GUI.skin.label);
            GUILayout.Label($"HP {commander.Hp}/{commander.MaxHp}   Shield {commander.Shield}   Infection {commander.Infection}/100 ({commander.GetInfectionLevel()})   Serum {commander.SerumCount}", NormalStyle());

            GUILayout.Space(6);
            GUILayout.Label("--- Enemies 敌人 (W/S 切换, 选中高亮) ---", GUI.skin.label);
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                EnemyState enemy = battle.Enemies[i];
                string intentText = enemy.IsDefeated ? "已击败" : (intents[i] != null ? intents[i].Description : "?");
                string label = $"[{i}] {enemy.DisplayName}\nHP {enemy.Hp}/{enemy.MaxHp}   Shield {enemy.Shield}   Intent: {intentText}";
                if (i == battle.SelectedEnemyIndex && !enemy.IsDefeated)
                {
                    GUILayout.Label(label, SelectedEnemyStyle(), GUILayout.Width(700), GUILayout.Height(54));
                }
                else
                {
                    GUILayout.Label(label, NormalStyle(), GUILayout.Width(700), GUILayout.Height(54));
                }
            }

            GUILayout.Space(6);
            GUILayout.Label($"--- Round {round} | AP {ap.ActionPoints}/{ap.MaxActionPoints} ---", NormalStyle());

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Basic Attack (普攻, 1AP)", GUILayout.Width(180)))
            {
                BasicAttack();
            }
            if (GUILayout.Button("Ultimate (R, 大招全体, 1AP)", GUILayout.Width(200)))
            {
                UseUltimate();
            }
            if (GUILayout.Button($"Serum (血清, -{CommanderState.SerumReduction}感染)", GUILayout.Width(200)))
            {
                UseSerum();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("End Turn (结束回合)", GUILayout.Width(180)))
            {
                EndTurn();
            }
            if (GUILayout.Button("Toggle Leave (当前构造体离队/归队)", GUILayout.Width(240)))
            {
                ToggleConstructLeave();
            }
            if (GUILayout.Button("Restart (重新开始)", GUILayout.Width(160)))
            {
                StartNewBattle();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Last: " + lastMessage, SelectedStyle());

            GUILayout.Space(8);
            GUILayout.Label("--- 键位 ---", NormalStyle());
            GUILayout.Label("A/D: 切换构造体   W/S: 切换攻击目标   R: 大招(能量满+1AP)", NormalStyle());
            GUILayout.Label("普攻获得能量(每次+1); 大招对全体敌人造成伤害后清空能量", NormalStyle());
            GUILayout.Label("规则: 指挥官受伤感染+伤害/2; 感染34+回合开始-1HP, 67+-3HP; 感染100失败", NormalStyle());

            GUILayout.EndArea();
        }

        private GUIStyle SelectedStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.yellow;
            style.normal.background = MakeTexture(new Color(0.25f, 0.25f, 0.05f));
            return style;
        }

        private GUIStyle SelectedEnemyStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.red;
            style.normal.background = MakeTexture(new Color(0.35f, 0.05f, 0.05f));
            return style;
        }

        private GUIStyle NormalStyle()
        {
            return new GUIStyle(GUI.skin.label);
        }

        private static Texture2D MakeTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static string EnergyBar(ConstructState member)
        {
            int bars = Mathf.RoundToInt(member.Energy / (float)member.EnergyMax * 10f);
            string filled = new string('█', bars);
            string empty = new string('░', 10 - bars);
            return $"{member.Energy}/{member.EnergyMax} {filled}{empty}";
        }
    }
}
