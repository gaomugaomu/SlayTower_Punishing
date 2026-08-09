using System.Collections.Generic;
using System.Linq;
using PunishingTower.Data;

namespace PunishingTower.SignalOrb
{
    /// <summary>
    /// Controls the signal orb lifecycle:
    /// DrawPile -&gt; Hand -&gt; Played -&gt; Discard -&gt; Shuffle
    /// Supports exhaust (removed permanently) and retained orbs.
    /// </summary>
    public class OrbPool
    {
        public const int HandLimit = 16;
        public const int QueueLimit = 8;

        private readonly List<OrbInstance> drawPile = new List<OrbInstance>();
        private readonly List<OrbInstance> hand = new List<OrbInstance>();
        private readonly List<OrbInstance> discard = new List<OrbInstance>();
        private readonly List<OrbInstance> exhaust = new List<OrbInstance>();
        private readonly List<OrbInstance> queue = new List<OrbInstance>();

        public IReadOnlyList<OrbInstance> DrawPile => drawPile;
        public IReadOnlyList<OrbInstance> Hand => hand;
        public IReadOnlyList<OrbInstance> Discard => discard;
        public IReadOnlyList<OrbInstance> Exhaust => exhaust;
        public IReadOnlyList<OrbInstance> Queue => queue;

        /// <summary>Creates a fresh pool from static orb definitions (one instance per entry).</summary>
        public OrbPool(IEnumerable<OrbData> definitions)
        {
            foreach (OrbData data in definitions)
            {
                drawPile.Add(new OrbInstance(data));
            }
        }

        public int HandCount => hand.Count;
        public int DrawCount => drawPile.Count;
        public int DiscardCount => discard.Count;
        public int ExhaustCount => exhaust.Count;
        public int TotalCount => drawPile.Count + hand.Count + discard.Count + exhaust.Count;

        public bool IsHandFull => hand.Count >= HandLimit;

        /// <summary>Draws one orb from the draw pile into the hand. Shuffles discard back when the draw pile is empty.</summary>
        public OrbInstance Draw()
        {
            if (drawPile.Count == 0)
            {
                ShuffleDiscardIntoDraw();
            }

            if (drawPile.Count == 0)
            {
                return null;
            }

            OrbInstance orb = drawPile[drawPile.Count - 1];
            drawPile.RemoveAt(drawPile.Count - 1);
            hand.Add(orb);
            return orb;
        }

        public List<OrbInstance> Draw(int count)
        {
            var drawn = new List<OrbInstance>();
            for (int i = 0; i < count; i++)
            {
                OrbInstance orb = Draw();
                if (orb == null)
                {
                    break;
                }
                drawn.Add(orb);
            }
            return drawn;
        }

        /// <summary>Moves an orb from hand to the played queue, then to discard (or exhaust).</summary>
        public void PlayFromHand(OrbInstance orb)
        {
            if (orb == null || !hand.Contains(orb))
            {
                return;
            }

            hand.Remove(orb);

            if (orb.Retained)
            {
                hand.Add(orb);
                return;
            }

            queue.Add(orb);
            if (queue.Count > QueueLimit)
            {
                queue.RemoveAt(0);
            }

            if (orb.ExhaustOnUse)
            {
                exhaust.Add(orb);
            }
            else
            {
                discard.Add(orb);
            }
        }

        /// <summary>Discards an orb from the hand without playing it.</summary>
        public void DiscardFromHand(OrbInstance orb)
        {
            if (orb == null || !hand.Contains(orb))
            {
                return;
            }

            hand.Remove(orb);
            discard.Add(orb);
        }

        public void DiscardAllHand()
        {
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                DiscardFromHand(hand[i]);
            }
        }

        /// <summary>Moves every discard pile orb back into the draw pile.</summary>
        public void ShuffleDiscardIntoDraw()
        {
            drawPile.AddRange(discard);
            discard.Clear();
        }

        /// <summary>Removes an orb from the hand entirely (used by effects or exhaustion).</summary>
        public void ExhaustFromHand(OrbInstance orb)
        {
            if (orb == null || !hand.Contains(orb))
            {
                return;
            }

            hand.Remove(orb);
            exhaust.Add(orb);
        }

        /// <summary>Removes an orb from the pool completely (retain/exhaust mechanics).</summary>
        public bool RemoveFromPool(OrbInstance orb)
        {
            if (drawPile.Remove(orb) || hand.Remove(orb) || discard.Remove(orb) || exhaust.Remove(orb))
            {
                return true;
            }
            return false;
        }

        /// <summary>Moves an orb held outside the pool (e.g. an external orb row) into the discard pile.</summary>
        public void DiscardOrb(OrbInstance orb)
        {
            if (orb == null)
            {
                return;
            }

            RemoveFromPool(orb);
            discard.Add(orb);
        }
    }
}
