using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// World-space prompt prefab shown above the current interactable.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(WorldCameraScaleCompensation))]
    public sealed class WorldInteractionPrompt : MonoBehaviour
    {
        private const int PromptSortingOrder = short.MaxValue;

        private SpriteRenderer promptRenderer;
        private WorldCameraScaleCompensation cameraScaleCompensation;

        private void Awake()
        {
            promptRenderer = GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = false;
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
                promptRenderer.sortingLayerID = referenceRenderer.sortingLayerID;
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = true;
        }

        public void Hide()
        {
            if (promptRenderer != null)
                promptRenderer.enabled = false;
        }
    }
}
