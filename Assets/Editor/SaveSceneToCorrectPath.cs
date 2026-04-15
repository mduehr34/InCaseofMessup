using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SaveSceneToCorrectPath
{
    public static void Execute()
    {
        // Save current scene to the correct path
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"[SaveFix] Active scene path: {scene.path}");

        // Save to the correct location
        bool saved = EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/MainMenu.unity");
        Debug.Log($"[SaveFix] Saved to Assets/_Game/Scenes/MainMenu.unity: {saved}");

        // Delete the duplicate that got created at the wrong path
        if (AssetDatabase.AssetPathExists("Assets/MainMenu.unity"))
        {
            AssetDatabase.DeleteAsset("Assets/MainMenu.unity");
            Debug.Log("[SaveFix] Deleted duplicate Assets/MainMenu.unity");
        }

        AssetDatabase.Refresh();
        Debug.Log("[SaveFix] Done.");
    }
}
