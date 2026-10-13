from pathlib import Path
p = Path(r'_ConfigExcels/generate_tables.js')
t = p.read_text(encoding='utf-8')
t = t.replace("'greeting_approach_max_seconds'", "'greeting_approach_max_seconds', 'social_reaction_seconds', 'social_farewell_seconds'")
t = t.replace("'float', 'float', 'float'", "'float', 'float', 'float', 'float', 'float'")
t = t.replace("'靠近会面位置的最长时间(s)'", "'靠近会面位置的最长时间(s)', '熟人表情每次停留时间(s)', '熟人结束时告别表情停留时间(s)'")
t = t.replace("1.7, 3.5", "1.7, 3.5, 1.5, 1.2")
p.write_text(t, encoding='utf-8', newline='\n')
print('table updated')
