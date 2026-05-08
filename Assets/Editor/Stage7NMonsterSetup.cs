// Stage 8-M: Obsolete — BehaviorGroup, MonsterBodyPart, FacingTable, behaviorDeckSizeRemovable,
// openingCards/escalationCards/apexCards/permanentCards, standardParts/hardenedParts/apexParts,
// frontFacing/flankFacing/rearFacing, and trapZoneParts all removed from MonsterSO/DataStructs.
// Monster data will be re-authored in the new pool/wound-deck format in Stage 8-N.
using UnityEngine;
using UnityEditor;

public class Stage7NMonsterSetup
{
    public static void Execute()
    {
        Debug.LogWarning("[7N MonsterSetup] Obsolete since Stage 8-M — " +
                         "monster SO authoring moved to pool arrays + WoundLocationSO wound decks. " +
                         "Re-author monster assets in Stage 8-N.");
    }
}
