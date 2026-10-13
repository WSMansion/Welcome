from PIL import Image
source = Image.open(r'C:\Users\XD\AppData\Roaming\Cindy\cindy-media\blobs\12\127849e44f9d411d4e2386e4d15299e3090f60cc5be1d15e31d3d06fbbe1ed13.png').convert('RGB')
panel = source.crop((0, 0, source.width // 3, source.height))
found = []
for y in range(0, panel.height, 4):
    row = []
    for x in range(0, panel.width, 4):
        r, g, b = panel.getpixel((x, y))
        row.append('R' if r > 150 and g < 130 and b < 130 else '.')
    if 'R' in row:
        found.append(f'{y:03d} ' + ''.join(row))
print('\n'.join(found[:40]))
print('rows', len(found), 'panel', panel.size)
