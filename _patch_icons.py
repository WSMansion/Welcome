from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSpeechBubble.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('''        public enum IconKind
        {
            Thought,
            Greeting,
            MoodDown,
            MoodHappy
        }''', '''        public enum IconKind
        {
            Thought,
            Greeting,
            MoodDown,
            MoodHappy,
            Angry,
            Sad,
            Wave,
            BrowseTogether
        }''')
t = t.replace('''        [SerializeField, InspectorName("开心")]
        private Sprite moodHappyIcon;''', '''        [SerializeField, InspectorName("开心")]
        private Sprite moodHappyIcon;
        [SerializeField, InspectorName("生气")]
        private Sprite angryIcon;
        [SerializeField, InspectorName("难过")]
        private Sprite sadIcon;
        [SerializeField, InspectorName("挥手告别")]
        private Sprite waveIcon;
        [SerializeField, InspectorName("一起逛")]
        private Sprite browseTogetherIcon;''')
t = t.replace('''                    && moodDownIcon != null
                    && moodHappyIcon != null;''', '''                    && moodDownIcon != null
                    && moodHappyIcon != null
                    && angryIcon != null
                    && sadIcon != null
                    && waveIcon != null
                    && browseTogetherIcon != null;''')
t = t.replace('''                case IconKind.MoodHappy:
                    return moodHappyIcon;
                default:''', '''                case IconKind.MoodHappy:
                    return moodHappyIcon;
                case IconKind.Angry:
                    return angryIcon;
                case IconKind.Sad:
                    return sadIcon;
                case IconKind.Wave:
                    return waveIcon;
                case IconKind.BrowseTogether:
                    return browseTogetherIcon;
                default:''')
p.write_text(t, encoding='utf-8', newline='\n')
print('bubble updated')
