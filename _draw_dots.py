from PIL import Image, ImageDraw
image = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)
color = (92, 64, 52, 255)
for center in (14, 32, 50):
    draw.ellipse((center - 6, 26, center + 6, 38), fill=color)
image.save(r'WelCome/Assets/Art/OpenShop/customer_thought.png')
print(image.size)
