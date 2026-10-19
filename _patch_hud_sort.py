from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            renderer.sortingLayerID = animationBinding.spriteRenderer.sortingLayerID;
            int baseOrder = animationBinding != null && animationBinding.spriteRenderer != null
                ? animationBinding.spriteRenderer.sortingOrder
                : 300 + Mathf.RoundToInt(-transform.position.y * 10f);
            renderer.sortingOrder = baseOrder + 3;'''
new = '''            renderer.sortingLayerID = animationBinding.spriteRenderer.sortingLayerID;
            renderer.sortingOrder = HudSortingOrder(4);'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            interactionBubble.SetSorting(animationBinding.spriteRenderer.sortingLayerID,
                animationBinding.spriteRenderer.sortingOrder + 2);'''
new2 = '''            interactionBubble.SetSorting(animationBinding.spriteRenderer.sortingLayerID,
                HudSortingOrder(2));'''
assert old2 in t
t = t.replace(old2, new2, 1)
anchor = '        private void UpdateIndicatorSortingOrder()'
method = '''        private int HudSortingOrder(int localOffset)
        {
            return 20000 + Mathf.RoundToInt(-transform.position.y * 10f) + localOffset;
        }

        private void UpdateIndicatorSortingOrder()'''
assert anchor in t
t = t.replace(anchor, method, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
