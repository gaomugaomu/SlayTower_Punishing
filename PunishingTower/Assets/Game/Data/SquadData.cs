using System.Collections.Generic;
using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>
    /// A deployable squad: member constructs plus commander baseline.
    /// Membership is dynamic (constructs may temporarily leave during a run).
    /// </summary>
    [CreateAssetMenu(fileName = "Squad_", menuName = "PunishingTower/Data/Squad")]
    public class SquadData : GameDataObject
    {
        [SerializeField] private List<ConstructData> members = new List<ConstructData>();

        [Header("Commander Baseline")]
        [SerializeField] private int commanderMaxHp = 100;
        [SerializeField] private int commanderMaxSerum = 3;

        public IReadOnlyList<ConstructData> Members => members;
        public int CommanderMaxHp => commanderMaxHp;
        public int CommanderMaxSerum => commanderMaxSerum;

#if UNITY_EDITOR
        public void AssignMembers(IEnumerable<ConstructData> newMembers, int maxHp, int maxSerum)
        {
            members.Clear();
            if (newMembers != null)
            {
                members.AddRange(newMembers);
            }
            commanderMaxHp = maxHp;
            commanderMaxSerum = maxSerum;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
