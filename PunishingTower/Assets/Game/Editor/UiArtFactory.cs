using System.IO;
using UnityEditor;
using UnityEngine;

namespace PunishingTower.Editor
{
    /// <summary>
    /// Procedurally generates UI art (cards, orb icons, intent icons, panels).
    /// Output follows asset naming conventions (doc 162) under Assets/Game/UI/Art/.
    /// All textures are generated at edit time and stored as Sprites.
    /// </summary>
    public static class UiArtFactory
    {
        private const string ArtRoot = "Assets/Game/UI/Art";

        private const int CardSize = 128;
        private const int IconSize = 96;
        private const int IntentSize = 64;

        // ---------------- Entry point ----------------

        [MenuItem("PunishingTower/UI/Generate Art")]
        public static void GenerateAll()
        {
            EnsureDirectory();

            // Orb card backgrounds (red / yellow / blue / white)
            SaveSprite(CreateOrbCard(ColorHex(0x9E2B25), ColorHex(0x5E1512)), "UI_Card_Orb_Red");
            SaveSprite(CreateOrbCard(ColorHex(0x8A6D0B), ColorHex(0x4C3A04)), "UI_Card_Orb_Yellow");
            SaveSprite(CreateOrbCard(ColorHex(0x1F4E79), ColorHex(0x0E2740)), "UI_Card_Orb_Blue");
            SaveSprite(CreateOrbCard(ColorHex(0x4A4A4A), ColorHex(0x232323)), "UI_Card_Orb_White");

            // Locked (corrupted) card variant
            SaveSprite(CreateOrbCard(ColorHex(0x3A2B4A), ColorHex(0x1C1424)), "UI_Card_Orb_Corrupted");

            // Orb ball icons
            SaveSprite(CreateOrbBall(ColorHex(0xE0403A)), "UI_Icon_Orb_Red");
            SaveSprite(CreateOrbBall(ColorHex(0xE3B51C)), "UI_Icon_Orb_Yellow");
            SaveSprite(CreateOrbBall(ColorHex(0x3E82D4)), "UI_Icon_Orb_Blue");
            SaveSprite(CreateOrbBall(ColorHex(0xD8D8D8)), "UI_Icon_Orb_White");
            SaveSprite(CreateOrbBall(ColorHex(0x6E5A8A), true), "UI_Icon_Orb_Corrupted");

            // Intent icons
            SaveSprite(CreateIntentAttack(), "UI_Icon_Intent_Attack");
            SaveSprite(CreateIntentDefense(), "UI_Icon_Intent_Defense");
            SaveSprite(CreateIntentBuff(), "UI_Icon_Intent_Buff");
            SaveSprite(CreateIntentDebuff(), "UI_Icon_Intent_Debuff");
            SaveSprite(CreateIntentSpecial(), "UI_Icon_Intent_Special");

            // Panel background (dark rounded panel, Slay-the-Spire style)
            SaveSprite(CreatePanel(), "UI_Panel_Dark");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UiArtFactory: generated UI art under " + ArtRoot);
        }

        // ---------------- Helpers ----------------

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder(ArtRoot))
            {
                string parent = ArtRoot.Substring(0, ArtRoot.LastIndexOf('/'));
                string leaf = ArtRoot.Substring(ArtRoot.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        private static void SaveSprite(Texture2D tex, string name)
        {
            string path = $"{ArtRoot}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        private static Color ColorHex(uint rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }

        // ---------------- Orb card (rounded rect, gradient, highlight) ----------------

        private static Texture2D CreateOrbCard(Color deep, Color dark)
        {
            var tex = new Texture2D(CardSize, CardSize, TextureFormat.RGBA32, false);
            for (int y = 0; y < CardSize; y++)
            {
                for (int x = 0; x < CardSize; x++)
                {
                    tex.SetPixel(x, y, CardPixel(x, y, deep, dark));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Color CardPixel(int x, int y, Color deep, Color dark)
        {
            // Rounded rect mask with 14px corner radius and 3px border.
            float cx = CardSize / 2f;
            float cy = CardSize / 2f;
            float rx = cx - 14f;
            float ry = cy - 14f;

            float nx = x < cx ? (x - cx + rx) : (x - cx - rx);
            float ny = y < cy ? (y - cy + ry) : (y - cy - ry);
            bool insideCorner = nx > 0 && ny > 0 ? (nx * nx + ny * ny) <= 14f * 14f : true;
            bool inside = x >= 14 && x < CardSize - 14 && y >= 14 && y < CardSize - 14 && insideCorner;

            // Border band.
            bool border = false;
            {
                float bx = x < cx ? (x - cx + rx + 3) : (x - cx - rx - 3);
                float by = y < cy ? (y - cy + ry + 3) : (y - cy - ry - 3);
                bool cornerOk = bx > 0 && by > 0 ? (bx * bx + by * by) <= 11f * 11f : true;
                bool inBorder = x >= 11 && x < CardSize - 11 && y >= 11 && y < CardSize - 11 && cornerOk;
                border = inBorder && !inside;
            }

            if (!inside && !border)
            {
                return Color.clear;
            }

            // Vertical gradient from deep (top) to dark (bottom).
            float t = 1f - y / (float)CardSize;
            Color baseColor = Color.Lerp(deep, dark, t);

            if (border)
            {
                return Color.Lerp(baseColor, Color.white, 0.25f);
            }

            // Top highlight sheen.
            float sheen = Mathf.Clamp01(1f - y / (CardSize * 0.45f));
            baseColor = Color.Lerp(baseColor, Color.white, sheen * 0.12f);

            return baseColor;
        }

        // ---------------- Orb ball icon ----------------

        private static Texture2D CreateOrbBall(Color color, bool corrupted = false)
        {
            var tex = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            float r = IconSize / 2f - 6f;
            Vector2 center = new Vector2(IconSize / 2f, IconSize / 2f);

            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                    float dist = p.magnitude;
                    Color col = Color.clear;

                    if (dist <= r)
                    {
                        // Radial shading: lighter at top-left.
                        float light = Vector2.Dot(p.normalized, new Vector2(-0.6f, 0.8f)) * 0.5f + 0.5f;
                        col = Color.Lerp(color, Color.white, light * 0.35f);
                        // Darker rim.
                        float rim = Mathf.Clamp01((dist / r) - 0.75f) * 0.6f;
                        col = Color.Lerp(col, Color.black, rim);

                        if (corrupted)
                        {
                            // Desaturate + purple tint + dark cracks look (simple).
                            float g = (col.r + col.g + col.b) / 3f;
                            col = new Color(g * 0.8f, g * 0.65f, g * 0.95f, 1f);
                        }
                    }
                    else if (dist <= r + 3f)
                    {
                        // Outline.
                        col = Color.Lerp(color, Color.white, 0.3f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            return tex;
        }

        // ---------------- Intent icons (simple vector-ish pixel art) ----------------

        private static Texture2D CreateIntentAttack()
        {
            var tex = Blank(IntentSize);
            // Sword: diagonal blade + crossguard + hilt.
            DrawLine(tex, 18, 46, 44, 20, Color.white);   // blade
            DrawLine(tex, 19, 47, 45, 21, Color.white);
            DrawLine(tex, 16, 40, 30, 26, ColorHex(0xAAAAAA)); // guard top
            DrawLine(tex, 12, 48, 26, 34, ColorHex(0xAAAAAA)); // guard bottom
            DrawLine(tex, 12, 52, 16, 48, ColorHex(0x999999)); // hilt
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateIntentDefense()
        {
            var tex = Blank(IntentSize);
            // Shield: rounded top, pointed bottom.
            for (int y = 10; y < 52; y++)
            {
                float progress = (y - 10f) / 42f;
                float half = 10f + (14f - 10f) * progress; // 10 -> 14 px half width
                if (progress > 0.8f)
                {
                    half = 14f - (4f * (progress - 0.8f) / 0.2f); // taper to point
                }
                for (int x = 0; x < IntentSize; x++)
                {
                    if (x >= IntentSize / 2 - half && x <= IntentSize / 2 + half)
                    {
                        tex.SetPixel(x, y, ColorHex(0xC0C8D8));
                    }
                }
            }
            // Inner emblem.
            for (int x = 28; x < 36; x++)
            {
                for (int y = 22; y < 40; y++)
                {
                    tex.SetPixel(x, y, ColorHex(0x5A6478));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateIntentBuff()
        {
            var tex = Blank(IntentSize);
            // Upward arrow.
            for (int x = 26; x < 38; x++)
            {
                for (int y = 22; y < 48; y++)
                {
                    tex.SetPixel(x, y, ColorHex(0x7FD97F));
                }
            }
            DrawLine(tex, 20, 28, 32, 16, ColorHex(0x7FD97F));
            DrawLine(tex, 21, 28, 33, 16, ColorHex(0x7FD97F));
            DrawLine(tex, 44, 28, 32, 16, ColorHex(0x7FD97F));
            DrawLine(tex, 43, 28, 31, 16, ColorHex(0x7FD97F));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateIntentDebuff()
        {
            var tex = Blank(IntentSize);
            // Downward arrow.
            for (int x = 26; x < 38; x++)
            {
                for (int y = 16; y < 42; y++)
                {
                    tex.SetPixel(x, y, ColorHex(0xE07070));
                }
            }
            DrawLine(tex, 20, 36, 32, 48, ColorHex(0xE07070));
            DrawLine(tex, 21, 36, 33, 48, ColorHex(0xE07070));
            DrawLine(tex, 44, 36, 32, 48, ColorHex(0xE07070));
            DrawLine(tex, 43, 36, 31, 48, ColorHex(0xE07070));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateIntentSpecial()
        {
            var tex = Blank(IntentSize);
            // Star-like sparkle.
            Color c = ColorHex(0xFFD75F);
            DrawLine(tex, 32, 8, 32, 56, c);
            DrawLine(tex, 8, 32, 56, 32, c);
            DrawLine(tex, 20, 20, 44, 44, c);
            DrawLine(tex, 44, 20, 20, 44, c);
            tex.Apply();
            return tex;
        }

        // ---------------- Panel ----------------

        private static Texture2D CreatePanel()
        {
            var tex = Blank(256);
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    bool inside = x >= 8 && x < 248 && y >= 8 && y < 248;
                    // Corner rounding (20px).
                    float cx = 20f;
                    if (x < 8 + cx && y < 8 + cx)
                    {
                        float dx = x - (8 + cx), dy = y - (8 + cx);
                        inside = dx * dx + dy * dy <= cx * cx;
                    }
                    else if (x >= 248 - cx && y < 8 + cx)
                    {
                        float dx = x - (248 - cx), dy = y - (8 + cx);
                        inside = dx * dx + dy * dy <= cx * cx;
                    }
                    else if (x < 8 + cx && y >= 248 - cx)
                    {
                        float dx = x - (8 + cx), dy = y - (248 - cx);
                        inside = dx * dx + dy * dy <= cx * cx;
                    }
                    else if (x >= 248 - cx && y >= 248 - cx)
                    {
                        float dx = x - (248 - cx), dy = y - (248 - cx);
                        inside = dx * dx + dy * dy <= cx * cx;
                    }

                    if (inside)
                    {
                        tex.SetPixel(x, y, new Color(0.10f, 0.11f, 0.14f, 0.92f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // ---------------- Low level ----------------

        private static Texture2D Blank(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
            return tex;
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                {
                    tex.SetPixel(x0, y0, color);
                }
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}
