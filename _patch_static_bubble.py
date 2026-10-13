from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('        private const string SpeechBubblePrefabPath = "OpenShop/CustomerSpeechBubble";\n\n', '')
t = t.replace('''        private ShopCustomerSpeechBubble interactionBubble;
        private GameObject interactionBubbleObject;
        private GameObject interactionBubblePrefab;
''', '''        [SerializeField, InspectorName("状态气泡")]
        private ShopCustomerSpeechBubble interactionBubble;
''')
start = t.find('        private bool TryGetInteractionBubble()')
end = t.find('        private Transform EnsureSpeechBubbleAnchor()')
print(start, end)
assert start > 0 and end > start
replacement = '''        private bool TryGetInteractionBubble()
        {
            if (interactionBubble != null)
                return true;

            Transform anchor = speechBubbleAnchor != null ? speechBubbleAnchor : transform.Find("气泡挂点");
            if (anchor != null)
                interactionBubble = anchor.GetComponentInChildren<ShopCustomerSpeechBubble>(true);
            if (interactionBubble != null)
                return true;

            Debug.LogError("顾客预制体的气泡挂点下缺少状态气泡。", this);
            return false;
        }

'''
t = t[:start] + replacement + t[end:]
anchor_start = t.find('        private Transform EnsureSpeechBubbleAnchor()')
anchor_end = t.find('        private void StartInteractionIndicator')
print('anchor', anchor_start, anchor_end)
assert anchor_start > 0 and anchor_end > anchor_start
t = t[:anchor_start] + t[anchor_end:]
assert 'Instantiate(interactionBubblePrefab' not in t
assert 'Resources.Load' not in t
p.write_text(t, encoding='utf-8', newline='\n')
print('ai ok')
