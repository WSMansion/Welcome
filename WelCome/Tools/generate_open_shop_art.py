from pathlib import Path
from PIL import Image, ImageDraw
import random
O=Path(__file__).resolve().parents[1]/'Assets'/'Art'/'OpenShop'; O.mkdir(parents=True,exist_ok=True)
I='#443c51'
def save(n,im): im.save(O/n)
def grass(seed,flowers=False):
 r=random.Random(seed); im=Image.new('RGBA',(32,32),'#87cf62'); d=ImageDraw.Draw(im)
 for _ in range(48): d.point((r.randrange(32),r.randrange(32)),fill=r.choice(['#79c255','#91d56a','#a0da72','#75bd50']))
 if flowers:
  for x,y,c in [(7,20,'#f3d976'),(25,12,'#e7a8c9'),(18,4,'#fff0b8')]:
   for dx,dy in [(0,0),(-1,0),(1,0),(0,-1),(0,1)]: d.point((x+dx,y+dy),fill=c)
 return im
for n,s,f in [('tile_grass_a.png',4,False),('tile_grass_b.png',17,False),('tile_grass_flowers.png',31,True)]: save(n,grass(s,f))
im=Image.new('RGBA',(32,32),'#827f7a'); d=ImageDraw.Draw(im)
for y in (1,10,19,28):
 for x in range((y*3)%8,32,8): d.rectangle((x,y,min(x+6,31),min(y+6,31)),fill=['#aaa69b','#b4b0a1','#9f9b91','#c0b9a8'][(x//8+y//8)%4],outline='#77766f')
save('tile_cobble.png',im)
im=Image.new('RGBA',(32,32),'#9a6548'); d=ImageDraw.Draw(im)
for y in range(0,32,8): d.rectangle((0,y,31,y+7),fill=['#a87352','#b78059','#a36e4f','#b47a56'][(y//8)%4]); d.line((0,y,31,y),fill='#704d43')
save('tile_wood.png',im)
def prop(n,size,fn):
 im=Image.new('RGBA',size,(0,0,0,0)); fn(ImageDraw.Draw(im)); save(n,im)
def house(d):
 d.ellipse((10,109,150,126),fill='#4f8154'); d.rectangle((24,40,136,112),fill='#f0d8ad',outline=I,width=3)
 d.polygon([(15,43),(39,18),(120,18),(145,43),(137,51),(23,51)],fill='#57455e',outline=I); d.rectangle((22,41,138,57),fill='#608e9a',outline=I,width=2)
 for x in [28,48,68,88,108,128]: d.rectangle((x,43,x+9,55),fill='#d8b766')
 d.rectangle((70,75,92,112),fill='#4d4357',outline=I,width=2); d.rectangle((75,80,87,94),fill='#91c5bb'); d.rectangle((42,68,61,89),fill='#85b9bc',outline=I,width=2)
 d.rectangle((99,63,123,83),fill='#d3a957',outline=I,width=2); d.polygon([(111,66),(114,71),(120,72),(115,76),(116,81),(111,78),(106,81),(107,76),(102,72),(108,71)],fill='#fff1bd')
prop('prop_workshop.png',(160,128),house)
def stall(d):
 d.ellipse((7,83,121,94),fill='#4e8153'); d.rectangle((13,22,115,47),fill='#5d8194',outline=I,width=3)
 for x,c in [(17,'#e7c46e'),(37,'#c7798b'),(57,'#e7c46e'),(77,'#c7798b'),(97,'#e7c46e')]: d.rectangle((x,24,x+14,43),fill=c)
 d.rectangle((20,48,26,79),fill='#704f48',outline=I); d.rectangle((101,48,107,79),fill='#704f48',outline=I); d.rectangle((13,68,115,82),fill='#8a5d49',outline=I,width=3); d.rectangle((11,64,117,71),fill='#d39a5d',outline=I,width=2)
 for x,c in [(24,'#70c1a8'),(40,'#d17fa0'),(58,'#e7c46e'),(76,'#8e81c4'),(94,'#77b6cf')]: d.rectangle((x,53,x+8,64),fill=c,outline=I)
prop('prop_stall.png',(128,96),stall)
def display(d):
 d.ellipse((3,52,93,62),fill='#4e8153'); d.rectangle((12,33,84,51),fill='#865942',outline=I,width=3); d.rectangle((8,28,88,36),fill='#d3a16b',outline=I,width=2)
 d.rectangle((17,21,31,29),fill='#8d79bc',outline=I,width=2); d.rectangle((39,17,48,28),fill='#72bea9',outline=I,width=2); d.polygon([(61,28),(67,15),(73,28)],fill='#76c5db',outline=I); d.rectangle((77,20,85,28),fill='#d8b86f',outline=I)
prop('prop_display.png',(96,64),display)
def bench(d):
 d.ellipse((3,68,92,78),fill='#4f8052'); d.rectangle((14,44,82,64),fill='#875942',outline=I,width=3); d.rectangle((10,39,86,47),fill='#c99560',outline=I,width=2)
 d.ellipse((36,22,63,42),fill='#568b8a',outline=I,width=3); d.rectangle((39,31,60,43),fill='#568b8a',outline=I,width=2); d.ellipse((39,25,60,35),fill='#83d6c0',outline='#3f6370',width=2); d.line((45,19,42,14,48,10,46,5),fill='#e9d98e',width=2)
prop('prop_alchemy.png',(96,80),bench)
def tree(d):
 d.ellipse((14,112,82,124),fill='#4c7c4f'); d.rectangle((42,73,56,116),fill='#805540',outline=I,width=3)
 for b,c in [((11,30,51,78),'#4d9656'),((37,12,85,64),'#63ad5d'),((17,48,79,94),'#78be62'),((28,26,71,78),'#8ccc68')]: d.ellipse(b,fill=c,outline='#487b4d',width=3)
prop('prop_tree.png',(96,128),tree)
def planter(d):
 d.ellipse((3,37,45,46),fill='#4f8052'); d.rectangle((9,27,39,40),fill='#a56c4f',outline=I,width=2); d.rectangle((7,24,41,29),fill='#d19b66',outline=I,width=2); d.ellipse((11,10,24,25),fill='#63aa5c',outline='#477a4e',width=2); d.ellipse((24,6,38,23),fill='#7cbd61',outline='#477a4e',width=2)
prop('prop_planter.png',(48,48),planter)
PAL=[('#3d3349','#5d8c92','#b54752'),('#9a543f','#7b9a63','#e0b35f'),('#3f5266','#9c6d83','#e1bd72'),('#755039','#497d74','#df7f5d')]
def visitor(k,face,frame):
 hair,coat,trim=PAL[k]; im=Image.new('RGBA',(24,32),(0,0,0,0)); d=ImageDraw.Draw(im); d.ellipse((4,27,20,31),fill='#4f8054'); s=frame%2
 if face in ('down','up'):
  for x in (7-s,13+s): d.rectangle((x,23,x+4,29),fill='#66594e',outline=I)
 else:
  if s==0:
   d.rectangle((7,24,11,29),fill='#66594e',outline=I)
   d.rectangle((13,22,17,29),fill='#66594e',outline=I)
  else:
   d.rectangle((7,22,11,29),fill='#66594e',outline=I)
   d.rectangle((13,24,17,29),fill='#66594e',outline=I)
 d.rectangle((6,14,18,25),fill=coat,outline=I); d.rectangle((8,14,16,17),fill=trim); d.rectangle((7,5,17,15),fill='#f0c2a0',outline=I)
 if face=='up': d.rectangle((6,4,18,12),fill=hair,outline=I)
 elif face=='left': d.rectangle((10,4,19,10),fill=hair,outline=I); d.rectangle((14,9,18,13),fill=hair)
 elif face=='right': d.rectangle((5,4,14,10),fill=hair,outline=I); d.rectangle((6,9,10,13),fill=hair)
 else: d.rectangle((5,4,19,9),fill=hair,outline=I); d.rectangle((6,8,9,12),fill=hair); d.rectangle((16,8,18,11),fill=hair)
 return im
for k in range(4):
 for face in ('down','left','right','up'):
  for frame in range(2): save(f'visitor_{k}_{face}_{frame}.png',visitor(k,face,frame))
def player(face,frame):
 im=Image.new('RGBA',(24,32),(0,0,0,0)); d=ImageDraw.Draw(im)
 d.ellipse((4,27,20,31),fill='#4b7550')
 step=frame%2
 if face in ('down','up'):
  d.rectangle((7-step,22,10-step,29),fill='#574b48',outline=I)
  d.rectangle((14+step,22,17+step,29),fill='#574b48',outline=I)
 else:
  if step==0:
   d.rectangle((7,24,11,29),fill='#574b48',outline=I)
   d.rectangle((13,22,17,29),fill='#574b48',outline=I)
  else:
   d.rectangle((7,22,11,29),fill='#574b48',outline=I)
   d.rectangle((13,24,17,29),fill='#574b48',outline=I)
 d.rectangle((6,14,18,25),fill='#4b8295',outline=I)
 d.rectangle((8,21,16,23),fill='#c79655')
 d.rectangle((8,14,16,17),fill='#b54752',outline=I)
 d.rectangle((15,15,18,17),fill='#e1bd72')
 skin='#f0c2a0'; hair='#302d3b'
 d.rectangle((7,5,17,16),fill=skin,outline=I)
 if face=='up':
  d.rectangle((6,4,18,15),fill=hair,outline=I)
  d.rectangle((8,13,16,16),fill='#4b8295')
 elif face=='left':
  d.rectangle((10,4,19,10),fill=hair,outline=I)
  d.rectangle((15,8,19,14),fill=hair)
  d.point((9,11),fill=I)
 elif face=='right':
  d.rectangle((5,4,14,10),fill=hair,outline=I)
  d.rectangle((5,8,9,14),fill=hair)
  d.point((15,11),fill=I)
 else:
  d.rectangle((5,4,19,9),fill=hair,outline=I)
  d.rectangle((6,8,9,12),fill=hair)
  d.rectangle((16,8,18,12),fill=hair)
  d.point((10,11),fill=I); d.point((15,11),fill=I)
 # A small gold clasp keeps the player readable among the visitors.
 d.rectangle((11,18,13,20),fill='#f1d27a',outline=I)
 return im
for face in ('down','left','right','up'):
 for frame in range(2): save(f'player_{face}_{frame}.png',player(face,frame))
print('Generated',len(list(O.glob('*.png'))),'sprites in',O)
