from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldInteractionPrompt.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('private const int PromptSortingOrder = short.MaxValue;', 'private const int PromptSortingOrder = 2000;')
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
