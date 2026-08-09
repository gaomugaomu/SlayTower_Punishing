using System.Collections.Generic;
using PunishingTower.Data;
using PunishingTower.SignalOrb;
using UnityEngine;

namespace PunishingTower.UI
{
    /// <summary>
    /// Manual test harness for the signal orb system (draw / hand / play / three match).
    /// No effects are applied - successful matches are only printed via Debug.Log.
    /// </summary>
    public class OrbTestDriver : MonoBehaviour
    {
        [SerializeField] private int redCount = 5;
        [SerializeField] private int yellowCount = 5;
        [SerializeField] private int blueCount = 5;
        [SerializeField] private int initialHand = 5;

        private OrbPool pool;
        private readonly List<OrbInstance> playedQueue = new List<OrbInstance>();
        private Vector2 handScroll;
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

        private void DrawInitialHand()
        {
            List<OrbInstance> drawn = pool.Draw(initialHand);
            Debug.Log($"[OrbTest] 开局抽牌:{drawn.Count} 张 | 抽牌堆剩余 {pool.DrawCount} | 手牌 {pool.HandCount}");
            if (drawn.Count < initialHand)
            {
                Debug.Log("[OrbTest] 警告:抽牌数量不足(抽牌堆+弃牌堆已空)");
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(20, 20, 720, 520));
            GUILayout.Label("=== Signal Orb Test (信号球手动测试) ===", GUI.skin.label);
            GUILayout.Label($"DrawPile 抽牌堆:{pool.DrawCount}   Hand 手牌:{pool.HandCount}   Discard 弃牌堆:{pool.DiscardCount}   Exhaust 耗尽:{pool.ExhaustCount}   Played 队列:{playedQueue.Count}", GUI.skin.label);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Draw 1 (抽1张)", GUILayout.Width(120)))
            {
                DrawOne();
            }
            if (GUILayout.Button("Draw 3 (抽3张)", GUILayout.Width(120)))
            {
                Draw(3);
            }
            if (GUILayout.Button("Shuffle (弃牌洗回)", GUILayout.Width(140)))
            {
                ShuffleBack();
            }
            if (GUILayout.Button("Discard All (弃全部手牌)", GUILayout.Width(180)))
            {
                DiscardAllHand();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("=== Played Queue (已打出序列,用于三消检测) ===", GUI.skin.label);
            GUILayout.Label(QueueText(), GUI.skin.label);
            GUILayout.Label("Last: " + lastMessage, GUI.skin.label);

            GUILayout.Space(10);
            GUILayout.Label("=== Hand (手牌,点击打出) ===", GUI.skin.label);
            handScroll = GUILayout.BeginScrollView(handScroll, GUILayout.Height(220));
            GUILayout.BeginVertical();
            for (int i = 0; i < pool.Hand.Count; i++)
            {
                OrbInstance orb = pool.Hand[i];
                GUILayout.BeginHorizontal();
                string label = OrbLabel(orb);
                if (GUILayout.Button(label, GUILayout.Width(160)))
                {
                    PlayOrb(orb);
                }
                GUILayout.Label($"pos {i}", GUILayout.Width(60));
                GUILayout.EndHorizontal();
            }
            if (pool.Hand.Count == 0)
            {
                GUILayout.Label("(手牌为空)", GUI.skin.label);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
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
            lastMessage = $"抽到 {OrbLabel(orb)} | 手牌 {pool.HandCount}";
            Debug.Log($"[OrbTest] 抽到 {OrbLabel(orb)} | 抽牌堆剩 {pool.DrawCount} | 手牌 {pool.HandCount}");
        }

        private void Draw(int count)
        {
            int before = pool.HandCount;
            List<OrbInstance> drawn = pool.Draw(count);
            Debug.Log($"[OrbTest] 抽 {drawn.Count} 张 | 手牌 {before} -> {pool.HandCount} | 抽牌堆剩 {pool.DrawCount}");
            lastMessage = $"抽 {drawn.Count} 张完成";
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

        private void DiscardAllHand()
        {
            int count = pool.HandCount;
            pool.DiscardAllHand();
            Debug.Log($"[OrbTest] 弃掉全部手牌 {count} 张 -> 弃牌堆 {pool.DiscardCount}");
            lastMessage = $"弃掉 {count} 张";
        }

        private void PlayOrb(OrbInstance orb)
        {
            if (orb.Locked)
            {
                Debug.Log($"[OrbTest] {OrbLabel(orb)} 是锁球,无法打出");
                lastMessage = "锁球无法打出";
                return;
            }

            pool.PlayFromHand(orb);
            playedQueue.Add(orb);
            Debug.Log($"[OrbTest] 打出 {OrbLabel(orb)} | 队列: {QueueText()}");

            List<ThreeMatchGroup> groups = ThreeMatchDetector.Detect(playedQueue);
            if (groups.Count == 0)
            {
                Debug.Log("[OrbTest] 未形成消球(连续同色不足3)");
                lastMessage = "未形成消球";
            }
            else
            {
                foreach (ThreeMatchGroup group in groups)
                {
                    string colorName = ColorName(group.Color);
                    Debug.Log($"*** 消球成功!*** {colorName} x{group.Count} (ThreeMatch)");
                    lastMessage = $"*** 消球成功 {colorName} x{group.Count} ***";
                }
            }
        }

        private string QueueText()
        {
            if (playedQueue.Count == 0)
            {
                return "(空)";
            }
            var parts = new List<string>();
            foreach (OrbInstance orb in playedQueue)
            {
                parts.Add(ShortLabel(orb));
            }
            return string.Join(" ", parts);
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
            return $"[{colorName}] {orb.Id}{suffix}";
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
