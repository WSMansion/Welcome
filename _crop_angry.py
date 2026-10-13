from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\12\127849e44f9d411d4e2386e4d15299e3090f60cc5be1d15e31d3d06fbbe1ed13.png').convert('RGBA')
panel = source.crop((0, 70, source.width // 3, 255))
pixels = panel.load()
for y in range(panel.height):
    for x in range(panel.width):
        r, g, b, a = pixels[x, y]
        pixels[x, y] = (198, 48, 48, 255) if r > 145 and g < 145 and b < 145 else (0, 0, 0, 0)
bbox = panel.getbbox()
icon = panel.crop(bbox)
canvas = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
scale = 56 / max(icon.size)
fitted = icon.resize((max(1, round(icon.width * scale)), max(1, round(icon.height * scale))), Image.Resampling.BOX)
canvas.paste(fitted, ((64 - fitted.width) // 2, (64 - fitted.height) // 2), fitted)
canvas.save(r'WelCome/Assets/Art/OpenShop/customer_mood_angry.png')
print(panel.size, bbox, icon.size, fitted.size)
