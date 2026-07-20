using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CleanMissingScripts
{
    [MenuItem("Tools/PRU213 Setup/Remove Missing Scripts From Open Scene", priority = 20)]
    public static void RemoveMissingScriptsFromActiveScene()
    {
        int totalRemoved = CleanActiveScene();
        if (totalRemoved > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log($"<color=lime>✅ Successfully removed {totalRemoved} missing script component(s) from active scene!</color>");
        }
        else
        {
            Debug.Log("CleanMissingScripts: No missing scripts found in active scene.");
        }
    }

    [MenuItem("Tools/PRU213 Setup/Remove Missing Scripts From ALL Scenes", priority = 21)]
    public static void RemoveMissingScriptsFromAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int grandTotal = 0;

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int removedInScene = CleanActiveScene();

            if (removedInScene > 0)
            {
                grandTotal += removedInScene;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"<color=yellow>Cleaned {removedInScene} missing script(s) in {Path.GetFileName(scenePath)}</color>");
            }
        }

        Debug.Log($"<color=lime>🎉 Done cleaning all scenes! Removed a total of {grandTotal} missing script components.</color>");
    }

    private static int CleanActiveScene()
    {
        int totalRemoved = 0;
        GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            totalRemoved += RemoveMissingScriptsRecursively(root);
        }

        return totalRemoved;
    }

    private static int RemoveMissingScriptsRecursively(GameObject go)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        foreach (Transform child in go.transform)
        {
            count += RemoveMissingScriptsRecursively(child.gameObject);
        }

        return count;
    }
}
