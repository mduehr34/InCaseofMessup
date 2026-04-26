using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_FixStandardCampaign
{
    public static void Execute()
    {
        // ── Fix Campaign_Standard crafterPool ────────────────────
        var standard = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Standard.asset");
        var ossuary  = AssetDatabase.LoadAssetAtPath<CrafterSO>("Assets/_Game/Data/Crafters/Crafter_TheOssuary.asset");

        if (standard == null) { Debug.LogError("[Fix] Campaign_Standard.asset not found"); return; }
        if (ossuary  == null) { Debug.LogError("[Fix] Crafter_TheOssuary.asset not found"); return; }

        var so = new SerializedObject(standard);
        var crafterPool = so.FindProperty("crafterPool");
        crafterPool.arraySize = 1;
        crafterPool.GetArrayElementAtIndex(0).objectReferenceValue = ossuary;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(standard);
        Debug.Log("[Fix] Campaign_Standard.crafterPool → [Crafter_TheOssuary]");

        // ── Set availableFromYear on each monster SO ──────────────
        SetYear("Assets/_Game/Data/Monsters/Monster_Gaunt.asset",           1);
        SetYear("Assets/_Game/Data/Monsters/Monster_Thornback.asset",       3);
        SetYear("Assets/_Game/Data/Monsters/Monster_TheIvoryStampede.asset",5);
        SetYear("Assets/_Game/Data/Monsters/Monster_GildedSerpent.asset",   8);
        SetYear("Assets/_Game/Data/Monsters/Monster_TheSpite.asset",       12);

        AssetDatabase.SaveAssets();
        Debug.Log("[Fix] All monster availableFromYear values set. Assets saved.");
    }

    static void SetYear(string path, int year)
    {
        var monster = AssetDatabase.LoadAssetAtPath<MonsterSO>(path);
        if (monster == null) { Debug.LogWarning($"[Fix] Not found: {path}"); return; }
        var so = new SerializedObject(monster);
        so.FindProperty("availableFromYear").intValue = year;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(monster);
        Debug.Log($"[Fix] {monster.monsterName}.availableFromYear = {year}");
    }
}
