from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\df\dfcaa9abc2630bc9452629a5d95445349ef23d7800561e5cbe5c4258988a29d9.png').convert('RGBA')
pixels = source.load()
minx, miny, maxx, maxy = source.width, source.height, 0, 0
count = 0
for y in range(source.height):
    for x in range(source.width):
        r, g, b, a = pixels[x, y]
        if r > 120 and r > g + 25 and r > b + 25 and g < 150 and b < 150:
            pixels[x, y] = (176, 58, 62, 255)
            count += 1
            minx, miny = min(minx, x), min(miny, y)
            maxx, maxy = max(maxx, x), max(maxy, y)
        else:
            pixels[x, y] = (0, 0, 0, 0)
icon = source.crop((minx, miny, maxx + 1, maxy + 1))
side = max(icon.size)
square = Image.new('RGBA', (side, side), (0, 0, 0, 0))
square.paste(icon, ((side - icon.width) // 2, (side - icon.height) // 2), icon)
square.resize((64, 64), Image.Resampling.BOX).save(r'WelCome/Assets/Art/OpenShop/customer_mood_angry.png')
print(source.size, count, icon.size)
