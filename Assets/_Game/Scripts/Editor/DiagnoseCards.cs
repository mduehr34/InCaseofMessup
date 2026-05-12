using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class DiagnoseCards
{
    public static void Execute()
    {
        // ── Action card names in registry ──────────────────────────
        var reg = Resources.Load<ActionCardRegistrySO>("ActionCardRegistry");
        if (reg == null) { Debug.LogError("[Diag] ActionCardRegistry not found"); }
        else
        {
            var names = new System.Text.StringBuilder("[Diag] Registered card names:\n");
            if (reg.cards != null)
                foreach (var c in reg.cards)
                    if (c != null) names.AppendLine($"  '{c.cardName}'");
            Debug.Log(names.ToString());
        }

        // ── Monster_Gaunt SO pool state ────────────────────────────
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>("Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null) { Debug.LogError("[Diag] Monster_Gaunt.asset not found"); return; }

        Debug.Log($"[Diag] Monster_Gaunt: " +
                  $"deckCompositions={gaunt.deckCompositions?.Length ?? 0} " +
                  $"baseCardPool={gaunt.baseCardPool?.Length ?? 0} " +
                  $"advancedCardPool={gaunt.advancedCardPool?.Length ?? 0}");

        // ── Fix: assign all behaviour cards as the base pool ──────
        var cards = AssetDatabase.FindAssets("t:BehaviorCardSO", new[] { "Assets/_Game/Data/Cards/Behavior/Gaunt" });
        var pool  = new BehaviorCardSO[cards.Length];
        for (int i = 0; i < cards.Length; i++)
            pool[i] = AssetDatabase.LoadAssetAtPath<BehaviorCardSO>(AssetDatabase.GUIDToAssetPath(cards[i]));

        var so = new SerializedObject(gaunt);
        var poolProp = so.FindProperty("baseCardPool");
        poolProp.arraySize = pool.Length;
        for (int i = 0; i < pool.Length; i++)
            poolProp.GetArrayElementAtIndex(i).objectReferenceValue = pool[i];
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        Debug.Log($"[Diag] Monster_Gaunt baseCardPool set to {pool.Length} Gaunt behavior cards");
    }
}
