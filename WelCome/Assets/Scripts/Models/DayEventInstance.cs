using System.Collections.Generic;

namespace WelCome.Models
{
    public class DayEventInstance
    {
        public int ConfigId;
        public string Title;
        public string Description;
        public EventType Type;
        public int ApCost;
        public int AttentionDelta;
        public int SupportDelta;
        public int GoldReward;
        public Dictionary<int, int> MaterialRewards = new();
        public int RemainingDays;
        public bool IsCompleted;

        /// <summary>
        /// Optional custom result text. When set, the popup uses this instead of auto-generated narrative.
        /// Future: populate from Luban table column "result_text".
        /// </summary>
        public string ResultText;
    }
}
