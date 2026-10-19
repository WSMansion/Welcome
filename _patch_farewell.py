from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '        private float greetingSymbolTimer;'
new = '''        private float greetingSymbolTimer;
        private float farewellTimer;'''
assert old in t
t = t.replace(old, new, 1)
t = t.replace('''            if (socializing)
            {
                UpdateSocialInteraction();
                return;
            }''', '''            if (socializing)
            {
                UpdateSocialInteraction();
                return;
            }

            if (farewellTimer > 0f)
            {
                farewellTimer -= Time.deltaTime;
                FacePartner(null);
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
                UpdateInteractionIndicator(Time.deltaTime);
                if (farewellTimer <= 0f)
                    ClearInteractionIndicator();
                return;
            }''', 1)
t = t.replace('''            if (showFarewell)
                ShowFarewellIndicator(browseTogether);
            else
                ClearInteractionIndicator();
            ChooseNextDestination();''', '''            if (showFarewell)
            {
                ShowFarewellIndicator(browseTogether);
                farewellTimer = behaviorSettings == null
                    ? 1.2f
                    : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);
            }
            else
            {
                ClearInteractionIndicator();
            }

            if (farewellTimer <= 0f)
                ChooseNextDestination();''')
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
