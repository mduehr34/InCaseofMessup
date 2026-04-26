using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MnM.Core.UI;
using MnM.Core.Systems;

public class Stage7P_WireCombatScene
{
    public static void Execute()
    {
        const string scenePath = "Assets/CombatScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        GameObject combatUI    = null;
        GameObject combatMgrGO = null;
        GameObject gridMgrGO   = null;

        foreach (var go in roots)
        {
            if (go.name == "CombatUI")      combatUI    = go;
            if (go.name == "CombatManager") combatMgrGO = go;
            if (go.name == "GridManager")   gridMgrGO   = go;
        }

        Debug.Log($"[Wire] Scene '{scene.name}' root objects: {roots.Length} — " +
                  $"CombatUI={combatUI != null} CombatManager={combatMgrGO != null} GridManager={gridMgrGO != null}");

        if (combatUI == null)    { Debug.LogError("[Wire] CombatUI not found"); return; }
        if (combatMgrGO == null) { Debug.LogError("[Wire] CombatManager GO not found"); return; }
        if (gridMgrGO == null)   { Debug.LogWarning("[Wire] GridManager GO not found"); }

        var screen      = combatUI.GetComponent<CombatScreenController>();
        var bootstrapper = combatUI.GetComponent<CombatTestBootstrapper>();
        var uiDoc       = combatUI.GetComponent<UnityEngine.UIElements.UIDocument>();
        var combatMgr   = combatMgrGO.GetComponent<CombatManager>();
        var gridMgr     = gridMgrGO?.GetComponent<GridManager>();

        // Wire CombatTestBootstrapper → CombatManager
        if (bootstrapper != null && combatMgr != null)
        {
            var so = new SerializedObject(bootstrapper);
            so.FindProperty("_combatManager").objectReferenceValue = combatMgr;
            so.ApplyModifiedProperties();
            Debug.Log("[Wire] Bootstrapper._combatManager → CombatManager ✓");
        }
        else
            Debug.LogError($"[Wire] bootstrapper={bootstrapper != null} combatMgr={combatMgr != null}");

        // Wire CombatScreenController fields (belt-and-suspenders — may already be set)
        if (screen != null)
        {
            var so = new SerializedObject(screen);

            if (uiDoc != null)
            {
                so.FindProperty("_uiDocument").objectReferenceValue = uiDoc;
                Debug.Log("[Wire] Screen._uiDocument ✓");
            }
            if (combatMgr != null)
            {
                so.FindProperty("_combatManager").objectReferenceValue = combatMgr;
                Debug.Log("[Wire] Screen._combatManager ✓");
            }
            if (gridMgr != null)
            {
                so.FindProperty("_gridManager").objectReferenceValue = gridMgr;
                Debug.Log("[Wire] Screen._gridManager ✓");
            }

            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Wire] CombatScene wired and saved.");
    }
}
