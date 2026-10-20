using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Data;
using WelCome.Models;
using WelCome.Systems;
using WelCome.World;

namespace WelCome.UI
{
    public sealed class DisplayCaseStockPanel : UIPanel
    {
        [SerializeField] private Text shelfName;
        [SerializeField] private Text priceValue;
        [SerializeField] private Text shelfStock;
        [SerializeField] private Text detailStock;
        [SerializeField] private Button shadeButton;
        [SerializeField] private Button priceEditButton;
        [SerializeField] private Image shelfIcon;
        [SerializeField] private Button shelfButton;
        [SerializeField] private RectTransform inventoryContent;
        [SerializeField] private DisplayCaseInventorySlot slotTemplate;
        [SerializeField] private GameObject detailRoot;
        [SerializeField] private Image detailIcon;
        [SerializeField] private Text detailName;
        [SerializeField] private Text costValue;
        [SerializeField] private InputField priceInput;
        [SerializeField] private Text marginText;
        [SerializeField] private Text detailHint;
        [SerializeField] private Button listButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button detailCloseButton;
        [SerializeField] private Button unshelfButton;

        private readonly List<DisplayCaseInventorySlot> slots = new List<DisplayCaseInventorySlot>();
        private ShopDisplayCaseView displayCase;
        private ShopPlayerController playerController;
        private ProductInstance pendingProduct;
        private bool adjustMode;
        private bool buttonsBound;
        private float previousTimeScale = 1f;
        private bool hasSavedTimeScale;

        public void SetContext(ShopDisplayCaseView targetCase, ShopPlayerController controller)
        {
            displayCase = targetCase;
            playerController = controller;
            if (IsVisible)
                Refresh();
        }

        protected override void OnShow()
        {
            BindButtons();
            if (!hasSavedTimeScale)
            {
                previousTimeScale = Time.timeScale;
                hasSavedTimeScale = true;
            }

            Time.timeScale = 0f;
            playerController?.SetInteractionPanelOpen(true);
            CloseDetail();
            Refresh();
        }

        protected override void OnHide()
        {
            ReleaseInteraction();
        }

        private void OnDisable()
        {
            ReleaseInteraction();
        }

        private void Update()
        {
            if (!IsVisible || !Input.GetKeyDown(KeyCode.Escape))
                return;
            if (detailRoot != null && detailRoot.activeSelf)
                CloseDetail();
            else
                ClosePanel();
        }

        private void BindButtons()
        {
            if (buttonsBound)
                return;
            shadeButton.onClick.AddListener(ClosePanel);
            priceEditButton.onClick.AddListener(OpenListedProduct);
            shelfButton.onClick.AddListener(OpenListedProduct);
            listButton.onClick.AddListener(ConfirmList);
            confirmButton.onClick.AddListener(ConfirmAdjust);
            removeButton.onClick.AddListener(ConfirmRemove);
            detailCloseButton.onClick.AddListener(CloseDetail);
            if (unshelfButton != null)
                unshelfButton.onClick.AddListener(ConfirmRemove);
            priceInput.onValueChanged.AddListener(delegate { RefreshMargin(); });
            buttonsBound = true;
        }

        private void Refresh()
        {
            RefreshShelf();
            RefreshInventory();
        }

        private void RefreshShelf()
        {
            ProductInstance listed = GetListedProduct();
            bool hasProduct = listed != null;
            if (shelfName != null)
                shelfName.text = hasProduct ? DisplayName(listed) : "未上架";
            if (priceValue != null)
                priceValue.text = hasProduct ? listed.SellingPrice + "金" : "—";
            if (shelfStock != null)
            {
                shelfStock.text = hasProduct ? "库存：" + CountOwned(listed, true) : "";
                SetWithPlate(shelfStock.gameObject, hasProduct);
            }

            if (unshelfButton != null)
                unshelfButton.gameObject.SetActive(hasProduct);

            if (shelfIcon != null)
            {
                Sprite sprite = hasProduct ? ProductIcons.Load(listed.RecipeId) : null;
                shelfIcon.sprite = sprite;
                shelfIcon.enabled = sprite != null;
                shelfIcon.preserveAspect = true;
                shelfIcon.color = Color.white;
                shelfIcon.gameObject.SetActive(hasProduct && sprite != null);
            }

        }

        private static void SetWithPlate(GameObject target, bool visible)
        {
            if (target == null)
                return;
            target.SetActive(visible);
            int index = target.transform.GetSiblingIndex();
            if (index <= 0)
                return;
            Transform plate = target.transform.parent.GetChild(index - 1);
            if (plate.name == "底")
                plate.gameObject.SetActive(visible);
        }

        private void RefreshInventory()
        {
            if (inventoryContent == null || slotTemplate == null)
                return;

            for (int i = inventoryContent.childCount - 1; i >= 0; i--)
                Destroy(inventoryContent.GetChild(i).gameObject);
            slots.Clear();

            var groups = new List<ItemGroup>();
            IReadOnlyList<ProductInstance> inventory = WorkshopSystem.Instance != null
                ? WorkshopSystem.Instance.Inventory
                : null;
            if (inventory != null)
            {
                foreach (ProductInstance product in inventory)
                {
                    if (product == null)
                        continue;
                    ItemGroup group = null;
                    for (int i = 0; i < groups.Count; i++)
                    {
                        ProductInstance sample = groups[i].Sample;
                        if (sample.RecipeId == product.RecipeId && sample.Quality == product.Quality && sample.ListedPrice == product.ListedPrice)
                        {
                            group = groups[i];
                            break;
                        }
                    }

                    if (group == null)
                    {
                        group = new ItemGroup(product);
                        groups.Add(group);
                    }
                    group.Count++;
                }
            }

            foreach (ItemGroup group in groups)
            {
                DisplayCaseInventorySlot slot = Instantiate(slotTemplate, inventoryContent, false);
                slot.gameObject.SetActive(true);
                ProductInstance captured = group.Sample;
                slot.Bind(ProductIcons.Load(captured.RecipeId), group.Count, captured.Name, () => OpenList(captured));
                slots.Add(slot);
            }
        }

        private void OpenListedProduct()
        {
            ProductInstance listed = GetListedProduct();
            if (listed == null)
                return;
            OpenDetail(listed, true);
        }

        private void OpenList(ProductInstance product)
        {
            if (product == null)
                return;
            OpenDetail(product, false);
        }

        private void OpenDetail(ProductInstance product, bool adjust)
        {
            pendingProduct = product;
            adjustMode = adjust;
            detailRoot.SetActive(true);
            Sprite sprite = ProductIcons.Load(product.RecipeId);
            detailIcon.sprite = sprite;
            detailIcon.enabled = sprite != null;
            detailIcon.preserveAspect = true;
            detailName.text = DisplayName(product);
            if (detailStock != null)
                detailStock.text = "库存: " + CountOwned(product, adjust);
            costValue.text = product.CostPrice.ToString();
            priceInput.text = product.SellingPrice.ToString();
            bool shelfOccupied = GetListedProduct() != null;
            bool blocked = !adjust && shelfOccupied;
            listButton.gameObject.SetActive(!adjust);
            listButton.interactable = !blocked;
            confirmButton.gameObject.SetActive(adjust);
            removeButton.gameObject.SetActive(adjust);
            detailHint.gameObject.SetActive(blocked);
            detailHint.text = blocked ? "请先下架当前商品，再上架其他商品。" : "";
            RefreshMargin();
        }

        private void RefreshMargin()
        {
            if (pendingProduct == null || marginText == null)
                return;
            int cost = pendingProduct.CostPrice;
            int price = ReadPrice(pendingProduct.SellingPrice);
            if (cost <= 0)
            {
                marginText.text = "";
                return;
            }

            int percent = Mathf.RoundToInt((price - cost) * 100f / cost);
            bool profit = percent >= 0;
            marginText.color = profit
                ? new Color(0.12f, 0.62f, 0.24f, 1f)
                : new Color(0.86f, 0.16f, 0.14f, 1f);
            marginText.text = (profit ? "+" : "") + percent + "%";
        }

        private void ConfirmList()
        {
            if (pendingProduct == null || displayCase == null || GetListedProduct() != null)
                return;
            pendingProduct.ListedPrice = ReadPrice(pendingProduct.SellingPrice);
            bool listed = DisplayCaseSystem.Instance.TryDisplayProduct(displayCase.ConfigId, 0, pendingProduct);
            CloseDetail();
            Refresh();
            if (!listed)
                Debug.LogWarning("[DisplayCaseStockPanel] 上架失败。", this);
        }

        private void ConfirmAdjust()
        {
            if (pendingProduct == null || displayCase == null)
                return;
            int price = ReadPrice(pendingProduct.SellingPrice);
            DisplayCaseSystem.Instance.TrySetDisplayedPrice(displayCase.ConfigId, 0, price);
            CloseDetail();
            Refresh();
        }

        private void ConfirmRemove()
        {
            if (displayCase == null)
                return;
            DisplayCaseSystem.Instance.TryTakeDisplayedProduct(displayCase.ConfigId, 0);
            CloseDetail();
            Refresh();
        }

        private void CloseDetail()
        {
            pendingProduct = null;
            adjustMode = false;
            if (detailRoot != null)
                detailRoot.SetActive(false);
        }

        private void ClosePanel()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.HideOverlay<DisplayCaseStockPanel>();
        }

        private static string DisplayName(ProductInstance product)
        {
            return product == null || string.IsNullOrWhiteSpace(product.Name) ? "未命名商品" : product.Name;
        }

        private int CountOwned(ProductInstance product, bool includeListed)
        {
            int count = 0;
            IReadOnlyList<ProductInstance> inventory = WorkshopSystem.Instance != null
                ? WorkshopSystem.Instance.Inventory
                : null;
            if (inventory != null)
            {
                foreach (ProductInstance item in inventory)
                {
                    if (SameProduct(item, product))
                        count++;
                }
            }

            if (includeListed && SameProduct(GetListedProduct(), product))
                count++;
            return count;
        }

        private static bool SameProduct(ProductInstance left, ProductInstance right)
        {
            return left != null && right != null
                && left.RecipeId == right.RecipeId
                && left.Quality == right.Quality;
        }

        private ProductInstance GetListedProduct()
        {
            if (displayCase == null || DisplayCaseSystem.Instance == null)
                return null;
            return DisplayCaseSystem.Instance.GetDisplayedProduct(displayCase.ConfigId, 0);
        }

        private int ReadPrice(int fallback)
        {
            if (priceInput == null || !int.TryParse(priceInput.text, out int price))
                return Mathf.Max(1, fallback);
            return Mathf.Max(1, price);
        }

        private void ReleaseInteraction()
        {
            if (hasSavedTimeScale)
            {
                Time.timeScale = previousTimeScale;
                hasSavedTimeScale = false;
            }

            if (playerController != null)
            {
                playerController.SetInteractionPanelOpen(false);
                playerController = null;
            }
        }

        private sealed class ItemGroup
        {
            public ItemGroup(ProductInstance sample)
            {
                Sample = sample;
            }

            public ProductInstance Sample { get; }
            public int Count { get; set; }
        }
    }
}
