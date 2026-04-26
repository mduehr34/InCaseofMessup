using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class Stage7P_WireBootstrapper
{
    public static void Execute()
    {
        const string scenePath = "Assets/CombatScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        GameObject combatUI = null;
        foreach (var go in roots)
            if (go.name == "CombatUI") { combatUI = go; break; }

        if (combatUI == null) { Debug.LogError("[Wire] CombatUI not found"); return; }

        var bootstrapper = combatUI.GetComponent<CombatTestBootstrapper>();
        if (bootstrapper == null) { Debug.LogError("[Wire] CombatTestBootstrapper not found on CombatUI"); return; }

        var gauntSO = AssetDatabase.LoadAssetAtPath<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gauntSO == null) { Debug.LogError("[Wire] Monster_Gaunt.asset not found"); return; }

        var so = new SerializedObject(bootstrapper);
        so.FindProperty("_mockMonsterSO").objectReferenceValue = gauntSO;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Wire] Bootstrapper._mockMonsterSO → Monster_Gaunt. CombatScene saved.");
    }
}
