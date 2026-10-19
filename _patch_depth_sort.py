from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            UpdateInteractionIndicator(Time.deltaTime);
            if (socializing)'''
new = '''            UpdateInteractionIndicator(Time.deltaTime);
            UpdateThoughtSorting();
            if (socializing)'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            renderer.sortingOrder = animationBinding.spriteRenderer.sortingOrder + 3;'''
new2 = '''            int baseOrder = animationBinding != null && animationBinding.spriteRenderer != null
                ? animationBinding.spriteRenderer.sortingOrder
                : 300 + Mathf.RoundToInt(-transform.position.y * 10f);
            renderer.sortingOrder = baseOrder + 3;'''
assert old2 in t
t = t.replace(old2, new2, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
