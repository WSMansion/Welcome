from pathlib import Path
p = Path(r'_ConfigExcels/generate_tables.js')
t = p.read_text(encoding='utf-8')
old = """        '##type', 'float', 'float', 'float', 'float', 'float', 'float', 'int', 'int', 'int', 'float', 'int', 'int', 'float', 'float', 'int',"""
new = """        '##type', 'float', 'float', 'float', 'float', 'int', 'int', 'int', 'float', 'int', 'int', 'float', 'float', 'int',"""
assert old in t
t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('fixed')
