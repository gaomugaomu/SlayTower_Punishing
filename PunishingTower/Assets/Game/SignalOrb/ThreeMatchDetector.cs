using System.Collections.Generic;
using PunishingTower.Core;
using PunishingTower.Core.Events;
using PunishingTower.Data;

namespace PunishingTower.SignalOrb
{
    /// <summary>A group of consecutive same-color orbs that can trigger a three match.</summary>
    public sealed class ThreeMatchGroup
    {
        public int Color { get; }
        public int Count { get; }
        public List<ISignalOrb> Orbs { get; }

        public ThreeMatchGroup(int color, List<ISignalOrb> orbs)
        {
            Color = color;
            Count = orbs.Count;
            Orbs = orbs;
        }
    }

    /// <summary>
    /// Detects three match opportunities in a sequence of played orbs.
    /// Algorithm (doc 27):
    ///   1. Split the sequence into same-color runs. Locked orbs and special (white)
    ///      orbs are blockers that never participate in a match.
    ///   2. Pairs of size 2 are removed; when removal joins two same-color neighbors
    ///      they merge into a longer run (collapse).
    ///   3. Any remaining run of 3+ triggers a match.
    /// Example: R R Y Y R B B -&gt; Y Y removed -&gt; R R R triggers.
    /// </summary>
    public static class ThreeMatchDetector
    {
        private sealed class Run
        {
            public int Color;
            public bool Matchable = true;
            public readonly List<ISignalOrb> Orbs = new List<ISignalOrb>();
        }

        public static List<ThreeMatchGroup> Detect(IReadOnlyList<ISignalOrb> orbs)
        {
            var groups = new List<ThreeMatchGroup>();
            if (orbs == null || orbs.Count == 0)
            {
                return groups;
            }

            List<Run> runs = BuildRuns(orbs);
            CollapsePairs(runs);

            foreach (Run run in runs)
            {
                if (run.Matchable && run.Orbs.Count >= 3)
                {
                    groups.Add(new ThreeMatchGroup(run.Color, new List<ISignalOrb>(run.Orbs)));
                }
            }

            return groups;
        }

        public static bool HasMatch(IReadOnlyList<ISignalOrb> orbs)
        {
            return Detect(orbs).Count > 0;
        }

        /// <summary>Publishes a ThreeMatchEvent for every detected group.</summary>
        public static void PublishMatches(IReadOnlyList<ISignalOrb> orbs)
        {
            foreach (ThreeMatchGroup group in Detect(orbs))
            {
                EventBus.Publish(new ThreeMatchEvent(group.Color, group.Count));
            }
        }

        private static List<Run> BuildRuns(IReadOnlyList<ISignalOrb> orbs)
        {
            var runs = new List<Run>();

            foreach (ISignalOrb orb in orbs)
            {
                bool isBlocker = orb is OrbInstance oi && (oi.Locked || oi.Data.Color == OrbColor.White);

                if (isBlocker)
                {
                    var blocker = new Run { Color = orb.Color, Matchable = false };
                    blocker.Orbs.Add(orb);
                    runs.Add(blocker);
                    continue;
                }

                if (runs.Count > 0)
                {
                    Run last = runs[runs.Count - 1];
                    if (last.Matchable && last.Color == orb.Color)
                    {
                        last.Orbs.Add(orb);
                        continue;
                    }
                }

                var run = new Run { Color = orb.Color };
                run.Orbs.Add(orb);
                runs.Add(run);
            }

            return runs;
        }

        /// <summary>
        /// Removes a run of exactly 2 orbs when it sits between two same-color runs;
        /// the neighbors then merge into a longer run.
        /// Example: R R [Y Y] R B B -&gt; Y Y removed -&gt; R R R B B triggers.
        /// Standalone pairs (at an edge or between different colors) are kept.
        /// </summary>
        private static void CollapsePairs(List<Run> runs)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < runs.Count; i++)
                {
                    Run run = runs[i];
                    if (!run.Matchable || run.Orbs.Count != 2)
                    {
                        continue;
                    }

                    Run left = i > 0 ? runs[i - 1] : null;
                    Run right = i < runs.Count - 1 ? runs[i + 1] : null;

                    bool merges = left != null && right != null &&
                                  left.Matchable && right.Matchable &&
                                  left.Color == right.Color;

                    if (!merges)
                    {
                        continue;
                    }

                    left.Orbs.AddRange(right.Orbs);
                    runs.RemoveAt(i + 1);
                    runs.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }
    }
}
