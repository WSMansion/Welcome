from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
old = '        private static bool IsFilled(bool[,] filled, int x, int y)'
new = '''        private static bool HasMask(bool[,] filled, int x, int y)
        {
            return IsFilled(filled, x, y);
        }

        private static bool IsFilled(bool[,] filled, int x, int y)'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('fixed')
