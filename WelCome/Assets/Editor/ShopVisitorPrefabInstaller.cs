using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WelCome.World;

namespace WelCome.EditorTools
{
    [InitializeOnLoad]
    internal static class ShopVisitorPrefabInstaller
    {
        private const string ScenePath = "Assets/Scenes/OpenShopScene.unity";
        private const string VisitorsRootName = "顾客（运行预览）";

        static ShopVisitorPrefabInstaller()
        {
            EditorApplication.delayCall += InstallIfRequested;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(ProjectRoot, "Library", "InstallOpenShopVisitorsPrefabRequested");

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

        [MenuItem("代号开店/整理顾客预制体")]
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
                GameObject visitorsRoot = FindNamedObject(targetScene, VisitorsRootName);
                if (visitorsRoot == null)
                    throw new InvalidOperationException("Could not find the customer group in the shop scene");

                List<VisitorSnapshot> snapshots = new List<VisitorSnapshot>();
                foreach (ShopVisitorWander wander in visitorsRoot.GetComponentsInChildren<ShopVisitorWander>(true))
                {
                    if (wander.gameObject != visitorsRoot)
                        snapshots.Add(new VisitorSnapshot(wander));
                }

                if (snapshots.Count == 0)
                    throw new InvalidOperationException("No customer preview objects were found in the shop scene");

                snapshots.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OpenShopSceneBuilder.VisitorPrefabPath);
                if (prefab == null)
                    prefab = CreatePrefabFrom(snapshots[0]);

                int convertedCount = 0;
                foreach (VisitorSnapshot snapshot in snapshots)
                {
                    if (PrefabUtility.GetCorrespondingObjectFromSource(snapshot.Source) == prefab)
                        continue;

                    ReplaceWithPrefabInstance(snapshot, prefab, targetScene);
                    convertedCount++;
                }

                if (convertedCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(targetScene);
                    if (!EditorSceneManager.SaveScene(targetScene, ScenePath))
                        throw new IOException("Could not save the shop scene after converting customers to prefabs");
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"[ShopVisitorPrefabInstaller] Customer prefab ready; converted {convertedCount} scene object(s): {OpenShopSceneBuilder.VisitorPrefabPath}");
            }
            finally
            {
                if (openedHere && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.CloseScene(targetScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
        }

        private static GameObject CreatePrefabFrom(VisitorSnapshot snapshot)
        {
            string prefabPath = OpenShopSceneBuilder.VisitorPrefabPath;
            EnsurePrefabFolders();

            string originalName = snapshot.Source.name;
            Transform[] originalWaypoints = snapshot.Waypoints;
            GameObject prefab;
            try
            {
                snapshot.Source.name = "顾客";
                snapshot.Wander.waypoints = Array.Empty<Transform>();
                prefab = PrefabUtility.SaveAsPrefabAsset(snapshot.Source, prefabPath);
            }
            finally
            {
                snapshot.Source.name = originalName;
                snapshot.Wander.waypoints = originalWaypoints;
            }

            if (prefab == null)
                throw new InvalidOperationException("Could not create customer prefab at " + prefabPath);

            AssetDatabase.SaveAssets();
            return prefab;
        }

        private static void EnsurePrefabFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/OpenShop"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "OpenShop");
        }

        private static void ReplaceWithPrefabInstance(VisitorSnapshot snapshot, GameObject prefab, Scene scene)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate customer prefab");

            instance.name = snapshot.Name;
            instance.transform.SetParent(snapshot.Parent, false);
            instance.transform.localPosition = snapshot.LocalPosition;
            instance.transform.localRotation = snapshot.LocalRotation;
            instance.transform.localScale = snapshot.LocalScale;
            if (snapshot.Parent != null)
                instance.transform.SetSiblingIndex(snapshot.SiblingIndex);

            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            ShopVisitorWander wander = instance.GetComponent<ShopVisitorWander>();
            if (renderer == null || wander == null)
                throw new InvalidOperationException("Customer prefab is missing its renderer or movement component");

            renderer.sprite = snapshot.Sprite;
            renderer.sortingLayerID = snapshot.SortingLayerId;
            renderer.sortingOrder = snapshot.SortingOrder;
            renderer.spriteSortPoint = snapshot.SpriteSortPoint;
            renderer.color = snapshot.Color;
            renderer.flipX = snapshot.FlipX;
            renderer.flipY = snapshot.FlipY;
            renderer.enabled = snapshot.RendererEnabled;

            wander.spriteRenderer = renderer;
            wander.waypoints = snapshot.Waypoints;
            wander.moveSpeed = snapshot.MoveSpeed;
            wander.pauseAtWaypoints = snapshot.PauseAtWaypoints;
            wander.downFrames = snapshot.DownFrames;
            wander.leftFrames = snapshot.LeftFrames;
            wander.rightFrames = snapshot.RightFrames;
            wander.upFrames = snapshot.UpFrames;
            wander.enabled = snapshot.WanderEnabled;

            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            PrefabUtility.RecordPrefabInstancePropertyModifications(wander);
            UnityEngine.Object.DestroyImmediate(snapshot.Source);
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

        private sealed class VisitorSnapshot
        {
            public readonly GameObject Source;
            public readonly string Name;
            public readonly Transform Parent;
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;
            public readonly int SiblingIndex;
            public readonly Sprite Sprite;
            public readonly int SortingLayerId;
            public readonly int SortingOrder;
            public readonly SpriteSortPoint SpriteSortPoint;
            public readonly Color Color;
            public readonly bool FlipX;
            public readonly bool FlipY;
            public readonly bool RendererEnabled;
            public readonly Transform[] Waypoints;
            public readonly float MoveSpeed;
            public readonly float PauseAtWaypoints;
            public readonly Sprite[] DownFrames;
            public readonly Sprite[] LeftFrames;
            public readonly Sprite[] RightFrames;
            public readonly Sprite[] UpFrames;
            public readonly bool WanderEnabled;
            public readonly ShopVisitorWander Wander;

            public VisitorSnapshot(ShopVisitorWander wander)
            {
                Source = wander.gameObject;
                Name = Source.name;
                Parent = Source.transform.parent;
                LocalPosition = Source.transform.localPosition;
                LocalRotation = Source.transform.localRotation;
                LocalScale = Source.transform.localScale;
                SiblingIndex = Source.transform.GetSiblingIndex();
                Wander = wander;
                Waypoints = wander.waypoints;
                MoveSpeed = wander.moveSpeed;
                PauseAtWaypoints = wander.pauseAtWaypoints;
                DownFrames = wander.downFrames;
                LeftFrames = wander.leftFrames;
                RightFrames = wander.rightFrames;
                UpFrames = wander.upFrames;
                WanderEnabled = wander.enabled;

                SpriteRenderer renderer = wander.spriteRenderer != null
                    ? wander.spriteRenderer
                    : Source.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Sprite = renderer.sprite;
                    SortingLayerId = renderer.sortingLayerID;
                    SortingOrder = renderer.sortingOrder;
                    SpriteSortPoint = renderer.spriteSortPoint;
                    Color = renderer.color;
                    FlipX = renderer.flipX;
                    FlipY = renderer.flipY;
                    RendererEnabled = renderer.enabled;
                }
            }
        }
    }
}
