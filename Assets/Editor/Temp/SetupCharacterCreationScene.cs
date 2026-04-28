using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SetupCharacterCreationScene
{
    public static void Execute()
    {
        // ── PanelSettings ────────────────────────────────────────
        const string psPath = "Assets/_Game/UI/CharacterCreationPanelSettings.asset";
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(psPath);
        if (ps == null)
        {
            ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            AssetDatabase.CreateAsset(ps, psPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup8C] Created CharacterCreationPanelSettings");
        }

        // ── UIManager GameObject ─────────────────────────────────
        var existing = GameObject.Find("UIManager");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var go = new GameObject("UIManager");

        // UIDocument
        var uid = go.AddComponent<UIDocument>();
        uid.panelSettings = ps;

        var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/_Game/UI/UXML/character-creation.uxml");
        if (uxmlAsset != null)
            uid.visualTreeAsset = uxmlAsset;
        else
            Debug.LogWarning("[Setup8C] character-creation.uxml not found");

        // CharacterCreationController
        var ctrl = go.AddComponent<MnM.Core.UI.CharacterCreationController>();

        // Assign _uiDocument via SerializedObject
        var so = new SerializedObject(ctrl);
        so.FindProperty("_uiDocument").objectReferenceValue = uid;
        so.ApplyModifiedProperties();

        // ── Save scene ───────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Setup8C] CharacterCreation scene wired successfully");
    }
}
