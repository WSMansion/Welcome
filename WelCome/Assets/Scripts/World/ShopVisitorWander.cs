using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// Lightweight scene-preview movement for shop visitors. Routes are edited as world points in the Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopVisitorWander : MonoBehaviour
    {
        [Header("Route")]
        public Transform[] waypoints;
        [Min(0.05f)] public float moveSpeed = 0.8f;
        [Min(0f)] public float pauseAtWaypoints = 0.8f;

        [Header("Sprites")]
        public SpriteRenderer spriteRenderer;
        public Sprite[] downFrames;
        public Sprite[] leftFrames;
        public Sprite[] rightFrames;
        public Sprite[] upFrames;

        private int targetIndex;
        private float pauseTimer;
        private float animationTimer;
        private Vector2 lastDirection = Vector2.down;
        private bool externallyDriven;
        private ShopCustomerWanderAI autonomousAI;

        public bool HasDirectionalFrames => HasFrames(downFrames)
            && HasFrames(leftFrames)
            && HasFrames(rightFrames)
            && HasFrames(upFrames);

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            autonomousAI = GetComponent<ShopCustomerWanderAI>();
            externallyDriven = autonomousAI != null && autonomousAI.enabled;

            if (!HasDirectionalFrames)
                Debug.LogWarning($"{name} 缺少完整四向动画帧，请检查顾客预制体的上下左右绑定。", this);

            if (externallyDriven)
            {
                SetFrame(0);
                UpdateSortingOrder();
                return;
            }

            if (waypoints == null || waypoints.Length < 2)
            {
                enabled = false;
                return;
            }

            transform.position = waypoints[0].position;
            targetIndex = 1;
            pauseTimer = Random.Range(0f, pauseAtWaypoints);
        }

        private void Update()
        {
            if (externallyDriven)
            {
                UpdateSortingOrder();
                return;
            }

            if (spriteRenderer == null || waypoints == null || waypoints.Length < 2)
                return;

            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                SetFrame(0);
                UpdateSortingOrder();
                return;
            }

            Vector2 current = transform.position;
            if (waypoints[targetIndex] == null)
                return;

            Vector2 target = waypoints[targetIndex].position;
            Vector2 delta = target - current;

            if (delta.sqrMagnitude <= 0.0025f)
            {
                targetIndex = (targetIndex + 1) % waypoints.Length;
                pauseTimer = Random.Range(pauseAtWaypoints * 0.35f, pauseAtWaypoints * 1.25f);
                SetFrame(0);
                return;
            }

            Vector2 next = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            Vector2 direction = next - current;
            if (direction.sqrMagnitude > 0.0001f)
                lastDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? new Vector2(Mathf.Sign(direction.x), 0f)
                    : new Vector2(0f, Mathf.Sign(direction.y));

            AdvanceAnimation(true, Time.deltaTime);
            UpdateSortingOrder();
        }

        public void SetExternalMotion(Vector2 direction, bool moving, float deltaTime, float animationScale = 1f)
        {
            if (!externallyDriven)
                externallyDriven = true;

            if (direction.sqrMagnitude > 0.0001f)
            {
                lastDirection = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? new Vector2(Mathf.Sign(direction.x), 0f)
                    : new Vector2(0f, Mathf.Sign(direction.y));
            }

            AdvanceAnimation(moving, deltaTime * Mathf.Max(0.25f, animationScale));
            UpdateSortingOrder();
        }

        private void AdvanceAnimation(bool moving, float deltaTime)
        {
            if (!moving)
            {
                animationTimer = 0f;
                SetFrame(0);
                return;
            }

            animationTimer += deltaTime;
            SetFrame(Mathf.FloorToInt(animationTimer / 0.2f) % 2);
        }

        private void SetFrame(int frame)
        {
            Sprite[] frames;
            if (lastDirection.y < -0.5f) frames = downFrames;
            else if (lastDirection.y > 0.5f) frames = upFrames;
            else if (lastDirection.x < 0f) frames = leftFrames;
            else frames = rightFrames;

            if (frames != null && frames.Length > 0)
                spriteRenderer.sprite = frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
        }

        private void UpdateSortingOrder()
        {
            if (spriteRenderer == null)
                return;
            spriteRenderer.sortingOrder = 300 + Mathf.RoundToInt(-transform.position.y * 10f);
        }

        private static bool HasFrames(Sprite[] frames) => frames != null && frames.Length > 0;
    }
}
