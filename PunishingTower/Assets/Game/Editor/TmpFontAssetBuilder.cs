using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Creates the TextMeshPro font asset from the copied simhei.ttf (Chinese UI font).
    /// Step 1 (ImportEssentials): imports TMP Essential Resources so the TMP shader exists.
    /// Step 2 (CreateFontAsset): creates the SDF font asset.
    /// </summary>
    public static class TmpFontAssetBuilder
    {
        private const string FontTtfPath = "Assets/Game/UI/Fonts/simhei.ttf";
        private const string OutputPath = "Assets/Game/UI/Fonts/SimHei SDF.asset";
        private const string EssentialsPackage = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";

        [MenuItem("PunishingTower/UI/Import TMP Essentials")]
        public static void ImportEssentials()
        {
            if (TMP_Settings.instance != null)
            {
                Debug.Log("TMP settings already present, skipping import.");
                return;
            }
            Debug.Log("Importing TMP Essential Resources...");
            AssetDatabase.ImportPackage(EssentialsPackage, false);
        }

        [MenuItem("PunishingTower/UI/Create Chinese Font Asset")]
        public static void CreateFontAsset()
        {
            if (!File.Exists(FontTtfPath))
            {
                Debug.LogError("simhei.ttf not found at " + FontTtfPath);
                return;
            }

            AssetDatabase.Refresh();

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            if (font == null)
            {
                Debug.LogError("Failed to load font asset");
                return;
            }

            AssetDatabase.DeleteAsset(OutputPath);

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);

            // In batchmode CreateFontAsset may not allocate the atlas texture; do it manually.
            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0 ||
                fontAsset.atlasTextures[0] == null)
            {
                var atlas = new Texture2D(512, 512, TextureFormat.Alpha8, false);
                fontAsset.atlasTextures = new[] { atlas };
            }

            // Ensure a material with the TMP SDF shader exists (required for rendering).
            if (fontAsset.material == null)
            {
                Shader sdfShader = Shader.Find("TextMeshPro/Distance Field");
                if (sdfShader == null)
                {
                    Debug.LogError("TextMeshPro/Distance Field shader not found - import TMP Essentials first.");
                    return;
                }
                fontAsset.material = new Material(sdfShader);
                fontAsset.material.mainTexture = fontAsset.atlasTextures[0];
            }

            AssetDatabase.CreateAsset(fontAsset, OutputPath);

            // Persist material + atlas textures as sub-assets, otherwise they are lost in batchmode.
            if (fontAsset.material != null)
            {
                fontAsset.material.name = OutputPath.Substring(OutputPath.LastIndexOf('/') + 1) + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, OutputPath);
            }
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    Texture2D atlas = fontAsset.atlasTextures[i];
                    if (atlas != null)
                    {
                        atlas.name = OutputPath.Substring(OutputPath.LastIndexOf('/') + 1) + " Atlas " + i;
                        AssetDatabase.AddObjectToAsset(atlas, OutputPath);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("TMP Chinese font asset created at " + OutputPath);
        }
    }
}
