using UnityEditor;
using UnityEngine;
using MnM.Core.Data;

public class Stage7P_CheckGauntSO
{
    public static void Execute()
    {
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[Check] Monster_Gaunt.asset not found"); return; }

        Debug.Log($"[Check] Monster_Gaunt: opening={gaunt.openingCards?.Length ?? 0} " +
                  $"escalation={gaunt.escalationCards?.Length ?? 0} " +
                  $"apex={gaunt.apexCards?.Length ?? 0} " +
                  $"permanent={gaunt.permanentCards?.Length ?? 0}");

        if (gaunt.openingCards != null)
            foreach (var c in gaunt.openingCards)
                Debug.Log($"  Opening: {(c != null ? c.cardName : "NULL")}");
    }
}
