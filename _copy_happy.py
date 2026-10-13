from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\c5\c5ef8fb35fda1a6daafd9931753fdfcc26d32318eee85ae20baf4359c7dd7440.png').convert('RGBA')
print(source.size, source.getextrema())
pixels = source.load()
minx, miny, maxx, maxy = source.width, source.height, 0, 0
count = 0
for y in range(source.height):
    for x in range(source.width):
        r, g, b, a = pixels[x, y]
        checker = (r > 230 and g > 230 and b > 230) or (abs(r - g) < 8 and abs(g - b) < 8 and 180 < r < 230)
        if a < 20 or checker:
            pixels[x, y] = (0, 0, 0, 0)
        else:
            count += 1
            minx, miny = min(minx, x), min(miny, y)
            maxx, maxy = max(maxx, x), max(maxy, y)
icon = source.crop((minx, miny, maxx + 1, maxy + 1))
side = max(icon.size)
square = Image.new('RGBA', (side, side), (0, 0, 0, 0))
square.paste(icon, ((side - icon.width) // 2, (side - icon.height) // 2), icon)
square.resize((64, 64), Image.Resampling.BOX).save(r'WelCome/Assets/Art/OpenShop/customer_mood_happy.png')
print(count, icon.size)
