using System.Collections.Generic;

namespace PunishingTower.Combat
{
    /// <summary>
    /// Holds both sides of the battle: the commander (player side) and a list of enemies.
    /// Player selects one enemy as the current target (W/S switching).
    /// </summary>
    public class BattleContext
    {
        public CommanderState Commander { get; set; }

        private readonly List<EnemyState> enemies = new List<EnemyState>();

        public IReadOnlyList<EnemyState> Enemies => enemies;
        public int SelectedEnemyIndex { get; private set; }

        public EnemyState SelectedEnemy => SelectedEnemyIndex >= 0 && SelectedEnemyIndex < enemies.Count ? enemies[SelectedEnemyIndex] : null;

        public bool AllEnemiesDefeated
        {
            get
            {
                foreach (EnemyState enemy in enemies)
                {
                    if (!enemy.IsDefeated)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public int AliveEnemyCount
        {
            get
            {
                int count = 0;
                foreach (EnemyState enemy in enemies)
                {
                    if (!enemy.IsDefeated)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public void AddEnemy(EnemyState enemy)
        {
            enemies.Add(enemy);
        }

        public void RemoveEnemy(EnemyState enemy)
        {
            int index = enemies.IndexOf(enemy);
            if (index < 0)
            {
                return;
            }
            enemies.RemoveAt(index);
            if (SelectedEnemyIndex >= enemies.Count)
            {
                SelectedEnemyIndex = enemies.Count - 1;
            }
        }

        public void ClearEnemies()
        {
            enemies.Clear();
            SelectedEnemyIndex = 0;
        }

        /// <summary>Selects the next alive enemy, wrapping around. Dead enemies are skipped.</summary>
        public void SelectNextEnemy()
        {
            if (enemies.Count == 0)
            {
                SelectedEnemyIndex = -1;
                return;
            }
            int start = SelectedEnemyIndex;
            do
            {
                SelectedEnemyIndex = (SelectedEnemyIndex + 1) % enemies.Count;
                if (!enemies[SelectedEnemyIndex].IsDefeated)
                {
                    return;
                }
            } while (SelectedEnemyIndex != start);
        }

        /// <summary>Selects the previous alive enemy, wrapping around. Dead enemies are skipped.</summary>
        public void SelectPreviousEnemy()
        {
            if (enemies.Count == 0)
            {
                SelectedEnemyIndex = -1;
                return;
            }
            int start = SelectedEnemyIndex;
            do
            {
                SelectedEnemyIndex = (SelectedEnemyIndex - 1 + enemies.Count) % enemies.Count;
                if (!enemies[SelectedEnemyIndex].IsDefeated)
                {
                    return;
                }
            } while (SelectedEnemyIndex != start);
        }
    }
}
