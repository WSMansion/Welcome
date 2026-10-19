using UnityEngine;

namespace WelCome.Data
{
    public static class ProductIcons
    {
        private static ProductIconLibrary library;

        public static void Bind(ProductIconLibrary value)
        {
            library = value;
        }

        public static Sprite Load(int recipeId)
        {
            ConfigManager configManager = ConfigManager.Instance;
            if (configManager == null || !configManager.IsLoaded)
                return null;
            if (!configManager.Tables.TbRecipe.DataMap.TryGetValue(recipeId, out cfg.Recipe recipe))
                return null;
            if (string.IsNullOrWhiteSpace(recipe.Icon) || library == null)
                return null;
            return library.Find(recipe.Icon);
        }
    }
}
