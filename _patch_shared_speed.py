from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '''            bool sameDirection = socialPartner != null
                && hasStreetPassThroughDestination
                && socialPartner.hasStreetPassThroughDestination
                && streetDirection == socialPartner.streetDirection;'''
new = '''            bool sameDirection = socialPartner != null
                && hasStreetPassThroughDestination
                && socialPartner.hasStreetPassThroughDestination
                && streetDirection == socialPartner.streetDirection;
            if (sameDirection)
                MatchTravelSpeed(socialPartner);'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            bool sameDirection = hasStreetPassThroughDestination
                && partner != null
                && partner.hasStreetPassThroughDestination
                && streetDirection == partner.streetDirection;'''
new2 = '''            bool sameDirection = hasStreetPassThroughDestination
                && partner != null
                && partner.hasStreetPassThroughDestination
                && streetDirection == partner.streetDirection;
            if (sameDirection)
                MatchTravelSpeed(partner);'''
assert old2 in t
t = t.replace(old2, new2, 1)
anchor = '        private void ResumeStreetPassThrough()'
method = '''        private void MatchTravelSpeed(ShopCustomerWanderAI partner)
        {
            float ownSpeed = Mathf.Max(0.1f, profile.MoveSpeed) * streetSpeedMultiplier;
            float partnerSpeed = Mathf.Max(0.1f, partner.profile.MoveSpeed) * partner.streetSpeedMultiplier;
            float sharedSpeed = (ownSpeed + partnerSpeed) * 0.5f;
            streetSpeedMultiplier = sharedSpeed / Mathf.Max(0.1f, profile.MoveSpeed);
        }

        private void ResumeStreetPassThrough()'''
assert anchor in t
t = t.replace(anchor, method, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
