using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
internal static class OpenShopTilePaletteSetup
{
    private const string ScenePath = "Assets/Scenes/OpenShopScene.unity";
    private const string DemoScenePath = "Assets/Scenes/OpenShopScene_MapDemo.unity";
    private const string PalettePath = "Assets/Tilemaps/OpenShop/Tiles/Ground.prefab";
    private const int MapWidth = 32;
    private const int MapHeight = 18;
    private const string GrassBTilePath = "Assets/Tilemaps/OpenShop/Tiles/Grass_B.asset";
    private const string GrassATilePath = "Assets/Tilemaps/OpenShop/Tiles/Grass_A.asset";
    private const string FlowersTilePath = "Assets/Tilemaps/OpenShop/Tiles/Grass_Flowers.asset";
    private const string PathTilePath = "Assets/Tilemaps/OpenShop/Tiles/Cobble_Path.asset";
    private const string WoodTilePath = "Assets/Tilemaps/OpenShop/Tiles/Wood_Floor.asset";

    private static readonly Vector3Int[] PalettePositions =
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(2, 0, 0),
        new Vector3Int(3, 0, 0),
        new Vector3Int(4, 0, 0)
    };

    private static readonly string[] TileAssetPaths =
    {
        GrassBTilePath,
        GrassATilePath,
        FlowersTilePath,
        PathTilePath,
        WoodTilePath
    };

    private static readonly Vector3Int[] DemoFlowerPositions =
    {
        new Vector3Int(3, 4, 0),
        new Vector3Int(7, 2, 0),
        new Vector3Int(11, 3, 0),
        new Vector3Int(27, 3, 0),
        new Vector3Int(29, 6, 0),
        new Vector3Int(8, 15, 0),
        new Vector3Int(12, 16, 0),
        new Vector3Int(20, 15, 0),
        new Vector3Int(25, 16, 0),
        new Vector3Int(4, 6, 0)
    };

    static OpenShopTilePaletteSetup()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string demoTriggerPath = Path.Combine(projectRoot, "Library", "CreateOpenShopMapDemo.once");
        if (File.Exists(demoTriggerPath))
        {
            File.Delete(demoTriggerPath);
            EditorApplication.delayCall += CreateOrOpenMapDemo;
        }

        string triggerPath = Path.Combine(projectRoot, "Library", "PrepareOpenShopTilePalette.once");
        if (!File.Exists(triggerPath))
            return;

        File.Delete(triggerPath);
        EditorApplication.delayCall += Prepare;
    }

    [MenuItem("代号开店/准备地图绘制")]
    private static void PrepareFromMenu()
    {
        Prepare();
    }

    [MenuItem("代号开店/打开绘制示范地图")]
    private static void CreateOrOpenMapDemoFromMenu()
    {
        CreateOrOpenMapDemo();
    }

    private static void CreateOrOpenMapDemo()
    {
        try
        {
            PopulatePalette();
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string demoFullPath = Path.Combine(projectRoot, DemoScenePath.Replace('/', Path.DirectorySeparatorChar));
            Scene activeScene = SceneManager.GetActiveScene();

            if (File.Exists(demoFullPath))
            {
                if (activeScene.path != DemoScenePath)
                {
                    if (!SaveSceneIfDirty(activeScene))
                    {
                        Debug.LogError("当前场景有未保存的修改。请先保存场景，再打开绘制示范地图。");
                        return;
                    }

                    activeScene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
                }

                PrepareDrawingTarget(activeScene);
                Debug.Log("绘制示范地图已打开：选中瓦片后可在 Scene 视图中拖动绘制。");
                return;
            }

            if (activeScene.path != ScenePath)
            {
                if (!SaveSceneIfDirty(activeScene))
                {
                    Debug.LogError("当前场景有未保存的修改。请先保存场景，再创建绘制示范地图。");
                    return;
                }

                activeScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            if (activeScene.isDirty && !EditorSceneManager.SaveScene(activeScene, ScenePath))
                throw new InvalidOperationException("保存 OpenShopScene.unity 失败，无法安全创建示范副本。");

            if (!EditorSceneManager.SaveScene(activeScene, DemoScenePath, true))
                throw new InvalidOperationException("另存示范场景失败：" + DemoScenePath);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Scene demoScene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            DrawDemoMap(demoScene);
            EditorSceneManager.MarkSceneDirty(demoScene);
            if (!EditorSceneManager.SaveScene(demoScene, DemoScenePath))
                throw new InvalidOperationException("保存示范地图失败：" + DemoScenePath);

            AssetDatabase.SaveAssets();
            PrepareDrawingTarget(demoScene);
            Debug.Log("绘制示范地图已创建：包含草地底色、石板主路、店铺木平台和花草点缀。");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void Prepare()
    {
        try
        {
            PopulatePalette();

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath && scene.path != DemoScenePath)
            {
                Debug.LogWarning("瓦片调色板已准备。请打开原场景或绘制示范地图后，再选择“代号开店/准备地图绘制”。");
                return;
            }

            Tilemap target = FindDrawingTarget(scene);
            if (target == null)
            {
                Debug.LogError("没有找到当前场景适用的 Tilemap，无法设为绘制目标。");
                return;
            }

            PrepareDrawingTarget(scene);

            Debug.Log("地图绘制已准备：Ground 调色板已装入 5 种瓦片，绘制目标已选为“" + target.gameObject.name + "”。");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static bool SaveSceneIfDirty(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || !scene.isDirty)
            return true;

        if (string.IsNullOrEmpty(scene.path))
            return false;

        return EditorSceneManager.SaveScene(scene, scene.path);
    }

    private static void PrepareDrawingTarget(Scene scene)
    {
        Tilemap target = FindDrawingTarget(scene);
        if (target == null)
            throw new InvalidOperationException("没有找到当前场景适用的 Tilemap，无法设为绘制目标。");

        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        Selection.activeGameObject = target.gameObject;
        EditorApplication.delayCall += () =>
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
                return;

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                sceneView.Frame(renderer.bounds, false);
            else
                sceneView.FrameSelected();
        };
    }

    private static Tilemap FindDrawingTarget(Scene scene)
    {
        if (scene.path == DemoScenePath)
            return FindTilemap(scene, new[] { "室内木地板 Shop floor", "门前木地板 Shop floor" });

        return FindTilemap(scene, "草地 Ground");
    }

    private static void DrawDemoMap(Scene scene)
    {
        Tilemap ground = FindTilemap(scene, "草地 Ground");
        Tilemap paths = FindTilemap(scene, "石板路 Paths");
        Tilemap shopFloor = FindTilemap(scene, "门前木地板 Shop floor");
        if (ground == null || paths == null || shopFloor == null)
            throw new InvalidOperationException("示范场景缺少草地、石板路或木地板图层。");

        TileBase grassA = LoadTile(GrassATilePath);
        TileBase grassB = LoadTile(GrassBTilePath);
        TileBase flowers = LoadTile(FlowersTilePath);
        TileBase cobble = LoadTile(PathTilePath);
        TileBase wood = LoadTile(WoodTilePath);

        ground.ClearAllTiles();
        paths.ClearAllTiles();
        shopFloor.ClearAllTiles();

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                int variation = (x * 17 + y * 31 + x * y) % 29;
                ground.SetTile(new Vector3Int(x, y, 0), variation < 8 ? grassB : grassA);
            }
        }

        foreach (Vector3Int position in DemoFlowerPositions)
            ground.SetTile(position, flowers);

        DrawHorizontal(paths, cobble, 0, MapWidth - 1, 8);
        DrawHorizontal(paths, cobble, 0, MapWidth - 1, 9);
        DrawVertical(paths, cobble, 15, 0, MapHeight - 1);
        DrawVertical(paths, cobble, 16, 0, MapHeight - 1);
        DrawVertical(paths, cobble, 10, 4, 7);
        DrawVertical(paths, cobble, 22, 4, 7);
        DrawVertical(paths, cobble, 4, 9, 9);
        DrawVertical(paths, cobble, 5, 9, 9);
        DrawVertical(paths, cobble, 24, 9, 9);
        DrawVertical(paths, cobble, 25, 9, 9);

        FillArea(shopFloor, wood, 2, 6, 10, 12);
        FillArea(shopFloor, wood, 23, 27, 10, 12);

        ground.CompressBounds();
        paths.CompressBounds();
        shopFloor.CompressBounds();
    }

    private static TileBase LoadTile(string assetPath)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (tile == null)
            throw new InvalidOperationException("找不到瓦片资源：" + assetPath);

        return tile;
    }

    private static void DrawHorizontal(Tilemap tilemap, TileBase tile, int minX, int maxX, int y)
    {
        for (int x = minX; x <= maxX; x++)
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    private static void DrawVertical(Tilemap tilemap, TileBase tile, int x, int minY, int maxY)
    {
        for (int y = minY; y <= maxY; y++)
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    private static void FillArea(Tilemap tilemap, TileBase tile, int minX, int maxX, int minY, int maxY)
    {
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }

    private static void PopulatePalette()
    {
        GameObject paletteRoot = PrefabUtility.LoadPrefabContents(PalettePath);
        if (paletteRoot == null)
            throw new InvalidOperationException("找不到 Ground 调色板：" + PalettePath);

        try
        {
            Tilemap paletteTilemap = paletteRoot.GetComponentInChildren<Tilemap>(true);
            if (paletteTilemap == null)
                throw new InvalidOperationException("Ground 调色板里没有 Tilemap。");

            for (int i = 0; i < TileAssetPaths.Length; i++)
            {
                TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(TileAssetPaths[i]);
                if (tile == null)
                    throw new InvalidOperationException("找不到瓦片资源：" + TileAssetPaths[i]);

                paletteTilemap.SetTile(PalettePositions[i], tile);
            }

            if (PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePath) == null)
                throw new InvalidOperationException("保存 Ground 调色板失败。");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(paletteRoot);
        }

        AssetDatabase.SaveAssets();
    }

    private static Tilemap FindTilemap(Scene scene, params string[] objectNames)
    {
        foreach (string objectName in objectNames)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Tilemap[] tilemaps = root.GetComponentsInChildren<Tilemap>(true);
                foreach (Tilemap tilemap in tilemaps)
                {
                    if (tilemap.gameObject.name == objectName)
                        return tilemap;
                }
            }
        }

        return null;
    }
}
