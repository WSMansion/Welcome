using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.Core
{
    public static class SaveManager
    {
        public const int SlotCount = 3;
        private const string SettingsFileName = "settings.json";
        private const string MetaFileName = "savemeta.json";

        private static string BasePath => Application.persistentDataPath;
        private static string SettingsPath => Path.Combine(BasePath, SettingsFileName);
        private static string MetaPath => Path.Combine(BasePath, MetaFileName);

        public static int ActiveSlot { get; set; } = -1;

        private static string SlotPath(int slot) => Path.Combine(BasePath, $"save_{slot}.json");

        #region Slot Queries

        public static bool HasSlot(int slot) => File.Exists(SlotPath(slot));

        public static bool HasAnySave()
        {
            for (int i = 0; i < SlotCount; i++)
                if (HasSlot(i)) return true;
            return false;
        }

        public static string GetSlotSummary(int slot)
        {
            if (!HasSlot(slot)) return null;
            try
            {
                var json = File.ReadAllText(SlotPath(slot));
                var data = JsonUtility.FromJson<SaveData>(json);
                return $"第 {data.currentDay} 天  |  金币: {data.gold}  |  {data.timestamp}";
            }
            catch
            {
                return "存档已损坏";
            }
        }

        public static int GetMostRecentSlot()
        {
            if (File.Exists(MetaPath))
            {
                try
                {
                    var json = File.ReadAllText(MetaPath);
                    var meta = JsonUtility.FromJson<SaveMeta>(json);
                    if (meta.lastSlot >= 0 && meta.lastSlot < SlotCount && HasSlot(meta.lastSlot))
                        return meta.lastSlot;
                }
                catch { }
            }

            DateTime latest = DateTime.MinValue;
            int best = -1;
            for (int i = 0; i < SlotCount; i++)
            {
                string path = SlotPath(i);
                if (!File.Exists(path)) continue;
                var info = new FileInfo(path);
                if (info.LastWriteTime > latest)
                {
                    latest = info.LastWriteTime;
                    best = i;
                }
            }
            return best;
        }

        #endregion

        #region Save / Load / Delete

        public static void Save(int slot)
        {
            try
            {
                var data = GatherSaveData();
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SlotPath(slot), json);
                ActiveSlot = slot;
                SaveMeta(slot);
                Debug.Log($"[SaveManager] Slot {slot}: saved day {data.currentDay}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save slot {slot} failed: {e.Message}");
            }
        }

        /// <summary>
        /// Auto-save to the active slot. If no active slot, defaults to slot 0.
        /// </summary>
        public static void AutoSave()
        {
            int slot = ActiveSlot >= 0 ? ActiveSlot : 0;
            Save(slot);
        }

        public static bool Load(int slot)
        {
            if (!HasSlot(slot)) return false;
            try
            {
                var json = File.ReadAllText(SlotPath(slot));
                var data = JsonUtility.FromJson<SaveData>(json);
                ActiveSlot = slot;
                SaveMeta(slot);
                ApplySaveData(data);
                Debug.Log($"[SaveManager] Slot {slot}: loaded day {data.currentDay}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load slot {slot} failed: {e.Message}");
                return false;
            }
        }

        public static void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
            if (ActiveSlot == slot) ActiveSlot = -1;
        }

        public static void DeleteAllSaves()
        {
            for (int i = 0; i < SlotCount; i++)
                DeleteSlot(i);
            if (File.Exists(MetaPath)) File.Delete(MetaPath);
        }

        #endregion

        #region Internal

        private static SaveData GatherSaveData()
        {
            var res = ResourceSystem.Instance;
            var ws = WorkshopSystem.Instance;
            var des = DayEventSystem.Instance;

            var data = new SaveData
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                currentDay = GameManager.Instance.CurrentDay + 1,
                gold = res.Gold,
                magicPointsMax = res.MagicPointsMax,
                attention = res.Attention,
                support = res.Support,
                unlockedDisplayCaseIds = DisplayCaseSystem.Instance.GetUnlockedCaseIds(),
                displayCaseStock = DisplayCaseSystem.Instance.GetStockForSave(),
                displayCasePlacements = DisplayCaseSystem.Instance.GetPlacementsForSave(),
            };

            foreach (var kv in res.Materials)
                data.materials.Add(new MaterialEntry { id = kv.Key, count = kv.Value });

            foreach (int id in des.GetCompletedEventIds())
                data.completedEventIds.Add(id);

            foreach (var p in ws.Inventory)
            {
                data.inventory.Add(new ProductEntry
                {
                    recipeId = p.RecipeId,
                    name = p.Name,
                    basePrice = p.BasePrice,
                    quality = (int)p.Quality,
                    needTag = p.NeedTag,
                    daysOld = p.DaysOld,
                    listedPrice = p.ListedPrice
                });
            }

            return data;
        }

        private static void ApplySaveData(SaveData data)
        {
            ResourceSystem.Instance.LoadState(data.gold, data.magicPointsMax,
                data.attention, data.support, data.materials);
            DisplayCaseSystem.Instance.LoadProgress(data.unlockedDisplayCaseIds,
                data.displayCaseStock, data.displayCasePlacements);
            DayEventSystem.Instance.LoadCompletedIds(data.completedEventIds);
            WorkshopSystem.Instance.LoadInventory(data.inventory);
            GameManager.Instance.LoadGame(data.currentDay);
        }

        private static void SaveMeta(int slot)
        {
            try
            {
                var meta = new SaveMeta { lastSlot = slot };
                File.WriteAllText(MetaPath, JsonUtility.ToJson(meta));
            }
            catch { }
        }

        #endregion

        #region Settings

        public static void SaveSettings(float master, float music, float sfx)
        {
            try
            {
                var data = new SaveData { masterVolume = master, musicVolume = music, sfxVolume = sfx };
                File.WriteAllText(SettingsPath, JsonUtility.ToJson(data));
                ApplyAudioSettings(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Settings save failed: {e.Message}");
            }
        }

        public static SaveData LoadSettings()
        {
            var defaults = new SaveData();
            if (!File.Exists(SettingsPath)) return defaults;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(SettingsPath));
            }
            catch { return defaults; }
        }

        public static void ApplyAudioSettings(SaveData data)
        {
            AudioListener.volume = data.masterVolume;
        }

        #endregion
    }

    [Serializable]
    internal class SaveMeta
    {
        public int lastSlot;
    }
}
