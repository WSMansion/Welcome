from pathlib import Path
p = Path(r'_ConfigExcels/generate_tables.js')
t = p.read_text(encoding='utf-8')
old = "'greeting_chance', 'greeting_min_seconds', 'greeting_max_seconds', 'conversation_radius',"
new = "'greeting_chance', 'conversation_radius',"
assert old in t, 'headers missing'
t = t.replace(old, new, 1)
old2 = "'passerby_speed_variation_percent', 'greeting_happy_chance',"
new2 = "'passerby_speed_variation_percent',"
assert old2 in t, 'happy missing'
t = t.replace(old2, new2, 1)
for text in ["'打招呼最短时间(s)', '打招呼最长时间(s)',", "'熟人打招呼后出现开心表情的概率(%)',"]:
    assert text in t, text
    t = t.replace(text, '', 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('source updated')
