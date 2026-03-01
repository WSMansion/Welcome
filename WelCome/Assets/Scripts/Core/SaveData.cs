using System;
using System.Collections.Generic;

namespace WelCome.Core
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string timestamp;

        public int currentDay;
        public int gold;
        public int magicPointsMax;
        public int attention;
        public int support;

        public List<MaterialEntry> materials = new();
        public List<int> completedEventIds = new();
        public List<ProductEntry> inventory = new();

        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
    }

    [Serializable]
    public class MaterialEntry
    {
        public int id;
        public int count;
    }

    [Serializable]
    public class ProductEntry
    {
        public int recipeId;
        public string name;
        public int basePrice;
        public int quality;
        public int needTag;
        public int daysOld;
    }
}
