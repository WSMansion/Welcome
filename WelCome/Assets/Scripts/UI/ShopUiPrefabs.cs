using UnityEngine;
using WelCome.Data;

namespace WelCome.UI
{
    public sealed class ShopUiPrefabs : MonoBehaviour
    {
        [SerializeField, InspectorName("陈列界面")]
        private DisplayCaseStockPanel displayCaseStockPanelPrefab;

        [SerializeField, InspectorName("商品图标库")]
        private ProductIconLibrary productIconLibrary;

        [SerializeField, InspectorName("金币栏")]
        private ShopGoldBar goldBarPrefab;

        public DisplayCaseStockPanel DisplayCaseStockPanelPrefab => displayCaseStockPanelPrefab;
        public ShopGoldBar GoldBarPrefab => goldBarPrefab;

        private void Awake()
        {
            ProductIcons.Bind(productIconLibrary);
        }

        private void Start()
        {
            if (goldBarPrefab != null)
                UIManager.Instance.MountShopGoldBar(goldBarPrefab);
        }
    }
}