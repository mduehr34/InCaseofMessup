using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MnM.Core.Data;

public class AddTravelEventsToCampaign
{
    public static void Execute()
    {
        // Find all CampaignSO assets in the project
        string[] campaignGuids = AssetDatabase.FindAssets("t:CampaignSO");
        if (campaignGuids.Length == 0)
        {
            Debug.LogWarning("[AddTravelEvents] No CampaignSO assets found");
            return;
        }

        // Load the 3 travel event SOs
        var trv01 = AssetDatabase.LoadAssetAtPath<EventSO>("Assets/_Game/Data/Events/Event_TRV01.asset");
        var trv02 = AssetDatabase.LoadAssetAtPath<EventSO>("Assets/_Game/Data/Events/Event_TRV02.asset");
        var trv03 = AssetDatabase.LoadAssetAtPath<EventSO>("Assets/_Game/Data/Events/Event_TRV03.asset");

        if (trv01 == null || trv02 == null || trv03 == null)
        {
            Debug.LogError("[AddTravelEvents] One or more TRV event assets not found — run CreateTravelEvents first");
            return;
        }

        foreach (string guid in campaignGuids)
        {
            string path     = AssetDatabase.GUIDToAssetPath(guid);
            var    campaign = AssetDatabase.LoadAssetAtPath<CampaignSO>(path);
            if (campaign == null) continue;

            var pool = new List<EventSO>(campaign.eventPool ?? new EventSO[0]);

            bool changed = false;
            foreach (var trv in new[] { trv01, trv02, trv03 })
            {
                if (!pool.Contains(trv))
                {
                    pool.Add(trv);
                    changed = true;
                    Debug.Log($"[AddTravelEvents] Added {trv.eventId} to {campaign.campaignName}");
                }
                else
                {
                    Debug.Log($"[AddTravelEvents] {trv.eventId} already in {campaign.campaignName} — skipping");
                }
            }

            if (changed)
            {
                campaign.eventPool = pool.ToArray();
                EditorUtility.SetDirty(campaign);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AddTravelEvents] Done — all CampaignSO assets updated");
    }
}
