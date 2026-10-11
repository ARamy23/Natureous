using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Unity 6 migration: GameAnalytics was removed, so scenes hold missing prefab instances.
public static class RemoveMissingPrefabs
{
    public static void RunOnBuildScenes()
    {
        foreach (var path in EditorBuildSettings.scenes.Select(s => s.path))
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var missing = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .Where(go => PrefabUtility.IsPrefabAssetMissing(go) && PrefabUtility.IsOutermostPrefabInstanceRoot(go))
                .ToList();

            foreach (var go in missing)
            {
                Debug.Log($"[migration] {path}: removing missing prefab instance '{go.name}'");
                Object.DestroyImmediate(go);
            }

            if (missing.Count > 0)
                EditorSceneManager.SaveScene(scene);
        }
    }
}
