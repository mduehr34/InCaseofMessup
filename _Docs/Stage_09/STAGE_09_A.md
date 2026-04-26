<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-A | Injury, Scar, Disorder & Fighting Art Card SOs
Status: Stage 8-R complete. Full v0.8 flow working.
Task: Create the ScriptableObject classes and all asset instances
for the four hunter lifecycle card types: Injuries (permanent
wounds with mechanical penalties), Scars (permanent marks that
sometimes grant bonuses), Disorders (psychological conditions
with situational penalties), and Fighting Arts (powerful unlocked
techniques). These types already exist in HunterState as string
ID arrays — now they need real SO data behind them.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_A.md
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/HunterState.cs (or CampaignState.cs
  wherever HunterState is defined)

Then confirm:
- InjurySO, ScarSO, DisorderSO, FightingArtSO are all new
  ScriptableObjects in Core.Data namespace
- They follow the same id/name/bodyText/mechanicalEffect pattern
  as ActionCardSO and EventSO
- Assets go in Assets/_Game/Data/Injuries/, /Scars/, /Disorders/,
  /FightingArts/
- The Hunter Info panel in the settlement should be able to display
  all four types from HunterState IDs
- What you will NOT build (card art generation — that's Stage 9 later)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-A: Injury, Scar, Disorder & Fighting Art Card SOs

**Resuming from:** Stage 8-R complete — full game flow working, tagged v0.8
**Done when:** All four ScriptableObject classes compile; all card assets created; Hunter Info panel displays a hunter's injuries, scars, disorders, and fighting arts correctly
**Commit:** `"9A: Injury, Scar, Disorder, FightingArt SOs — classes and all card assets"`
**Next session:** STAGE_09_B.md

---

## What These Cards Are

When hunters survive hunts, bad things happen to their bodies and minds:

- **Injuries** — wounds that never fully heal. A broken arm, a gouged eye. Each applies a permanent stat penalty.
- **Scars** — marks left by near-death experiences. They look terrible but sometimes make a hunter harder, granting a situational bonus.
- **Disorders** — psychological damage. Hunters who survive too much horror develop phobias and compulsions that fire in specific situations.
- **Fighting Arts** — techniques learned through blood and experience. Powerful active bonuses that require specific conditions to use.

These cards are added to a hunter's permanent record and displayed in the Hunter Info panel.

---

## Part 1: The ScriptableObject Classes

### InjurySO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/InjurySO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Injury_", menuName = "MnM/Hunter Cards/Injury")]
    public class InjurySO : ScriptableObject
    {
        [Header("Identity")]
        public string injuryId;       // e.g. "INJ-01"
        public string injuryName;     // e.g. "Broken Arm"
        public string bodyLocation;   // "Arm" | "Leg" | "Torso" | "Head" | "Eye"

        [TextArea(3, 6)]
        public string flavourText;    // Brief description in settler voice

        [Header("Mechanical Effect")]
        [TextArea(2, 4)]
        public string mechanicalEffect;
        // Format: "-1 Accuracy" | "-1 Speed" | "-1 Toughness" | "Cannot carry two-handed weapons"
        // Multiple effects separated by semicolons

        [Header("Severity")]
        public bool isCrippling;     // If true, hunter cannot hunt until treated (future feature)
    }
}
```

### ScarSO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/ScarSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Scar_", menuName = "MnM/Hunter Cards/Scar")]
    public class ScarSO : ScriptableObject
    {
        [Header("Identity")]
        public string scarId;        // e.g. "SCAR-01"
        public string scarName;      // e.g. "Bite Mark"

        [TextArea(3, 6)]
        public string flavourText;

        [Header("Mechanical Effect")]
        [TextArea(2, 4)]
        public string mechanicalEffect;
        // Format: "+1 Toughness when below half Flesh" | "Reroll one die when Shaken"
        // Scars are conditional bonuses — they fire only in specific situations
    }
}
```

### DisorderSO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/DisorderSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Disorder_", menuName = "MnM/Hunter Cards/Disorder")]
    public class DisorderSO : ScriptableObject
    {
        [Header("Identity")]
        public string disorderId;     // e.g. "DIS-01"
        public string disorderName;   // e.g. "Nightmare"

        [TextArea(3, 6)]
        public string flavourText;

        [Header("Trigger")]
        [TextArea(2, 3)]
        public string triggerCondition;
        // When this fires: "At the start of each hunt" | "When a hunter dies in the same combat"

        [Header("Mechanical Effect")]
        [TextArea(2, 4)]
        public string mechanicalEffect;
        // What happens when triggered: "-1 Accuracy this round" | "Gain Shaken at round start"
    }
}
```

### FightingArtSO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/FightingArtSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "FightingArt_", menuName = "MnM/Hunter Cards/Fighting Art")]
    public class FightingArtSO : ScriptableObject
    {
        [Header("Identity")]
        public string artId;          // e.g. "FA-01"
        public string artName;        // e.g. "Trample"
        public string archetype;      // "Aggressor" | "Stalker" | "Warden" | "Scholar"

        [TextArea(3, 6)]
        public string flavourText;

        [Header("Unlock")]
        public int    unlockYear;     // Minimum years active before a hunter can learn this
        public string prerequisiteId; // Another art that must be learned first (optional)

        [Header("Mechanical Effect")]
        [TextArea(2, 4)]
        public string mechanicalEffect;
        // "Once per round: after a successful hit, push the monster back 1 space."
    }
}
```

---

## Part 2: Injury Assets

Create these in `Assets/_Game/Data/Injuries/`. Right-click → Create → MnM → Hunter Cards → Injury.

| Asset | injuryId | injuryName | bodyLocation | mechanicalEffect | flavourText |
|---|---|---|---|---|---|
| `Injury_BrokenArm` | `INJ-01` | Broken Arm | Arm | -1 Accuracy; Cannot use two-handed weapons | "Set at a wrong angle and it healed wrong. The ache never left." |
| `Injury_TornLeg` | `INJ-02` | Torn Leg | Leg | -1 Speed | "They said it would heal. It did. But not all the way." |
| `Injury_CrushedRibs` | `INJ-03` | Crushed Ribs | Torso | -1 Toughness | "Every breath is a reminder of what it cost to get away." |
| `Injury_GougedEye` | `INJ-04` | Gouged Eye | Eye | -1 Accuracy; -1 Luck | "The depth perception never came back." |
| `Injury_SeveredFingers` | `INJ-05` | Severed Fingers | Arm | -1 Accuracy | "Two fingers. The ones you miss most." |
| `Injury_BrokenJaw` | `INJ-06` | Broken Jaw | Head | -1 Grit | "Can't shout the calls anymore. Hunts quieter now." |
| `Injury_LameHip` | `INJ-07` | Lame Hip | Leg | -1 Speed; Cannot Dash | "Not useless. Just slower than they were." |
| `Injury_ConcussedSkull` | `INJ-08` | Concussed Skull | Head | -1 Grit; -1 Luck | "They're fine. They say they're fine." |
| `Injury_BurntHand` | `INJ-09` | Burnt Hand | Arm | -1 Accuracy | "The marrow burn goes deep. The skin grew back wrong." |
| `Injury_BittenCalf` | `INJ-10` | Bitten Calf | Leg | -1 Toughness | "Infected before they could clean it. The scar is ugly but it closed." |

---

## Part 3: Scar Assets

Create these in `Assets/_Game/Data/Scars/`.

| Asset | scarId | scarName | mechanicalEffect | flavourText |
|---|---|---|---|---|
| `Scar_BiteMark` | `SCAR-01` | Bite Mark | +1 Toughness when below half Flesh | "The thing bit down and let go. They think it didn't like the taste." |
| `Scar_BrandedChest` | `SCAR-02` | Branded Chest | Once per hunt: ignore 1 Shaken | "The marrow burn left a pattern on the skin. Settlers say it's a ward. Maybe." |
| `Scar_CrookedNose` | `SCAR-03` | Crooked Nose | +1 Luck on any check that targets monster head | "Broken twice. Set once. Good enough." |
| `Scar_DeepFurrow` | `SCAR-04` | Deep Furrow | Reroll one die when at 1 Flesh | "That close and still standing. Something in them refuses." |
| `Scar_SplitLip` | `SCAR-05` | Split Lip | +1 Grit on first round of any hunt | "Smiled at the wrong thing once. Doesn't stop them smiling." |
| `Scar_MissingSkin` | `SCAR-06` | Missing Skin | +1 Toughness vs poison/rot effects | "The patch of missing skin never grew back. Desensitized, maybe." |
| `Scar_LopedEar` | `SCAR-07` | Lopped Ear | +1 Evasion on rounds hunter has not attacked | "They hear differently now. Better in some ways." |
| `Scar_ClawRaking` | `SCAR-08` | Claw Raking | +1 Accuracy on attacks against the monster part that scarred them | "Knows that limb now. Knows its timing." |

---

## Part 4: Disorder Assets

Create these in `Assets/_Game/Data/Disorders/`.

| Asset | disorderId | disorderName | triggerCondition | mechanicalEffect | flavourText |
|---|---|---|---|---|---|
| `Disorder_Nightmare` | `DIS-01` | Nightmare | At the start of each hunt | Gain Shaken token at round start of Round 1 | "They wake before the camp does. Don't say why." |
| `Disorder_Bloodlust` | `DIS-02` | Bloodlust | When this hunter lands a killing blow | Must spend next action attacking the same target again | "Something in them doesn't stop when it should." |
| `Disorder_Paranoia` | `DIS-03` | Paranoia | When another hunter in the party takes damage | -1 Evasion for the rest of the round | "They flinch when it isn't even aimed at them." |
| `Disorder_Agoraphobia` | `DIS-04` | Agoraphobia | When in an open grid zone (no adjacent obstacles) | -1 Accuracy; -1 Evasion | "Need a wall. Need something at their back." |
| `Disorder_Bloodphobia` | `DIS-05` | Bloodphobia | When this hunter reaches below half Flesh | Cannot play Aggressor cards this round | "They've seen enough blood. Even their own undoes them." |
| `Disorder_Fixation` | `DIS-06` | Fixation | At the start of each hunt | Must target the same monster part as their first attack each round | "They decide before the fight starts. Can't be talked out of it." |
| `Disorder_Catatonia` | `DIS-07` | Catatonia | When this hunter is the only conscious hunter remaining | Skip first action this round | "Alone is the worst thing. They lock up." |
| `Disorder_Megalophobia` | `DIS-08` | Megalophobia | When fighting an overlord-tier monster | -1 Grit; -1 Toughness for the first round | "The size of it breaks something in the mind." |

---

## Part 5: Fighting Art Assets

Create these in `Assets/_Game/Data/FightingArts/`.

| Asset | artId | artName | archetype | unlockYear | mechanicalEffect | flavourText |
|---|---|---|---|---|---|---|
| `FightingArt_Trample` | `FA-01` | Trample | Aggressor | 3 | Once per round: after a successful hit, push monster back 1 space | "The follow-through is the kill. They never stop moving." |
| `FightingArt_QuietStep` | `FA-02` | Quiet Step | Stalker | 2 | First attack each hunt: +2 Accuracy if monster has not yet moved | "They reach the strike before the creature knows they're hunting." |
| `FightingArt_BracePosition` | `FA-03` | Brace Position | Warden | 2 | When adjacent to another hunter: reduce incoming damage by 1 | "They put themselves between the thing and the one next to them." |
| `FightingArt_WoundReading` | `FA-04` | Wound Reading | Scholar | 3 | +1 Accuracy when targeting a part that is already injured | "They study the damage. Know where the creature flinches." |
| `FightingArt_RecklessCharge` | `FA-05` | Reckless Charge | Aggressor | 4 | Once per hunt: gain +3 Accuracy and +1 damage; gain Shaken after | "Everything, all at once. Dangerous for everyone in range." |
| `FightingArt_GhostLeap` | `FA-06` | Ghost Leap | Stalker | 5 | Once per round: move through one occupied grid space without triggering monster reaction | "They've learned the gaps. The creature sees them a second late." |
| `FightingArt_IronStance` | `FA-07` | Iron Stance | Warden | 4 | When stationary two rounds in a row: ignore the next knockback effect | "They've planted roots the creature can't find." |
| `FightingArt_VenomAnalysis` | `FA-08` | Venom Analysis | Scholar | 5 | +2 Toughness vs poison; can identify monster abilities one round early | "They've been poisoned enough to learn the taste." |
| `FightingArt_FrenzyBlow` | `FA-09` | Frenzy Blow | Aggressor | 6 | Once per hunt: deal 3 hits to one part simultaneously | "Not skill. Just violence, concentrated." |
| `FightingArt_AbyssalRead` | `FA-10` | Abyssal Read | Scholar | 7 | Once per hunt: name a monster ability; if correct, gain +2 Evasion this round | "They've seen this one before. Or one just like it." |

---

## Part 6: Hunter Info Panel — Displaying All Four Types

Update the Hunter Info section in `SettlementScreenController` to show a hunter's full record when you click their name.

Add a helper method that resolves IDs to display strings:

```csharp
// In SettlementScreenController.cs

[SerializeField] private InjurySO[]    _allInjuries;
[SerializeField] private ScarSO[]      _allScars;
[SerializeField] private DisorderSO[]  _allDisorders;
[SerializeField] private FightingArtSO[] _allFightingArts;

private void ShowHunterDetail(HunterState hunter)
{
    var root       = _uiDocument.rootVisualElement;
    var detailPanel = root.Q("hunter-detail-panel");
    if (detailPanel == null) return;
    detailPanel.Clear();

    // Name + build
    AddDetailHeader(detailPanel,
        $"{hunter.hunterName.ToUpper()}  ·  {hunter.buildName}",
        $"Year {hunter.yearsActive} Active");

    // Stats
    AddDetailRow(detailPanel, "ACC", hunter.accuracy.ToString());
    AddDetailRow(detailPanel, "EVA", hunter.evasion.ToString());
    AddDetailRow(detailPanel, "TOU", hunter.toughness.ToString());
    AddDetailRow(detailPanel, "SPD", hunter.speed.ToString());
    AddDetailRow(detailPanel, "GRIT", hunter.grit.ToString());
    AddDetailRow(detailPanel, "LUCK", hunter.luck.ToString());

    // Permanent debuffs
    if (!string.IsNullOrEmpty(hunter.permanentDebuffs))
        AddDetailSection(detailPanel, "STATUS", hunter.permanentDebuffs,
            new Color(0.60f, 0.20f, 0.20f));

    // Injuries
    BuildCardSection(detailPanel, "INJURIES", hunter.injuryIds,
        _allInjuries, i => i.injuryId, i => i.injuryName,
        i => i.mechanicalEffect, new Color(0.60f, 0.20f, 0.20f));

    // Scars
    BuildCardSection(detailPanel, "SCARS", hunter.scarIds ?? new string[0],
        _allScars, s => s.scarId, s => s.scarName,
        s => s.mechanicalEffect, new Color(0.54f, 0.54f, 0.54f));

    // Disorders
    BuildCardSection(detailPanel, "DISORDERS", hunter.disorderIds,
        _allDisorders, d => d.disorderId, d => d.disorderName,
        d => d.mechanicalEffect, new Color(0.45f, 0.25f, 0.45f));

    // Fighting Arts
    BuildCardSection(detailPanel, "FIGHTING ARTS", hunter.fightingArtIds,
        _allFightingArts, f => f.artId, f => f.artName,
        f => f.mechanicalEffect, new Color(0.72f, 0.52f, 0.04f));
}

private void BuildCardSection<T>(VisualElement parent, string sectionLabel,
    string[] ids, T[] pool,
    System.Func<T, string> idGetter,
    System.Func<T, string> nameGetter,
    System.Func<T, string> effectGetter,
    Color accentColor)
    where T : UnityEngine.Object
{
    if (ids == null || ids.Length == 0) return;

    var section = new Label(sectionLabel);
    section.style.color    = accentColor;
    section.style.fontSize = 7;
    section.style.marginTop = 10;
    section.style.marginBottom = 4;
    parent.Add(section);

    foreach (var id in ids)
    {
        T found = null;
        foreach (var item in pool)
            if (idGetter(item) == id) { found = item; break; }

        if (found == null) continue;

        var row = new VisualElement();
        row.style.marginBottom = 6;
        row.style.paddingLeft  = 8;
        parent.Add(row);

        var name = new Label(nameGetter(found).ToUpper());
        name.style.color    = new Color(0.83f, 0.80f, 0.73f);
        name.style.fontSize = 9;
        name.style.unityFontStyleAndWeight = FontStyle.Bold;
        row.Add(name);

        var effect = new Label(effectGetter(found));
        effect.style.color     = new Color(0.54f, 0.54f, 0.54f);
        effect.style.fontSize  = 8;
        effect.style.whiteSpace = WhiteSpace.Normal;
        row.Add(effect);
    }
}

private void AddDetailHeader(VisualElement parent, string name, string sub)
{
    var n = new Label(name);
    n.style.color    = new Color(0.83f, 0.80f, 0.73f);
    n.style.fontSize = 12;
    n.style.unityFontStyleAndWeight = FontStyle.Bold;
    n.style.marginBottom = 2;
    parent.Add(n);

    var s = new Label(sub);
    s.style.color    = new Color(0.45f, 0.43f, 0.40f);
    s.style.fontSize = 8;
    s.style.marginBottom = 12;
    parent.Add(s);
}

private void AddDetailRow(VisualElement parent, string label, string value)
{
    var row = new VisualElement();
    row.style.flexDirection = FlexDirection.Row;
    row.style.marginBottom  = 2;
    parent.Add(row);

    var lbl = new Label(label);
    lbl.style.color    = new Color(0.45f, 0.43f, 0.40f);
    lbl.style.fontSize = 8;
    lbl.style.width    = 48;
    row.Add(lbl);

    var val = new Label(value);
    val.style.color    = new Color(0.83f, 0.80f, 0.73f);
    val.style.fontSize = 8;
    row.Add(val);
}

private void AddDetailSection(VisualElement parent, string label,
                               string value, Color color)
{
    var lbl = new Label($"{label}: {value}");
    lbl.style.color    = color;
    lbl.style.fontSize = 8;
    lbl.style.marginBottom = 4;
    parent.Add(lbl);
}
```

Add `scarIds` to `HunterState` if not already present:

```csharp
public string[] scarIds;    // Complement to injuryIds — may have been omitted in earlier stages
```

---

## Part 7: Grant System — How a Hunter Gains These Cards

Add these methods to `GameStateManager`:

```csharp
public void GrantInjury(string hunterId, string injuryId)
{
    var hunter = FindHunter(hunterId);
    if (hunter == null) return;
    hunter.injuryIds = AppendId(hunter.injuryIds, injuryId);
    Debug.Log($"[Injury] {hunter.hunterName} gained {injuryId}");
}

public void GrantScar(string hunterId, string scarId)
{
    var hunter = FindHunter(hunterId);
    if (hunter == null) return;
    hunter.scarIds = AppendId(hunter.scarIds, scarId);
    Debug.Log($"[Scar] {hunter.hunterName} gained {scarId}");
}

public void GrantDisorder(string hunterId, string disorderId)
{
    var hunter = FindHunter(hunterId);
    if (hunter == null) return;
    hunter.disorderIds = AppendId(hunter.disorderIds, disorderId);
    Debug.Log($"[Disorder] {hunter.hunterName} gained {disorderId}");
}

public void GrantFightingArt(string hunterId, string artId)
{
    var hunter = FindHunter(hunterId);
    if (hunter == null) return;
    hunter.fightingArtIds = AppendId(hunter.fightingArtIds, artId);
    Debug.Log($"[FightingArt] {hunter.hunterName} learned {artId}");
}

private HunterState FindHunter(string hunterId)
{
    if (CampaignState.hunters == null) return null;
    foreach (var h in CampaignState.hunters)
        if (h.hunterId == hunterId) return h;
    return null;
}

private string[] AppendId(string[] existing, string id)
{
    var list = new System.Collections.Generic.List<string>(
        existing ?? new string[0]);
    if (!list.Contains(id)) list.Add(id);
    return list.ToArray();
}
```

---

## Verification Test

- [ ] All four SO classes compile without errors in Unity
- [ ] All 10 Injury assets exist in `Assets/_Game/Data/Injuries/`
- [ ] All 8 Scar assets exist in `Assets/_Game/Data/Scars/`
- [ ] All 8 Disorder assets exist in `Assets/_Game/Data/Disorders/`
- [ ] All 10 Fighting Art assets exist in `Assets/_Game/Data/FightingArts/`
- [ ] Each asset has non-empty injuryId/scarId/disorderId/artId and mechanicalEffect
- [ ] Click a hunter name in Settlement → detail panel shows stats section
- [ ] Grant INJ-01 to Aldric via Debug call → "INJURIES: BROKEN ARM / -1 Accuracy..." appears in panel
- [ ] Grant FA-01 to Aldric → "FIGHTING ARTS: TRAMPLE / Once per round..." appears
- [ ] Hunter with no cards → no section headers shown (no empty sections)
- [ ] Two disorders shown correctly — separate entries, not merged

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_B.md`
**Covers:** 4-Way Directional Sprites & Facing Logic — generating north/south/east/west sprites for all 8 hunter build types, updating the HunterTokenController to flip/swap sprites based on movement direction, and implementing the facing system on the combat grid
