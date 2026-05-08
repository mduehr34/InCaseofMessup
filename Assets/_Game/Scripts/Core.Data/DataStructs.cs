using UnityEngine;

namespace MnM.Core.Data
{
    // Marker interface — all runtime state classes implement this
    public interface IJsonSerializable { }

    [System.Serializable]
    public class GridOccupant
    {
        public string occupantId;       // hunterId or "monster"
        public bool isHunter;
        public int gridX;
        public int gridY;
        public int footprintW;          // Monster: 2 or 3. Hunter: always 1
        public int footprintH;
    }

    [System.Serializable]
    public struct LinkPoint
    {
        public string affinityTag;
        public Vector2Int direction;    // Which edge of the item the link point is on (Y-down screen space)
        // Stat bonus applied when this link is active
        public int bonusAccuracy;
        public int bonusStrength;
        public int bonusToughness;
        public int bonusEvasion;
        public int bonusLuck;
        public int bonusMovement;
    }

    [System.Serializable]
    public struct SetBonusEntry
    {
        public int requiredPieceCount;
        public int bonusAccuracy;
        public int bonusStrength;
        public int bonusToughness;
        public int bonusEvasion;
        public int bonusLuck;
        public int bonusMovement;
        // Non-stat effect resolved by CombatManager via string tag
        public string effectTag;
        [TextArea] public string effectDescription;
    }

    [System.Serializable]
    public struct MonsterStatBlock
    {
        public int movement;
        public int accuracy;
        public int strength;
        public int toughness;
        public int evasion;
        // Deck size is derived at runtime from BehaviorCardSO[].Length — no field needed
    }

    [System.Serializable]
    public struct BehaviorDeckComposition
    {
        public int baseCardCount;           // Cards drawn from the monster's base pool
        public int advancedCardCount;       // Cards drawn from the advanced pool
        public int overwhelmingCardCount;   // Cards drawn from the overwhelming pool
        // Total drawn = health pool for this difficulty
        // Pools are authored larger than counts — each fight draws a different random subset
    }

    [System.Serializable]
    public struct FacingAccuracyBonus
    {
        public FacingArc arc;
        public int accuracyModifier;        // e.g. Rear = +2 (easier to hit from behind)
    }

    [System.Serializable]
    public struct LootEntry
    {
        public ResourceSO resource;
        public int minAmount;
        public int maxAmount;
        public int weight;                  // Relative probability weight
    }

    [System.Serializable]
    public struct StanceDefinition
    {
        public string stanceName;
        public string stanceTag;
        [TextArea] public string effect;
    }

    [System.Serializable]
    public struct EventChoice
    {
        public string choiceLabel;              // "A:" or "B:"
        [TextArea] public string outcomeText;
        [TextArea] public string mechanicalEffect;  // Human-readable; resolved by Core.Logic
        public string artifactUnlockId;
        public string codexEntryId;
        public string guidingPrincipalTrigger;
    }

    [System.Serializable]
    public struct WeaponProficiency
    {
        public WeaponType weaponType;
        public int tier;                        // 1–5
        public int successfulActivations;
    }
}
