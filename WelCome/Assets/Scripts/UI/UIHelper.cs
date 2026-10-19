using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WelCome.UI
{
    public static class UIHelper
    {
        public static readonly Color BgDark = new(0.12f, 0.12f, 0.16f, 0.95f);
        public static readonly Color BgPanel = new(0.18f, 0.18f, 0.24f, 0.9f);
        public static readonly Color BgCard = new(0.22f, 0.22f, 0.30f, 1f);
        public static readonly Color AccentGold = new(0.92f, 0.78f, 0.42f, 1f);
        public static readonly Color AccentBlue = new(0.35f, 0.55f, 0.85f, 1f);
        public static readonly Color TextWhite = new(0.95f, 0.93f, 0.88f, 1f);
        public static readonly Color TextGray = new(0.65f, 0.63f, 0.58f, 1f);

        public static readonly Color Parchment = new(0.82f, 0.71f, 0.55f, 1f);
        public static readonly Color ParchmentDark = new(0.55f, 0.45f, 0.35f, 1f);
        public static readonly Color WoodDark = new(0.28f, 0.22f, 0.16f, 1f);
        public static readonly Color WoodMedium = new(0.4f, 0.32f, 0.22f, 1f);
        public static readonly Color WoodLight = new(0.52f, 0.42f, 0.3f, 1f);
        public static readonly Color SkinTone = new(0.88f, 0.76f, 0.65f, 1f);

        private static Font _cachedFont;
        public static Font DefaultFont
        {
            get
            {
                if (_cachedFont == null)
                {
#if UNITY_EDITOR
                    _cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/UiChinese.ttf");
#endif
                    if (_cachedFont == null)
                        _cachedFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
                }
                return _cachedFont;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        public static Image CreatePanel(string name, Transform parent, Color? color = null,
            bool blockRaycast = false)
        {
            var rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = rect.gameObject.AddComponent<Image>();
            img.color = color ?? BgDark;
            img.raycastTarget = blockRaycast;
            return img;
        }

        public static Text CreateText(string name, Transform parent, string text,
            int fontSize = 24, TextAnchor align = TextAnchor.MiddleLeft, Color? color = null)
        {
            var rect = CreateRect(name, parent);
            var txt = rect.gameObject.AddComponent<Text>();
            txt.text = text;
            txt.font = DefaultFont;
            txt.fontSize = fontSize;
            txt.alignment = align;
            txt.color = color ?? TextWhite;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            return txt;
        }

        public static Button CreateButton(string name, Transform parent, string label,
            UnityAction onClick, Color? bgColor = null, int fontSize = 22)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(240, 56);

            var img = rect.gameObject.AddComponent<Image>();
            img.color = bgColor ?? AccentGold;
            img.raycastTarget = true;

            var btn = rect.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var textRect = CreateRect("Label", rect);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 4);
            textRect.offsetMax = new Vector2(-8, -4);

            var txt = textRect.gameObject.AddComponent<Text>();
            txt.text = label;
            txt.font = DefaultFont;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            txt.raycastTarget = false;

            return btn;
        }

        public static ScrollRect CreateScrollView(string name, Transform parent,
            bool horizontal = false)
        {
            var rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var scrollImg = rect.gameObject.AddComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0.01f);
            scrollImg.raycastTarget = true;

            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = horizontal;
            scroll.vertical = !horizontal;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            var viewport = CreateRect("Viewport", rect);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = CreateRect("Content", viewport);
            if (horizontal)
            {
                content.anchorMin = new Vector2(0, 0);
                content.anchorMax = new Vector2(0, 1);
                content.pivot = new Vector2(0, 0.5f);
            }
            else
            {
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1);
            }
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            scroll.viewport = viewport;
            scroll.content = content;

            return scroll;
        }

        /// <summary>
        /// Creates a simple character composed of body + head + name label.
        /// Root pivot is at bottom-center for floor-standing placement.
        /// </summary>
        public static RectTransform CreateCharacter(string name, Transform parent,
            Color bodyColor, string displayName)
        {
            var root = CreateRect(name, parent);
            root.sizeDelta = new Vector2(70, 130);
            root.pivot = new Vector2(0.5f, 0f);

            var nameLabel = CreateText("Name", root, displayName, 13,
                TextAnchor.LowerCenter, TextWhite);
            nameLabel.GetComponent<RectTransform>().Set(0f, 0.87f, 1f, 1f);

            var head = CreateRect("Head", root);
            head.Set(0.2f, 0.54f, 0.8f, 0.86f);
            var headImg = head.gameObject.AddComponent<Image>();
            headImg.color = SkinTone;
            headImg.raycastTarget = false;

            var eyes = CreateText("Eyes", head, ". .", 11,
                TextAnchor.MiddleCenter, new Color(0.2f, 0.15f, 0.1f));
            eyes.GetComponent<RectTransform>().Set(0f, 0.2f, 1f, 0.9f);

            var body = CreateRect("Body", root);
            body.Set(0.12f, 0.08f, 0.88f, 0.57f);
            var bodyImg = body.gameObject.AddComponent<Image>();
            bodyImg.color = bodyColor;
            bodyImg.raycastTarget = false;

            return root;
        }

        /// <summary>
        /// Creates a portrait image loaded as Sprite asset from Resources/.
        /// Pivot at bottom-center for floor-standing placement.
        /// </summary>
        public static RectTransform CreatePortrait(string goName, Transform parent,
            string resourcePath, float width = 200f, float height = 400f)
        {
            var root = CreateRect(goName, parent);
            root.sizeDelta = new Vector2(width, height);
            root.pivot = new Vector2(0.5f, 0f);

            var img = root.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;

            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.5f, 0.3f, 0.5f, 0.5f);
                Debug.LogWarning($"[UIHelper] Sprite not found: {resourcePath}");
            }

            return root;
        }

        /// <summary>
        /// Draws a dotted path between two anchor positions.
        /// </summary>
        public static void DrawDottedPath(Transform parent, Vector2 from, Vector2 to,
            Color color, int dots = 10, float dotSize = 4f)
        {
            for (int i = 0; i <= dots; i++)
            {
                float t = (float)i / dots;
                var pos = Vector2.Lerp(from, to, t);
                var dot = CreateRect("dot", parent);
                dot.anchorMin = dot.anchorMax = pos;
                dot.anchoredPosition = Vector2.zero;
                dot.sizeDelta = new Vector2(dotSize, dotSize);
                dot.SetAsFirstSibling();
                var img = dot.gameObject.AddComponent<Image>();
                img.color = color;
                img.raycastTarget = false;
            }
        }
    }

    public static class RectTransformExtensions
    {
        public static void Set(this RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
