// Stage 8-M: Obsolete — standardParts/openingCards/escalationCards removed from MonsterSO.
// Gaunt wound locations are now WoundLocationSO assets in standardWoundDeck[].
using UnityEngine;
using UnityEditor;

public class Stage8L_InspectGauntParts
{
    public static void Execute()
    {
        Debug.LogWarning("[8L InspectGauntParts] Obsolete since Stage 8-M — " +
                         "Monster body parts removed. Inspect standardWoundDeck on Monster_Gaunt.asset instead.");
    }
}
