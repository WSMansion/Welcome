from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSpeechBubble.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('                    && authoredText != null\n', '')
start = t.find('        public bool ShowAuthoredText()')
end = t.find('        public void Hide()')
print('span', start, end)
assert start > 0 and end > start
t = t[:start] + t[end:]
old = '''            if (authoredText == null)
            {
                Transform textTransform = transform.Find("气泡文字");
                if (textTransform == null)
                    textTransform = transform.Find("气泡符号");
                if (textTransform != null)
                    authoredText = textTransform.GetComponent<TextMesh>();
            }

            if (authoredText != null && textRenderer == null)
                textRenderer = authoredText.GetComponent<MeshRenderer>();
'''
assert old in t, 'resolve missing'
t = t.replace(old, '', 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('authored calls', t.count('ShowAuthoredText'), 'field', t.count('authoredText'))
