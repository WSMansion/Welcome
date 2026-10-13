from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static Color32[] CreateBrowseTogetherPixels()')
assert start > 0 and end > start
new = '''        private static Color32[] CreateWavePixels()
        {
            const int size = 14;
            Color32 skin = new Color32(244, 214, 176, 255);
            Color32 outline = new Color32(92, 68, 58, 255);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            FillRect(pixels, size, 6, 1, 8, 6, skin, outline);
            FillRect(pixels, size, 3, 6, 5, 8, skin, outline);
            FillRect(pixels, size, 5, 7, 7, 9, skin, outline);
            FillRect(pixels, size, 7, 6, 9, 8, skin, outline);
            FillRect(pixels, size, 9, 5, 11, 7, skin, outline);
            return pixels;
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

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
