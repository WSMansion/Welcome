using UnityEngine;
using WelCome.Core;

namespace WelCome.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class ShopCameraFollow : MonoBehaviour
    {
        public Transform target;
        [Min(0f)] public float smoothTime = 0.12f;
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        [Header("镜头活动框")]
        [InspectorName("相机中心可移动范围"), Tooltip("青色框表示相机中心的活动区域；跟随时相机中心不会越过框线。")]
        public Rect cameraMoveBounds = new Rect(0f, 0f, 32f, 18f);

        [Header("缩放")]
        [Min(0.01f), InspectorName("滚轮缩放速度"), Tooltip("滚轮输入对正交相机视野大小的调整速度。")]
        public float zoomSpeed = 8f;
        [Min(0.1f), InspectorName("最近视野高度"), Tooltip("相机可缩放到的最小正交视野高度。")]
        public float minOrthographicSize = 6f;
        [Min(0.1f), InspectorName("最远视野高度"), Tooltip("相机可缩放到的最大正交视野高度。")]
        public float maxOrthographicSize = 13f;

        private Camera sceneCamera;
        private GameManager gameManager;
        private Vector3 smoothVelocity;
        private bool initialized;

        private void Awake()
        {
            sceneCamera = GetComponent<Camera>();
            gameManager = FindObjectOfType<GameManager>();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            if (sceneCamera == null)
                sceneCamera = GetComponent<Camera>();
            if (sceneCamera == null)
                return;

            if (!IsShopOpenOrStandalonePreview())
            {
                smoothVelocity = Vector3.zero;
                return;
            }

            ApplyZoom();

            Vector3 desired = ClampToCameraBounds(target.position + offset);

            if (!initialized)
            {
                transform.position = desired;
                initialized = true;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref smoothVelocity,
                smoothTime);
            transform.position = ClampToCameraBounds(transform.position);
        }

        private bool IsShopOpenOrStandalonePreview()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            return gameManager == null
                || gameManager.CurrentState == GameState.None
                || gameManager.CurrentState == GameState.DayPhase;
        }

        private void ApplyZoom()
        {
            if (!sceneCamera.orthographic)
                return;

            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scrollDelta, 0f))
                return;

            float minSize = Mathf.Max(0.1f, Mathf.Min(minOrthographicSize, maxOrthographicSize));
            float maxSize = Mathf.Max(minSize, Mathf.Max(minOrthographicSize, maxOrthographicSize));
            float adjustedZoomSpeed = Mathf.Max(0f, zoomSpeed);
            sceneCamera.orthographicSize = Mathf.Clamp(
                sceneCamera.orthographicSize - scrollDelta * adjustedZoomSpeed,
                minSize,
                maxSize);
        }

        private Vector3 ClampToCameraBounds(Vector3 position)
        {
            Rect bounds = GetOrderedCameraBounds();
            position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
            position.y = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
            return position;
        }

        private Rect GetOrderedCameraBounds()
        {
            float x0 = cameraMoveBounds.x;
            float x1 = x0 + cameraMoveBounds.width;
            float y0 = cameraMoveBounds.y;
            float y1 = y0 + cameraMoveBounds.height;
            return Rect.MinMaxRect(
                Mathf.Min(x0, x1),
                Mathf.Min(y0, y1),
                Mathf.Max(x0, x1),
                Mathf.Max(y0, y1));
        }

        private void OnDrawGizmos()
        {
            Rect bounds = GetOrderedCameraBounds();
            Color previousColor = Gizmos.color;
            Gizmos.color = new Color(0.15f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireCube(
                new Vector3(bounds.center.x, bounds.center.y, 0f),
                new Vector3(bounds.width, bounds.height, 0f));
            Gizmos.color = previousColor;
        }
    }
}
