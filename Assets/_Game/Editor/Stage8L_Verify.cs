using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class Stage8L_Verify
{
    public static void Execute()
    {
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>(
            "Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[8L Verify] Monster_Gaunt not found"); return; }

        Debug.Log($"[8L Verify] ── Monster_Gaunt Behavior Deck ──────────────────────");
        Debug.Log($"  Opening ({gaunt.openingCards?.Length ?? 0}):");
        if (gaunt.openingCards != null)
            foreach (var c in gaunt.openingCards)
                Debug.Log($"    {c.cardName} | move={c.movementPattern}/{c.movementDistance} | " +
                          $"atk={c.attackTargetType}/{c.attackDamage} | trigger='{c.triggerCondition}'");

        Debug.Log($"  Escalation ({gaunt.escalationCards?.Length ?? 0}):");
        if (gaunt.escalationCards != null)
            foreach (var c in gaunt.escalationCards)
                Debug.Log($"    {c.cardName} | move={c.movementPattern}/{c.movementDistance} | " +
                          $"atk={c.attackTargetType}/{c.attackDamage} | trigger='{c.triggerCondition}'");

        Debug.Log($"  Apex ({gaunt.apexCards?.Length ?? 0}), Permanent ({gaunt.permanentCards?.Length ?? 0})");
        Debug.Log($"[8L Verify] ── Done ─────────────────────────────────────────────");
    }
}
