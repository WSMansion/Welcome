from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            bool showFarewell = socialPartner != null;
            socialPartner = null;'''
new = '''            bool sameDirection = socialPartner != null
                && hasStreetPassThroughDestination
                && socialPartner.hasStreetPassThroughDestination
                && streetDirection == socialPartner.streetDirection;
            bool showFarewell = socialPartner != null && !sameDirection;
            socialPartner = null;'''
assert old in t
t = t.replace(old, new, 1)
old_cancel = '''            if (socialPartner != partner)
                return;
            socializing = false;
            socialPartner = null;'''
new_cancel = '''            if (socialPartner != partner)
                return;
            bool sameDirection = hasStreetPassThroughDestination
                && partner != null
                && partner.hasStreetPassThroughDestination
                && streetDirection == partner.streetDirection;
            socializing = false;
            socialPartner = null;'''
assert old_cancel in t
t = t.replace(old_cancel, new_cancel, 1)
t = t.replace('''            ShowFarewellIndicator();
            farewellTimer = behaviorSettings == null
                ? 1.2f
                : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);''', '''            if (sameDirection)
            {
                ClearInteractionIndicator();
                ChooseNextDestination();
                return;
            }

            ShowFarewellIndicator();
            farewellTimer = behaviorSettings == null
                ? 1.2f
                : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);''', 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
