from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            socialApproachReached = false;
            socialPartner = null;
            bool showFarewell = socialPartner != null;
            bool browseTogether = showFarewell && !navigation.IsOutside(transform.position) && !navigation.IsOutside(socialPartner.transform.position);'''
new = '''            socialApproachReached = false;
            bool showFarewell = socialPartner != null;
            bool browseTogether = showFarewell && navigation != null
                && !navigation.IsOutside(transform.position)
                && !navigation.IsOutside(socialPartner.transform.position);
            socialPartner = null;'''
assert old in t, 'finish block missing'
t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('fixed')
