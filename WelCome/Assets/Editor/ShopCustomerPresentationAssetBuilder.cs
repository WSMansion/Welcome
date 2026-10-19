using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WelCome.World;

namespace WelCome.EditorTools
{
    internal static class ShopCustomerPresentationAssetBuilder
    {
        private const int PixelsPerUnit = 96;
        private const string BubbleSpritePath = "Assets/Art/OpenShop/customer_speech_bubble.png";
        private const string ThoughtIconSpritePath = "Assets/Art/OpenShop/customer_thought.png";
        private const string GreetingIconSpritePath = "Assets/Art/OpenShop/customer_exclamation.png";
        private const string UnhappySpritePath = "Assets/Art/OpenShop/customer_mood_down.png";
        private const string HappySpritePath = "Assets/Art/OpenShop/customer_mood_happy.png";
        private const string AngrySpritePath = "Assets/Art/OpenShop/customer_mood_angry.png";
        private const string SadSpritePath = "Assets/Art/OpenShop/customer_mood_sad.png";
        private const string WaveSpritePath = "Assets/Art/OpenShop/customer_mood_wave.png";
        private const string BrowseTogetherSpritePath = "Assets/Art/OpenShop/customer_mood_browse.png";
        private const string CustomerBubblePrefabPath = "Assets/Prefabs/OpenShop/CustomerSpeechBubble.prefab";
        private const string InteractionPromptPrefabPath = "Assets/Prefabs/OpenShop/InteractionPrompt.prefab";

        [MenuItem("代号开店/更新顾客表情资源")]
        private static void UpdateCustomerMoodAssetsFromMenu()
        {
            EnsureCustomerMoodAssets();
            AssetDatabase.SaveAssets();
        }

        public static void EnsureCustomerMoodAssets()
        {
            EnsureCustomerBubbleSpriteAssets();
            AssignCustomerMoodSprites();
        }

        private static void AssignCustomerMoodSprites()
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(CustomerBubblePrefabPath);
            try
            {
                ShopCustomerSpeechBubble bubble = prefab.GetComponent<ShopCustomerSpeechBubble>();
                SerializedObject serialized = new SerializedObject(bubble);
                AssignSprite(serialized, "angryIcon", AngrySpritePath);
                AssignSprite(serialized, "sadIcon", SadSpritePath);
                AssignSprite(serialized, "waveIcon", WaveSpritePath);
                AssignSprite(serialized, "browseTogetherIcon", BrowseTogetherSpritePath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefab, CustomerBubblePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void AssignSprite(SerializedObject serialized, string propertyName, string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                throw new InvalidOperationException("缺少顾客表情图片：" + assetPath);

            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException("顾客气泡缺少表情字段：" + propertyName);
            property.objectReferenceValue = sprite;
        }

        public static void EnsurePresentationPrefabs()
        {
            EnsureCustomerBubbleSupportAssets();
            EnsureInteractionPromptScaler();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureCustomerBubbleSupportAssets()
        {
            EnsureCustomerBubbleSpriteAssets();
            ValidateCustomerBubblePrefab();
        }

        public static void EnsureCustomerBubbleSpriteAssets()
        {
            EnsureSpriteAsset(BubbleSpritePath, 36, 28, CreateBubblePixels);
            EnsureSpriteAsset(ThoughtIconSpritePath, 64, 64, CreateThoughtIconPixels);
            EnsureSpriteAsset(GreetingIconSpritePath, 64, 64, CreateGreetingIconPixels);
            EnsureSpriteAsset(UnhappySpritePath, 64, 64, CreateUnhappyPixels);
            EnsureSpriteAsset(HappySpritePath, 64, 64, CreateHappyPixels);
            EnsureSpriteAsset(AngrySpritePath, 64, 64, CreateAngryPixels);
            EnsureSpriteAsset(SadSpritePath, 64, 64, CreateSadPixels);
            EnsureSpriteAsset(WaveSpritePath, 64, 64, CreateWavePixels);
            EnsureSpriteAsset(BrowseTogetherSpritePath, 64, 64, CreateBrowseTogetherPixels);
        }

        private static Sprite EnsureSpriteAsset(string assetPath, int width, int height, Func<Color32[]> pixelFactory)
        {
            string relativeAssetPath = assetPath.Substring("Assets/".Length)
                .Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(Application.dataPath, relativeAssetPath);
            if (!File.Exists(fullPath))
            {
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = Path.GetFileNameWithoutExtension(assetPath),
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                texture.SetPixels32(pixelFactory());
                texture.Apply(false, false);
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("无法设置气泡图片导入方式：" + assetPath);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            if (string.Equals(assetPath, BubbleSpritePath, StringComparison.Ordinal))
            {
                TextureImporterSettings spriteSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteBorder = new Vector4(16f, 10f, 3f, 3f);
                spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(spriteSettings);
            }
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                throw new InvalidOperationException("无法加载气泡图片：" + assetPath);
            return sprite;
        }

        private static void ValidateCustomerBubblePrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CustomerBubblePrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("缺少顾客状态气泡预制体：" + CustomerBubblePrefabPath);

            ShopCustomerSpeechBubble bubble = prefab.GetComponent<ShopCustomerSpeechBubble>();
            if (bubble == null || !bubble.HasRequiredVisuals)
                throw new InvalidOperationException("顾客状态气泡预制体缺少文字槽、图标槽或图标资源：" + CustomerBubblePrefabPath);

            if (prefab.GetComponentInChildren<WorldCameraScaleCompensation>(true) != null)
                throw new InvalidOperationException("顾客状态气泡不应挂镜头缩放补偿组件：" + CustomerBubblePrefabPath);
        }

        private static void EnsureInteractionPromptScaler()
        {
            GameObject prompt = PrefabUtility.LoadPrefabContents(InteractionPromptPrefabPath);
            try
            {
                if (prompt.GetComponent<WorldInteractionPrompt>() == null)
                    throw new InvalidOperationException("交互提示预制体缺少 WorldInteractionPrompt：" + InteractionPromptPrefabPath);

                WorldCameraScaleCompensation scale = prompt.GetComponent<WorldCameraScaleCompensation>();
                if (scale == null)
                    scale = prompt.AddComponent<WorldCameraScaleCompensation>();
                SerializedObject serializedScale = new SerializedObject(scale);
                SerializedProperty multiplier = serializedScale.FindProperty("sizeMultiplier");
                if (multiplier != null)
                    multiplier.floatValue = 1.5f;
                serializedScale.ApplyModifiedPropertiesWithoutUndo();

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(prompt, InteractionPromptPrefabPath);
                if (saved == null)
                    throw new InvalidOperationException("无法保存带镜头缩放组件的交互提示预制体：" + InteractionPromptPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prompt);
            }
        }

        private static Color32[] CreateBubblePixels()
        {
            const int width = 36;
            const int height = 28;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 outline = new Color32(105, 78, 73, 255);
            Color32 fill = new Color32(255, 247, 225, 255);
            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool outerBody = IsInsideRoundedRectangle(x, y, 3, 8, 33, 25, 4);
                    bool outerTail = IsInsideTriangle(new Vector2(x + 0.5f, y + 0.5f),
                        new Vector2(6f, 10f), new Vector2(6f, 2f), new Vector2(17f, 9f));
                    bool innerBody = IsInsideRoundedRectangle(x, y, 4, 9, 32, 24, 3);
                    bool innerTail = IsInsideTriangle(new Vector2(x + 0.5f, y + 0.5f),
                        new Vector2(7f, 10f), new Vector2(7f, 4f), new Vector2(14f, 9f));
                    pixels[y * width + x] = innerBody || innerTail
                        ? fill
                        : outerBody || outerTail ? outline : transparent;
                }
            }

            return pixels;
        }

        private static Color32[] CreateThoughtIconPixels()
        {
            const int size = 16;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 ink = new Color32(77, 62, 57, 255);
            Color32[] pixels = new Color32[size * size];
            for (int dot = 0; dot < 3; dot++)
            {
                int centerX = 3 + dot * 5;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (x * x + y * y <= 1)
                            SetPixel(pixels, size, centerX + x, 7 + y, ink);
                    }
                }
            }
            return pixels;
        }

        private static Color32[] CreateGreetingIconPixels()
        {
            const int size = 16;
            Color32[] pixels = new Color32[size * size];
            Color32 ink = new Color32(77, 62, 57, 255);
            for (int y = 3; y <= 13; y++)
            {
                int halfWidth = y >= 11 ? 1 : 2;
                for (int x = 8 - halfWidth; x <= 8 + halfWidth; x++)
                    SetPixel(pixels, size, x, y, ink);
            }
            for (int y = 0; y <= 1; y++)
            {
                for (int x = 7; x <= 9; x++)
                    SetPixel(pixels, size, x, y, ink);
            }
            return pixels;
        }

        private static Color32[] CreateUnhappyPixels()
        {
            const int size = 14;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 outline = new Color32(77, 62, 57, 255);
            Color32 face = new Color32(249, 199, 105, 255);
            Color32 tear = new Color32(76, 164, 202, 255);
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - 7;
                    int dy = y - 7;
                    int distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared <= 39)
                        pixels[y * size + x] = distanceSquared >= 29 ? outline : face;
                    else
                        pixels[y * size + x] = transparent;
                }
            }

            SetPixel(pixels, size, 4, 9, outline);
            SetPixel(pixels, size, 5, 9, outline);
            SetPixel(pixels, size, 9, 9, outline);
            SetPixel(pixels, size, 10, 9, outline);
            SetPixel(pixels, size, 4, 5, outline);
            SetPixel(pixels, size, 5, 6, outline);
            SetPixel(pixels, size, 6, 7, outline);
            SetPixel(pixels, size, 7, 7, outline);
            SetPixel(pixels, size, 8, 6, outline);
            SetPixel(pixels, size, 9, 5, outline);
            SetPixel(pixels, size, 10, 8, tear);
            SetPixel(pixels, size, 10, 7, tear);
            SetPixel(pixels, size, 11, 6, tear);
            return pixels;
        }

        private static Color32[] CreateHappyPixels()
        {
            const int size = 14;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 outline = new Color32(77, 62, 57, 255);
            Color32 face = new Color32(249, 199, 105, 255);
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - 7;
                    int dy = y - 7;
                    int distanceSquared = dx * dx + dy * dy;
                    pixels[y * size + x] = distanceSquared <= 39
                        ? distanceSquared >= 29 ? outline : face
                        : transparent;
                }
            }

            SetPixel(pixels, size, 4, 9, outline);
            SetPixel(pixels, size, 5, 9, outline);
            SetPixel(pixels, size, 9, 9, outline);
            SetPixel(pixels, size, 10, 9, outline);
            SetPixel(pixels, size, 4, 6, outline);
            SetPixel(pixels, size, 5, 5, outline);
            SetPixel(pixels, size, 6, 4, outline);
            SetPixel(pixels, size, 7, 4, outline);
            SetPixel(pixels, size, 8, 4, outline);
            SetPixel(pixels, size, 9, 5, outline);
            SetPixel(pixels, size, 10, 6, outline);
            return pixels;
        }

        private static bool IsInsideRoundedRectangle(int x, int y, int left, int bottom,
            int right, int top, int radius)
        {
            if (x < left || x > right || y < bottom || y > top)
                return false;

            int nearestX = Mathf.Clamp(x, left + radius, right - radius);
            int nearestY = Mathf.Clamp(y, bottom + radius, top - radius);
            int dx = x - nearestX;
            int dy = y - nearestY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool IsInsideTriangle(Vector2 point, Vector2 first, Vector2 second, Vector2 third)
        {
            float firstSide = Cross(point, first, second);
            float secondSide = Cross(point, second, third);
            float thirdSide = Cross(point, third, first);
            bool hasNegative = firstSide < 0f || secondSide < 0f || thirdSide < 0f;
            bool hasPositive = firstSide > 0f || secondSide > 0f || thirdSide > 0f;
            return !(hasNegative && hasPositive);
        }

        private static float Cross(Vector2 point, Vector2 first, Vector2 second)
        {
            return (point.x - second.x) * (first.y - second.y)
                - (first.x - second.x) * (point.y - second.y);
        }


        private static Color32[] CreateAngryPixels()
        {
            Color32[] pixels = CreateFaceBase(out Color32 outline);
            SetPixel(pixels, 14, 4, 10, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 10, outline);
            SetPixel(pixels, 14, 4, 9, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 9, outline);
            SetPixel(pixels, 14, 5, 5, outline); SetPixel(pixels, 14, 6, 5, outline);
            SetPixel(pixels, 14, 7, 5, outline); SetPixel(pixels, 14, 8, 5, outline);
            return pixels;
        }

        private static Color32[] CreateSadPixels()
        {
            Color32[] pixels = CreateFaceBase(out Color32 outline);
            Color32 tear = new Color32(76, 164, 202, 255);
            SetPixel(pixels, 14, 4, 8, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 8, outline);
            SetPixel(pixels, 14, 4, 9, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 9, outline);
            SetPixel(pixels, 14, 5, 5, outline); SetPixel(pixels, 14, 6, 6, outline);
            SetPixel(pixels, 14, 8, 6, outline); SetPixel(pixels, 14, 9, 5, outline);
            SetPixel(pixels, 14, 5, 7, tear); SetPixel(pixels, 14, 5, 6, tear);
            return pixels;
        }

        private static Color32[] CreateWavePixels()
        {
            const int size = 64;
            Color32 skin = new Color32(245, 214, 176, 255);
            Color32 shade = new Color32(214, 166, 128, 255);
            Color32 outline = new Color32(92, 64, 52, 255);
            Color32 motion = new Color32(255, 248, 230, 220);
            bool[,] mask = new bool[size, size];

            StampOval(mask, 34, 26, 15, 13);
            StampCapsule(mask, 22, 30, 22, 50, 4);
            StampCapsule(mask, 30, 31, 30, 54, 4);
            StampCapsule(mask, 38, 31, 38, 53, 4);
            StampCapsule(mask, 46, 29, 46, 48, 4);
            StampCapsule(mask, 16, 28, 28, 24, 5);
            StampCapsule(mask, 32, 6, 36, 18, 6);

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!mask[x, y])
                        continue;
                    bool edge = !HasMask(mask, x - 1, y) || !HasMask(mask, x + 1, y)
                        || !HasMask(mask, x, y - 1) || !HasMask(mask, x, y + 1);
                    pixels[y * size + x] = edge ? outline : x < 20 ? shade : skin;
                }
            }

            ColorMotion(pixels, size, motion);
            return pixels;
        }

        private static void StampCapsule(bool[,] mask, int x0, int y0, int x1, int y1, int radius)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int step = 0; step <= steps; step++)
            {
                float t = steps == 0 ? 0f : step / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                StampOval(mask, x, y, radius, radius);
            }
        }

        private static void StampPalm(bool[,] mask)
        {
            for (int y = 14; y <= 38; y++)
            {
                float progress = (y - 14) / 24f;
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(8f, 16f, Mathf.Sin(progress * Mathf.PI)));
                for (int x = 32 - halfWidth; x <= 34 + halfWidth; x++)
                    StampPixel(mask, x, y);
            }
        }

        private static void StampFinger(bool[,] mask, int centerX, int baseY, int width, int height, int lean)
        {
            int radius = width / 2;
            for (int step = 0; step <= height; step++)
            {
                int y = baseY + step;
                int x = centerX + Mathf.RoundToInt(lean * step / (float)height);
                int currentRadius = step > height - radius ? radius - (step - (height - radius)) : radius;
                for (int dx = -currentRadius; dx <= currentRadius; dx++)
                    StampPixel(mask, x + dx, y);
            }
        }

        private static void StampThumb(bool[,] mask)
        {
            for (int i = 0; i < 14; i++)
            {
                int x = 18 - Mathf.RoundToInt(i * 0.65f);
                int y = 27 + Mathf.RoundToInt(Mathf.Sin(i / 13f * Mathf.PI) * 6f);
                int radius = i < 10 ? 4 : 5;
                StampOval(mask, x, y, radius, radius - 1);
            }
        }

        private static void StampWrist(bool[,] mask)
        {
            for (int y = 5; y <= 16; y++)
            {
                int halfWidth = Mathf.RoundToInt(Mathf.Lerp(6f, 9f, (y - 5) / 11f));
                for (int x = 30 - halfWidth; x <= 34 + halfWidth; x++)
                    StampPixel(mask, x, y);
            }
        }

        private static void DrawFingerCrease(Color32[] pixels, int size, int x, int y, int length, Color32 color)
        {
            for (int i = 0; i < length; i++)
                SetPixel(pixels, size, x + i, y + (i % 3 == 0 ? 1 : 0), color);
        }

        private static void StampOval(bool[,] filled, int centerX, int centerY, int radiusX, int radiusY)
        {
            for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    float nx = (x - centerX) / (float)radiusX;
                    float ny = (y - centerY) / (float)radiusY;
                    if (nx * nx + ny * ny <= 1f)
                        StampPixel(filled, x, y);
                }
            }
        }

        private static void StampRect(bool[,] filled, int left, int bottom, int right, int top)
        {
            for (int y = bottom; y <= top; y++)
            {
                for (int x = left; x <= right; x++)
                    StampPixel(filled, x, y);
            }
        }

        private static void DrawMotionArc(bool[,] filled, int centerX, int centerY, int radius)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = Mathf.Lerp(50f, 140f, i / 15f) * Mathf.Deg2Rad;
                int x = centerX + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                int y = centerY + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                StampPixel(filled, x, y);
                StampPixel(filled, x, y + 1);
            }
        }

        private static void ColorMotion(Color32[] pixels, int size, Color32 motion)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = Mathf.Lerp(50f, 140f, i / 15f) * Mathf.Deg2Rad;
                int x = 34 + Mathf.RoundToInt(Mathf.Cos(angle) * 24f);
                int y = 39 + Mathf.RoundToInt(Mathf.Sin(angle) * 24f);
                SetPixel(pixels, size, x, y, motion);
                SetPixel(pixels, size, x, y + 1, motion);
            }
        }

        private static void StampPixel(bool[,] filled, int x, int y)
        {
            if (x >= 0 && y >= 0 && x < filled.GetLength(0) && y < filled.GetLength(1))
                filled[x, y] = true;
        }

        private static bool HasMask(bool[,] filled, int x, int y)
        {
            return IsFilled(filled, x, y);
        }

        private static bool IsFilled(bool[,] filled, int x, int y)
        {
            return x >= 0 && y >= 0 && x < filled.GetLength(0) && y < filled.GetLength(1) && filled[x, y];
        }

        private static void DrawMotionArc(Color32[] pixels, int size, int centerX, int centerY,
            int radius, Color32 color)
        {
            for (int i = 0; i < 18; i++)
            {
                float angle = Mathf.Lerp(35f, 145f, i / 17f) * Mathf.Deg2Rad;
                int x = centerX + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                int y = centerY + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                SetPixel(pixels, size, x, y, color);
                SetPixel(pixels, size, x + 1, y, color);
            }
        }

        private static void DrawHandFinger(Color32[] pixels, int size, int x, int y, int height,
            int width, Color32 skin, Color32 shade, Color32 outline)
        {
            FillHandRect(pixels, size, x, y, x + width - 1, y + height, skin, shade, outline);
        }

        private static void FillHandRect(Color32[] pixels, int size, int left, int bottom,
            int right, int top, Color32 fill, Color32 shade, Color32 outline)
        {
            for (int y = bottom; y <= top; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    bool border = x == left || x == right || y == bottom || y == top;
                    bool shaded = !border && (x == left + 1 || y == bottom + 1);
                    SetPixel(pixels, size, x, y, border ? outline : shaded ? shade : fill);
                }
            }
        }

        private static void FillRect(Color32[] pixels, int size, int left, int bottom,
            int right, int top, Color32 fill, Color32 outline)
        {
            for (int y = bottom; y <= top; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    bool border = x == left || x == right || y == bottom || y == top;
                    SetPixel(pixels, size, x, y, border ? outline : fill);
                }
            }
        }

        private static Color32[] CreateBrowseTogetherPixels()
        {
            Color32[] pixels = CreateFaceBase(out Color32 outline);
            Color32 heart = new Color32(214, 92, 92, 255);
            SetPixel(pixels, 14, 4, 9, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 9, outline);
            SetPixel(pixels, 14, 5, 6, outline); SetPixel(pixels, 14, 6, 5, outline);
            SetPixel(pixels, 14, 8, 5, outline); SetPixel(pixels, 14, 9, 6, outline);
            SetPixel(pixels, 14, 7, 8, heart); SetPixel(pixels, 14, 6, 8, heart);
            SetPixel(pixels, 14, 8, 8, heart);
            return pixels;
        }

        private static Color32[] CreateFaceBase(out Color32 outline)
        {
            const int size = 14;
            outline = new Color32(77, 62, 57, 255);
            Color32 face = new Color32(249, 199, 105, 255);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - 7;
                    int dy = y - 7;
                    int distanceSquared = dx * dx + dy * dy;
                    pixels[y * size + x] = distanceSquared <= 39
                        ? distanceSquared >= 29 ? outline : face
                        : new Color32(0, 0, 0, 0);
                }
            }
            return pixels;
        }

        private static void SetPixel(Color32[] pixels, int size, int x, int y, Color32 color)
        {
            if (x < 0 || x >= size || y < 0 || y >= size)
                return;
            pixels[y * size + x] = color;
        }
    }
}
