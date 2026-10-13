from PIL import Image
from collections import Counter
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\72\72701481f98bd3a3ccb10f194527b7d2f4885c706394599393a1b790a671e028.png').convert('RGB')
small = source.resize((64, 64), Image.Resampling.BOX)
colors = Counter(small.getdata())
for color, count in colors.most_common(12):
    print(count, color)
print('size', source.size)
