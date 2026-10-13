using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WelCome.World;

namespace WelCome.EditorTools
{
    [InitializeOnLoad]
    internal static class ShopScenePlayerInstaller
    {
        private const string ScenePath = "Assets/Scenes/OpenShopScene.unity";
        private const string PlayerName = "主控角色（占位）";
        private const string SpawnMarkerName = "主角起点（接入雀或莱亚）";
        private const int SortBase = 300;

        static ShopScenePlayerInstaller()
        {
            EditorApplication.delayCall += InstallIfRequested;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(ProjectRoot, "Library", "InstallOpenShopPlayerRequested");

        private static void InstallIfRequested()
        {
            if (!File.Exists(RequestPath))
                return;

            try
            {
                InstallIntoScene();
                File.Delete(RequestPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        [MenuItem("代号开店/添加可操控主角")]
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
            string absoluteScenePath = Path.Combine(ProjectRoot, ScenePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absoluteScenePath))
                throw new FileNotFoundException("Open shop scene not found", absoluteScenePath);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Scene previousScene = SceneManager.GetActiveScene();
            Scene targetScene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !targetScene.IsValid() || !targetScene.isLoaded;
            if (openedHere)
                targetScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                if (!targetScene.IsValid() || !targetScene.isLoaded)
                    throw new InvalidOperationException("Could not open the shop scene");

                SceneManager.SetActiveScene(targetScene);
                GameObject spawnMarker = FindNamedObject(targetScene, SpawnMarkerName);
                GameObject player = FindNamedObject(targetScene, PlayerName);

                if (player == null)
                {
                    Vector3 spawnPosition = spawnMarker != null
                        ? spawnMarker.transform.position
                        : new Vector3(16f, 7f, 0f);
                    player = CreatePlayerObject(spawnPosition);
                }
                else if (player.GetComponent<ShopPlayerController>() == null)
                {
                    ConfigurePlayerObject(player);
                }

                if (spawnMarker != null)
                    UnityEngine.Object.DestroyImmediate(spawnMarker);

                EnsureBlockerRoot(targetScene);
                ConfigureCamera(targetScene, player.transform);

                EditorSceneManager.MarkSceneDirty(targetScene);
                if (!EditorSceneManager.SaveScene(targetScene, ScenePath))
                    throw new IOException("Could not save the shop scene after adding the player");

                AssetDatabase.SaveAssets();
                Debug.Log("[ShopScenePlayerInstaller] Added a controllable player to " + ScenePath);
            }
            finally
            {
                if (openedHere && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.CloseScene(targetScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
        }

        internal static GameObject CreatePlayerObject(Vector3 position)
        {
            GameObject player = new GameObject(PlayerName);
            player.transform.position = position;
            ConfigurePlayerObject(player);
            return player;
        }

        private static void ConfigurePlayerObject(GameObject player)
        {
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = player.AddComponent<SpriteRenderer>();
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingOrder = SortBase + Mathf.RoundToInt(-player.transform.position.y * 10f);
            renderer.sprite = LoadFrames("up")[0];

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body == null)
                body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.34f, 0.22f);
            collider.offset = new Vector2(0f, 0.13f);

            ShopPlayerController controller = player.GetComponent<ShopPlayerController>();
            if (controller == null)
                controller = player.AddComponent<ShopPlayerController>();
            controller.spriteRenderer = renderer;
            controller.moveSpeed = 3f;
            controller.minBounds = new Vector2(0.3f, 0.1f);
            controller.maxBounds = new Vector2(31.7f, 17.2f);
            controller.reverseFacingSprites = false;
            controller.downFrames = LoadFrames("down");
            controller.leftFrames = LoadFrames("left");
            controller.rightFrames = LoadFrames("right");
            controller.upFrames = LoadFrames("up");
        }

        private static Sprite[] LoadFrames(string direction)
        {
            return new[]
            {
                OpenShopSceneBuilder.LoadSprite("player_" + direction + "_0.png", true),
                OpenShopSceneBuilder.LoadSprite("player_" + direction + "_1.png", true)
            };
        }

        private static GameObject FindNamedObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                    return root;

                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == objectName)
                        return child.gameObject;
                }
            }

            return null;
        }

        private static void EnsureBlockerRoot(Scene scene)
        {
            const string rootName = "自定义阻挡区（添加 BoxCollider2D）";
            if (FindNamedObject(scene, rootName) != null)
                return;

            new GameObject(rootName);
        }

        private static void ConfigureCamera(Scene scene, Transform target)
        {
            Camera sceneCamera = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                sceneCamera = root.GetComponentInChildren<Camera>(true);
                if (sceneCamera != null)
                    break;
            }

            if (sceneCamera == null)
                throw new InvalidOperationException("Main Camera was not found in the shop scene");

            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 7f;
            ShopCameraFollow follow = sceneCamera.GetComponent<ShopCameraFollow>();
            if (follow == null)
                follow = sceneCamera.gameObject.AddComponent<ShopCameraFollow>();
            follow.target = target;
            follow.cameraMoveBounds = new Rect(0f, 0f, 32f, 18f);
        }
    }
}
