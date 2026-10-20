using UnityEngine;
using UnityEngine.EventSystems;

namespace WelCome.UI
{
    public sealed class UiPointerBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerClick(PointerEventData eventData) { }

        public void OnPointerDown(PointerEventData eventData) { }

        public void OnPointerUp(PointerEventData eventData) { }
    }
}
