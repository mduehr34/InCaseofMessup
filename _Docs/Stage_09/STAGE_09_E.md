<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-E | Gear Grid Adjacency & Link Bonus Logic
Status: Stage 9-D complete. Gear overlays working.
Task: Implement the gear grid adjacency bonus system. Each
hunter has a 3×3 gear grid. Gear pieces placed in adjacent
cells from the same craft set grant a "Link Bonus" — a small
extra stat benefit. Detect adjacency, calculate the total
bonus, display it in the gear panel, and apply it to the
hunter's effective stats in combat.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_E.md
- Assets/_Game/Scripts/Core.Data/GearSO.cs
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- The gear grid is 3×3 (9 slots total) — some slots are
  locked based on hunter build
- GearSO has a craftSet field (e.g. "Carapace")
- Two adjacent same-set pieces = 1 link bonus
- Link bonus definitions live in CraftSetSO ScriptableObjects
- Effective stats = base stats + injury penalties + link bonuses
- What you will NOT build (diagonal adjacency — orthogonal
  only for MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-E: Gear Grid Adjacency & Link Bonus Logic

**Resuming from:** Stage 9-D complete — gear overlay sprite system working
**Done when:** Adjacent same-set gear pieces on the hunter's gear grid grant displayed link bonuses; effective combat stats account for bonuses and injury penalties
**Commit:** `"9E: Gear grid adjacency and link bonus logic — CraftSetSO, adjacency detection, effective stats"`
**Next session:** STAGE_09_F.md

---

## What the Gear Grid Is

Each hunter has a 3×3 grid of gear slots:

```
[ 0,2 ] [ 1,2 ] [ 2,2 ]    ← Row 2 (top)
[ 0,1 ] [ 1,1 ] [ 2,1 ]    ← Row 1 (middle)
[ 0,0 ] [ 1,0 ] [ 2,0 ]    ← Row 0 (bottom)
```

Each cell can hold one gear piece. Cells are identified by (x, y) where (0,0) is bottom-left.

**Adjacency rule:** Two cells are adjacent if they share an edge (orthogonal only — not diagonal). So (0,0) is adjacent to (1,0) and (0,1). It is NOT adjacent to (1,1).

**Link Bonus rule:** If two adjacent cells contain gear pieces from the same craft set (e.g., both "Carapace"), the hunter gains the set's link bonus once per pair.

---

## Part 1: CraftSetSO.cs

**Path:** `Assets/_Game/Scripts/Core.Data/CraftSetSO.cs`

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "CraftSet_", menuName = "MnM/Craft Set")]
    public class CraftSetSO : ScriptableObject
    {
        [Header("Identity")]
        public string setId;         // e.g. "Carapace"
        public string setName;       // Display name: "Carapace Forge"

        [Header("Link Bonus")]
        [TextArea(2, 3)]
        public string linkBonusDescription;
        // Human-readable: "+1 Toughness per link pair"

        // Stat bonuses per link pair (additive)
        public int accuracyPerLink;
        public int evasionPerLink;
        public int toughnessPerLink;
        public int speedPerLink;
        public int gritPerLink;
        public int luckPerLink;

        [Header("Full Set Bonus (all 5 slots from same set)")]
        [TextArea(2, 3)]
        public string fullSetBonusDescription;
        public int fullSetAccuracy;
        public int fullSetEvasion;
        public int fullSetToughness;
        public int fullSetSpeed;
        public int fullSetGrit;
        public int fullSetLuck;
    }
}
```

### Craft Set Assets

Create these in `Assets/_Game/Data/CraftSets/`:

| Asset | setId | setName | Link Bonus (per pair) | Full Set Bonus |
|---|---|---|---|---|
| `CraftSet_Carapace` | `Carapace` | Carapace Forge | +1 Toughness | +2 Toughness, +1 Evasion |
| `CraftSet_Membrane` | `Membrane` | Membrane Loft | +1 Evasion | +2 Evasion, +1 Speed |
| `CraftSet_Ichor` | `Ichor` | Ichor Works | +1 Grit | +2 Grit, +1 Toughness |
| `CraftSet_Auric` | `Auric` | Auric Scales | +1 Accuracy | +2 Accuracy, +1 Luck |
| `CraftSet_Rot` | `Rot` | Rot Garden | +1 Luck | +2 Luck, +1 Grit |
| `CraftSet_Ivory` | `Ivory` | Ivory Hall | +1 Speed | +2 Speed, +1 Accuracy |
| `CraftSet_Mire` | `Mire` | Mire Apothecary | +1 Grit, +1 Luck | +1 all stats |

---

## Part 2: Update GearSO

Add to `GearSO.cs`:

```csharp
[Header("Gear Grid")]
public string craftSet;        // "Carapace" | "Membrane" | "Ichor" | etc.
public Vector2Int gridSlot;    // Default slot position: (x, y) in the 3×3 grid
```

---

## Part 3: GearGridState

Add to `HunterState`:

```csharp
// The 3×3 gear grid — stored as a flat array of 9 gear IDs.
// Index = y * 3 + x. Empty slot = null or "".
public string[] gearGrid;  // Length 9
```

Convenience accessors — add to a partial class or extension:

```csharp
public static string GetGridSlot(string[] grid, int x, int y)
{
    if (grid == null || grid.Length < 9) return null;
    return grid[y * 3 + x];
}

public static void SetGridSlot(string[] grid, int x, int y, string gearId)
{
    if (grid == null || grid.Length < 9) return;
    grid[y * 3 + x] = gearId;
}
```

---

## Part 4: AdjacencyBonusCalculator.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/AdjacencyBonusCalculator.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public static class AdjacencyBonusCalculator
    {
        private static readonly Vector2Int[] Orthogonal =
        {
            new( 1,  0),
            new(-1,  0),
            new( 0,  1),
            new( 0, -1),
        };

        /// <summary>
        /// Calculate total link bonus stats for a hunter based on their gear grid.
        /// Returns a StatBonus with accumulated stat bonuses.
        /// </summary>
        public static StatBonus Calculate(string[] gearGrid,
                                          GearSO[]      allGear,
                                          CraftSetSO[]  allSets)
        {
            var result = new StatBonus();
            if (gearGrid == null || gearGrid.Length < 9) return result;

            // Track which pairs we've already counted (to avoid double-counting)
            var counted = new HashSet<(int, int)>();

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    string aId = GearGridState.GetGridSlot(gearGrid, x, y);
                    if (string.IsNullOrEmpty(aId)) continue;

                    GearSO aGear = FindGear(aId, allGear);
                    if (aGear == null || string.IsNullOrEmpty(aGear.craftSet)) continue;

                    foreach (var dir in Orthogonal)
                    {
                        int bx = x + dir.x;
                        int by = y + dir.y;
                        if (bx < 0 || bx > 2 || by < 0 || by > 2) continue;

                        string bId = GearGridState.GetGridSlot(gearGrid, bx, by);
                        if (string.IsNullOrEmpty(bId)) continue;

                        GearSO bGear = FindGear(bId, allGear);
                        if (bGear == null || bGear.craftSet != aGear.craftSet) continue;

                        // Create a canonical pair key (smaller index first)
                        int idxA = y * 3 + x;
                        int idxB = by * 3 + bx;
                        var pairKey = (Mathf.Min(idxA, idxB), Mathf.Max(idxA, idxB));
                        if (counted.Contains(pairKey)) continue;
                        counted.Add(pairKey);

                        // Add link bonus
                        CraftSetSO set = FindSet(aGear.craftSet, allSets);
                        if (set == null) continue;

                        result.accuracy  += set.accuracyPerLink;
                        result.evasion   += set.evasionPerLink;
                        result.toughness += set.toughnessPerLink;
                        result.speed     += set.speedPerLink;
                        result.grit      += set.gritPerLink;
                        result.luck      += set.luckPerLink;
                    }
                }
            }

            // Full set bonus check
            ApplyFullSetBonuses(gearGrid, allGear, allSets, result);

            return result;
        }

        private static void ApplyFullSetBonuses(string[] grid, GearSO[] allGear,
                                                  CraftSetSO[] allSets, StatBonus result)
        {
            // Count pieces per set
            var setCounts = new Dictionary<string, int>();
            for (int i = 0; i < 9; i++)
            {
                if (string.IsNullOrEmpty(grid[i])) continue;
                var gear = FindGear(grid[i], allGear);
                if (gear == null || string.IsNullOrEmpty(gear.craftSet)) continue;
                if (!setCounts.ContainsKey(gear.craftSet)) setCounts[gear.craftSet] = 0;
                setCounts[gear.craftSet]++;
            }

            foreach (var kvp in setCounts)
            {
                if (kvp.Value < 5) continue; // Full set = 5+ pieces
                var set = FindSet(kvp.Key, allSets);
                if (set == null) continue;
                result.accuracy  += set.fullSetAccuracy;
                result.evasion   += set.fullSetEvasion;
                result.toughness += set.fullSetToughness;
                result.speed     += set.fullSetSpeed;
                result.grit      += set.fullSetGrit;
                result.luck      += set.fullSetLuck;
            }
        }

        public static int CountLinkedPairs(string[] gearGrid, GearSO[] allGear,
                                            string setId)
        {
            // Helper used by UI to show "X link pairs active"
            int count = 0;
            var counted = new HashSet<(int, int)>();

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    string aId = GearGridState.GetGridSlot(gearGrid, x, y);
                    if (string.IsNullOrEmpty(aId)) continue;
                    GearSO aGear = FindGear(aId, allGear);
                    if (aGear?.craftSet != setId) continue;

                    foreach (var dir in Orthogonal)
                    {
                        int bx = x + dir.x, by = y + dir.y;
                        if (bx < 0 || bx > 2 || by < 0 || by > 2) continue;
                        string bId = GearGridState.GetGridSlot(gearGrid, bx, by);
                        if (string.IsNullOrEmpty(bId)) continue;
                        GearSO bGear = FindGear(bId, allGear);
                        if (bGear?.craftSet != setId) continue;

                        int idxA = y * 3 + x, idxB = by * 3 + bx;
                        var key = (Mathf.Min(idxA, idxB), Mathf.Max(idxA, idxB));
                        if (counted.Contains(key)) continue;
                        counted.Add(key);
                        count++;
                    }
                }
            }
            return count;
        }

        private static GearSO FindGear(string id, GearSO[] pool)
        {
            foreach (var g in pool)
                if (g != null && g.gearId == id) return g;
            return null;
        }

        private static CraftSetSO FindSet(string setId, CraftSetSO[] pool)
        {
            foreach (var s in pool)
                if (s != null && s.setId == setId) return s;
            return null;
        }
    }

    [System.Serializable]
    public class StatBonus
    {
        public int accuracy;
        public int evasion;
        public int toughness;
        public int speed;
        public int grit;
        public int luck;

        public bool IsEmpty =>
            accuracy == 0 && evasion == 0 && toughness == 0 &&
            speed    == 0 && grit    == 0 && luck     == 0;
    }

    // Static helper used by AdjacencyBonusCalculator
    public static class GearGridState
    {
        public static string GetGridSlot(string[] grid, int x, int y)
        {
            if (grid == null || grid.Length < 9) return null;
            return grid[y * 3 + x];
        }
        public static void SetGridSlot(string[] grid, int x, int y, string gearId)
        {
            if (grid == null || grid.Length < 9) return;
            grid[y * 3 + x] = gearId;
        }
    }
}
```

---

## Part 5: Effective Stat Calculation

Add to `GameStateManager` or a utility class:

```csharp
/// <summary>
/// Returns the hunter's effective stat total after injuries and link bonuses.
/// </summary>
public EffectiveStats GetEffectiveStats(HunterState hunter)
{
    var bonus = AdjacencyBonusCalculator.Calculate(
        hunter.gearGrid, _allGear, _allCraftSets);

    int injAccuracy  = 0, injToughness = 0, injSpeed = 0, injGrit = 0;
    // Parse injury penalties
    if (hunter.injuryIds != null)
        foreach (var injId in hunter.injuryIds)
        {
            var inj = FindInjury(injId);
            if (inj == null) continue;
            // Parse mechanicalEffect string like "-1 Accuracy;-1 Speed"
            ParseInjuryPenalties(inj.mechanicalEffect,
                ref injAccuracy, ref injToughness, ref injSpeed, ref injGrit);
        }

    return new EffectiveStats
    {
        accuracy  = hunter.accuracy  + bonus.accuracy  - injAccuracy,
        evasion   = hunter.evasion   + bonus.evasion,
        toughness = hunter.toughness + bonus.toughness - injToughness,
        speed     = hunter.speed     + bonus.speed     - injSpeed,
        grit      = hunter.grit      + bonus.grit      - injGrit,
        luck      = hunter.luck      + bonus.luck,
        linkBonus = bonus,
    };
}

private void ParseInjuryPenalties(string effect,
    ref int acc, ref int tou, ref int spd, ref int grt)
{
    if (string.IsNullOrEmpty(effect)) return;
    var parts = effect.Split(';');
    foreach (var part in parts)
    {
        var p = part.Trim();
        if (p.Contains("Accuracy"))  { if (p.StartsWith("-")) acc  += GetNum(p); }
        if (p.Contains("Toughness")) { if (p.StartsWith("-")) tou  += GetNum(p); }
        if (p.Contains("Speed"))     { if (p.StartsWith("-")) spd  += GetNum(p); }
        if (p.Contains("Grit"))      { if (p.StartsWith("-")) grt  += GetNum(p); }
    }
}

private int GetNum(string s)
{
    // Extract the absolute number from strings like "-1 Accuracy"
    foreach (var token in s.Split(' '))
        if (int.TryParse(token.Replace("-", ""), out int n)) return n;
    return 0;
}
```

```csharp
[System.Serializable]
public class EffectiveStats
{
    public int accuracy;
    public int evasion;
    public int toughness;
    public int speed;
    public int grit;
    public int luck;
    public StatBonus linkBonus;
}
```

---

## Part 6: Settlement Gear Panel — Link Bonus Display

In the settlement gear panel, after showing the gear grid, display active link bonuses:

```csharp
private void RefreshLinkBonusDisplay(HunterState hunter)
{
    var root      = _uiDocument.rootVisualElement;
    var bonusPanel = root.Q("link-bonus-panel");
    if (bonusPanel == null) return;
    bonusPanel.Clear();

    var effective = GameStateManager.Instance.GetEffectiveStats(hunter);
    var bonus     = effective.linkBonus;

    if (bonus.IsEmpty)
    {
        var none = new Label("No link bonuses active.");
        none.style.color    = new Color(0.35f, 0.33f, 0.28f);
        none.style.fontSize = 8;
        bonusPanel.Add(none);
        return;
    }

    var header = new Label("LINK BONUSES");
    header.style.color    = new Color(0.72f, 0.52f, 0.04f);
    header.style.fontSize = 8;
    header.style.marginBottom = 4;
    bonusPanel.Add(header);

    void AddBonusRow(string stat, int val)
    {
        if (val == 0) return;
        var row = new Label($"+{val} {stat}");
        row.style.color    = new Color(0.40f, 0.70f, 0.40f);
        row.style.fontSize = 8;
        bonusPanel.Add(row);
    }

    AddBonusRow("Accuracy",  bonus.accuracy);
    AddBonusRow("Evasion",   bonus.evasion);
    AddBonusRow("Toughness", bonus.toughness);
    AddBonusRow("Speed",     bonus.speed);
    AddBonusRow("Grit",      bonus.grit);
    AddBonusRow("Luck",      bonus.luck);
}
```

Add to the Settlement UXML gear panel:

```xml
<ui:VisualElement name="link-bonus-panel"
    style="margin-top:16px; padding:8px;
           border-color:rgba(184,134,11,0.3); border-width:1px;" />
```

---

## Part 7: Gear Grid Drag-and-Drop UI

The gear grid in the settlement needs to let players drag gear pieces into grid cells. Since UIToolkit drag-and-drop requires careful event handling:

```csharp
private void BuildGearGrid(HunterState hunter)
{
    var root = _uiDocument.rootVisualElement;
    var grid = root.Q("gear-grid-cells");
    if (grid == null) return;
    grid.Clear();

    // Ensure the grid array exists
    if (hunter.gearGrid == null || hunter.gearGrid.Length < 9)
        hunter.gearGrid = new string[9];

    for (int y = 2; y >= 0; y--)   // Top row first visually
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        grid.Add(row);

        for (int x = 0; x < 3; x++)
        {
            int capturedX = x, capturedY = y;
            string gearId = GearGridState.GetGridSlot(hunter.gearGrid, x, y);
            GearSO gear   = string.IsNullOrEmpty(gearId) ? null : FindGearById(gearId);

            var cell = new VisualElement();
            cell.style.width           = 48;
            cell.style.height          = 48;
            cell.style.marginRight     = cell.style.marginBottom = 4;
            cell.style.backgroundColor = new StyleColor(new Color(0.08f, 0.07f, 0.05f));
            cell.style.borderTopColor  = cell.style.borderBottomColor =
            cell.style.borderLeftColor = cell.style.borderRightColor =
                gear != null
                    ? GetSetColor(gear.craftSet)
                    : new StyleColor(new Color(0.20f, 0.18f, 0.14f));
            cell.style.borderTopWidth  = cell.style.borderBottomWidth =
            cell.style.borderLeftWidth = cell.style.borderRightWidth = 1;
            cell.style.alignItems      = Align.Center;
            cell.style.justifyContent  = Justify.Center;

            if (gear != null)
            {
                if (gear.overlaySprite != null)
                {
                    var img = new VisualElement();
                    img.style.width           = 36;
                    img.style.height          = 36;
                    img.style.backgroundImage = new StyleBackground(gear.overlaySprite);
                    img.style.backgroundScaleMode = new StyleEnum<ScaleMode>(ScaleMode.ScaleToFit);
                    cell.Add(img);
                }
                else
                {
                    var lbl = new Label(gear.gearName.Substring(0, Mathf.Min(4, gear.gearName.Length)));
                    lbl.style.color    = new Color(0.72f, 0.52f, 0.04f);
                    lbl.style.fontSize = 7;
                    cell.Add(lbl);
                }

                // Right-click to unequip
                cell.RegisterCallback<ContextClickEvent>(_ =>
                {
                    GearGridState.SetGridSlot(hunter.gearGrid, capturedX, capturedY, null);
                    BuildGearGrid(hunter);
                    RefreshLinkBonusDisplay(hunter);
                });
            }

            row.Add(cell);
        }
    }
}

private StyleColor GetSetColor(string setId)
{
    return setId switch
    {
        "Carapace" => new StyleColor(new Color(0.40f, 0.30f, 0.10f)),
        "Membrane" => new StyleColor(new Color(0.15f, 0.30f, 0.40f)),
        "Ichor"    => new StyleColor(new Color(0.30f, 0.15f, 0.40f)),
        "Auric"    => new StyleColor(new Color(0.50f, 0.45f, 0.10f)),
        "Rot"      => new StyleColor(new Color(0.20f, 0.35f, 0.15f)),
        "Ivory"    => new StyleColor(new Color(0.45f, 0.45f, 0.45f)),
        "Mire"     => new StyleColor(new Color(0.25f, 0.35f, 0.25f)),
        _          => new StyleColor(new Color(0.20f, 0.18f, 0.14f)),
    };
}
```

---

## Verification Test

- [ ] All 7 CraftSet SO assets exist in `Assets/_Game/Data/CraftSets/`
- [ ] Place two Carapace pieces in adjacent grid cells → "+1 Toughness" appears in link bonus panel
- [ ] Place two Carapace pieces in diagonal cells → NO bonus (diagonal not adjacent)
- [ ] Place three Carapace pieces in a row → TWO pairs detected → "+2 Toughness" bonus
- [ ] Place 5 Carapace pieces → full set bonus added on top of link bonuses
- [ ] Place Carapace + Membrane adjacent → no link bonus (different sets)
- [ ] Remove one piece → link bonus recalculates immediately
- [ ] Combat: hunter with +1 Toughness link bonus has effective toughness +1
- [ ] Hunter with Broken Arm injury (-1 Accuracy): effective accuracy is base - 1
- [ ] Hunter with Broken Arm + Auric link bonus (+1 Accuracy): bonuses cancel, net = 0
- [ ] Gear grid cells show craft set border colours (Carapace = amber, Membrane = blue)
- [ ] Right-click a gear cell → gear removed, grid redraws

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_F.md`
**Covers:** Thornback Behavior Cards — full 16-card behavior deck for the Thornback monster, including attack patterns, movement behaviors, and special abilities, as BehaviorCardSO assets wired into the Thornback MonsterSO
