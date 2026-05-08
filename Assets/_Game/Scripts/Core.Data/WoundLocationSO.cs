using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Cards/WoundLocation", fileName = "New WoundLocation")]
    public class WoundLocationSO : ScriptableObject
    {
        [Header("Identity")]
        public string locationName;
        public BodyPartTag partTag;

        [Header("Wound Threshold")]
        public int woundTarget;             // d10 + Hunter.Strength > this = wound
                                            // Critical sub-check: d10 natural result >= (10 - Hunter.Luck)

        [Header("Trap")]
        public bool isTrap;                 // True = monster responds, no wound, no behavior card removed
        [TextArea] public string trapEffect;

        [Header("Impervious")]
        public bool isImpervious;           // True = force roll cannot remove a behavior card here
                                            // Wound/critical effects still fire; resources still granted
                                            // Strategic value: criticals set wound tags that alter behavior cards
                                            // Does NOT interact with isTrap — a location is one or the other

        [Header("Outcomes")]
        [TextArea] public string failureEffect;
        [TextArea] public string woundEffect;
        [TextArea] public string criticalEffect;

        [Header("Critical Wound Tracking")]
        public string criticalWoundTag;     // Runtime flag set when a critical lands here
                                            // e.g. "GauntJaw_Critical"
                                            // Behavior cards read this tag to alter their resolution

        [Header("Resources")]
        public ResourceEntry[] woundResources;
        public ResourceEntry[] criticalResources;
    }
}
