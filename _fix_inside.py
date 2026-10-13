from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('bool browseTogether = showFarewell && IsInsideShop && socialPartner.IsInsideShop;', 'bool browseTogether = showFarewell && !navigation.IsOutside(transform.position) && !navigation.IsOutside(socialPartner.transform.position);')
t = t.replace('''

        private bool IsInsideShop => navigation != null && navigation.IsInside(transform.position);

        private void ShowThoughtIndicator''', '''

        private void ShowThoughtIndicator''')
p.write_text(t, encoding='utf-8', newline='\n')
print('fixed')
