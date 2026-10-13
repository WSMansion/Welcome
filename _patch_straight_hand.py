from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static void StampPalm')
if end < 0:
    end = t.find('        private static void StampOval')
assert start > 0 and end > start, (start, end)
new = r'''        private static Color32[] CreateWavePixels()
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

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
