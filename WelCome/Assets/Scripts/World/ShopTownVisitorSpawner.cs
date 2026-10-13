using System.Collections.Generic;
using UnityEngine;
using WelCome.Data;

namespace WelCome.World
{
    /// <summary>
    /// Loads customer prefabs at runtime and fills the shop street from Luban counts.
    /// </summary>
    [DefaultExecutionOrder(110)]
    [DisallowMultipleComponent]
    public sealed class ShopTownVisitorSpawner : MonoBehaviour
    {
        private const string CustomerPrefabResourceFolder = "OpenShop/Customers";

        [Header("横穿街道的路人路线")]
        [SerializeField, Min(0f), InspectorName("左侧入场偏移")]
        private float passerbyStartMargin = 0.5f;

        [SerializeField, Min(0f), InspectorName("右侧离场偏移")]
        private float passerbyExitMargin = 6f;

        private readonly List<cfg.ShopNpcProfile> townVisitorProfiles = new List<cfg.ShopNpcProfile>();
        private readonly List<cfg.ShopNpcProfile> passThroughProfiles = new List<cfg.ShopNpcProfile>();
        private readonly List<GameObject> visitorPrefabs = new List<GameObject>();
        private readonly List<ShopCustomerWanderAI> spawnedCustomers = new List<ShopCustomerWanderAI>();
        private readonly List<ShopCustomerWanderAI> activePassersby = new List<ShopCustomerWanderAI>();

        private cfg.ShopBehavior behaviorSettings;
        private ShopCustomerNavigation navigation;
        private float nextCustomerSpawnTime;
        private float nextPasserbySpawnTime;
        private int nextCustomerNumber = 1;

        public int SpawnedCustomerCount => spawnedCustomers.Count;
        public int ActivePasserbyCount => activePassersby.Count;

        private void Start()
        {
            if (!Initialize())
            {
                enabled = false;
                return;
            }

            SpawnConfiguredCustomers();
            nextCustomerSpawnTime = Time.time + Mathf.Max(0.1f, behaviorSettings.CustomerSpawnInterval);
            nextPasserbySpawnTime = Time.time;
        }

        private void Update()
        {
            ConfigManager configManager = ConfigManager.Instance;
            if (configManager != null && configManager.IsLoaded)
                behaviorSettings = configManager.Tables.TbShopBehavior.Data;

            if (behaviorSettings == null)
                return;

            spawnedCustomers.RemoveAll(visitor => visitor == null);
            int customerTarget = Mathf.Clamp(behaviorSettings.CustomerCount, 0, 40);
            RequestDepartureForExcessCustomers(customerTarget);
            if (spawnedCustomers.Count < customerTarget && Time.time >= nextCustomerSpawnTime)
            {
                float spawnInterval = Mathf.Max(0.1f, behaviorSettings.CustomerSpawnInterval);
                if (SpawnConfiguredCustomer())
                    nextCustomerSpawnTime = Time.time + spawnInterval;
                else
                    nextCustomerSpawnTime = Time.time + Mathf.Max(1f, spawnInterval);
            }

            activePassersby.RemoveAll(visitor => visitor == null || !visitor.IsPasserbyOnStreet);
            if (passThroughProfiles.Count == 0
                || activePassersby.Count >= Mathf.Max(0, behaviorSettings.PasserbyCount)
                || Time.time < nextPasserbySpawnTime)
                return;

            SpawnPasserby();
            nextPasserbySpawnTime = Time.time + Mathf.Max(0.1f, behaviorSettings.PasserbySpawnInterval);
        }

        private bool Initialize()
        {
            ConfigManager configManager = ConfigManager.Instance;
            if (!configManager.IsLoaded)
                configManager.LoadAllConfigs();
            if (!configManager.IsLoaded)
            {
                Debug.LogError("街区顾客配置未加载，无法生成顾客。", this);
                return false;
            }

            navigation = FindObjectOfType<ShopCustomerNavigation>();
            if (navigation == null)
            {
                Debug.LogError("场景找不到 ShopCustomerNavigation，无法生成顾客。", this);
                return false;
            }
            if (!navigation.IsReady)
                navigation.RefreshNavigation();

            behaviorSettings = configManager.Tables.TbShopBehavior.Data;
            foreach (cfg.ShopNpcProfile profile in configManager.Tables.TbShopNpcProfile.DataList)
            {
                if (profile.PassThrough)
                    passThroughProfiles.Add(profile);
                else if (!profile.OutdoorOnly)
                    townVisitorProfiles.Add(profile);
            }

            foreach (GameObject prefab in Resources.LoadAll<GameObject>(CustomerPrefabResourceFolder))
            {
                if (prefab != null && prefab.GetComponent<ShopVisitorWander>() != null
                    && prefab.GetComponent<ShopCustomerWanderAI>() != null)
                    visitorPrefabs.Add(prefab);
            }

            if (behaviorSettings == null
                || (behaviorSettings.CustomerCount > 0 && townVisitorProfiles.Count == 0)
                || visitorPrefabs.Count == 0)
            {
                Debug.LogError("顾客配置或 Resources/OpenShop/Customers 下的顾客预制体不完整。", this);
                return false;
            }

            if (behaviorSettings.PasserbyCount > 0 && passThroughProfiles.Count == 0)
            {
                Debug.LogError("街道横穿路人数量大于 0，但人物档案表没有启用横穿档案。", this);
                return false;
            }

            return true;
        }

        private void SpawnConfiguredCustomers()
        {
            int count = Mathf.Clamp(behaviorSettings.CustomerCount, 0, 40);
            for (int i = 0; i < count; i++)
            {
                if (!SpawnConfiguredCustomer())
                {
                    Debug.LogWarning("找不到可用的店内顾客起点，稍后会重试补充。", this);
                    return;
                }
            }
        }

        private bool SpawnConfiguredCustomer()
        {
            if (townVisitorProfiles.Count == 0)
                return false;

            cfg.ShopNpcProfile profile = townVisitorProfiles[Random.Range(0, townVisitorProfiles.Count)];
            for (int attempt = 0; attempt < 60; attempt++)
            {
                if (!navigation.TryGetRandomInteriorPoint(out Vector2 spawnPosition)
                    || !IsSpawnPositionClear(spawnPosition))
                    continue;

                ShopCustomerWanderAI visitor = CreateVisitor(profile, spawnPosition, false);
                if (visitor == null)
                    return false;

                spawnedCustomers.Add(visitor);
                return true;
            }

            return false;
        }

        private bool IsSpawnPositionClear(Vector2 candidate)
        {
            foreach (ShopCustomerWanderAI visitor in spawnedCustomers)
            {
                if (visitor != null && Vector2.Distance(visitor.transform.position, candidate) < 1.1f)
                    return false;
            }

            return true;
        }

        private void SpawnPasserby()
        {
            cfg.ShopNpcProfile profile = passThroughProfiles[Random.Range(0, passThroughProfiles.Count)];
            StreetRoute route = CreateStreetRoute();
            Vector2 start = new Vector2(route.StartX, route.LaneY);
            ShopCustomerWanderAI passerby = CreateVisitor(profile, start, true, route);
            if (passerby != null)
                activePassersby.Add(passerby);
        }

        private ShopCustomerWanderAI CreateVisitor(cfg.ShopNpcProfile profile, Vector2 position,
            bool passThrough, StreetRoute? configuredRoute = null)
        {
            StreetRoute route = configuredRoute ?? CreateStreetRoute();
            GameObject prefab = visitorPrefabs[Random.Range(0, visitorPrefabs.Count)];
            GameObject instance = Instantiate(prefab,
                new Vector3(position.x, position.y, 0f), Quaternion.identity, transform);
            instance.name = profile.ProfileName + " " + nextCustomerNumber.ToString("00");
            nextCustomerNumber++;

            ShopCustomerWanderAI customer = instance.GetComponent<ShopCustomerWanderAI>();
            ShopVisitorWander animation = instance.GetComponent<ShopVisitorWander>();
            if (customer == null || animation == null)
            {
                Debug.LogError("顾客预制体必须包含 ShopVisitorWander 和 ShopCustomerWanderAI。", prefab);
                Destroy(instance);
                return null;
            }

            customer.behaviorProfileId = profile.Id;
            customer.navigation = navigation;
            customer.animationBinding = animation;
            if (passThrough)
                customer.ConfigureStreetPassThrough(new Vector2(route.ExitX, route.LaneY),
                    route.SpeedMultiplier, route.LaneIndex);
            else
                customer.ConfigureStreetDeparture(new Vector2(route.ExitX, route.LaneY),
                    route.SpeedMultiplier, route.LaneIndex);

            animation.enabled = true;
            customer.enabled = true;
            return customer;
        }

        private StreetRoute CreateStreetRoute()
        {
            int laneCount = Mathf.Clamp(behaviorSettings.PasserbyLaneCount, 2, 10);
            int rightToLeftChance = Mathf.Clamp(behaviorSettings.PasserbyRightToLeftChance, 0, 100);
            int direction = Random.Range(0, 100) < rightToLeftChance ? -1 : 1;
            int laneIndex = ChooseLane(direction, laneCount);
            float laneY = GetLaneY(laneIndex, laneCount);
            float speedVariation = Mathf.Clamp(behaviorSettings.PasserbySpeedVariationPercent, 0, 80) / 100f;
            float speedMultiplier = Random.Range(1f - speedVariation, 1f + speedVariation);

            bool movesRight = direction > 0;
            float startX = movesRight
                ? navigation.navigationMin.x - passerbyStartMargin
                : navigation.navigationMax.x + passerbyStartMargin;
            float exitX = movesRight
                ? navigation.navigationMax.x + passerbyExitMargin
                : navigation.navigationMin.x - passerbyExitMargin;
            return new StreetRoute(laneIndex, direction, laneY, startX, exitX, speedMultiplier);
        }

        private int ChooseLane(int direction, int laneCount)
        {
            List<int> leastOccupiedLanes = new List<int>();
            int firstLane = direction > 0 ? 0 : 1;
            int lowestOccupancy = int.MaxValue;

            for (int lane = firstLane; lane < laneCount; lane += 2)
            {
                int occupancy = 0;
                foreach (ShopCustomerWanderAI visitor in activePassersby)
                {
                    if (visitor != null && visitor.IsPasserbyOnStreet
                        && visitor.StreetDirection == direction && visitor.StreetLaneIndex == lane)
                    {
                        occupancy++;
                    }
                }

                if (occupancy < lowestOccupancy)
                {
                    lowestOccupancy = occupancy;
                    leastOccupiedLanes.Clear();
                }
                if (occupancy == lowestOccupancy)
                    leastOccupiedLanes.Add(lane);
            }

            return leastOccupiedLanes[Random.Range(0, leastOccupiedLanes.Count)];
        }

        private float GetLaneY(int laneIndex, int laneCount)
        {
            float minY = navigation.outdoorMin.y + 0.15f;
            float maxY = navigation.outdoorMax.y - 0.15f;
            if (maxY < minY)
            {
                float center = (navigation.outdoorMin.y + navigation.outdoorMax.y) * 0.5f;
                minY = center;
                maxY = center;
            }

            float laneFraction = laneCount <= 1 ? 0.5f : laneIndex / (float)(laneCount - 1);
            return Mathf.Lerp(minY, maxY, laneFraction);
        }

        private void RequestDepartureForExcessCustomers(int targetCount)
        {
            int excess = spawnedCustomers.Count - targetCount;
            for (int index = spawnedCustomers.Count - 1; index >= 0 && excess > 0; index--)
            {
                ShopCustomerWanderAI visitor = spawnedCustomers[index];
                if (visitor != null && !visitor.IsLeavingShop && visitor.RequestLeaveFromShop())
                    excess--;
            }
        }

        private readonly struct StreetRoute
        {
            public readonly int LaneIndex;
            public readonly int Direction;
            public readonly float LaneY;
            public readonly float StartX;
            public readonly float ExitX;
            public readonly float SpeedMultiplier;

            public StreetRoute(int laneIndex, int direction, float laneY, float startX, float exitX,
                float speedMultiplier)
            {
                LaneIndex = laneIndex;
                Direction = direction;
                LaneY = laneY;
                StartX = startX;
                ExitX = exitX;
                SpeedMultiplier = speedMultiplier;
            }
        }
    }
}
