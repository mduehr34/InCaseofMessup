using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class Stage7P_WireCampaignSelect
{
    public static void Execute()
    {
        const string scenePath = "Assets/_Game/Scenes/CampaignSelect.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        GameObject controllerGO = null;
        foreach (var go in roots)
        {
            if (go.GetComponent<CampaignSelectController>() != null) { controllerGO = go; break; }
            foreach (Transform child in go.transform)
                if (child.GetComponent<CampaignSelectController>() != null) { controllerGO = child.gameObject; break; }
        }

        if (controllerGO == null)
        {
            Debug.LogError("[Wire] CampaignSelectController not found in scene");
            return;
        }

        var tutorialSO = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset");
        var standardSO = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Standard.asset");

        if (tutorialSO == null) { Debug.LogError("[Wire] Campaign_Tutorial.asset not found"); return; }
        if (standardSO == null) { Debug.LogError("[Wire] Campaign_Standard.asset not found"); return; }

        var controller = controllerGO.GetComponent<CampaignSelectController>();
        var so = new SerializedObject(controller);

        // _tutorialCampaign
        so.FindProperty("_tutorialCampaign").objectReferenceValue = tutorialSO;

        // _availableCampaigns = [Tutorial, Standard]
        var availProp = so.FindProperty("_availableCampaigns");
        availProp.arraySize = 2;
        availProp.GetArrayElementAtIndex(0).objectReferenceValue = tutorialSO;
        availProp.GetArrayElementAtIndex(1).objectReferenceValue = standardSO;

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Wire] CampaignSelect wired: _tutorialCampaign=Tutorial, _availableCampaigns=[Tutorial, Standard]. Scene saved.");
    }
}
