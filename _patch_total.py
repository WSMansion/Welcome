from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerWanderAI.cs')
t = p.read_text(encoding='utf-8')
old = '            pendingSocialDuration = Mathf.Max(0.2f, duration);'
new = '''            pendingSocialDuration = socialGreeting
                ? SocialReactionTotalSeconds
                : Mathf.Max(0.2f, duration);'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''        private float SocialReactionSeconds => behaviorSettings == null
            ? 1.5f
            : Mathf.Max(0.2f, behaviorSettings.SocialReactionSeconds);'''
new2 = '''        private float SocialReactionSeconds => behaviorSettings == null
            ? 1.5f
            : Mathf.Max(0.2f, behaviorSettings.SocialReactionSeconds);

        private float SocialReactionTotalSeconds => behaviorSettings == null
            ? 4.5f
            : Mathf.Max(SocialReactionSeconds, behaviorSettings.SocialReactionTotalSeconds);'''
assert old2 in t
t = t.replace(old2, new2, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
