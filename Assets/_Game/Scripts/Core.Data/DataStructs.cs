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
        public Vector2Int direction;    // Which edge of the item the link point is on
    }

    [System.Serializable]
    public struct MonsterStatBlock
    {
        public int movement;
        public int accuracy;
        public int strength;
        public int toughness;
        public int evasion;
        public int behaviorDeckSizeRemovable;
    }

    [System.Serializable]
    public struct MonsterBodyPart
    {
        public string partName;
        public BodyPartTag partTag;
        public int shellDurability;
        public int fleshDurability;
        // Names must exactly match BehaviorCardSO asset names
        public string[] breakRemovesCardNames;
        public string[] woundRemovesCardNames;
        public bool isTrapZone;
    }

    [System.Serializable]
    public struct FacingTable
    {
        public BodyPartTag primaryZone;
        public BodyPartTag secondaryZone;
        public BodyPartTag tertiaryZone;
        public int primaryZoneWeight;       // Must sum to 100 with secondary + tertiary
        public int secondaryZoneWeight;
        public int tertiaryZoneWeight;
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
