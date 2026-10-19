using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WelCome.EditorTools
{
    internal static class AdditionalFunctionsMenu
    {
        private const string MenuPath = "GameObject/附加功能/获取目标hierarchy的路径";

        [MenuItem(MenuPath, false, 1000)]
        private static void CopyHierarchyPath()
        {
            GameObject[] targets = Selection.gameObjects;
            if (targets == null || targets.Length == 0)
                return;

            var lines = new string[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                lines[i] = GetHierarchyPath(targets[i].transform);

            string pathText = string.Join("\n", lines);
            EditorGUIUtility.systemCopyBuffer = pathText;
            Debug.Log(targets.Length == 1
                ? "已复制 Hierarchy 路径：" + pathText
                : "已复制 " + targets.Length + " 条 Hierarchy 路径：\n" + pathText, targets[0]);
        }

        [MenuItem(MenuPath, true)]
        private static bool CanCopyHierarchyPath()
        {
            return Selection.activeGameObject != null;
        }

        private static string GetHierarchyPath(Transform target)
        {
            var names = new List<string>();
            while (target != null)
            {
                names.Add(target.name);
                target = target.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}
