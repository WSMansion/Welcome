from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('pendingSocialDuration = socialGreeting ? 3f : Mathf.Max(0.2f, duration);', 'pendingSocialDuration = Mathf.Max(0.2f, duration);')
old = '''        private void ShowNextSocialReaction()
        {
            int reaction = Random.Range(0, 3);
            ShopCustomerSpeechBubble.IconKind icon = reaction == 0
                ? ShopCustomerSpeechBubble.IconKind.MoodHappy
                : reaction == 1
                    ? ShopCustomerSpeechBubble.IconKind.MoodDown
                    : ShopCustomerSpeechBubble.IconKind.Thought;
            ShowBubbleIcon(icon, 1f);
            greetingSymbolTimer = 1f;
        }'''
new = '''        private void ShowNextSocialReaction()
        {
            ShopCustomerSpeechBubble.IconKind[] reactions =
            {
                ShopCustomerSpeechBubble.IconKind.MoodHappy,
                ShopCustomerSpeechBubble.IconKind.MoodDown,
                ShopCustomerSpeechBubble.IconKind.Thought,
                ShopCustomerSpeechBubble.IconKind.Angry,
                ShopCustomerSpeechBubble.IconKind.Sad
            };
            float duration = SocialReactionSeconds;
            ShowBubbleIcon(reactions[Random.Range(0, reactions.Length)], duration);
            greetingSymbolTimer = duration;
        }

        private float SocialReactionSeconds => behaviorSettings == null
            ? 1.5f
            : Mathf.Max(0.2f, behaviorSettings.SocialReactionSeconds);'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            socialTimer = 0f;
            greetingSymbolTimer = 0f;
            pendingSocialDuration = 0f;
            path.Clear();
            nextPathIndex = 0;
            ClearInteractionIndicator();
            ChooseNextDestination();'''
new2 = '''            bool showFarewell = socialPartner != null;
            bool browseTogether = showFarewell && IsInsideShop && socialPartner.IsInsideShop;
            socialTimer = 0f;
            greetingSymbolTimer = 0f;
            pendingSocialDuration = 0f;
            path.Clear();
            nextPathIndex = 0;
            if (showFarewell)
                ShowFarewellIndicator(browseTogether);
            else
                ClearInteractionIndicator();
            ChooseNextDestination();'''
assert old2 in t, 'finish missing'
t = t.replace(old2, new2, 1)
anchor = '        private void ShowThoughtIndicator'
insert = '''        private void ShowFarewellIndicator(bool browseTogether)
        {
            ShowBubbleIcon(browseTogether
                    ? ShopCustomerSpeechBubble.IconKind.BrowseTogether
                    : ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }

        private bool IsInsideShop => navigation != null && navigation.IsInside(transform.position);

        private void ShowThoughtIndicator'''
assert anchor in t
t = t.replace(anchor, insert, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('ai updated')
