using System.Collections.Generic;
using PunishingTower.Data;

namespace PunishingTower.Construct
{
    /// <summary>
    /// Runtime squad: ordered member list (dynamic, no fixed cap) plus the currently
    /// selected construct. Selection cycles and skips members that are not Active.
    /// </summary>
    public class SquadRuntime
    {
        private readonly List<ConstructState> members = new List<ConstructState>();

        public IReadOnlyList<ConstructState> Members => members;
        public int CurrentIndex { get; private set; }

        public SquadRuntime(IEnumerable<ConstructData> definitions)
        {
            if (definitions != null)
            {
                foreach (ConstructData data in definitions)
                {
                    members.Add(new ConstructState(data));
                }
            }
            CurrentIndex = FindFirstActive();
        }

        public ConstructState Current => CurrentIndex >= 0 && CurrentIndex < members.Count ? members[CurrentIndex] : null;

        public int Count => members.Count;
        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach (ConstructState member in members)
                {
                    if (member.IsActive)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public ConstructState AddMember(ConstructData data)
        {
            var state = new ConstructState(data);
            members.Add(state);
            if (CurrentIndex < 0)
            {
                CurrentIndex = members.Count - 1;
            }
            return state;
        }

        /// <summary>Removes a member from the squad entirely.</summary>
        public bool RemoveMember(ConstructState state)
        {
            int index = members.IndexOf(state);
            if (index < 0)
            {
                return false;
            }
            members.RemoveAt(index);
            if (CurrentIndex >= members.Count)
            {
                CurrentIndex = members.Count - 1;
            }
            return true;
        }

        /// <summary>Selects the next Active member, wrapping around.</summary>
        public void SelectNext()
        {
            if (members.Count == 0)
            {
                CurrentIndex = -1;
                return;
            }
            int start = CurrentIndex;
            do
            {
                CurrentIndex = (CurrentIndex + 1) % members.Count;
                if (members[CurrentIndex].IsActive)
                {
                    return;
                }
            } while (CurrentIndex != start);
        }

        /// <summary>Selects the previous Active member, wrapping around.</summary>
        public void SelectPrevious()
        {
            if (members.Count == 0)
            {
                CurrentIndex = -1;
                return;
            }
            int start = CurrentIndex;
            do
            {
                CurrentIndex = (CurrentIndex - 1 + members.Count) % members.Count;
                if (members[CurrentIndex].IsActive)
                {
                    return;
                }
            } while (CurrentIndex != start);
        }

        private int FindFirstActive()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].IsActive)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
