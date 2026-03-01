using System;
using UnityEngine;
using SimpleJSON;
using WelCome.Core;

namespace WelCome.Data
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public cfg.Tables Tables { get; private set; }
        public bool IsLoaded => Tables != null;

        protected override void OnInit()
        {
            LoadAllConfigs();
        }

        public void LoadAllConfigs()
        {
            try
            {
                Tables = new cfg.Tables(LoadJson);
                Debug.Log("[ConfigManager] All configs loaded.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConfigManager] Failed to load configs: {e.Message}\n{e.StackTrace}");
                Tables = null;
            }
        }

        private JSONNode LoadJson(string fileName)
        {
            var textAsset = Resources.Load<TextAsset>($"Luban/Output/Json/{fileName}");
            if (textAsset == null)
            {
                Debug.LogError($"[ConfigManager] Missing config file: {fileName}");
                return new JSONObject();
            }
            return JSON.Parse(textAsset.text);
        }
    }
}
