using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.UI;

public class AddPauseMenuToSettlement
{
    private const string ScenePath      = "Assets/_Game/Scenes/Settlement.unity";
    private const string UxmlPath       = "Assets/_Game/UI/UXML/pause-menu.uxml";
    private const string PanelPath      = "Assets/_Game/UI/CombatPanelSettings.asset";
    private const string GoName         = "PauseMenuOverlay";

    public static string Execute()
    {
        // Open the correct scene
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Remove any stale duplicate first
        var existing = GameObject.Find(GoName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log($"[Setup] Removed existing {GoName}");
        }

        // Load assets
        var uxml  = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        var panel = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(PanelPath);

        if (uxml  == null) return $"ERROR: UXML not found at {UxmlPath}";
        if (panel == null) return $"ERROR: PanelSettings not found at {PanelPath}";

        // Create GameObject
        var go  = new GameObject(GoName);
        var doc = go.AddComponent<UIDocument>();
        doc.visualTreeAsset = uxml;
        doc.panelSettings   = panel;
        doc.sortingOrder    = 10;

        var ctrl = go.AddComponent<PauseMenuController>();

        // Wire _uiDocument via serialized property
        var so = new SerializedObject(ctrl);
        so.FindProperty("_uiDocument").objectReferenceValue = doc;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(go);
        EditorSceneManager.SaveScene(scene);

        return $"SUCCESS: PauseMenuOverlay added and saved to {ScenePath}";
    }
}
