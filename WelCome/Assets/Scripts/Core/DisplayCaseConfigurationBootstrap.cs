using System.Collections.Generic;
using UnityEngine;
using WelCome.Data;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.Core
{
    public static class DisplayCaseConfigurationBootstrap
    {
        public static void InitializeForDay(int day)
        {
            ConfigManager configManager = ConfigManager.Instance;
            if (!configManager.IsLoaded)
                configManager.LoadAllConfigs();

            List<DisplayCaseDefinition> definitions = new List<DisplayCaseDefinition>();
            if (configManager.IsLoaded)
            {
                foreach (cfg.DisplayCase config in configManager.Tables.TbDisplayCase.DataList)
                {
                    definitions.Add(new DisplayCaseDefinition
                    {
                        Id = config.Id,
                        DisplayName = config.DisplayName,
                        PrefabKey = config.PrefabKey,
                        UnlockDay = config.UnlockDay,
                        SlotCount = config.SlotCount
                    });
                }
            }
            else
            {
                Debug.LogWarning("[DisplayCaseConfigurationBootstrap] Display case configuration is not loaded.");
            }

            DisplayCaseSystem.Instance.SetDefinitions(definitions, day);
        }
    }
}
