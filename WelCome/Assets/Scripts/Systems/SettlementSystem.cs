using UnityEngine;
using WelCome.Core;
using WelCome.Models;

namespace WelCome.Systems
{
    public class SettlementSystem : Singleton<SettlementSystem>
    {
        public SettlementResult LastResult { get; private set; }

        public SettlementResult CalculateSettlement()
        {
            var result = new SettlementResult
            {
                DayNumber = GameManager.Instance.CurrentDay,
                GoldEarned = WorkshopSystem.Instance.TonightRevenue,
                CustomersServed = WorkshopSystem.Instance.TonightCustomersServed,
                SupportChange = ResourceSystem.Instance.Support,
                AttentionChange = ResourceSystem.Instance.Attention,
                GoldAfter = ResourceSystem.Instance.Gold
            };

            WorkshopSystem.Instance.AgeInventory();

            LastResult = result;

            SaveManager.AutoSave();
            Debug.Log($"[Settlement] Day {result.DayNumber} auto-saved to slot {SaveManager.ActiveSlot}.");

            EventBus.Publish(new SettlementCompletedEvent
            {
                DayNumber = result.DayNumber,
                GoldEarned = result.GoldEarned
            });

            return result;
        }
    }
}
