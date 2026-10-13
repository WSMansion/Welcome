from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
text = p.read_text(encoding='utf-8')
old = '        [SerializeField, InspectorName("气泡位置偏移")]\n        private Vector3 interactionBubbleOffset = new Vector3(0.24f, 0.94f, -0.05f);'
neu = '        [SerializeField, InspectorName("气泡挂点")]\n        private Transform speechBubbleAnchor;'
assert old in text
text = text.replace(old, neu, 1)
old2 = '            interactionBubbleObject = Instantiate(interactionBubblePrefab, transform, false);\n            interactionBubbleObject.name = "顾客状态气泡";\n            interactionBubbleObject.transform.localPosition = interactionBubbleOffset;'
neu2 = '            interactionBubbleObject = Instantiate(interactionBubblePrefab, EnsureSpeechBubbleAnchor(), false);\n            interactionBubbleObject.name = "顾客状态气泡";\n            interactionBubbleObject.transform.localPosition = Vector3.zero;'
assert old2 in text
text = text.replace(old2, neu2, 1)
marker = '            return true;\n        }\n\n        private void StartInteractionIndicator'
insert = '''            return true;
        }

        private Transform EnsureSpeechBubbleAnchor()
        {
            if (speechBubbleAnchor != null)
                return speechBubbleAnchor;

            Transform existing = transform.Find("气泡挂点");
            if (existing != null)
            {
                speechBubbleAnchor = existing;
                return speechBubbleAnchor;
            }

            GameObject anchor = new GameObject("气泡挂点");
            speechBubbleAnchor = anchor.transform;
            speechBubbleAnchor.SetParent(transform, false);
            speechBubbleAnchor.localPosition = new Vector3(0.24f, 0.94f, -0.05f);
            return speechBubbleAnchor;
        }

        private void StartInteractionIndicator'''
assert marker in text
text = text.replace(marker, insert, 1)
p.write_text(text, encoding='utf-8', newline='\n')
print('patched')
