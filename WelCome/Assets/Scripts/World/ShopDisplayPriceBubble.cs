using UnityEngine;

namespace WelCome.World
{
    public sealed class ShopDisplayPriceBubble : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private TextMesh label;

        public void ShowPrice(int price)
        {
            if (label != null)
                label.text = price + "金";
            gameObject.SetActive(true);
        }

        public void HidePrice()
        {
            gameObject.SetActive(false);
        }

        public void SetSorting(int sortingLayerId, int order)
        {
            if (background != null)
            {
                background.sortingLayerID = sortingLayerId;
                background.sortingOrder = order;
            }
            MeshRenderer textRenderer = label != null ? label.GetComponent<MeshRenderer>() : null;
            if (textRenderer != null)
            {
                textRenderer.sortingLayerID = sortingLayerId;
                textRenderer.sortingOrder = order + 1;
            }
        }
    }
}