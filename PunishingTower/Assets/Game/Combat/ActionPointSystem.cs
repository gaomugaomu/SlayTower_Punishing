namespace PunishingTower.Combat
{
    /// <summary>
    /// Action points: 3 per player turn. AP resets every turn and does not carry over.
    /// Actions consuming AP: orb skill, normal attack, ultimate.
    /// </summary>
    public class ActionPointSystem
    {
        public const int DefaultMax = 3;

        public int ActionPoints { get; private set; }
        public int MaxActionPoints { get; }

        public ActionPointSystem(int max = DefaultMax)
        {
            MaxActionPoints = max;
            ActionPoints = 0;
        }

        /// <summary>Resets AP to max at the start of the player turn.</summary>
        public void BeginTurn()
        {
            ActionPoints = MaxActionPoints;
        }

        /// <summary>AP never carries over: dropping to zero at the end of the turn.</summary>
        public void EndTurn()
        {
            ActionPoints = 0;
        }

        public bool CanSpend(int cost)
        {
            return cost >= 0 && ActionPoints >= cost;
        }

        /// <summary>Spends AP when affordable. Returns true on success.</summary>
        public bool TrySpend(int cost)
        {
            if (!CanSpend(cost))
            {
                return false;
            }
            ActionPoints -= cost;
            return true;
        }

        public void AddPoints(int amount)
        {
            if (amount > 0)
            {
                ActionPoints += amount;
            }
        }
    }
}
