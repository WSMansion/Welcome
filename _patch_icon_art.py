from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('''        private const string HappySpritePath = "Assets/Art/OpenShop/customer_mood_happy.png";''', '''        private const string HappySpritePath = "Assets/Art/OpenShop/customer_mood_happy.png";
        private const string AngrySpritePath = "Assets/Art/OpenShop/customer_mood_angry.png";
        private const string SadSpritePath = "Assets/Art/OpenShop/customer_mood_sad.png";
        private const string WaveSpritePath = "Assets/Art/OpenShop/customer_mood_wave.png";
        private const string BrowseTogetherSpritePath = "Assets/Art/OpenShop/customer_mood_browse.png";''')
t = t.replace('''            EnsureSpriteAsset(HappySpritePath, 14, 14, CreateHappyPixels);''', '''            EnsureSpriteAsset(HappySpritePath, 14, 14, CreateHappyPixels);
            EnsureSpriteAsset(AngrySpritePath, 14, 14, CreateAngryPixels);
            EnsureSpriteAsset(SadSpritePath, 14, 14, CreateSadPixels);
            EnsureSpriteAsset(WaveSpritePath, 14, 14, CreateWavePixels);
            EnsureSpriteAsset(BrowseTogetherSpritePath, 14, 14, CreateBrowseTogetherPixels);''')
insert = '''
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
            Color32[] pixels = CreateFaceBase(out Color32 outline);
            Color32 hand = new Color32(244, 214, 176, 255);
            SetPixel(pixels, 14, 4, 9, outline); SetPixel(pixels, 14, 5, 9, outline);
            SetPixel(pixels, 14, 9, 9, outline); SetPixel(pixels, 14, 10, 9, outline);
            SetPixel(pixels, 14, 6, 6, outline); SetPixel(pixels, 14, 7, 5, outline);
            SetPixel(pixels, 14, 8, 6, outline);
            SetPixel(pixels, 14, 11, 8, hand); SetPixel(pixels, 14, 12, 9, hand);
            SetPixel(pixels, 14, 12, 10, outline);
            return pixels;
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

        private static void SetPixel'''
assert 'private static void SetPixel' in t
t = t.replace('        private static void SetPixel', insert, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('builder updated')
