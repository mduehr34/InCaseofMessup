<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-E | Gear Grid Screen
Status: Stage 6-D complete. Chronicle event modal, Guiding
Principal modal, Crafters and Innovations tabs all working.
Task: Create gear-grid.uxml and GearGridController.cs.
Build the 4×4 gear grid with item placement via click (not
drag-drop — simpler and stable). Wire GearLinkResolver to
update stats summary on equip. Show consumable slots.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_E.md
- Assets/_Game/Scripts/Core.Logic/GearLinkResolver.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/Scripts/Core.Data/ItemSO.cs
- Assets/_Game/UI/USS/tokens.uss
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- That item equipping uses click-to-select + click-to-place
  NOT drag-and-drop (UI Toolkit drag-drop is complex;
  click-based is stable and functional)
- That GearLinkResolver is called on every equip change
- That stats summary shows base + gear bonus separately
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-E: Gear Grid Screen

**Resuming from:** Stage 6-D complete  
**Done when:** Gear Grid opens from Character loadout button; 4×4 grid shows equipped items; clicking an item selects it; clicking a cell equips/moves it; stats summary updates with link bonuses; consumable slots visible  
**Commit:** `"6E: Gear Grid — 4×4 layout, item equip, link resolver, stats summary"`  
**Next session:** STAGE_06_F.md  

---

## GDD Spec — Gear Grid Layout

```
┌────────────────┬─────────────────────────┬──────────────────┐
│ HUNTER         │  4×4 GEAR GRID          │  ITEM DETAILS    │
│ PORTRAIT       │  (96×96px per cell)     │  PANEL           │
│ (Left 240px)   │  (Center ~400px)        │  (Right 400px)   │
│                ├─────────────────────────┤                  │
│                │  CONSUMABLE SLOTS (3)   │  SET BONUS       │
│                │  (below gear grid)      │  TRACKER         │
└────────────────┴─────────────────────────┴──────────────────┘
│  STATS SUMMARY (bottom, full width)                         │
└─────────────────────────────────────────────────────────────┘
```

---

## Step 1: gear-grid.uxml

**Path:** `Assets/_Game/UI/UXML/gear-grid.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>
    <Style src="../USS/gear-grid.uss"/>

    <ui:VisualElement name="gear-grid-root" class="fullscreen-bg gear-grid-root">

        <!-- Header -->
        <ui:VisualElement class="era-bar stone-panel--raised">
            <ui:Label name="hunter-name-header" text="Loadout" class="era-year"/>
            <ui:VisualElement style="flex:1"/>
            <ui:Button name="btn-close" text="CLOSE" class="era-btn"/>
        </ui:VisualElement>

        <!-- Main Content -->
        <ui:VisualElement name="gear-main" class="gear-main">

            <!-- Left: Hunter Portrait placeholder -->
            <ui:VisualElement name="hunter-portrait" class="hunter-portrait stone-panel">
                <ui:Label name="portrait-name" text="---" class="stone-panel__header"/>
                <ui:Label name="portrait-build" text="---" class="proficiency-label"/>
            </ui:VisualElement>

            <!-- Center: Grid + Consumables -->
            <ui:VisualElement name="gear-center" class="gear-center">
                <ui:Label text="GEAR GRID" class="stone-panel__header"/>
                <ui:VisualElement name="gear-grid-4x4" class="gear-grid-4x4"/>
                <ui:Label text="CONSUMABLES" class="stone-panel__header"/>
                <ui:VisualElement name="consumable-slots" class="consumable-slots-row">
                    <ui:VisualElement name="consumable-0" class="consumable-slot stone-panel"/>
                    <ui:VisualElement name="consumable-1" class="consumable-slot stone-panel"/>
                    <ui:VisualElement name="consumable-2" class="consumable-slot stone-panel"/>
                </ui:VisualElement>
            </ui:VisualElement>

            <!-- Right: Item Details + Set Bonus -->
            <ui:VisualElement name="item-detail-panel" class="item-detail-panel stone-panel">
                <ui:Label name="item-name"      text="Select an item" class="stone-panel__header"/>
                <ui:Label name="item-tier"      text=""               class="proficiency-label"/>
                <ui:Label name="item-stats"     text=""               class="item-stat-block"/>
                <ui:Label name="item-links"     text=""               class="proficiency-label"/>
                <ui:Label name="item-special"   text=""               class="event-narrative"/>
                <ui:VisualElement class="stone-panel--raised" style="margin-top:8px; padding:8px">
                    <ui:Label text="SET BONUS"            class="stone-panel__header"/>
                    <ui:Label name="set-name"  text="---" class="proficiency-label"/>
                    <ui:Label name="set-bonus" text="---" class="proficiency-label"/>
                </ui:VisualElement>
                <ui:Button name="btn-unequip" text="UNEQUIP" class="action-btn action-btn--danger"/>
            </ui:VisualElement>

        </ui:VisualElement>

        <!-- Stats Summary Bar -->
        <ui:VisualElement name="stats-bar" class="stats-bar stone-panel--raised">
            <ui:Label name="stat-accuracy"  text="ACC: 0+0"  class="stat-summary-label"/>
            <ui:Label name="stat-strength"  text="STR: 0+0"  class="stat-summary-label"/>
            <ui:Label name="stat-evasion"   text="EVA: 0+0"  class="stat-summary-label"/>
            <ui:Label name="stat-toughness" text="TGH: 0+0"  class="stat-summary-label"/>
            <ui:Label name="stat-luck"      text="LCK: 0+0"  class="stat-summary-label"/>
            <ui:Label name="stat-movement"  text="MOV: 3+0"  class="stat-summary-label"/>
            <ui:Label name="active-links"   text=""           class="proficiency-label"/>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

---

## Step 2: gear-grid.uss

**Path:** `Assets/_Game/UI/USS/gear-grid.uss`

```css
.gear-grid-root { flex-direction: column; }

.gear-main {
    flex:           1;
    flex-direction: row;
    min-height:     0;
}

.hunter-portrait {
    width:           200px;
    flex-shrink:     0;
    flex-direction:  column;
    align-items:     center;
    margin:          2px;
}

.gear-center {
    flex-direction: column;
    align-items:    center;
    padding:        var(--spacing-sm);
    flex-shrink:    0;
}

.gear-grid-4x4 {
    width:          384px;
    height:         384px;
    flex-wrap:      wrap;
    flex-direction: row;
}

.gear-cell {
    width:            96px;
    height:           96px;
    border-color:     var(--color-border);
    border-width:     1px;
    border-radius:    0px;
    background-color: var(--color-bg-deep);
    align-items:      center;
    justify-content:  center;
}

.gear-cell:hover {
    border-color: var(--color-border-accent);
}

.gear-cell--occupied {
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-accent);
}

.gear-cell--selected {
    border-color:     var(--color-text-accent);
    border-width:     2px;
}

.gear-cell-label {
    font-size:   var(--font-size-small);
    color:       var(--color-text-dim);
    white-space: normal;
    -unity-text-align: middle-center;
}

.consumable-slots-row {
    flex-direction: row;
    margin-top:     var(--spacing-sm);
}

.consumable-slot {
    width:      120px;
    height:     80px;
    margin:     0 var(--spacing-xs);
    align-items:      center;
    justify-content:  center;
    border-color: var(--color-border-accent);
}

.item-detail-panel {
    flex:           1;
    flex-direction: column;
    margin:         2px;
    overflow:       hidden;
}

.item-stat-block {
    font-size:    var(--font-size-label);
    color:        var(--color-text-primary);
    white-space:  normal;
    margin:       var(--spacing-xs) 0;
}

.stats-bar {
    flex-direction: row;
    height:         50px;
    align-items:    center;
    padding:        0 var(--spacing-md);
    flex-shrink:    0;
}

.stat-summary-label {
    font-size:        var(--font-size-body);
    color:            var(--color-text-primary);
    -unity-font-style: bold;
    margin-right:     var(--spacing-lg);
}
```

---

## Step 3: GearGridController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/GearGridController.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Logic;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class GearGridController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _root;
        private VisualElement _grid4x4;
        private VisualElement[,] _cells = new VisualElement[4, 4];

        // Current character being managed
        private RuntimeCharacterState _character;
        private CampaignSO            _campaignSO;

        // Item placement state
        private string _selectedItemName = null;  // Item name picked up from grid or list
        private int    _selectedCellX    = -1;
        private int    _selectedCellY    = -1;

        // Flat list: which item is in each cell (null = empty)
        // Grid is 4×4 = 16 cells, stored as [x + y*4]
        private string[] _gridContents = new string[16];

        // ── Open / Init ──────────────────────────────────────────
        public void Open(RuntimeCharacterState character, CampaignSO campaignSO)
        {
            _character  = character;
            _campaignSO = campaignSO;

            _root = _uiDocument.rootVisualElement;
            BuildGrid();
            WireButtons();
            LoadEquippedItems();
            RefreshAll();
        }

        private void WireButtons()
        {
            _root.Q<Button>("btn-close").clicked   += OnClose;
            _root.Q<Button>("btn-unequip").clicked += OnUnequip;

            _root.Q<Label>("hunter-name-header").text = _character.characterName;
            _root.Q<Label>("portrait-name").text      = _character.characterName;
            _root.Q<Label>("portrait-build").text     = $"{_character.sex} · {_character.bodyBuild}";
        }

        // ── Grid Build ───────────────────────────────────────────
        private void BuildGrid()
        {
            _grid4x4 = _root.Q<VisualElement>("gear-grid-4x4");
            if (_grid4x4 == null) return;
            _grid4x4.Clear();

            for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
            {
                var cell = new VisualElement();
                cell.AddToClassList("gear-cell");

                int cx = x, cy = y;
                cell.RegisterCallback<ClickEvent>(_ => OnCellClicked(cx, cy));

                _cells[x, y] = cell;
                _grid4x4.Add(cell);
            }
        }

        private void LoadEquippedItems()
        {
            // Place equipped items in grid sequentially (left-to-right, top-to-bottom)
            _gridContents = new string[16];
            if (_character.equippedItemNames == null) return;

            for (int i = 0; i < _character.equippedItemNames.Length && i < 16; i++)
                _gridContents[i] = _character.equippedItemNames[i];
        }

        // ── Cell Click ───────────────────────────────────────────
        private void OnCellClicked(int x, int y)
        {
            int idx = x + y * 4;

            if (_selectedItemName != null)
            {
                // Place selected item into this cell
                // Remove from old cell if it came from the grid
                if (_selectedCellX >= 0)
                {
                    int oldIdx = _selectedCellX + _selectedCellY * 4;
                    _gridContents[oldIdx] = null;
                }
                _gridContents[idx] = _selectedItemName;
                _selectedItemName = null;
                _selectedCellX    = -1;
                _selectedCellY    = -1;
                SaveEquippedItems();
                RefreshAll();
            }
            else if (_gridContents[idx] != null)
            {
                // Pick up item from this cell
                _selectedItemName = _gridContents[idx];
                _selectedCellX    = x;
                _selectedCellY    = y;
                ShowItemDetail(_selectedItemName);
                RefreshGridVisuals();
            }
            else
            {
                // Empty cell clicked with no selection — deselect
                _selectedItemName = null;
                _selectedCellX    = -1;
                _selectedCellY    = -1;
                RefreshGridVisuals();
            }
        }

        private void OnUnequip()
        {
            if (_selectedCellX < 0) return;
            int idx = _selectedCellX + _selectedCellY * 4;
            _gridContents[idx] = null;
            _selectedItemName  = null;
            _selectedCellX     = -1;
            _selectedCellY     = -1;
            SaveEquippedItems();
            RefreshAll();
        }

        // ── Visuals ──────────────────────────────────────────────
        private void RefreshAll()
        {
            RefreshGridVisuals();
            RefreshStatsSummary();
        }

        private void RefreshGridVisuals()
        {
            for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
            {
                var cell = _cells[x, y];
                if (cell == null) continue;
                cell.Clear();

                int idx     = x + y * 4;
                bool hasItem = !string.IsNullOrEmpty(_gridContents[idx]);
                bool isSelected = _selectedCellX == x && _selectedCellY == y;

                cell.EnableInClassList("gear-cell--occupied", hasItem);
                cell.EnableInClassList("gear-cell--selected",  isSelected);

                if (hasItem)
                {
                    var lbl = new Label(_gridContents[idx]);
                    lbl.AddToClassList("gear-cell-label");
                    cell.Add(lbl);
                }
            }
        }

        private void ShowItemDetail(string itemName)
        {
            var item = LoadItemSO(itemName);

            _root.Q<Label>("item-name").text = item != null ? item.itemName : itemName;
            _root.Q<Label>("item-tier").text = item != null ? $"Tier {item.materialTier}" : "";

            if (item != null)
            {
                var statParts = new List<string>();
                if (item.accuracyMod  != 0) statParts.Add($"ACC {item.accuracyMod:+0;-0}");
                if (item.strengthMod  != 0) statParts.Add($"STR {item.strengthMod:+0;-0}");
                if (item.evasionMod   != 0) statParts.Add($"EVA {item.evasionMod:+0;-0}");
                if (item.toughnessMod != 0) statParts.Add($"TGH {item.toughnessMod:+0;-0}");
                if (item.luckMod      != 0) statParts.Add($"LCK {item.luckMod:+0;-0}");
                if (item.movementMod  != 0) statParts.Add($"MOV {item.movementMod:+0;-0}");
                _root.Q<Label>("item-stats").text   = string.Join("  ", statParts);
                _root.Q<Label>("item-special").text = item.specialEffect ?? "";
                _root.Q<Label>("set-name").text     = string.IsNullOrEmpty(item.setNameTag)
                    ? "No set" : item.setNameTag;
            }
        }

        private void RefreshStatsSummary()
        {
            // Load all equipped ItemSOs
            var equippedSOs = _gridContents
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(LoadItemSO)
                .Where(s => s != null)
                .ToArray();

            var gearStats = GearLinkResolver.SumEquippedStats(equippedSOs);
            var links     = GearLinkResolver.ResolveLinks(equippedSOs);

            // Base stats from character
            _root.Q<Label>("stat-accuracy").text  = $"ACC {_character.accuracy}+{gearStats.accuracy}";
            _root.Q<Label>("stat-strength").text  = $"STR {_character.strength}+{gearStats.strength}";
            _root.Q<Label>("stat-evasion").text   = $"EVA {_character.evasion}+{gearStats.evasion}";
            _root.Q<Label>("stat-toughness").text = $"TGH {_character.toughness}+{gearStats.toughness}";
            _root.Q<Label>("stat-luck").text      = $"LCK {_character.luck}+{gearStats.luck}";
            _root.Q<Label>("stat-movement").text  = $"MOV {_character.movement}+{gearStats.movement}";

            _root.Q<Label>("active-links").text = links.Length > 0
                ? $"{links.Length} link{(links.Length == 1 ? "" : "s")} active"
                : "";
        }

        private void SaveEquippedItems()
        {
            _character.equippedItemNames = _gridContents
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
        }

        private void OnClose()
        {
            // Return to Settlement screen — reload Settlement scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Settlement");
        }

        // Resources.Load fallback — Stage 7 wires a proper registry
        private ItemSO LoadItemSO(string itemName) =>
            Resources.Load<ItemSO>($"Data/Items/{itemName}");
    }
}
```

Also update `SettlementScreenController.OpenGearGrid()`:

```csharp
private void OpenGearGrid(string characterId)
{
    var state = GameStateManager.Instance.CampaignState;
    var ch    = System.Array.Find(state.characters, c => c.characterId == characterId);
    if (ch == null) return;
    // Load Gear Grid scene (additive or replace — simple replace for now)
    UnityEngine.SceneManagement.SceneManager.LoadScene("GearGrid");
    // GearGridController.Open() called by the GearGrid scene on load via a bootstrapper
    // Store characterId in GameStateManager for the new scene to pick up
    Debug.Log($"[Settlement] Opening gear grid for {ch.characterName}");
}
```

Add `GearGrid` to Build Settings (index 6).

---

## Verification Test

1. From Settlement → Characters tab → click LOADOUT on a character
2. Gear Grid screen loads
3. 4×4 grid (16 cells) renders correctly — all empty initially
4. Clicking an empty cell does nothing (no item selected)
5. Character name shows in header
6. Stats summary shows base values with +0 gear bonus
7. Add a mock item name to `_gridContents[0]` in code temporarily — verify it shows in cell 0
8. Click that cell → item detail panel updates with item name
9. Click another cell → item moves
10. Click Unequip → cell clears
11. Click Close → returns to Settlement

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_F.md`  
**Covers:** Hunt selection modal, Travel phase screen, travel event flow
