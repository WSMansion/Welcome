from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('EnsureSpriteAsset(WaveSpritePath, 14, 14, CreateWavePixels);', 'EnsureSpriteAsset(WaveSpritePath, 32, 32, CreateWavePixels);')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static void FillRect')
assert start > 0 and end > start
new = r'''        private static Color32[] CreateWavePixels()
        {
            const int size = 32;
            Color32 skin = new Color32(244, 210, 170, 255);
            Color32 shade = new Color32(214, 164, 126, 255);
            Color32 outline = new Color32(92, 64, 52, 255);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            DrawHandFinger(pixels, size, 7, 18, 5, skin, shade, outline);
            DrawHandFinger(pixels, size, 12, 20, 6, skin, shade, outline);
            DrawHandFinger(pixels, size, 17, 20, 6, skin, shade, outline);
            DrawHandFinger(pixels, size, 22, 17, 5, skin, shade, outline);
            FillHandRect(pixels, size, 8, 8, 24, 18, skin, shade, outline);
            FillHandRect(pixels, size, 4, 11, 9, 16, skin, shade, outline);
            FillHandRect(pixels, size, 14, 3, 18, 9, skin, shade, outline);
            return pixels;
        }

        private static void DrawHandFinger(Color32[] pixels, int size, int x, int y, int height,
            Color32 skin, Color32 shade, Color32 outline)
        {
            FillHandRect(pixels, size, x, y, x + 3, y + height, skin, shade, outline);
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

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
