using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class Stage7P_WireMainMenu
{
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        GameObject mainMenuGO = null;
        foreach (var go in roots)
        {
            if (go.GetComponent<MainMenuController>() != null) { mainMenuGO = go; break; }
            foreach (Transform child in go.transform)
                if (child.GetComponent<MainMenuController>() != null) { mainMenuGO = child.gameObject; break; }
        }

        if (mainMenuGO == null)
        {
            Debug.LogError("[WireMainMenu] MainMenuController not found in scene");
            return;
        }

        var controller = mainMenuGO.GetComponent<MainMenuController>();
        var so = new SerializedObject(controller);

        var allCampaignsProp = so.FindProperty("_allCampaigns");
        allCampaignsProp.arraySize = 2;
        allCampaignsProp.GetArrayElementAtIndex(0).objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset");
        allCampaignsProp.GetArrayElementAtIndex(1).objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Standard.asset");

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[WireMainMenu] _allCampaigns wired: Tutorial + Standard. Scene saved.");
    }
}
