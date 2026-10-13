from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('        private bool socialMoodHappy;\n', '')
t = t.replace('''            socialGreeting = greeting;
            socialMoodHappy = moodHappy;
            pendingSocialDuration = Mathf.Max(0.2f, duration);
            socialTimer = socialApproaching ? 0f : pendingSocialDuration;
            greetingSymbolTimer = greeting && !socialApproaching
                ? Mathf.Min(0.75f, socialTimer)
                : 0f;''', '''            socialGreeting = greeting;
            pendingSocialDuration = socialGreeting ? 3f : Mathf.Max(0.2f, duration);
            socialTimer = socialApproaching ? 0f : pendingSocialDuration;
            greetingSymbolTimer = 0f;''')
t = t.replace('''            FacePartner(partner);
            ShowSocialIndicator(greeting,
                pendingSocialDuration + (socialApproaching ? socialApproachMaxSeconds : 0f));
            return true;''', '''            FacePartner(partner);
            if (socialApproaching)
                ShowSocialIndicator(socialGreeting, socialApproachMaxSeconds);
            else
                BeginSocialReaction();
            return true;''')
t = t.replace('''            socialTimer -= Time.deltaTime;
            if (socialGreeting && greetingSymbolTimer > 0f)
            {
                greetingSymbolTimer -= Time.deltaTime;
                if (greetingSymbolTimer <= 0f)
                    ShowSocialMoodIndicator(socialMoodHappy, socialTimer);
            }

            if (socialTimer <= 0f)''', '''            socialTimer -= Time.deltaTime;
            if (socialGreeting)
            {
                greetingSymbolTimer -= Time.deltaTime;
                if (greetingSymbolTimer <= 0f && socialTimer > 0f)
                    ShowNextSocialReaction();
            }

            if (socialTimer <= 0f)''')
old = '''        private void BeginSocialConversation()
        {'''
new = '''        private void BeginSocialReaction()
        {
            if (!socialGreeting)
            {
                ShowSocialIndicator(false, socialTimer);
                return;
            }

            ShowNextSocialReaction();
        }

        private void ShowNextSocialReaction()
        {
            int reaction = Random.Range(0, 3);
            ShopCustomerSpeechBubble.IconKind icon = reaction == 0
                ? ShopCustomerSpeechBubble.IconKind.MoodHappy
                : reaction == 1
                    ? ShopCustomerSpeechBubble.IconKind.MoodDown
                    : ShopCustomerSpeechBubble.IconKind.Thought;
            ShowBubbleIcon(icon, 1f);
            greetingSymbolTimer = 1f;
        }

        private void BeginSocialConversation()
        {'''
assert old in t, 'begin missing'
t = t.replace(old, new, 1)
t = t.replace('''            socialTimer = pendingSocialDuration;
            greetingSymbolTimer = socialGreeting ? Mathf.Min(0.75f, socialTimer) : 0f;
            socialApproachSpeedMultiplier = 1f;
            socialApproachElapsed = 0f;
            ShowSocialIndicator(socialGreeting, socialTimer);''', '''            socialTimer = pendingSocialDuration;
            greetingSymbolTimer = 0f;
            socialApproachSpeedMultiplier = 1f;
            socialApproachElapsed = 0f;
            BeginSocialReaction();''')
t = t.replace('            socialMoodHappy = false;\n', '')
p.write_text(t, encoding='utf-8', newline='\n')
print('mood refs', t.count('socialMoodHappy'))
