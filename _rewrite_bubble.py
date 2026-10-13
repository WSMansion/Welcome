from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/ShopCustomerSpeechBubble.cs')
p.write_text('''using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// One reusable prefab that shows one configured icon. Spoken or inner text uses TextThought.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopCustomerSpeechBubble : MonoBehaviour
    {
        public enum IconKind
        {
            Thought,
            Greeting,
            MoodDown,
            MoodHappy
        }

        [SerializeField, InspectorName("气泡底图")]
        private SpriteRenderer backgroundRenderer;

        [SerializeField, InspectorName("可替换图标")]
        private SpriteRenderer iconRenderer;

        [Header("图标资源")]
        [SerializeField, InspectorName("思考")]
        private Sprite thoughtIcon;
        [SerializeField, InspectorName("打招呼")]
        private Sprite greetingIcon;
        [SerializeField, InspectorName("低落")]
        private Sprite moodDownIcon;
        [SerializeField, InspectorName("开心")]
        private Sprite moodHappyIcon;

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
                    && moodHappyIcon != null;
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
            return true;
        }

        public void Hide()
        {
            ResolveVisualParts();
            if (backgroundRenderer != null)
                backgroundRenderer.enabled = false;
            if (iconRenderer != null)
                iconRenderer.enabled = false;
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

        private void RestoreDefaultAppearance()
        {
            if (hasBubbleScale)
                transform.localScale = defaultBubbleScale;
            if (backgroundRenderer != null && hasBackgroundSize)
                backgroundRenderer.size = defaultBackgroundSize;
        }
    }
}
''', encoding='utf-8', newline='\n')
print('rewritten')
