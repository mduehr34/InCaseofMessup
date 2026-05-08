using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/BehaviorCard", fileName = "New BehaviorCard")]
    public class BehaviorCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string cardName;
        public BehaviorCardType cardType;   // Removable, Mood, or SingleTrigger

        [Header("Trigger & Effect")]
        [TextArea] public string triggerCondition;
        [TextArea] public string effectDescription;

        [Header("Monster Turn Sub-Phases")]
        public bool hasTargetIdentification;
        public string targetRule;           // "nearest", "aggro", "mostInjured", "last_attacker"
        public bool hasMovement;
        public bool hasDamage;
        public string forcedHunterBodyPart; // Leave empty for random roll; override e.g. "Head", "Torso"

        [Header("Mood Card — Removal Condition")]
        [TextArea] public string removalCondition;
        // Examples:
        //   "Hunter spends 1 Grit"
        //   "Hunter inflicts a wound"
        //   "3 turns"
        // Evaluated by Core.Logic each turn. When met: card → BehaviorDiscard (re-enters health pool)

        [Header("Critical Wound — Alternate Behavior")]
        public string criticalWoundCondition;           // Tag from WoundLocationSO.criticalWoundTag
                                                        // e.g. "GauntJaw_Critical"
        [TextArea] public string alternateTriggerCondition;
        [TextArea] public string alternateEffectDescription;
        // If criticalWoundCondition is set and that tag is active at runtime,
        // the alternate fields replace triggerCondition and effectDescription for this draw.

        [Header("Tags")]
        public string stanceTag;
        public string groupTag;
    }
}
