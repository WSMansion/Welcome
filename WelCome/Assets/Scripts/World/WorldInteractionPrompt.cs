using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// World-space prompt prefab shown above the current interactable.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WorldCameraScaleCompensation))]
    public sealed class WorldInteractionPrompt : MonoBehaviour
    {
        private const int PromptSortingOrder = 3000;

        private static int HudSortingLayerId => SortingLayer.NameToID("HUD");

        [SerializeField, InspectorName("按键文字")] private TextMesh keyLabel;
        private SpriteRenderer promptRenderer;
        private MeshRenderer keyRenderer;
        private WorldCameraScaleCompensation cameraScaleCompensation;

        private void Awake()
        {
            Transform background = transform.Find("bg");
            promptRenderer = background != null ? background.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();
            if (keyLabel == null)
            {
                Transform child = transform.Find("按键文字");
                if (child != null)
                    keyLabel = child.GetComponent<TextMesh>();
            }
            keyRenderer = keyLabel != null ? keyLabel.GetComponent<MeshRenderer>() : null;
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = false;
            SetKeyVisible(false);
        }

        public void ShowAt(Vector3 worldPosition, SpriteRenderer referenceRenderer)
        {
            if (promptRenderer == null)
                return;

            transform.position = worldPosition;
            transform.rotation = Quaternion.identity;
            if (cameraScaleCompensation != null)
                cameraScaleCompensation.RefreshScale();

            if (referenceRenderer != null)
                promptRenderer.sortingLayerID = HudSortingLayerId;
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = true;
            if (keyRenderer != null)
            {
                keyRenderer.sortingLayerID = promptRenderer.sortingLayerID;
                keyRenderer.sortingOrder = PromptSortingOrder + 1;
            }
            SetKeyVisible(true);
        }

        public void SetKeyText(string keyText)
        {
            if (keyLabel != null && !string.IsNullOrEmpty(keyText))
                keyLabel.text = keyText;
        }

        public void Hide()
        {
            if (promptRenderer != null)
                promptRenderer.enabled = false;
            SetKeyVisible(false);
        }

        private void SetKeyVisible(bool visible)
        {
            if (keyLabel != null)
                keyLabel.gameObject.SetActive(visible);
        }
    }
}
