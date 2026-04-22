using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PlaceAudioManager
{
    public static void Execute()
    {
        // Ensure MainMenu scene is open
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogWarning("[AudioPlace] Active scene is not MainMenu — open it first.");
            return;
        }

        // Remove any existing AudioManager in the scene
        var existing = GameObject.Find("AudioManager");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[AudioPlace] Removed existing AudioManager from scene.");
        }

        // Instantiate prefab
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/AudioManager.prefab");
        if (prefab == null)
        {
            Debug.LogError("[AudioPlace] AudioManager prefab not found at Assets/_Game/Prefabs/AudioManager.prefab");
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        instance.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[AudioPlace] AudioManager placed and scene saved.");
    }
}
