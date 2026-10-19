from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            bool showFarewell = socialPartner != null && !sameDirection;
            socialPartner = null;'''
new = '''            bool showClosingIcon = socialPartner != null;
            socialPartner = null;'''
assert old in t
t = t.replace(old, new, 1)
t = t.replace('''            if (showFarewell)
            {
                ShowFarewellIndicator();''', '''            if (showClosingIcon)
            {
                ShowFarewellIndicator(sameDirection);''')
t = t.replace('''            if (sameDirection)
            {
                ClearInteractionIndicator();
                ChooseNextDestination();
                return;
            }

            ShowFarewellIndicator();''', '''            ShowFarewellIndicator(sameDirection);''')
old_show = '''        private void ShowFarewellIndicator()
        {
            ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }'''
new_show = '''        private void ShowFarewellIndicator(bool travelTogether)
        {
            ShowBubbleIcon(travelTogether
                    ? ShopCustomerSpeechBubble.IconKind.BrowseTogether
                    : ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }'''
assert old_show in t
t = t.replace(old_show, new_show, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
