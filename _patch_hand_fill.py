from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
start = t.find('        private static Color32[] CreateWavePixels()')
end = t.find('        private static void DrawMotionArc')
assert start > 0 and end > start
new = r'''        private static Color32[] CreateWavePixels()
        {
            const int size = 64;
            Color32 skin = new Color32(245, 214, 176, 255);
            Color32 shade = new Color32(214, 166, 128, 255);
            Color32 outline = new Color32(92, 64, 52, 255);
            Color32 motion = new Color32(255, 248, 230, 230);
            bool[,] filled = new bool[size, size];
            StampOval(filled, 33, 27, 14, 12);
            StampRect(filled, 27, 8, 39, 24);
            StampOval(filled, 23, 43, 4, 9);
            StampOval(filled, 30, 46, 4, 11);
            StampOval(filled, 37, 46, 4, 11);
            StampOval(filled, 44, 42, 4, 8);
            StampOval(filled, 18, 25, 7, 5);
            DrawMotionArc(filled, 34, 39, 24);

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!filled[x, y])
                        continue;

                    bool edge = !IsFilled(filled, x - 1, y) || !IsFilled(filled, x + 1, y)
                        || !IsFilled(filled, x, y - 1) || !IsFilled(filled, x, y + 1);
                    bool shaded = !edge && (!IsFilled(filled, x - 2, y) || !IsFilled(filled, x, y - 2));
                    pixels[y * size + x] = edge ? outline : shaded ? shade : skin;
                }
            }

            ColorMotion(pixels, size, motion);
            return pixels;
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

        private static bool IsFilled(bool[,] filled, int x, int y)
        {
            return x >= 0 && y >= 0 && x < filled.GetLength(0) && y < filled.GetLength(1) && filled[x, y];
        }

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
