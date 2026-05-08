// Stage 8-M: Obsolete — openingCards/escalationCards/apexCards/permanentCards removed from MonsterSO.
// Use the Unity Inspector to verify Gaunt pool arrays and wound deck.
using UnityEngine;
using UnityEditor;

public class Stage7P_CheckGauntSO
{
    public static void Execute()
    {
        Debug.LogWarning("[7P CheckGauntSO] Obsolete since Stage 8-M — " +
                         "Gaunt SO now uses baseCardPool/advancedCardPool/standardWoundDeck. " +
                         "Inspect Mock_GauntStandard.asset in the Unity Editor.");
    }
}
