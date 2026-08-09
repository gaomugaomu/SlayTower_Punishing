using PunishingTower.Combat;
using PunishingTower.Enemy;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for M3 combat basics:
    /// commander (HP/infection/serum), enemy (HP/shield/intent), action points,
    /// basic attack, serum, end turn (enemy acts + infection penalty), victory/defeat.
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

        [Header("Player")]
        [SerializeField] private int basicAttackDamage = 6;
        [SerializeField] private int actionPointsPerTurn = 3;

        private CommanderState commander;
        private EnemyState enemy;
        private ActionPointSystem ap;
        private EnemyAiController ai;
        private EnemyIntent currentIntent;
        private int round;
        private bool battleOver;
        private string lastMessage = string.Empty;
        private Vector2 scroll;

        private void Start()
        {
            StartNewBattle();
        }

        private void StartNewBattle()
        {
            commander = new CommanderState(commanderMaxHp, commanderMaxSerum);
            enemy = new EnemyState("enemy_1", "Infected Mechanical Unit", enemyMaxHp);
            ap = new ActionPointSystem(actionPointsPerTurn);
            ai = new EnemyAiController();
            round = 1;
            battleOver = false;

            StartPlayerTurn();
            Debug.Log($"[CombatTest] === 新战斗开始 Round {round} === 指挥官 HP {commander.Hp} | 敌人 HP {enemy.Hp} | AP {ap.ActionPoints}");
        }

        private void StartPlayerTurn()
        {
            ap.BeginTurn();
            currentIntent = ai.DecideIntent(enemy, enemyAttack, enemyDefenseShield);
            Debug.Log($"[CombatTest] Round {round} 玩家回合 | AP {ap.ActionPoints}/{ap.MaxActionPoints} | 敌人意图: {currentIntent.Description}");
        }

        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(20, 20, 720, 560), GUI.skin.box);

            GUILayout.Label("=== Combat Test (战斗基础手动测试) ===", GUI.skin.label);
            GUILayout.Space(6);

            GUILayout.Label($"--- Commander 指挥官 ---", GUI.skin.label);
            GUILayout.Label($"HP {commander.Hp}/{commander.MaxHp}   Shield 盾 {commander.Shield}   Infection 感染 {commander.Infection}/100 ({commander.GetInfectionLevel()})   Serum 血清 {commander.SerumCount}", GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.Label($"--- Enemy 敌人: {enemy.DisplayName} ---", GUI.skin.label);
            GUILayout.Label($"HP {enemy.Hp}/{enemy.MaxHp}   Shield 盾 {enemy.Shield}   Intent 意图: {currentIntent.Description}", GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.Label($"--- Round {round} | AP {ap.ActionPoints}/{ap.MaxActionPoints} ---", GUI.skin.label);

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Basic Attack (普攻, 1AP, {basicAttackDamage}伤)", GUILayout.Width(300)))
            {
                BasicAttack();
            }
            if (GUILayout.Button($"Use Serum (用血清, 减{CommanderState.SerumReduction}感染)", GUILayout.Width(300)))
            {
                UseSerum();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            if (GUILayout.Button("End Turn (结束回合: 感染惩罚 + 敌人行动 + 新回合)", GUILayout.Width(600)))
            {
                EndTurn();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Restart (重新开始战斗)", GUILayout.Width(600)))
            {
                StartNewBattle();
            }

            GUILayout.Space(10);
            GUILayout.Label("Last: " + lastMessage, GUI.skin.label);

            GUILayout.Space(8);
            GUILayout.Label("--- 规则说明 ---", GUI.skin.label);
            GUILayout.Label("- 指挥官受伤时感染 + 伤害/2; 感染 100 = 失败", GUI.skin.label);
            GUILayout.Label("- 感染惩罚: 34+ 回合开始 -1HP, 67+ 回合开始 -3HP", GUI.skin.label);
            GUILayout.Label("- 敌人每回合意图: 单数回合攻击, 双数回合防御+8盾", GUI.skin.label);
            GUILayout.Label("- 胜利 = 敌人 HP 归零; 失败 = 指挥官 HP 归零或感染 100", GUI.skin.label);

            GUILayout.EndArea();
        }

        private void BasicAttack()
        {
            if (battleOver)
            {
                lastMessage = "战斗已结束";
                return;
            }
            if (!ap.TrySpend(1))
            {
                lastMessage = $"AP 不足 (剩余 {ap.ActionPoints})";
                Debug.Log($"[CombatTest] 普攻失败: AP 不足 (剩余 {ap.ActionPoints})");
                return;
            }

            int dealt = enemy.TakeDamage(basicAttackDamage);
            Debug.Log($"[CombatTest] 普攻命中 {enemy.DisplayName}: {dealt} 伤害 | 敌人 HP {enemy.Hp} 盾 {enemy.Shield} | AP {ap.ActionPoints}");
            lastMessage = $"普攻 {dealt} 伤";

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

            string result = EnemyAiController.ExecuteIntent(enemy, currentIntent, commander);
            Debug.Log($"[CombatTest] 敌人行动: {result} | 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
            lastMessage = result;

            if (commander.IsDefeated)
            {
                Defeat();
                return;
            }

            round++;
            StartPlayerTurn();
        }

        private void CheckVictory()
        {
            if (enemy.IsDefeated)
            {
                battleOver = true;
                Debug.Log("*** 胜利 *** 敌人被击败!");
                lastMessage = "*** 胜利 ***";
            }
        }

        private void Defeat()
        {
            battleOver = true;
            Debug.Log($"*** 失败 *** 指挥官 HP {commander.Hp} 感染 {commander.Infection}");
            lastMessage = "*** 失败 ***";
        }
    }
}
