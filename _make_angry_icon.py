from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\12\127849e44f9d411d4e2386e4d15299e3090f60cc5be1d15e31d3d06fbbe1ed13.png').convert('RGBA')
width, height = source.size
panel = source.crop((0, 0, width // 3, height))
pixels = panel.load()
minx, miny, maxx, maxy = panel.width, panel.height, 0, 0
for y in range(panel.height):
    for x in range(panel.width):
        r, g, b, a = pixels[x, y]
        if r > 140 and r > g + 30 and r > b + 30:
            pixels[x, y] = (198, 48, 48, 255)
            minx, miny = min(minx, x), min(miny, y)
            maxx, maxy = max(maxx, x), max(maxy, y)
        else:
            pixels[x, y] = (0, 0, 0, 0)
icon = panel.crop((minx, miny, maxx + 1, maxy + 1))
canvas = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
fitted = icon.resize((52, 52), Image.Resampling.NEAREST)
canvas.paste(fitted, (6, 6), fitted)
canvas.save(r'WelCome/Assets/Art/OpenShop/customer_mood_angry.png')
print(panel.size, (minx, miny, maxx, maxy), icon.size)
