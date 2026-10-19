from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            socialTimer -= Time.deltaTime;
            if (socialGreeting)
            {
                greetingSymbolTimer -= Time.deltaTime;
                if (greetingSymbolTimer <= 0f && socialTimer > 0f)
                    ShowNextSocialReaction();
            }

            if (socialTimer <= 0f)
                FinishSocialInteraction();'''
new = '''            if (socialGreeting)
            {
                greetingSymbolTimer -= Time.deltaTime;
                if (greetingSymbolTimer > 0f)
                    return;

                socialTimer -= SocialReactionSeconds;
                if (socialTimer > 0.05f)
                {
                    ShowNextSocialReaction();
                    return;
                }
            }
            else
            {
                socialTimer -= Time.deltaTime;
                if (socialTimer > 0f)
                    return;
            }

            FinishSocialInteraction();'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('updated')
