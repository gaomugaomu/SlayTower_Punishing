using System;
using System.Collections.Generic;
using PunishingTower.Core;
using PunishingTower.Data;

namespace PunishingTower.SignalOrb
{
    /// <summary>
    /// The group of orbs eliminated together when one of them is played.
    /// MatchCount is 1 (single), 2 (pair) or 3 (three match). Three is the maximum.
    /// </summary>
    public sealed class OrbPlayGroup
    {
        public int Color { get; }
        public List<ISignalOrb> Orbs { get; }
        public int MatchCount => Orbs.Count;

        public OrbPlayGroup(int color, List<ISignalOrb> orbs)
        {
            Color = color;
            Orbs = orbs;
        }
    }

    /// <summary>
    /// Three match resolution for the Punishing Gray Raven orb row.
    /// Rules:
    ///   - The orb row is ordered from left (oldest) to right (newest).
    ///   - Grouping is evaluated right to left.
    ///   - Consecutive same-color orbs belong to one group; a group holds at most 3 orbs,
    ///     so 4+ same-color orbs split into groups of 3 (rightmost first) plus leftovers.
    ///     Example R R R R: the rightmost 3 form a three match, the leftmost R is a single.
    ///   - Resolution happens on selection: playing any orb in a group eliminates the whole group.
    ///   - Locked or white (special) orbs are blockers: they cannot be played and split groups.
    /// </summary>
    public static class ThreeMatchDetector
    {
        /// <summary>
        /// Returns the play group containing the orb at <paramref name="index"/> of the row,
        /// or null when that orb is a blocker (locked / special) and cannot be played.
        /// </summary>
        public static OrbPlayGroup ResolvePlay(IReadOnlyList<ISignalOrb> row, int index)
        {
            if (row == null || index < 0 || index >= row.Count)
            {
                return null;
            }

            ISignalOrb selected = row[index];
            if (IsBlocker(selected))
            {
                return null;
            }

            int segLeft = index;
            while (segLeft > 0 && !IsBlocker(row[segLeft - 1]) && row[segLeft - 1].Color == selected.Color)
            {
                segLeft--;
            }

            int segRight = index;
            while (segRight < row.Count - 1 && !IsBlocker(row[segRight + 1]) && row[segRight + 1].Color == selected.Color)
            {
                segRight++;
            }

            // Group the segment right-to-left into chunks of at most 3.
            int groupRight = segRight;
            while (groupRight >= segLeft)
            {
                int groupLeft = Math.Max(segLeft, groupRight - 2);
                if (index >= groupLeft && index <= groupRight)
                {
                    var orbs = new List<ISignalOrb>(groupRight - groupLeft + 1);
                    for (int i = groupLeft; i <= groupRight; i++)
                    {
                        orbs.Add(row[i]);
                    }
                    return new OrbPlayGroup(selected.Color, orbs);
                }
                groupRight = groupLeft - 1;
            }

            return null;
        }

        /// <summary>
        /// Partitions the whole row into play groups (right-to-left, max 3 per group).
        /// Blockers appear as single non-playable groups with MatchCount 1 but are marked via their orb state.
        /// </summary>
        public static List<OrbPlayGroup> GetAllGroups(IReadOnlyList<ISignalOrb> row)
        {
            var groups = new List<OrbPlayGroup>();
            if (row == null || row.Count == 0)
            {
                return groups;
            }

            int i = row.Count - 1;
            while (i >= 0)
            {
                if (IsBlocker(row[i]))
                {
                    groups.Add(new OrbPlayGroup(row[i].Color, new List<ISignalOrb> { row[i] }));
                    i--;
                    continue;
                }

                int color = row[i].Color;
                int segRight = i;
                int segLeft = i;
                while (segLeft > 0 && !IsBlocker(row[segLeft - 1]) && row[segLeft - 1].Color == color)
                {
                    segLeft--;
                }

                int groupRight = segRight;
                while (groupRight >= segLeft)
                {
                    int groupLeft = Math.Max(segLeft, groupRight - 2);
                    var orbs = new List<ISignalOrb>(groupRight - groupLeft + 1);
                    for (int j = groupLeft; j <= groupRight; j++)
                    {
                        orbs.Add(row[j]);
                    }
                    groups.Add(new OrbPlayGroup(color, orbs));
                    groupRight = groupLeft - 1;
                }

                i = segLeft - 1;
            }

            groups.Reverse();
            return groups;
        }

        private static bool IsBlocker(ISignalOrb orb)
        {
            if (orb is OrbInstance oi)
            {
                return oi.Locked || oi.Data.Color == OrbColor.White;
            }
            return orb.Color == (int)OrbColor.White;
        }
    }
}
