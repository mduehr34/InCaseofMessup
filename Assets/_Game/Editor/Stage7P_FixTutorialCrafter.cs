using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_FixTutorialCrafter
{
    public static void Execute()
    {
        var tutorial = AssetDatabase.LoadAssetAtPath<CampaignSO>("Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset");
        if (tutorial == null) { Debug.LogError("[Fix] Campaign_Tutorial.asset not found"); return; }

        var ossuary = AssetDatabase.LoadAssetAtPath<CrafterSO>("Assets/_Game/Data/Crafters/Crafter_TheOssuary.asset");
        if (ossuary == null) { Debug.LogError("[Fix] Crafter_TheOssuary.asset not found"); return; }

        var so = new SerializedObject(tutorial);
        var crafterPool = so.FindProperty("crafterPool");
        crafterPool.arraySize = 1;
        crafterPool.GetArrayElementAtIndex(0).objectReferenceValue = ossuary;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(tutorial);
        AssetDatabase.SaveAssets();
        Debug.Log("[Fix] Campaign_Tutorial.crafterPool → [Crafter_TheOssuary]. Saved.");
    }
}
