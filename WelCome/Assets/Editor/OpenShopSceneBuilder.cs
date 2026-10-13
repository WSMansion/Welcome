using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using WelCome.World;

namespace WelCome.EditorTools
{
    [InitializeOnLoad]
    internal static class OpenShopSceneBuilder
    {
        private const int PixelsPerUnit = 32;
        private const int MapWidth = 32;
        private const int MapHeight = 18;
        private const int SortBase = 300;
        private const string ArtRoot = "Assets/Art/OpenShop";
        private const string TileRoot = "Assets/Tilemaps/OpenShop/Tiles";
        private const string ScenePath = "Assets/Scenes/OpenShopScene.unity";
        internal const string VisitorPrefabPath = "Assets/Prefabs/OpenShop/ShopVisitor.prefab";
        internal const string DisplayCasePrefabPath = "Assets/Prefabs/OpenShop/DisplayCase.prefab";
        private const string CustomerPrefabFolder = "Assets/Prefabs/OpenShop/Customers";

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        static OpenShopSceneBuilder()
        {
            EditorApplication.delayCall += RunRequestedBuild;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string BuildRequestPath => Path.Combine(ProjectRoot, "Library", "OpenShopSceneBuildRequested");

        private static void RunRequestedBuild()
        {
            if (!File.Exists(BuildRequestPath))
                return;

            try
            {
                BuildScene(false);
                File.Delete(BuildRequestPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        [MenuItem("代号开店/生成或重建开店场景")]
        private static void BuildFromMenu()
        {
            BuildScene(true);
        }

        private static void BuildScene(bool allowReplace)
        {
            bool sceneExists = File.Exists(Path.Combine(ProjectRoot, ScenePath.Replace('/', Path.DirectorySeparatorChar)));
            if (sceneExists)
            {
                if (!allowReplace || !EditorUtility.DisplayDialog(
                        "重建开店场景",
                        "这会重建 OpenShopScene.unity。场景中的手动摆放会被替换，是否继续？",
                        "重建",
                        "取消"))
                {
                    Debug.Log("[OpenShopSceneBuilder] Existing scene kept: " + ScenePath);
                    return;
                }
            }

            Directory.CreateDirectory(Path.Combine(ProjectRoot, TileRoot.Replace('/', Path.DirectorySeparatorChar)));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SpriteCache.Clear();

            Tile grassA = CreateTile("Grass_A", "tile_grass_a.png", false);
            Tile grassB = CreateTile("Grass_B", "tile_grass_b.png", false);
            Tile grassFlowers = CreateTile("Grass_Flowers", "tile_grass_flowers.png", false);
            Tile cobble = CreateTile("Cobble_Path", "tile_cobble.png", false);
            Tile wood = CreateTile("Wood_Floor", "tile_wood.png", false);
            AssetDatabase.SaveAssets();

            Scene previousScene = SceneManager.GetActiveScene();
            Scene generatedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(generatedScene);
                Camera camera = CreateCamera();
                CreateTilemaps(grassA, grassB, grassFlowers, cobble, wood);
                CreateProps();
                CreateGroup("自定义阻挡区（添加 BoxCollider2D）", null);
                Transform player = CreateVisitors();
                camera.GetComponent<ShopCameraFollow>().target = player;

                EditorSceneManager.MarkSceneDirty(generatedScene);
                EditorSceneManager.SaveScene(generatedScene, ScenePath);
            }
            finally
            {
                if (generatedScene.IsValid() && generatedScene.isLoaded)
                    EditorSceneManager.CloseScene(generatedScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[OpenShopSceneBuilder] Created an editable shop scene at " + ScenePath);
        }

        internal static Sprite LoadSprite(string fileName, bool bottomPivot)
        {
            if (SpriteCache.TryGetValue(fileName, out Sprite cached))
                return cached;

            string assetPath = ArtRoot + "/" + fileName;
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                throw new FileNotFoundException("Missing generated sprite: " + assetPath);

            Vector2 pivot = bottomPivot ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            bool changed = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.spritePixelsPerUnit != PixelsPerUnit
                || importer.filterMode != FilterMode.Point
                || importer.textureCompression != TextureImporterCompression.Uncompressed
                || settings.spriteAlignment != (int)SpriteAlignment.Custom
                || settings.spritePivot != pivot
                || !importer.alphaIsTransparency;

            if (changed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                throw new InvalidOperationException("Could not import sprite: " + assetPath);
            SpriteCache[fileName] = sprite;
            return sprite;
        }

        private static Tile CreateTile(string assetName, string spriteName, bool collider)
        {
            string assetPath = TileRoot + "/" + assetName + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = assetName;
                AssetDatabase.CreateAsset(tile, assetPath);
            }

            tile.sprite = LoadSprite(spriteName, false);
            tile.colliderType = collider ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            GameObject layer = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            layer.transform.SetParent(parent, false);
            TilemapRenderer renderer = layer.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            return layer.GetComponent<Tilemap>();
        }

        private static void CreateTilemaps(Tile grassA, Tile grassB, Tile grassFlowers, Tile cobble, Tile wood)
        {
            GameObject gridObject = new GameObject("地面图块 Tilemaps");
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;

            Tilemap ground = CreateTilemap(gridObject.transform, "草地 Ground", -100);
            Tilemap paths = CreateTilemap(gridObject.transform, "石板路 Paths", -90);
            Tilemap shopFloor = CreateTilemap(gridObject.transform, "门前木地板 Shop floor", -80);

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int variant = (x * 17 + y * 31 + x * y) % 29;
                    Tile tile = variant == 0 ? grassFlowers : variant < 9 ? grassB : grassA;
                    ground.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }

            for (int x = 0; x < MapWidth; x++)
            {
                paths.SetTile(new Vector3Int(x, 8, 0), cobble);
                if (x == 4 || x == 5 || x == 24 || x == 25)
                    for (int y = 9; y <= 11; y++)
                        paths.SetTile(new Vector3Int(x, y, 0), cobble);
            }

            for (int y = 0; y < MapHeight; y++)
            {
                paths.SetTile(new Vector3Int(15, y, 0), cobble);
                if (y <= 7)
                    paths.SetTile(new Vector3Int(16, y, 0), cobble);
            }

            for (int y = 4; y <= 8; y++)
            {
                paths.SetTile(new Vector3Int(10, y, 0), cobble);
                paths.SetTile(new Vector3Int(22, y, 0), cobble);
            }

            for (int y = 10; y <= 12; y++)
                for (int x = 2; x <= 6; x++)
                    shopFloor.SetTile(new Vector3Int(x, y, 0), wood);

            ground.CompressBounds();
            paths.CompressBounds();
            shopFloor.CompressBounds();
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(MapWidth * 0.5f, MapHeight * 0.5f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.36f, 0.65f, 0.43f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            ShopCameraFollow follow = cameraObject.AddComponent<ShopCameraFollow>();
            follow.cameraMoveBounds = new Rect(0f, 0f, MapWidth, MapHeight);
            return camera;
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static void PlaceProp(Transform parent, string name, string spriteFile, Vector2 position, Vector2 colliderSize)
        {
            GameObject prop = new GameObject(name);
            prop.transform.SetParent(parent, false);
            prop.transform.position = new Vector3(position.x, position.y, 0f);
            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(spriteFile, true);
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingOrder = SortBase + Mathf.RoundToInt(-position.y * 10f);

            if (colliderSize.x > 0f && colliderSize.y > 0f)
            {
                BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
                collider.size = colliderSize;
                collider.offset = new Vector2(0f, colliderSize.y * 0.5f);
            }
        }

        private static void CreateProps()
        {
            Transform props = CreateGroup("店铺摆设（可自由移动）", null);
            PlaceProp(props, "工坊入口", "prop_workshop.png", new Vector2(4.4f, 12.2f), new Vector2(3.6f, 0.8f));
            PlaceProp(props, "炼金台", "prop_alchemy.png", new Vector2(10.0f, 11.3f), new Vector2(2.4f, 0.65f));
            PlaceProp(props, "露天摊位", "prop_stall.png", new Vector2(25.2f, 11.8f), new Vector2(3.2f, 0.75f));
            PlaceProp(props, "西侧陈列架", "prop_display.png", new Vector2(9.5f, 3.6f), new Vector2(3.8f, 1.8f));
            PlaceProp(props, "东侧陈列架", "prop_display.png", new Vector2(22.0f, 3.6f), new Vector2(3.8f, 1.8f));
            PlaceProp(props, "西侧花园树木", "prop_tree.png", new Vector2(1.6f, 1.6f), new Vector2(0.8f, 0.45f));
            PlaceProp(props, "东侧花园树木", "prop_tree.png", new Vector2(30.2f, 1.6f), new Vector2(0.8f, 0.45f));
            PlaceProp(props, "北侧花园树木", "prop_tree.png", new Vector2(29.4f, 13.0f), new Vector2(0.8f, 0.45f));
            PlaceProp(props, "西侧花盆", "prop_planter.png", new Vector2(7.2f, 4.1f), new Vector2(0.55f, 0.35f));
            PlaceProp(props, "东侧花盆", "prop_planter.png", new Vector2(26.1f, 4.1f), new Vector2(0.55f, 0.35f));
        }

        private static Transform[] CreateRoute(Transform parent, string name, Vector2[] points)
        {
            Transform route = CreateGroup(name, parent);
            Transform[] result = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject waypoint = new GameObject("路点 " + (i + 1).ToString("00"));
                waypoint.transform.SetParent(route, false);
                waypoint.transform.position = new Vector3(points[i].x, points[i].y, 0f);
                waypoint.AddComponent<ShopWaypointGizmo>();
                result[i] = waypoint.transform;
            }
            return result;
        }

        private static Sprite[] LoadVisitorFrames(int visitor, string facing)
        {
            return new[]
            {
                LoadSprite("visitor_" + visitor + "_" + facing + "_0.png", true),
                LoadSprite("visitor_" + visitor + "_" + facing + "_1.png", true)
            };
        }

        internal static GameObject GetOrCreateVisitorPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisitorPrefabPath);
            if (prefab != null)
                return prefab;

            EnsureVisitorPrefabFolders();

            GameObject template = new GameObject("顾客");
            SpriteRenderer renderer = template.AddComponent<SpriteRenderer>();
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sprite = LoadSprite("visitor_0_down_0.png", true);
            renderer.sortingOrder = SortBase;

            ShopVisitorWander wander = template.AddComponent<ShopVisitorWander>();
            wander.spriteRenderer = renderer;
            wander.downFrames = LoadVisitorFrames(0, "down");
            wander.leftFrames = LoadVisitorFrames(0, "left");
            wander.rightFrames = LoadVisitorFrames(0, "right");
            wander.upFrames = LoadVisitorFrames(0, "up");

            prefab = PrefabUtility.SaveAsPrefabAsset(template, VisitorPrefabPath);
            UnityEngine.Object.DestroyImmediate(template);
            if (prefab == null)
                throw new InvalidOperationException("Could not create customer prefab at " + VisitorPrefabPath);

            return prefab;
        }

        internal static GameObject GetOrCreateVisitorPrefab(int appearanceIndex)
        {
            appearanceIndex = Mathf.Clamp(appearanceIndex, 0, 3);
            EnsureCustomerPrefabFolders();

            string prefabPath = CustomerPrefabFolder + "/Visitor_" + appearanceIndex.ToString("00") + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
                return prefab;

            GameObject template = new GameObject("顾客形象 " + (appearanceIndex + 1).ToString("00"));
            SpriteRenderer renderer = template.AddComponent<SpriteRenderer>();
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sprite = LoadSprite("visitor_" + appearanceIndex + "_down_0.png", true);
            renderer.sortingOrder = SortBase;

            ShopVisitorWander wander = template.AddComponent<ShopVisitorWander>();
            wander.spriteRenderer = renderer;
            wander.downFrames = LoadVisitorFrames(appearanceIndex, "down");
            wander.leftFrames = LoadVisitorFrames(appearanceIndex, "left");
            wander.rightFrames = LoadVisitorFrames(appearanceIndex, "right");
            wander.upFrames = LoadVisitorFrames(appearanceIndex, "up");

            ShopCustomerWanderAI ai = template.AddComponent<ShopCustomerWanderAI>();
            ai.animationBinding = wander;
            ai.enabled = false;

            prefab = PrefabUtility.SaveAsPrefabAsset(template, prefabPath);
            UnityEngine.Object.DestroyImmediate(template);
            if (prefab == null)
                throw new InvalidOperationException("Could not create customer appearance prefab at " + prefabPath);
            return prefab;
        }

        internal static GameObject GetOrCreateDisplayCasePrefab()
        {
            EnsureVisitorPrefabFolders();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DisplayCasePrefabPath);
            if (prefab != null)
                return prefab;

            Sprite sprite = LoadSprite("prop_display.png", true);
            GameObject template = new GameObject("木质陈列架");
            SpriteRenderer renderer = template.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingOrder = SortBase;

            BoxCollider2D collider = template.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(3.8f, 1.8f);
            collider.offset = new Vector2(0f, sprite.bounds.center.y);

            Transform browsingPoint = CreatePrefabChild(template.transform, "顾客浏览位置");
            browsingPoint.localPosition = new Vector3(0f, -0.9f, 0f);

            float slotY = sprite.bounds.center.y;
            float slotOffsetX = sprite.bounds.size.x * 0.23f;
            Transform leftSlot = CreatePrefabChild(template.transform, "商品槽位 01");
            leftSlot.localPosition = new Vector3(-slotOffsetX, slotY, 0f);
            SpriteRenderer leftProductRenderer = leftSlot.gameObject.AddComponent<SpriteRenderer>();
            leftProductRenderer.sprite = LoadSprite("display_case_product_potion.png", false);
            leftProductRenderer.enabled = false;
            leftProductRenderer.sortingOrder = SortBase + 1;

            Transform rightSlot = CreatePrefabChild(template.transform, "商品槽位 02");
            rightSlot.localPosition = new Vector3(slotOffsetX, slotY, 0f);
            SpriteRenderer rightProductRenderer = rightSlot.gameObject.AddComponent<SpriteRenderer>();
            rightProductRenderer.sprite = LoadSprite("display_case_product_crystal.png", false);
            rightProductRenderer.enabled = false;
            rightProductRenderer.sortingOrder = SortBase + 1;

            Transform lockedIndicator = CreatePrefabChild(template.transform, "锁定标记");
            lockedIndicator.localPosition = new Vector3(0f, slotY, 0f);
            SpriteRenderer lockedIndicatorRenderer = lockedIndicator.gameObject.AddComponent<SpriteRenderer>();
            lockedIndicatorRenderer.sprite = LoadSprite("display_case_lock.png", false);
            lockedIndicatorRenderer.enabled = false;
            lockedIndicatorRenderer.sortingOrder = SortBase + 1;

            ShopDisplayCaseView view = template.AddComponent<ShopDisplayCaseView>();
            view.configId = 8101;
            view.spriteRenderer = renderer;
            view.blockingCollider = collider;
            view.lockedIndicatorRenderer = lockedIndicatorRenderer;
            view.browsingPoint = browsingPoint;
            view.productSlots = new[] { leftSlot, rightSlot };

            prefab = PrefabUtility.SaveAsPrefabAsset(template, DisplayCasePrefabPath);
            UnityEngine.Object.DestroyImmediate(template);
            if (prefab == null)
                throw new InvalidOperationException("Could not create display case prefab at " + DisplayCasePrefabPath);
            return prefab;
        }

        private static Transform CreatePrefabChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void EnsureCustomerPrefabFolders()
        {
            EnsureVisitorPrefabFolders();
            if (!AssetDatabase.IsValidFolder(CustomerPrefabFolder))
                AssetDatabase.CreateFolder("Assets/Prefabs/OpenShop", "Customers");
        }

        private static void EnsureVisitorPrefabFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/OpenShop"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "OpenShop");
        }

        private static Transform CreateVisitors()
        {
            Transform visitors = CreateGroup("顾客（运行预览）", null);
            Transform routeRoot = CreateGroup("顾客路线（移动圆点调整）", null);

            Transform[] mainRoute = CreateRoute(routeRoot, "A 中央逛店路线", new[]
            {
                new Vector2(7.5f, 8.5f), new Vector2(11.5f, 8.5f), new Vector2(15.5f, 9.5f),
                new Vector2(20f, 8.5f), new Vector2(24.5f, 8.5f), new Vector2(24.5f, 10.5f),
                new Vector2(21.5f, 10.5f), new Vector2(18f, 10f), new Vector2(14f, 10f), new Vector2(9f, 10.5f)
            });
            Transform[] workshopRoute = CreateRoute(routeRoot, "B 工坊门口", new[]
            {
                new Vector2(2f, 8.5f), new Vector2(4.5f, 9.5f), new Vector2(4.5f, 11f),
                new Vector2(7.5f, 10.7f), new Vector2(8.5f, 9.5f), new Vector2(7f, 8.5f)
            });
            Transform[] westDisplayRoute = CreateRoute(routeRoot, "C 西侧陈列架", new[]
            {
                new Vector2(7.5f, 6.8f), new Vector2(8f, 5.8f), new Vector2(11.5f, 5.8f),
                new Vector2(12.5f, 7.5f), new Vector2(15.5f, 8.5f), new Vector2(12f, 9.5f)
            });
            Transform[] eastStallRoute = CreateRoute(routeRoot, "D 东侧摊位", new[]
            {
                new Vector2(28f, 8.5f), new Vector2(28f, 10f), new Vector2(27.5f, 10.7f),
                new Vector2(23f, 10.7f), new Vector2(20.5f, 9.5f), new Vector2(19f, 8.5f)
            });
            Transform[] northRoute = CreateRoute(routeRoot, "E 北侧通道", new[]
            {
                new Vector2(8f, 15f), new Vector2(12f, 15.4f), new Vector2(15.5f, 14.8f),
                new Vector2(19f, 15.4f), new Vector2(22f, 15f), new Vector2(23f, 12f)
            });

            Transform[][] routes = { mainRoute, workshopRoute, westDisplayRoute, eastStallRoute, northRoute, mainRoute };
            for (int i = 0; i < routes.Length; i++)
            {
                GameObject visitorPrefab = GetOrCreateVisitorPrefab(i % 4);
                GameObject visitor = PrefabUtility.InstantiatePrefab(visitorPrefab, visitors.gameObject.scene) as GameObject;
                if (visitor == null)
                    throw new InvalidOperationException("Could not instantiate customer prefab");

                visitor.name = "顾客 " + (i + 1).ToString("00");
                visitor.transform.SetParent(visitors, false);
                visitor.transform.position = routes[i][0].position;
                SpriteRenderer renderer = visitor.GetComponent<SpriteRenderer>();
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;
                renderer.sortingOrder = SortBase + Mathf.RoundToInt(-visitor.transform.position.y * 10f);

                ShopVisitorWander wander = visitor.GetComponent<ShopVisitorWander>();
                wander.spriteRenderer = renderer;
                wander.waypoints = routes[i];
                wander.moveSpeed = 0.65f + (i % 3) * 0.12f;
                wander.pauseAtWaypoints = 0.65f;

                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                PrefabUtility.RecordPrefabInstancePropertyModifications(wander);
            }

            GameObject player = ShopScenePlayerInstaller.CreatePlayerObject(new Vector3(16f, 7f, 0f));
            return player.transform;
        }
    }
}
