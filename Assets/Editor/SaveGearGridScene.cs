using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveGearGridScene
{
    public static void Execute()
    {
        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene(),
            "Assets/_Game/Scenes/GearGrid.unity");
    }
}
