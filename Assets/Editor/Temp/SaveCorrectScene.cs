using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveCorrectScene
{
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.Debug.Log($"[SaveScene] Saving: {scene.path}");
        EditorSceneManager.SaveScene(scene, scene.path);
    }
}
