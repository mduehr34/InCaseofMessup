using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_FixEventTags
{
    public static void Execute()
    {
        // EVT-01 campaignTag='Year 1' is redundant — yearRange=1-1 already restricts it.
        // Clearing it so MatchesCampaignTag passes (empty = always eligible).
        var evt01 = AssetDatabase.LoadAssetAtPath<EventSO>("Assets/_Game/Data/Events/Event_EVT01.asset");
        if (evt01 != null)
        {
            var so = new SerializedObject(evt01);
            so.FindProperty("campaignTag").stringValue = "";
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(evt01);
            Debug.Log("[Fix] EVT-01 campaignTag cleared.");
        }
        else
            Debug.LogWarning("[Fix] EVT-01.asset not found — check path");

        AssetDatabase.SaveAssets();
    }
}
