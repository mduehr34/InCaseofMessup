using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class Stage8L_InspectGauntParts
{
    public static void Execute()
    {
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>(
            "Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[Inspect] Monster_Gaunt not found"); return; }

        Debug.Log($"[Inspect] standardParts count: {gaunt.standardParts?.Length ?? 0}");
        if (gaunt.standardParts != null)
            foreach (var p in gaunt.standardParts)
                Debug.Log($"  Part: '{p.partName}' shell={p.shellDurability} flesh={p.fleshDurability} " +
                          $"break=[{string.Join(",", p.breakRemovesCardNames ?? new string[0])}] " +
                          $"wound=[{string.Join(",", p.woundRemovesCardNames ?? new string[0])}]");

        Debug.Log($"[Inspect] openingCards: [{string.Join(", ", System.Array.ConvertAll(gaunt.openingCards ?? new BehaviorCardSO[0], c => c?.cardName ?? "null"))}]");
        Debug.Log($"[Inspect] escalationCards: [{string.Join(", ", System.Array.ConvertAll(gaunt.escalationCards ?? new BehaviorCardSO[0], c => c?.cardName ?? "null"))}]");
    }
}
