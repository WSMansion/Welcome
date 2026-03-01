using UnityEngine;
using WelCome.Core;
using WelCome.Data;

namespace WelCome.Systems
{
    public class TimeSystem : Singleton<TimeSystem>
    {
        public int CurrentDay { get; private set; }
        public TimePhase CurrentPhase { get; private set; }

        public int ActionPointsMax { get; private set; } = 10;
        public int ActionPointsCurrent { get; private set; }

        public int NightSlotsMax { get; private set; } = 5;
        public int NightSlotsCurrent { get; private set; }

        protected override void OnInit()
        {
            var cfg = Data.ConfigManager.Instance;
            if (cfg.IsLoaded)
            {
                ActionPointsMax = cfg.Tables.TbGlobalParam.ApPerDay;
                NightSlotsMax = cfg.Tables.TbGlobalParam.NightSlots;
            }
        }

        public void StartDay(int day)
        {
            CurrentDay = day;
            CurrentPhase = TimePhase.Day;
            ActionPointsCurrent = ActionPointsMax;
            EventBus.Publish(new ActionPointsChangedEvent
            {
                Current = ActionPointsCurrent,
                Max = ActionPointsMax
            });
        }

        public void StartNight(int day)
        {
            CurrentDay = day;
            CurrentPhase = TimePhase.Night;
            NightSlotsCurrent = NightSlotsMax;
            ResourceSystem.Instance.RefreshDailyMagic();
        }

        public bool SpendActionPoints(int cost)
        {
            if (ActionPointsCurrent < cost) return false;

            ActionPointsCurrent -= cost;
            EventBus.Publish(new ActionPointsChangedEvent
            {
                Current = ActionPointsCurrent,
                Max = ActionPointsMax
            });

            if (ActionPointsCurrent <= 0)
                GameManager.Instance.EndDayPhase();

            return true;
        }

        public bool HasNightSlot(int cost = 1) => NightSlotsCurrent >= cost;

        public bool SpendNightSlot(int cost = 1)
        {
            if (NightSlotsCurrent < cost) return false;
            NightSlotsCurrent -= cost;
            return true;
        }

        public void SetMaxAP(int max)
        {
            ActionPointsMax = max;
        }

        public void SetMaxNightSlots(int max)
        {
            NightSlotsMax = max;
        }
    }
}
