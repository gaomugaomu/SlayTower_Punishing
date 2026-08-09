using System.Collections.Generic;
using PunishingTower.Core;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for the Punishing Gray Raven orb row.
    /// Hand holds up to 16 orbs; only the rightmost 8 are visible and clickable.
    /// Orbs align to the right: after elimination the remaining orbs stay right-aligned
    /// and hidden orbs slide in from the left when space opens up.
    /// Three match is resolved at selection time against the visible row only.
    /// No effects are applied - results are printed via Debug.Log.
    /// </summary>
    public class OrbTestDriver : MonoBehaviour
    {
        public const int VisibleRowSize = 8;

        [SerializeField] private int redCount = 5;
        [SerializeField] private int yellowCount = 5;
        [SerializeField] private int blueCount = 5;
        [SerializeField] private int initialHand = 8;

        private OrbPool pool;
        private string lastMessage = string.Empty;

        private void Start()
        {
            BuildPool();
            DrawInitialHand();
        }

        private void BuildPool()
        {
            var defs = new List<OrbData>();
            for (int i = 0; i < redCount; i++) { defs.Add(CreateOrb("red_" + i, OrbColor.Red)); }
            for (int i = 0; i < yellowCount; i++) { defs.Add(CreateOrb("yellow_" + i, OrbColor.Yellow)); }
            for (int i = 0; i < blueCount; i++) { defs.Add(CreateOrb("blue_" + i, OrbColor.Blue)); }
            pool = new OrbPool(defs);
            pool.ShuffleDrawPile();
            Debug.Log($"[OrbTest] 球库构建完成:红 {redCount} 黄 {yellowCount} 蓝 {blueCount},共 {pool.TotalCount} 球,已洗牌");
        }

        private static OrbData CreateOrb(string id, OrbColor color)
        {
            var data = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            data.AssignIdentity(id, id);
            data.AssignColor(color);
#endif
            return data;
        }

        private void DrawInitialHand()
        {
            int drawn = pool.Draw(initialHand).Count;
            Debug.Log($"[OrbTest] 开局抽牌:{drawn} 张 | 手牌 {pool.HandCount}/{OrbPool.HandLimit} | 抽牌堆剩 {pool.DrawCount}");
        }

        private List<OrbInstance> VisibleRow()
        {
            return pool.GetVisibleRow(VisibleRowSize);
        }

        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(20, 20, 800, 560));
            GUILayout.Label("=== Signal Orb Row Test (信号球队列手动测试) ===", GUI.skin.label);
            GUILayout.Label($"Hand 手牌:{pool.HandCount}/{OrbPool.HandLimit}   DrawPile 抽牌堆:{pool.DrawCount}   Discard 弃牌堆:{pool.DiscardCount}   Exhaust 耗尽:{pool.ExhaustCount}", GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Draw 1 (抽1张)", GUILayout.Width(150)))
            {
                DrawOne();
            }
            if (GUILayout.Button("Draw 5 (抽5张)", GUILayout.Width(150)))
            {
                Draw(5);
            }
            if (GUILayout.Button("Shuffle (弃牌洗回)", GUILayout.Width(170)))
            {
                ShuffleBack();
            }
            if (GUILayout.Button("Discard All (弃全部手牌)", GUILayout.Width(200)))
            {
                DiscardAllHand();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("=== Visible Row (可见队列,靠右对齐,slot 0 在最右,点击消球) ===", GUI.skin.label);
            GUILayout.Label("Last: " + lastMessage, GUI.skin.label);

            GUILayout.Space(6);
            DrawVisibleRow();

            GUILayout.Space(10);
            GUILayout.Label("=== Group Preview (可见队列分组,右->左,每3个一组) ===", GUI.skin.label);
            GUILayout.Label(GroupPreviewText(), GUI.skin.label);

            GUILayout.Space(10);
            GUILayout.Label($"=== Full Hand (全部手牌 {pool.HandCount} 球,左=最旧,右=最新) ===", GUI.skin.label);
            GUILayout.Label(HandPreviewText(), GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.Label("=== DrawPile 抽牌堆 ===", GUI.skin.label);
            GUILayout.Label(PilePreviewText(pool.DrawPile), GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.Label("=== Discard 弃牌堆 ===", GUI.skin.label);
            GUILayout.Label(PilePreviewText(pool.Discard), GUI.skin.label);
            GUILayout.EndArea();
        }

        private void DrawVisibleRow()
        {
            List<OrbInstance> visible = VisibleRow();

            // UI slot 0 is the RIGHTMOST (newest) orb; slot 7 is the LEFTMOST (oldest).
            // visible[0] is leftmost(oldest), visible[Count-1] is rightmost(newest).
            GUILayout.BeginHorizontal();
            for (int slot = VisibleRowSize - 1; slot >= 0; slot--)
            {
                int visibleIndex = visible.Count - 1 - slot;
                if (visibleIndex >= 0)
                {
                    OrbInstance orb = visible[visibleIndex];
                    string label = OrbLabel(orb);
                    if (IsBlocker(orb))
                    {
                        GUILayout.Label(label, GUILayout.Width(88), GUILayout.Height(64));
                    }
                    else if (GUILayout.Button(label, GUILayout.Width(88), GUILayout.Height(64)))
                    {
                        PlayVisibleOrb(slot);
                    }
                }
                else
                {
                    GUILayout.Label("(空)", GUILayout.Width(88), GUILayout.Height(64));
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int slot = VisibleRowSize - 1; slot >= 0; slot--)
            {
                GUILayout.Label(slot.ToString(), GUILayout.Width(88));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>Plays the orb at the given visible slot (0 = rightmost/newest).</summary>
        private void PlayVisibleOrb(int slot)
        {
            List<OrbInstance> visible = VisibleRow();
            int visibleIndex = visible.Count - 1 - slot;
            if (visibleIndex < 0 || visibleIndex >= visible.Count)
            {
                return;
            }

            OrbInstance orb = visible[visibleIndex];

            if (IsBlocker(orb))
            {
                Debug.Log($"[OrbTest] {OrbLabel(orb)} 是锁球/特殊球,无法消球");
                lastMessage = "锁球/特殊球无法消球";
                return;
            }

            List<ISignalOrb> visibleAsInterface = new List<ISignalOrb>(visible);
            OrbPlayGroup group = ThreeMatchDetector.ResolvePlay(visibleAsInterface, visibleIndex);
            if (group == null || group.MatchCount == 0)
            {
                Debug.Log("[OrbTest] 消球失败:未找到可消组(不应该发生)");
                lastMessage = "消球失败";
                return;
            }

            string colorName = ColorName(group.Color);
            string groupDesc = RowText(group.Orbs);

            Debug.Log($"[OrbTest] 选中 slot {slot} {OrbLabel(orb)} | 判定组({group.MatchCount}消): {groupDesc}");

            if (group.MatchCount == 1)
            {
                Debug.Log($"*** 单消成功 *** {colorName} x1");
                lastMessage = $"*** 单消成功 {colorName} x1 ***";
            }
            else if (group.MatchCount == 2)
            {
                Debug.Log($"*** 2消成功 *** {colorName} x2");
                lastMessage = $"*** 2消成功 {colorName} x2 ***";
            }
            else
            {
                Debug.Log($"*** 3消成功 *** {colorName} x3 (ThreeMatch)");
                lastMessage = $"*** 3消成功 {colorName} x3 ***";
            }

            var toDiscard = new List<OrbInstance>();
            foreach (ISignalOrb g in group.Orbs)
            {
                toDiscard.Add((OrbInstance)g);
            }
            foreach (OrbInstance g in toDiscard)
            {
                pool.DiscardFromHand(g);
            }

            Debug.Log($"[OrbTest] 消球完成:手牌 {pool.HandCount} | 弃牌堆 {pool.DiscardCount} | 抽牌堆 {pool.DrawCount}");
        }

        private void DrawOne()
        {
            if (pool.IsHandFull)
            {
                lastMessage = $"手牌已满({OrbPool.HandLimit})";
                Debug.Log($"[OrbTest] 手牌已满({OrbPool.HandLimit}),无法抽牌");
                return;
            }
            OrbInstance orb = pool.Draw();
            if (orb == null)
            {
                lastMessage = "抽牌失败:抽牌堆与弃牌堆均为空";
                Debug.Log("[OrbTest] 抽牌失败:抽牌堆与弃牌堆均为空");
                return;
            }
            lastMessage = $"抽到 {OrbLabel(orb)} -> 手牌 {pool.HandCount}";
            Debug.Log($"[OrbTest] 抽到 {OrbLabel(orb)} | 手牌 {pool.HandCount}/{OrbPool.HandLimit} | 抽牌堆剩 {pool.DrawCount}");
        }

        private void Draw(int count)
        {
            if (pool.IsHandFull)
            {
                lastMessage = $"手牌已满({OrbPool.HandLimit})";
                Debug.Log($"[OrbTest] 手牌已满({OrbPool.HandLimit}),无法抽牌");
                return;
            }
            int before = pool.HandCount;
            int drawn = pool.Draw(count).Count;
            Debug.Log($"[OrbTest] 抽 {drawn} 张 | 手牌 {before} -> {pool.HandCount} | 抽牌堆剩 {pool.DrawCount}");
            lastMessage = $"抽 {drawn} 张完成";
        }

        private void ShuffleBack()
        {
            if (pool.DiscardCount == 0)
            {
                lastMessage = "弃牌堆为空,无需洗回";
                Debug.Log("[OrbTest] 弃牌堆为空,无需洗回");
                return;
            }
            int count = pool.DiscardCount;
            pool.ShuffleDiscardIntoDraw();
            Debug.Log($"[OrbTest] 洗回 {count} 张弃牌(已随机打乱)-> 抽牌堆 {pool.DrawCount}");
            lastMessage = $"洗回 {count} 张(已打乱)";
        }

        private void DiscardAllHand()
        {
            int count = pool.HandCount;
            pool.DiscardAllHand();
            Debug.Log($"[OrbTest] 弃掉全部手牌 {count} 张 -> 弃牌堆 {pool.DiscardCount}");
            lastMessage = $"弃掉 {count} 张";
        }

        private string GroupPreviewText()
        {
            List<ISignalOrb> visible = new List<ISignalOrb>(VisibleRow());
            List<OrbPlayGroup> groups = ThreeMatchDetector.GetAllGroups(visible);
            if (groups.Count == 0)
            {
                return "(空)";
            }
            var parts = new List<string>();
            foreach (OrbPlayGroup group in groups)
            {
                string tag = group.MatchCount == 1 ? "单" : (group.MatchCount == 2 ? "2消" : "3消");
                parts.Add($"{ColorName(group.Color)}x{group.MatchCount}[{tag}]");
            }
            return string.Join("  ", parts);
        }

        private string HandPreviewText()
        {
            if (pool.HandCount == 0)
            {
                return "(空)";
            }
            var parts = new List<string>();
            foreach (OrbInstance orb in pool.Hand)
            {
                parts.Add(ShortLabel(orb));
            }
            return string.Join(" ", parts);
        }

        private static string RowText(IReadOnlyList<ISignalOrb> orbs)
        {
            var parts = new List<string>();
            foreach (ISignalOrb orb in orbs)
            {
                parts.Add(ShortLabel((OrbInstance)orb));
            }
            return string.Join(" ", parts);
        }

        private static string PilePreviewText(IReadOnlyList<OrbInstance> pile)
        {
            if (pile.Count == 0)
            {
                return "(空)";
            }
            var parts = new List<string>();
            foreach (OrbInstance orb in pile)
            {
                parts.Add(ShortLabel(orb));
            }
            return string.Join(" ", parts);
        }

        private static bool IsBlocker(OrbInstance orb)
        {
            return orb.Locked || orb.Data.Color == OrbColor.White;
        }

        private static string ShortLabel(OrbInstance orb)
        {
            if (orb.Locked)
            {
                return "L";
            }
            switch (orb.Color)
            {
                case (int)OrbColor.Red: return "R";
                case (int)OrbColor.Yellow: return "Y";
                case (int)OrbColor.Blue: return "B";
                case (int)OrbColor.White: return "W";
                default: return "?";
            }
        }

        private static string OrbLabel(OrbInstance orb)
        {
            string colorName = ColorName(orb.Color);
            string suffix = string.Empty;
            if (orb.Locked) { suffix = " [LOCKED]"; }
            if (orb.ExhaustOnUse) { suffix += " [EX]"; }
            if (orb.Retained) { suffix += " [RET]"; }
            return $"[{colorName}]{suffix}";
        }

        private static string ColorName(int color)
        {
            switch (color)
            {
                case (int)OrbColor.Red: return "红";
                case (int)OrbColor.Yellow: return "黄";
                case (int)OrbColor.Blue: return "蓝";
                case (int)OrbColor.White: return "白";
                default: return "未知";
            }
        }
    }
}
