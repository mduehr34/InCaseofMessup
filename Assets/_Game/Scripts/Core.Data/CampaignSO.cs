using UnityEngine;

namespace MnM.Core.Data
{
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

        [Header("Overlord")]
        public MonsterSO overlordMonster;           // Year 30 final boss
        public int[] overlordApproachYears;         // Years where Overlord warning events fire
    }
}
