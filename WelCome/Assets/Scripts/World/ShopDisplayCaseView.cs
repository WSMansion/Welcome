using UnityEngine;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.World
{
    /// <summary>
    /// Scene representation of one display case placement. The Luban row controls its unlock day and slot count.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopDisplayCaseView : MonoBehaviour
    {
        private SpriteRenderer[] productRenderers;

        [Header("配置")]
        [Min(1), InspectorName("陈列柜配置 ID")] public int configId = 8101;

        [Header("组件")]
        [InspectorName("柜子图片")] public SpriteRenderer spriteRenderer;
        [InspectorName("阻挡碰撞体")] public Collider2D blockingCollider;
        [InspectorName("锁定标记")] public SpriteRenderer lockedIndicatorRenderer;
        [InspectorName("顾客浏览位置")] public Transform browsingPoint;
        [InspectorName("商品槽位")] public Transform[] productSlots;

        [Header("外观")]
        [InspectorName("锁定柜体颜色")] public Color lockedTint = new Color(0.45f, 0.43f, 0.48f, 1f);

        public int ConfigId => configId;
        public Vector2 BrowsingPosition => browsingPoint != null
            ? (Vector2)browsingPoint.position
            : (Vector2)(transform.position + Vector3.down * 0.9f);

        public bool IsUnlocked => DisplayCaseSystem.Instance.IsUnlocked(configId);

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (blockingCollider == null)
                blockingCollider = GetComponent<Collider2D>();
            if (lockedIndicatorRenderer == null)
                lockedIndicatorRenderer = transform.Find("锁定标记")?.GetComponent<SpriteRenderer>();
            CacheProductRenderers();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            EventBus.Subscribe<DisplayCasesChangedEvent>(OnDisplayCasesChanged);
            ApplySavedPlacement();
            RefreshLockState();
        }

        private void OnDisable()
        {
            SyncPlacementToSave();
            EventBus.Unsubscribe<DisplayCasesChangedEvent>(OnDisplayCasesChanged);
        }

        private void LateUpdate()
        {
            SyncPlacementToSave();
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 300 + Mathf.RoundToInt(-transform.position.y * 10f);

            int overlaySortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 301;
            if (lockedIndicatorRenderer != null)
                lockedIndicatorRenderer.sortingOrder = overlaySortingOrder;
            if (productRenderers == null)
                return;

            foreach (SpriteRenderer productRenderer in productRenderers)
            {
                if (productRenderer != null)
                    productRenderer.sortingOrder = overlaySortingOrder;
            }
        }

        public int GetSlotCount()
        {
            return DisplayCaseSystem.Instance.GetSlotCount(configId);
        }

        public Transform GetSlot(int slotIndex)
        {
            if (productSlots == null || slotIndex < 0 || slotIndex >= productSlots.Length)
                return null;
            return productSlots[slotIndex];
        }

        public void RefreshLockState()
        {
            DisplayCaseSystem displayCaseSystem = DisplayCaseSystem.Instance;
            bool unlocked = displayCaseSystem != null && displayCaseSystem.IsUnlocked(configId);
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = unlocked ? Color.white : lockedTint;
            }
            if (lockedIndicatorRenderer != null)
                lockedIndicatorRenderer.enabled = !unlocked;
            // A locked cabinet still occupies floor space; locking only blocks use of its slots.
            if (blockingCollider != null)
                blockingCollider.enabled = true;

            RefreshProductVisuals(displayCaseSystem, unlocked);
        }

        public void SyncPlacementToSave()
        {
            if (!Application.isPlaying)
                return;

            DisplayCaseSystem displayCaseSystem = DisplayCaseSystem.Instance;
            if (displayCaseSystem == null)
                return;

            displayCaseSystem.SetPlacement(configId, transform.position, transform.eulerAngles.z);
        }

        private void OnDisplayCasesChanged(DisplayCasesChangedEvent evt)
        {
            ApplySavedPlacement();
            RefreshLockState();
        }

        private void ApplySavedPlacement()
        {
            if (DisplayCaseSystem.Instance.TryGetPlacement(configId, out DisplayCasePlacementEntry placement))
            {
                transform.position = new Vector3(placement.x, placement.y, placement.z);
                transform.rotation = Quaternion.Euler(0f, 0f, placement.rotationZ);
            }
            else
            {
                SyncPlacementToSave();
            }
        }

        private void RefreshProductVisuals(DisplayCaseSystem displayCaseSystem, bool unlocked)
        {
            if (productSlots == null)
                return;

            if (productRenderers == null || productRenderers.Length != productSlots.Length)
                CacheProductRenderers();

            int configuredSlotCount = displayCaseSystem != null
                ? displayCaseSystem.GetSlotCount(configId)
                : 0;

            for (int i = 0; i < productSlots.Length; i++)
            {
                SpriteRenderer productRenderer = productRenderers[i];
                if (productRenderer == null)
                    continue;

                ProductInstance displayed = unlocked && displayCaseSystem != null && i < configuredSlotCount
                    ? displayCaseSystem.GetDisplayedProduct(configId, i)
                    : null;
                bool hasProduct = displayed != null;
                productRenderer.enabled = hasProduct;
                if (hasProduct)
                {
                    Sprite icon = ProductIcons.Load(displayed.RecipeId);
                    if (icon != null)
                        productRenderer.sprite = icon;
                }
            }
        }

        private void CacheProductRenderers()
        {
            if (productSlots == null)
            {
                productRenderers = null;
                return;
            }

            productRenderers = new SpriteRenderer[productSlots.Length];
            for (int i = 0; i < productSlots.Length; i++)
            {
                Transform slot = productSlots[i];
                productRenderers[i] = slot != null ? slot.GetComponent<SpriteRenderer>() : null;
            }
        }
    }
}
