from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''        [SerializeField, InspectorName("气泡挂点")]
        private Transform speechBubbleAnchor;
        [SerializeField, InspectorName("左侧气泡挂点")]
        private Transform leftSpeechBubbleAnchor;'''
new = '''        [SerializeField, InspectorName("气泡挂点")]
        private Transform speechBubbleAnchor;'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            if (!TryGetInteractionBubble())
                return;

            PlaceInteractionBubble(ChooseSpeechBubbleAnchor());'''
new2 = '''            if (!TryGetInteractionBubble())
                return;'''
assert old2 in t
t = t.replace(old2, new2, 1)
start = t.find('        private Transform ChooseSpeechBubbleAnchor()')
end = t.find('        private bool TryGetInteractionBubble()')
assert start > 0 and end > start
t = t[:start] + t[end:]
assert 'leftSpeechBubbleAnchor' not in t and 'ChooseSpeechBubbleAnchor' not in t
p.write_text(t, encoding='utf-8', newline='\n')
print('anchor removed')
