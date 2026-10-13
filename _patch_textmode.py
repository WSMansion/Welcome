from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
a = t.find('        private void ShowEmptyDisplayIndicator')
b = t.find('        private bool TryShowThoughtText')
c = t.find('        private TMP_Text EnsureThoughtText')
d = t.find('        private void ShowBubbleText')
e = t.find('        private bool TryGetInteractionBubble')
print(a,b,c,d,e)
assert min(a,b,c,d,e) > 0 and a < b < c < d < e
replacement = '''        private void ShowEmptyDisplayIndicator(float duration)
        {
            ShowThoughtText(duration);
        }

        private void ShowThoughtText(float duration)
        {
            TMP_Text text = EnsureThoughtText();
            if (text == null)
            {
                Debug.LogError("顾客预制体缺少头顶内心文字 TextThought。", this);
                return;
            }

            if (interactionBubble != null)
                interactionBubble.Hide();
            text.gameObject.SetActive(true);
            if (text.transform.parent != null)
                text.transform.parent.gameObject.SetActive(true);
            indicatorTimer = Mathf.Max(0.2f, duration);
            UpdateThoughtSorting();
        }

'''
t = t[:a] + replacement + t[c:d] + t[e:]
assert 'ShowBubbleText' not in t and 'ShowAuthoredText' not in t
p.write_text(t, encoding='utf-8', newline='\n')
print('ok')
