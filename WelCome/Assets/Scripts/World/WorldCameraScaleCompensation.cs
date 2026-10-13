using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// Keeps a world-space object's apparent size steady while an orthographic camera zooms.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldCameraScaleCompensation : MonoBehaviour
    {
        [SerializeField, Min(0.05f), InspectorName("屏幕尺寸倍率"),
         Tooltip("以预制体原始尺寸为 1；调大后对象在镜头缩放时仍保持对应的屏幕尺寸。")]
        private float sizeMultiplier = 1f;

        [SerializeField, Range(0f, 1f), InspectorName("缩放响应"),
         Tooltip("1 表示完全抵消镜头缩放；0 表示完全跟着世界缩放。文字可调低，让尺寸变化更缓和。")]
        private float zoomResponse = 1f;

        [SerializeField, InspectorName("指定相机"),
         Tooltip("留空时使用 Main Camera。")]
        private Camera targetCamera;

        private static float sharedReferenceOrthographicSize;

        private Vector3 baseScale;
        private bool hasBaseScale;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedReference()
        {
            sharedReferenceOrthographicSize = 0f;
        }

        private void Awake()
        {
            CaptureBaseScale();
            CacheCameraReference();
            RefreshScale();
        }

        private void OnEnable()
        {
            if (hasBaseScale)
                RefreshScale();
        }

        private void LateUpdate()
        {
            RefreshScale();
        }

        /// <summary>Reapplies the zoom compensation immediately after a position or camera change.</summary>
        public void RefreshScale()
        {
            CaptureBaseScale();
            CacheCameraReference();

            float zoomCompensation = 1f;
            if (targetCamera != null && targetCamera.orthographic && sharedReferenceOrthographicSize > 0f)
                zoomCompensation = targetCamera.orthographicSize / sharedReferenceOrthographicSize;

            float easedZoom = Mathf.Lerp(1f, zoomCompensation, Mathf.Clamp01(zoomResponse));
            transform.localScale = baseScale * Mathf.Max(0.05f, sizeMultiplier) * easedZoom;
        }

        private void CaptureBaseScale()
        {
            if (hasBaseScale)
                return;

            baseScale = transform.localScale;
            hasBaseScale = true;
        }

        private void CacheCameraReference()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (sharedReferenceOrthographicSize <= 0f && targetCamera != null && targetCamera.orthographic)
                sharedReferenceOrthographicSize = targetCamera.orthographicSize;
        }
    }
}
