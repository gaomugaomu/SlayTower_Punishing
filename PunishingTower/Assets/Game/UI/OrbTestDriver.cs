using System.Collections.Generic;
using PunishingTower.Core;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for the Punishing Gray Raven orb row.
    /// The row holds up to 8 orbs; clicking any orb resolves its play group
    /// (right-to-left grouping, max 3) and eliminates the whole group.
    /// No effects are applied - results are printed via Debug.Log.
    /// </summary>
    public class OrbTestDriver : MonoBehaviour
    {
        [SerializeField] private int redCount = 5;
        [SerializeField] private int yellowCount = 5;
        [SerializeField] private int blueCount = 5;
        [SerializeField] private int rowSize = 8;

        private OrbPool pool;
        private readonly List<OrbInstance> row = new List<OrbInstance>();
        private string lastMessage = string.Empty;

        private void Start()
        {
            BuildPool();
            FillRow();
        }

        private void BuildPool()
        {
            var defs = new List<OrbData>();
            for (int i = 0; i < redCount; i++) { defs.Add(CreateOrb("red_" + i, OrbColor.Red)); }
            for (int i = 0; i < yellowCount; i++) { defs.Add(CreateOrb("yellow_" + i, OrbColor.Yellow)); }
            for (int i = 0; i < blueCount; i++) { defs.Add(CreateOrb("blue_" + i, OrbColor.Blue)); }
            pool = new OrbPool(defs);
            Debug.Log($"[OrbTest] 球库构建完成:红 {redCount} 黄 {yellowCount} 蓝 {blueCount},共 {pool.TotalCount} 球");
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

        private void FillRow()
        {
            while (row.Count < rowSize)
            {
                OrbInstance orb = pool.Draw();
                if (orb == null)
                {
                    break;
                }
                row.Add(orb);
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(20, 20, 760, 540));
            GUILayout.Label("=== Signal Orb Row Test (信号球队列手动测试) ===", GUI.skin.label);
            GUILayout.Label($"DrawPile 抽牌堆:{pool.DrawCount}   Discard 弃牌堆:{pool.DiscardCount}   Exhaust 耗尽:{pool.ExhaustCount}   Row 队列:{row.Count}/{rowSize}", GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Draw 1 (抽1张补队列)", GUILayout.Width(160)))
            {
                DrawOne();
            }
            if (GUILayout.Button("Shuffle (弃牌洗回)", GUILayout.Width(150)))
            {
                ShuffleBack();
            }
            if (GUILayout.Button("Refill (补满队列)", GUILayout.Width(140)))
            {
                FillRow();
                Debug.Log($"[OrbTest] 补满队列 -> {row.Count} 球");
                lastMessage = $"补满队列 -> {row.Count}";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("=== Row (队列,从右往左判定,点击球消球) ===", GUI.skin.label);
            GUILayout.Label("Last: " + lastMessage, GUI.skin.label);

            GUILayout.Space(6);
            DrawRow();

            GUILayout.Space(10);
            GUILayout.Label("=== Group Preview (当前分组,右->左,每3个一组) ===", GUI.skin.label);
            GUILayout.Label(GroupPreviewText(), GUI.skin.label);

            GUILayout.Space(10);
            GUILayout.Label("=== DrawPile 抽牌堆 ===", GUI.skin.label);
            GUILayout.Label(PilePreviewText(pool.DrawPile), GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.Label("=== Discard 弃牌堆 ===", GUI.skin.label);
            GUILayout.Label(PilePreviewText(pool.Discard), GUI.skin.label);
            GUILayout.EndArea();
        }

        private void DrawRow()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < row.Count; i++)
            {
                OrbInstance orb = row[i];
                string label = OrbLabel(orb);
                bool clickable = !IsBlocker(orb);
                if (clickable)
                {
                    if (GUILayout.Button(label, GUILayout.Width(84), GUILayout.Height(64)))
                    {
                        PlayOrb(i);
                    }
                }
                else
                {
                    GUILayout.Label(label, GUILayout.Width(84), GUILayout.Height(64));
                }
            }
            for (int i = row.Count; i < rowSize; i++)
            {
                GUILayout.Label("(空)", GUILayout.Width(84), GUILayout.Height(64));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < row.Count; i++)
            {
                GUILayout.Label(i.ToString(), GUILayout.Width(84));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawOne()
        {
            OrbInstance orb = pool.Draw();
            if (orb == null)
            {
                lastMessage = "抽牌失败:抽牌堆与弃牌堆均为空";
                Debug.Log("[OrbTest] 抽牌失败:抽牌堆与弃牌堆均为空");
                return;
            }
            row.Add(orb);
            lastMessage = $"抽到 {OrbLabel(orb)} -> 队列 {row.Count}";
            Debug.Log($"[OrbTest] 抽到 {OrbLabel(orb)} | 抽牌堆剩 {pool.DrawCount} | 队列 {row.Count}");
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
            Debug.Log($"[OrbTest] 洗回 {count} 张弃牌 -> 抽牌堆 {pool.DrawCount}");
            lastMessage = $"洗回 {count} 张";
        }

        private void PlayOrb(int index)
        {
            OrbInstance orb = row[index];

            if (IsBlocker(orb))
            {
                Debug.Log($"[OrbTest] {OrbLabel(orb)} 是锁球/特殊球,无法消球");
                lastMessage = "锁球/特殊球无法消球";
                return;
            }

            OrbPlayGroup group = ThreeMatchDetector.ResolvePlay(row, index);
            if (group == null || group.MatchCount == 0)
            {
                Debug.Log("[OrbTest] 消球失败:未找到可消组(不应该发生)");
                lastMessage = "消球失败";
                return;
            }

            string colorName = ColorName(group.Color);
            string groupDesc = RowText(group.Orbs);

            Debug.Log($"[OrbTest] 选中 pos {index} {OrbLabel(orb)} | 判定组({group.MatchCount}消): {groupDesc}");

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
                row.Remove(g);
                pool.DiscardOrb(g);
            }

            Debug.Log($"[OrbTest] 消球完成:队列 {row.Count} 球 | 弃牌堆 {pool.DiscardCount} | 抽牌堆 {pool.DrawCount}");
        }

        private string GroupPreviewText()
        {
            List<OrbPlayGroup> groups = ThreeMatchDetector.GetAllGroups(row);
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
