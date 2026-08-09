using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>
    /// Base class for all data-driven game definitions.
    /// All balance values must be editable in the inspector without code changes.
    /// </summary>
    public abstract class GameDataObject : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        public string Id => id;
        public string DisplayName => displayName;

#if UNITY_EDITOR
        public void AssignIdentity(string newId, string newDisplayName)
        {
            id = newId;
            displayName = newDisplayName;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
