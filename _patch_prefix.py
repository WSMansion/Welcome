from pathlib import Path
replacements = {
    Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs'): {
        '顾客预制体缺少头顶内心文字 TextThought。': '顾客预制体缺少头顶内心文字 CustomerTextThought。',
        'transform.Find("TextThought")': 'transform.Find("CustomerTextThought")',
    },
    Path(r'WelCome/Assets/Scripts/World/ShopCustomerSpeechBubble.cs'): {
        'inner text uses TextThought.': 'inner text uses CustomerTextThought.',
    },
    Path('AGENTS.md'): {},
}
for path, pairs in replacements.items():
    if path.name == 'AGENTS.md':
        continue
    text = path.read_text(encoding='utf-8')
    for old, new in pairs.items():
        assert old in text, f'{old} missing in {path}'
        text = text.replace(old, new, 1)
    path.write_text(text, encoding='utf-8', newline='\n')
agents = Path('AGENTS.md')
text = agents.read_text(encoding='utf-8')
needle = '- Prefab 文件名与根 GameObject 名保持一致，方便在 Project 窗口和 Hierarchy 中定位。'
addition = needle + '\n- 同一业务域的可搜索资源使用统一前缀；顾客相关资源统一使用 `Customer` 前缀，例如 `Customer_00`、`CustomerSpeechBubble`、`CustomerTextThought`。'
assert needle in text
if '顾客相关资源统一使用' not in text:
    text = text.replace(needle, addition, 1)
agents.write_text(text, encoding='utf-8', newline='\n')
print('code updated')
