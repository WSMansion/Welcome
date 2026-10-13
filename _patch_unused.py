from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSocialCoordinator.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('''                            && StartPairInteraction(first, second, settings.GreetingMinSeconds,
                                settings.GreetingMaxSeconds, true, settings.GreetingHappyChance,
                                firstTarget, secondTarget, approachBeforeChat,''', '''                            && StartPairInteraction(first, second, 0f, 0f, true,
                                firstTarget, secondTarget, approachBeforeChat,''')
t = t.replace('''                        StartPairInteraction(first, second, settings.ConversationMinSeconds,
                            settings.ConversationMaxSeconds, false, settings.GreetingHappyChance,''', '''                        StartPairInteraction(first, second, settings.ConversationMinSeconds,
                            settings.ConversationMaxSeconds, false,''')
t = t.replace('''            float minimumDuration, float maximumDuration, bool greeting, int happyChance,
            Vector2 firstTarget, Vector2 secondTarget, bool approachBeforeChat,''', '''            float minimumDuration, float maximumDuration, bool greeting,
            Vector2 firstTarget, Vector2 secondTarget, bool approachBeforeChat,''')
t = t.replace('''            float duration = Random.Range(low, high);
            int moodChance = Mathf.Clamp(happyChance, 0, 100);
            bool firstHappy = Random.Range(0, 100) < moodChance;
            bool secondHappy = Random.Range(0, 100) < moodChance;
            if (!first.TryStartSocialInteraction(second, duration, greeting, firstHappy, firstTarget,''', '''            float duration = Random.Range(low, high);
            if (!first.TryStartSocialInteraction(second, duration, greeting, false, firstTarget,''')
t = t.replace('if (second.TryStartSocialInteraction(first, duration, greeting, secondHappy, secondTarget,', 'if (second.TryStartSocialInteraction(first, duration, greeting, false, secondTarget,')
assert 'GreetingHappyChance' not in t and 'GreetingMinSeconds' not in t and 'GreetingMaxSeconds' not in t
p.write_text(t, encoding='utf-8', newline='\n')
print('coordinator updated')
