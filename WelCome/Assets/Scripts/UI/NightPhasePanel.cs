using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.UI
{
    public class NightPhasePanel : UIPanel
    {
        private static readonly Color[] CustomerColors =
        {
            new(0.6f, 0.35f, 0.35f),
            new(0.35f, 0.55f, 0.6f),
            new(0.55f, 0.4f, 0.6f),
            new(0.5f, 0.6f, 0.35f),
            new(0.6f, 0.5f, 0.35f),
        };

        private Transform _shelfContent;
        private RectTransform _customerSlot;
        private RectTransform _currentCustomerRT;
        private Text _speechName;
        private Text _speechText;
        private Text _speechTagLabel;
        private Image _speechTagBg;
        private GameObject _speechBubble;
        private Transform _sellBtnArea;
        private Text _inventoryLabel;
        private Transform _craftContent;
        private Text _statusText;
        private Text _resourceText;
        private Text _revenueText;

        private CustomerInstance _currentCustomer;
        private Coroutine _loopRoutine;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("BG", transform, new Color(0.1f, 0.08f, 0.06f));

            BuildShopArea();
            BuildCraftBar();
            BuildBottomBar();
        }

        private void BuildShopArea()
        {
            var wall = UIHelper.CreatePanel("Wall", transform, UIHelper.WoodMedium);
            wall.GetComponent<RectTransform>().Set(0f, 0.42f, 1f, 0.92f);

            var shelfBg = UIHelper.CreatePanel("ShelfBG", transform,
                new Color(0.35f, 0.28f, 0.18f));
            shelfBg.GetComponent<RectTransform>().Set(0.03f, 0.7f, 0.97f, 0.9f);

            var shelfLabel = UIHelper.CreateText("ShelfLabel", transform, "货架",
                16, TextAnchor.LowerLeft, new Color(0.8f, 0.7f, 0.5f, 0.7f));
            shelfLabel.GetComponent<RectTransform>().Set(0.04f, 0.9f, 0.2f, 0.92f);

            _inventoryLabel = UIHelper.CreateText("InvLabel", transform, "",
                16, TextAnchor.LowerRight, new Color(0.8f, 0.7f, 0.5f, 0.7f));
            _inventoryLabel.GetComponent<RectTransform>().Set(0.6f, 0.9f, 0.96f, 0.92f);

            var shelfScroll = UIHelper.CreateScrollView("ShelfScroll", transform, horizontal: true);
            shelfScroll.GetComponent<RectTransform>().Set(0.04f, 0.71f, 0.96f, 0.89f);
            var shelfLayout = shelfScroll.content.gameObject.AddComponent<HorizontalLayoutGroup>();
            shelfLayout.spacing = 8;
            shelfLayout.padding = new RectOffset(8, 8, 4, 4);
            shelfLayout.childControlWidth = false;
            shelfLayout.childControlHeight = true;
            shelfLayout.childForceExpandWidth = false;
            shelfLayout.childForceExpandHeight = true;
            shelfScroll.content.gameObject.AddComponent<ContentSizeFitter>()
                .horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _shelfContent = shelfScroll.content;

            var shelfBoard = UIHelper.CreatePanel("ShelfBoard", transform,
                new Color(0.3f, 0.22f, 0.14f));
            shelfBoard.GetComponent<RectTransform>().Set(0.02f, 0.695f, 0.98f, 0.71f);

            var sign = UIHelper.CreatePanel("Sign", transform,
                new Color(0.25f, 0.2f, 0.12f));
            sign.GetComponent<RectTransform>().Set(0.35f, 0.56f, 0.65f, 0.68f);
            UIHelper.CreateText("SignText", sign.transform, "注 魔 工 坊", 20,
                TextAnchor.MiddleCenter, UIHelper.AccentGold)
                .GetComponent<RectTransform>().Set(0f, 0f, 1f, 1f);

            var counter = UIHelper.CreatePanel("Counter", transform, UIHelper.WoodDark);
            counter.GetComponent<RectTransform>().Set(0.02f, 0.4f, 0.98f, 0.44f);
            var counterTop = UIHelper.CreatePanel("CounterTop", transform, UIHelper.WoodLight);
            counterTop.GetComponent<RectTransform>().Set(0.02f, 0.44f, 0.98f, 0.46f);

            var floor = UIHelper.CreatePanel("Floor", transform,
                new Color(0.22f, 0.18f, 0.12f));
            floor.GetComponent<RectTransform>().Set(0f, 0.12f, 1f, 0.4f);

            var shopkeeper = UIHelper.CreatePortrait("Shopkeeper", transform,
                "Characters/Que_sprite", 200, 380);
            shopkeeper.anchorMin = shopkeeper.anchorMax = new Vector2(0.18f, 0.12f);
            shopkeeper.anchoredPosition = Vector2.zero;

            _customerSlot = UIHelper.CreateRect("CustomerSlot", transform);
            _customerSlot.anchorMin = _customerSlot.anchorMax = new Vector2(0.7f, 0.12f);
            _customerSlot.sizeDelta = Vector2.zero;

            BuildSpeechBubble();

            _statusText = UIHelper.CreateText("Status", transform,
                "", 16, TextAnchor.MiddleCenter, UIHelper.TextGray);
            _statusText.GetComponent<RectTransform>().Set(0.05f, 0.30f, 0.95f, 0.39f);

            var sellArea = UIHelper.CreateRect("SellArea", transform);
            sellArea.Set(0.02f, 0.14f, 0.98f, 0.28f);
            _sellBtnArea = sellArea;

            _revenueText = UIHelper.CreateText("Revenue", transform,
                "", 14, TextAnchor.MiddleRight, UIHelper.AccentGold);
            _revenueText.GetComponent<RectTransform>().Set(0.55f, 0.40f, 0.97f, 0.44f);
        }

        private void BuildSpeechBubble()
        {
            _speechBubble = new GameObject("SpeechBubble");
            _speechBubble.transform.SetParent(transform, false);
            var bubbleRect = _speechBubble.AddComponent<RectTransform>();
            bubbleRect.Set(0.40f, 0.47f, 0.96f, 0.68f);
            var bubbleBg = _speechBubble.AddComponent<Image>();
            bubbleBg.color = new Color(0.95f, 0.93f, 0.88f, 0.95f);
            bubbleBg.raycastTarget = false;

            var border = UIHelper.CreatePanel("Border", _speechBubble.transform,
                new Color(0.7f, 0.6f, 0.45f, 0.6f));
            border.GetComponent<RectTransform>().Set(0f, 0f, 1f, 0.02f);
            var borderTop = UIHelper.CreatePanel("BorderTop", _speechBubble.transform,
                new Color(0.7f, 0.6f, 0.45f, 0.6f));
            borderTop.GetComponent<RectTransform>().Set(0f, 0.98f, 1f, 1f);

            _speechName = UIHelper.CreateText("SpeechName", _speechBubble.transform, "",
                18, TextAnchor.UpperLeft, new Color(0.5f, 0.35f, 0.15f));
            _speechName.GetComponent<RectTransform>().Set(0.03f, 0.72f, 0.55f, 0.95f);
            _speechName.fontStyle = FontStyle.Bold;

            var tagArea = UIHelper.CreateRect("TagArea", _speechBubble.transform);
            tagArea.Set(0.6f, 0.73f, 0.97f, 0.95f);
            _speechTagBg = tagArea.gameObject.AddComponent<Image>();
            _speechTagBg.color = Color.gray;
            _speechTagBg.raycastTarget = false;

            _speechTagLabel = UIHelper.CreateText("TagLabel", tagArea, "",
                14, TextAnchor.MiddleCenter, Color.white);
            _speechTagLabel.GetComponent<RectTransform>().Set(0f, 0f, 1f, 1f);

            _speechText = UIHelper.CreateText("SpeechText", _speechBubble.transform, "",
                16, TextAnchor.UpperLeft, new Color(0.2f, 0.18f, 0.15f));
            _speechText.GetComponent<RectTransform>().Set(0.03f, 0.04f, 0.97f, 0.7f);

            _speechBubble.SetActive(false);
        }

        private void BuildCraftBar()
        {
            var craftBar = UIHelper.CreatePanel("CraftBar", transform,
                new Color(0.15f, 0.12f, 0.1f, 0.95f));
            craftBar.GetComponent<RectTransform>().Set(0f, 0f, 0.7f, 0.12f);

            _resourceText = UIHelper.CreateText("ResText", craftBar.transform, "",
                14, TextAnchor.MiddleLeft, UIHelper.TextWhite);
            _resourceText.GetComponent<RectTransform>().Set(0.01f, 0.55f, 0.99f, 0.95f);

            var craftLabel = UIHelper.CreateText("CraftLabel", craftBar.transform, "制作:",
                15, TextAnchor.MiddleLeft, UIHelper.AccentGold);
            craftLabel.GetComponent<RectTransform>().Set(0.01f, 0f, 0.06f, 0.55f);

            var craftScroll = UIHelper.CreateScrollView("CraftScroll", craftBar.transform,
                horizontal: true);
            craftScroll.GetComponent<RectTransform>().Set(0.06f, 0.02f, 1f, 0.55f);

            var layout = craftScroll.content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(4, 4, 2, 2);
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            craftScroll.content.gameObject.AddComponent<ContentSizeFitter>()
                .horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _craftContent = craftScroll.content;
        }

        private void BuildBottomBar()
        {
            UIHelper.CreateButton("EndNightBtn", transform, "结束营业",
                () => GameManager.Instance.EndNightPhase(), UIHelper.AccentGold, 18)
                .GetComponent<RectTransform>().Set(0.71f, 0.02f, 0.98f, 0.1f);
        }

        protected override void OnShow()
        {
            WorkshopSystem.Instance.BeginNight();
            CustomerSystem.Instance.GenerateCustomersForNight(GameManager.Instance.CurrentDay);

            PopulateRecipes();
            RefreshShelf();
            RefreshResources();
            _statusText.text = "夜幕降临，工坊开始营业……";
            _revenueText.text = "";
            _speechBubble.SetActive(false);
            ClearSellButtons();
            _currentCustomer = null;

            if (_currentCustomerRT != null)
            {
                Destroy(_currentCustomerRT.gameObject);
                _currentCustomerRT = null;
            }

            _loopRoutine = StartCoroutine(CustomerLoop());
        }

        protected override void OnHide()
        {
            StopAllCoroutines();
            _loopRoutine = null;
        }

        #region Resources

        private void RefreshResources()
        {
            var rs = ResourceSystem.Instance;
            var ts = TimeSystem.Instance;
            _resourceText.text =
                $"<color=#EBCA6A>金币:{rs.Gold}</color>  " +
                $"<color=#88BBFF>魔力:{rs.MagicPointsCurrent}/{rs.MagicPointsMax}</color>  " +
                $"时段:{ts.NightSlotsCurrent}/{ts.NightSlotsMax}";
        }

        private void RefreshRevenue()
        {
            var ws = WorkshopSystem.Instance;
            if (ws.TonightCustomersServed > 0)
                _revenueText.text = $"今晚营收: {ws.TonightRevenue}G ({ws.TonightCustomersServed}位客人)";
        }

        #endregion

        #region Crafting

        private void PopulateRecipes()
        {
            foreach (Transform child in _craftContent)
                Destroy(child.gameObject);

            if (!ConfigManager.Instance.IsLoaded) return;

            foreach (var recipe in ConfigManager.Instance.Tables.TbRecipe.DataList)
            {
                if (recipe.UnlockDay > GameManager.Instance.CurrentDay) continue;
                int recipeId = recipe.Id;
                CreateRecipeCard(recipe, recipeId);
            }
        }

        private void CreateRecipeCard(cfg.Recipe recipe, int recipeId)
        {
            var card = UIHelper.CreateRect($"R_{recipe.Id}", _craftContent);
            card.sizeDelta = new Vector2(220, 0);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = new Color(0.25f, 0.2f, 0.15f);
            cardImg.raycastTarget = false;

            var tagColor = NeedTagHelper.GetColor(recipe.NeedTag);

            var tagStripe = UIHelper.CreateRect("TagStripe", card);
            tagStripe.Set(0f, 0f, 0.02f, 1f);
            tagStripe.gameObject.AddComponent<Image>().color = tagColor;

            UIHelper.CreateText("Name", card,
                $"{NeedTagHelper.GetIcon(recipe.NeedTag)} {recipe.ProductName}", 13,
                TextAnchor.MiddleLeft, UIHelper.TextWhite)
                .GetComponent<RectTransform>().Set(0.04f, 0f, 0.42f, 1f);

            UIHelper.CreateButton("N", card, "普",
                () => DoCraft(recipeId, ProductQuality.Normal),
                new Color(0.4f, 0.55f, 0.4f), 13)
                .GetComponent<RectTransform>().Set(0.43f, 0.1f, 0.6f, 0.9f);

            UIHelper.CreateButton("F", card, "精",
                () => DoCraft(recipeId, ProductQuality.Fine),
                UIHelper.AccentBlue, 13)
                .GetComponent<RectTransform>().Set(0.62f, 0.1f, 0.79f, 0.9f);

            UIHelper.CreateButton("E", card, "卓",
                () => DoCraft(recipeId, ProductQuality.Excellent),
                UIHelper.AccentGold, 13)
                .GetComponent<RectTransform>().Set(0.81f, 0.1f, 0.98f, 0.9f);
        }

        private void DoCraft(int recipeId, ProductQuality quality)
        {
            string blockReason = WorkshopSystem.Instance.GetCraftBlockReason(recipeId, quality);
            if (blockReason != null)
            {
                _statusText.text = $"<color=#E05050>制作失败：{blockReason}</color>";
                return;
            }

            var product = WorkshopSystem.Instance.CraftProduct(recipeId, quality);
            if (product != null)
            {
                string qualLabel = quality switch
                {
                    ProductQuality.Fine => "<color=#88BBFF>精品</color>",
                    ProductQuality.Excellent => "<color=#EBCA6A>卓越</color>",
                    _ => "普通"
                };
                _statusText.text = $"制作完成！{qualLabel}「{product.Name}」已上架。";
                RefreshShelf();
                RefreshResources();
            }
        }

        #endregion

        #region Shelf

        private void RefreshShelf()
        {
            foreach (Transform child in _shelfContent)
                Destroy(child.gameObject);

            var inv = WorkshopSystem.Instance.Inventory;
            _inventoryLabel.text = $"库存: {inv.Count} 件";

            for (int i = 0; i < inv.Count; i++)
                CreateShelfItem(inv[i], i);
        }

        private void CreateShelfItem(ProductInstance product, int index)
        {
            var card = UIHelper.CreateRect($"P_{index}", _shelfContent);
            card.sizeDelta = new Vector2(120, 0);

            var tagColor = NeedTagHelper.GetColor(product.NeedTag);
            var qualColor = product.Quality switch
            {
                ProductQuality.Fine => UIHelper.AccentBlue,
                ProductQuality.Excellent => UIHelper.AccentGold,
                _ => new Color(0.5f, 0.5f, 0.45f)
            };

            var bg = card.gameObject.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.17f, 0.12f);
            bg.raycastTarget = true;

            var tagStripe = UIHelper.CreateRect("TagStripe", card);
            tagStripe.Set(0f, 0f, 1f, 0.06f);
            tagStripe.gameObject.AddComponent<Image>().color = tagColor;

            var qualStripe = UIHelper.CreateRect("QualStripe", card);
            qualStripe.Set(0f, 0.94f, 1f, 1f);
            qualStripe.gameObject.AddComponent<Image>().color = qualColor;

            bool isMatch = _currentCustomer != null &&
                           _currentCustomer.State == CustomerState.Waiting &&
                           product.NeedTag == _currentCustomer.NeedTag;

            if (isMatch)
            {
                var glow = UIHelper.CreatePanel("Glow", card.transform,
                    new Color(tagColor.r, tagColor.g, tagColor.b, 0.15f));
                glow.GetComponent<RectTransform>().Set(0f, 0f, 1f, 1f);
            }

            UIHelper.CreateText("Tag", card,
                NeedTagHelper.GetIcon(product.NeedTag), 12,
                TextAnchor.MiddleLeft, tagColor)
                .GetComponent<RectTransform>().Set(0.05f, 0.7f, 0.3f, 0.92f);

            UIHelper.CreateText("Name", card, product.Name, 13,
                TextAnchor.MiddleCenter, UIHelper.TextWhite)
                .GetComponent<RectTransform>().Set(0.03f, 0.35f, 0.97f, 0.7f);

            UIHelper.CreateText("Price", card,
                $"{product.SellingPrice}G", 14, TextAnchor.MiddleCenter,
                isMatch ? UIHelper.AccentGold : UIHelper.TextGray)
                .GetComponent<RectTransform>().Set(0.03f, 0.08f, 0.97f, 0.35f);

            var capturedProduct = product;
            var btn = bg.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => OnShelfItemClicked(capturedProduct));

            var colors = btn.colors;
            colors.highlightedColor = isMatch
                ? new Color(tagColor.r * 0.4f, tagColor.g * 0.4f, tagColor.b * 0.4f)
                : new Color(0.3f, 0.25f, 0.18f);
            colors.pressedColor = new Color(0.15f, 0.12f, 0.08f);
            btn.colors = colors;
        }

        private void OnShelfItemClicked(ProductInstance product)
        {
            if (_currentCustomer == null || _currentCustomer.State != CustomerState.Waiting)
            {
                string tagName = NeedTagHelper.GetName(product.NeedTag);
                _statusText.text = $"「{product.Name}」[{tagName}] 售价 {product.SellingPrice}G";
                return;
            }

            var sat = CustomerSystem.Instance.ServeCustomer(_currentCustomer, product);
            if (sat == Satisfaction.None) return;

            bool tagMatch = product.NeedTag == _currentCustomer.NeedTag;
            string reaction = sat switch
            {
                Satisfaction.VeryHappy =>
                    $"「太棒了！这正是我梦寐以求的！」",
                Satisfaction.Happy =>
                    $"「很好，这东西质量不错！」",
                Satisfaction.OK =>
                    $"「嗯，够用了，谢谢。」",
                Satisfaction.Unhappy =>
                    $"「……这不太是我要的东西，算了凑合吧。」",
                _ => ""
            };

            string satLabel = sat switch
            {
                Satisfaction.VeryHappy => "<color=#FF88CC>★ 非常满意！</color>",
                Satisfaction.Happy => "<color=#88DD88>满意</color>",
                Satisfaction.OK => "<color=#BBBB88>还行</color>",
                Satisfaction.Unhappy => "<color=#CC8888>不满意</color>",
                _ => ""
            };

            _speechText.text = reaction;
            _statusText.text = $"售出「{product.Name}」— {satLabel} — 获得 <color=#EBCA6A>{_currentCustomer.PaidPrice}G</color>";

            RefreshShelf();
            RefreshResources();
            RefreshRevenue();
        }

        #endregion

        #region Customers

        private IEnumerator CustomerLoop()
        {
            yield return new WaitForSeconds(1f);

            var cs = CustomerSystem.Instance;
            while (cs.HasMoreCustomers())
            {
                _currentCustomer = cs.GetNextCustomer();
                if (_currentCustomer == null) break;

                RefreshShelf();
                yield return CustomerEnter(_currentCustomer);

                while (_currentCustomer.State == CustomerState.Waiting)
                    yield return null;

                bool happy = _currentCustomer.State == CustomerState.Served &&
                             _currentCustomer.Satisfaction >= Satisfaction.OK;

                ClearSellButtons();

                yield return new WaitForSeconds(happy ? 0.8f : 0.4f);
                _speechBubble.SetActive(false);

                yield return CustomerLeave(_currentCustomerRT, happy);
                _currentCustomerRT = null;
                _currentCustomer = null;

                RefreshShelf();

                yield return new WaitForSeconds(0.3f);
            }

            _currentCustomer = null;
            _statusText.text = "今晚的顾客都已离开。可以结束营业了。";
            _speechBubble.SetActive(false);
            ClearSellButtons();
            RefreshRevenue();
        }

        private IEnumerator CustomerEnter(CustomerInstance customer)
        {
            if (_currentCustomerRT != null)
            {
                Destroy(_currentCustomerRT.gameObject);
                _currentCustomerRT = null;
            }

            int colorIdx = customer.ConfigId % CustomerColors.Length;
            var npc = UIHelper.CreateCharacter("Customer", transform,
                CustomerColors[colorIdx], customer.Name);
            _currentCustomerRT = npc;

            npc.pivot = new Vector2(0.5f, 0f);
            npc.localScale = new Vector3(1.1f, 1.1f, 1f);

            var startAnchor = new Vector2(1.15f, 0.12f);
            var endAnchor = new Vector2(0.72f, 0.12f);

            npc.anchorMin = npc.anchorMax = startAnchor;
            npc.anchoredPosition = Vector2.zero;

            float duration = 0.6f;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (npc == null) yield break;
                float ease = 1f - Mathf.Pow(1f - (t / duration), 3f);
                var pos = Vector2.Lerp(startAnchor, endAnchor, ease);
                npc.anchorMin = npc.anchorMax = pos;
                yield return null;
            }

            if (npc == null) yield break;
            npc.anchorMin = npc.anchorMax = endAnchor;

            ShowSpeechBubble(customer);
            ShowSellButtons();
        }

        private IEnumerator CustomerLeave(RectTransform rt, bool happy)
        {
            if (rt == null) yield break;

            if (happy)
            {
                var head = rt.Find("Head");
                if (head != null)
                {
                    var eyes = head.Find("Eyes");
                    if (eyes != null) eyes.GetComponent<Text>().text = "^ ^";
                }
            }

            yield return new WaitForSeconds(0.3f);
            if (rt == null) yield break;

            var startPos = rt.anchorMin;
            var endPos = new Vector2(happy ? -0.2f : 1.15f, startPos.y);
            float duration = 0.5f;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (rt == null) yield break;
                var pos = Vector2.Lerp(startPos, endPos, t / duration);
                rt.anchorMin = rt.anchorMax = pos;
                yield return null;
            }

            if (rt != null) Destroy(rt.gameObject);
        }

        private void ShowSpeechBubble(CustomerInstance customer)
        {
            _speechName.text = customer.Name;
            _speechText.text = $"\"{customer.NeedDescription}\"";

            var tagColor = NeedTagHelper.GetColor(customer.NeedTag);
            _speechTagBg.color = tagColor;
            _speechTagLabel.text = $"{NeedTagHelper.GetIcon(customer.NeedTag)} {NeedTagHelper.GetName(customer.NeedTag)}";

            _speechBubble.SetActive(true);
            _statusText.text = $"顾客来了！需要：<color=#{ColorUtility.ToHtmlStringRGB(tagColor)}>{NeedTagHelper.GetName(customer.NeedTag)}</color>类商品。选择货架上的商品出售。";
        }

        private void ShowSellButtons()
        {
            ClearSellButtons();

            var container = UIHelper.CreateRect("Btns", _sellBtnArea);
            container.anchorMin = Vector2.zero;
            container.anchorMax = Vector2.one;
            container.offsetMin = container.offsetMax = Vector2.zero;

            var layout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(4, 4, 2, 2);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            UIHelper.CreateButton("SkipCust", container, "跳过此顾客",
                OnSkipCustomer, new Color(0.5f, 0.35f, 0.2f), 15);

            if (_currentCustomer != null)
            {
                var tagColor = NeedTagHelper.GetColor(_currentCustomer.NeedTag);
                bool hasMatch = WorkshopSystem.Instance
                    .GetProductsMatchingNeed(_currentCustomer.NeedTag).Count > 0;
                string hint = hasMatch
                    ? $"<color=#{ColorUtility.ToHtmlStringRGB(tagColor)}>← 点击带 {NeedTagHelper.GetIcon(_currentCustomer.NeedTag)} 标记的商品出售</color>"
                    : "<color=#CC8888>货架上没有匹配商品，试着制作一件？</color>";

                UIHelper.CreateText("Hint", container, hint,
                    14, TextAnchor.MiddleCenter, UIHelper.TextGray);
            }
        }

        private void ClearSellButtons()
        {
            foreach (Transform child in _sellBtnArea)
                Destroy(child.gameObject);
        }

        private void OnSkipCustomer()
        {
            if (_currentCustomer == null || _currentCustomer.State != CustomerState.Waiting) return;
            _currentCustomer.State = CustomerState.Left;
            _speechText.text = "\"算了，我去别处看看吧……\"";
            _statusText.text = $"{_currentCustomer.Name} 失望地离开了。";
        }

        #endregion
    }
}
