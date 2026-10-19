from pathlib import Path
p = Path(r'WelCome/ProjectSettings/ProjectSettings.asset')
t = p.read_text(encoding='utf-8')
old = 'UNITY_MCP_READY;UNITY_MCP_DEPS_8'
new = 'UNITY_MCP_READY;UNITY_MCP_DEPS_12'
count = t.count(old)
assert count == 18, count
p.write_text(t.replace(old, new), encoding='utf-8', newline='\n')
print('updated', count)
