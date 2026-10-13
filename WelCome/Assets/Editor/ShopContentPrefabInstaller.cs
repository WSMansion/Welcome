using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WelCome.EditorTools;
using WelCome.World;

namespace WelCome.EditorTools
{
    [InitializeOnLoad]
    internal static class ShopContentPrefabInstaller
    {
        private const string ScenePath = "Assets/Scenes/OpenShopScene_MapDemo.unity";
        private const string CustomerPrefabPathPrefix = "Assets/Prefabs/OpenShop/Customers/Visitor_";
        private const string VisitorsRootName = "顾客（运行预览）";
        private const string PropsRootName = "店铺摆设（可自由移动）";
        private const string RoutesRootName = "顾客路线（移动圆点调整）";

        static ShopContentPrefabInstaller()
        {
            EditorApplication.delayCall += InstallIfRequested;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && File.Exists(RequestPath))
                EditorApplication.delayCall += InstallIfRequested;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(ProjectRoot, "Library", "InstallShopContentPrefabsRequested");

        private static void InstallIfRequested()
        {
            if (!File.Exists(RequestPath))
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Keep the request until the edit-mode callback can resume it.
                EditorApplication.isPlaying = false;
                return;
            }

            try
            {
                InstallIntoScene();
                File.Delete(RequestPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                File.Delete(RequestPath);
            }
        }

        [MenuItem("代号开店/整理顾客形象与陈列柜预制体")]
        private static void InstallFromMenu()
        {
            try
            {
                InstallIntoScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void InstallIntoScene()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("请先退出播放模式，再整理顾客和陈列柜预制体。");

            Scene targetScene = SceneManager.GetSceneByPath(ScenePath);
            if (targetScene.IsValid() && targetScene.isLoaded && targetScene.isDirty)
                throw new InvalidOperationException("带外景示范场景有未保存修改。请先保存场景，再重新运行“整理顾客形象与陈列柜预制体”。");

            GameObject[] visitorPrefabs = new GameObject[4];
            for (int i = 0; i < visitorPrefabs.Length; i++)
                visitorPrefabs[i] = OpenShopSceneBuilder.GetOrCreateVisitorPrefab(i);
            GameObject displayCasePrefab = OpenShopSceneBuilder.GetOrCreateDisplayCasePrefab();
            AssetDatabase.SaveAssets();

            Scene previousScene = SceneManager.GetActiveScene();
            bool openedHere = !targetScene.IsValid() || !targetScene.isLoaded;
            if (openedHere)
                targetScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                if (!targetScene.IsValid() || !targetScene.isLoaded)
                    throw new InvalidOperationException("无法打开带外景示范场景：" + ScenePath);

                SceneManager.SetActiveScene(targetScene);
                Transform visitorsRoot = FindNamedObject(targetScene, VisitorsRootName);
                Transform propsRoot = FindNamedObject(targetScene, PropsRootName);
                Transform routesRoot = FindNamedObject(targetScene, RoutesRootName);
                if (visitorsRoot == null || propsRoot == null || routesRoot == null)
                    throw new InvalidOperationException("带外景示范场景缺少顾客、店铺摆设或路线对象。");

                ShopCustomerNavigation navigation = FindOrCreateNavigation(routesRoot);
                EnsureDemoBootstrap(routesRoot);
                int visitorCount = InstallVisitorPrefabs(visitorsRoot, visitorPrefabs, navigation);
                int displayCaseCount = InstallDisplayCasePrefabs(propsRoot, displayCasePrefab);

                EditorSceneManager.MarkSceneDirty(targetScene);
                if (!EditorSceneManager.SaveScene(targetScene, ScenePath))
                    throw new IOException("保存带外景示范场景失败：" + ScenePath);

                AssetDatabase.SaveAssets();
                Debug.Log($"[ShopContentPrefabInstaller] 完成：{visitorCount} 个顾客实例改用形象预制体，{displayCaseCount} 个陈列架实例改用陈列柜预制体。");
            }
            finally
            {
                if (openedHere && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.CloseScene(targetScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
        }

        private static ShopCustomerNavigation FindOrCreateNavigation(Transform routesRoot)
        {
            ShopCustomerNavigation navigation = routesRoot.GetComponentInChildren<ShopCustomerNavigation>(true);
            if (navigation == null)
            {
                GameObject navigationObject = new GameObject("顾客寻路与行为");
                navigationObject.transform.SetParent(routesRoot, false);
                navigation = navigationObject.AddComponent<ShopCustomerNavigation>();
            }

            navigation.navigationMin = new Vector2(5f, 3f);
            navigation.navigationMax = new Vector2(35f, 23f);
            navigation.cellSize = 0.5f;
            navigation.outdoorMin = new Vector2(13f, 5.7f);
            navigation.outdoorMax = new Vector2(27f, 8.4f);
            navigation.doorwayPosition = new Vector2(20f, 9.6f);
            navigation.counterPosition = new Vector2(20f, 17.6f);
            navigation.outdoorThresholdY = 9.25f;
            return navigation;
        }

        private static void EnsureDemoBootstrap(Transform routesRoot)
        {
            if (routesRoot.GetComponentInChildren<ShopDemoBootstrap>(true) != null)
                return;

            GameObject bootstrapObject = new GameObject("店铺场景初始化");
            bootstrapObject.transform.SetParent(routesRoot, false);
            bootstrapObject.AddComponent<ShopDemoBootstrap>();
        }

        private static int InstallVisitorPrefabs(Transform visitorsRoot, GameObject[] prefabs,
            ShopCustomerNavigation navigation)
        {
            List<VisitorSnapshot> visitors = new List<VisitorSnapshot>();
            foreach (ShopVisitorWander visitor in visitorsRoot.GetComponentsInChildren<ShopVisitorWander>(true))
            {
                if (visitor.gameObject != visitorsRoot.gameObject)
                    visitors.Add(new VisitorSnapshot(visitor));
            }
            visitors.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

            int converted = 0;
            for (int i = 0; i < visitors.Count; i++)
            {
                VisitorSnapshot snapshot = visitors[i];
                if (IsManagedVisitorPrefabInstance(snapshot.Source))
                    continue;

                int appearanceIndex = snapshot.AppearanceIndex > 0
                    ? snapshot.AppearanceIndex
                    : i % prefabs.Length;
                ReplaceVisitor(snapshot, prefabs[appearanceIndex], navigation, i);
                converted++;
            }

            return converted;
        }

        private static bool IsManagedVisitorPrefabInstance(GameObject visitor)
        {
            if (visitor == null || !PrefabUtility.IsPartOfPrefabInstance(visitor))
                return false;

            UnityEngine.Object prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(visitor);
            string sourcePath = AssetDatabase.GetAssetPath(prefabSource);
            return sourcePath.StartsWith(CustomerPrefabPathPrefix, StringComparison.Ordinal)
                && sourcePath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase);
        }

        private static void ReplaceVisitor(VisitorSnapshot snapshot, GameObject prefab,
            ShopCustomerNavigation navigation, int index)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, snapshot.Source.scene) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("无法创建顾客形象预制体实例：" + prefab.name);

            instance.name = snapshot.Name;
            instance.transform.SetParent(snapshot.Parent, false);
            instance.transform.localPosition = snapshot.LocalPosition;
            instance.transform.localRotation = snapshot.LocalRotation;
            instance.transform.localScale = snapshot.LocalScale;
            if (snapshot.Parent != null)
                instance.transform.SetSiblingIndex(snapshot.SiblingIndex);

            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            ShopVisitorWander wander = instance.GetComponent<ShopVisitorWander>();
            ShopCustomerWanderAI ai = instance.GetComponent<ShopCustomerWanderAI>();
            if (renderer == null || wander == null || ai == null)
                throw new InvalidOperationException("顾客形象预制体缺少 SpriteRenderer、ShopVisitorWander 或 ShopCustomerWanderAI。");

            renderer.sortingLayerID = snapshot.SortingLayerId;
            renderer.color = snapshot.Color;
            renderer.flipX = snapshot.FlipX;
            renderer.flipY = snapshot.FlipY;
            renderer.enabled = snapshot.RendererEnabled;

            wander.spriteRenderer = renderer;
            wander.waypoints = snapshot.Waypoints;
            wander.moveSpeed = snapshot.MoveSpeed;
            wander.pauseAtWaypoints = snapshot.PauseAtWaypoints;
            wander.enabled = true;

            ai.navigation = navigation;
            ai.animationBinding = wander;
            ai.moveSpeed = Mathf.Max(0.65f, snapshot.MoveSpeed);
            ai.browseChance = index % 3 == 0 ? 0.72f : 0.58f;
            ai.counterChance = 0.16f;
            ai.leaveChance = index % 2 == 0 ? 0.2f : 0.12f;
            ai.outdoorStayChance = index % 2 == 0 ? 0.45f : 0.25f;
            ai.enabled = true;

            Vector2[] startPositions =
            {
                new Vector2(15.5f, 7.1f), new Vector2(24.5f, 7.1f),
                new Vector2(8.5f, 11.8f), new Vector2(31.5f, 11.8f),
                new Vector2(16.5f, 14.5f), new Vector2(23.5f, 14.5f)
            };
            instance.transform.position = startPositions[index % startPositions.Length];

            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            PrefabUtility.RecordPrefabInstancePropertyModifications(wander);
            PrefabUtility.RecordPrefabInstancePropertyModifications(ai);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            UnityEngine.Object.DestroyImmediate(snapshot.Source);
        }

        private static int InstallDisplayCasePrefabs(Transform propsRoot, GameObject prefab)
        {
            List<Transform> props = new List<Transform>();
            foreach (Transform child in propsRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == propsRoot || child.GetComponent<ShopDisplayCaseView>() != null)
                    continue;
                if (child.name.StartsWith("展示架", StringComparison.Ordinal))
                    props.Add(child);
            }

            int converted = 0;
            foreach (Transform source in props)
            {
                int configId = GetCaseConfigId(source.position);
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, source.gameObject.scene) as GameObject;
                if (instance == null)
                    throw new InvalidOperationException("无法创建陈列柜预制体实例：" + prefab.name);

                instance.name = source.name;
                instance.transform.SetParent(source.parent, false);
                instance.transform.localPosition = source.localPosition;
                instance.transform.localRotation = source.localRotation;
                instance.transform.localScale = source.localScale;
                instance.transform.SetSiblingIndex(source.GetSiblingIndex());

                ShopDisplayCaseView view = instance.GetComponent<ShopDisplayCaseView>();
                if (view == null)
                    throw new InvalidOperationException("陈列柜预制体缺少 ShopDisplayCaseView。");
                view.configId = configId;
                PrefabUtility.RecordPrefabInstancePropertyModifications(view);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
                UnityEngine.Object.DestroyImmediate(source.gameObject);
                converted++;
            }

            return converted;
        }

        private static int GetCaseConfigId(Vector3 position)
        {
            bool east = position.x > 20f;
            int row = Mathf.Clamp(Mathf.RoundToInt((position.y - 12.7f) / 4f), 0, 2);
            int column = east
                ? (position.x < 30f ? 0 : 1)
                : (position.x < 10f ? 0 : 1);
            return (east ? 8111 : 8101) + row * 2 + column;
        }

        private static Transform FindNamedObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                    return root.transform;

                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == objectName)
                        return child;
                }
            }

            return null;
        }

        private sealed class VisitorSnapshot
        {
            public readonly GameObject Source;
            public readonly string Name;
            public readonly Transform Parent;
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;
            public readonly int SiblingIndex;
            public readonly int SortingLayerId;
            public readonly Color Color;
            public readonly bool FlipX;
            public readonly bool FlipY;
            public readonly bool RendererEnabled;
            public readonly Transform[] Waypoints;
            public readonly float MoveSpeed;
            public readonly float PauseAtWaypoints;
            public readonly int AppearanceIndex;

            public VisitorSnapshot(ShopVisitorWander wander)
            {
                Source = wander.gameObject;
                Name = Source.name;
                Parent = Source.transform.parent;
                LocalPosition = Source.transform.localPosition;
                LocalRotation = Source.transform.localRotation;
                LocalScale = Source.transform.localScale;
                SiblingIndex = Source.transform.GetSiblingIndex();
                Waypoints = wander.waypoints;
                MoveSpeed = wander.moveSpeed;
                PauseAtWaypoints = wander.pauseAtWaypoints;
                AppearanceIndex = ParseAppearanceIndex(wander);

                SpriteRenderer renderer = wander.spriteRenderer != null
                    ? wander.spriteRenderer
                    : Source.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    SortingLayerId = renderer.sortingLayerID;
                    Color = renderer.color;
                    FlipX = renderer.flipX;
                    FlipY = renderer.flipY;
                    RendererEnabled = renderer.enabled;
                }
            }

            private static int ParseAppearanceIndex(ShopVisitorWander wander)
            {
                Sprite candidate = wander.downFrames != null && wander.downFrames.Length > 0
                    ? wander.downFrames[0]
                    : null;
                if (candidate == null && wander.spriteRenderer != null)
                    candidate = wander.spriteRenderer.sprite;
                if (candidate == null)
                    return -1;

                string spriteName = candidate.name;
                if (!spriteName.StartsWith("visitor_", StringComparison.Ordinal) || spriteName.Length < 9)
                    return -1;
                return int.TryParse(spriteName.Substring(8, 1), out int appearanceIndex)
                    ? Mathf.Clamp(appearanceIndex, 0, 3)
                    : -1;
            }
        }
    }
}
