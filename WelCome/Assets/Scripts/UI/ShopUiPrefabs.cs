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

        public DisplayCaseStockPanel DisplayCaseStockPanelPrefab => displayCaseStockPanelPrefab;

        private void Awake()
        {
            ProductIcons.Bind(productIconLibrary);
        }
    }
}
