from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldInteractionPrompt.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('private const int PromptSortingOrder = 2000;', '''private const int PromptSortingOrder = 3000;

        private static int HudSortingLayerId => SortingLayer.NameToID("HUD");''')
t = t.replace('promptRenderer.sortingLayerID = referenceRenderer.sortingLayerID;', 'promptRenderer.sortingLayerID = HudSortingLayerId;')
p.write_text(t, encoding='utf-8', newline='\n')
print('script updated')
