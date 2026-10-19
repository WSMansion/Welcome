using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WelCome.UI
{
    public sealed class DisplayCaseInventorySlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text countText;
        [SerializeField] private Text nameText;
        [SerializeField] private Button button;

        public void Bind(Sprite sprite, int count, string productName, UnityAction onClick)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.preserveAspect = true;
            countText.text = "×" + count;
            nameText.text = string.IsNullOrWhiteSpace(productName) ? "未命名商品" : productName;
            button.onClick.RemoveAllListeners();
            if (onClick != null)
                button.onClick.AddListener(onClick);
        }
    }
}
