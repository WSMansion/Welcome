using System.Collections.Generic;
using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// Selects changing shop and outdoor destinations, then follows paths from ShopCustomerNavigation.
    /// This lightweight behavior is intended for the open-shop scene preview.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShopVisitorWander))]
    public sealed class ShopCustomerWanderAI : MonoBehaviour
    {
        [Header("行为倾向")]
        [Range(0f, 1f), InspectorName("浏览陈列柜倾向")] public float browseChance = 0.62f;
        [Range(0f, 1f), InspectorName("靠近柜台倾向")] public float counterChance = 0.16f;
        [Range(0f, 1f), InspectorName("离店倾向")] public float leaveChance = 0.18f;
        [Range(0f, 1f), InspectorName("室外继续闲逛倾向")] public float outdoorStayChance = 0.38f;

        [Header("动作节奏")]
        [Min(0.05f), InspectorName("移动速度")] public float moveSpeed = 0.72f;
        [Min(0f), InspectorName("最短停留时间")] public float pauseMin = 0.8f;
        [Min(0f), InspectorName("最长停留时间")] public float pauseMax = 2.8f;

        [Header("场景引用")]
        [InspectorName("顾客寻路器")] public ShopCustomerNavigation navigation;
        [InspectorName("四向动画绑定")] public ShopVisitorWander animationBinding;

        private readonly List<Vector2> path = new List<Vector2>();
        private int nextPathIndex;
        private float pauseTimer;
        private Vector2 lastDirection = Vector2.down;
        private ShopDisplayCaseView reservedCase;

        private void Awake()
        {
            if (animationBinding == null)
                animationBinding = GetComponent<ShopVisitorWander>();
        }

        private void Start()
        {
            if (navigation == null)
                navigation = FindObjectOfType<ShopCustomerNavigation>();

            if (navigation == null)
            {
                Debug.LogWarning($"{name} 找不到 ShopCustomerNavigation，暂时无法自主游逛。", this);
                enabled = false;
                return;
            }

            if (!navigation.IsReady)
                navigation.RefreshNavigation();

            pauseTimer = Random.Range(0f, 0.45f);
            ChooseNextDestination();
        }

        private void Update()
        {
            if (navigation == null || animationBinding == null)
                return;

            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
                if (pauseTimer <= 0f)
                {
                    ReleaseBrowseReservation();
                    ChooseNextDestination();
                }
                return;
            }

            if (nextPathIndex >= path.Count)
            {
                BeginPause();
                return;
            }

            Vector2 current = transform.position;
            Vector2 target = path[nextPathIndex];
            Vector2 delta = target - current;
            if (delta.sqrMagnitude <= 0.0025f)
            {
                nextPathIndex++;
                if (nextPathIndex >= path.Count)
                    BeginPause();
                return;
            }

            Vector2 next = Vector2.MoveTowards(current, target, Mathf.Max(0.1f, moveSpeed) * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            lastDirection = next - current;
            animationBinding.SetExternalMotion(lastDirection, true, Time.deltaTime);
        }

        private void ChooseNextDestination()
        {
            if (navigation == null)
                return;

            bool outside = navigation.IsOutside(transform.position);
            if (outside)
            {
                if (Random.value < outdoorStayChance && navigation.TryGetRandomOutdoorPoint(out Vector2 outdoorPoint))
                {
                    SetDestinationOrPause(outdoorPoint);
                    return;
                }

                // Step through the doorway before choosing an indoor activity.
                SetDestinationOrPause(navigation.doorwayPosition);
                return;
            }

            float roll = Random.value;
            if (roll < leaveChance && navigation.TryGetRandomOutdoorPoint(out Vector2 exitPoint))
            {
                SetDestinationOrPause(exitPoint);
                return;
            }

            roll -= leaveChance;
            if (roll < browseChance && TryChooseBrowseDestination())
                return;

            roll -= browseChance;
            if (roll < counterChance)
            {
                SetDestinationOrPause(navigation.counterPosition);
                return;
            }

            if (navigation.TryGetInteriorWanderPoint(out Vector2 interiorPoint))
            {
                SetDestinationOrPause(interiorPoint);
                return;
            }

            if (TryChooseBrowseDestination())
                return;
            BeginPause();
        }

        private bool TryChooseBrowseDestination()
        {
            if (!navigation.TryGetBrowseTarget(this, out ShopDisplayCaseView displayCase))
                return false;

            reservedCase = displayCase;
            if (TrySetDestination(displayCase.BrowsingPosition))
                return true;

            ReleaseBrowseReservation();
            return false;
        }

        private bool TrySetDestination(Vector2 destination)
        {
            if (!navigation.TryFindPath(transform.position, destination, path))
            {
                path.Clear();
                nextPathIndex = 0;
                return false;
            }

            nextPathIndex = 0;
            if (path.Count == 0)
                BeginPause();
            return true;
        }

        private void SetDestinationOrPause(Vector2 destination)
        {
            if (!TrySetDestination(destination))
                BeginPause(0.4f, 0.9f);
        }

        private void BeginPause()
        {
            BeginPause(pauseMin, pauseMax);
        }

        private void BeginPause(float minimum, float maximum)
        {
            path.Clear();
            nextPathIndex = 0;
            float low = Mathf.Max(0f, Mathf.Min(minimum, maximum));
            float high = Mathf.Max(low, Mathf.Max(minimum, maximum));
            pauseTimer = Random.Range(low, high);
            if (animationBinding != null)
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
        }

        private void ReleaseBrowseReservation()
        {
            if (reservedCase == null)
                return;
            if (navigation != null)
                navigation.ReleaseBrowseTarget(this);
            reservedCase = null;
        }

        private void OnDisable()
        {
            ReleaseBrowseReservation();
        }
    }
}
