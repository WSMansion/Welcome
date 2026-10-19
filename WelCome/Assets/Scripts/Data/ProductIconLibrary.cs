using UnityEngine;

namespace WelCome.Data
{
    [CreateAssetMenu(fileName = "ProductIconLibrary", menuName = "WelCome/商品图标库")]
    public sealed class ProductIconLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string key;
            public Sprite sprite;
        }

        public Entry[] entries;

        public Sprite Find(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || entries == null)
                return null;

            string name = System.IO.Path.GetFileNameWithoutExtension(key.Replace('\\', '/'));
            foreach (Entry entry in entries)
            {
                if (entry.sprite == null)
                    continue;
                if (entry.key == key || entry.key == name || entry.sprite.name == name)
                    return entry.sprite;
            }

            return null;
        }
    }
}
