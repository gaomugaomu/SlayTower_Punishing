namespace PunishingTower.Data
{
    /// <summary>Reusable effect types used by skills (doc 28 / 214).</summary>
    public enum EffectType
    {
        Damage,
        Shield,
        Heal,
        Energy,
        DrawOrb,
        Infection,
        CoreMark,
        Weaken
    }

    /// <summary>A single effect instance: type + amount.</summary>
    [System.Serializable]
    public class EffectData
    {
        public EffectType type;
        public int amount;

        public EffectData()
        {
        }

        public EffectData(EffectType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }

        public override string ToString()
        {
            return $"{type}:{amount}";
        }
    }
}
