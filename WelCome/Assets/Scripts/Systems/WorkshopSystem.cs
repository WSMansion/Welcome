using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;

namespace WelCome.Systems
{
    public class WorkshopSystem : Singleton<WorkshopSystem>
    {
        private readonly List<ProductInstance> _inventory = new();
        public IReadOnlyList<ProductInstance> Inventory => _inventory;

        public int TonightRevenue { get; private set; }
        public int TonightCustomersServed { get; private set; }

        public void ClearInventory()
        {
            _inventory.Clear();
            TonightRevenue = 0;
            TonightCustomersServed = 0;
        }

        public void LoadInventory(List<ProductEntry> entries)
        {
            _inventory.Clear();
            if (entries == null) return;
            foreach (var e in entries)
            {
                _inventory.Add(new ProductInstance
                {
                    RecipeId = e.recipeId,
                    Name = e.name,
                    BasePrice = e.basePrice,
                    Quality = (ProductQuality)e.quality,
                    NeedTag = e.needTag,
                    DaysOld = e.daysOld
                });
            }
        }

        public ProductInstance CraftProduct(int recipeId, ProductQuality quality)
        {
            if (!ConfigManager.Instance.IsLoaded) return null;

            var recipe = ConfigManager.Instance.Tables.TbRecipe.GetOrDefault(recipeId);
            if (recipe == null) return null;

            int magicCost = quality switch
            {
                ProductQuality.Normal => recipe.MagicCostBase,
                ProductQuality.Fine => recipe.MagicCostBase * 2,
                ProductQuality.Excellent => recipe.MagicCostBase * 4,
                _ => recipe.MagicCostBase
            };

            if (!ResourceSystem.Instance.SpendMagic(magicCost)) return null;
            if (!ResourceSystem.Instance.SpendMaterial(recipe.MaterialId, recipe.MaterialCost)) return null;
            if (!TimeSystem.Instance.SpendNightSlot()) return null;

            var product = new ProductInstance
            {
                RecipeId = recipeId,
                Name = recipe.ProductName,
                BasePrice = recipe.BasePrice,
                Quality = quality,
                NeedTag = recipe.NeedTag,
                DaysOld = 0
            };

            _inventory.Add(product);
            return product;
        }

        public bool RemoveFromInventory(ProductInstance product)
        {
            return _inventory.Remove(product);
        }

        public void RecordSale(int price)
        {
            TonightRevenue += price;
            TonightCustomersServed++;
        }

        public void BeginNight()
        {
            TonightRevenue = 0;
            TonightCustomersServed = 0;
        }

        public void AgeInventory()
        {
            foreach (var p in _inventory)
                p.DaysOld++;
            _inventory.RemoveAll(p => p.SellingPrice <= 0);
        }

        public List<ProductInstance> GetProductsMatchingNeed(int needTagId)
        {
            return _inventory.Where(p => p.NeedTag == needTagId).ToList();
        }

        /// <summary>
        /// Check if the player can afford to craft a recipe at a given quality.
        /// Returns a failure reason string, or null if craftable.
        /// </summary>
        public string GetCraftBlockReason(int recipeId, ProductQuality quality)
        {
            if (!ConfigManager.Instance.IsLoaded) return "配置未加载";

            var recipe = ConfigManager.Instance.Tables.TbRecipe.GetOrDefault(recipeId);
            if (recipe == null) return "配方不存在";

            int magicCost = quality switch
            {
                ProductQuality.Normal => recipe.MagicCostBase,
                ProductQuality.Fine => recipe.MagicCostBase * 2,
                ProductQuality.Excellent => recipe.MagicCostBase * 4,
                _ => recipe.MagicCostBase
            };

            if (ResourceSystem.Instance.MagicPointsCurrent < magicCost)
                return $"魔力不足 ({ResourceSystem.Instance.MagicPointsCurrent}/{magicCost})";
            if (ResourceSystem.Instance.GetMaterial(recipe.MaterialId) < recipe.MaterialCost)
                return $"素材不足";
            if (!TimeSystem.Instance.HasNightSlot())
                return "时段已满";

            return null;
        }
    }
}
