using UnityEngine;

namespace MnM.Core.Data
{
    [System.Serializable]
    public struct OverlordScheduleEntry
    {
        public MonsterSO overlordMonster;
        public int arrivalYear;
        public int[] approachYears;     // Years where warning events fire before arrival
    }

    [CreateAssetMenu(menuName = "MnM/Campaign", fileName = "New Campaign")]
    public class CampaignSO : ScriptableObject
    {
        [Header("Identity")]
        public string campaignName;
        public DifficultyLevel difficulty;
        public int campaignLengthYears;     // Default 30

        [Header("Starting Conditions")]
        public int startingCharacterCount;  // 6 / 8 / 10 per difficulty
        public int baseMovement;
        public int startingGrit;
        public bool ironmanMode;

        [Header("Content Pools")]
        public MonsterSO[] monsterRoster;
        public EventSO[] eventPool;
        public InnovationSO[] startingInnovations;  // Base deck — seeds 12 cards
        public CrafterSO[] crafterPool;
        public GuidingPrincipalSO[] guidingPrincipals;

        [Header("Thresholds")]
        public int retirementHuntCount;
        public int birthConditionAge;

        [Header("Overlord Schedule")]
        public OverlordScheduleEntry[] overlordSchedule;    // Ordered list of overlord encounters
    }
}
