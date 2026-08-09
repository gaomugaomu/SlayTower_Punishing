using System;
using PunishingTower.Combat;

namespace PunishingTower.Enemy
{
    /// <summary>
    /// Enemy AI: decides the intent for the next enemy turn and executes it.
    /// Infected Mechanical Unit pattern: alternates attack and defense.
    /// </summary>
    public class EnemyAiController
    {
        private readonly Random rng;
        private int turnCount;

        public EnemyAiController(int seed = -1)
        {
            rng = seed >= 0 ? new Random(seed) : new Random();
        }

        /// <summary>Decides the enemy's planned action for the upcoming enemy turn.</summary>
        public EnemyIntent DecideIntent(EnemyState enemy, int attackDamage, int defenseShield)
        {
            turnCount++;
            bool attack = turnCount % 2 == 1;

            if (attack)
            {
                return EnemyIntent.Attack(attackDamage);
            }
            return EnemyIntent.Defense(defenseShield);
        }

        /// <summary>Executes an intent against the commander. Returns a readable result string.</summary>
        public static string ExecuteIntent(EnemyState enemy, EnemyIntent intent, CommanderState commander)
        {
            switch (intent.Type)
            {
                case IntentType.Attack:
                    int damage = commander.TakeDamage(intent.Amount);
                    return $"{enemy.DisplayName} attacks commander for {damage} damage";
                case IntentType.Defense:
                    enemy.AddShield(intent.Amount);
                    return $"{enemy.DisplayName} gains {intent.Amount} shield";
                default:
                    return $"{enemy.DisplayName} does nothing";
            }
        }
    }
}
