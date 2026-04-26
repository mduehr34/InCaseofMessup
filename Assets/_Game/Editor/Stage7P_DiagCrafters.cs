using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_DiagCrafters
{
    public static void Execute()
    {
        string[] paths = {
            "Assets/_Game/Data/Crafters/Crafter_TheOssuary.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheCarapaceForge.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheIvoryHall.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheAuricScales.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheMireApothecary.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheIchorWorks.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheMembraneLoft.asset",
            "Assets/_Game/Data/Crafters/Crafter_TheRotGarden.asset",
        };

        foreach (var path in paths)
        {
            var c = AssetDatabase.LoadAssetAtPath<CrafterSO>(path);
            if (c == null) { Debug.LogWarning($"[Diag] Not found: {path}"); continue; }
            Debug.Log($"[Diag] {c.crafterName} | monsterTag='{c.monsterTag}' | tier={c.materialTier} | unlockCost={c.unlockCost?.Length ?? 0} entries");
        }
    }
}
