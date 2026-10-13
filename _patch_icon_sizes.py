from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
for name in ['ThoughtIconSpritePath', 'GreetingIconSpritePath', 'UnhappySpritePath', 'HappySpritePath', 'AngrySpritePath', 'SadSpritePath', 'BrowseTogetherSpritePath']:
    old = f'EnsureSpriteAsset({name}, 14, 14,' if 'SpritePath' in name and name not in ['ThoughtIconSpritePath', 'GreetingIconSpritePath'] else f'EnsureSpriteAsset({name}, 16, 16,'
print('skip')
replacements = {
 'EnsureSpriteAsset(ThoughtIconSpritePath, 16, 16,': 'EnsureSpriteAsset(ThoughtIconSpritePath, 64, 64,',
 'EnsureSpriteAsset(GreetingIconSpritePath, 16, 16,': 'EnsureSpriteAsset(GreetingIconSpritePath, 64, 64,',
 'EnsureSpriteAsset(UnhappySpritePath, 14, 14,': 'EnsureSpriteAsset(UnhappySpritePath, 64, 64,',
 'EnsureSpriteAsset(HappySpritePath, 14, 14,': 'EnsureSpriteAsset(HappySpritePath, 64, 64,',
 'EnsureSpriteAsset(AngrySpritePath, 14, 14,': 'EnsureSpriteAsset(AngrySpritePath, 64, 64,',
 'EnsureSpriteAsset(SadSpritePath, 14, 14,': 'EnsureSpriteAsset(SadSpritePath, 64, 64,',
 'EnsureSpriteAsset(BrowseTogetherSpritePath, 14, 14,': 'EnsureSpriteAsset(BrowseTogetherSpritePath, 64, 64,',
}
for old, new in replacements.items():
    assert old in t, old
    t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('sizes updated')
