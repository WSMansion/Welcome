using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using WelCome.EditorTools;
using WelCome.World;

[InitializeOnLoad]
internal static class OpenShopInteriorLayoutBuilder
{
    private const string ScenePath = "Assets/Scenes/OpenShopScene_MapDemo.unity";
    private const string ArtRoot = "Assets/Art/OpenShop/";
    private const string GrassAPath = "Assets/Tilemaps/OpenShop/Tiles/Grass_A.asset";
    private const string GrassBPath = "Assets/Tilemaps/OpenShop/Tiles/Grass_B.asset";
    private const string FlowersPath = "Assets/Tilemaps/OpenShop/Tiles/Grass_Flowers.asset";
    private const string CobblePath = "Assets/Tilemaps/OpenShop/Tiles/Cobble_Path.asset";
    private const string WoodPath = "Assets/Tilemaps/OpenShop/Tiles/Wood_Floor.asset";
    private const int MapWidth = 40;
    private const int MapHeight = 26;
    private const int ShopLeft = 4;
    private const int ShopRight = 35;
    private const int ShopFrontY = 9;
    private const int ShopBackY = 23;
    private const int DoorLeftX = 18;
    private const int DoorRightX = 21;
    private const string GroundLayerName = "草地 Ground";
    private const string PathLayerName = "店铺边界与外部石路 Paths";
    private const string FloorLayerName = "室内木地板 Shop floor";

    private static readonly Color WallTint = new Color(0.58f, 0.61f, 0.66f, 1f);
    private static readonly Color PathTint = new Color(0.94f, 0.92f, 0.86f, 1f);

    static OpenShopInteriorLayoutBuilder()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string requestPath = Path.Combine(projectRoot, "Library", "RebuildOpenShopInteriorDemo.once");
        if (!File.Exists(requestPath))
            return;

        File.Delete(requestPath);
        EditorApplication.delayCall += ApplyRequestedLayout;
    }

    [MenuItem("代号开店/重建带外景示范地图")]
    private static void RebuildFromMenu()
    {
        if (!EditorUtility.DisplayDialog(
                "重建带外景示范地图",
                "这会替换示范场景里的地面、摆设、顾客路线和玩家活动范围。",
                "重建",
                "取消"))
            return;

        ApplyLayout();
    }

    private static void ApplyRequestedLayout()
    {
        ApplyLayout();
    }

    private static void ApplyLayout()
    {
        try
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                Debug.LogError("请先打开 Assets/Scenes/OpenShopScene_MapDemo.unity，再运行带外景商店布局。");
                return;
            }

            Tilemap ground = FindTilemap(scene, GroundLayerName);
            Tilemap paths = FindTilemap(scene, PathLayerName, "房间边界与门口 Paths", "石板路 Paths");
            Tilemap floor = FindTilemap(scene, FloorLayerName, "门前木地板 Shop floor");
            Transform propsRoot = FindObject(scene, "店铺摆设（可自由移动）");
            Transform visitorsRoot = FindObject(scene, "顾客（运行预览）");
            Transform routesRoot = FindObject(scene, "顾客路线（移动圆点调整）");
            Transform blockersRoot = FindObject(scene, "自定义阻挡区（添加 BoxCollider2D）");
            Transform player = FindObject(scene, "主控角色（占位）");
            if (ground == null || paths == null || floor == null || propsRoot == null
                || visitorsRoot == null || routesRoot == null || blockersRoot == null || player == null)
                throw new InvalidOperationException("示范场景缺少需要的地面、摆设、顾客、路线、阻挡区或主角对象。");

            RebuildFloor(ground, paths, floor);
            ClearChildren(propsRoot);
            ClearChildren(routesRoot);
            ClearChildren(blockersRoot);
            BuildProps(propsRoot);
            BuildRoutesAndAssignVisitors(routesRoot, visitorsRoot);
            BuildRoomColliders(blockersRoot);
            ConfigurePlayerAndCamera(scene, player);

            paths.gameObject.name = PathLayerName;
            floor.gameObject.name = FloorLayerName;

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("保存带外景商店示范地图失败：" + ScenePath);

            AssetDatabase.SaveAssets();
            PrepareSceneView(ground, floor);
            Debug.Log("带外景商店示范地图已更新：室外石板街与绿化已布置，顾客可从门口进出，玩家活动范围留在店内。");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void RebuildFloor(Tilemap ground, Tilemap paths, Tilemap floor)
    {
        TileBase grassA = LoadTile(GrassAPath);
        TileBase grassB = LoadTile(GrassBPath);
        TileBase flowers = LoadTile(FlowersPath);
        TileBase cobble = LoadTile(CobblePath);
        TileBase wood = LoadTile(WoodPath);

        ground.ClearAllTiles();
        paths.ClearAllTiles();
        floor.ClearAllTiles();

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                int variation = (x * 17 + y * 31 + x * y) % 29;
                ground.SetTile(new Vector3Int(x, y, 0), variation < 8 ? grassB : grassA);
            }
        }

        Vector3Int[] flowerCells =
        {
            new Vector3Int(1, 1, 0), new Vector3Int(3, 2, 0), new Vector3Int(6, 7, 0),
            new Vector3Int(7, 8, 0), new Vector3Int(32, 7, 0), new Vector3Int(33, 8, 0),
            new Vector3Int(38, 1, 0), new Vector3Int(36, 2, 0), new Vector3Int(1, 12, 0),
            new Vector3Int(2, 19, 0), new Vector3Int(37, 14, 0), new Vector3Int(38, 21, 0)
        };
        foreach (Vector3Int flowerCell in flowerCells)
            ground.SetTile(flowerCell, flowers);

        FillPathArea(paths, cobble, 0, MapWidth - 1, 3, 5);
        FillPathArea(paths, cobble, 9, 30, 6, 8);
        FillPathArea(paths, cobble, DoorLeftX, DoorRightX, 5, ShopFrontY);

        FillArea(floor, wood, ShopLeft + 1, ShopRight - 1, ShopFrontY + 1, ShopBackY - 1);
        FillArea(floor, wood, DoorLeftX, DoorRightX, ShopFrontY, ShopFrontY);

        for (int x = ShopLeft; x <= ShopRight; x++)
        {
            if (x < DoorLeftX || x > DoorRightX)
                SetWallTile(paths, cobble, x, ShopFrontY);
        }
        for (int x = ShopLeft; x <= ShopRight; x++)
            SetWallTile(paths, cobble, x, ShopBackY);
        for (int y = ShopFrontY + 1; y < ShopBackY; y++)
        {
            SetWallTile(paths, cobble, ShopLeft, y);
            SetWallTile(paths, cobble, ShopRight, y);
        }

        ground.CompressBounds();
        paths.CompressBounds();
        floor.CompressBounds();
    }

    private static void SetWallTile(Tilemap tilemap, TileBase tile, int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        tilemap.SetTile(position, tile);
        tilemap.SetColor(position, WallTint);
    }

    private static void FillPathArea(Tilemap tilemap, TileBase tile, int minX, int maxX, int minY, int maxY)
    {
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);
                tilemap.SetColor(position, PathTint);
            }
        }
    }

    private static void BuildProps(Transform propsRoot)
    {
        BuildOutdoorProps(propsRoot);

        Transform shelfRoot = CreateGroup(propsRoot, "两侧陈列区（后续陈列物品）");
        Transform leftShelves = CreateGroup(shelfRoot, "左侧展示架");
        Transform rightShelves = CreateGroup(shelfRoot, "右侧展示架");

        float[] leftColumns = { 7.5f, 12.5f };
        float[] rightColumns = { 27.5f, 32.5f };
        float[] rows = { 12.7f, 16.7f, 20.7f };
        int shelfNumber = 1;
        for (int row = 0; row < rows.Length; row++)
        {
            for (int column = 0; column < leftColumns.Length; column++)
            {
                int configId = 8101 + row * 2 + column;
                CreateDisplayCase(leftShelves, "展示架 " + shelfNumber.ToString("00"),
                    new Vector2(leftColumns[column], rows[row]), configId);
                shelfNumber++;
            }
        }

        shelfNumber = 1;
        for (int row = 0; row < rows.Length; row++)
        {
            for (int column = 0; column < rightColumns.Length; column++)
            {
                int configId = 8111 + row * 2 + column;
                CreateDisplayCase(rightShelves, "展示架 " + shelfNumber.ToString("00"),
                    new Vector2(rightColumns[column], rows[row]), configId);
                shelfNumber++;
            }
        }

        Transform counter = CreateGroup(propsRoot, "U 形服务柜台（玩家起点在内）");
        CreateProp(counter, "柜台正面", "prop_stall.png", new Vector2(20f, 18.75f), new Vector2(3.2f, 0.75f));
        CreateProp(counter, "柜台左侧", "prop_display.png", new Vector2(18.95f, 20.15f),
            new Vector2(3.8f, 1.8f), 90f);
        CreateProp(counter, "柜台右侧", "prop_display.png", new Vector2(21.6f, 20.15f),
            new Vector2(3.8f, 1.8f), 90f);
    }

    private static void BuildOutdoorProps(Transform propsRoot)
    {
        Transform outdoor = CreateGroup(propsRoot, "店外街景与绿化（玩家活动范围之外）");
        Transform trees = CreateGroup(outdoor, "店铺两侧的树木");
        CreateProp(trees, "西侧树木", "prop_tree.png", new Vector2(2f, 12.2f),
            new Vector2(1.2f, 0.6f));
        CreateProp(trees, "东侧树木", "prop_tree.png", new Vector2(38f, 12.2f),
            new Vector2(1.2f, 0.6f));

        Transform planters = CreateGroup(outdoor, "门前花盆");
        CreateProp(planters, "左侧花盆", "prop_planter.png", new Vector2(7.5f, 6.8f),
            new Vector2(0.9f, 0.45f));
        CreateProp(planters, "右侧花盆", "prop_planter.png", new Vector2(32.5f, 6.8f),
            new Vector2(0.9f, 0.45f));
    }

    private static void BuildRoutesAndAssignVisitors(Transform routesRoot, Transform visitorsRoot)
    {
        Transform[] doorRoute = CreateRoute(routesRoot, "A 店门外徘徊", new[]
        {
            new Vector2(14f, 7.1f), new Vector2(15.2f, 8.15f), new Vector2(18f, 8.35f),
            new Vector2(22f, 8.35f), new Vector2(24.8f, 8.15f), new Vector2(26f, 7.1f),
            new Vector2(23.8f, 6.45f), new Vector2(16.2f, 6.45f)
        }, new Color(1f, 0.78f, 0.28f, 0.95f));

        Vector2[] westPoints =
        {
            new Vector2(20f, 7.5f), new Vector2(20f, 9.6f), new Vector2(20f, 10.8f),
            new Vector2(16f, 11.8f), new Vector2(12.5f, 11.8f), new Vector2(7.5f, 11.8f),
            new Vector2(12.5f, 11.8f), new Vector2(12.5f, 15.8f), new Vector2(7.5f, 15.8f),
            new Vector2(12.5f, 15.8f), new Vector2(12.5f, 19.8f), new Vector2(7.5f, 19.8f),
            new Vector2(12.5f, 19.8f), new Vector2(16.3f, 19.2f), new Vector2(18.1f, 16.7f),
            new Vector2(18.1f, 13f), new Vector2(20f, 10.8f), new Vector2(20f, 9.6f),
            new Vector2(20f, 7.5f)
        };
        Transform[] westRoute = CreateRoute(routesRoot, "B 西侧陈列架浏览", westPoints,
            new Color(0.32f, 0.82f, 1f, 0.95f));
        Transform[] eastRoute = CreateRoute(routesRoot, "C 东侧陈列架浏览", MirrorRoute(westPoints),
            new Color(1f, 0.55f, 0.36f, 0.95f));

        ShopVisitorWander[] visitors = visitorsRoot.GetComponentsInChildren<ShopVisitorWander>(true);
        if (visitors.Length < 6)
            throw new InvalidOperationException("带外景示范布局需要至少 6 个顾客预览对象。");

        Transform navigationObject = CreateGroup(routesRoot, "顾客寻路与行为");
        ShopCustomerNavigation navigation = navigationObject.gameObject.AddComponent<ShopCustomerNavigation>();
        Vector2[] initialPositions =
        {
            new Vector2(15.5f, 7.1f), new Vector2(24.5f, 7.1f),
            new Vector2(8.5f, 11.8f), new Vector2(31.5f, 11.8f),
            new Vector2(16.5f, 14.5f), new Vector2(23.5f, 14.5f)
        };

        for (int i = 0; i < visitors.Length; i++)
        {
            Transform[] route = i < 2 ? doorRoute : i < 4 ? westRoute : eastRoute;
            ShopVisitorWander visitor = visitors[i];
            visitor.waypoints = route;
            visitor.pauseAtWaypoints = i < 2 ? 1.6f : 1.1f;
            visitor.moveSpeed = 0.7f + (i % 3) * 0.08f;
            visitor.enabled = true;
            visitor.transform.position = initialPositions[i % initialPositions.Length];

            ShopCustomerWanderAI ai = visitor.GetComponent<ShopCustomerWanderAI>();
            if (ai == null)
                ai = visitor.gameObject.AddComponent<ShopCustomerWanderAI>();
            ai.navigation = navigation;
            ai.animationBinding = visitor;
            ai.enabled = true;
            ai.browseChance = i % 3 == 0 ? 0.72f : 0.58f;
            ai.counterChance = 0.16f;
            ai.leaveChance = i % 2 == 0 ? 0.2f : 0.12f;
            ai.outdoorStayChance = i % 2 == 0 ? 0.45f : 0.25f;

            SpriteRenderer renderer = visitor.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 300 + Mathf.RoundToInt(-visitor.transform.position.y * 10f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(visitor);
            PrefabUtility.RecordPrefabInstancePropertyModifications(visitor.transform);
        }
    }

    private static GameObject CreateDisplayCase(Transform parent, string name, Vector2 position, int configId)
    {
        GameObject prefab = OpenShopSceneBuilder.GetOrCreateDisplayCasePrefab();
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("无法创建陈列柜预制体实例。");

        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = new Vector3(position.x, position.y, 0f);
        instance.transform.localRotation = Quaternion.identity;
        ShopDisplayCaseView view = instance.GetComponent<ShopDisplayCaseView>();
        if (view == null)
            throw new InvalidOperationException("陈列柜预制体缺少 ShopDisplayCaseView 组件。");
        view.configId = configId;
        PrefabUtility.RecordPrefabInstancePropertyModifications(view);
        return instance;
    }

    private static Vector2[] MirrorRoute(Vector2[] points)
    {
        Vector2[] result = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
            result[i] = new Vector2(MapWidth - points[i].x, points[i].y);
        return result;
    }

    private static Transform[] CreateRoute(Transform parent, string name, Vector2[] points, Color routeColor)
    {
        Transform routeRoot = CreateGroup(parent, name);
        Transform[] waypoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            GameObject waypoint = new GameObject("路点 " + (i + 1).ToString("00"));
            waypoint.transform.SetParent(routeRoot, false);
            waypoint.transform.localPosition = new Vector3(points[i].x, points[i].y, 0f);
            ShopWaypointGizmo gizmo = waypoint.AddComponent<ShopWaypointGizmo>();
            gizmo.routeColor = routeColor;
            waypoints[i] = waypoint.transform;
        }

        return waypoints;
    }

    private static void BuildRoomColliders(Transform root)
    {
        float wallHeight = ShopBackY - ShopFrontY + 1f;
        float wallCenterY = (ShopFrontY + ShopBackY + 1f) * 0.5f;
        float leftFrontWidth = DoorLeftX - ShopLeft;
        float rightFrontStart = DoorRightX + 1f;
        float rightFrontWidth = ShopRight - DoorRightX;

        CreateWall(root, "北墙", new Vector2((ShopLeft + ShopRight + 1f) * 0.5f, ShopBackY + 0.5f),
            new Vector2(ShopRight - ShopLeft + 1f, 1f));
        CreateWall(root, "西墙", new Vector2(ShopLeft + 0.5f, wallCenterY), new Vector2(1f, wallHeight));
        CreateWall(root, "东墙", new Vector2(ShopRight + 0.5f, wallCenterY), new Vector2(1f, wallHeight));
        CreateWall(root, "南墙（入口左侧）",
            new Vector2((ShopLeft + DoorLeftX) * 0.5f, ShopFrontY + 0.5f),
            new Vector2(leftFrontWidth, 1f));
        CreateWall(root, "南墙（入口右侧）",
            new Vector2((rightFrontStart + ShopRight + 1f) * 0.5f, ShopFrontY + 0.5f),
            new Vector2(rightFrontWidth, 1f));
    }

    private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(name, typeof(BoxCollider2D));
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = new Vector3(position.x, position.y, 0f);
        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static void ConfigurePlayerAndCamera(Scene scene, Transform player)
    {
        player.position = new Vector3(20f, 20.15f, 0f);
        ShopPlayerController controller = player.GetComponent<ShopPlayerController>();
        if (controller == null)
            throw new InvalidOperationException("主控角色缺少 ShopPlayerController。");

        controller.minBounds = new Vector2(ShopLeft + 1.2f, ShopFrontY + 1.15f);
        controller.maxBounds = new Vector2(ShopRight - 0.2f, ShopBackY - 0.75f);
        controller.runSpeedMultiplier = 1.8f;
        controller.reverseFacingSprites = false;
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
            playerRenderer.sortingOrder = 300 + Mathf.RoundToInt(-player.position.y * 10f);
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        PrefabUtility.RecordPrefabInstancePropertyModifications(player);

        Camera sceneCamera = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            sceneCamera = root.GetComponentInChildren<Camera>(true);
            if (sceneCamera != null)
                break;
        }

        if (sceneCamera == null)
            throw new InvalidOperationException("示范场景里没有找到相机。");

        sceneCamera.orthographic = true;
        sceneCamera.orthographicSize = 11f;
        ShopCameraFollow follow = sceneCamera.GetComponent<ShopCameraFollow>();
        if (follow == null)
            follow = sceneCamera.gameObject.AddComponent<ShopCameraFollow>();
        follow.target = player;
        follow.offset = new Vector3(0f, 0f, -10f);
        follow.cameraMoveBounds = new Rect(0f, 0f, MapWidth, MapHeight);
        follow.zoomSpeed = 8f;
        follow.minOrthographicSize = 6f;
        follow.maxOrthographicSize = 13f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(follow);
    }

    private static void PrepareSceneView(Tilemap ground, Tilemap floor)
    {
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        Selection.activeGameObject = floor.gameObject;
        EditorApplication.delayCall += () =>
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (sceneView == null || renderer == null)
                return;

            Bounds bounds = renderer.bounds;
            sceneView.Frame(bounds, false);
            sceneView.pivot = bounds.center;
            sceneView.size = Mathf.Max(bounds.extents.magnitude * 1.1f, 5f);
            sceneView.Repaint();
        };
    }

    private static GameObject CreateProp(Transform parent, string name, string spriteFile, Vector2 position,
        Vector2 colliderSize, float rotation = 0f)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + spriteFile);
        if (sprite == null)
            throw new InvalidOperationException("找不到店铺摆设图片：" + ArtRoot + spriteFile);

        GameObject prop = new GameObject(name);
        prop.transform.SetParent(parent, false);
        prop.transform.localPosition = new Vector3(position.x, position.y, 0f);
        prop.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        renderer.sortingOrder = 300 + Mathf.RoundToInt(-position.y * 10f);

        BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
        collider.size = colliderSize;
        collider.offset = new Vector2(0f, colliderSize.y * 0.5f);
        return prop;
    }

    private static Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static void FillArea(Tilemap tilemap, TileBase tile, int minX, int maxX, int minY, int maxY)
    {
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }

    private static TileBase LoadTile(string assetPath)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (tile == null)
            throw new InvalidOperationException("找不到瓦片资源：" + assetPath);
        return tile;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static Transform FindObject(Scene scene, string objectName)
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

    private static Tilemap FindTilemap(Scene scene, params string[] objectNames)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Tilemap tilemap in root.GetComponentsInChildren<Tilemap>(true))
            {
                for (int i = 0; i < objectNames.Length; i++)
                {
                    if (tilemap.gameObject.name == objectNames[i])
                        return tilemap;
                }
            }
        }

        return null;
    }
}
