using System.Collections.Generic;
using UnityEngine;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;
using EventType = WelCome.Models.EventType;

namespace WelCome.Systems
{
    public class DayEventSystem : Singleton<DayEventSystem>
    {
        private readonly List<DayEventInstance> _availableEvents = new();
        private readonly HashSet<int> _completedEventIds = new();
        private readonly List<EventCompletionRecord> _completionLog = new();

        public IReadOnlyList<DayEventInstance> AvailableEvents => _availableEvents;
        public IReadOnlyList<EventCompletionRecord> CompletionLog => _completionLog;
        public bool HasCompleted(int eventId) => _completedEventIds.Contains(eventId);

        public IEnumerable<int> GetCompletedEventIds() => _completedEventIds;

        public void LoadCompletedIds(List<int> ids)
        {
            _completedEventIds.Clear();
            _completionLog.Clear();
            _availableEvents.Clear();
            if (ids != null)
                foreach (int id in ids)
                    _completedEventIds.Add(id);
        }

        public void ClearAll()
        {
            _completedEventIds.Clear();
            _completionLog.Clear();
            _availableEvents.Clear();
        }

        public void GenerateEventsForDay(int day)
        {
            _availableEvents.Clear();

            if (!ConfigManager.Instance.IsLoaded) return;

            var allEvents = ConfigManager.Instance.Tables.TbDayEvent.DataList;
            foreach (var cfg in allEvents)
            {
                if (cfg.DayMin > day || cfg.DayMax < day) continue;
                if (cfg.OneTime && _completedEventIds.Contains(cfg.Id)) continue;

                _availableEvents.Add(new DayEventInstance
                {
                    ConfigId = cfg.Id,
                    Title = cfg.Title,
                    Description = cfg.Desc,
                    Type = (EventType)cfg.EventType,
                    ApCost = cfg.ApCost,
                    AttentionDelta = cfg.AttentionDelta,
                    SupportDelta = cfg.SupportDelta,
                    GoldReward = cfg.GoldReward,
                    MaterialRewards = ParseRewards(cfg.MaterialRewards),
                    RemainingDays = cfg.Duration,
                    IsCompleted = false,
                    ResultText = null
                });
            }
        }

        public bool ExecuteEvent(DayEventInstance evt)
        {
            if (evt.IsCompleted) return false;
            if (!TimeSystem.Instance.SpendActionPoints(evt.ApCost)) return false;

            if (evt.GoldReward != 0)
                ResourceSystem.Instance.AddGold(evt.GoldReward);

            foreach (var kv in evt.MaterialRewards)
                ResourceSystem.Instance.AddMaterial(kv.Key, kv.Value);

            if (evt.AttentionDelta != 0)
                ResourceSystem.Instance.ChangeAttention(evt.AttentionDelta);

            if (evt.SupportDelta != 0)
                ResourceSystem.Instance.ChangeSupport(evt.SupportDelta);

            evt.IsCompleted = true;
            _completedEventIds.Add(evt.ConfigId);
            _availableEvents.Remove(evt);

            _completionLog.Add(new EventCompletionRecord
            {
                EventId = evt.ConfigId,
                CompletedOnDay = GameManager.Instance.CurrentDay,
                Type = evt.Type,
                Title = evt.Title
            });

            EventBus.Publish(new EventCompletedEvent { EventId = evt.ConfigId });
            return true;
        }

        public void SkipToNight()
        {
            GameManager.Instance.EndDayPhase();
        }

        /// <summary>
        /// Returns how many events of a given type have been completed across all days.
        /// Useful for unlocking content based on player behavior patterns.
        /// </summary>
        public int GetCompletedCountByType(EventType type)
        {
            int count = 0;
            foreach (var r in _completionLog)
                if (r.Type == type) count++;
            return count;
        }

        private Dictionary<int, int> ParseRewards(string raw)
        {
            var result = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(raw)) return result;

            var pairs = raw.Split(';');
            foreach (var pair in pairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int amt))
                    result[id] = amt;
            }
            return result;
        }
    }
}
