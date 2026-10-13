from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('using UnityEngine.UI;\n', '')
t = t.replace('private Text thoughtText;', 'private TextMesh thoughtText;')
t = t.replace('Text text = EnsureThoughtText();', 'TextMesh text = EnsureThoughtText();')
t = t.replace('''            text.gameObject.SetActive(true);
            if (text.transform.parent != null)
                text.transform.parent.gameObject.SetActive(true);''', '            text.gameObject.SetActive(true);')
old = '''        private Text EnsureThoughtText()
        {
            if (thoughtText != null)
                return thoughtText;

            Transform named = transform.Find("Canvas/TextThought");
            if (named == null)
                return null;

            thoughtText = named.GetComponent<Text>();
            if (thoughtText != null && thoughtText.font == null)
                thoughtText.font = WelCome.UI.UIHelper.DefaultFont;
            return thoughtText;
        }

        private void UpdateThoughtSorting()
        {
            if (thoughtText == null)
                return;

            Canvas canvas = thoughtText.GetComponentInParent<Canvas>(true);
            if (canvas == null || animationBinding == null || animationBinding.spriteRenderer == null)
                return;

            canvas.overrideSorting = true;
            canvas.sortingLayerID = animationBinding.spriteRenderer.sortingLayerID;
            canvas.sortingOrder = animationBinding.spriteRenderer.sortingOrder + 3;
        }'''
new = '''        private TextMesh EnsureThoughtText()
        {
            if (thoughtText != null)
                return thoughtText;

            Transform named = transform.Find("TextThought");
            if (named == null)
                return null;

            thoughtText = named.GetComponent<TextMesh>();
            if (thoughtText != null && thoughtText.font == null)
                thoughtText.font = WelCome.UI.UIHelper.DefaultFont;
            return thoughtText;
        }

        private void UpdateThoughtSorting()
        {
            if (thoughtText == null || animationBinding == null || animationBinding.spriteRenderer == null)
                return;

            MeshRenderer renderer = thoughtText.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            renderer.sortingLayerID = animationBinding.spriteRenderer.sortingLayerID;
            renderer.sortingOrder = animationBinding.spriteRenderer.sortingOrder + 3;
        }'''
assert old in t, 'block missing'
t = t.replace(old, new, 1)
old_hide = '''            thoughtText.gameObject.SetActive(false);
            if (thoughtText.transform.parent != null)
                thoughtText.transform.parent.gameObject.SetActive(false);'''
assert old_hide in t
t = t.replace(old_hide, '            thoughtText.gameObject.SetActive(false);', 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('code updated')
