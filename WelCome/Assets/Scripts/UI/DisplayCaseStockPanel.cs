using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WelCome.Models;
using WelCome.Systems;
using WelCome.World;

namespace WelCome.UI
{
    public sealed class DisplayCaseStockPanel : UIPanel
    {
        private const string CounterFrontResource = "OpenShop/counter_display_front";

        private static readonly Color WindowFrameColor = new Color(0.16f, 0.11f, 0.09f, 1f);
        private static readonly Color PaperColor = new Color(0.94f, 0.87f, 0.74f, 1f);
        private static readonly Color PaperLightColor = new Color(0.99f, 0.95f, 0.86f, 1f);
        private static readonly Color InkColor = new Color(0.22f, 0.16f, 0.12f, 1f);
        private static readonly Color MutedInkColor = new Color(0.43f, 0.34f, 0.26f, 1f);

        private Text caseNameText;
        private Text inventoryCountText;
        private Text stockedNameText;
        private Text stockedMetaText;
        private Text stockedPriceText;
        private Text stockIconText;
        private Text statusText;
        private Image stockIconImage;
        private Button stockIconButton;
        private Button takeDownButton;
        private Transform inventoryContent;

        private ShopDisplayCaseView displayCase;
        private ShopPlayerController playerController;
        private float previousTimeScale = 1f;
        private bool hasSavedTimeScale;

        public void SetContext(ShopDisplayCaseView targetCase, ShopPlayerController controller)
        {
            displayCase = targetCase;
            playerController = controller;

            if (IsVisible)
                RefreshContents();
        }

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("Shade", transform, new Color(0.04f, 0.03f, 0.025f, 0.76f), true);

            Image frame = CreateBlock("WindowFrame", transform, WindowFrameColor,
                0.07f, 0.06f, 0.93f, 0.94f, true);
            CreateBlock("FrameTrim", frame.transform, UIHelper.AccentGold,
                0.007f, 0.007f, 0.993f, 0.993f);
            Image paper = CreateBlock("WindowPaper", frame.transform, PaperColor,
                0.015f, 0.015f, 0.985f, 0.985f);
            Transform window = paper.transform;

            CreateBlock("Header", window, UIHelper.WoodDark,
                0.02f, 0.885f, 0.98f, 0.98f);
            CreateBlock("HeaderRule", window, UIHelper.AccentGold,
                0.02f, 0.87f, 0.98f, 0.885f);

            Text titleText = CreateLabel("Title", window, "商品陈列",
                32, TextAnchor.MiddleLeft, UIHelper.AccentGold,
                0.045f, 0.9f, 0.32f, 0.97f, FontStyle.Bold);
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            caseNameText = CreateLabel("CaseName", window, "",
                21, TextAnchor.MiddleRight, PaperLightColor,
                0.38f, 0.9f, 0.895f, 0.97f);

            CreateButtonSurface("CloseButton", window, "×", ClosePanel,
                UIHelper.AccentGold, InkColor, 24,
                0.92f, 0.9f, 0.972f, 0.975f);

            CreateLabel("Instructions", window,
                "一柜一件 · 柜台空着时点背包商品上架；点击柜台商品即可取回",
                18, TextAnchor.MiddleLeft, MutedInkColor,
                0.04f, 0.815f, 0.96f, 0.865f);

            BuildCounterPreview(window);
            BuildInventory(window);

            CreateBlock("FooterRule", window, new Color(0.68f, 0.55f, 0.39f, 0.65f),
                0.04f, 0.095f, 0.96f, 0.099f);
            statusText = CreateLabel("Status", window, "",
                17, TextAnchor.MiddleLeft, InkColor,
                0.045f, 0.025f, 0.79f, 0.085f);
            CreateButtonSurface("DoneButton", window, "完成", ClosePanel,
                UIHelper.WoodDark, UIHelper.AccentGold, 18,
                0.83f, 0.022f, 0.96f, 0.088f);
        }

        protected override void OnShow()
        {
            if (!hasSavedTimeScale)
            {
                previousTimeScale = Time.timeScale;
                hasSavedTimeScale = true;
            }

            Time.timeScale = 0f;
            playerController?.SetInteractionPanelOpen(true);
            RefreshContents();
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
            if (Input.GetKeyDown(KeyCode.Escape))
                ClosePanel();
        }

        private void BuildCounterPreview(Transform parent)
        {
            Image frame = CreateBlock("CounterPreviewFrame", parent, UIHelper.WoodMedium,
                0.04f, 0.43f, 0.96f, 0.805f);
            Transform stage = frame.transform;
            CreateBlock("StagePaper", stage, new Color(0.88f, 0.78f, 0.61f, 1f),
                0.008f, 0.025f, 0.992f, 0.975f);

            CreateLabel("StageTitle", stage, "顾客面前的柜台",
                20, TextAnchor.MiddleLeft, InkColor,
                0.035f, 0.87f, 0.43f, 0.97f, FontStyle.Bold);
            CreateLabel("OneProductBadge", stage, "单件陈列  ·  1 / 1",
                16, TextAnchor.MiddleRight, MutedInkColor,
                0.68f, 0.87f, 0.965f, 0.97f);

            Sprite counterSprite = Resources.Load<Sprite>(CounterFrontResource);
            if (counterSprite != null)
            {
                Image counter = CreateBlock("CounterFront", stage, Color.white,
                    0.16f, 0.025f, 0.84f, 0.70f);
                counter.sprite = counterSprite;
                counter.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning("[DisplayCaseStockPanel] 柜台预览素材未找到：" + CounterFrontResource);
                BuildCounterFallback(stage);
            }

            Image namePlate = CreateBlock("ProductNamePlateFrame", stage, UIHelper.WoodMedium,
                0.075f, 0.635f, 0.43f, 0.86f);
            CreateBlock("ProductNamePlate", namePlate.transform, PaperLightColor,
                0.018f, 0.035f, 0.982f, 0.965f);
            Image pricePlate = CreateBlock("ProductPricePlateFrame", stage, UIHelper.WoodMedium,
                0.575f, 0.635f, 0.78f, 0.86f);
            CreateBlock("ProductPricePlate", pricePlate.transform, PaperLightColor,
                0.025f, 0.035f, 0.975f, 0.965f);

            stockedNameText = CreateLabel("StockedProductName", stage, "",
                25, TextAnchor.MiddleRight, InkColor,
                0.095f, 0.72f, 0.415f, 0.85f, FontStyle.Bold);
            stockedMetaText = CreateLabel("StockedProductMeta", stage, "",
                16, TextAnchor.MiddleRight, MutedInkColor,
                0.095f, 0.64f, 0.415f, 0.73f);
            stockedPriceText = CreateLabel("StockedProductPrice", stage, "",
                24, TextAnchor.MiddleLeft, UIHelper.WoodDark,
                0.59f, 0.72f, 0.78f, 0.85f, FontStyle.Bold);

            Image iconFrame = CreateBlock("StockedProductIconFrame", stage, UIHelper.WoodDark,
                0.465f, 0.575f, 0.535f, 0.875f, true);
            stockIconButton = iconFrame.gameObject.AddComponent<Button>();
            stockIconButton.targetGraphic = iconFrame;
            stockIconButton.onClick.AddListener(OnTakeDownClicked);
            stockIconButton.colors = CreateButtonColors(UIHelper.WoodDark);

            stockIconImage = CreateBlock("StockedProductIcon", iconFrame.transform,
                new Color(0.9f, 0.83f, 0.66f, 1f),
                0.07f, 0.07f, 0.93f, 0.93f);
            stockIconText = CreateLabel("StockedProductSymbol", iconFrame.transform, "+",
                48, TextAnchor.MiddleCenter, InkColor,
                0.08f, 0.08f, 0.92f, 0.92f, FontStyle.Bold);

            takeDownButton = CreateButtonSurface("TakeDownButton", stage,
                "收回商品", OnTakeDownClicked,
                UIHelper.WoodDark, UIHelper.AccentGold, 16,
                0.78f, 0.59f, 0.925f, 0.71f);
        }

        private void BuildCounterFallback(Transform parent)
        {
            CreateBlock("CounterShadow", parent, new Color(0.19f, 0.13f, 0.1f, 1f),
                0.17f, 0.04f, 0.83f, 0.39f);
            CreateBlock("CounterFace", parent, UIHelper.WoodMedium,
                0.18f, 0.07f, 0.82f, 0.43f);
            CreateBlock("CounterPanel", parent, UIHelper.WoodDark,
                0.32f, 0.105f, 0.68f, 0.37f);
            CreateBlock("CounterTop", parent, UIHelper.WoodLight,
                0.13f, 0.43f, 0.87f, 0.51f);
            CreateBlock("CounterTopHighlight", parent, UIHelper.AccentGold,
                0.14f, 0.49f, 0.86f, 0.505f);
        }

        private void BuildInventory(Transform parent)
        {
            Image frame = CreateBlock("InventoryFrame", parent, UIHelper.WoodMedium,
                0.04f, 0.105f, 0.96f, 0.405f);
            Transform inventoryPanel = frame.transform;
            CreateBlock("InventoryPaper", inventoryPanel, PaperLightColor,
                0.008f, 0.025f, 0.992f, 0.975f);

            CreateLabel("InventoryTitle", inventoryPanel, "玩家物品栏",
                22, TextAnchor.MiddleLeft, InkColor,
                0.025f, 0.81f, 0.4f, 0.97f, FontStyle.Bold);
            inventoryCountText = CreateLabel("InventoryCount", inventoryPanel, "0 件",
                17, TextAnchor.MiddleRight, MutedInkColor,
                0.72f, 0.81f, 0.975f, 0.97f);

            ScrollRect scroll = UIHelper.CreateScrollView("InventoryScroll", inventoryPanel, true);
            scroll.GetComponent<RectTransform>().Set(0.018f, 0.04f, 0.982f, 0.78f);
            Image scrollBackground = scroll.GetComponent<Image>();
            scrollBackground.color = new Color(0.8f, 0.7f, 0.54f, 0.48f);

            HorizontalLayoutGroup layout = scroll.content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = scroll.content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            inventoryContent = scroll.content;
        }

        private void RefreshContents()
        {
            if (caseNameText == null || displayCase == null || inventoryContent == null)
                return;

            DisplayCaseSystem caseSystem = DisplayCaseSystem.Instance;
            if (caseSystem == null)
                return;

            int caseId = displayCase.ConfigId;
            int configuredSlotCount = caseSystem.GetSlotCount(caseId);
            bool hasVisualSlot = configuredSlotCount > 0 && displayCase.GetSlot(0) != null;
            bool isUnlocked = displayCase.IsUnlocked;
            ProductInstance stockedProduct = configuredSlotCount > 0
                ? caseSystem.GetDisplayedProduct(caseId, 0)
                : null;

            caseNameText.text = caseSystem.GetDisplayName(caseId);
            UpdateCounterPreview(stockedProduct, hasVisualSlot, isUnlocked);

            IReadOnlyList<ProductInstance> inventory = WorkshopSystem.Instance != null
                ? WorkshopSystem.Instance.Inventory
                : null;
            int inventoryCount = inventory != null ? inventory.Count : 0;
            inventoryCountText.text = inventoryCount + " 件";

            ClearChildren(inventoryContent);
            bool canPlace = hasVisualSlot && isUnlocked && stockedProduct == null;
            if (inventoryCount == 0)
            {
                AddEmptyInventoryMessage(inventoryContent);
            }
            else
            {
                for (int i = 0; i < inventoryCount; i++)
                    CreateInventoryCard(inventory[i], i, canPlace);
            }

            if (!hasVisualSlot)
                statusText.text = "此陈列柜没有配置商品展示位。";
            else if (!isUnlocked)
                statusText.text = "这组陈列柜尚未解锁。";
            else if (stockedProduct != null)
                statusText.text = "柜台上已有商品；点击柜台商品或“收回商品”按钮即可取回。";
            else if (inventoryCount == 0)
                statusText.text = "背包里还没有商品，可以先去工坊制作。";
            else
                statusText.text = "点击下方任意一件商品，即可直接摆上柜台。";
        }

        private void UpdateCounterPreview(ProductInstance product, bool hasSlot, bool isUnlocked)
        {
            if (product == null)
            {
                stockIconImage.color = hasSlot
                    ? new Color(0.91f, 0.84f, 0.69f, 1f)
                    : new Color(0.67f, 0.61f, 0.53f, 1f);
                stockIconText.text = hasSlot ? "+" : "—";
                stockIconText.color = hasSlot ? UIHelper.WoodDark : MutedInkColor;
                stockedNameText.text = hasSlot ? "柜台空着" : "没有商品展示位";
                stockedMetaText.text = !hasSlot
                    ? "请检查陈列柜预制体"
                    : !isUnlocked
                        ? "解锁后即可陈列商品"
                        : "点击下方商品直接上架";
                stockedPriceText.text = "";
                takeDownButton.gameObject.SetActive(false);
                stockIconButton.interactable = false;
                return;
            }

            Color tagColor = NeedTagHelper.GetColor(product.NeedTag);
            stockIconImage.color = Color.Lerp(tagColor, PaperLightColor, 0.18f);
            stockIconText.text = GetNeedGlyph(product.NeedTag);
            stockIconText.color = InkColor;
            stockedNameText.text = GetProductName(product);
            stockedMetaText.text = GetQualityName(product.Quality) + "品质  ·  "
                + NeedTagHelper.GetName(product.NeedTag);
            stockedPriceText.text = product.SellingPrice + " 金币";
            takeDownButton.gameObject.SetActive(true);
            stockIconButton.interactable = true;
        }

        private void CreateInventoryCard(ProductInstance product, int index, bool canPlace)
        {
            Color tagColor = NeedTagHelper.GetColor(product.NeedTag);
            Color cardColor = canPlace
                ? PaperLightColor
                : new Color(0.86f, 0.82f, 0.74f, 1f);

            RectTransform card = UIHelper.CreateRect("InventoryProduct_" + index, inventoryContent);
            card.sizeDelta = new Vector2(236f, 178f);

            Image background = card.gameObject.AddComponent<Image>();
            background.color = UIHelper.WoodDark;
            background.raycastTarget = true;

            Button button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => OnInventoryProductClicked(product));
            button.interactable = canPlace;
            button.colors = CreateButtonColors(UIHelper.WoodDark);

            LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 236f;
            layoutElement.preferredWidth = 236f;
            layoutElement.minHeight = 178f;
            layoutElement.preferredHeight = 178f;

            CreateBlock("CardPaper", card, cardColor,
                0.018f, 0.025f, 0.982f, 0.975f);
            CreateBlock("CategoryStripe", card, tagColor,
                0.018f, 0.025f, 0.055f, 0.975f);
            CreateBlock("IconTile", card, Color.Lerp(tagColor, PaperLightColor, 0.7f),
                0.09f, 0.51f, 0.35f, 0.9f);
            CreateLabel("Icon", card, GetNeedGlyph(product.NeedTag),
                34, TextAnchor.MiddleCenter, tagColor,
                0.09f, 0.51f, 0.35f, 0.9f, FontStyle.Bold);
            CreateLabel("ProductName", card, GetProductName(product),
                18, TextAnchor.MiddleLeft, InkColor,
                0.4f, 0.59f, 0.955f, 0.9f, FontStyle.Bold);
            CreateLabel("ProductNeed", card, NeedTagHelper.GetName(product.NeedTag),
                14, TextAnchor.MiddleLeft, MutedInkColor,
                0.4f, 0.46f, 0.955f, 0.59f);
            CreateBlock("CardRule", card, new Color(0.58f, 0.45f, 0.32f, 0.5f),
                0.09f, 0.405f, 0.955f, 0.415f);
            CreateLabel("ProductQuality", card, GetQualityName(product.Quality),
                14, TextAnchor.MiddleLeft, GetQualityColor(product.Quality),
                0.09f, 0.255f, 0.48f, 0.39f, FontStyle.Bold);
            CreateLabel("ProductPrice", card, product.SellingPrice + " 金币",
                18, TextAnchor.MiddleRight, UIHelper.WoodDark,
                0.5f, 0.255f, 0.955f, 0.39f, FontStyle.Bold);
            string actionHint = canPlace
                ? "点击上架"
                : !displayCase.IsUnlocked
                    ? "尚未解锁"
                    : displayCase.GetSlot(0) == null
                        ? "没有展示位"
                        : "先收回柜台商品";
            CreateLabel("ActionHint", card, actionHint,
                13, TextAnchor.MiddleRight, canPlace ? MutedInkColor : new Color(0.54f, 0.49f, 0.42f, 1f),
                0.25f, 0.09f, 0.955f, 0.22f);
        }

        private void AddEmptyInventoryMessage(Transform parent)
        {
            RectTransform card = UIHelper.CreateRect("EmptyInventory", parent);
            card.sizeDelta = new Vector2(430f, 178f);

            Image background = card.gameObject.AddComponent<Image>();
            background.color = new Color(0.9f, 0.84f, 0.72f, 1f);
            background.raycastTarget = false;

            LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 430f;
            layoutElement.preferredWidth = 430f;
            layoutElement.minHeight = 178f;
            layoutElement.preferredHeight = 178f;

            CreateLabel("EmptyMark", card, "✦", 30,
                TextAnchor.MiddleCenter, UIHelper.AccentGold,
                0.04f, 0.58f, 0.96f, 0.93f, FontStyle.Bold);
            CreateLabel("EmptyTitle", card, "背包里还没有商品", 18,
                TextAnchor.MiddleCenter, InkColor,
                0.04f, 0.34f, 0.96f, 0.62f, FontStyle.Bold);
            CreateLabel("EmptyHint", card, "先去工坊制作几件，再回来陈列。", 14,
                TextAnchor.MiddleCenter, MutedInkColor,
                0.04f, 0.08f, 0.96f, 0.35f);
        }

        private void OnInventoryProductClicked(ProductInstance product)
        {
            if (displayCase == null)
                return;

            DisplayCaseSystem caseSystem = DisplayCaseSystem.Instance;
            int caseId = displayCase.ConfigId;
            if (!displayCase.IsUnlocked)
            {
                statusText.text = "这组陈列柜尚未解锁。";
                return;
            }

            if (caseSystem.GetSlotCount(caseId) <= 0 || displayCase.GetSlot(0) == null)
            {
                statusText.text = "此陈列柜还没有配置商品展示位。";
                return;
            }

            if (caseSystem.GetDisplayedProduct(caseId, 0) != null)
            {
                statusText.text = "柜台上已有商品，请先取回再更换。";
                return;
            }

            bool placed = caseSystem.TryDisplayProduct(caseId, 0, product);
            RefreshContents();
            statusText.text = placed
                ? GetProductName(product) + " 已摆上柜台。"
                : "上架失败，商品可能已经不在背包中。";
        }

        private void OnTakeDownClicked()
        {
            if (displayCase == null)
                return;

            DisplayCaseSystem caseSystem = DisplayCaseSystem.Instance;
            int caseId = displayCase.ConfigId;
            ProductInstance product = caseSystem.GetDisplayedProduct(caseId, 0);
            if (product == null)
                return;

            bool removed = caseSystem.TryTakeDisplayedProduct(caseId, 0);
            RefreshContents();
            statusText.text = removed
                ? GetProductName(product) + " 已取回背包。"
                : "取回失败，请再试一次。";
        }

        private void ClosePanel()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.HideOverlay<DisplayCaseStockPanel>();
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

        private static Image CreateBlock(string name, Transform parent, Color color,
            float left, float bottom, float right, float top, bool raycastTarget = false)
        {
            Image image = UIHelper.CreatePanel(name, parent, color, raycastTarget);
            image.raycastTarget = raycastTarget;
            image.GetComponent<RectTransform>().Set(left, bottom, right, top);
            return image;
        }

        private static Text CreateLabel(string name, Transform parent, string value,
            int fontSize, TextAnchor anchor, Color color,
            float left, float bottom, float right, float top,
            FontStyle style = FontStyle.Normal)
        {
            Text label = UIHelper.CreateText(name, parent, value, fontSize, anchor, color);
            label.fontStyle = style;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.GetComponent<RectTransform>().Set(left, bottom, right, top);
            return label;
        }

        private static Button CreateButtonSurface(string name, Transform parent, string label,
            UnityAction onClick, Color background, Color textColor, int fontSize,
            float left, float bottom, float right, float top)
        {
            Image image = CreateBlock(name, parent, background,
                left, bottom, right, top, true);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            button.colors = CreateButtonColors(background);

            CreateLabel("Label", image.transform, label, fontSize,
                TextAnchor.MiddleCenter, textColor,
                0.04f, 0.04f, 0.96f, 0.96f, FontStyle.Bold);
            return button;
        }

        private static ColorBlock CreateButtonColors(Color background)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, UIHelper.WoodDark, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = Color.Lerp(background, Color.gray, 0.4f);
            return colors;
        }

        private static string GetQualityName(ProductQuality quality)
        {
            return quality switch
            {
                ProductQuality.Fine => "优良",
                ProductQuality.Excellent => "卓越",
                _ => "普通"
            };
        }

        private static Color GetQualityColor(ProductQuality quality)
        {
            return quality switch
            {
                ProductQuality.Fine => new Color(0.38f, 0.57f, 0.78f, 1f),
                ProductQuality.Excellent => new Color(0.67f, 0.43f, 0.12f, 1f),
                _ => new Color(0.38f, 0.48f, 0.32f, 1f)
            };
        }

        private static string GetProductName(ProductInstance product)
        {
            return string.IsNullOrWhiteSpace(product.Name)
                ? "商品 " + product.RecipeId
                : product.Name;
        }

        private static string GetNeedGlyph(int needTag)
        {
            return needTag switch
            {
                1 => "光",
                2 => "暖",
                3 => "净",
                4 => "力",
                5 => "奢",
                6 => "农",
                _ => "物"
            };
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
                return;

            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
    }
}
