from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSpeechBubble.cs')
t = p.read_text(encoding='utf-8')
old = '''        [Header("图标资源")]'''
new = '''        [SerializeField, Min(0f), InspectorName("图标内边距"),
         Tooltip("气泡底图会按图标尺寸加上这里的左右、上下留白自动拉伸。")]
        private Vector2 iconPadding = new Vector2(0.18f, 0.14f);

        [Header("图标资源")]'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            RestoreDefaultAppearance();
            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;
            backgroundRenderer.enabled = true;
            return true;'''
new2 = '''            RestoreDefaultAppearance();
            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;
            backgroundRenderer.enabled = true;
            FitBackgroundToIcon();
            return true;'''
assert old2 in t
t = t.replace(old2, new2, 1)
old3 = '''        private void RestoreDefaultAppearance()'''
new3 = '''        private void FitBackgroundToIcon()
        {
            if (backgroundRenderer == null || backgroundRenderer.drawMode == SpriteDrawMode.Simple
                || iconRenderer == null || iconRenderer.sprite == null)
                return;

            Vector2 iconSize = Vector2.Scale(iconRenderer.sprite.bounds.size, Abs(iconRenderer.transform.localScale));
            backgroundRenderer.size = new Vector2(
                Mathf.Max(defaultBackgroundSize.x, iconSize.x + iconPadding.x),
                Mathf.Max(defaultBackgroundSize.y, iconSize.y + iconPadding.y));
        }

        private static Vector2 Abs(Vector3 scale)
        {
            return new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }

        private void RestoreDefaultAppearance()'''
assert old3 in t
t = t.replace(old3, new3, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
