using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using WelCome.EditorTools;
using WelCome.World;

namespace WelCome.EditorTools
{
    [InitializeOnLoad]
    internal static class ShopContentPrefabInstaller
    {
        private const string ScenePath = "Assets/Scenes/OpenShopScene_MapDemo.unity";
        private const string VisitorsRootName = "顾客（运行预览）";
        private const string PropsRootName = "店铺摆设（可自由移动）";
        private const string RoutesRootName = "顾客路线（移动圆点调整）";
        private const string OutdoorPasserbyRequestFileName = "ConfigureOutdoorPassersbyRequested";

        static ShopContentPrefabInstaller()
        {
            EditorApplication.delayCall += InstallIfRequested;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode
                && (File.Exists(RequestPath) || File.Exists(OutdoorPasserbyRequestPath)))
                EditorApplication.delayCall += InstallIfRequested;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(ProjectRoot, "Library", "InstallShopContentPrefabsRequested");
        private static string OutdoorPasserbyRequestPath => Path.Combine(ProjectRoot, "Library", OutdoorPasserbyRequestFileName);

        private static void InstallIfRequested()
        {
            bool configureOutdoorPassersby = File.Exists(OutdoorPasserbyRequestPath);
            if (!File.Exists(RequestPath) && !configureOutdoorPassersby)
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Keep the request until the user returns to edit mode.
                return;
            }

            try
            {
                if (configureOutdoorPassersby)
                {
                    InstallIntoScene();
                    File.Delete(OutdoorPasserbyRequestPath);
                    if (File.Exists(RequestPath))
                        File.Delete(RequestPath);
                    return;
                }

                InstallIntoScene();
                File.Delete(RequestPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (File.Exists(RequestPath))
                    File.Delete(RequestPath);
                if (File.Exists(OutdoorPasserbyRequestPath))
                    File.Delete(OutdoorPasserbyRequestPath);
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

            for (int i = 0; i < 4; i++)
                OpenShopSceneBuilder.GetOrCreateCustomerPrefab(i);
            ShopCustomerPresentationAssetBuilder.EnsurePresentationPrefabs();
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

                FindOrCreateNavigation(routesRoot);
                EnsureDemoBootstrap(routesRoot, visitorsRoot);
                int visitorCount = RemoveStaticVisitors(visitorsRoot);
                int displayCaseCount = InstallDisplayCasePrefabs(propsRoot, displayCasePrefab);
                int streetDecorationCount = EnsureTownStreetDressing(targetScene, propsRoot);

                EditorSceneManager.MarkSceneDirty(targetScene);
                if (!EditorSceneManager.SaveScene(targetScene, ScenePath))
                    throw new IOException("保存带外景示范场景失败：" + ScenePath);

                AssetDatabase.SaveAssets();
                Debug.Log($"[ShopContentPrefabInstaller] 完成：移除 {visitorCount} 个场景内静态顾客，启用表驱动运行时顾客；补充 {streetDecorationCount} 项街景装饰，整理 {displayCaseCount} 个陈列柜实例。");
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

        private static void EnsureDemoBootstrap(Transform routesRoot, Transform visitorsRoot)
        {
            if (routesRoot.GetComponentInChildren<ShopDemoBootstrap>(true) == null)
            {
                GameObject bootstrapObject = new GameObject("店铺场景初始化");
                bootstrapObject.transform.SetParent(routesRoot, false);
                bootstrapObject.AddComponent<ShopDemoBootstrap>();
            }

            if (visitorsRoot.GetComponent<ShopTownVisitorSpawner>() == null)
                visitorsRoot.gameObject.AddComponent<ShopTownVisitorSpawner>();
        }

        private static int RemoveStaticVisitors(Transform visitorsRoot)
        {
            HashSet<GameObject> visitors = new HashSet<GameObject>();
            foreach (ShopVisitorWander visitor in visitorsRoot.GetComponentsInChildren<ShopVisitorWander>(true))
            {
                if (visitor.gameObject != visitorsRoot.gameObject)
                    visitors.Add(visitor.gameObject);
            }

            foreach (GameObject visitor in visitors)
            {
                if (visitor != null)
                    UnityEngine.Object.DestroyImmediate(visitor);
            }

            return visitors.Count;
        }

        private static int EnsureTownStreetDressing(Scene scene, Transform propsRoot)
        {
            Transform outdoorRoot = FindNamedObject(scene, "店外街景与绿化（玩家活动范围之外）");
            if (outdoorRoot == null)
            {
                GameObject rootObject = new GameObject("店外街景与绿化（玩家活动范围之外）");
                rootObject.transform.SetParent(propsRoot, false);
                outdoorRoot = rootObject.transform;
            }

            Transform townRoot = FindDirectChild(outdoorRoot, "街区生活补充");
            if (townRoot == null)
            {
                GameObject townObject = new GameObject("街区生活补充");
                townObject.transform.SetParent(outdoorRoot, false);
                townRoot = townObject.transform;
            }

            int added = 0;
            added += EnsureTownProp(townRoot, "西侧街边摊位", "prop_stall.png",
                new Vector2(12f, 9.25f), new Vector2(2.5f, 0.4f));
            added += EnsureTownProp(townRoot, "东侧街边摊位", "prop_stall.png",
                new Vector2(28f, 9.25f), new Vector2(2.5f, 0.4f));
            added += EnsureTownProp(townRoot, "西侧路口树木", "prop_tree.png",
                new Vector2(7.5f, 8f), new Vector2(0.7f, 0.35f));
            added += EnsureTownProp(townRoot, "东侧路口树木", "prop_tree.png",
                new Vector2(32.5f, 8f), new Vector2(0.7f, 0.35f));
            added += EnsureTownProp(townRoot, "左侧门前花箱", "prop_planter.png",
                new Vector2(15f, 5.9f), new Vector2(0.45f, 0.25f));
            added += EnsureTownProp(townRoot, "右侧门前花箱", "prop_planter.png",
                new Vector2(25f, 5.9f), new Vector2(0.45f, 0.25f));
            added += EnsureStreetFlowers(scene);
            return added;
        }

        private static int EnsureTownProp(Transform parent, string objectName, string spriteFile,
            Vector2 position, Vector2 colliderSize)
        {
            Transform existing = parent.Find(objectName);
            bool created = existing == null;
            if (created)
            {
                GameObject instance = new GameObject(objectName);
                existing = instance.transform;
                existing.SetParent(parent, false);
                existing.position = new Vector3(position.x, position.y, 0f);
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/OpenShop/" + spriteFile);
            if (sprite == null)
                throw new InvalidOperationException("找不到街景素材：" + spriteFile);

            SpriteRenderer renderer = existing.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = existing.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingOrder = 300 + Mathf.RoundToInt(-existing.position.y * 10f);

            if (colliderSize.x > 0f && colliderSize.y > 0f)
            {
                BoxCollider2D collider = existing.GetComponent<BoxCollider2D>();
                if (collider == null)
                    collider = existing.gameObject.AddComponent<BoxCollider2D>();
                collider.size = colliderSize;
                collider.offset = new Vector2(0f, colliderSize.y * 0.5f);
            }

            return created ? 1 : 0;
        }

        private static int EnsureStreetFlowers(Scene scene)
        {
            Tilemap ground = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Tilemap tilemap in root.GetComponentsInChildren<Tilemap>(true))
                {
                    if (tilemap.gameObject.name == "草地 Ground")
                    {
                        ground = tilemap;
                        break;
                    }
                }
                if (ground != null)
                    break;
            }

            TileBase flowers = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tilemaps/OpenShop/Tiles/Grass_Flowers.asset");
            if (ground == null || flowers == null)
                return 0;

            Vector3Int[] cells =
            {
                new Vector3Int(9, 1, 0), new Vector3Int(13, 2, 0),
                new Vector3Int(17, 1, 0), new Vector3Int(21, 2, 0),
                new Vector3Int(25, 1, 0), new Vector3Int(29, 2, 0),
                new Vector3Int(33, 1, 0), new Vector3Int(11, 2, 0),
                new Vector3Int(19, 1, 0), new Vector3Int(27, 2, 0)
            };

            int added = 0;
            foreach (Vector3Int cell in cells)
            {
                TileBase current = ground.GetTile(cell);
                if (current != null && current.name != "Grass_A" && current.name != "Grass_B")
                    continue;
                ground.SetTile(cell, flowers);
                added++;
            }

            return added;
        }

        private static Transform FindDirectChild(Transform parent, string objectName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == objectName)
                    return child;
            }
            return null;
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
                int siblingIndex = source.GetSiblingIndex();
                Vector3 localOffset = source.localRotation * Vector3.right
                    * Mathf.Abs(source.localScale.x);

                CreateDisplayCaseInstance(prefab, source, configId, source.name + " - A",
                    source.localPosition - localOffset, siblingIndex);
                CreateDisplayCaseInstance(prefab, source, configId + 1000, source.name + " - B",
                    source.localPosition + localOffset, siblingIndex + 1);
                UnityEngine.Object.DestroyImmediate(source.gameObject);
                converted += 2;
            }

            return converted;
        }

        private static GameObject CreateDisplayCaseInstance(GameObject prefab, Transform source,
            int configId, string instanceName, Vector3 localPosition, int siblingIndex)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, source.gameObject.scene) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("无法创建陈列柜预制体实例：" + prefab.name);

            instance.name = instanceName;
            instance.transform.SetParent(source.parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = source.localRotation;
            instance.transform.localScale = source.localScale;
            instance.transform.SetSiblingIndex(siblingIndex);

            ShopDisplayCaseView view = instance.GetComponent<ShopDisplayCaseView>();
            if (view == null)
                throw new InvalidOperationException("陈列柜预制体缺少 ShopDisplayCaseView。");
            view.configId = configId;
            PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
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

    }
}
