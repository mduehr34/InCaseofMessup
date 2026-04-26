using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_Verify
{
    public static void Execute()
    {
        var tut = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset");
        var std = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Standard.asset");

        Debug.Log("=== TUTORIAL CAMPAIGN ===");
        Debug.Log($"  Name:            {tut.campaignName}");
        Debug.Log($"  Length:          {tut.campaignLengthYears} years");
        Debug.Log($"  MonsterRoster:   {tut.monsterRoster.Length} (expected 1)");
        Debug.Log($"  EventPool:       {tut.eventPool.Length} (expected 5)");
        Debug.Log($"  Innovations:     {tut.startingInnovations.Length} (expected 3)");
        Debug.Log($"  Crafters:        {tut.crafterPool.Length} (expected 1)");
        Debug.Log($"  GuidingPrincipals: {tut.guidingPrincipals.Length} (expected 1)");
        Debug.Log($"  OverlordSchedule: {tut.overlordSchedule.Length} (expected 0)");

        foreach (var m in tut.monsterRoster)
            Debug.Log($"    Monster: {(m != null ? m.monsterName : "NULL")}");
        foreach (var e in tut.eventPool)
            Debug.Log($"    Event: {(e != null ? e.eventId : "NULL")}");

        // Check EVT-04 is NOT in Tutorial
        bool hasBadEvent = false;
        foreach (var e in tut.eventPool)
            if (e != null && e.eventId == "EVT-04") hasBadEvent = true;
        Debug.Log($"  EVT-04 excluded: {!hasBadEvent}");

        Debug.Log("=== STANDARD CAMPAIGN ===");
        Debug.Log($"  Name:            {std.campaignName}");
        Debug.Log($"  Length:          {std.campaignLengthYears} years");
        Debug.Log($"  MonsterRoster:   {std.monsterRoster.Length} (expected 5, no Suture)");
        Debug.Log($"  EventPool:       {std.eventPool.Length} (expected 30)");
        Debug.Log($"  Innovations:     {std.startingInnovations.Length} (expected 6)");
        Debug.Log($"  Crafters:        {std.crafterPool.Length} (expected 1)");
        Debug.Log($"  GuidingPrincipals: {std.guidingPrincipals.Length} (expected 5)");
        Debug.Log($"  OverlordSchedule: {std.overlordSchedule.Length} (expected 4)");

        foreach (var m in std.monsterRoster)
            Debug.Log($"    Monster: {(m != null ? m.monsterName : "NULL")}");

        bool hassuture = false;
        foreach (var m in std.monsterRoster)
            if (m != null && m.monsterName == "The Suture") hassuture = true;
        Debug.Log($"  Suture excluded from roster: {!hassuture}");

        foreach (var o in std.overlordSchedule)
            Debug.Log($"    Overlord Y{o.arrivalYear}: {(o.overlordMonster != null ? o.overlordMonster.monsterName : "NULL")}, approach: [{string.Join(", ", o.approachYears)}]");

        Debug.Log("=== VERIFY COMPLETE ===");
    }
}
