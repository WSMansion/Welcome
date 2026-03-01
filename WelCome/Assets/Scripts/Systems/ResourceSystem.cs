using System.Collections.Generic;
using UnityEngine;
using WelCome.Core;

namespace WelCome.Systems
{
    public class ResourceSystem : Singleton<ResourceSystem>
    {
        public int Gold { get; private set; }
        public int MagicPointsCurrent { get; private set; }
        public int MagicPointsMax { get; private set; } = 10;
        public int Attention { get; private set; }
        public int Support { get; private set; }

        private readonly Dictionary<int, int> _materials = new();

        public void LoadState(int gold, int magicMax, int attention, int support,
            List<MaterialEntry> materials)
        {
            Gold = gold;
            MagicPointsMax = magicMax;
            MagicPointsCurrent = magicMax;
            Attention = attention;
            Support = support;
            _materials.Clear();
            if (materials != null)
            {
                foreach (var m in materials)
                    _materials[m.id] = m.count;
            }
        }

        public void ResetAll()
        {
            var cfg = Data.ConfigManager.Instance;
            if (cfg.IsLoaded)
            {
                Gold = cfg.Tables.TbGlobalParam.StartingGold;
                MagicPointsMax = cfg.Tables.TbGlobalParam.MagicPerNight;
            }
            else
            {
                Gold = 100;
                MagicPointsMax = 10;
            }
            MagicPointsCurrent = MagicPointsMax;
            Attention = 0;
            Support = 0;
            _materials.Clear();
        }

        #region Gold

        public void AddGold(int amount)
        {
            Gold += amount;
            EventBus.Publish(new GoldChangedEvent { Current = Gold, Delta = amount });
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            EventBus.Publish(new GoldChangedEvent { Current = Gold, Delta = -amount });
            return true;
        }

        #endregion

        #region Magic Points

        public void RefreshDailyMagic()
        {
            MagicPointsCurrent = MagicPointsMax;
            EventBus.Publish(new MagicPointsChangedEvent
            {
                Current = MagicPointsCurrent,
                Max = MagicPointsMax
            });
        }

        public bool SpendMagic(int amount)
        {
            if (MagicPointsCurrent < amount) return false;
            MagicPointsCurrent -= amount;
            EventBus.Publish(new MagicPointsChangedEvent
            {
                Current = MagicPointsCurrent,
                Max = MagicPointsMax
            });
            return true;
        }

        public void SetMagicMax(int max)
        {
            MagicPointsMax = max;
        }

        #endregion

        #region Materials

        public int GetMaterial(int materialId)
        {
            return _materials.TryGetValue(materialId, out int count) ? count : 0;
        }

        public void AddMaterial(int materialId, int amount)
        {
            _materials.TryGetValue(materialId, out int current);
            _materials[materialId] = current + amount;
            EventBus.Publish(new MaterialChangedEvent
            {
                MaterialId = materialId,
                NewCount = _materials[materialId],
                Delta = amount
            });
        }

        public bool SpendMaterial(int materialId, int amount)
        {
            if (GetMaterial(materialId) < amount) return false;
            _materials[materialId] -= amount;
            EventBus.Publish(new MaterialChangedEvent
            {
                MaterialId = materialId,
                NewCount = _materials[materialId],
                Delta = -amount
            });
            return true;
        }

        public IReadOnlyDictionary<int, int> Materials => _materials;

        #endregion

        #region Attention & Support

        public void ChangeAttention(int delta)
        {
            Attention = Mathf.Clamp(Attention + delta, 0, 100);
            EventBus.Publish(new AttentionChangedEvent { Current = Attention, Delta = delta });
        }

        public void ChangeSupport(int delta)
        {
            Support = Mathf.Clamp(Support + delta, 0, 100);
            EventBus.Publish(new SupportChangedEvent { Current = Support, Delta = delta });
        }

        #endregion
    }
}
