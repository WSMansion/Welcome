using UnityEngine;
using WelCome.Core;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.World
{
    /// <summary>
    /// Initializes display case definitions and example stock for the standalone shop preview.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopDemoBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            DisplayCaseConfigurationBootstrap.InitializeForDay(GameManager.Instance.CurrentDay);
            SeedPreviewProducts();
        }

        private static void SeedPreviewProducts()
        {
            // 独立示范地图没有制作界面；仅在背包为空时放入样例，方便立即试陈列。
            WorkshopSystem workshop = WorkshopSystem.Instance;
            if (workshop.Inventory.Count > 0)
                return;

            workshop.AddToInventory(new ProductInstance
            {
                RecipeId = 3001,
                Name = "长明灯",
                BasePrice = 15,
                Quality = ProductQuality.Normal,
                NeedTag = 1
            });
            workshop.AddToInventory(new ProductInstance
            {
                RecipeId = 3002,
                Name = "保温罐",
                BasePrice = 18,
                Quality = ProductQuality.Normal,
                NeedTag = 2
            });
        }
    }
}
