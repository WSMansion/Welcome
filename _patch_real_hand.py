from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static void StampOval')
assert start > 0 and end > start
new = r'''        private static Color32[] CreateWavePixels()
        {
            const int size = 64;
            Color32 skin = new Color32(245, 214, 176, 255);
            Color32 shade = new Color32(205, 154, 116, 255);
            Color32 line = new Color32(154, 104, 78, 255);
            Color32 outline = new Color32(92, 64, 52, 255);
            Color32 motion = new Color32(255, 248, 230, 230);
            bool[,] mask = new bool[size, size];

            StampPalm(mask);
            StampFinger(mask, 20, 34, 7, 16, -8);
            StampFinger(mask, 29, 36, 7, 18, -2);
            StampFinger(mask, 38, 36, 7, 17, 3);
            StampFinger(mask, 46, 33, 6, 14, 9);
            StampThumb(mask);
            StampWrist(mask);

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!mask[x, y])
                        continue;
                    bool edge = !HasMask(mask, x - 1, y) || !HasMask(mask, x + 1, y)
                        || !HasMask(mask, x, y - 1) || !HasMask(mask, x, y + 1);
                    bool shaded = !edge && (x < 18 || y < 14);
                    pixels[y * size + x] = edge ? outline : shaded ? shade : skin;
                }
            }

            DrawFingerCrease(pixels, size, 22, 35, 8, line);
            DrawFingerCrease(pixels, size, 30, 37, 8, line);
            DrawFingerCrease(pixels, size, 39, 37, 8, line);
            DrawFingerCrease(pixels, size, 47, 34, 7, line);
            ColorMotion(pixels, size, motion);
            return pixels;
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

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
