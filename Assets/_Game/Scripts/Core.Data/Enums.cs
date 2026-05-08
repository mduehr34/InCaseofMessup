namespace MnM.Core.Data
{
    public enum WeaponType
    {
        FistWeapon, Dagger, SwordAndShield, Axe,
        HammerMaul, Spear, Greatsword, Bow
    }

    public enum ElementTag { None, Fire, Ice, Venom, Shock }

    public enum BodyPartTag
    {
        Head, Throat, Torso, LeftFlank, RightFlank,
        HindLegs, Tail, Arms, Legs, Waist, Back
    }

    public enum ResourceType
    {
        Bone, Hide, Organ,
        UniqueCommon, UniqueUncommon, UniqueRare
    }

    public enum BehaviorCardType { Removable, Mood, SingleTrigger }
    public enum WoundOutcome { Wound, Critical, Failure, Trap }

    public enum CardCategory
    {
        Opener, Linker, Finisher,
        BasicAttack, Reaction, Signature
    }

    public enum DifficultyLevel { Easy, Medium, Hard }
    public enum InjurySeverity { Minor, Major, Critical }
    public enum CodexCategory { Monsters, Artifacts, SettlementRecords }
    public enum CharacterSex { Male, Female }

    public enum CharacterBuild
    {
        // Male builds
        Aethel, Beorn, Cyne, Duna,
        // Female builds
        Eira, Freya, Gerd, Hild
    }

    public enum GuidingPrincipalTag
    {
        LifeOrStrength, BloodPrice, MarrowKnowledge,
        LegacyOrForgetting, TheSuture
    }

    public enum StatusEffect { Shaken, Slowed, Pinned, Exposed, Bleeding, Marked, Broken, Inspired }
    public enum CombatPhase { VitalityPhase, HunterPhase, BehaviorRefresh, MonsterPhase }
    public enum DamageType { Shell, Flesh }    // Used for hunter body zone damage only
    public enum FacingArc { Front, Flank, Rear }
    public enum AudioContext { MainMenu, SettlementEarly, SettlementLate, HuntTravel, CombatStandard, CombatOverlord }

    public enum MovementPattern
    {
        None,
        Approach,   // Step-by-step toward aggro target
        Charge,     // Full distance in a straight line, push through hunters
        Pivot,      // Face toward lowest-Flesh hunter, no position change
    }

    public enum AttackTargetType
    {
        None,
        AggroTarget,    // Single hit on the aggro holder
        AllAdjacent,    // All hunters within 1 cell
        AllBehind,      // All hunters in rear arc (behind monster facing)
        AllInFront,     // All hunters in front arc
        AllInRange,     // All hunters within attackRange cells
    }
}
