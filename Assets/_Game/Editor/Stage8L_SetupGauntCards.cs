// Stage 8-M: Obsolete — BehaviorGroup, openingCards/escalationCards, movementPattern/attackTargetType
// fields removed from BehaviorCardSO and MonsterSO. Gaunt cards are now authored as pool arrays
// (baseCardPool, advancedCardPool) via MockDataCreator. Full authored content in Stage 8-N.
using UnityEngine;
using UnityEditor;

public class Stage8L_SetupGauntCards
{
    public static void Execute()
    {
        Debug.LogWarning("[8L SetupGauntCards] Obsolete since Stage 8-M — " +
                         "Gaunt behavior decks now use pool arrays. See MockDataCreator.");
    }
}
