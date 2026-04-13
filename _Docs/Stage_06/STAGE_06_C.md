<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-C | Settlement Screen — Layout & Characters Tab
Status: Stage 6-B complete. Main menu and campaign select
working end-to-end. Test verified.
Task: Create settlement-screen.uxml, settlement-screen.uss,
and SettlementScreenController.cs. Wire era header (year,
era name), Characters tab (character rows with stats,
injury count, hunt count, loadout button), and action bar
buttons (stubs for Hunt, Craft, End Year).
Crafters tab and Innovations tab are stubs this session.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_C.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Systems/SettlementManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/UI/USS/tokens.uss
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- The 3 files you will create (1 UXML, 1 USS, 1 C#)
- That Crafters and Innovations tabs are stubs (just log)
- That Hunt / Craft / End Year buttons are stubs this session
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-C: Settlement Screen — Layout & Characters Tab

**Resuming from:** Stage 6-B complete  
**Done when:** Settlement scene loads from GameStateManager; era header shows correct year and era name; Characters tab lists all active characters with injuries, proficiency, and hunt count; switching tabs logs correctly  
**Commit:** `"6C: Settlement screen layout, era header, Characters tab"`  
**Next session:** STAGE_06_D.md  

---

## Era Name Logic

| Years | Era Name |
|---|---|
| 1–3 | The Ember |
| 4–8 | The Refuge |
| 9–14 | The Outpost |
| 15–22 | The Settlement |
| 23–30 | The Stronghold |

---

## Step 1: settlement-screen.uxml

**Path:** `Assets/_Game/UI/UXML/settlement-screen.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>
    <Style src="../USS/settlement-screen.uss"/>

    <ui:VisualElement name="settlement-root" class="fullscreen-bg">

        <!-- Era Header -->
        <ui:VisualElement name="era-bar" class="era-bar stone-panel--raised">
            <ui:Label name="year-label" text="Year 1"     class="era-year"/>
            <ui:Label name="era-name"  text="The Ember"   class="era-name"/>
            <ui:VisualElement style="flex:1"/>
            <ui:Label name="resource-summary" text=""     class="resource-summary-label"/>
            <ui:Button name="btn-codex"    text="CODEX"   class="era-btn"/>
        </ui:VisualElement>

        <!-- Main Content Row -->
        <ui:VisualElement name="settlement-main" class="settlement-main">

            <!-- Left: Settlement Scene -->
            <ui:VisualElement name="settlement-scene" class="settlement-scene stone-panel">
                <ui:Label text="SETTLEMENT" class="settlement-scene-placeholder"/>
                <!-- Crafter sprites added dynamically -->
            </ui:VisualElement>

            <!-- Right: Management Panel -->
            <ui:VisualElement name="management-panel" class="management-panel stone-panel">

                <!-- Tab Bar -->
                <ui:VisualElement name="tab-bar" class="tab-bar">
                    <ui:Button name="tab-characters"  text="CHARACTERS"  class="tab-btn tab-btn--active"/>
                    <ui:Button name="tab-crafters"    text="CRAFTERS"    class="tab-btn"/>
                    <ui:Button name="tab-innovations" text="INNOVATIONS" class="tab-btn"/>
                </ui:VisualElement>

                <!-- Tab Content -->
                <ui:ScrollView name="tab-content" class="tab-content"/>

            </ui:VisualElement>
        </ui:VisualElement>

        <!-- Action Bar -->
        <ui:VisualElement name="action-bar" class="action-bar stone-panel--raised">
            <ui:Button name="btn-hunt"     text="SEND HUNTING PARTY" class="action-btn action-btn--primary"/>
            <ui:Button name="btn-craft"    text="CRAFT GEAR"         class="action-btn"/>
            <ui:Button name="btn-end-year" text="END YEAR"           class="action-btn action-btn--danger"/>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

---

## Step 2: settlement-screen.uss

**Path:** `Assets/_Game/UI/USS/settlement-screen.uss`

```css
/* Settlement Screen specific layout */
.settlement-main {
    flex:           1;
    flex-direction: row;
    min-height:     0;
}

.settlement-scene {
    width:           600px;
    flex-shrink:     0;
    margin:          2px;
    align-items:     center;
    justify-content: center;
    background-color: var(--color-bg-deep);
}

.settlement-scene-placeholder {
    color:            var(--color-text-dim);
    font-size:        var(--font-size-label);
    -unity-font-style: italic;
}

.management-panel {
    flex:           1;
    flex-direction: column;
    margin:         2px;
    overflow:       hidden;
}

.resource-summary-label {
    font-size:    var(--font-size-small);
    color:        var(--color-text-dim);
    margin-right: var(--spacing-md);
}

/* Character rows inside tab-content */
.character-row {
    margin-bottom: var(--spacing-xs);
}

.hunt-count-label {
    font-size:    var(--font-size-small);
    color:        var(--color-text-dim);
    margin-right: var(--spacing-sm);
}
```

---

## Step 3: SettlementScreenController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs`

```csharp
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class SettlementScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private CampaignSO  _campaignSO;

        private VisualElement  _root;
        private VisualElement  _tabContent;
        private SettlementManager _settlement;
        private string         _activeTab = "characters";

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            // Init SettlementManager with current campaign state
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null)
            {
                Debug.LogError("[Settlement] No CampaignState in GameStateManager");
                return;
            }

            _settlement = new SettlementManager();
            _settlement.Initialize(state, _campaignSO);

            _tabContent = _root.Q<ScrollView>("tab-content");

            WireButtons();
            RefreshEraHeader();
            RefreshResourceSummary();
            BuildCharactersTab();  // Default tab

            // Apply any pending hunt result
            if (state.pendingHuntResult.monsterName != null)
            {
                _settlement.ApplyHuntResults(state.pendingHuntResult);
                RefreshEraHeader();
                RefreshResourceSummary();
            }
        }

        // ── Era Header ────────────────────────────────────────────
        private void RefreshEraHeader()
        {
            var state = GameStateManager.Instance.CampaignState;
            _root.Q<Label>("year-label").text = $"Year {state.currentYear}";
            _root.Q<Label>("era-name").text   = GetEraName(state.currentYear);
        }

        private void RefreshResourceSummary()
        {
            var state    = GameStateManager.Instance.CampaignState;
            var topThree = state.resources
                .OrderByDescending(r => r.amount)
                .Take(3)
                .Select(r => $"{r.resourceName}: {r.amount}");
            var summary = string.Join("  |  ", topThree);
            var label   = _root.Q<Label>("resource-summary");
            if (label != null) label.text = summary;
        }

        private static string GetEraName(int year) => year switch
        {
            <= 3  => "The Ember",
            <= 8  => "The Refuge",
            <= 14 => "The Outpost",
            <= 22 => "The Settlement",
            _     => "The Stronghold",
        };

        // ── Tab Wiring ────────────────────────────────────────────
        private void WireButtons()
        {
            _root.Q<Button>("tab-characters").clicked  += () => SwitchTab("characters");
            _root.Q<Button>("tab-crafters").clicked    += () => SwitchTab("crafters");
            _root.Q<Button>("tab-innovations").clicked += () => SwitchTab("innovations");

            _root.Q<Button>("btn-codex").clicked    += OnCodexClicked;
            _root.Q<Button>("btn-hunt").clicked     += OnHuntClicked;
            _root.Q<Button>("btn-craft").clicked    += OnCraftClicked;
            _root.Q<Button>("btn-end-year").clicked += OnEndYearClicked;
        }

        private void SwitchTab(string tabName)
        {
            _activeTab = tabName;

            // Update tab button active states
            foreach (var btn in new[] { "tab-characters", "tab-crafters", "tab-innovations" })
            {
                var b = _root.Q<Button>(btn);
                b?.EnableInClassList("tab-btn--active",
                    btn == $"tab-{tabName}");
            }

            switch (tabName)
            {
                case "characters":  BuildCharactersTab();  break;
                case "crafters":    BuildCraftersTab();    break;
                case "innovations": BuildInnovationsTab(); break;
            }
        }

        // ── Characters Tab ────────────────────────────────────────
        private void BuildCharactersTab()
        {
            _tabContent.Clear();
            var state = GameStateManager.Instance.CampaignState;

            foreach (var ch in state.characters.Where(c => !c.isRetired))
            {
                var row = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList("stone-panel");

                // Name
                var nameLabel = new Label(ch.characterName);
                nameLabel.AddToClassList("character-name");
                row.Add(nameLabel);

                // Sex / build
                var buildLabel = new Label($"{ch.sex} · {ch.bodyBuild}");
                buildLabel.AddToClassList("proficiency-label");
                row.Add(buildLabel);

                // Injuries
                if (ch.injuryCardNames.Length > 0)
                {
                    var injLabel = new Label($"⚑ {ch.injuryCardNames.Length} injur{(ch.injuryCardNames.Length == 1 ? "y" : "ies")}");
                    injLabel.AddToClassList("injury-indicator");
                    row.Add(injLabel);
                }

                // Proficiency
                for (int i = 0; i < ch.proficiencyWeaponTypes.Length; i++)
                {
                    var pLabel = new Label(
                        $"{ch.proficiencyWeaponTypes[i]}: T{ch.proficiencyTiers[i]}");
                    pLabel.AddToClassList("proficiency-label");
                    row.Add(pLabel);
                }

                // Hunt count
                var huntLabel = new Label($"Hunts: {ch.huntCount}");
                huntLabel.AddToClassList("hunt-count-label");
                row.Add(huntLabel);

                // Loadout button (opens Gear Grid — Session 6-E)
                string capturedId = ch.characterId;
                var loadoutBtn = new Button(() => OpenGearGrid(capturedId))
                    { text = "LOADOUT" };
                loadoutBtn.AddToClassList("small-btn");
                row.Add(loadoutBtn);

                _tabContent.Add(row);
            }

            // Show retired count
            int retiredCount = state.retiredCharacters?.Length ?? 0;
            if (retiredCount > 0)
            {
                var retiredLabel = new Label($"Retired: {retiredCount}");
                retiredLabel.AddToClassList("proficiency-label");
                _tabContent.Add(retiredLabel);
            }
        }

        // ── Crafters Tab — Stub ───────────────────────────────────
        private void BuildCraftersTab()
        {
            _tabContent.Clear();
            var stub = new Label("Crafters tab — implemented Session 6-D");
            stub.AddToClassList("proficiency-label");
            _tabContent.Add(stub);
            Debug.Log("[Settlement] Crafters tab stub — implement 6-D");
        }

        // ── Innovations Tab — Stub ────────────────────────────────
        private void BuildInnovationsTab()
        {
            _tabContent.Clear();
            var stub = new Label("Innovations tab — implemented Session 6-D");
            stub.AddToClassList("proficiency-label");
            _tabContent.Add(stub);
            Debug.Log("[Settlement] Innovations tab stub — implement 6-D");
        }

        // ── Action Bar Handlers — Stubs ───────────────────────────
        private void OpenGearGrid(string characterId)
        {
            Debug.Log($"[Settlement] Open Gear Grid for: {characterId} — implement 6-E");
        }

        private void OnCodexClicked()
        {
            Debug.Log("[Settlement] Codex — implement 6-G");
        }

        private void OnHuntClicked()
        {
            Debug.Log("[Settlement] Hunt selection — implement 6-F");
        }

        private void OnCraftClicked()
        {
            Debug.Log("[Settlement] Craft — implement 6-D");
        }

        private void OnEndYearClicked()
        {
            var state = GameStateManager.Instance.CampaignState;
            _settlement.CheckAllRetirements();
            _settlement.AdvanceYear();
            RefreshEraHeader();
            RefreshResourceSummary();
            BuildCharactersTab();
            Debug.Log($"[Settlement] Year advanced to {state.currentYear}");
        }
    }
}
```

---

## Verification Test

1. Start from MainMenu → New Campaign → select campaign → Begin
2. Settlement scene loads
3. Era header shows "Year 1" and "The Ember"
4. Characters tab is active by default — shows all starting characters
5. Each character row shows name, build, hunt count (0), proficiency (FistWeapon T1)
6. Clicking Crafters tab → logs stub message
7. Clicking Innovations tab → logs stub message
8. Clicking End Year → year increments to 2, era header updates

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_D.md`  
**Covers:** Chronicle Event modal, Guiding Principal modal, Crafters tab, Innovations tab
