from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = 'Debug.LogError("顾客状态气泡预制体缺少当前图标配置。", interactionBubbleObject);'
new = 'Debug.LogError("顾客状态气泡缺少当前图标配置。", interactionBubble);'
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('fixed')
