using UnityEditor;
using UnityEngine;

public class InspectCampaignAssets
{
    public static void Execute()
    {
        string[] paths = new[]
        {
            "Assets/_Game/Data/Campaigns/LifeorStrengthTest.asset",
            "Assets/_Game/Data/Campaigns/BloodPriceTest.asset",
            "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset"
        };

        foreach (var path in paths)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null)
                Debug.LogWarning($"[Inspect] NOT FOUND: {path}");
            else
                Debug.Log($"[Inspect] {path} → type: {obj.GetType().FullName}");
        }
    }
}
