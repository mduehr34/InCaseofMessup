<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-P | Craft Sets Part 1 — Carapace, Membrane & Mire
Status: Stage 9-O complete. All three overlords done.
Task: Build all craftable GearSO assets for three craft sets:
Carapace Forge, Membrane Loft, and Mire Apothecary. Each set
needs 6 craftable items (head, chest, arms, weapon, off-hand,
and one accessory). Each item needs a resource recipe, stat
bonuses, gearSlot, craftSet, and a reference to its overlay
sprite. Wire items into the Settlement Crafting panel.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_P.md
- Assets/_Game/Scripts/Core.Data/GearSO.cs
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- GearSO has: gearId, gearName, gearSlot, craftSet, recipe
  (ResourceCost[]), statBonuses (StatBonus), overlaySprite,
  description
- ResourceCost is a struct: resourceType (string) + amount (int)
- Craft sets are only available in the settlement crafting panel
  if they are unlocked (in CampaignState.unlockedCraftSetIds)
- Carapace Forge is always available (starter set)
- What you will NOT build (gear icons in the UI — overlay sprites
  are referenced but not generated in this session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-P: Craft Sets Part 1 — Carapace, Membrane & Mire

**Resuming from:** Stage 9-O complete — all three overlords done
**Done when:** 18 GearSO assets exist across three craft sets; crafting panel shows the correct items for each unlocked set; crafting an item consumes resources and adds the gear to the hunter's inventory
**Commit:** `"9P: Craft sets part 1 — Carapace Forge, Membrane Loft, Mire Apothecary GearSO assets"`
**Next session:** STAGE_09_Q.md

---

## GearSO — Confirm Full Field Set

Open `Assets/_Game/Scripts/Core.Data/GearSO.cs` and ensure all these fields exist:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Gear_", menuName = "MnM/Gear Item")]
    public class GearSO : ScriptableObject
    {
        [Header("Identity")]
        public string  gearId;           // e.g. "GEAR-CAR-01"
        public string  gearName;         // e.g. "Carapace Helm"
        public string  gearSlot;         // "Head" | "Chest" | "Arms" | "Weapon" | "OffHand" | "Accessory"
        public string  craftSet;         // "Carapace" | "Membrane" | "Ichor" | etc.

        [TextArea(2, 4)]
        public string  description;      // Flavour + mechanical summary

        [Header("Recipe")]
        public ResourceCost[] recipe;    // Resources needed to craft

        [Header("Stat Bonuses")]
        public int bonusAccuracy;
        public int bonusEvasion;
        public int bonusToughness;
        public int bonusSpeed;
        public int bonusGrit;
        public int bonusLuck;

        [Header("Special")]
        [TextArea(1, 3)]
        public string specialEffect;     // Optional unique mechanic beyond stat bonuses

        [Header("Visuals")]
        public Sprite overlaySprite;     // Drawn on top of hunter token
    }

    [System.Serializable]
    public struct ResourceCost
    {
        public string resourceType;   // "Bone" | "Hide" | "Sinew" | "Ichor" | "Ivory" | "Membrane" | "RotGland"
        public int    amount;
    }
}
```

---

## Crafting System — GameStateManager Methods

Add if not already present:

```csharp
public bool CanCraft(GearSO gear)
{
    if (gear?.recipe == null) return false;
    foreach (var cost in gear.recipe)
    {
        int have = GetResourceAmount(cost.resourceType);
        if (have < cost.amount) return false;
    }
    return true;
}

public void CraftItem(GearSO gear, string hunterId)
{
    if (!CanCraft(gear)) return;

    // Consume resources
    foreach (var cost in gear.recipe)
        SpendResource(cost.resourceType, cost.amount);

    // Add to hunter's inventory (equippedGearIds for now — future: a bag)
    GrantGearToHunter(hunterId, gear.gearId);

    // Track yearly craft count
    CampaignState.yearItemsCrafted++;

    // Chronicle (optional — only for significant gear)
    Debug.Log($"[Craft] {gear.gearName} crafted for hunter {hunterId}.");
}

private int GetResourceAmount(string type) => type switch
{
    "Bone"     => CampaignState.bone,
    "Hide"     => CampaignState.hide,
    "Sinew"    => CampaignState.sinew,
    "Ichor"    => CampaignState.ichor,
    "Ivory"    => CampaignState.ivory,
    "Membrane" => CampaignState.membrane,
    "RotGland" => CampaignState.rotGland,
    _          => 0
};

private void SpendResource(string type, int amount)
{
    switch (type)
    {
        case "Bone":     CampaignState.bone     -= amount; break;
        case "Hide":     CampaignState.hide     -= amount; break;
        case "Sinew":    CampaignState.sinew    -= amount; break;
        case "Ichor":    CampaignState.ichor    -= amount; break;
        case "Ivory":    CampaignState.ivory    -= amount; break;
        case "Membrane": CampaignState.membrane -= amount; break;
        case "RotGland": CampaignState.rotGland -= amount; break;
    }
}

public void GrantGearToHunter(string hunterId, string gearId)
{
    var hunter = FindHunter(hunterId);
    if (hunter == null) return;
    hunter.equippedGearIds = AppendId(hunter.equippedGearIds, gearId);
}
```

---

## Set 1: Carapace Forge

**Always unlocked — starter set.**
**Flavour:** Armour built from the shell and bones of the monsters you've already killed. Hard, heavy, reliable.
**Resources used:** Bone, Hide, Sinew

Create in `Assets/_Game/Data/Gear/Carapace/`.

### CAR-01 — Carapace Helm
```
gearId: GEAR-CAR-01
gearName: Carapace Helm
gearSlot: Head
craftSet: Carapace
recipe: Bone×3, Hide×2
bonusToughness: +1
bonusGrit: +1
description: "A half-skull helm of fused chitin plates. Heavy but reassuring."
specialEffect: —
```

### CAR-02 — Carapace Chest
```
gearId: GEAR-CAR-02
gearName: Carapace Chest
gearSlot: Chest
craftSet: Carapace
recipe: Bone×5, Sinew×3
bonusToughness: +2
description: "Layered chitin plates over a padded hide backing. The best protection the
  settlement can produce in the early years."
specialEffect: —
```

### CAR-03 — Carapace Vambraces
```
gearId: GEAR-CAR-03
gearName: Carapace Vambraces
gearSlot: Arms
craftSet: Carapace
recipe: Bone×2, Hide×2
bonusToughness: +1
bonusAccuracy: +1
description: "Forearm guards with a stabilising ridge on the back of the wrist. Hunters
  say it steadies the weapon arm."
specialEffect: —
```

### CAR-04 — Bone Cleaver
```
gearId: GEAR-CAR-04
gearName: Bone Cleaver
gearSlot: Weapon
craftSet: Carapace
recipe: Bone×4, Sinew×2
bonusAccuracy: +2
description: "A heavy chopping blade ground from a large femur. Crude, effective.
  Does not break against shell."
specialEffect: Attacks against Shell parts deal +1 damage.
```

### CAR-05 — Shell Shield
```
gearId: GEAR-CAR-05
gearName: Shell Shield
gearSlot: OffHand
craftSet: Carapace
recipe: Bone×3, Hide×3
bonusEvasion: +1
bonusToughness: +1
description: "A curved section of monster shell, strapped to the forearm.
  Not elegant but it stops things."
specialEffect: Once per hunt: reduce one incoming attack by 2 damage.
```

### CAR-06 — Bone Amulet
```
gearId: GEAR-CAR-06
gearName: Bone Amulet
gearSlot: Accessory
craftSet: Carapace
recipe: Bone×2
bonusLuck: +1
description: "A small carved bone worn at the throat. Settlers who wear them
  say they feel watched over. Settlers who don't say the same thing."
specialEffect: —
```

---

## Set 2: Membrane Loft

**Unlocked after first standard-tier hunt (Year 1 auto-unlocked if Membrane resource collected).**
**Flavour:** Supple armour made from stretched monster membrane and sinew binding. Light, fast, whispering.
**Resources used:** Membrane, Sinew, Hide

Create in `Assets/_Game/Data/Gear/Membrane/`.

### MEM-01 — Membrane Hood
```
gearId: GEAR-MEM-01
gearName: Membrane Hood
gearSlot: Head
craftSet: Membrane
recipe: Membrane×2, Sinew×1
bonusEvasion: +1
bonusSpeed: +1
description: "A close-fitted hood of stretched monster membrane. It muffles sound
  from the wearer's ears — useful for staying quiet."
specialEffect: —
```

### MEM-02 — Membrane Vest
```
gearId: GEAR-MEM-02
gearName: Membrane Vest
gearSlot: Chest
craftSet: Membrane
recipe: Membrane×4, Sinew×2
bonusEvasion: +2
bonusSpeed: +1
description: "A multi-layered vest that moves with the hunter's body.
  Less protection than Carapace. More room to move."
specialEffect: —
```

### MEM-03 — Wrapped Bracers
```
gearId: GEAR-MEM-03
gearName: Wrapped Bracers
gearSlot: Arms
craftSet: Membrane
recipe: Membrane×2, Sinew×2
bonusEvasion: +1
bonusAccuracy: +1
description: "Sinew-wrapped forearm bindings with a membrane liner.
  Keeps the grip steady and the joints loose."
specialEffect: —
```

### MEM-04 — Slick Blades
```
gearId: GEAR-MEM-04
gearName: Slick Blades
gearSlot: Weapon
craftSet: Membrane
recipe: Bone×2, Membrane×2, Sinew×1
bonusAccuracy: +1
bonusSpeed: +1
description: "Twin short blades coated in rendered membrane fat.
  They don't bite as deep but they never stick."
specialEffect: On a hit: reduce monster's counter-attack by 1 (if any).
```

### MEM-05 — Membrane Wrap (OffHand)
```
gearId: GEAR-MEM-05
gearName: Membrane Wrap
gearSlot: OffHand
craftSet: Membrane
recipe: Membrane×2, Sinew×2
bonusEvasion: +2
description: "A thick wrap of layered membrane worn on the forearm.
  Not a shield — more of a deflection surface."
specialEffect: When dodging (Evasion success): no Shaken on a near-miss.
```

### MEM-06 — Sinew Brace
```
gearId: GEAR-MEM-06
gearName: Sinew Brace
gearSlot: Accessory
craftSet: Membrane
recipe: Sinew×3
bonusSpeed: +1
bonusGrit: +1
description: "A tight sinew brace around the dominant wrist.
  Hunters swear it makes the arm feel faster. Maybe they're right."
specialEffect: —
```

---

## Set 3: Mire Apothecary

**Unlocked after killing The Siltborn.**
**Flavour:** Treatments and augmentations from bog-harvested ichor and rot. Strange effects. Not for the faint of heart.
**Resources used:** Ichor, RotGland, Sinew

Create in `Assets/_Game/Data/Gear/Mire/`.

### MIR-01 — Bog-Glass Mask
```
gearId: GEAR-MIR-01
gearName: Bog-Glass Mask
gearSlot: Head
craftSet: Mire
recipe: Ichor×3, Sinew×1
bonusGrit: +2
bonusLuck: +1
description: "A hardened ichor mask that filters the air the hunter breathes.
  Everything smells like the marsh now. They don't seem to mind."
specialEffect: Immune to Poison status effect.
```

### MIR-02 — Rot-Treated Coat
```
gearId: GEAR-MIR-02
gearName: Rot-Treated Coat
gearSlot: Chest
craftSet: Mire
recipe: RotGland×3, Hide×2, Sinew×2
bonusToughness: +1
bonusGrit: +2
description: "A hide coat treated in dissolved rot gland. It stinks. It also
  seems to confuse monsters — they hesitate before attacking the wearer."
specialEffect: Monsters targeting this hunter lose −1 Accuracy on the first attack each round.
```

### MIR-03 — Ichor Gauntlets
```
gearId: GEAR-MIR-03
gearName: Ichor Gauntlets
gearSlot: Arms
craftSet: Mire
recipe: Ichor×2, Sinew×2
bonusAccuracy: +1
bonusGrit: +1
description: "Gauntlets coated in hardened ichor resin. Weapons grip better.
  The resin also absorbs some of the feedback from hard strikes."
specialEffect: —
```

### MIR-04 — Rot Spear
```
gearId: GEAR-MIR-04
gearName: Rot Spear
gearSlot: Weapon
craftSet: Mire
recipe: Bone×3, RotGland×2, Sinew×2
bonusAccuracy: +2
bonusLuck: +1
description: "A bone spear whose tip has been treated in rot gland extract.
  Hits leave a residue that weakens flesh over time."
specialEffect: On a hit: target gains Poison (1 Flesh/rd, 1 round).
```

### MIR-05 — Ichor Flask (OffHand)
```
gearId: GEAR-MIR-05
gearName: Ichor Flask
gearSlot: OffHand
craftSet: Mire
recipe: Ichor×4
bonusGrit: +1
description: "A flask of concentrated ichor worn at the belt.
  Hunters use it to dull pain mid-combat. The effects are... pronounced."
specialEffect: Once per hunt: spend this item to restore 3 Flesh HP instantly.
  Gain Shaken for 1 round after use.
```

### MIR-06 — Bog Stone
```
gearId: GEAR-MIR-06
gearName: Bog Stone
gearSlot: Accessory
craftSet: Mire
recipe: RotGland×1, Ichor×1
bonusLuck: +2
description: "A smooth stone extracted from deep in the marsh, still warm.
  No one knows what it does, exactly. Hunters who carry them seem to survive
  things that should have killed them."
specialEffect: Once per campaign: reroll any single d10 roll. Choose the result you prefer.
```

---

## Crafting Panel UI Update

In `SettlementScreenController`, update the Crafting tab to filter by unlocked sets:

```csharp
private void BuildCraftingPanel()
{
    var root = _uiDocument.rootVisualElement;
    var list = root.Q("crafting-recipe-list");
    if (list == null) return;
    list.Clear();

    var state    = GameStateManager.Instance.CampaignState;
    var unlocked = state.unlockedCraftSetIds ?? new string[0];

    // Always show Carapace
    var activeSets = new System.Collections.Generic.List<string>{ "Carapace" };
    foreach (var setId in unlocked)
        if (!activeSets.Contains(setId)) activeSets.Add(setId);

    foreach (var gear in _allGear)
    {
        if (gear == null) continue;
        if (!activeSets.Contains(gear.craftSet)) continue;

        bool canCraft = GameStateManager.Instance.CanCraft(gear);
        BuildRecipeCard(list, gear, canCraft);
    }
}

private void BuildRecipeCard(VisualElement parent, GearSO gear, bool canCraft)
{
    var card = new VisualElement();
    card.style.marginBottom     = 8;
    card.style.padding          = new StyleEnum<Align>(Align.Auto); // reuse padding pattern
    card.style.paddingTop       = card.style.paddingBottom =
    card.style.paddingLeft      = card.style.paddingRight = 10;
    card.style.backgroundColor  = new StyleColor(new Color(0.07f, 0.06f, 0.04f));
    card.style.borderTopColor   = card.style.borderBottomColor =
    card.style.borderLeftColor  = card.style.borderRightColor  =
        canCraft
            ? new StyleColor(new Color(0.31f, 0.27f, 0.20f))
            : new StyleColor(new Color(0.18f, 0.16f, 0.12f));
    card.style.borderTopWidth   = card.style.borderBottomWidth =
    card.style.borderLeftWidth  = card.style.borderRightWidth  = 1;
    parent.Add(card);

    // Header row
    var header = new VisualElement();
    header.style.flexDirection = FlexDirection.Row;
    header.style.justifyContent = Justify.SpaceBetween;
    card.Add(header);

    var nameLabel = new Label(gear.gearName.ToUpper());
    nameLabel.style.color    = canCraft
        ? new Color(0.83f, 0.80f, 0.73f)
        : new Color(0.40f, 0.38f, 0.34f);
    nameLabel.style.fontSize = 10;
    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    header.Add(nameLabel);

    var slotLabel = new Label($"{gear.gearSlot}  ·  {gear.craftSet}");
    slotLabel.style.color    = new Color(0.45f, 0.43f, 0.40f);
    slotLabel.style.fontSize = 8;
    header.Add(slotLabel);

    // Recipe
    var recipeRow = new VisualElement();
    recipeRow.style.flexDirection = FlexDirection.Row;
    recipeRow.style.marginTop = 4;
    card.Add(recipeRow);

    if (gear.recipe != null)
    {
        foreach (var cost in gear.recipe)
        {
            int have = GameStateManager.Instance
                .CampaignState != null
                ? GetResourceAmount(cost.resourceType)
                : 0;
            bool enough = have >= cost.amount;
            var costLabel = new Label($"{cost.resourceType} ×{cost.amount}");
            costLabel.style.color    = enough
                ? new Color(0.40f, 0.70f, 0.40f)
                : new Color(0.70f, 0.30f, 0.30f);
            costLabel.style.fontSize = 8;
            costLabel.style.marginRight = 10;
            recipeRow.Add(costLabel);
        }
    }

    // Stat bonuses
    var bonusText = BuildBonusString(gear);
    if (!string.IsNullOrEmpty(bonusText))
    {
        var bonusLabel = new Label(bonusText);
        bonusLabel.style.color    = new Color(0.40f, 0.70f, 0.40f);
        bonusLabel.style.fontSize = 8;
        bonusLabel.style.marginTop = 4;
        card.Add(bonusLabel);
    }

    // Special effect
    if (!string.IsNullOrEmpty(gear.specialEffect))
    {
        var specLabel = new Label(gear.specialEffect);
        specLabel.style.color     = new Color(0.72f, 0.52f, 0.04f);
        specLabel.style.fontSize  = 8;
        specLabel.style.whiteSpace = WhiteSpace.Normal;
        specLabel.style.marginTop = 2;
        card.Add(specLabel);
    }

    // Craft button
    if (canCraft)
    {
        var craftBtn = new Button { text = "CRAFT" };
        craftBtn.style.alignSelf = Align.FlexEnd;
        craftBtn.style.marginTop = 8;
        craftBtn.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.2f));
        craftBtn.style.borderTopColor   = craftBtn.style.borderBottomColor =
        craftBtn.style.borderLeftColor  = craftBtn.style.borderRightColor  =
            new StyleColor(new Color(0.72f, 0.52f, 0.04f));
        craftBtn.style.borderTopWidth   = craftBtn.style.borderBottomWidth =
        craftBtn.style.borderLeftWidth  = craftBtn.style.borderRightWidth  = 1;
        craftBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
        craftBtn.style.fontSize = 9;
        craftBtn.RegisterCallback<ClickEvent>(_ =>
        {
            // TODO: show hunter picker if multiple hunters; for now craft for hunter[0]
            string hunterId = GameStateManager.Instance.CampaignState.hunters?[0]?.hunterId ?? "";
            GameStateManager.Instance.CraftItem(gear, hunterId);
            BuildCraftingPanel();  // Refresh
            _settlementAnim?.AnimateCraftSuccess(card);
        });
        card.Add(craftBtn);
    }
}

private string BuildBonusString(GearSO gear)
{
    var parts = new System.Collections.Generic.List<string>();
    if (gear.bonusAccuracy  != 0) parts.Add($"+{gear.bonusAccuracy} ACC");
    if (gear.bonusEvasion   != 0) parts.Add($"+{gear.bonusEvasion} EVA");
    if (gear.bonusToughness != 0) parts.Add($"+{gear.bonusToughness} TOU");
    if (gear.bonusSpeed     != 0) parts.Add($"+{gear.bonusSpeed} SPD");
    if (gear.bonusGrit      != 0) parts.Add($"+{gear.bonusGrit} GRT");
    if (gear.bonusLuck      != 0) parts.Add($"+{gear.bonusLuck} LCK");
    return string.Join("  ", parts);
}

private int GetResourceAmount(string type)
{
    var state = GameStateManager.Instance.CampaignState;
    return type switch
    {
        "Bone"     => state.bone,
        "Hide"     => state.hide,
        "Sinew"    => state.sinew,
        "Ichor"    => state.ichor,
        "Ivory"    => state.ivory,
        "Membrane" => state.membrane,
        "RotGland" => state.rotGland,
        _          => 0
    };
}
```

---

## Verification Test

- [ ] All 18 GearSO assets created (6 per set) in correct folders
- [ ] Each asset has non-empty gearId, gearName, gearSlot, craftSet
- [ ] Each asset has at least one recipe cost
- [ ] Crafting panel shows Carapace items at campaign start (always unlocked)
- [ ] Membrane items hidden until Membrane resources are collected (or unlock condition)
- [ ] Mire items hidden until Siltborn killed (Mire craft set unlocked)
- [ ] Recipe costs shown in green when hunter has enough resources
- [ ] Recipe costs shown in red when insufficient
- [ ] Click CRAFT with sufficient resources → resources deducted, item added to hunter
- [ ] Craft animation flash plays on the card
- [ ] Craft panel refreshes after craft — CRAFT button disappears if no longer affordable
- [ ] CAR-04 (Bone Cleaver) special effect: +1 damage vs Shell — can verify via Debug log
- [ ] MIR-05 (Ichor Flask) special: use once per hunt → grant 3 Flesh, apply Shaken
- [ ] MIR-06 (Bog Stone) special: reroll one d10 (log "Bog Stone reroll used")
- [ ] Gear bonuses appear in the effective stats calculation (Stage 9-E system)

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_Q.md`
**Covers:** Craft Sets Part 2 — building all recipe assets for the Ichor Works, Auric Scales, Rot Garden, and Ivory Hall craft sets (completing the full 7-set gear economy)
