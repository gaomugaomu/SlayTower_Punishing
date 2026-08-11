using System.IO;
using PunishingTower.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Builds the OrbCard prefab: vertical card with upper orb icon area and lower
    /// reserved description area (Slay the Spire style). Run from menu after art generation.
    /// </summary>
    public static class OrbCardPrefabBuilder
    {
        private const string PrefabPath = "Assets/Game/UI/Prefabs/OrbCard.prefab";
        private const string ArtRoot = "Assets/Game/UI/Resources/Art";

        [MenuItem("PunishingTower/UI/Build OrbCard Prefab")]
        public static void Build()
        {
            Sprite cardRed = LoadSprite("UI_Card_Orb_Red");
            Sprite iconRed = LoadSprite("UI_Icon_Orb_Red");
            Sprite iconLocked = LoadSprite("UI_Icon_Orb_Corrupted");

            if (cardRed == null || iconRed == null || iconLocked == null)
            {
                Debug.LogError("OrbCardPrefabBuilder: art assets missing - run PunishingTower/UI/Generate Art first.");
                return;
            }

            TMP_FontAsset font = LoadFont();

            GameObject root = new GameObject("OrbCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(OrbCardView));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(96, 132);
            var rootImage = root.GetComponent<Image>();
            rootImage.sprite = cardRed;
            rootImage.type = Image.Type.Simple;

            // Icon area (upper ~60%).
            GameObject iconGo = CreateChild("Icon", rootRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 30), new Vector2(76, 76));
            Image iconImage = iconGo.AddComponent<Image>();
            iconImage.sprite = iconRed;
            iconImage.raycastTarget = false;

            // Lock overlay (covers the icon when corrupted).
            GameObject lockGo = CreateChild("LockOverlay", rootRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 30), new Vector2(76, 76));
            Image lockImage = lockGo.AddComponent<Image>();
            lockImage.sprite = iconLocked;
            lockImage.raycastTarget = false;
            lockGo.SetActive(false);

            // Description area (lower part, reserved for affixes).
            GameObject descGo = CreateChild("Description", rootRect, new Vector2(0, 0f), new Vector2(0, 0f), new Vector2(0, 10), new Vector2(84, 52));
            TMP_Text descText = descGo.AddComponent<TextMeshProUGUI>();
            descText.font = font;
            descText.fontSize = 12;
            descText.alignment = TextAlignmentOptions.Top;
            descText.raycastTarget = false;
            descText.text = string.Empty;

            // Footer (color / corrupted label at the bottom edge).
            GameObject footerGo = CreateChild("Footer", rootRect, new Vector2(0, -1f), new Vector2(0, -1f), new Vector2(0, 4), new Vector2(84, 18));
            TMP_Text footerText = footerGo.AddComponent<TextMeshProUGUI>();
            footerText.font = font;
            footerText.fontSize = 11;
            footerText.alignment = TextAlignmentOptions.Center;
            footerText.raycastTarget = false;
            footerText.text = "[绾";

            var view = root.GetComponent<OrbCardView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("cardBackground").objectReferenceValue = rootImage;
            so.FindProperty("orbIcon").objectReferenceValue = iconImage;
            so.FindProperty("lockOverlay").objectReferenceValue = lockImage;
            so.FindProperty("descriptionText").objectReferenceValue = descText;
            so.FindProperty("footerText").objectReferenceValue = footerText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureDirectory();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            Debug.Log("OrbCardPrefabBuilder: prefab created at " + PrefabPath);
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtRoot}/{name}.png");
        }

        private static TMP_FontAsset LoadFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/UI/Fonts/SimHei SDF.asset");
            if (font == null)
            {
                Debug.LogWarning("SimHei SDF font missing - TextMeshPro texts will use default font.");
            }
            return font;
        }

        private static GameObject CreateChild(string name, RectTransform parent, Vector2 anchor,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        private static void EnsureDirectory()
        {
            const string dir = "Assets/Game/UI/Prefabs";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                AssetDatabase.CreateFolder("Assets/Game/UI", "Prefabs");
            }
        }
    }
}
