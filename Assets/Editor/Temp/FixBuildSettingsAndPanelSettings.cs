using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FixBuildSettingsAndPanelSettings
{
    public static void Execute()
    {
        FixBuildSettings();
        CreateMainMenuPanelSettings();
    }

    static void FixBuildSettings()
    {
        var current = EditorBuildSettings.scenes;

        // Build new list: MainMenu first, then all others except SampleScene
        var newList = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        // Find MainMenu entry
        EditorBuildSettingsScene mainMenu = null;
        foreach (var s in current)
            if (s.path.Contains("MainMenu")) { mainMenu = s; break; }

        if (mainMenu == null) { Debug.LogError("[BuildFix] MainMenu scene not found!"); return; }
        mainMenu.enabled = true;
        newList.Add(mainMenu);

        // Add all others except SampleScene and MainMenu
        foreach (var s in current)
        {
            if (s.path.Contains("SampleScene")) continue;
            if (s.path.Contains("MainMenu"))    continue;
            newList.Add(s);
        }

        EditorBuildSettings.scenes = newList.ToArray();

        Debug.Log($"[BuildFix] Build settings updated. MainMenu is now index 0. Total: {newList.Count} scenes");
        for (int i = 0; i < newList.Count; i++)
            Debug.Log($"[BuildFix] [{i}] {newList[i].path}");
    }

    static void CreateMainMenuPanelSettings()
    {
        string path = "Assets/_Game/UI/MainMenuPanelSettings.asset";

        // Check if it already exists
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
        if (existing != null)
        {
            Debug.Log("[PanelSettings] MainMenuPanelSettings already exists.");
            return;
        }

        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1280, 720);

        AssetDatabase.CreateAsset(ps, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PanelSettings] Created MainMenuPanelSettings at {path}");
    }
}
