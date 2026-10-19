from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            interactionBubble.transform.SetParent(anchor, false);
            interactionBubble.transform.localPosition = Vector3.zero;
            interactionBubble.transform.localRotation = Quaternion.identity;
            interactionBubble.transform.localScale = Vector3.one;'''
new = '''            interactionBubble.transform.SetParent(anchor, false);
            interactionBubble.transform.localPosition = Vector3.zero;
            interactionBubble.transform.localRotation = Quaternion.identity;'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('scale preserved')
