using System.Collections.Generic;
using UnityEngine;
using WelCome.Core;
using WelCome.Models;

namespace WelCome.Systems
{
    /// <summary>
    /// Owns display case unlocks, displayed products, and saved placement coordinates.
    /// </summary>
    public sealed class DisplayCaseSystem : Singleton<DisplayCaseSystem>
    {
        private const int SplitPartnerCaseIdOffset = 1000;
        private const float SplitCasePlacementOffset = 1f;

        private readonly HashSet<int> unlockedCaseIds = new HashSet<int>();
        private readonly Dictionary<int, DisplayCaseDefinition> caseDefinitions =
            new Dictionary<int, DisplayCaseDefinition>();
        private readonly List<DisplayCaseStockEntry> displayedProducts = new List<DisplayCaseStockEntry>();
        private readonly Dictionary<int, DisplayCasePlacementEntry> placements =
            new Dictionary<int, DisplayCasePlacementEntry>();

        protected override void OnInit()
        {
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            RefreshProgression(1);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            base.OnDestroy();
        }

        public bool IsUnlocked(int caseId)
        {
            if (unlockedCaseIds.Contains(caseId))
                return true;

            DisplayCaseDefinition config = GetConfig(caseId);
            return config != null && config.UnlockDay <= GameManager.Instance.CurrentDay;
        }

        public int GetSlotCount(int caseId)
        {
            DisplayCaseDefinition config = GetConfig(caseId);
            return config == null ? 0 : Mathf.Max(0, config.SlotCount);
        }

        public string GetDisplayName(int caseId)
        {
            DisplayCaseDefinition config = GetConfig(caseId);
            return config == null || string.IsNullOrWhiteSpace(config.DisplayName)
                ? $"陈列柜 {caseId}"
                : config.DisplayName;
        }

        public string GetPrefabKey(int caseId)
        {
            DisplayCaseDefinition config = GetConfig(caseId);
            return config == null ? string.Empty : config.PrefabKey;
        }

        public void SetDefinitions(IEnumerable<DisplayCaseDefinition> definitions, int currentDay)
        {
            caseDefinitions.Clear();
            if (definitions != null)
            {
                foreach (DisplayCaseDefinition definition in definitions)
                {
                    if (definition != null && definition.Id > 0)
                        caseDefinitions[definition.Id] = definition;
                }
            }

            RefreshProgression(Mathf.Max(1, currentDay));
        }

        public void ResetProgress()
        {
            unlockedCaseIds.Clear();
            displayedProducts.Clear();
            placements.Clear();
            RefreshProgression(1);
        }

        public void LoadProgress(List<int> unlockedIds, List<DisplayCaseStockEntry> stock,
            List<DisplayCasePlacementEntry> savedPlacements)
        {
            unlockedCaseIds.Clear();
            if (unlockedIds != null)
            {
                foreach (int id in unlockedIds)
                {
                    unlockedCaseIds.Add(id);
                    if (IsFirstHalfDisplayCaseId(id))
                        unlockedCaseIds.Add(id + SplitPartnerCaseIdOffset);
                }
            }

            displayedProducts.Clear();
            if (stock != null)
            {
                foreach (DisplayCaseStockEntry entry in stock)
                {
                    if (entry != null && entry.product != null)
                    {
                        DisplayCaseStockEntry migratedEntry = Clone(entry);
                        if (migratedEntry.slotIndex == 1
                            && IsFirstHalfDisplayCaseId(migratedEntry.caseId))
                        {
                            migratedEntry.caseId += SplitPartnerCaseIdOffset;
                            migratedEntry.slotIndex = 0;
                        }
                        displayedProducts.Add(migratedEntry);
                    }
                }
            }

            placements.Clear();
            if (savedPlacements != null)
            {
                HashSet<int> savedPlacementIds = new HashSet<int>();
                foreach (DisplayCasePlacementEntry entry in savedPlacements)
                {
                    if (entry != null)
                        savedPlacementIds.Add(entry.caseId);
                }

                foreach (DisplayCasePlacementEntry entry in savedPlacements)
                {
                    if (entry != null)
                    {
                        int partnerCaseId = GetSplitPartnerCaseId(entry.caseId);
                        if (partnerCaseId > 0 && !savedPlacementIds.Contains(partnerCaseId))
                        {
                            float radians = entry.rotationZ * Mathf.Deg2Rad;
                            float offsetX = Mathf.Cos(radians) * SplitCasePlacementOffset;
                            float offsetY = Mathf.Sin(radians) * SplitCasePlacementOffset;
                            DisplayCasePlacementEntry firstHalf = Clone(entry);
                            firstHalf.x -= offsetX;
                            firstHalf.y -= offsetY;

                            DisplayCasePlacementEntry secondHalf = Clone(entry);
                            secondHalf.caseId = partnerCaseId;
                            secondHalf.x += offsetX;
                            secondHalf.y += offsetY;
                            placements[firstHalf.caseId] = firstHalf;
                            placements[secondHalf.caseId] = secondHalf;
                        }
                        else
                        {
                            placements[entry.caseId] = Clone(entry);
                        }
                    }
                }
            }

            RefreshProgression(GameManager.Instance.CurrentDay);
            EventBus.Publish(new DisplayCasesChangedEvent { DayNumber = GameManager.Instance.CurrentDay });
        }

        public List<int> GetUnlockedCaseIds()
        {
            List<int> result = new List<int>(unlockedCaseIds);
            result.Sort();
            return result;
        }

        public List<DisplayCaseStockEntry> GetStockForSave()
        {
            List<DisplayCaseStockEntry> result = new List<DisplayCaseStockEntry>(displayedProducts.Count);
            foreach (DisplayCaseStockEntry entry in displayedProducts)
                result.Add(Clone(entry));
            return result;
        }

        public List<DisplayCasePlacementEntry> GetPlacementsForSave()
        {
            List<DisplayCasePlacementEntry> result = new List<DisplayCasePlacementEntry>(placements.Count);
            foreach (DisplayCasePlacementEntry entry in placements.Values)
                result.Add(Clone(entry));
            result.Sort((left, right) => left.caseId.CompareTo(right.caseId));
            return result;
        }

        public bool TryGetPlacement(int caseId, out DisplayCasePlacementEntry placement)
        {
            if (placements.TryGetValue(caseId, out DisplayCasePlacementEntry stored))
            {
                placement = Clone(stored);
                return true;
            }

            placement = null;
            return false;
        }

        public void SetPlacement(int caseId, Vector3 position, float rotationZ)
        {
            if (caseId <= 0)
                return;

            if (placements.TryGetValue(caseId, out DisplayCasePlacementEntry current)
                && Mathf.Abs(current.x - position.x) < 0.001f
                && Mathf.Abs(current.y - position.y) < 0.001f
                && Mathf.Abs(current.z - position.z) < 0.001f
                && Mathf.Abs(Mathf.DeltaAngle(current.rotationZ, rotationZ)) < 0.01f)
                return;

            placements[caseId] = new DisplayCasePlacementEntry
            {
                caseId = caseId,
                x = position.x,
                y = position.y,
                z = position.z,
                rotationZ = rotationZ
            };
        }

        public ProductInstance GetDisplayedProduct(int caseId, int slotIndex)
        {
            DisplayCaseStockEntry entry = FindStockEntry(caseId, slotIndex);
            return entry == null ? null : ToProduct(entry.product);
        }

        public bool TryDisplayProduct(int caseId, int slotIndex, ProductInstance product)
        {
            if (product == null || !IsUnlocked(caseId) || slotIndex < 0 || slotIndex >= GetSlotCount(caseId)
                || FindStockEntry(caseId, slotIndex) != null)
                return false;

            if (!WorkshopSystem.Instance.RemoveFromInventory(product))
                return false;

            displayedProducts.Add(new DisplayCaseStockEntry
            {
                caseId = caseId,
                slotIndex = slotIndex,
                product = ToSaveEntry(product)
            });
            EventBus.Publish(new DisplayCasesChangedEvent { DayNumber = GameManager.Instance.CurrentDay });
            return true;
        }

        public bool TryTakeDisplayedProduct(int caseId, int slotIndex)
        {
            DisplayCaseStockEntry entry = FindStockEntry(caseId, slotIndex);
            if (entry == null)
                return false;

            WorkshopSystem.Instance.AddToInventory(ToProduct(entry.product));
            displayedProducts.Remove(entry);
            EventBus.Publish(new DisplayCasesChangedEvent { DayNumber = GameManager.Instance.CurrentDay });
            return true;
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            RefreshProgression(evt.DayNumber);
        }

        private void RefreshProgression(int day)
        {
            bool changed = false;
            foreach (DisplayCaseDefinition config in caseDefinitions.Values)
            {
                if (config.UnlockDay <= day && unlockedCaseIds.Add(config.Id))
                    changed = true;
            }

            if (changed)
                EventBus.Publish(new DisplayCasesChangedEvent { DayNumber = day });
        }

        private DisplayCaseDefinition GetConfig(int caseId)
        {
            return caseDefinitions.TryGetValue(caseId, out DisplayCaseDefinition definition)
                ? definition
                : null;
        }

        private static bool IsFirstHalfDisplayCaseId(int caseId)
        {
            return (caseId >= 8101 && caseId <= 8106)
                || (caseId >= 8111 && caseId <= 8116);
        }

        private static int GetSplitPartnerCaseId(int caseId)
        {
            return IsFirstHalfDisplayCaseId(caseId)
                ? caseId + SplitPartnerCaseIdOffset
                : -1;
        }

        private DisplayCaseStockEntry FindStockEntry(int caseId, int slotIndex)
        {
            foreach (DisplayCaseStockEntry entry in displayedProducts)
            {
                if (entry.caseId == caseId && entry.slotIndex == slotIndex)
                    return entry;
            }
            return null;
        }

        private static ProductEntry ToSaveEntry(ProductInstance product)
        {
            return new ProductEntry
            {
                recipeId = product.RecipeId,
                name = product.Name,
                basePrice = product.BasePrice,
                quality = (int)product.Quality,
                needTag = product.NeedTag,
                daysOld = product.DaysOld
            };
        }

        private static ProductInstance ToProduct(ProductEntry entry)
        {
            if (entry == null)
                return null;
            return new ProductInstance
            {
                RecipeId = entry.recipeId,
                Name = entry.name,
                BasePrice = entry.basePrice,
                Quality = (ProductQuality)entry.quality,
                NeedTag = entry.needTag,
                DaysOld = entry.daysOld
            };
        }

        private static DisplayCaseStockEntry Clone(DisplayCaseStockEntry entry)
        {
            return new DisplayCaseStockEntry
            {
                caseId = entry.caseId,
                slotIndex = entry.slotIndex,
                product = entry.product == null ? null : new ProductEntry
                {
                    recipeId = entry.product.recipeId,
                    name = entry.product.name,
                    basePrice = entry.product.basePrice,
                    quality = entry.product.quality,
                    needTag = entry.product.needTag,
                    daysOld = entry.product.daysOld
                }
            };
        }

        private static DisplayCasePlacementEntry Clone(DisplayCasePlacementEntry entry)
        {
            return new DisplayCasePlacementEntry
            {
                caseId = entry.caseId,
                x = entry.x,
                y = entry.y,
                z = entry.z,
                rotationZ = entry.rotationZ
            };
        }
    }
}
