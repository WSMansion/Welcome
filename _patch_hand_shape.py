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
            Color32 motion = new Color32(255, 248, 230, 210);
            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            string[] rows =
            {
                ".........................ooooo................",
                "........................o.....o...............",
                "........................o.....o...............",
                "...................ooooo......o...............",
                "..................o...........o...............",
                "..................o...........o...............",
                ".............ooooo............o...............",
                "............o.................o...............",
                "............o.................o...............",
                ".......ooooo..................o...............",
                "......o.......................o...............",
                "......o.......................o...............",
                "......o.......................o...............",
                "......o.......................ooooooo.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                "......o.............................o.........",
                ".......ooooooooooooooooooooooooooooo..........",
                "..........o........................o..........",
                "..........o........................o..........",
                "..........o........................o..........",
                "..........oooooooooooooooooooooooooo.........."
            };

            int originX = 8;
            int originY = 12;
            for (int row = 0; row < rows.Length; row++)
            {
                string line = rows[row];
                int y = originY + rows.Length - 1 - row;
                for (int column = 0; column < line.Length; column++)
                {
                    if (line[column] != 'o')
                        continue;
                    int x = originX + column;
                    bool edge = IsHandEdge(rows, row, column);
                    SetPixel(pixels, size, x, y, edge ? outline : column < 4 || row > 22 ? shade : skin);
                }
            }

            DrawMotionArc(pixels, size, 31, 34, 27, motion);
            return pixels;
        }

        private static bool IsHandEdge(string[] rows, int row, int column)
        {
            for (int y = row - 1; y <= row + 1; y++)
            {
                for (int x = column - 1; x <= column + 1; x++)
                {
                    if (y < 0 || y >= rows.Length || x < 0 || x >= rows[y].Length || rows[y][x] != 'o')
                        return true;
                }
            }
            return false;
        }

'''
t = t[:start] + new + t[end:]
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
