from PIL import Image
from pathlib import Path
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\72\72701481f98bd3a3ccb10f194527b7d2f4885c706394599393a1b790a671e028.png').convert('RGBA')
target = source.resize((64, 64), Image.Resampling.BOX)
pixels = target.load()
skin = (245, 196, 72, 255)
shade = (214, 154, 42, 255)
outline = (92, 64, 52, 255)
motion = (122, 78, 42, 255)
for y in range(64):
    for x in range(64):
        r, g, b, a = pixels[x, y]
        if r > 245 and g > 245 and b > 245:
            pixels[x, y] = (0, 0, 0, 0)
            continue
        if b < 90 and r > 90 and g > 50 and abs(r - g) < 80:
            pixels[x, y] = motion if r < 170 else outline
            continue
        if r > 180 and g > 120:
            pixels[x, y] = shade if g < 165 else skin
        else:
            pixels[x, y] = (0, 0, 0, 0)
out = Path(r'WelCome/Assets/Art/OpenShop/customer_mood_wave.png')
target.save(out)
print(out, target.size)
