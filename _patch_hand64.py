from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('EnsureSpriteAsset(WaveSpritePath, 32, 32, CreateWavePixels);', 'EnsureSpriteAsset(WaveSpritePath, 64, 64, CreateWavePixels);')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static void DrawHandFinger')
assert start > 0 and end > start
new = r'''        private static Color32[] CreateWavePixels()
        {
            const int size = 64;
            Color32 skin = new Color32(245, 214, 176, 255);
            Color32 shade = new Color32(214, 166, 128, 255);
            Color32 outline = new Color32(92, 64, 52, 255);
            Color32 motion = new Color32(255, 248, 230, 190);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            DrawMotionArc(pixels, size, 18, 34, 42, motion);
            DrawHandFinger(pixels, size, 18, 38, 12, 5, skin, shade, outline);
            DrawHandFinger(pixels, size, 26, 42, 14, 5, skin, shade, outline);
            DrawHandFinger(pixels, size, 34, 42, 14, 5, skin, shade, outline);
            DrawHandFinger(pixels, size, 42, 36, 11, 5, skin, shade, outline);
            FillHandRect(pixels, size, 20, 18, 46, 39, skin, shade, outline);
            FillHandRect(pixels, size, 10, 23, 21, 33, skin, shade, outline);
            FillHandRect(pixels, size, 30, 8, 38, 19, skin, shade, outline);
            return pixels;
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

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
