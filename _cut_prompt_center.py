from PIL import Image
image = Image.open(r'WelCome/Assets/Art/OpenShop/interaction_prompt_e.png').convert('RGBA')
pixels = image.load()
width, height = image.size
visited = [[False] * width for _ in range(height)]
stack = [(width // 2, height // 2)]
removed = 0
while stack:
    x, y = stack.pop()
    if x < 0 or y < 0 or x >= width or y >= height or visited[y][x]:
        continue
    r, g, b, a = pixels[x, y]
    if a < 20 or r < 150 or g < 130 or b < 90:
        continue
    visited[y][x] = True
    pixels[x, y] = (0, 0, 0, 0)
    removed += 1
    stack.extend(((x+1,y),(x-1,y),(x,y+1),(x,y-1)))
image.save(r'WelCome/Assets/Art/OpenShop/interaction_prompt_e.png')
print(image.size, removed)
