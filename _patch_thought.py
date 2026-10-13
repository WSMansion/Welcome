from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
text = p.read_text(encoding='utf-8')
if 'using TMPro;' not in text:
    text = text.replace('using UnityEngine;\n', 'using TMPro;\nusing UnityEngine;\n', 1)
old = '        private ShopCustomerSpeechBubble interactionBubble;'
neu = '''        [SerializeField, InspectorName("头顶内心文字")]
        private TMP_Text thoughtText;
        private ShopCustomerSpeechBubble interactionBubble;'''
assert old in text
text = text.replace(old, neu, 1)
old2 = '''        private void ShowEmptyDisplayIndicator(float duration)
        {
            ShowBubbleText(duration);
        }'''
neu2 = '''        private void ShowEmptyDisplayIndicator(float duration)
        {
            if (!TryShowThoughtText(duration))
                ShowBubbleText(duration);
        }

        private bool TryShowThoughtText(float duration)
        {
            TMP_Text text = EnsureThoughtText();
            if (text == null)
                return false;

            if (interactionBubble != null)
                interactionBubble.Hide();
            text.gameObject.SetActive(true);
            if (text.transform.parent != null)
                text.transform.parent.gameObject.SetActive(true);
            indicatorTimer = Mathf.Max(0.2f, duration);
            UpdateThoughtSorting();
            return true;
        }

        private TMP_Text EnsureThoughtText()
        {
            if (thoughtText != null)
                return thoughtText;

            Transform named = transform.Find("Canvas/TextThought");
            if (named != null)
                thoughtText = named.GetComponent<TMP_Text>();
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
assert old2 in text, 'show missing'
text = text.replace(old2, neu2, 1)
old3 = '''            if (indicatorTimer > 0f)
            {
                indicatorTimer -= deltaTime;
                if (indicatorTimer <= 0f && !socializing)
                    interactionBubble.Hide();
            }
            UpdateIndicatorSortingOrder();'''
neu3 = '''            if (indicatorTimer > 0f)
            {
                indicatorTimer -= deltaTime;
                if (indicatorTimer <= 0f && !socializing)
                {
                    interactionBubble.Hide();
                    HideThoughtText();
                }
            }
            UpdateIndicatorSortingOrder();
            UpdateThoughtSorting();'''
assert old3 in text, 'timer missing'
text = text.replace(old3, neu3, 1)
old4 = '''            if (interactionBubble != null)
                interactionBubble.Hide();
            indicatorTimer = 0f;'''
neu4 = '''            if (interactionBubble != null)
                interactionBubble.Hide();
            HideThoughtText();
            indicatorTimer = 0f;'''
assert old4 in text, 'clear missing'
text = text.replace(old4, neu4, 1)
anchor = '        private void ClearInteractionIndicator()'
hide = '''        private void HideThoughtText()
        {
            if (thoughtText == null)
                return;

            thoughtText.gameObject.SetActive(false);
            if (thoughtText.transform.parent != null)
                thoughtText.transform.parent.gameObject.SetActive(false);
        }

        private void ClearInteractionIndicator()'''
assert anchor in text
text = text.replace(anchor, hide, 1)
p.write_text(text, encoding='utf-8', newline='\n')
print('patched')
