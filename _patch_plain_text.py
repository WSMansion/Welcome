from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('using TMPro;\n', 'using UnityEngine.UI;\n')
t = t.replace('TMP_Text', 'Text')
old = '''            Transform named = transform.Find("Canvas/TextThought");
            if (named != null)
                thoughtText = named.GetComponent<Text>();
            return thoughtText;'''
new = '''            Transform named = transform.Find("Canvas/TextThought");
            if (named == null)
                return null;

            thoughtText = named.GetComponent<Text>();
            if (thoughtText != null && thoughtText.font == null)
                thoughtText.font = WelCome.UI.UIHelper.DefaultFont;
            return thoughtText;'''
assert old in t, 'lookup missing'
t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('code ok')
