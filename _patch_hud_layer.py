from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('renderer.sortingLayerID = animationBinding.spriteRenderer.sortingLayerID;', 'renderer.sortingLayerID = HudSortingLayerId;')
t = t.replace('interactionBubble.SetSorting(animationBinding.spriteRenderer.sortingLayerID,', 'interactionBubble.SetSorting(HudSortingLayerId,')
old = '''        private int HudSortingOrder(int localOffset)
        {
            return 20000 + Mathf.RoundToInt(-transform.position.y * 10f) + localOffset;
        }'''
new = '''        private static int HudSortingLayerId => SortingLayer.NameToID("HUD");

        private int HudSortingOrder(int localOffset)
        {
            return 1000 + Mathf.RoundToInt(-transform.position.y * 10f) + localOffset;
        }'''
assert old in t
t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('code updated')
