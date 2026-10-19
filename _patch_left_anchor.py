from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''        [SerializeField, InspectorName("气泡挂点")]
        private Transform speechBubbleAnchor;'''
new = '''        [SerializeField, InspectorName("气泡挂点")]
        private Transform speechBubbleAnchor;
        [SerializeField, InspectorName("左侧气泡挂点")]
        private Transform leftSpeechBubbleAnchor;'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            if (!TryGetInteractionBubble())
                return;'''
new2 = '''            if (!TryGetInteractionBubble())
                return;

            PlaceInteractionBubble(ChooseSpeechBubbleAnchor());'''
assert old2 in t
t = t.replace(old2, new2, 1)
anchor = '        private bool TryGetInteractionBubble()'
method = '''        private Transform ChooseSpeechBubbleAnchor()
        {
            bool useLeft = socialPartner != null && transform.position.x > socialPartner.transform.position.x;
            if (useLeft && leftSpeechBubbleAnchor != null)
                return leftSpeechBubbleAnchor;
            return speechBubbleAnchor != null ? speechBubbleAnchor : transform;
        }

        private void PlaceInteractionBubble(Transform anchor)
        {
            if (interactionBubble == null || anchor == null || interactionBubble.transform.parent == anchor)
                return;

            interactionBubble.transform.SetParent(anchor, false);
            interactionBubble.transform.localPosition = Vector3.zero;
            interactionBubble.transform.localRotation = Quaternion.identity;
            interactionBubble.transform.localScale = Vector3.one;
        }

        private bool TryGetInteractionBubble()'''
assert anchor in t
t = t.replace(anchor, method, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
