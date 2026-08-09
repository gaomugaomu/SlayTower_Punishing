using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>Signal orb colors. White is the special color.</summary>
    public enum OrbColor
    {
        Red = 0,
        Yellow = 1,
        Blue = 2,
        White = 3
    }

    /// <summary>Static definition of a signal orb. Runtime instances are separate.</summary>
    [CreateAssetMenu(fileName = "Orb_", menuName = "PunishingTower/Data/Orb")]
    public class OrbData : GameDataObject
    {
        [SerializeField] private OrbColor color;

        public OrbColor Color => color;

#if UNITY_EDITOR
        public void AssignColor(OrbColor newColor)
        {
            color = newColor;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
