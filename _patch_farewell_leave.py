from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''                if (farewellTimer <= 0f)
                    ClearInteractionIndicator();
                return;'''
new = '''                if (farewellTimer <= 0f)
                {
                    ClearInteractionIndicator();
                    ChooseNextDestination();
                }
                return;'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('updated')
