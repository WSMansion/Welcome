using System.Collections.Generic;
using UnityEngine;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;

namespace WelCome.Systems
{
    public class CustomerSystem : Singleton<CustomerSystem>
    {
        private readonly List<CustomerInstance> _queue = new();
        private int _spawnIndex;

        public IReadOnlyList<CustomerInstance> CustomerQueue => _queue;

        public void GenerateCustomersForNight(int day)
        {
            _queue.Clear();
            _spawnIndex = 0;

            if (!ConfigManager.Instance.IsLoaded) return;

            var allCustomers = ConfigManager.Instance.Tables.TbCustomer.DataList;
            foreach (var cfg in allCustomers)
            {
                if (cfg.DayMin > day || cfg.DayMax < day) continue;

                float roll = Random.value * 100f;
                if (roll > cfg.SpawnChance) continue;

                _queue.Add(new CustomerInstance
                {
                    ConfigId = cfg.Id,
                    Name = cfg.CustomerName,
                    NeedTag = cfg.NeedTag,
                    NeedDescription = cfg.NeedDesc,
                    Patience = cfg.Patience,
                    SupportDelta = cfg.SupportDelta,
                    AttentionDelta = cfg.AttentionDelta,
                    IsSpecial = cfg.IsSpecial,
                    StoryId = cfg.StoryId,
                    State = CustomerState.Waiting,
                    Satisfaction = Satisfaction.None,
                    PaidPrice = 0
                });
            }
        }

        public CustomerInstance GetNextCustomer()
        {
            if (_spawnIndex >= _queue.Count) return null;
            var customer = _queue[_spawnIndex];
            _spawnIndex++;
            EventBus.Publish(new CustomerArrivedEvent { CustomerId = customer.ConfigId });
            return customer;
        }

        /// <summary>
        /// Serves a customer with a product. Returns satisfaction result.
        /// Core mechanic: need_tag matching determines satisfaction level.
        /// </summary>
        public Satisfaction ServeCustomer(CustomerInstance customer, ProductInstance product)
        {
            if (customer.State != CustomerState.Waiting) return Satisfaction.None;
            if (product == null) return Satisfaction.None;

            if (!WorkshopSystem.Instance.RemoveFromInventory(product))
                return Satisfaction.None;

            bool tagMatch = product.NeedTag == customer.NeedTag;
            bool highQuality = product.Quality >= ProductQuality.Fine;
            bool excellent = product.Quality == ProductQuality.Excellent;

            Satisfaction sat;
            float priceMul;

            if (tagMatch && excellent)
            {
                sat = Satisfaction.VeryHappy;
                priceMul = 1.5f;
            }
            else if (tagMatch && highQuality)
            {
                sat = Satisfaction.Happy;
                priceMul = 1.2f;
            }
            else if (tagMatch)
            {
                sat = Satisfaction.OK;
                priceMul = 1.0f;
            }
            else
            {
                sat = Satisfaction.Unhappy;
                priceMul = 0.5f;
            }

            int finalPrice = Mathf.CeilToInt(product.SellingPrice * priceMul);
            ResourceSystem.Instance.AddGold(finalPrice);

            WorkshopSystem.Instance.RecordSale(finalPrice);

            customer.State = CustomerState.Served;
            customer.Satisfaction = sat;
            customer.PaidPrice = finalPrice;

            if (tagMatch)
            {
                if (customer.SupportDelta != 0)
                    ResourceSystem.Instance.ChangeSupport(customer.SupportDelta);
            }
            if (customer.AttentionDelta != 0)
                ResourceSystem.Instance.ChangeAttention(customer.AttentionDelta);

            EventBus.Publish(new ProductSoldEvent
            {
                ProductId = product.RecipeId,
                Price = finalPrice,
                CustomerId = customer.ConfigId
            });

            return sat;
        }

        public bool HasMoreCustomers() => _spawnIndex < _queue.Count;

        public int GetServedCount()
        {
            int count = 0;
            foreach (var c in _queue)
                if (c.State == CustomerState.Served) count++;
            return count;
        }

        public int GetTotalSatisfactionScore()
        {
            int score = 0;
            foreach (var c in _queue)
            {
                score += c.Satisfaction switch
                {
                    Satisfaction.VeryHappy => 3,
                    Satisfaction.Happy => 2,
                    Satisfaction.OK => 1,
                    _ => 0
                };
            }
            return score;
        }
    }
}
