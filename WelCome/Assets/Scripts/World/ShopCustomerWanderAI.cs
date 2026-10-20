using System.Collections.Generic;
using UnityEngine;
using WelCome.Data;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.World
{
    /// <summary>
    /// Runs one visitor from Luban profile data: walking, noticing the shop, browsing, and social pauses.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ShopVisitorWander))]
    public sealed class ShopCustomerWanderAI : MonoBehaviour
    {
        [Header("人物行为档案")]
        [Min(1), InspectorName("Luban 行为档案 ID")] public int behaviorProfileId = 1001;

        [Header("场景引用")]
        [InspectorName("顾客寻路器")] public ShopCustomerNavigation navigation;
        [InspectorName("四向动画绑定")] public ShopVisitorWander animationBinding;
        [SerializeField, InspectorName("气泡挂点")]
        private Transform speechBubbleAnchor;

        // Kept only so existing scene overrides remain readable while behavior values move to Luban.
        [SerializeField, HideInInspector] private bool outdoorOnly;
        [SerializeField, HideInInspector] private float browseChance;
        [SerializeField, HideInInspector] private float counterChance;
        [SerializeField, HideInInspector] private float leaveChance;
        [SerializeField, HideInInspector] private float outdoorStayChance;
        [SerializeField, HideInInspector] private float moveSpeed;
        [SerializeField, HideInInspector] private float pauseMin;
        [SerializeField, HideInInspector] private float pauseMax;

        private readonly List<Vector2> path = new List<Vector2>();
        private readonly HashSet<int> inspectedEmptyDisplayIds = new HashSet<int>();
        private int nextPathIndex;
        private int consecutiveEmptyDisplayCount;
        private float pauseTimer;
        private Vector2 lastDirection = Vector2.down;
        private ShopDisplayCaseView reservedCase;
        private cfg.ShopNpcProfile profile;
        private cfg.ShopBehavior behaviorSettings;
        private ShopCustomerSocialCoordinator socialCoordinator;
        private bool noticedShopThisPass;
        private bool awaitingNoticeDecision;
        private bool socializing;
        private bool socialGreeting;
        private bool browseThoughtBubbleShown;
        private bool showingUnhappyBrowseOutcome;
        private float socialTimer;
        private float greetingSymbolTimer;
        private float farewellTimer;
        private float pendingSocialDuration;
        private float socialApproachSpeedMultiplier = 1f;
        private float socialApproachMaxSeconds;
        private float socialApproachElapsed;
        private bool socialApproaching;
        private bool socialApproachReached;
        private float socialCooldownUntil;
        private ShopCustomerWanderAI socialPartner;
        [SerializeField, InspectorName("头顶内心文字")]
        private TextMesh thoughtText;
        [SerializeField, InspectorName("状态气泡")]
        private ShopCustomerSpeechBubble interactionBubble;
        private float indicatorTimer;
        private PurchasePhase purchasePhase;
        private bool leaveAfterRejection;
        private float offerDuration;
        private Vector2 streetPassThroughDestination;
        private bool hasStreetPassThroughDestination;
        private bool streetCustomerInsideShop;
        private bool streetDepartureStarted;
        private bool forceLeaveRequested;
        private float streetSpeedMultiplier = 1f;
        private int streetLaneIndex = -1;
        private int streetDirection = 1;

        public cfg.ShopNpcProfile Profile => profile;
        public float Sociability => profile == null ? 0f : Mathf.Clamp01(profile.Sociability / 100f);
        public bool IsLeavingShop => streetDepartureStarted || forceLeaveRequested;
        public bool IsPasserbyOnStreet => profile != null && profile.PassThrough
            && hasStreetPassThroughDestination && !streetCustomerInsideShop
            && navigation != null && navigation.IsOutside(transform.position);
        public int StreetLaneIndex => streetLaneIndex;
        public int StreetDirection => streetDirection;
        public bool IsPausedAtDestination => pauseTimer > 0f && !awaitingNoticeDecision && !socializing && purchasePhase == PurchasePhase.None;
        public bool HasInspectedEmptyDisplay(ShopDisplayCaseView displayCase) =>
            displayCase != null && inspectedEmptyDisplayIds.Contains(displayCase.GetInstanceID());
        private bool ShouldContinueStreetRoute => hasStreetPassThroughDestination
            && profile != null
            && (profile.PassThrough
                ? !streetCustomerInsideShop
                : streetDepartureStarted && navigation != null
                    && navigation.IsOutside(transform.position) && nextPathIndex >= path.Count);

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
                Debug.LogWarning(name + " 找不到 ShopCustomerNavigation，暂时无法自主游逛。", this);
                enabled = false;
                return;
            }

            if (!navigation.IsReady)
                navigation.RefreshNavigation();

            ConfigManager configManager = ConfigManager.Instance;
            if (!configManager.IsLoaded)
                configManager.LoadAllConfigs();
            if (!configManager.IsLoaded)
            {
                Debug.LogError("顾客行为配置未加载，已停用 " + name + "。", this);
                enabled = false;
                return;
            }

            profile = configManager.Tables.TbShopNpcProfile.GetOrDefault(behaviorProfileId);
            behaviorSettings = configManager.Tables.TbShopBehavior.Data;
            if (profile == null || behaviorSettings == null)
            {
                Debug.LogError("找不到顾客行为档案 ID " + behaviorProfileId + "，已停用 " + name + "。", this);
                enabled = false;
                return;
            }

            socialCoordinator = navigation.GetComponent<ShopCustomerSocialCoordinator>();
            if (socialCoordinator == null)
                socialCoordinator = navigation.gameObject.AddComponent<ShopCustomerSocialCoordinator>();
            socialCoordinator.Initialize(behaviorSettings);
            socialCoordinator.Register(this);

            if (profile.PassThrough)
            {
                if (!hasStreetPassThroughDestination)
                {
                    streetPassThroughDestination = new Vector2(navigation.navigationMax.x + 6f, transform.position.y);
                    hasStreetPassThroughDestination = true;
                }
                lastDirection = Vector2.right;
                return;
            }

            pauseTimer = Random.Range(0f, 0.45f);
            ChooseNextDestination();
        }

        private void Update()
        {
            if (navigation == null || animationBinding == null || profile == null)
                return;

            if (ShouldContinueStreetRoute)
            {
                UpdateStreetPassThrough(Time.deltaTime, profile.PassThrough && !streetCustomerInsideShop);
                return;
            }

            if (forceLeaveRequested && !socializing && !awaitingNoticeDecision
                && TryStartForcedDeparture())
                return;

            UpdateInteractionIndicator(Time.deltaTime);
            UpdateThoughtSorting();
            if (socializing)
            {
                UpdateSocialInteraction();
                return;
            }

            if (farewellTimer > 0f)
            {
                farewellTimer -= Time.deltaTime;
                FacePartner(null);
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
                UpdateInteractionIndicator(Time.deltaTime);
                if (farewellTimer <= 0f)
                {
                    ClearInteractionIndicator();
                    ChooseNextDestination();
                }
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                if (purchasePhase == PurchasePhase.Offering)
                    UpdatePaymentCountdown();
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
                if (pauseTimer <= 0f)
                    CompletePause();
                return;
            }

            if (TryStartShopNotice())
                return;

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
                    HandlePathDestinationReached();
                return;
            }

            float hurry = IsHeadingToStockedShelf ? 1.5f : 1f;
            Vector2 next = Vector2.MoveTowards(current, target, Mathf.Max(0.1f, profile.MoveSpeed) * hurry * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            lastDirection = next - current;
            animationBinding.SetExternalMotion(lastDirection, true, Time.deltaTime, IsHeadingToStockedShelf ? 1.5f : 1f);
        }

        private bool IsHeadingToStockedShelf
        {
            get
            {
                if (reservedCase == null || DisplayCaseSystem.Instance == null)
                    return false;
                return DisplayCaseSystem.Instance.GetDisplayedProduct(reservedCase.ConfigId, 0) != null;
            }
        }

        private void HandlePathDestinationReached()
        {
            if (streetDepartureStarted && navigation.IsOutside(transform.position))
            {
                streetCustomerInsideShop = false;
                path.Clear();
                nextPathIndex = 0;
                ClearInteractionIndicator();
                ReleaseBrowseReservation();
                return;
            }

            BeginPause();
        }

        private bool TryStartShopNotice()
        {
            Vector2 current = transform.position;
            if (!navigation.IsOutside(current))
            {
                noticedShopThisPass = false;
                return false;
            }

            float distanceToDoor = Vector2.Distance(current, navigation.doorwayPosition);
            if (distanceToDoor > behaviorSettings.NoticeRadius + behaviorSettings.NoticeResetMargin)
                noticedShopThisPass = false;
            if (noticedShopThisPass || distanceToDoor > behaviorSettings.NoticeRadius)
                return false;

            noticedShopThisPass = true;
            if (Random.Range(0, 100) >= Mathf.Clamp(profile.NoticeChance, 0, 100))
                return false;

            path.Clear();
            nextPathIndex = 0;
            awaitingNoticeDecision = true;
            pauseTimer = Random.Range(
                Mathf.Max(0f, behaviorSettings.GlanceMinSeconds),
                Mathf.Max(behaviorSettings.GlanceMinSeconds, behaviorSettings.GlanceMaxSeconds));
            lastDirection = navigation.doorwayPosition - current;
            ClearInteractionIndicator();
            if (RollChance(behaviorSettings.NoticeThoughtBubbleChance))
                ShowThoughtIndicator(pauseTimer);
            animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
            return true;
        }

        private void CompletePause()
        {
            pauseTimer = 0f;
            if (awaitingNoticeDecision)
            {
                awaitingNoticeDecision = false;
                ClearInteractionIndicator();
                if (ShouldEnterShop())
                {
                    if (profile.PassThrough && hasStreetPassThroughDestination)
                    {
                        if (TrySetDestination(navigation.doorwayPosition))
                        {
                            streetCustomerInsideShop = true;
                            return;
                        }

                        ResumeStreetPassThrough();
                        return;
                    }

                    SetDestinationOrPause(navigation.doorwayPosition);
                    return;
                }

                ChooseNextDestination();
                return;
            }

            if (showingUnhappyBrowseOutcome)
            {
                showingUnhappyBrowseOutcome = false;
                ClearInteractionIndicator();
                ReleaseBrowseReservation();
                ChooseNextDestination();
                return;
            }

            if (purchasePhase != PurchasePhase.None)
            {
                if (ContinuePurchase())
                    return;
            }

            ClearInteractionIndicator();
            ReleaseBrowseReservation();
            ChooseNextDestination();
        }

        private bool ShouldEnterShop()
        {
            if (profile.OutdoorOnly)
                return false;

            float curiosity = Mathf.Clamp01(profile.Curiosity / 100f);
            float hurry = Mathf.Clamp01(profile.Hurry / 100f);
            float hurryPenalty = Mathf.Clamp01(behaviorSettings.HurryEntryPenalty / 100f);
            float displayBonus = Mathf.Max(0, navigation.CountDisplayedProducts())
                * Mathf.Max(0, behaviorSettings.DisplayBonusPerItem) / 100f;
            float chance = profile.EnterChance / 100f
                * curiosity
                * (1f + displayBonus)
                * (1f - hurry * hurryPenalty);
            return Random.value < Mathf.Clamp01(chance);
        }

        private void ChooseNextDestination()
        {
            if (navigation == null || profile == null)
                return;

            bool resumingStreetRoute = (profile.PassThrough && !streetCustomerInsideShop)
                || (streetDepartureStarted && navigation.IsOutside(transform.position)
                    && nextPathIndex >= path.Count);
            if (hasStreetPassThroughDestination && resumingStreetRoute)
            {
                ResumeStreetPassThrough();
                return;
            }

            if (forceLeaveRequested && TryStartForcedDeparture())
                return;

            bool outside = navigation.IsOutside(transform.position);
            if (profile.OutdoorOnly)
            {
                if (navigation.TryGetRandomOutdoorPoint(out Vector2 passerbyPoint))
                {
                    if (outside)
                        SetDestinationOrPause(passerbyPoint);
                    else
                        SetDestinationThroughDoorwayOrPause(passerbyPoint);
                }
                else
                {
                    BeginPause();
                }
                return;
            }

            if (outside)
            {
                if (navigation.TryGetRandomOutdoorPoint(out Vector2 outdoorPoint))
                    SetDestinationOrPause(outdoorPoint);
                else
                    BeginPause();
                return;
            }

            ChooseIndoorDestination();
        }

        private void ChooseIndoorDestination()
        {
            List<DestinationCandidate> candidates = new List<DestinationCandidate>(4);
            ShopDisplayCaseView browseTarget = null;
            if (profile.BrowseWeight > 0 && navigation.TryGetBrowseTarget(this, out browseTarget))
            {
                reservedCase = browseTarget;
                bool stockedShelf = DisplayCaseSystem.Instance != null
                    && DisplayCaseSystem.Instance.GetDisplayedProduct(browseTarget.ConfigId, 0) != null;
                if (stockedShelf)
                {
                    if (!TrySetDestination(browseTarget.BrowsingPosition))
                    {
                        ReleaseBrowseReservation();
                        BeginPause(0.4f, 0.9f);
                    }
                    return;
                }

                candidates.Add(new DestinationCandidate(DestinationKind.Browse, profile.BrowseWeight,
                    browseTarget.BrowsingPosition));
            }

            AddCandidateIfReachable(candidates, DestinationKind.Counter, profile.CounterWeight,
                navigation.counterPosition, false);

            Vector2 outdoorExit;
            bool hasOutdoorExit = hasStreetPassThroughDestination
                ? TryGetStreetDeparturePoint(out outdoorExit)
                : navigation.TryGetRandomOutdoorPoint(out outdoorExit);
            if (profile.LeaveWeight > 0 && hasOutdoorExit && HasPathThroughDoorway(outdoorExit))
            {
                candidates.Add(new DestinationCandidate(DestinationKind.Leave, profile.LeaveWeight, outdoorExit));
            }

            if (profile.WanderWeight > 0
                && navigation.TryGetInteriorWanderPoint(out Vector2 interiorPoint)
                && HasPathTo(interiorPoint))
            {
                candidates.Add(new DestinationCandidate(DestinationKind.Wander, profile.WanderWeight, interiorPoint));
            }

            int totalWeight = 0;
            foreach (DestinationCandidate candidate in candidates)
                totalWeight += candidate.Weight;

            if (totalWeight <= 0)
            {
                ReleaseBrowseReservation();
                BeginPause();
                return;
            }

            int roll = Random.Range(0, totalWeight);
            DestinationCandidate selected = candidates[candidates.Count - 1];
            foreach (DestinationCandidate candidate in candidates)
            {
                roll -= candidate.Weight;
                if (roll < 0)
                {
                    selected = candidate;
                    break;
                }
            }

            if (selected.Kind == DestinationKind.Browse)
            {
                if (!TrySetDestination(selected.Position))
                {
                    ReleaseBrowseReservation();
                    BeginPause(0.4f, 0.9f);
                }
                return;
            }

            ReleaseBrowseReservation();
            if (selected.Kind == DestinationKind.Leave)
            {
                if (hasStreetPassThroughDestination)
                {
                    if (TrySetDestinationThroughDoorway(selected.Position))
                        streetDepartureStarted = true;
                    else
                        BeginPause(0.4f, 0.9f);
                }
                else
                {
                    SetDestinationThroughDoorwayOrPause(selected.Position);
                }
            }
            else
                SetDestinationOrPause(selected.Position);
        }

        private bool TryGetStreetDeparturePoint(out Vector2 point)
        {
            float direction = streetDirection >= 0 ? 1f : -1f;
            point = new Vector2(navigation.doorwayPosition.x + direction * 0.75f, streetPassThroughDestination.y);
            return navigation.IsOutside(point);
        }

        private bool TryStartForcedDeparture()
        {
            if (!forceLeaveRequested || navigation == null || profile == null)
                return false;

            ReleaseBrowseReservation();
            ClearInteractionIndicator();
            if (navigation.IsOutside(transform.position))
            {
                forceLeaveRequested = false;
                streetDepartureStarted = true;
                ResumeStreetPassThrough();
                return true;
            }

            if (!hasStreetPassThroughDestination)
            {
                float streetY = (navigation.outdoorMin.y + navigation.outdoorMax.y) * 0.5f;
                ConfigureStreetDeparture(new Vector2(navigation.navigationMax.x + 6f, streetY));
            }

            if (!TryGetStreetDeparturePoint(out Vector2 outdoorExit)
                || !TrySetDestinationThroughDoorway(outdoorExit))
                return false;

            forceLeaveRequested = false;
            streetDepartureStarted = true;
            return true;
        }

        private void AddCandidateIfReachable(List<DestinationCandidate> candidates, DestinationKind kind,
            int weight, Vector2 position, bool throughDoorway)
        {
            if (weight <= 0)
                return;

            bool reachable = throughDoorway ? HasPathThroughDoorway(position) : HasPathTo(position);
            if (reachable)
                candidates.Add(new DestinationCandidate(kind, weight, position));
        }

        private bool HasPathTo(Vector2 destination)
        {
            return navigation.TryFindPath(transform.position, destination, new List<Vector2>());
        }

        private bool HasPathThroughDoorway(Vector2 outdoorDestination)
        {
            List<Vector2> firstLeg = new List<Vector2>();
            List<Vector2> secondLeg = new List<Vector2>();
            return navigation.TryFindPath(transform.position, navigation.doorwayPosition, firstLeg)
                && navigation.TryFindPath(navigation.doorwayPosition, outdoorDestination, secondLeg);
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
            // Street-to-street movement stays outdoors so a passerby cannot cut through the shop.
            bool staysOutdoors = navigation.IsOutside(transform.position) && navigation.IsOutside(destination);
            bool foundPath = staysOutdoors
                ? navigation.TryFindOutdoorPath(transform.position, destination, path)
                : navigation.TryFindPath(transform.position, destination, path);
            if (!foundPath)
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

        private bool TrySetDestinationThroughDoorway(Vector2 destination)
        {
            List<Vector2> firstLeg = new List<Vector2>();
            List<Vector2> secondLeg = new List<Vector2>();
            if (!navigation.TryFindPath(transform.position, navigation.doorwayPosition, firstLeg)
                || !navigation.TryFindPath(navigation.doorwayPosition, destination, secondLeg))
                return false;

            path.Clear();
            AppendPath(firstLeg);
            AppendPath(secondLeg);
            nextPathIndex = 0;
            if (path.Count == 0)
                BeginPause();
            return true;
        }

        private void AppendPath(List<Vector2> segment)
        {
            foreach (Vector2 point in segment)
            {
                if (path.Count == 0 || (path[path.Count - 1] - point).sqrMagnitude > 0.01f)
                    path.Add(point);
            }
        }

        private void SetDestinationThroughDoorwayOrPause(Vector2 destination)
        {
            if (!TrySetDestinationThroughDoorway(destination))
                BeginPause(0.4f, 0.9f);
        }

        private void SetDestinationOrPause(Vector2 destination)
        {
            if (!TrySetDestination(destination))
                BeginPause(0.4f, 0.9f);
        }

        private void BeginPause()
        {
            BeginPause(profile.PauseMin, profile.PauseMax);
        }

        private void BeginPause(float minimum, float maximum)
        {
            path.Clear();
            nextPathIndex = 0;
            float low = Mathf.Max(0f, Mathf.Min(minimum, maximum));
            float high = Mathf.Max(low, Mathf.Max(minimum, maximum));
            pauseTimer = Random.Range(low, high);
            browseThoughtBubbleShown = false;
            if (reservedCase != null)
                BeginBrowseLook();
            if (animationBinding != null)
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
        }

        private bool ShouldShowUnhappyBrowseOutcome()
        {
            if (reservedCase == null || profile == null || profile.PreferredNeedTag <= 0)
                return false;

            DisplayCaseSystem displayCaseSystem = DisplayCaseSystem.Instance;
            if (displayCaseSystem == null)
                return false;

            ProductInstance displayedProduct = displayCaseSystem.GetDisplayedProduct(reservedCase.ConfigId, 0);
            return displayedProduct != null
                && displayedProduct.NeedTag != profile.PreferredNeedTag
                && RollChance(behaviorSettings.UnhappyBubbleChance);
        }

        private bool TryHandleEmptyDisplayBrowse()
        {
            if (reservedCase == null)
                return false;

            DisplayCaseSystem displayCaseSystem = DisplayCaseSystem.Instance;
            if (displayCaseSystem == null)
                return false;

            ProductInstance displayedProduct = displayCaseSystem.GetDisplayedProduct(reservedCase.ConfigId, 0);
            if (displayedProduct != null)
            {
                consecutiveEmptyDisplayCount = 0;
                inspectedEmptyDisplayIds.Clear();
                return false;
            }

            inspectedEmptyDisplayIds.Add(reservedCase.GetInstanceID());
            consecutiveEmptyDisplayCount++;
            ClearInteractionIndicator();
            ReleaseBrowseReservation();

            if (consecutiveEmptyDisplayCount >= Mathf.Max(1, behaviorSettings.EmptyDisplayLeaveThreshold))
            {
                LeaveAfterEmptyDisplays();
                return true;
            }

            if (TryChooseBrowseDestination())
                return true;

            ChooseNextDestination();
            return true;
        }

        private void LeaveAfterEmptyDisplays()
        {
            if (!hasStreetPassThroughDestination)
            {
                float streetY = (navigation.outdoorMin.y + navigation.outdoorMax.y) * 0.5f;
                ConfigureStreetDeparture(new Vector2(navigation.navigationMax.x + 6f, streetY));
            }

            ClearInteractionIndicator();
            ReleaseBrowseReservation();
            inspectedEmptyDisplayIds.Clear();
            consecutiveEmptyDisplayCount = 0;
            ShowEmptyDisplayIndicator(behaviorSettings.EmptyDisplayBubbleSeconds);

            if (TryGetStreetDeparturePoint(out Vector2 outdoorExit)
                && TrySetDestinationThroughDoorway(outdoorExit))
            {
                streetDepartureStarted = true;
                return;
            }

            Debug.LogWarning("顾客连续遇到空展台后找不到离店路线。", this);
            ChooseNextDestination();
        }

        private static bool RollChance(int chancePercent)
        {
            return Random.Range(0, 100) < Mathf.Clamp(chancePercent, 0, 100);
        }

        public void ConfigureStreetPassThrough(Vector2 destination, float speedMultiplier = 1f, int laneIndex = -1)
        {
            ConfigureStreetRoute(destination, speedMultiplier, laneIndex);
        }

        public void ConfigureStreetDeparture(Vector2 destination, float speedMultiplier = 1f, int laneIndex = -1)
        {
            ConfigureStreetRoute(destination, speedMultiplier, laneIndex);
        }

        private void ConfigureStreetRoute(Vector2 destination, float speedMultiplier, int laneIndex)
        {
            streetPassThroughDestination = destination;
            hasStreetPassThroughDestination = true;
            streetSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            streetLaneIndex = laneIndex;
            float horizontalDelta = destination.x - transform.position.x;
            if (Mathf.Abs(horizontalDelta) > 0.02f)
                streetDirection = horizontalDelta >= 0f ? 1 : -1;
        }

        public bool RequestLeaveFromShop()
        {
            if (profile == null || profile.OutdoorOnly || streetDepartureStarted || forceLeaveRequested)
                return false;

            forceLeaveRequested = true;
            if (!socializing && !awaitingNoticeDecision)
                TryStartForcedDeparture();
            return true;
        }

        private void MatchTravelSpeed(ShopCustomerWanderAI partner)
        {
            float ownSpeed = Mathf.Max(0.1f, profile.MoveSpeed) * streetSpeedMultiplier;
            float partnerSpeed = Mathf.Max(0.1f, partner.profile.MoveSpeed) * partner.streetSpeedMultiplier;
            float sharedSpeed = (ownSpeed + partnerSpeed) * 0.5f;
            streetSpeedMultiplier = sharedSpeed / Mathf.Max(0.1f, profile.MoveSpeed);
        }

        private void ResumeStreetPassThrough()
        {
            path.Clear();
            nextPathIndex = 0;
            pauseTimer = 0f;
            lastDirection = new Vector2(streetDirection, 0f);
        }

        private void UpdateStreetPassThrough(float deltaTime, bool allowShopNotice)
        {
            UpdateInteractionIndicator(deltaTime);
            if (socializing)
            {
                UpdateSocialInteraction();
                return;
            }

            if (pauseTimer > 0f)
            {
                pauseTimer -= deltaTime;
                animationBinding.SetExternalMotion(lastDirection, false, deltaTime);
                if (pauseTimer <= 0f)
                    CompletePause();
                return;
            }

            if (allowShopNotice && TryStartShopNotice())
                return;

            Vector2 current = transform.position;
            bool arrived = streetDirection > 0
                ? current.x >= streetPassThroughDestination.x - 0.02f
                : current.x <= streetPassThroughDestination.x + 0.02f;
            if (arrived)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 next = Vector2.MoveTowards(current, streetPassThroughDestination,
                Mathf.Max(0.1f, profile.MoveSpeed) * streetSpeedMultiplier * deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            lastDirection = next - current;
            animationBinding.SetExternalMotion(lastDirection, true, deltaTime);
        }

        private bool CanJoinSocial
        {
            get
            {
                return enabled && profile != null && navigation != null && !socializing
                    && !awaitingNoticeDecision && reservedCase == null && Sociability > 0f
                    && !streetDepartureStarted && !forceLeaveRequested
                    && Time.time >= socialCooldownUntil;
            }
        }

        public bool IsAtSafeSocialPosition(float keepoutRadius)
        {
            if (!CanJoinSocial)
                return false;

            Vector2 position = transform.position;
            if (navigation.IsOutside(position))
                return Vector2.Distance(position, navigation.doorwayPosition) > keepoutRadius;

            return !profile.OutdoorOnly
                && Vector2.Distance(position, navigation.doorwayPosition) > keepoutRadius
                && Vector2.Distance(position, navigation.counterPosition) > keepoutRadius;
        }

        public bool IsInSameSocialArea(ShopCustomerWanderAI other)
        {
            return other != null && navigation != null && other.navigation == navigation
                && navigation.IsOutside(transform.position) == navigation.IsOutside(other.transform.position);
        }

        public bool CanReachSocialMeetingPoint(Vector2 point, float keepoutRadius)
        {
            if (!CanJoinSocial || navigation == null || !IsSafeSocialMeetingPoint(point, keepoutRadius))
                return false;

            List<Vector2> testPath = new List<Vector2>();
            return navigation.IsOutside(transform.position)
                ? navigation.TryFindOutdoorPath(transform.position, point, testPath)
                : navigation.TryFindPath(transform.position, point, testPath);
        }

        private bool IsSafeSocialMeetingPoint(Vector2 point, float keepoutRadius)
        {
            bool currentlyOutside = navigation.IsOutside(transform.position);
            if (currentlyOutside != navigation.IsOutside(point)
                || Vector2.Distance(point, navigation.doorwayPosition) <= keepoutRadius)
                return false;

            return currentlyOutside
                || Vector2.Distance(point, navigation.counterPosition) > keepoutRadius;
        }

        public bool IsAcquaintedWith(ShopCustomerWanderAI other)
        {
            return other != null && profile != null && other.profile != null
                && profile.SocialGroupId > 0 && profile.SocialGroupId == other.profile.SocialGroupId;
        }

        public bool TryStartSocialInteraction(ShopCustomerWanderAI partner, float duration, bool greeting,
            bool moodHappy, Vector2 meetingPoint, bool approachBeforeChat, float approachSpeedMultiplier,
            float approachMaxSeconds)
        {
            if (partner == null || !CanJoinSocial)
                return false;

            path.Clear();
            nextPathIndex = 0;
            socialApproaching = approachBeforeChat
                && Vector2.Distance(transform.position, meetingPoint) > 0.05f;
            socialApproachReached = false;
            if (socialApproaching)
            {
                bool foundPath = navigation.IsOutside(transform.position)
                    ? navigation.TryFindOutdoorPath(transform.position, meetingPoint, path)
                    : navigation.TryFindPath(transform.position, meetingPoint, path);
                if (!foundPath)
                {
                    path.Clear();
                    socialApproaching = false;
                    return false;
                }
                socialApproaching = path.Count > 0;
            }

            socialPartner = partner;
            socializing = true;
            socialGreeting = greeting;
            pendingSocialDuration = socialGreeting
                ? SocialReactionTotalSeconds
                : Mathf.Max(0.2f, duration);
            socialTimer = socialApproaching ? 0f : pendingSocialDuration;
            greetingSymbolTimer = 0f;
            socialApproachSpeedMultiplier = Mathf.Max(1f, approachSpeedMultiplier);
            socialApproachMaxSeconds = Mathf.Max(0.2f, approachMaxSeconds);
            socialApproachElapsed = 0f;
            socialCooldownUntil = Time.time + pendingSocialDuration + socialApproachMaxSeconds
                + Mathf.Max(0f, behaviorSettings.SocialCooldownSeconds);
            pauseTimer = 0f;
            FacePartner(partner);
            if (socialApproaching)
                ShowSocialIndicator(socialGreeting, socialApproachMaxSeconds);
            else
                BeginSocialReaction();
            return true;
        }

        private void UpdateSocialInteraction()
        {
            if (socialPartner == null || !socialPartner.isActiveAndEnabled)
            {
                FinishSocialInteraction();
                return;
            }

            if (socialApproaching)
            {
                UpdateSocialApproach();
                return;
            }

            if (socialPartner.socialApproaching)
            {
                FacePartner(socialPartner);
                animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
                return;
            }

            FacePartner(socialPartner);
            animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
            if (socialGreeting)
            {
                greetingSymbolTimer -= Time.deltaTime;
                if (greetingSymbolTimer > 0f)
                    return;

                socialTimer -= SocialReactionSeconds;
                if (socialTimer > 0.05f)
                {
                    ShowNextSocialReaction();
                    return;
                }
            }
            else
            {
                socialTimer -= Time.deltaTime;
                if (socialTimer > 0f)
                    return;
            }

            FinishSocialInteraction();
        }

        private void UpdateSocialApproach()
        {
            if (!socialApproachReached)
            {
                socialApproachElapsed += Time.deltaTime;
                if (nextPathIndex < path.Count && socialApproachElapsed < socialApproachMaxSeconds)
                {
                    Vector2 current = transform.position;
                    Vector2 target = path[nextPathIndex];
                    Vector2 delta = target - current;
                    if (delta.sqrMagnitude <= 0.0025f)
                    {
                        nextPathIndex++;
                    }
                    else
                    {
                        Vector2 next = Vector2.MoveTowards(current, target,
                            Mathf.Max(0.1f, profile.MoveSpeed) * socialApproachSpeedMultiplier * Time.deltaTime);
                        transform.position = new Vector3(next.x, next.y, transform.position.z);
                        lastDirection = next - current;
                        animationBinding.SetExternalMotion(lastDirection, true, Time.deltaTime, IsHeadingToStockedShelf ? 1.5f : 1f);
                        return;
                    }
                }

                if (nextPathIndex >= path.Count || socialApproachElapsed >= socialApproachMaxSeconds)
                {
                    path.Clear();
                    nextPathIndex = 0;
                    socialApproachReached = true;
                }
            }

            FacePartner(socialPartner);
            animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
            if (socialApproachReached && socialPartner != null && socialPartner.socialApproachReached)
                BeginSocialConversation();
        }

        private void BeginSocialReaction()
        {
            if (!socialGreeting)
            {
                ShowSocialIndicator(false, socialTimer);
                return;
            }

            ShowNextSocialReaction();
        }

        private void ShowNextSocialReaction()
        {
            ShopCustomerSpeechBubble.IconKind[] reactions =
            {
                ShopCustomerSpeechBubble.IconKind.MoodHappy,
                ShopCustomerSpeechBubble.IconKind.MoodDown,
                ShopCustomerSpeechBubble.IconKind.Thought,
                ShopCustomerSpeechBubble.IconKind.Angry,
                ShopCustomerSpeechBubble.IconKind.Sad
            };
            float duration = SocialReactionSeconds;
            ShowBubbleIcon(reactions[Random.Range(0, reactions.Length)], duration);
            greetingSymbolTimer = duration;
        }

        private float SocialReactionSeconds => behaviorSettings == null
            ? 1.5f
            : Mathf.Max(0.2f, behaviorSettings.SocialReactionSeconds);

        private float SocialReactionTotalSeconds => behaviorSettings == null
            ? 4.5f
            : Mathf.Max(SocialReactionSeconds, behaviorSettings.SocialReactionTotalSeconds);

        private void BeginSocialConversation()
        {
            socialApproaching = false;
            socialApproachElapsed = 0f;
            socialTimer = pendingSocialDuration;
            greetingSymbolTimer = socialGreeting ? Mathf.Min(0.75f, socialTimer) : 0f;
            FacePartner(socialPartner);
            animationBinding.SetExternalMotion(lastDirection, false, Time.deltaTime);
            ShowSocialIndicator(socialGreeting, socialTimer);
        }

        private void FacePartner(ShopCustomerWanderAI partner)
        {
            if (partner == null)
                return;
            Vector2 direction = (Vector2)partner.transform.position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                lastDirection = direction;
        }

        private void FinishSocialInteraction()
        {
            socializing = false;
            socialGreeting = false;
            socialApproaching = false;
            socialApproachReached = false;
            bool sameDirection = socialPartner != null
                && hasStreetPassThroughDestination
                && socialPartner.hasStreetPassThroughDestination
                && streetDirection == socialPartner.streetDirection;
            if (sameDirection)
                MatchTravelSpeed(socialPartner);
            bool showClosingIcon = socialPartner != null;
            socialPartner = null;
            socialTimer = 0f;
            greetingSymbolTimer = 0f;
            pendingSocialDuration = 0f;
            path.Clear();
            nextPathIndex = 0;
            if (showClosingIcon)
            {
                ShowFarewellIndicator(sameDirection);
                farewellTimer = behaviorSettings == null
                    ? 1.2f
                    : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);
            }
            else
            {
                ClearInteractionIndicator();
            }

            if (farewellTimer <= 0f)
                ChooseNextDestination();
        }

        internal void CancelSocialInteractionFromPartner(ShopCustomerWanderAI partner)
        {
            if (socialPartner != partner)
                return;
            bool sameDirection = hasStreetPassThroughDestination
                && partner != null
                && partner.hasStreetPassThroughDestination
                && streetDirection == partner.streetDirection;
            if (sameDirection)
                MatchTravelSpeed(partner);
            socializing = false;
            socialPartner = null;
            socialApproaching = false;
            socialApproachReached = false;
            socialTimer = 0f;
            greetingSymbolTimer = 0f;
            pendingSocialDuration = 0f;
            path.Clear();
            nextPathIndex = 0;
            ShowFarewellIndicator(sameDirection);
            farewellTimer = behaviorSettings == null
                ? 1.2f
                : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds);
        }

        private void ReleaseBrowseReservation()
        {
            if (reservedCase == null)
                return;
            if (navigation != null)
                navigation.ReleaseBrowseTarget(this);
            reservedCase = null;
            browseThoughtBubbleShown = false;
        }

        private void ShowFarewellIndicator(bool travelTogether)
        {
            ShowBubbleIcon(travelTogether
                    ? ShopCustomerSpeechBubble.IconKind.BrowseTogether
                    : ShopCustomerSpeechBubble.IconKind.Wave,
                behaviorSettings == null ? 1.2f : Mathf.Max(0.2f, behaviorSettings.SocialFarewellSeconds));
        }

        private void ShowThoughtIndicator(float duration)
        {
            ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind.Thought, duration);
        }

        private void ShowEmptyDisplayIndicator(float duration)
        {
            ShowThoughtText(duration);
        }

        private void ShowThoughtText(float duration)
        {
            TextMesh text = EnsureThoughtText();
            if (text == null)
            {
                Debug.LogError("顾客预制体缺少头顶内心文字 CustomerTextThought。", this);
                return;
            }

            if (interactionBubble != null)
                interactionBubble.Hide();
            text.gameObject.SetActive(true);
            indicatorTimer = Mathf.Max(0.2f, duration);
            UpdateThoughtSorting();
        }

        private TextMesh EnsureThoughtText()
        {
            if (thoughtText != null)
                return thoughtText;

            Transform named = transform.Find("CustomerTextThought");
            if (named == null)
                return null;

            thoughtText = named.GetComponent<TextMesh>();
            if (thoughtText != null && thoughtText.font == null)
                thoughtText.font = WelCome.UI.UIHelper.DefaultFont;
            return thoughtText;
        }

        private void UpdateThoughtSorting()
        {
            if (thoughtText == null || animationBinding == null || animationBinding.spriteRenderer == null)
                return;

            MeshRenderer renderer = thoughtText.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            renderer.sortingLayerID = HudSortingLayerId;
            renderer.sortingOrder = HudSortingOrder(4);
        }

        private void ShowUnhappyInteractionIndicator(float duration)
        {
            ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind.MoodDown, duration);
        }

        private void ShowSocialIndicator(bool greeting, float duration)
        {
            ShowBubbleIcon(greeting ? ShopCustomerSpeechBubble.IconKind.Greeting
                : ShopCustomerSpeechBubble.IconKind.Thought, duration);
        }

        private void ShowSocialMoodIndicator(bool happy, float duration)
        {
            ShowBubbleIcon(happy ? ShopCustomerSpeechBubble.IconKind.MoodHappy
                : ShopCustomerSpeechBubble.IconKind.MoodDown, duration);
        }

        private void ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind iconKind, float duration)
        {
            if (!TryGetInteractionBubble())
                return;

            if (!interactionBubble.ShowIcon(iconKind))
            {
                ClearInteractionIndicator();
                Debug.LogError("顾客状态气泡缺少当前图标配置。", interactionBubble);
                return;
            }

            StartInteractionIndicator(duration);
        }

        private bool TryGetInteractionBubble()
        {
            if (interactionBubble != null)
                return true;

            Transform anchor = speechBubbleAnchor != null ? speechBubbleAnchor : transform.Find("气泡挂点");
            if (anchor != null)
                interactionBubble = anchor.GetComponentInChildren<ShopCustomerSpeechBubble>(true);
            if (interactionBubble != null)
                return true;

            Debug.LogError("顾客预制体的气泡挂点下缺少状态气泡。", this);
            return false;
        }

        private void StartInteractionIndicator(float duration)
        {
            indicatorTimer = Mathf.Max(0.2f, duration);
            UpdateIndicatorSortingOrder();
        }

        private void UpdateInteractionIndicator(float deltaTime)
        {
            if (interactionBubble == null)
                return;
            if (indicatorTimer > 0f)
            {
                indicatorTimer -= deltaTime;
                if (indicatorTimer <= 0f && !socializing)
                {
                    interactionBubble.Hide();
                    HideThoughtText();
                }
            }
            UpdateIndicatorSortingOrder();
            UpdateThoughtSorting();
        }

        private static int HudSortingLayerId => SortingLayer.NameToID("HUD");

        private int HudSortingOrder(int localOffset)
        {
            return 1000 + Mathf.RoundToInt(-transform.position.y * 10f) + localOffset;
        }

        private void UpdateIndicatorSortingOrder()
        {
            if (interactionBubble == null || animationBinding == null
                || animationBinding.spriteRenderer == null)
                return;
            interactionBubble.SetSorting(HudSortingLayerId,
                HudSortingOrder(2));
        }

        private void HideThoughtText()
        {
            if (thoughtText == null)
                return;

            thoughtText.gameObject.SetActive(false);
        }

        private void ClearInteractionIndicator()
        {
            if (interactionBubble != null)
                interactionBubble.Hide();
            HideThoughtText();
            indicatorTimer = 0f;
        }

        private void OnDisable()
        {
            ReleaseBrowseReservation();
            if (socialCoordinator != null)
                socialCoordinator.Unregister(this);
            if (socialPartner != null)
                socialPartner.CancelSocialInteractionFromPartner(this);
            socialPartner = null;
            socializing = false;
            purchasePhase = PurchasePhase.None;
            browseThoughtBubbleShown = false;
            showingUnhappyBrowseOutcome = false;
            inspectedEmptyDisplayIds.Clear();
            consecutiveEmptyDisplayCount = 0;
            ClearInteractionIndicator();
        }

        public bool CanCollectPayment => purchasePhase == PurchasePhase.Offering && pauseTimer > 0f;
        public Vector3 PaymentAnchor => speechBubbleAnchor != null ? speechBubbleAnchor.position : transform.position + Vector3.up * 1.1f;

        public bool TryCollectPayment()
        {
            if (!CanCollectPayment || reservedCase == null || DisplayCaseSystem.Instance == null)
                return false;
            if (!DisplayCaseSystem.Instance.TrySellDisplayedProduct(reservedCase.ConfigId, 0, out int price))
                return false;
            purchasePhase = PurchasePhase.Paid;
            pauseTimer = 1.1f;
            ClearInteractionIndicator();
            ShopCustomerCoinBurst burst = GetComponent<ShopCustomerCoinBurst>();
            if (burst == null)
                burst = gameObject.AddComponent<ShopCustomerCoinBurst>();
            burst.Play();
            return price >= 0;
        }

        private void BeginBrowseLook()
        {
            FaceReservedCase();
            ClearInteractionIndicator();
            if (ReservedShelfIsEmpty())
            {
                RejectShelf("没货开什么店");
                return;
            }

            purchasePhase = PurchasePhase.Looking;
            pauseTimer = ShopPurchaseTuning.LookDuration();
            browseThoughtBubbleShown = true;
            ShowThoughtIndicator(pauseTimer);
        }

        private bool ReservedShelfIsEmpty()
        {
            if (reservedCase == null || DisplayCaseSystem.Instance == null)
                return false;
            return DisplayCaseSystem.Instance.GetDisplayedProduct(reservedCase.ConfigId, 0) == null;
        }

        private bool ContinuePurchase()
        {
            if (purchasePhase == PurchasePhase.Looking)
            {
                ResolveBrowseLook();
                return true;
            }

            bool leave = leaveAfterRejection;
            if (purchasePhase == PurchasePhase.Offering)
                leave = RememberUnboughtShelf();
            if (purchasePhase == PurchasePhase.Paid)
            {
                if (reservedCase != null)
                    inspectedEmptyDisplayIds.Add(reservedCase.GetInstanceID());
                leave = false;
            }

            purchasePhase = PurchasePhase.None;
            leaveAfterRejection = false;
            ClearInteractionIndicator();
            ReleaseBrowseReservation();
            if (leave)
                StartShopDeparture();
            else
                ChooseNextDestination();
            return true;
        }

        private void ResolveBrowseLook()
        {
            ProductInstance displayed = reservedCase != null && DisplayCaseSystem.Instance != null
                ? DisplayCaseSystem.Instance.GetDisplayedProduct(reservedCase.ConfigId, 0)
                : null;
            if (displayed == null)
            {
                RejectShelf("没货开什么店");
                return;
            }

            int roll = Random.Range(0, 100);
            int interest = ShopPurchaseTuning.InterestChance;
            int expensive = ShopPurchaseTuning.TooExpensiveChance;
            int again = ShopPurchaseTuning.LookAgainChance;
            if (displayed.CostPrice > 0 && displayed.SellingPrice > displayed.CostPrice * 2)
            {
                interest = Mathf.Max(0, interest - 20);
                expensive += 20;
            }

            if (roll < interest)
            {
                purchasePhase = PurchasePhase.Offering;
                offerDuration = Mathf.Max(0.5f, ShopPurchaseTuning.OfferSeconds);
                pauseTimer = offerDuration;
                FaceReservedCase();
                ShowBubbleIcon(ShopCustomerSpeechBubble.IconKind.Pay, offerDuration);
                if (TryGetInteractionBubble())
                    interactionBubble.SetCountdown(1f);
                return;
            }

            roll -= interest;
            if (roll < expensive)
            {
                RejectShelf("太贵了");
                return;
            }

            roll -= expensive;
            if (roll < again)
            {
                RejectShelf("再看看吧");
                return;
            }

            RejectShelf(null);
        }

        private void RejectShelf(string line)
        {
            leaveAfterRejection = RememberUnboughtShelf();
            if (string.IsNullOrEmpty(line))
            {
                purchasePhase = PurchasePhase.None;
                bool leave = leaveAfterRejection;
                leaveAfterRejection = false;
                ClearInteractionIndicator();
                ReleaseBrowseReservation();
                if (leave)
                    StartShopDeparture();
                else
                    ChooseNextDestination();
                return;
            }

            ShowReaction(line);
        }

        private bool RememberUnboughtShelf()
        {
            if (reservedCase == null)
                return false;
            inspectedEmptyDisplayIds.Add(reservedCase.GetInstanceID());
            consecutiveEmptyDisplayCount++;
            int threshold = behaviorSettings == null ? 3 : Mathf.Max(1, behaviorSettings.EmptyDisplayLeaveThreshold);
            return consecutiveEmptyDisplayCount >= threshold;
        }

        private void StartShopDeparture()
        {
            if (navigation == null)
            {
                ChooseNextDestination();
                return;
            }

            if (!hasStreetPassThroughDestination)
            {
                float streetY = (navigation.outdoorMin.y + navigation.outdoorMax.y) * 0.5f;
                ConfigureStreetDeparture(new Vector2(navigation.navigationMax.x + 6f, streetY));
            }

            ReleaseBrowseReservation();
            if (TryGetStreetDeparturePoint(out Vector2 outdoorExit)
                && TrySetDestinationThroughDoorway(outdoorExit))
            {
                streetDepartureStarted = true;
                return;
            }

            ChooseNextDestination();
        }

        private void ShowReaction(string line)
        {
            purchasePhase = PurchasePhase.Reacting;
            pauseTimer = Mathf.Max(0.4f, ShopPurchaseTuning.ReactionSeconds);
            ShowThoughtLine(line, pauseTimer);
        }

        private void ShowThoughtLine(string line, float duration)
        {
            TextMesh text = EnsureThoughtText();
            if (text == null)
            {
                ShowThoughtText(duration);
                return;
            }
            text.text = line;
            ShowThoughtText(duration);
        }

        private void FaceReservedCase()
        {
            if (reservedCase == null)
                return;
            Vector2 direction = (Vector2)reservedCase.transform.position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                lastDirection = direction;
        }

        private void UpdatePaymentCountdown()
        {
            if (!TryGetInteractionBubble() || offerDuration <= 0f)
                return;
            interactionBubble.SetCountdown(Mathf.Clamp01(pauseTimer / offerDuration));
        }

        private enum PurchasePhase
        {
            None,
            Looking,
            Reacting,
            Offering,
            Paid
        }


        private enum DestinationKind
        {
            Browse,
            Counter,
            Leave,
            Wander
        }

        private readonly struct DestinationCandidate
        {
            public readonly DestinationKind Kind;
            public readonly int Weight;
            public readonly Vector2 Position;

            public DestinationCandidate(DestinationKind kind, int weight, Vector2 position)
            {
                Kind = kind;
                Weight = weight;
                Position = position;
            }
        }
    }
}
