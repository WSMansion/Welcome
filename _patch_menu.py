from pathlib import Path
p = Path(r'WelCome/Assets/Editor/ShopCustomerPresentationAssetBuilder.cs')
t = p.read_text(encoding='utf-8')
old = '        public static void EnsurePresentationPrefabs()'
new = '''        [MenuItem("代号开店/更新顾客表情资源")]
        private static void UpdateCustomerMoodAssetsFromMenu()
        {
            EnsureCustomerMoodAssets();
            AssetDatabase.SaveAssets();
        }

        public static void EnsureCustomerMoodAssets()
        {
            EnsureCustomerBubbleSpriteAssets();
            AssignCustomerMoodSprites();
        }

        private static void AssignCustomerMoodSprites()
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(CustomerBubblePrefabPath);
            try
            {
                ShopCustomerSpeechBubble bubble = prefab.GetComponent<ShopCustomerSpeechBubble>();
                SerializedObject serialized = new SerializedObject(bubble);
                AssignSprite(serialized, "angryIcon", AngrySpritePath);
                AssignSprite(serialized, "sadIcon", SadSpritePath);
                AssignSprite(serialized, "waveIcon", WaveSpritePath);
                AssignSprite(serialized, "browseTogetherIcon", BrowseTogetherSpritePath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefab, CustomerBubblePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void AssignSprite(SerializedObject serialized, string propertyName, string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                throw new InvalidOperationException("缺少顾客表情图片：" + assetPath);

            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException("顾客气泡缺少表情字段：" + propertyName);
            property.objectReferenceValue = sprite;
        }

        public static void EnsurePresentationPrefabs()'''
assert old in t
t = t.replace(old, new, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('menu added')
