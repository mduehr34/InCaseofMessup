using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SaveSceneToCorrectPath
{
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"[SaveFix] Active scene: {scene.name} | path: {scene.path}");

        string correctPath = $"Assets/_Game/Scenes/{scene.name}.unity";
        bool saved = EditorSceneManager.SaveScene(scene, correctPath);
        Debug.Log($"[SaveFix] Saved to {correctPath}: {saved}");

        // Clean up any stray duplicate at root
        string strayPath = $"Assets/{scene.name}.unity";
        if (AssetDatabase.AssetPathExists(strayPath))
        {
            AssetDatabase.DeleteAsset(strayPath);
            Debug.Log($"[SaveFix] Deleted stray duplicate: {strayPath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("[SaveFix] Done.");
    }
}
