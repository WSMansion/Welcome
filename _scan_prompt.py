from PIL import Image
im = Image.open(r'WelCome/Assets/Art/OpenShop/interaction_prompt_e.png').convert('RGBA')
pixels = im.load()
minx, miny, maxx, maxy = im.width, im.height, 0, 0
count = 0
for y in range(im.height):
    for x in range(im.width):
        r, g, b, a = pixels[x, y]
        if a > 200 and 70 < r < 130 and 40 < g < 90 and b < 70:
            count += 1
            minx, miny = min(minx, x), min(miny, y)
            maxx, maxy = max(maxx, x), max(maxy, y)
print('brown', count, minx, miny, maxx, maxy)
print('center from bottom', ((minx+maxx)/2), im.height - (miny+maxy)/2)
