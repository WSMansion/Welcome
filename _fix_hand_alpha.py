from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\72\72701481f98bd3a3ccb10f194527b7d2f4885c706394599393a1b790a671e028.png').convert('RGBA')
pixels = source.load()
width, height = source.size
for y in range(height):
    for x in range(width):
        r, g, b, a = pixels[x, y]
        if r > 225 and g > 225 and b > 225:
            pixels[x, y] = (0, 0, 0, 0)
out = source.resize((64, 64), Image.Resampling.NEAREST)
out.save(r'WelCome/Assets/Art/OpenShop/customer_mood_wave.png')
print(out.size, out.mode)
