using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_FixStandardCrafters
{
    public static void Execute()
    {
        var standard = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Standard.asset");
        if (standard == null) { Debug.LogError("[Fix] Campaign_Standard.asset not found"); return; }

        string[] crafterPaths = {
            "Assets/_Game/Data/Crafters/Crafter_TheOssuary.asset",       // Gaunt  (Year 1)
            "Assets/_Game/Data/Crafters/Crafter_TheCarapaceForge.asset", // Thornback (Year 3)
            "Assets/_Game/Data/Crafters/Crafter_TheIvoryHall.asset",     // Ivory Stampede (Year 5)
            "Assets/_Game/Data/Crafters/Crafter_TheAuricScales.asset",   // Gilded Serpent (Year 8)
            "Assets/_Game/Data/Crafters/Crafter_TheIchorWorks.asset",    // The Spite (Year 12)
        };

        var so = new SerializedObject(standard);
        var crafterPool = so.FindProperty("crafterPool");
        crafterPool.arraySize = crafterPaths.Length;

        for (int i = 0; i < crafterPaths.Length; i++)
        {
            var crafter = AssetDatabase.LoadAssetAtPath<CrafterSO>(crafterPaths[i]);
            if (crafter == null) { Debug.LogWarning($"[Fix] Not found: {crafterPaths[i]}"); continue; }
            crafterPool.GetArrayElementAtIndex(i).objectReferenceValue = crafter;
            Debug.Log($"[Fix] Added to pool: {crafter.crafterName}");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(standard);

        // Fix The Spite: availableFromYear=0 — rely entirely on EVT-21 codex flag
        var spite = AssetDatabase.LoadAssetAtPath<MonsterSO>("Assets/_Game/Data/Monsters/Monster_TheSpite.asset");
        if (spite != null)
        {
            var msо = new SerializedObject(spite);
            msо.FindProperty("availableFromYear").intValue = 0;
            msо.ApplyModifiedProperties();
            EditorUtility.SetDirty(spite);
            Debug.Log("[Fix] Monster_TheSpite.availableFromYear = 0 (gated by EVT-21 only)");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Fix] Campaign_Standard crafterPool updated. Assets saved.");
    }
}
