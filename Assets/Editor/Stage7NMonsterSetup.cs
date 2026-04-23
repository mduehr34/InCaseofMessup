using UnityEngine;
using UnityEditor;
using MnM.Core.Data;
using System.IO;

public class Stage7NMonsterSetup
{
    const string MONSTERS = "Assets/_Game/Data/Monsters";
    const string RESOURCES = "Assets/_Game/Data/Resources";
    const string BEHAVIOR  = "Assets/_Game/Data/Cards/Behavior";

    // ── helpers ──────────────────────────────────────────────────────────────

    static T GetOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) { init(existing); EditorUtility.SetDirty(existing); return existing; }
        var so = ScriptableObject.CreateInstance<T>();
        init(so);
        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    static T Load<T>(string path) where T : Object
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) Debug.LogWarning($"[7N] missing: {path}");
        return a;
    }

    static BehaviorCardSO Card(string path, string name, BehaviorCardType type,
        BehaviorGroup group, string trigger, string effect, string removal = "")
    {
        return GetOrCreate<BehaviorCardSO>(path, c =>
        {
            c.cardName         = name;
            c.cardType         = type;
            c.group            = group;
            c.triggerCondition = trigger;
            c.effectDescription = effect;
            c.removalCondition = removal;
            c.stanceTag        = "";
            c.groupTag         = "";
        });
    }

    static MonsterBodyPart Part(string name, BodyPartTag tag, int shell, int flesh,
        string[] br = null, string[] wr = null, bool trap = false) => new MonsterBodyPart
        {
            partName               = name,
            partTag                = tag,
            shellDurability        = shell,
            fleshDurability        = flesh,
            breakRemovesCardNames  = br ?? new string[0],
            woundRemovesCardNames  = wr ?? new string[0],
            isTrapZone             = trap
        };

    static FacingTable Facing(BodyPartTag p, int pw, BodyPartTag s, int sw, BodyPartTag t, int tw) =>
        new FacingTable { primaryZone=p, primaryZoneWeight=pw, secondaryZone=s, secondaryZoneWeight=sw, tertiaryZone=t, tertiaryZoneWeight=tw };

    static LootEntry Loot(ResourceSO res, int min, int max, int w) =>
        new LootEntry { resource=res, minAmount=min, maxAmount=max, weight=w };

    // ── entry point ──────────────────────────────────────────────────────────

    public static void Execute()
    {
        // Step 1 — directories
        foreach (var sub in new[]{"Thornback","IvoryStampede","Serpent","Spite"})
        {
            string full = Path.Combine(Application.dataPath, "_Game/Data/Cards/Behavior", sub);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
        }
        AssetDatabase.Refresh();

        // Step 2 — rename legacy mocks
        string e;
        e = AssetDatabase.RenameAsset($"{MONSTERS}/Mock_GauntStandard.asset", "Monster_Gaunt");
        if (!string.IsNullOrEmpty(e)) Debug.LogWarning($"[7N] rename Gaunt: {e}");
        e = AssetDatabase.RenameAsset($"{RESOURCES}/Mock_GauntFang.asset", "Resource_GauntFang");
        if (!string.IsNullOrEmpty(e)) Debug.LogWarning($"[7N] rename GauntFang: {e}");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Step 3 — resources
        var r = BuildResources();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Step 4 — behavior cards for new monsters
        BuildThornbackCards();
        BuildStampedeCards();
        BuildSerpentCards();
        BuildSpiteStubs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Step 5 — monster SOs
        SetupGaunt(r);
        BuildThornback(r);
        BuildIvoryStampede(r);
        BuildGildedSerpent(r);
        BuildTheSpite(r);
        BuildSuture();
        BuildPenitent(r);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[7N] All monster SO assets complete.");
    }

    // ── resources ────────────────────────────────────────────────────────────

    struct Res
    {
        public ResourceSO bone, hide, sinew;
        public ResourceSO gauntFang, gauntPelt, gauntEye;
        public ResourceSO tbackPlate, tbackTusk, tbackCrystal;
        public ResourceSO ivoryTusk, herdHide, herdEye;
        public ResourceSO serpScale, serpFang, serpHeart;
        public ResourceSO ironClaw, ironPelt, gallShard;
        public ResourceSO penitentGland;
    }

    static Res BuildResources()
    {
        ResourceSO R(string path, string rname, ResourceType type, int tier) =>
            GetOrCreate<ResourceSO>(path, x => { x.resourceName=rname; x.type=type; x.tier=tier; x.conversionRate=1; });

        var r = new Res();
        r.bone          = R($"{RESOURCES}/Resource_Bone.asset",           "Bone",             ResourceType.Bone,           1);
        r.hide          = R($"{RESOURCES}/Resource_Hide.asset",           "Hide",             ResourceType.Hide,           1);
        r.sinew         = R($"{RESOURCES}/Resource_Sinew.asset",          "Sinew",            ResourceType.Organ,          1);
        r.gauntFang     = Load<ResourceSO>($"{RESOURCES}/Resource_GauntFang.asset");
        if (r.gauntFang) { r.gauntFang.resourceName="Gaunt Fang"; r.gauntFang.type=ResourceType.UniqueCommon; r.gauntFang.tier=1; EditorUtility.SetDirty(r.gauntFang); }
        r.gauntPelt     = R($"{RESOURCES}/Resource_GauntPelt.asset",      "Gaunt Pelt",       ResourceType.UniqueCommon,   1);
        r.gauntEye      = R($"{RESOURCES}/Resource_GauntEye.asset",       "Gaunt Eye",        ResourceType.UniqueRare,     1);
        r.tbackPlate    = R($"{RESOURCES}/Resource_ThornbackPlate.asset", "Thornback Plate",  ResourceType.UniqueCommon,   2);
        r.tbackTusk     = R($"{RESOURCES}/Resource_ThornbackTusk.asset",  "Thornback Tusk",   ResourceType.UniqueUncommon, 2);
        r.tbackCrystal  = R($"{RESOURCES}/Resource_ThornbackCrystal.asset","Thornback Crystal",ResourceType.UniqueRare,    2);
        r.ivoryTusk     = R($"{RESOURCES}/Resource_IvoryTusk.asset",      "Ivory Tusk",       ResourceType.UniqueCommon,   1);
        r.herdHide      = R($"{RESOURCES}/Resource_HerdHide.asset",       "Herd Hide",        ResourceType.UniqueUncommon, 1);
        r.herdEye       = R($"{RESOURCES}/Resource_HerdEye.asset",        "Herd Eye",         ResourceType.UniqueRare,     1);
        r.serpScale     = R($"{RESOURCES}/Resource_SerpentScale.asset",   "Serpent Scale",    ResourceType.UniqueCommon,   3);
        r.serpFang      = R($"{RESOURCES}/Resource_SerpentFang.asset",    "Serpent Fang",     ResourceType.UniqueUncommon, 3);
        r.serpHeart     = R($"{RESOURCES}/Resource_SerpentHeart.asset",   "Serpent Heart",    ResourceType.UniqueRare,     3);
        r.ironClaw      = R($"{RESOURCES}/Resource_IronClaw.asset",       "Iron Claw",        ResourceType.UniqueCommon,   3);
        r.ironPelt      = R($"{RESOURCES}/Resource_IronhidePelt.asset",   "Ironhide Pelt",    ResourceType.UniqueUncommon, 3);
        r.gallShard     = R($"{RESOURCES}/Resource_GallShard.asset",      "Gall Shard",       ResourceType.UniqueRare,     3);
        r.penitentGland = R($"{RESOURCES}/Resource_PenitentGland.asset",  "Penitent Gland",   ResourceType.UniqueRare,     4);
        return r;
    }

    // ── thornback cards ──────────────────────────────────────────────────────

    static void BuildThornbackCards()
    {
        string b = $"{BEHAVIOR}/Thornback/";
        var R = BehaviorCardType.Removable;
        var P = BehaviorCardType.Permanent;
        var Op = BehaviorGroup.Opening;
        var Es = BehaviorGroup.Escalation;
        var Ap = BehaviorGroup.Apex;

        // Opening (3)
        Card($"{b}Thornback_GuardedCharge.asset",    "Guarded Charge",    R, Op, "End of round",
            "Move 4 squares toward Aggro holder in a straight line. Hunters in path make Evasion check (4) or apply Shaken.");
        Card($"{b}Thornback_FrontLinePress.asset",   "Front Line Press",  R, Op, "Hunter enters Front arc",
            "Rotate to face that hunter. Transfer Aggro.");
        Card($"{b}Thornback_SpikeWarn.asset",        "Spike Warn",        R, Op, "End of round if Thornback did not move this round",
            "All hunters in Front arc apply Shaken for 1 round.");

        // Escalation (7)
        Card($"{b}Thornback_BoneRush.asset",         "Bone Rush",         R, Es, "End of round",
            "Move 6 squares in a straight line toward Aggro holder. Hunters in path take 1 Flesh damage.");
        Card($"{b}Thornback_TuskHook.asset",         "Tusk Hook",         R, Es, "Hunter is adjacent to Snout",
            "Apply Bleeding to that hunter. Transfer Aggro.");
        Card($"{b}Thornback_ImpactTremor.asset",     "Impact Tremor",     R, Es, "End of round if Thornback moved 5 or more squares this round",
            "All hunters within 2 squares apply Shaken.");
        Card($"{b}Thornback_ChargeFrenzy.asset",     "Charge Frenzy",     R, Es, "Two or more hunters in Front arc simultaneously",
            "Standard attack against both hunters at -1 Accuracy each.");
        Card($"{b}Thornback_SnoutGore.asset",        "Snout Gore",        R, Es, "End of round if a hunter is in Front arc",
            "Rotate to face nearest hunter in Front arc. Standard attack against that hunter.");
        Card($"{b}Thornback_ThornBarrage.asset",     "Thorn Barrage",     R, Es, "Hunter attacks Dorsal Ridge",
            "Rotate to face that hunter. Apply Shaken to that hunter. Thornback gains +1 Toughness until end of next round.");
        Card($"{b}Thornback_CrushingBlow.asset",     "Crushing Blow",     R, Es, "End of round if Haunches are intact",
            "Move 3 squares toward Aggro holder. Standard attack at +1 Strength.");

        // Apex (6)
        Card($"{b}Thornback_Skullbreaker.asset",     "Skullbreaker",      R, Ap, "End of round if Skull is intact",
            "Move 5 squares toward Aggro holder. Standard attack. On hit, apply Pinned for 1 round.");
        Card($"{b}Thornback_UnyieldingCharge.asset", "Unyielding Charge", R, Ap, "End of round",
            "Move 8 squares toward Aggro holder ignoring terrain penalties. Hunters hit make Evasion (6) or lose their next full turn.");
        Card($"{b}Thornback_BloodScent.asset",       "Blood Scent",       R, Ap, "Any hunter's Flesh reaches 2 or below",
            "Move immediately adjacent to that hunter. Transfer Aggro. Apply Shaken.");
        Card($"{b}Thornback_BoneWall.asset",         "Bone Wall",         R, Ap, "Both shoulder parts are broken",
            "Thornback gains +2 Toughness until end of next round.");
        Card($"{b}Thornback_TerrorCharge.asset",     "Terror Charge",     R, Ap, "Start of round",
            "Transfer Aggro to the hunter furthest from Thornback. Move 3 squares toward that hunter.");
        Card($"{b}Thornback_GoreStorm.asset",        "Gore Storm",        R, Ap, "End of round if 3 or more parts are broken",
            "Standard attack against all hunters in Front arc. Apply Shaken to every hunter hit.");

        // Permanent
        Card($"{b}Thornback_PanicHerd.asset",        "Panic Herd",        P, Op, "End of every round",
            "Rotate to face the largest cluster of hunters within 4 squares.");
    }

    // ── stampede cards ───────────────────────────────────────────────────────

    static void BuildStampedeCards()
    {
        string b = $"{BEHAVIOR}/IvoryStampede/";
        var R = BehaviorCardType.Removable;
        var P = BehaviorCardType.Permanent;
        var Op = BehaviorGroup.Opening;
        var Es = BehaviorGroup.Escalation;
        var Ap = BehaviorGroup.Apex;

        // Opening (3)
        Card($"{b}Stampede_HerdWall.asset",        "Herd Wall",        R, Op, "End of round",
            "All 3 elephants move 2 squares toward Aggro holder. Hunters surrounded by 2 or more elephants apply Pinned.");
        Card($"{b}Stampede_EchoCharge.asset",      "Echo Charge",      R, Op, "End of round",
            "All 3 elephants move 4 squares toward the nearest hunter to each of them.");
        Card($"{b}Stampede_TuskBrush.asset",       "Tusk Brush",       R, Op, "Any elephant adjacent to a hunter",
            "That elephant makes a standard attack. Transfer Aggro to the attacked hunter.");

        // Escalation (6)
        Card($"{b}Stampede_PackSurge.asset",       "Pack Surge",       R, Es, "End of round",
            "All 3 elephants move 3 squares toward Aggro holder. Hunters in the combined path apply Shaken.");
        Card($"{b}Stampede_TuskRush.asset",        "Tusk Rush",        R, Es, "End of round if 2 or more hunters are clustered within 3 squares",
            "Closest elephant to cluster charges 5 squares through it. Hunters hit make Evasion (5) or lose 1 AP next round.");
        Card($"{b}Stampede_CirclePress.asset",     "Circle Press",     R, Es, "End of round if 2 elephants are in the same 5x5 zone",
            "Those 2 elephants move to opposite sides of Aggro holder. Apply Shaken to Aggro holder.");
        Card($"{b}Stampede_IvoryImpact.asset",     "Ivory Impact",     R, Es, "End of round",
            "Elephant with Aggro token makes standard attack. Other elephants move 2 squares toward same target.");
        Card($"{b}Stampede_WildKick.asset",        "Wild Kick",        R, Es, "Hunter attacks Legs part on any elephant",
            "That elephant kicks back. Hunter makes Evasion (4) or is pushed 2 squares and applies Shaken.");
        Card($"{b}Stampede_AggroPulse.asset",      "Aggro Pulse",      R, Es, "An elephant is killed",
            "All remaining elephants move 3 squares toward the hunter who dealt the killing blow.");

        // Apex (6)
        Card($"{b}Stampede_CoreBreaker.asset",     "Core Breaker",     R, Ap, "Start of round",
            "All 3 elephants move 2 squares toward Aggro holder. Each makes a standard attack if adjacent after movement.");
        Card($"{b}Stampede_IvoryFury.asset",       "Ivory Fury",       R, Ap, "First elephant is killed",
            "All remaining elephants gain +2 Movement and +1 Strength for the rest of the hunt.");
        Card($"{b}Stampede_BlindedHerd.asset",     "Blinded Herd",     R, Ap, "Any elephant's Head is broken",
            "All elephants move toward nearest hunter regardless of Aggro. Apply Shaken to all adjacent hunters.");
        Card($"{b}Stampede_RampageCharge.asset",   "Rampage Charge",   R, Ap, "End of round",
            "All 3 elephants make a full-speed charge (8 squares) toward their nearest hunter. Hunters hit take 1 Flesh damage.");
        Card($"{b}Stampede_LastElephant.asset",    "Last Elephant",    R, Ap, "Two elephants are killed",
            "The remaining elephant gains +3 Movement, +2 Strength, and +1 Accuracy for the rest of the hunt.");
        Card($"{b}Stampede_TremorStomp.asset",     "Tremor Stomp",     R, Ap, "End of round if all 3 elephants are in the Front arc of a single hunter",
            "All 3 elephants stomp simultaneously. That hunter takes 3 Shell damage.");

        // Permanent
        Card($"{b}Stampede_HerdSense.asset",       "Herd Sense",       P, Op, "Start of every round",
            "All 3 elephants orient toward Aggro holder. Their front arcs all face Aggro holder's current position.");
    }

    // ── serpent cards ─────────────────────────────────────────────────────────

    static void BuildSerpentCards()
    {
        string b = $"{BEHAVIOR}/Serpent/";
        var R = BehaviorCardType.Removable;
        var P = BehaviorCardType.Permanent;
        var Op = BehaviorGroup.Opening;
        var Es = BehaviorGroup.Escalation;
        var Ap = BehaviorGroup.Apex;

        // Opening (3)
        Card($"{b}Serpent_CoilSettle.asset",       "Coil Settle",       R, Op, "End of round",
            "Move 3 squares into terrain-controlling position. Hunters within 2 squares apply Slowed.");
        Card($"{b}Serpent_VenomSpray.asset",       "Venom Spray",       R, Op, "End of round if adjacent to a hunter",
            "Standard attack against that hunter. On hit, apply Bleeding for 2 rounds in addition to normal damage.");
        Card($"{b}Serpent_TerrainGrip.asset",      "Terrain Grip",      R, Op, "Hunter moves into a trap zone square",
            "That hunter makes Evasion (5) or applies Pinned for 1 round.");

        // Escalation (7)
        Card($"{b}Serpent_PrecisionStrike.asset",  "Precision Strike",  R, Es, "Hunter plays a Movement card",
            "Serpent makes standard attack against that hunter at +2 Accuracy.");
        Card($"{b}Serpent_ConstrictionZone.asset", "Constriction Zone", R, Es, "End of round if 2 or more hunters within 3 squares",
            "Serpent repositions to maximize coil coverage. All hunters in affected area apply Slowed.");
        Card($"{b}Serpent_SilentApproach.asset",   "Silent Approach",   R, Es, "End of round if no hunter is in Front arc",
            "Move 5 squares without triggering Aggro transfer. Reposition to flanking position.");
        Card($"{b}Serpent_MassToxin.asset",        "Mass Toxin",        R, Es, "End of round if 2 or more hunters are adjacent to each other",
            "Venom cloud in a 2x2 area centered between them. All hunters in area apply Bleeding.");
        Card($"{b}Serpent_TailWhip.asset",         "Tail Whip",         R, Es, "Hunter is in Rear arc",
            "Standard attack against that hunter. Push hunter 2 squares away from Serpent.");
        Card($"{b}Serpent_ScaleShimmer.asset",     "Scale Shimmer",     R, Es, "Hunter plays a Precision card",
            "Serpent gains +2 Evasion until end of round. Transfer Aggro to nearest other hunter.");
        Card($"{b}Serpent_CoilTrap.asset",         "Coil Trap",         R, Es, "End of round",
            "Move 2 squares. Serpent's new coil position is treated as an active trap zone until next round.");

        // Apex (6)
        Card($"{b}Serpent_GildedRush.asset",       "Gilded Rush",       R, Ap, "End of round",
            "Move 7 squares toward Aggro holder. Standard attack on arrival at +1 Accuracy.");
        Card($"{b}Serpent_CrushingCoil.asset",     "Crushing Coil",     R, Ap, "Hunter is adjacent to both Upper Coil and Lower Coil simultaneously",
            "Apply Pinned and Bleeding to that hunter. Serpent does not move this phase.");
        Card($"{b}Serpent_FangBarrage.asset",      "Fang Barrage",      R, Ap, "End of round if Head is intact",
            "Standard attack against Aggro holder at +2 Accuracy and +1 Strength.");
        Card($"{b}Serpent_VenomOverdose.asset",    "Venom Overdose",    R, Ap, "3 or more hunters have Bleeding status",
            "All hunters with Bleeding refresh Bleeding duration and additionally apply Shaken.");
        Card($"{b}Serpent_TerrainMastery.asset",   "Terrain Mastery",   R, Ap, "End of round if Serpent has not moved for 2 or more consecutive rounds",
            "Serpent designates a 4x1 zone. Hunters entering that zone automatically apply Slowed.");
        Card($"{b}Serpent_UncoilSurge.asset",      "Uncoil Surge",      R, Ap, "Start of round if any hunter has Slowed status",
            "Serpent moves full Movement toward Aggro holder, gaining +2 Movement this round.");

        // Permanent
        Card($"{b}Serpent_GoldenPresence.asset",   "Golden Presence",   P, Op, "End of every round",
            "Serpent rotates for optimal coil coverage. Hunters cannot voluntarily move adjacent to Tail Tip without an Evasion check (3).");
    }

    // ── spite stub cards ──────────────────────────────────────────────────────

    static void BuildSpiteStubs()
    {
        string b = $"{BEHAVIOR}/Spite/";
        var R  = BehaviorCardType.Removable;
        var P  = BehaviorCardType.Permanent;
        var Op = BehaviorGroup.Opening;
        var Es = BehaviorGroup.Escalation;
        var Ap = BehaviorGroup.Apex;

        // Opening stubs
        Card($"{b}Spite_Relentless.asset",       "Relentless",       R, Op, "", "");
        Card($"{b}Spite_NoQuarter.asset",        "No Quarter",       R, Op, "", "");
        Card($"{b}Spite_Endure.asset",           "Endure",           R, Op, "", "");
        // Escalation stubs
        Card($"{b}Spite_WoundThrough.asset",     "Wound Through",    R, Es, "", "");
        Card($"{b}Spite_IronHide.asset",         "Iron Hide",        R, Es, "", "");
        Card($"{b}Spite_BloodIgnore.asset",      "Blood Ignore",     R, Es, "", "");
        Card($"{b}Spite_PersistentAggro.asset",  "Persistent Aggro", R, Es, "", "");
        Card($"{b}Spite_RelentlessCharge.asset", "Relentless Charge",R, Es, "", "");
        Card($"{b}Spite_GallBite.asset",         "Gall Bite",        R, Es, "", "");
        Card($"{b}Spite_SpiteTurn.asset",        "Spite Turn",       R, Es, "", "");
        // Apex stubs
        Card($"{b}Spite_Untameable.asset",       "Untameable",       R, Ap, "", "");
        Card($"{b}Spite_ShedsBlood.asset",       "Sheds Blood",      R, Ap, "", "");
        Card($"{b}Spite_VenomRage.asset",        "Venom Rage",       R, Ap, "", "");
        Card($"{b}Spite_DeathDefying.asset",     "Death Defying",    R, Ap, "", "");
        Card($"{b}Spite_MarrowSense.asset",      "Marrow Sense",     R, Ap, "", "");
        Card($"{b}Spite_FinalFury.asset",        "Final Fury",       R, Ap, "", "");
        // Permanent stub
        Card($"{b}Spite_Tenacity.asset",         "Tenacity",         P, Op, "", "");
    }

    // ── monster: Gaunt ───────────────────────────────────────────────────────

    static void SetupGaunt(Res r)
    {
        var g = AssetDatabase.LoadAssetAtPath<MonsterSO>($"{MONSTERS}/Monster_Gaunt.asset");
        if (!g) { Debug.LogError("[7N] Monster_Gaunt.asset not found"); return; }

        g.standardParts = new[]
        {
            Part("Head",        BodyPartTag.Head,       2, 3, null, new[]{"Gaunt_TremorRead","Gaunt_PackMemory"}),
            Part("Throat",      BodyPartTag.Throat,     2, 3, new[]{"Gaunt_TheHowl","Gaunt_ScentLock"}),
            Part("Torso",       BodyPartTag.Torso,      2, 3, null, new[]{"Gaunt_Frenzy"}),
            Part("Left Flank",  BodyPartTag.LeftFlank,  2, 3, new[]{"Gaunt_ThroatLock"}),
            Part("Right Flank", BodyPartTag.RightFlank, 2, 3, new[]{"Gaunt_CreepingAdvance"}),
            Part("Hind Legs",   BodyPartTag.HindLegs,   2, 3, new[]{"Gaunt_Lunge"}),
            Part("Tail",        BodyPartTag.Tail,       2, 3, new[]{"Gaunt_FlankSense"}),
        };
        g.hardenedParts = g.standardParts;
        g.apexParts     = g.standardParts;

        string gb = $"{BEHAVIOR}/Gaunt/";
        g.openingCards = new[]
        {
            Load<BehaviorCardSO>($"{gb}Gaunt_CreepingAdvance.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_ScentLock.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_FlankSense.asset"),
        };
        g.escalationCards = new[]
        {
            Load<BehaviorCardSO>($"{gb}Gaunt_TremorRead.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_Lunge.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_TheHowl.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_PackMemory.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_BloodFrenzy.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_DeathSilence.asset"),
        };
        g.apexCards = new[]
        {
            Load<BehaviorCardSO>($"{gb}Gaunt_Frenzy.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_ThroatLock.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_SavageLunge.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_PackInstinct.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_MarrowHunger.asset"),
            Load<BehaviorCardSO>($"{gb}Gaunt_ApexPredator.asset"),
        };
        g.permanentCards = new[]
        {
            Load<BehaviorCardSO>($"{gb}Gaunt_Stillness.asset"),
        };

        g.weaknesses   = new ElementTag[0];
        g.resistances  = new ElementTag[0];
        g.trapZoneParts = new string[0];

        // Facing tables already correct in mock — preserved on load
        // Stat blocks and footprints already correct in mock — preserved on load

        g.lootTable = new[]
        {
            Loot(r.bone,     2, 4, 40),
            Loot(r.hide,     2, 3, 30),
            Loot(r.sinew,    1, 2, 20),
            Loot(r.gauntFang,2, 3, 60),
            Loot(r.gauntPelt,1, 2, 40),
            Loot(r.gauntEye, 0, 1, 20),
        };

        EditorUtility.SetDirty(g);
    }

    // ── monster: Thornback ───────────────────────────────────────────────────

    static void BuildThornback(Res r)
    {
        GetOrCreate<MonsterSO>($"{MONSTERS}/Monster_Thornback.asset", m =>
        {
            m.monsterName  = "Thornback";
            m.materialTier = 2;
            m.animalBasis  = "Marrow-enhanced wild boar, armored with calcified bone spikes from dorsal ridge";
            m.combatEmotion = "Aggression — charges and tramples, does not retreat";

            m.gridFootprintStandard = new Vector2Int(3,2);
            m.gridFootprintHardened = new Vector2Int(3,2);
            m.gridFootprintApex     = new Vector2Int(4,3);

            m.statBlocks = new[]
            {
                new MonsterStatBlock{movement=7, accuracy=2, strength=3, toughness=2, evasion=1, behaviorDeckSizeRemovable=10},
                new MonsterStatBlock{movement=9, accuracy=3, strength=4, toughness=3, evasion=2, behaviorDeckSizeRemovable=13},
                new MonsterStatBlock{movement=11,accuracy=4, strength=5, toughness=4, evasion=3, behaviorDeckSizeRemovable=16},
            };

            var parts = new[]
            {
                Part("Skull",          BodyPartTag.Head,       3, 3),
                Part("Snout",          BodyPartTag.Throat,     3, 3, trap:true),
                Part("Left Shoulder",  BodyPartTag.LeftFlank,  3, 3),
                Part("Right Shoulder", BodyPartTag.RightFlank, 3, 3),
                Part("Dorsal Ridge",   BodyPartTag.Back,       3, 3, trap:true),
                Part("Haunches",       BodyPartTag.HindLegs,   3, 3),
                Part("Tail Bone",      BodyPartTag.Tail,       3, 3),
            };
            m.standardParts = parts; m.hardenedParts = parts; m.apexParts = parts;

            string b = $"{BEHAVIOR}/Thornback/";
            m.openingCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Thornback_GuardedCharge.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_FrontLinePress.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_SpikeWarn.asset"),
            };
            m.escalationCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Thornback_BoneRush.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_TuskHook.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_ImpactTremor.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_ChargeFrenzy.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_SnoutGore.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_ThornBarrage.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_CrushingBlow.asset"),
            };
            m.apexCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Thornback_Skullbreaker.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_UnyieldingCharge.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_BloodScent.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_BoneWall.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_TerrorCharge.asset"),
                Load<BehaviorCardSO>($"{b}Thornback_GoreStorm.asset"),
            };
            m.permanentCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Thornback_PanicHerd.asset"),
            };

            m.weaknesses   = new ElementTag[0];
            m.resistances  = new ElementTag[0];
            m.trapZoneParts = new[]{"Snout","Dorsal Ridge"};

            m.frontFacing = Facing(BodyPartTag.Head,     50, BodyPartTag.Throat,   30, BodyPartTag.LeftFlank, 20);
            m.flankFacing = Facing(BodyPartTag.LeftFlank,50, BodyPartTag.Back,     30, BodyPartTag.HindLegs,  20);
            m.rearFacing  = Facing(BodyPartTag.HindLegs, 50, BodyPartTag.Tail,     30, BodyPartTag.Back,      20);

            m.lootTable = new[]
            {
                Loot(r.bone,       3, 5, 40),
                Loot(r.hide,       3, 4, 30),
                Loot(r.tbackPlate, 2, 3, 50),
                Loot(r.tbackTusk,  1, 2, 30),
                Loot(r.tbackCrystal,0,1, 15),
            };
        });
    }

    // ── monster: Ivory Stampede (PackMonsterSO) ──────────────────────────────

    static void BuildIvoryStampede(Res r)
    {
        string path = $"{MONSTERS}/Monster_TheIvoryStampede.asset";
        var s = AssetDatabase.LoadAssetAtPath<PackMonsterSO>(path);
        if (!s) { s = ScriptableObject.CreateInstance<PackMonsterSO>(); AssetDatabase.CreateAsset(s, path); }

        s.monsterName   = "The Ivory Stampede";
        s.materialTier  = 1;
        s.animalBasis   = "A pack of three Marrow-touched elephants, small and dense, coordinated through Marrow resonance";
        s.combatEmotion = "Herd instinct — move and strike in unison, coordinating through Marrow resonance";
        s.unitCount     = 3;

        s.gridFootprintStandard = new Vector2Int(1,1);
        s.gridFootprintHardened = new Vector2Int(1,1);
        s.gridFootprintApex     = new Vector2Int(1,1);

        s.statBlocks = new[]
        {
            new MonsterStatBlock{movement=8,  accuracy=2, strength=2, toughness=1, evasion=3, behaviorDeckSizeRemovable=9},
            new MonsterStatBlock{movement=10, accuracy=3, strength=3, toughness=2, evasion=4, behaviorDeckSizeRemovable=12},
            new MonsterStatBlock{movement=12, accuracy=4, strength=4, toughness=3, evasion=5, behaviorDeckSizeRemovable=15},
        };

        var parts = new[]
        {
            Part("Head", BodyPartTag.Head,   2, 2),
            Part("Neck", BodyPartTag.Throat, 2, 2),
            Part("Body", BodyPartTag.Torso,  2, 2),
            Part("Legs", BodyPartTag.Legs,   2, 2),
        };
        s.standardParts = parts; s.hardenedParts = parts; s.apexParts = parts;

        string b = $"{BEHAVIOR}/IvoryStampede/";
        s.openingCards = new[]
        {
            Load<BehaviorCardSO>($"{b}Stampede_HerdWall.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_EchoCharge.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_TuskBrush.asset"),
        };
        s.escalationCards = new[]
        {
            Load<BehaviorCardSO>($"{b}Stampede_PackSurge.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_TuskRush.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_CirclePress.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_IvoryImpact.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_WildKick.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_AggroPulse.asset"),
        };
        s.apexCards = new[]
        {
            Load<BehaviorCardSO>($"{b}Stampede_CoreBreaker.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_IvoryFury.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_BlindedHerd.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_RampageCharge.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_LastElephant.asset"),
            Load<BehaviorCardSO>($"{b}Stampede_TremorStomp.asset"),
        };
        s.permanentCards = new[]
        {
            Load<BehaviorCardSO>($"{b}Stampede_HerdSense.asset"),
        };

        s.weaknesses   = new ElementTag[0];
        s.resistances  = new ElementTag[0];
        s.trapZoneParts = new string[0];

        s.frontFacing = Facing(BodyPartTag.Head,   50, BodyPartTag.Torso,   30, BodyPartTag.Throat, 20);
        s.flankFacing = Facing(BodyPartTag.Torso,  50, BodyPartTag.Legs,    30, BodyPartTag.Head,   20);
        s.rearFacing  = Facing(BodyPartTag.Legs,   50, BodyPartTag.Torso,   30, BodyPartTag.Throat, 20);

        s.lootTable = new[]
        {
            Loot(r.bone,     2, 4, 40),
            Loot(r.hide,     2, 3, 30),
            Loot(r.ivoryTusk,2, 3, 60),
            Loot(r.herdHide, 1, 2, 35),
            Loot(r.herdEye,  0, 1, 15),
        };

        EditorUtility.SetDirty(s);
    }

    // ── monster: Gilded Serpent ───────────────────────────────────────────────

    static void BuildGildedSerpent(Res r)
    {
        GetOrCreate<MonsterSO>($"{MONSTERS}/Monster_GildedSerpent.asset", m =>
        {
            m.monsterName   = "The Gilded Serpent";
            m.materialTier  = 3;
            m.animalBasis   = "Vast serpent, Marrow-saturated, gold luminescent scales";
            m.combatEmotion = "Patient menace — coils, controls terrain, strikes with precision";

            m.gridFootprintStandard = new Vector2Int(4,1);
            m.gridFootprintHardened = new Vector2Int(4,1);
            m.gridFootprintApex     = new Vector2Int(5,1);

            m.statBlocks = new[]
            {
                new MonsterStatBlock{movement=5, accuracy=3, strength=3, toughness=3, evasion=2, behaviorDeckSizeRemovable=10},
                new MonsterStatBlock{movement=7, accuracy=4, strength=4, toughness=4, evasion=3, behaviorDeckSizeRemovable=13},
                new MonsterStatBlock{movement=9, accuracy=5, strength=5, toughness=5, evasion=4, behaviorDeckSizeRemovable=16},
            };

            var parts = new[]
            {
                Part("Head",       BodyPartTag.Head,       3, 4),
                Part("Upper Coil", BodyPartTag.LeftFlank,  3, 4, trap:true),
                Part("Mid Coil",   BodyPartTag.Torso,      3, 4),
                Part("Lower Coil", BodyPartTag.RightFlank, 3, 4, trap:true),
                Part("Tail Tip",   BodyPartTag.Tail,       3, 4),
            };
            m.standardParts = parts; m.hardenedParts = parts; m.apexParts = parts;

            string b = $"{BEHAVIOR}/Serpent/";
            m.openingCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Serpent_CoilSettle.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_VenomSpray.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_TerrainGrip.asset"),
            };
            m.escalationCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Serpent_PrecisionStrike.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_ConstrictionZone.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_SilentApproach.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_MassToxin.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_TailWhip.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_ScaleShimmer.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_CoilTrap.asset"),
            };
            m.apexCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Serpent_GildedRush.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_CrushingCoil.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_FangBarrage.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_VenomOverdose.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_TerrainMastery.asset"),
                Load<BehaviorCardSO>($"{b}Serpent_UncoilSurge.asset"),
            };
            m.permanentCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Serpent_GoldenPresence.asset"),
            };

            m.weaknesses   = new[]{ElementTag.Ice};
            m.resistances  = new[]{ElementTag.Fire};
            m.trapZoneParts = new[]{"Upper Coil","Lower Coil"};

            m.frontFacing = Facing(BodyPartTag.Head,       50, BodyPartTag.Torso,      30, BodyPartTag.LeftFlank,  20);
            m.flankFacing = Facing(BodyPartTag.LeftFlank,  50, BodyPartTag.Torso,      30, BodyPartTag.Tail,       20);
            m.rearFacing  = Facing(BodyPartTag.Tail,       50, BodyPartTag.RightFlank, 30, BodyPartTag.Torso,      20);

            m.lootTable = new[]
            {
                Loot(r.serpScale,2, 3, 60),
                Loot(r.serpFang, 1, 2, 40),
                Loot(r.serpHeart,0, 1, 20),
            };
        });
    }

    // ── monster: The Spite ───────────────────────────────────────────────────

    static void BuildTheSpite(Res r)
    {
        GetOrCreate<MonsterSO>($"{MONSTERS}/Monster_TheSpite.asset", m =>
        {
            m.monsterName   = "The Spite";
            m.materialTier  = 3;
            m.animalBasis   = "Massive Marrow-enhanced honey badger, Marrow saturation concentrated in the hide and jaw";
            m.combatEmotion = "Relentless — does not retreat, does not hesitate, continues through wounds that would collapse anything else";
            m.coreSkillTaught = "TBD — pending combat identity selection (Option A: shell regen vs Option B: wound resistance)";

            m.gridFootprintStandard = new Vector2Int(2,3);
            m.gridFootprintHardened = new Vector2Int(2,3);
            m.gridFootprintApex     = new Vector2Int(3,4);

            m.statBlocks = new[]
            {
                new MonsterStatBlock{movement=9,  accuracy=3, strength=3, toughness=2, evasion=4, behaviorDeckSizeRemovable=10},
                new MonsterStatBlock{movement=11, accuracy=4, strength=4, toughness=3, evasion=5, behaviorDeckSizeRemovable=13},
                new MonsterStatBlock{movement=13, accuracy=5, strength=5, toughness=4, evasion=6, behaviorDeckSizeRemovable=16},
            };

            var parts = new[]
            {
                Part("Head",         BodyPartTag.Head,       2, 4),
                Part("Jaw",          BodyPartTag.Throat,     2, 4),
                Part("Neck",         BodyPartTag.Back,       2, 4),
                Part("Torso",        BodyPartTag.Torso,      2, 4),
                Part("Left Flank",   BodyPartTag.LeftFlank,  2, 4),
                Part("Right Flank",  BodyPartTag.RightFlank, 2, 4),
                Part("Hindquarters", BodyPartTag.HindLegs,   2, 4),
            };
            m.standardParts = parts; m.hardenedParts = parts; m.apexParts = parts;

            string b = $"{BEHAVIOR}/Spite/";
            m.openingCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Spite_Relentless.asset"),
                Load<BehaviorCardSO>($"{b}Spite_NoQuarter.asset"),
                Load<BehaviorCardSO>($"{b}Spite_Endure.asset"),
            };
            m.escalationCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Spite_WoundThrough.asset"),
                Load<BehaviorCardSO>($"{b}Spite_IronHide.asset"),
                Load<BehaviorCardSO>($"{b}Spite_BloodIgnore.asset"),
                Load<BehaviorCardSO>($"{b}Spite_PersistentAggro.asset"),
                Load<BehaviorCardSO>($"{b}Spite_RelentlessCharge.asset"),
                Load<BehaviorCardSO>($"{b}Spite_GallBite.asset"),
                Load<BehaviorCardSO>($"{b}Spite_SpiteTurn.asset"),
            };
            m.apexCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Spite_Untameable.asset"),
                Load<BehaviorCardSO>($"{b}Spite_ShedsBlood.asset"),
                Load<BehaviorCardSO>($"{b}Spite_VenomRage.asset"),
                Load<BehaviorCardSO>($"{b}Spite_DeathDefying.asset"),
                Load<BehaviorCardSO>($"{b}Spite_MarrowSense.asset"),
                Load<BehaviorCardSO>($"{b}Spite_FinalFury.asset"),
            };
            m.permanentCards = new[]
            {
                Load<BehaviorCardSO>($"{b}Spite_Tenacity.asset"),
            };

            m.weaknesses   = new ElementTag[0];
            m.resistances  = new ElementTag[0];
            m.trapZoneParts = new string[0];

            m.frontFacing = Facing(BodyPartTag.Head,     50, BodyPartTag.Torso,    30, BodyPartTag.Throat,  20);
            m.flankFacing = Facing(BodyPartTag.LeftFlank,50, BodyPartTag.Torso,    30, BodyPartTag.HindLegs,20);
            m.rearFacing  = Facing(BodyPartTag.HindLegs, 50, BodyPartTag.Back,     30, BodyPartTag.Torso,   20);

            m.lootTable = new[]
            {
                Loot(r.ironClaw, 2, 3, 60),
                Loot(r.ironPelt, 1, 2, 40),
                Loot(r.gallShard,0, 1, 20),
            };
        });
    }

    // ── monster: Suture (stat blocks only) ───────────────────────────────────

    static void BuildSuture()
    {
        GetOrCreate<MonsterSO>($"{MONSTERS}/Monster_Suture.asset", m =>
        {
            m.monsterName  = "The Suture";
            m.materialTier = 4;
            m.animalBasis  = "";
            m.combatEmotion = "";

            m.gridFootprintStandard = new Vector2Int(4,4);
            m.gridFootprintHardened = new Vector2Int(4,4);
            m.gridFootprintApex     = new Vector2Int(5,5);

            m.statBlocks = new[]
            {
                new MonsterStatBlock{movement=8,  accuracy=4, strength=5, toughness=4, evasion=3, behaviorDeckSizeRemovable=0},
                new MonsterStatBlock{movement=10, accuracy=5, strength=6, toughness=5, evasion=4, behaviorDeckSizeRemovable=0},
                new MonsterStatBlock{movement=12, accuracy=6, strength=7, toughness=6, evasion=5, behaviorDeckSizeRemovable=0},
            };

            m.standardParts  = new MonsterBodyPart[0];
            m.hardenedParts  = new MonsterBodyPart[0];
            m.apexParts      = new MonsterBodyPart[0];
            m.openingCards   = new BehaviorCardSO[0];
            m.escalationCards= new BehaviorCardSO[0];
            m.apexCards      = new BehaviorCardSO[0];
            m.permanentCards = new BehaviorCardSO[0];
            m.lootTable      = new LootEntry[0];
            m.weaknesses     = new ElementTag[0];
            m.resistances    = new ElementTag[0];
            m.trapZoneParts  = new string[0];
        });
    }

    // ── overlord: Penitent (skeleton) ────────────────────────────────────────

    static void BuildPenitent(Res r)
    {
        GetOrCreate<MonsterSO>($"{MONSTERS}/Overlord_Penitent.asset", m =>
        {
            m.monsterName   = "The Penitent";
            m.materialTier  = 4;
            m.animalBasis   = "Massive Marrow-corrupted primate, twisted into a permanent hunched posture by Marrow saturation";
            m.combatEmotion = "Does not attack out of hunger or territory — senses harvested Marrow and is drawn to whoever carries the most. Deliberate, ancient, wrong.";
            m.coreSkillTaught = "AggroManager.GetPenitentTarget() — targets MarrowBeacon carrier, else highest total Shell";

            m.gridFootprintStandard = new Vector2Int(3,3);
            m.gridFootprintHardened = new Vector2Int(3,3);
            m.gridFootprintApex     = new Vector2Int(4,4);

            m.statBlocks = new[]
            {
                new MonsterStatBlock{movement=0, accuracy=0, strength=0, toughness=0, evasion=0, behaviorDeckSizeRemovable=0},
                new MonsterStatBlock{movement=0, accuracy=0, strength=0, toughness=0, evasion=0, behaviorDeckSizeRemovable=0},
                new MonsterStatBlock{movement=0, accuracy=0, strength=0, toughness=0, evasion=0, behaviorDeckSizeRemovable=0},
            };

            m.standardParts  = new MonsterBodyPart[0];
            m.hardenedParts  = new MonsterBodyPart[0];
            m.apexParts      = new MonsterBodyPart[0];
            m.openingCards   = new BehaviorCardSO[0];
            m.escalationCards= new BehaviorCardSO[0];
            m.apexCards      = new BehaviorCardSO[0];
            m.permanentCards = new BehaviorCardSO[0];
            m.weaknesses     = new ElementTag[0];
            m.resistances    = new ElementTag[0];
            m.trapZoneParts  = new string[0];

            m.lootTable = new[]
            {
                Loot(r.bone,        3, 5, 40),
                Loot(r.hide,        3, 4, 30),
                Loot(r.penitentGland,1,1,100),
            };
        });
    }
}
