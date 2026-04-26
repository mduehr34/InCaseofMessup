using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_DiagEvents
{
    public static void Execute()
    {
        var tutorial = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset");
        if (tutorial == null) { Debug.LogError("[Diag] Campaign_Tutorial.asset not found"); return; }

        Debug.Log($"[Diag] Tutorial eventPool size: {tutorial.eventPool?.Length ?? 0}");

        if (tutorial.eventPool == null) return;

        foreach (var evt in tutorial.eventPool)
        {
            if (evt == null) { Debug.LogWarning("[Diag] null entry in eventPool"); continue; }
            Debug.Log($"[Diag] Pool: {evt.eventId} '{evt.eventName}' " +
                      $"years={evt.yearRangeMin}-{evt.yearRangeMax} " +
                      $"mandatory={evt.isMandatory} " +
                      $"campaignTag='{evt.campaignTag}' " +
                      $"monsterTag='{evt.monsterTag}'");
        }
    }
}
