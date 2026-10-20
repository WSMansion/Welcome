using UnityEngine;
using WelCome.UI;
using System.Collections.Generic;

namespace WelCome.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ShopPlayerController : MonoBehaviour
    {
        [Header("移动")]
        [Min(0.1f), InspectorName("移动速度")] public float moveSpeed = 3f;
        [Min(1f), InspectorName("奔跑速度倍率"), Tooltip("按住左或右 Shift 时，相对普通移动速度的倍数。")]
        public float runSpeedMultiplier = 1.8f;
        [InspectorName("行走范围最小值"), Tooltip("可移动矩形范围的左下角世界坐标。")]
        public Vector2 minBounds = new Vector2(0.3f, 0.1f);
        [InspectorName("行走范围最大值"), Tooltip("可移动矩形范围的右上角世界坐标。")]
        public Vector2 maxBounds = new Vector2(31.7f, 17.2f);

        [Header("动画帧（脚本切换，无 Animator）")]
        public SpriteRenderer spriteRenderer;
        public Sprite[] downFrames;
        public Sprite[] leftFrames;
        public Sprite[] rightFrames;
        public Sprite[] upFrames;
        [InspectorName("反转四向图片"), Tooltip("仅当素材命名与实际朝向相反时开启。")]
        public bool reverseFacingSprites;
        [Min(0.05f), InspectorName("动画帧时长")] public float frameDuration = 0.18f;

        [Header("交互")]
        [Min(0.1f), InspectorName("陈列柜交互距离")] public float displayCaseInteractRange = 1.6f;
        [Min(0f), InspectorName("交互提示与柜子间距")] public float interactionPromptGap = 0.08f;
        [SerializeField, InspectorName("交互提示预制体")]
        private WorldInteractionPrompt interactionPromptPrefab;

        private Rigidbody2D body;
        private Vector2 moveInput;
        private Vector2 facing = Vector2.down;
        private float animationTimer;
        private bool runHeld;
        private bool interactionPanelOpen;
        private ShopDisplayCaseView nearbyDisplayCase;
        private WorldInteractionPrompt interactionPrompt;
        private float nextDisplayCaseScanTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            interactionPrompt = GetComponentInChildren<WorldInteractionPrompt>(true);
            if (interactionPrompt == null)
            {
                if (interactionPromptPrefab != null)
                {
                    interactionPrompt = Instantiate(interactionPromptPrefab, transform, false);
                    interactionPrompt.gameObject.name = "交互提示（水滴）";
                }
                else
                {
                    Debug.LogError(
                        "[ShopPlayerController] 主控角色未指定交互提示预制体：Assets/Prefabs/OpenShop/InteractionPrompt.prefab",
                        this);
                }
            }
            interactionPrompt?.Hide();
            SetFrame(0);
        }

        private void Update()
        {
            if (interactionPanelOpen)
            {
                interactionPrompt?.Hide();
                moveInput = Vector2.zero;
                runHeld = false;
                animationTimer = 0f;
                SetFrame(0);
                return;
            }

            RefreshNearbyDisplayCase();
            ShopCustomerWanderAI payingCustomer = FindPayingCustomer();
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (payingCustomer != null && payingCustomer.TryCollectPayment())
                    return;
                if (nearbyDisplayCase != null)
                    OpenDisplayCasePanel();
            }

            runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            moveInput = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) moveInput.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) moveInput.x += 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) moveInput.y -= 1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) moveInput.y += 1f;
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            if (moveInput.sqrMagnitude > 0.001f)
            {
                facing = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
                    ? new Vector2(Mathf.Sign(moveInput.x), 0f)
                    : new Vector2(0f, Mathf.Sign(moveInput.y));
                animationTimer += Time.deltaTime;
                SetFrame(Mathf.FloorToInt(animationTimer / frameDuration) % 2);
            }
            else
            {
                animationTimer = 0f;
                SetFrame(0);
            }

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 300 + Mathf.RoundToInt(-transform.position.y * 10f);
        }

        private void LateUpdate()
        {
            ShopCustomerWanderAI payingCustomer = FindPayingCustomer();
            if (interactionPrompt == null || interactionPanelOpen || (nearbyDisplayCase == null && payingCustomer == null))
            {
                interactionPrompt?.Hide();
                return;
            }
            if (payingCustomer != null)
            {
                interactionPrompt.ShowAt(payingCustomer.PaymentAnchor, payingCustomer.GetComponentInChildren<SpriteRenderer>());
                return;
            }

            Bounds targetBounds = nearbyDisplayCase.spriteRenderer != null
                ? nearbyDisplayCase.spriteRenderer.bounds
                : nearbyDisplayCase.blockingCollider != null
                    ? nearbyDisplayCase.blockingCollider.bounds
                    : new Bounds(nearbyDisplayCase.transform.position, Vector3.one);
            Vector3 promptPosition = new Vector3(
                targetBounds.center.x,
                targetBounds.max.y + interactionPromptGap,
                nearbyDisplayCase.transform.position.z - 0.1f);
            interactionPrompt.ShowAt(promptPosition, nearbyDisplayCase.spriteRenderer);
        }

        private void FixedUpdate()
        {
            if (body == null)
                return;
            if (interactionPanelOpen)
            {
                body.velocity = Vector2.zero;
                return;
            }

            float currentMoveSpeed = runHeld
                ? moveSpeed * Mathf.Max(1f, runSpeedMultiplier)
                : moveSpeed;
            Vector2 velocity = moveInput * currentMoveSpeed;
            Vector2 position = body.position;
            if ((position.x <= minBounds.x && velocity.x < 0f)
                || (position.x >= maxBounds.x && velocity.x > 0f))
                velocity.x = 0f;
            if ((position.y <= minBounds.y && velocity.y < 0f)
                || (position.y >= maxBounds.y && velocity.y > 0f))
                velocity.y = 0f;

            body.velocity = velocity;
        }

        public void SetInteractionPanelOpen(bool isOpen)
        {
            interactionPanelOpen = isOpen;
            if (isOpen)
            {
                interactionPrompt?.Hide();
                moveInput = Vector2.zero;
                runHeld = false;
                if (body != null)
                    body.velocity = Vector2.zero;
            }
        }

        private void RefreshNearbyDisplayCase()
        {
            if (Time.unscaledTime < nextDisplayCaseScanTime)
                return;

            nextDisplayCaseScanTime = Time.unscaledTime + 0.2f;
            ShopDisplayCaseView closestCase = null;
            float closestDistance = displayCaseInteractRange;
            Vector2 playerPosition = body != null ? body.position : (Vector2)transform.position;

            foreach (ShopDisplayCaseView candidate in FindObjectsOfType<ShopDisplayCaseView>())
            {
                if (candidate == null || !candidate.isActiveAndEnabled)
                    continue;

                Vector2 interactionPoint = candidate.blockingCollider != null
                    ? candidate.blockingCollider.ClosestPoint(playerPosition)
                    : (Vector2)candidate.transform.position;
                float distance = Vector2.Distance(playerPosition, interactionPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCase = candidate;
                }
            }

            nearbyDisplayCase = closestCase;
        }

        private ShopCustomerWanderAI FindPayingCustomer()
        {
            Vector2 playerPosition = body != null ? body.position : (Vector2)transform.position;
            ShopCustomerWanderAI closest = null;
            float closestDistance = displayCaseInteractRange;
            ShopCustomerWanderAI[] customers = FindObjectsOfType<ShopCustomerWanderAI>();
            for (int i = 0; i < customers.Length; i++)
            {
                ShopCustomerWanderAI customer = customers[i];
                if (customer == null || !customer.CanCollectPayment)
                    continue;
                float distance = Vector2.Distance(playerPosition, customer.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = customer;
                }
            }
            return closest;
        }

        private void OpenDisplayCasePanel()
        {
            UIManager uiManager = UIManager.Instance;
            DisplayCaseStockPanel panel = uiManager != null
                ? uiManager.GetPanel<DisplayCaseStockPanel>()
                : null;
            if (panel == null)
                return;

            panel.SetContext(nearbyDisplayCase, this);
            uiManager.ShowOverlay<DisplayCaseStockPanel>();
        }

        private void OnDisable()
        {
            interactionPrompt?.Hide();
        }

        private void SetFrame(int frame)
        {
            if (spriteRenderer == null)
                return;

            Vector2 spriteFacing = reverseFacingSprites ? -facing : facing;
            Sprite[] frames;
            if (spriteFacing.y < -0.5f) frames = downFrames;
            else if (spriteFacing.y > 0.5f) frames = upFrames;
            else if (spriteFacing.x < 0f) frames = leftFrames;
            else frames = rightFrames;

            if (frames != null && frames.Length > 0)
                spriteRenderer.sprite = frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 min = Vector2.Min(minBounds, maxBounds);
            Vector2 max = Vector2.Max(minBounds, maxBounds);
            Gizmos.color = new Color(0.25f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireCube(
                new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f),
                new Vector3(max.x - min.x, max.y - min.y, 0f));
        }
    }
}
