<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-R | Gear Logic — Links, Set Bonuses, Once-Per-Hunt, Consumables
Status: Stage 7-Q complete. Balance pass verified.
Task: Implement all gear special-effect logic that was
deferred during Stage 7-O item creation:
  1. Directional link resolution + stat application
  2. Set bonus counting (2/3/5-piece)
  3. Once-per-hunt ability tracking
  4. Consumable use, removal, and adjacency targeting

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_R.md
- Assets/_Game/Scripts/Core.Data/DataStructs.cs      ← LinkPoint lives here
- Assets/_Game/Scripts/Core.Data/ItemSO.cs
- Assets/_Game/Scripts/Core.Data/CombatState.cs      ← HunterCombatState
- Assets/_Game/Scripts/Core.Logic/GearLinkResolver.cs
- Assets/_Game/Scripts/Core.UI/GearGridController.cs ← gear grid layout

Then confirm:
- You understand the current LinkPoint struct (affinityTag + direction only)
- You will extend LinkPoint with bonus stat fields, NOT add a parallel array
- HunterCombatState will gain spentHuntAbilities[] for once-per-hunt tracking
- What you will NOT do this session (other monster craft sets, Suture deck)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-R: Gear Logic — Links, Set Bonuses, Once-Per-Hunt, Consumables

**Resuming from:** Stage 7-Q complete  
**Done when:** All five verification tests below pass against the Aldric / Gaunt Skull Cap + Gaunt Hide Vest loadout  
**Commit:** `"7R: Gear logic — directional links, set bonuses, once-per-hunt tracking, consumable use"`  
**Next session:** Stage 8 (TBD)

---

## Context — What Was Deferred from Stage 7-O

`GearLinkResolver` detects affinity matches but does not:
- Check link *direction* (above/below/right) against gear grid position
- Apply any stat delta from a link — `LinkBonus` only carries a text description
- Count set pieces or apply 2/3/5-piece set bonuses
- Track once-per-hunt abilities (EyePendant scar discard, 5-piece collapse survival)
- Remove consumables on use or check hunter adjacency for targeting

All five gaps are addressed in this session.

---

## Step 1 — Extend `LinkPoint` with Bonus Stats (`DataStructs.cs`)

**Note:** `DataStructs.cs` is a Stage 1 file. After this change, re-run the Stage 1 verification test (compile + Aldric SO inspectable) before continuing.

Add bonus stat fields directly to the existing `LinkPoint` struct:

```csharp
[System.Serializable]
public struct LinkPoint
{
    public string affinityTag;
    public Vector2Int direction;        // Which edge of THIS item the link exits from
    // Stat bonus applied when this link is active (most are a single +1 to one stat)
    public int bonusAccuracy;
    public int bonusStrength;
    public int bonusToughness;
    public int bonusEvasion;
    public int bonusLuck;
    public int bonusMovement;
}
```

After editing, re-populate the `LinkPoint` bonus fields on each Gaunt armor asset. Values from the stage 7-O spec:

| Asset | direction | Active bonus |
|---|---|---|
| `Item_GauntSkullCap` | (0, -1) below | bonusAccuracy: +1 |
| `Item_GauntHideVest` | (0, +1) above + (0, -1) below | bonusToughness: +1 (requires BOTH links active) |
| `Item_GauntSinewWrap` | (0, +1) above + (0, -1) below | bonusMovement: +1 (requires BOTH links active) |
| `Item_GauntBoneBracers` | (+1, 0) right | bonusStrength: +1 |
| `Item_GauntHideBoots` | (0, +1) above | bonusEvasion: +1 |

> **Dual-link items (HideVest, SinewWrap):** a link bonus that requires BOTH directions active should only fire when BOTH `LinkPoint` entries on that item resolve. Store the bonus on **both** `LinkPoint` entries with a value of +1; the resolver sums them — if only one direction is connected, only half the bonus is attempted. Since both link points carry the same stat, the effective result is the resolver sees `+1` only when both link points fire, which is correct as long as the resolver deduplicates per-item bonuses. See Step 2 resolver note.

---

## Step 2 — Rework `GearLinkResolver` for Directional Resolution

`GearLinkResolver.ResolveLinks()` currently takes `ItemSO[]`. It needs to know **where** each item sits on the gear grid to evaluate directional links.

**New signature:**

```csharp
// GearGridSlot: item + its top-left cell on the gear grid
public struct GearGridSlot
{
    public ItemSO item;
    public Vector2Int cell;     // Top-left anchor cell of this item on the gear grid
}

public static LinkBonus[] ResolveLinks(GearGridSlot[] loadout)
```

**Resolution logic (per link point on item A):**

1. Compute the neighbor cell: `cell_A + linkPoint.direction`
2. Find any item B in `loadout` whose footprint (`cell_B` to `cell_B + gridDimensions - (1,1)`) contains that neighbor cell
3. If item B's `affinityTags` contains `linkPoint.affinityTag` → link is active
4. Accumulate `linkPoint.bonus*` into a `StatModifiers` delta for item A
5. Deduplication: track (itemA, itemB, direction) triplets — don't double-count

**`LinkBonus` struct — add delta:**

```csharp
public struct LinkBonus
{
    public string itemAName;
    public string itemBName;
    public string affinityTag;
    public string effectDescription;
    public StatModifiers delta;     // ← new: the numeric stat change from this link
}
```

**Dual-link deduplication note (HideVest/SinewWrap):** both link points on these items carry the same bonus stat. Use a `HashSet<string>` keyed on `"itemName_statName"` to ensure a given stat bonus from a given item is counted at most once per `ResolveLinks` call, regardless of how many link points resolved.

**`SumEquippedStats` update:** call `ResolveLinks` internally and add all `delta` values on top of base item stat sums. Callers only need `SumEquippedStats(GearGridSlot[] loadout)` — no separate link pass needed.

---

## Step 3 — Set Bonus Resolver

### 3a — Add `SetBonusEntry[]` to `ItemSO`

The set bonus data is currently free-text in `Item_GauntHideVest.specialEffect`. Make it machine-readable.

Add to `ItemSO.cs`:

```csharp
[Header("Set Bonuses")]
public SetBonusEntry[] setBonuses;   // Populated only on the anchor piece (HideVest)
```

Add to `DataStructs.cs`:

```csharp
[System.Serializable]
public struct SetBonusEntry
{
    public int requiredPieceCount;  // 2, 3, or 5
    // Flat stat bonuses while this threshold is met
    public int bonusAccuracy;
    public int bonusStrength;
    public int bonusToughness;
    public int bonusEvasion;
    public int bonusLuck;
    public int bonusMovement;
    // Non-stat effect — resolved by CombatManager via string tag
    public string effectTag;        // e.g. "GAUNT_3PC_LOUD_SUPPRESS", "GAUNT_5PC_DEATH_CHEAT"
    [TextArea] public string effectDescription;
}
```

### 3b — Populate `Item_GauntHideVest.setBonuses`

| requiredPieceCount | Stat bonus | effectTag | effectDescription |
|---|---|---|---|
| 2 | bonusEvasion: +1 | — | +1 Evasion flat while 2+ GAUNT pieces equipped |
| 3 | — | GAUNT_3PC_LOUD_SUPPRESS | Behavior cards triggered by Loud card plays have movement effect -2 squares |
| 5 | — | GAUNT_5PC_DEATH_CHEAT | Once per hunt: when this hunter would collapse, survive with 1 Flesh on struck part |

### 3c — New `SetBonusResolver` (static class in `Core.Logic`)

```csharp
public static class SetBonusResolver
{
    // Returns total stat mods from all active set bonuses in this loadout
    public static StatModifiers ResolveSetBonuses(ItemSO[] equippedItems, out string[] activeEffectTags)
```

Logic:
1. Group equipped items by `setNameTag` (skip empty)
2. For each group, find the anchor item — the one with `setBonuses != null && setBonuses.Length > 0`
3. For each `SetBonusEntry` on the anchor: if `equippedCount >= requiredPieceCount`, apply its stat mods and collect its `effectTag`
4. Return `StatModifiers` total + `activeEffectTags[]`

### 3d — Surface `activeEffectTags` on `HunterCombatState`

Add to `HunterCombatState`:
```csharp
public string[] activeGearEffectTags;   // e.g. ["GAUNT_3PC_LOUD_SUPPRESS"]
```

`CombatManager` reads this when resolving Loud card plays (3-piece suppression) and collapse detection (5-piece survival). Do NOT implement those handlers in this session — add `// TODO: 7R — handle GAUNT_3PC_LOUD_SUPPRESS` and `// TODO: 7R — handle GAUNT_5PC_DEATH_CHEAT` stubs so they're findable.

---

## Step 4 — Once-Per-Hunt Tracking

### 4a — Add tracking field to `HunterCombatState`

```csharp
public string[] spentHuntAbilities;     // Item names whose once-per-hunt effect has fired
```

### 4b — `EyePendant` scar discard

When a hunter draws a Scar Card:
1. Check `HunterCombatState.equippedItemNames` contains `"Gaunt Eye Pendant"`
2. Check `spentHuntAbilities` does NOT contain `"Gaunt Eye Pendant"`
3. If both: present discard option in UI. On confirm: card is not applied; add `"Gaunt Eye Pendant"` to `spentHuntAbilities`

Add a `// TODO: 7R — EyePendant scar intercept` stub in `CombatManager` at the injury card application site.

### 4c — 5-piece collapse survival

When `HunterCombatState.isCollapsed` would be set to `true`:
1. Check `activeGearEffectTags` contains `"GAUNT_5PC_DEATH_CHEAT"`
2. Check `spentHuntAbilities` does NOT contain `"GAUNT_5PC_DEATH_CHEAT"`
3. If both: set the struck `BodyZoneState.fleshCurrent = 1` instead of triggering collapse; add `"GAUNT_5PC_DEATH_CHEAT"` to `spentHuntAbilities`; log `"[GearEffect] 5-piece Gaunt set: collapse prevented — 1 Flesh remaining on [zone]"`

Add a `// TODO: 7R — GAUNT_5PC_DEATH_CHEAT collapse intercept` stub in `CombatManager` at the collapse trigger site.

---

## Step 5 — Consumable Use and Removal

### 5a — `ConsumableResolver` (static class in `Core.Logic`)

```csharp
public static class ConsumableResolver
{
    // Returns true if hunter at (ax, ay) can target hunter at (bx, by) with a consumable
    public static bool IsValidConsumableTarget(int ax, int ay, int bx, int by)
    {
        return Mathf.Abs(ax - bx) <= 1 && Mathf.Abs(ay - by) <= 1;
    }

    // Applies BoneSplint effect: +2 Shell to the named zone, capped at shellMax
    public static void ApplyBoneSplint(HunterCombatState target, string zoneName)
    {
        for (int i = 0; i < target.bodyZones.Length; i++)
        {
            if (target.bodyZones[i].zone == zoneName)
            {
                target.bodyZones[i].shellCurrent =
                    Mathf.Min(target.bodyZones[i].shellCurrent + 2,
                              target.bodyZones[i].shellMax);
                Debug.Log($"[Consumable] BoneSplint: {target.hunterName} {zoneName} " +
                          $"shell restored to {target.bodyZones[i].shellCurrent}");
                return;
            }
        }
        Debug.LogWarning($"[Consumable] BoneSplint: zone '{zoneName}' not found on {target.hunterName}");
    }
}
```

### 5b — Remove consumable from `RuntimeCharacterState` on use

In the settlement/loadout system (wherever items are used):
- After `ApplyBoneSplint` resolves, remove `"Bone Splint"` from `RuntimeCharacterState.equippedItemNames`
- Log: `"[Consumable] Bone Splint consumed — removed from {characterName} loadout"`

### 5c — `isConsumable` guard in `GearLinkResolver`

Consumables should never generate link bonuses. Add to `ResolveLinks`:
```csharp
if (itemA.isConsumable) continue;
```

---

## Update Existing Gaunt Armor Assets

After extending `LinkPoint` with bonus stats (Step 1), run an Editor script to update the existing 5 armor assets in `Assets/_Game/Data/Items/Gaunt/` with the correct bonus values. Use the table in Step 1 as the source of truth.

After adding `setBonuses` to `ItemSO` (Step 3), run an Editor script to populate `Item_GauntHideVest.setBonuses` with the three set bonus entries.

---

## Verification Tests

### Test 1 — Directional link fires correctly
1. Equip Aldric with `Gaunt Skull Cap` (cell 0,0) and `Gaunt Hide Vest` (cell 0,1)
2. Call `GearLinkResolver.SumEquippedStats(loadout)`
3. Expected log: `[GearLink] Link active: Gaunt Skull Cap ↔ Gaunt Hide Vest: GAUNT link active`
4. Expected stat delta includes `accuracy: +1` (SkullCap link) + `toughness: +1` (HideVest upper link)

### Test 2 — Link does NOT fire when spatial condition is unmet
1. Equip Aldric with `Gaunt Skull Cap` (cell 0,0) and `Gaunt Bone Bracers` (cell 3,0)
2. SkullCap's link point direction is (0,-1) — looks DOWN for a GAUNT piece
3. BoneBracers are to the right, not below — link should NOT fire
4. Expected log: `[GearLink] No active links in current loadout`

### Test 3 — 2-piece set bonus applies
1. Equip `Gaunt Skull Cap` + `Gaunt Hide Vest` (2 GAUNT pieces)
2. Call `SetBonusResolver.ResolveSetBonuses(equippedItems, out tags)`
3. Expected: `StatModifiers.evasion == 1`, `tags` contains no effect tags (2-piece is stat only)

### Test 4 — Once-per-hunt spent flag blocks second use
1. Set `HunterCombatState.spentHuntAbilities = ["Gaunt Eye Pendant"]`
2. Trigger Scar Card draw for a hunter with EyePendant equipped
3. Confirm discard option is NOT presented (ability spent)

### Test 5 — BoneSplint adjacency and removal
1. Hunter A at (3,3), Hunter B at (4,3) — adjacent ✓
2. Hunter A at (3,3), Hunter C at (6,3) — not adjacent ✗
3. `ConsumableResolver.IsValidConsumableTarget(3,3,4,3)` → `true`
4. `ConsumableResolver.IsValidConsumableTarget(3,3,6,3)` → `false`
5. Apply BoneSplint to Hunter B zone "LeftArm" (shellCurrent 0, shellMax 3) → shellCurrent becomes 2
6. After use: `"Bone Splint"` absent from `RuntimeCharacterState.equippedItemNames`

---

## What This Session Will NOT Do

- Other monster craft sets (Spite, Suture, Thornback, Serpent, Stampede)
- Suture behavior deck (pending design decision)
- Implementing `GAUNT_3PC_LOUD_SUPPRESS` handler in behavior card resolution (stubbed TODO only)
- Full UI for consumable targeting (stub the resolver; wire UI in a later session)
- Gear grid visual layout (GearGridController UI already exists; this session only adds resolver logic)

---

## Next Session

**File:** Stage 8 (TBD)  
**Covers:** Post-Stage 7 content — Suture behavior deck design session, or remaining crafter buildings, depending on design decisions made after Stage 7-Q balance pass.
