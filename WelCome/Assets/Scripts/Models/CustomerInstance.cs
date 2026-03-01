namespace WelCome.Models
{
    public enum Satisfaction
    {
        None,
        Unhappy,
        OK,
        Happy,
        VeryHappy
    }

    public class CustomerInstance
    {
        public int ConfigId;
        public string Name;
        public int NeedTag;
        public string NeedDescription;
        public int Patience;
        public int SupportDelta;
        public int AttentionDelta;
        public CustomerState State;
        public bool IsSpecial;
        public int StoryId;

        public Satisfaction Satisfaction;
        public int PaidPrice;
    }
}
