from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSocialCoordinator.cs')
t = p.read_text(encoding='utf-8')
old = '            float halfSeparation = Mathf.Min(0.35f, Mathf.Max(0.2f, settings.GreetingRadius * 0.35f));'
new = '            float halfSeparation = Mathf.Max(0.85f, settings.GreetingRadius * 0.75f);'
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('spacing updated')
