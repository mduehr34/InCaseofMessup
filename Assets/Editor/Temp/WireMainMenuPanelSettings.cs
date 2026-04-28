using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class WireMainMenuPanelSettings
{
    public static void Execute()
    {
        // Find the UIDocument GO in the open scene
        var uiDocGO = GameObject.Find("UIDocument");
        if (uiDocGO == null) { Debug.LogError("[Wire] UIDocument GameObject not found in scene."); return; }

        var uiDoc = uiDocGO.GetComponent<UIDocument>();
        if (uiDoc == null) { Debug.LogError("[Wire] UIDocument component not found."); return; }

        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/_Game/UI/MainMenuPanelSettings.asset");
        if (ps == null) { Debug.LogError("[Wire] MainMenuPanelSettings not found."); return; }

        uiDoc.panelSettings = ps;
        EditorUtility.SetDirty(uiDocGO);

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Wire] UIDocument now uses MainMenuPanelSettings. Scene saved.");
    }
}
