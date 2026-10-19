from pathlib import Path
p = Path(r'WelCome/ProjectSettings/TagManager.asset')
t = p.read_text(encoding='utf-8')
old = '''  m_SortingLayers:
  - name: Default
    uniqueID: 0
    locked: 0'''
new = '''  m_SortingLayers:
  - name: Default
    uniqueID: 0
    locked: 0
  - name: HUD
    uniqueID: 2054887253
    locked: 0'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('layer added')
