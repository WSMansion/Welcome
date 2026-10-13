from pathlib import Path
p = Path(r'_ConfigExcels/generate_tables.js')
t = p.read_text(encoding='utf-8')
i = t.find("Creating shop_behavior.xlsx")
j = t.find("const typeRow", i)
k = t.find("];", j)
print(t[j:k+2])
