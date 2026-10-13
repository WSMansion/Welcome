from PIL import Image, ImageDraw
from pathlib import Path
root = Path(r'WelCome/Assets/Art/OpenShop')
size = 64
outline = (92, 64, 52, 255)
face = (249, 214, 112, 255)
shade = (214, 164, 82, 255)
tear = (76, 164, 202, 255)
red = (214, 82, 82, 255)
white = (255, 248, 230, 255)

def canvas():
    image = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.ellipse((8, 8, 55, 55), fill=face, outline=outline, width=3)
    return image, draw

def eyes(draw, y=24, angry=False, sad=False):
    if angry:
        draw.line((18, 20, 27, 25), fill=outline, width=3)
        draw.line((46, 20, 37, 25), fill=outline, width=3)
    elif sad:
        draw.line((18, 26, 27, 21), fill=outline, width=3)
        draw.line((46, 26, 37, 21), fill=outline, width=3)
    else:
        draw.ellipse((18, y, 25, y+7), fill=outline)
        draw.ellipse((39, y, 46, y+7), fill=outline)

def save(name, image):
    image.save(root / name)

happy, draw = canvas()
eyes(draw)
draw.arc((20, 28, 44, 48), 20, 160, fill=outline, width=3)
save('customer_mood_happy.png', happy)

down, draw = canvas()
eyes(draw, sad=True)
draw.arc((21, 38, 43, 50), 200, 340, fill=outline, width=3)
draw.line((45, 31, 47, 42), fill=tear, width=3)
save('customer_mood_down.png', down)

angry, draw = canvas()
eyes(draw, angry=True)
draw.line((22, 43, 42, 43), fill=outline, width=4)
draw.polygon([(30, 18), (34, 18), (32, 13)], fill=red)
save('customer_mood_angry.png', angry)

sad, draw = canvas()
eyes(draw, sad=True)
draw.arc((21, 39, 43, 51), 200, 340, fill=outline, width=3)
draw.ellipse((42, 34, 49, 46), fill=tear, outline=outline)
save('customer_mood_sad.png', sad)

thought = Image.new('RGBA', (size, size), (0, 0, 0, 0))
draw = ImageDraw.Draw(thought)
draw.ellipse((8, 24, 20, 36), fill=outline)
draw.ellipse((24, 20, 40, 36), fill=outline)
draw.ellipse((44, 24, 56, 36), fill=outline)
save('customer_thought.png', thought)

greeting = Image.new('RGBA', (size, size), (0, 0, 0, 0))
draw = ImageDraw.Draw(greeting)
draw.rounded_rectangle((27, 6, 37, 42), radius=5, fill=red, outline=outline, width=2)
draw.ellipse((25, 46, 39, 60), fill=red, outline=outline, width=2)
save('customer_exclamation.png', greeting)

together, draw = canvas()
eyes(draw)
draw.arc((20, 30, 44, 48), 20, 160, fill=outline, width=3)
draw.polygon([(32, 20), (27, 25), (29, 31), (32, 28), (35, 31), (37, 25)], fill=red)
save('customer_mood_browse.png', together)
print('icons saved')
