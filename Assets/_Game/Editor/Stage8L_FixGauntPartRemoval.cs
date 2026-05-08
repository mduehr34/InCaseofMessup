// Stage 8-M: Obsolete — standardParts and breakRemovesCardNames removed from MonsterSO/MonsterBodyPart.
// Wound-triggered card removal is now handled by WoundLocationSO entries in standardWoundDeck[].
using UnityEngine;
using UnityEditor;

public class Stage8L_FixGauntPartRemoval
{
    public static void Execute()
    {
        Debug.LogWarning("[8L FixGauntPartRemoval] Obsolete since Stage 8-M — " +
                         "Part-break card removal replaced by WoundLocationSO wound deck system.");
    }
}
