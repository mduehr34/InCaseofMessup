using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.UI;

public class WireCampaignSelectScene
{
    public static void Execute()
    {
        // ── Only confirmed CampaignSO in the project ───────────────
        var mockCampaign = AssetDatabase.LoadAssetAtPath<CampaignSO>(
            "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset");

        if (mockCampaign == null)
        {
            Debug.LogError("[WireCampaignSelect] Mock_TutorialCampaign.asset not found — aborting");
            return;
        }
        Debug.Log($"[WireCampaignSelect] Loaded CampaignSO: {mockCampaign.name}");

        // ── Find controller ────────────────────────────────────────
        var controller = Object.FindAnyObjectByType<CampaignSelectController>();
        if (controller == null)
        {
            Debug.LogError("[WireCampaignSelect] CampaignSelectController not found in scene");
            return;
        }

        // ── Assign via SerializedObject ────────────────────────────
        var so = new SerializedObject(controller);

        // _availableCampaigns — one entry: Mock_TutorialCampaign
        var campaignsProp = so.FindProperty("_availableCampaigns");
        campaignsProp.ClearArray();
        campaignsProp.InsertArrayElementAtIndex(0);
        campaignsProp.GetArrayElementAtIndex(0).objectReferenceValue = mockCampaign;

        // _tutorialCampaign — same asset
        var tutorialProp = so.FindProperty("_tutorialCampaign");
        tutorialProp.objectReferenceValue = mockCampaign;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);

        Debug.Log("[WireCampaignSelect] _availableCampaigns[0] = Mock_TutorialCampaign");
        Debug.Log("[WireCampaignSelect] _tutorialCampaign     = Mock_TutorialCampaign");

        // ── Save scene to correct path ─────────────────────────────
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        bool saved = EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/CampaignSelect.unity");
        Debug.Log($"[WireCampaignSelect] Scene saved to correct path: {saved}");

        // Clean up stray duplicate if one appeared
        if (AssetDatabase.AssetPathExists("Assets/CampaignSelect.unity"))
        {
            AssetDatabase.DeleteAsset("Assets/CampaignSelect.unity");
            Debug.Log("[WireCampaignSelect] Deleted stray duplicate.");
        }

        AssetDatabase.Refresh();
        Debug.Log("[WireCampaignSelect] Done.");
    }
}
