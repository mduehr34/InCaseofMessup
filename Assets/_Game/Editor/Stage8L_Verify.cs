// Stage 8-M: Obsolete — openingCards/escalationCards/apexCards/permanentCards removed from MonsterSO.
// Use the Unity Inspector to verify Gaunt's baseCardPool, advancedCardPool, and deckCompositions.
using UnityEngine;
using UnityEditor;

public class Stage8L_Verify
{
    public static void Execute()
    {
        Debug.LogWarning("[8L Verify] Obsolete since Stage 8-M — " +
                         "Gaunt uses pool arrays, not opening/escalation/apex/permanent decks. " +
                         "Inspect Mock_GauntStandard.asset in the Unity Editor to verify.");
    }
}
