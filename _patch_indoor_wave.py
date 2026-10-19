from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            bool showFarewell = socialPartner != null;
            bool browseTogether = showFarewell && navigation != null
                && !navigation.IsOutside(transform.position)
                && !navigation.IsOutside(socialPartner.transform.position);'''
new = '''            bool showFarewell = socialPartner != null;'''
assert old in t
t = t.replace(old, new, 1)
t = t.replace('ShowFarewellIndicator(browseTogether);', 'ShowFarewellIndicator();')
old_cancel = '''            path.Clear();
            nextPathIndex = 0;
            ClearInteractionIndicator();
            if (enabled && profile != null)
                ChooseNextDestination();'''
new_cancel = '''            path.Clear();
            nextPathIndex = 0;
            ShowFarewellIndicator();
            farewellTimer = behaviorSettings == null
                ? 1.2f
                : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);'''
assert old_cancel in t
t = t.replace(old_cancel, new_cancel, 1)
old_show = '''        private void ShowFarewellIndicator(bool browseTogether)
        {
            ShowBubbleIcon(browseTogether
                    ? ShopCustomerSpeechBubble.IconKind.BrowseTogether
                    : ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }'''
new_show = '''        private void ShowFarewellIndicator()
        {
            ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }'''
assert old_show in t
t = t.replace(old_show, new_show, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
