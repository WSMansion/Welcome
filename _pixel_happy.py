from PIL import Image
size = 32
image = Image.new('RGBA', (size, size), (0, 0, 0, 0))
pixels = image.load()
outline = (78, 58, 48, 255)
face = (246, 204, 78, 255)
shade = (232, 170, 58, 255)
eye = (58, 58, 78, 255)
mouth = (186, 62, 72, 255)
tongue = (236, 132, 132, 255)

def dot(x, y, color):
    if 0 <= x < size and 0 <= y < size:
        pixels[x, y] = color

for y in range(size):
    for x in range(size):
        dx, dy = x - 15.5, y - 15.5
        distance = dx * dx + dy * dy
        if distance <= 13.2 * 13.2:
            edge = distance >= 11.4 * 11.4
            dot(x, y, outline if edge else shade if y > 22 else face)

for offset, direction in ((8, 1), (23, -1)):
    for step in range(5):
        dot(offset + direction * step, 9 + step, eye)
        dot(offset + direction * step, 17 - step, eye)

for y in range(18, 27):
    half = 8 - abs(y - 22)
    for x in range(16 - half, 16 + half):
        edge = x == 16 - half or x == 15 + half or y in (18, 26)
        lower = y >= 23
        dot(x, y, outline if edge else tongue if lower else mouth)
image.resize((64, 64), Image.Resampling.NEAREST).save(r'WelCome/Assets/Art/OpenShop/customer_mood_happy.png')
print('pixel happy')
