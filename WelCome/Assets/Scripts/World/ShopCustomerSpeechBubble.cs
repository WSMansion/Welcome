using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// One reusable prefab that shows one configured icon. Spoken or inner text uses CustomerTextThought.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopCustomerSpeechBubble : MonoBehaviour
    {
        public enum IconKind
        {
            Thought,
            Greeting,
            MoodDown,
            MoodHappy,
            Angry,
            Sad,
            Wave,
            BrowseTogether,
            Pay
        }

        [SerializeField, InspectorName("气泡底图")]
        private SpriteRenderer backgroundRenderer;

        [SerializeField, InspectorName("可替换图标")]
        private SpriteRenderer iconRenderer;
        private SpriteRenderer countdownMask;

        [SerializeField, Min(0f), InspectorName("图标内边距"),
         Tooltip("气泡底图会按图标尺寸加上这里的左右、上下留白自动拉伸。")]
        private Vector2 iconPadding = new Vector2(0.18f, 0.14f);

        [Header("图标资源")]
        [SerializeField, InspectorName("思考")]
        private Sprite thoughtIcon;
        [SerializeField, InspectorName("打招呼")]
        private Sprite greetingIcon;
        [SerializeField, InspectorName("低落")]
        private Sprite moodDownIcon;
        [SerializeField, InspectorName("开心")]
        private Sprite moodHappyIcon;
        [SerializeField, InspectorName("生气")]
        private Sprite angryIcon;
        [SerializeField, InspectorName("难过")]
        private Sprite sadIcon;
        [SerializeField, InspectorName("挥手告别")]
        private Sprite waveIcon;
        [SerializeField, InspectorName("一起逛")]
        private Sprite browseTogetherIcon;
        [SerializeField, InspectorName("等待结账")]
        private Sprite payIcon;

        private Vector3 defaultBubbleScale;
        private Vector2 defaultBackgroundSize;
        private bool hasBubbleScale;
        private bool hasBackgroundSize;

        public bool HasRequiredVisuals
        {
            get
            {
                ResolveVisualParts();
                return backgroundRenderer != null
                    && iconRenderer != null
                    && thoughtIcon != null
                    && greetingIcon != null
                    && moodDownIcon != null
                    && moodHappyIcon != null
                    && angryIcon != null
                    && sadIcon != null
                    && waveIcon != null
                    && browseTogetherIcon != null;
            }
        }

        private void Awake()
        {
            ResolveVisualParts();
            CaptureDefaultAppearance();
            Hide();
        }

        /// <summary>Wires the visual slots and icon assets when the editor builds the prefab.</summary>
        public void Configure(SpriteRenderer background, SpriteRenderer icon,
            Sprite thought, Sprite greeting, Sprite moodDown, Sprite moodHappy)
        {
            backgroundRenderer = background;
            iconRenderer = icon;
            thoughtIcon = thought;
            greetingIcon = greeting;
            moodDownIcon = moodDown;
            moodHappyIcon = moodHappy;
            CaptureDefaultAppearance();
        }

        public bool ShowIcon(IconKind iconKind)
        {
            ResolveVisualParts();
            CaptureDefaultAppearance();
            Sprite icon = ResolveIcon(iconKind);
            if (backgroundRenderer == null || iconRenderer == null || icon == null)
                return false;

            RestoreDefaultAppearance();
            iconRenderer.sprite = icon;
            iconRenderer.enabled = true;
            backgroundRenderer.enabled = true;
            EnsureCountdownMask();
            if (countdownMask != null)
                countdownMask.enabled = false;
            FitBackgroundToIcon();
            return true;
        }

        public void Hide()
        {
            ResolveVisualParts();
            if (backgroundRenderer != null)
                backgroundRenderer.enabled = false;
            if (iconRenderer != null)
                iconRenderer.enabled = false;
            if (countdownMask != null)
                countdownMask.enabled = false;
        }

        public void SetSorting(int sortingLayerId, int backgroundOrder)
        {
            ResolveVisualParts();
            if (backgroundRenderer != null)
            {
                backgroundRenderer.sortingLayerID = sortingLayerId;
                backgroundRenderer.sortingOrder = backgroundOrder;
            }
            if (iconRenderer != null)
            {
                iconRenderer.sortingLayerID = sortingLayerId;
                iconRenderer.sortingOrder = backgroundOrder + 1;
            }
            if (countdownMask != null)
            {
                countdownMask.sortingLayerID = sortingLayerId;
                countdownMask.sortingOrder = backgroundOrder + 2;
            }
        }

        public void SetCountdown(float remaining)
        {
            EnsureCountdownMask();
            if (countdownMask == null || countdownMask.sprite == null
                || iconRenderer == null || iconRenderer.sprite == null)
                return;

            bool show = backgroundRenderer != null && backgroundRenderer.enabled;
            float cover = 1f - Mathf.Clamp01(remaining);
            if (!show || cover <= 0.001f)
            {
                countdownMask.enabled = false;
                return;
            }

            Transform mask = countdownMask.transform;
            if (mask.parent != iconRenderer.transform)
                mask.SetParent(iconRenderer.transform, false);

            Vector2 iconSize = iconRenderer.sprite.bounds.size;
            Vector2 maskSize = countdownMask.sprite.bounds.size;
            float width = maskSize.x > 0.0001f ? iconSize.x / maskSize.x : 1f;
            float height = maskSize.y > 0.0001f ? iconSize.y * cover / maskSize.y : cover;
            mask.localScale = new Vector3(width, Mathf.Max(0.001f, height), 1f);
            mask.localPosition = new Vector3(0f, iconSize.y * 0.5f, -0.01f);
            countdownMask.sortingLayerID = iconRenderer.sortingLayerID;
            countdownMask.sortingOrder = iconRenderer.sortingOrder + 1;
            countdownMask.enabled = true;
        }

        private void EnsureCountdownMask()
        {
            if (countdownMask != null)
                return;
            Transform existing = transform.Find("倒计时遮罩");
            if (existing == null && iconRenderer != null)
                existing = iconRenderer.transform.Find("倒计时遮罩");
            if (existing != null)
                countdownMask = existing.GetComponent<SpriteRenderer>();
        }

        private Sprite ResolveIcon(IconKind iconKind)
        {
            switch (iconKind)
            {
                case IconKind.Thought:
                    return thoughtIcon;
                case IconKind.Greeting:
                    return greetingIcon;
                case IconKind.MoodDown:
                    return moodDownIcon;
                case IconKind.MoodHappy:
                    return moodHappyIcon;
                case IconKind.Angry:
                    return angryIcon;
                case IconKind.Sad:
                    return sadIcon;
                case IconKind.Wave:
                    return waveIcon;
                case IconKind.BrowseTogether:
                    return browseTogetherIcon;
                case IconKind.Pay:
                    return payIcon;
                default:
                    return null;
            }
        }

        private void ResolveVisualParts()
        {
            if (backgroundRenderer == null)
                backgroundRenderer = GetComponent<SpriteRenderer>();

            if (iconRenderer == null)
            {
                Transform iconTransform = transform.Find("气泡图标");
                if (iconTransform != null)
                    iconRenderer = iconTransform.GetComponent<SpriteRenderer>();
            }
        }

        private void CaptureDefaultAppearance()
        {
            if (!hasBubbleScale)
            {
                defaultBubbleScale = transform.localScale;
                hasBubbleScale = true;
            }

            if (backgroundRenderer != null && !hasBackgroundSize)
            {
                defaultBackgroundSize = backgroundRenderer.size;
                hasBackgroundSize = true;
            }
        }

        private void FitBackgroundToIcon()
        {
            if (backgroundRenderer == null || backgroundRenderer.drawMode == SpriteDrawMode.Simple
                || iconRenderer == null || iconRenderer.sprite == null)
                return;

            Vector2 iconSize = Vector2.Scale(iconRenderer.sprite.bounds.size, Abs(iconRenderer.transform.localScale));
            backgroundRenderer.size = new Vector2(
                Mathf.Max(defaultBackgroundSize.x, iconSize.x + iconPadding.x),
                Mathf.Max(defaultBackgroundSize.y, iconSize.y + iconPadding.y));
        }

        private static Vector2 Abs(Vector3 scale)
        {
            return new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }

        private void RestoreDefaultAppearance()
        {
            if (hasBubbleScale)
                transform.localScale = defaultBubbleScale;
            if (backgroundRenderer != null && hasBackgroundSize)
                backgroundRenderer.size = defaultBackgroundSize;
        }
    }
}
