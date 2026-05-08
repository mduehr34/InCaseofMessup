// Stage 8-M: Obsolete — BehaviorGroup and BehaviorCardType.Permanent removed.
// Spite card designs preserved in git history; will be re-authored with new sub-phase fields
// (hasTargetIdentification, hasMovement, hasDamage, targetRule) in Stage 8-N.
using UnityEngine;
using UnityEditor;

public class Stage7NSpiteCards
{
    public static void Execute()
    {
        Debug.LogWarning("[7N SpiteCards] Obsolete since Stage 8-M — " +
                         "BehaviorGroup and BehaviorCardType.Permanent removed. " +
                         "Re-author Spite cards with new sub-phase format in Stage 8-N.");
    }
}
