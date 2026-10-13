from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
old = '''        private static void DrawHandFinger(Color32[] pixels, int size, int x, int y, int height,
            Color32 skin, Color32 shade, Color32 outline)
        {
            FillHandRect(pixels, size, x, y, x + 3, y + height, skin, shade, outline);
        }'''
new = '''        private static void DrawHandFinger(Color32[] pixels, int size, int x, int y, int height,
            int width, Color32 skin, Color32 shade, Color32 outline)
        {
            FillHandRect(pixels, size, x, y, x + width - 1, y + height, skin, shade, outline);
        }'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('fixed')
