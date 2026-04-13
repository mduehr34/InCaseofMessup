<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 5-B | Combat Screen UXML Layout & USS
Status: Stage 5-A complete. tokens.uss and stone-panel.uss
exist with zero USS errors in Console.
Task: Create combat-screen.uxml (the root layout), and
combat-screen.uss (combat-specific layout rules).
No C# controller yet — layout and visual structure only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_05/STAGE_05_B.md
- Assets/_Game/UI/USS/tokens.uss
- Assets/_Game/UI/USS/stone-panel.uss

Then confirm:
- The 2 files you will create
- That layout is 1920×1080 with the 4-zone structure
- That no C# controller is written this session
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 5-B: Combat Screen UXML Layout & USS

**Resuming from:** Stage 5-A complete — USS tokens verified  
**Done when:** `combat-screen.uxml` opens in UI Builder with the correct 4-zone layout visible; `combat-screen.uss` applies stone panel classes correctly; zero Console errors  
**Commit:** `"5B: Combat screen UXML layout and combat-screen.uss"`  
**Next session:** STAGE_05_C.md  

---

## Layout Zones (1920×1080)

```
┌───────────────────────────────────────────────────────────────┐
│  PHASE BAR (full width, height: 60px)                         │
├──────────────┬──────────────────────────────┬─────────────────┤
│ HUNTER       │                              │ MONSTER         │
│ PANELS       │     GRID CONTAINER           │ PANEL           │
│ width: 300px │     flex: 1                  │ width: 380px    │
│ height: 760px│     height: 760px            │ height: 760px   │
├──────────────┴──────────────────────────────┴─────────────────┤
│  CARD HAND (full width, height: 260px)                        │
└───────────────────────────────────────────────────────────────┘
Total: 1920×1080. Phase bar (60) + content row (760) + card hand (260) = 1080.
```

---

## Step 1: combat-screen.uxml

**Path:** `Assets/_Game/UI/UXML/combat-screen.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:noNamespaceSchemaLocation="../../UIElementsSchema/UIElements.xsd"
         editor-extension-mode="False">

    <Style src="project://database/Assets/_Game/UI/USS/tokens.uss?fileID=7433441132597879392&amp;guid=TOKENS_GUID&amp;type=3#tokens"/>
    <Style src="project://database/Assets/_Game/UI/USS/stone-panel.uss?fileID=7433441132597879392&amp;guid=STONE_GUID&amp;type=3#stone-panel"/>
    <Style src="project://database/Assets/_Game/UI/USS/combat-screen.uss?fileID=7433441132597879392&amp;guid=COMBAT_GUID&amp;type=3#combat-screen"/>

    <!-- Root -->
    <ui:VisualElement name="combat-root" class="fullscreen-bg combat-root">

        <!-- ── Zone 1: Phase Bar ─────────────────────────────── -->
        <ui:VisualElement name="phase-bar" class="phase-bar stone-panel--raised">
            <ui:Label name="phase-label" text="VITALITY PHASE" class="phase-label"/>
            <ui:VisualElement class="phase-bar__spacer"/>
            <ui:Label name="round-label" text="Round 1" class="round-label"/>
        </ui:VisualElement>

        <!-- ── Zone 2: Content Row ───────────────────────────── -->
        <ui:VisualElement name="content-row" class="content-row">

            <!-- Left: 4 Hunter Panels stacked -->
            <ui:VisualElement name="hunter-panel-container" class="hunter-panel-container">
                <ui:VisualElement name="hunter-panel-0" class="hunter-panel stone-panel">
                    <ui:VisualElement class="hunter-header">
                        <ui:Label name="hunter-name-0" text="---" class="stone-panel__header hunter-name"/>
                        <ui:Label name="aggro-0" text="⚑" class="aggro-indicator"/>
                    </ui:VisualElement>
                    <ui:VisualElement name="body-zones-0" class="body-zones"/>
                    <ui:VisualElement name="status-effects-0" class="status-effects-row"/>
                    <ui:VisualElement name="active-info-0" class="active-hunter-info">
                        <ui:Label name="ap-label-0" text="AP: 2" class="ap-label"/>
                        <ui:VisualElement name="grit-pips-0" class="grit-pips-row"/>
                    </ui:VisualElement>
                </ui:VisualElement>

                <ui:VisualElement name="hunter-panel-1" class="hunter-panel stone-panel">
                    <ui:VisualElement class="hunter-header">
                        <ui:Label name="hunter-name-1" text="---" class="stone-panel__header hunter-name"/>
                        <ui:Label name="aggro-1" text="⚑" class="aggro-indicator"/>
                    </ui:VisualElement>
                    <ui:VisualElement name="body-zones-1" class="body-zones"/>
                    <ui:VisualElement name="status-effects-1" class="status-effects-row"/>
                    <ui:VisualElement name="active-info-1" class="active-hunter-info">
                        <ui:Label name="ap-label-1" text="AP: 2" class="ap-label"/>
                        <ui:VisualElement name="grit-pips-1" class="grit-pips-row"/>
                    </ui:VisualElement>
                </ui:VisualElement>

                <ui:VisualElement name="hunter-panel-2" class="hunter-panel stone-panel">
                    <ui:VisualElement class="hunter-header">
                        <ui:Label name="hunter-name-2" text="---" class="stone-panel__header hunter-name"/>
                        <ui:Label name="aggro-2" text="⚑" class="aggro-indicator"/>
                    </ui:VisualElement>
                    <ui:VisualElement name="body-zones-2" class="body-zones"/>
                    <ui:VisualElement name="status-effects-2" class="status-effects-row"/>
                    <ui:VisualElement name="active-info-2" class="active-hunter-info">
                        <ui:Label name="ap-label-2" text="AP: 2" class="ap-label"/>
                        <ui:VisualElement name="grit-pips-2" class="grit-pips-row"/>
                    </ui:VisualElement>
                </ui:VisualElement>

                <ui:VisualElement name="hunter-panel-3" class="hunter-panel stone-panel">
                    <ui:VisualElement class="hunter-header">
                        <ui:Label name="hunter-name-3" text="---" class="stone-panel__header hunter-name"/>
                        <ui:Label name="aggro-3" text="⚑" class="aggro-indicator"/>
                    </ui:VisualElement>
                    <ui:VisualElement name="body-zones-3" class="body-zones"/>
                    <ui:VisualElement name="status-effects-3" class="status-effects-row"/>
                    <ui:VisualElement name="active-info-3" class="active-hunter-info">
                        <ui:Label name="ap-label-3" text="AP: 2" class="ap-label"/>
                        <ui:VisualElement name="grit-pips-3" class="grit-pips-row"/>
                    </ui:VisualElement>
                </ui:VisualElement>
            </ui:VisualElement>

            <!-- Center: Combat Grid (cells generated in C#) -->
            <ui:VisualElement name="grid-container" class="grid-container stone-panel"/>

            <!-- Right: Monster Panel -->
            <ui:VisualElement name="monster-panel" class="monster-panel stone-panel">
                <ui:Label name="monster-name"       text="---"               class="stone-panel__header"/>
                <ui:Label name="monster-difficulty" text="Standard"          class="monster-difficulty-label"/>
                <ui:Label name="monster-deck-count" text="Removable: --"     class="monster-deck-label"/>
                <ui:Label name="monster-stance"     text=""                  class="monster-stance-label"/>
                <ui:VisualElement name="monster-parts-container"             class="monster-parts-container"/>
            </ui:VisualElement>

        </ui:VisualElement>

        <!-- ── Zone 3: Card Hand ─────────────────────────────── -->
        <ui:VisualElement name="card-hand-zone" class="card-hand-zone stone-panel--raised">
            <ui:VisualElement name="hand-cards" class="hand-cards"/>
            <ui:VisualElement name="hand-actions" class="hand-actions">
                <ui:Label  name="ap-display"   text="AP: 2"    class="hand-ap-display"/>
                <ui:Label  name="grit-display" text="Grit: 3"  class="hand-grit-display"/>
                <ui:Button name="end-turn-btn" text="END TURN" class="action-btn action-btn--primary end-turn-btn"/>
            </ui:VisualElement>
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

> ⚑ The `guid` values in Style `src` attributes are placeholders. Unity generates real GUIDs when USS files are imported. After creating the files in the Editor, open `combat-screen.uxml` in a text editor and update the Style references to match the actual GUIDs Unity assigned, or use the UI Builder's stylesheet picker to link them correctly.

---

## Step 2: combat-screen.uss

**Path:** `Assets/_Game/UI/USS/combat-screen.uss`

```css
/* ============================================================
   Marrow & Myth — Combat Screen Layout
   Import tokens.uss and stone-panel.uss before this file.
   ============================================================ */

/* ── Root ────────────────────────────────────────────────────── */
.combat-root {
    flex-direction: column;
    width:          100%;
    height:         100%;
}

/* ── Zone 1: Phase Bar ───────────────────────────────────────── */
.phase-bar {
    flex-direction:  row;
    align-items:     center;
    height:          60px;
    padding-left:    var(--spacing-md);
    padding-right:   var(--spacing-md);
    flex-shrink:     0;
}

.phase-label {
    font-size:        var(--font-size-title);
    color:            var(--color-text-accent);
    -unity-font-style: bold;
}

.phase-bar__spacer {
    flex: 1;
}

.round-label {
    font-size: var(--font-size-body);
    color:     var(--color-text-dim);
}

/* ── Zone 2: Content Row ─────────────────────────────────────── */
.content-row {
    flex-direction: row;
    flex:           1;
    min-height:     0;   /* Prevents flex overflow */
}

/* ── Hunter Panel Container ──────────────────────────────────── */
.hunter-panel-container {
    flex-direction: column;
    width:          300px;
    flex-shrink:    0;
}

.hunter-panel {
    flex:          1;
    margin:        2px;
    flex-direction: column;
}

.hunter-panel--collapsed {
    opacity:       0.5;
    border-color:  var(--color-border-danger);
}

.hunter-header {
    flex-direction: row;
    align-items:    center;
    margin-bottom:  var(--spacing-xs);
}

.hunter-name {
    flex:           1;
    font-size:      var(--font-size-body);
    margin-bottom:  0;
    border-bottom-width: 0;   /* Override stone-panel__header bottom border for compact layout */
}

/* ── Body Zones ──────────────────────────────────────────────── */
.body-zones {
    flex-direction: column;
    flex:           1;
}

.body-zone-row {
    flex-direction: row;
    align-items:    center;
    margin-bottom:  3px;
}

.body-zone-bars {
    flex:           1;
    flex-direction: column;
}

/* ── Status Effects Row ──────────────────────────────────────── */
.status-effects-row {
    flex-direction: row;
    flex-wrap:      wrap;
    margin-top:     var(--spacing-xs);
    min-height:     18px;
}

/* ── Active Hunter Info ──────────────────────────────────────── */
.active-hunter-info {
    flex-direction: row;
    align-items:    center;
    margin-top:     var(--spacing-xs);
    border-top-color:  var(--color-border);
    border-top-width:  1px;
    padding-top:    var(--spacing-xs);
}

.ap-label {
    font-size:  var(--font-size-label);
    color:      var(--color-text-primary);
    margin-right: var(--spacing-sm);
}

.grit-pips-row {
    flex-direction: row;
    align-items:    center;
}

/* ── Grid Container ──────────────────────────────────────────── */
.grid-container {
    flex:           1;
    flex-direction: column;
    margin:         2px;
    overflow:       hidden;
}

.grid-row {
    flex-direction: row;
    flex:           1;
}

.grid-cell {
    width:            var(--grid-cell-size);
    height:           var(--grid-cell-size);
    border-color:     var(--color-border);
    border-width:     var(--grid-cell-border);
    background-color: var(--color-bg-deep);
    flex-shrink:      0;
}

.grid-cell:hover {
    background-color: var(--color-highlight-valid);
}

.grid-cell--selected {
    background-color: var(--color-highlight-select);
    border-color:     var(--color-text-accent);
}

.grid-cell--denied {
    background-color: var(--color-denied);
}

.grid-cell--marrow {
    background-color: var(--color-marrow-sink);
}

.grid-cell--hunter {
    border-color:     var(--color-text-primary);
    border-width:     2px;
}

.grid-cell--monster {
    border-color:     var(--color-text-danger);
    border-width:     2px;
}

/* ── Monster Panel ───────────────────────────────────────────── */
.monster-panel {
    width:          380px;
    flex-shrink:    0;
    margin:         2px;
    flex-direction: column;
    overflow:       hidden;
}

.monster-difficulty-label {
    font-size:  var(--font-size-label);
    color:      var(--color-text-dim);
    margin-bottom: var(--spacing-xs);
}

.monster-deck-label {
    font-size:  var(--font-size-label);
    color:      var(--color-text-dim);
    margin-bottom: var(--spacing-sm);
}

.monster-stance-label {
    font-size:        var(--font-size-label);
    color:            var(--color-text-accent);
    -unity-font-style: italic;
    margin-bottom:    var(--spacing-sm);
}

.monster-parts-container {
    flex:           1;
    flex-direction: column;
    overflow:       hidden;
}

.monster-part-row {
    flex-direction: row;
    align-items:    center;
    margin-bottom:  4px;
    padding:        3px var(--spacing-xs);
    background-color: var(--color-bg-deep);
    border-color:   var(--color-border);
    border-width:   1px;
}

.monster-part-row--broken {
    background-color: var(--color-broken);
    opacity:          0.7;
}

.part-name {
    font-size:  var(--font-size-label);
    color:      var(--color-text-primary);
    width:      72px;
    flex-shrink: 0;
}

.part-bars {
    flex:           1;
    flex-direction: column;
}

/* ── Zone 3: Card Hand ───────────────────────────────────────── */
.card-hand-zone {
    flex-direction: row;
    height:         260px;
    flex-shrink:    0;
    align-items:    center;
    padding:        var(--spacing-sm);
}

.hand-cards {
    flex:           1;
    flex-direction: row;
    align-items:    center;
    overflow:       hidden;
}

.hand-actions {
    flex-direction: column;
    align-items:    flex-end;
    margin-left:    var(--spacing-md);
    flex-shrink:    0;
}

.hand-ap-display {
    font-size:        var(--font-size-title);
    color:            var(--color-text-primary);
    -unity-font-style: bold;
    margin-bottom:    var(--spacing-sm);
}

.hand-grit-display {
    font-size:     var(--font-size-body);
    color:         var(--color-grit);
    margin-bottom: var(--spacing-md);
}

.end-turn-btn {
    width:  140px;
    height: 48px;
    font-size: var(--font-size-body);
}

/* ── Action Cards ────────────────────────────────────────────── */
.card {
    width:          160px;
    min-height:     200px;
    margin:         0 var(--spacing-xs);
    flex-direction: column;
    flex-shrink:    0;
}

.card--unplayable {
    opacity: 0.4;
}

.card--selected {
    border-color:     var(--color-text-accent);
    background-color: var(--color-bg-panel-active);
}

.card:hover {
    border-color: var(--color-border-accent);
}

.card-header {
    flex-direction: row;
    align-items:    flex-start;
    margin-bottom:  var(--spacing-xs);
}

.card-name {
    font-size:        var(--font-size-body);
    color:            var(--color-text-primary);
    -unity-font-style: bold;
    flex:             1;
    white-space:      normal;
}

.card-category {
    font-size:  var(--font-size-small);
    color:      var(--color-text-dim);
    margin-bottom: var(--spacing-xs);
    -unity-font-style: italic;
}

.card-effect {
    font-size:    var(--font-size-small);
    color:        var(--color-text-primary);
    white-space:  normal;
    flex:         1;
}

.card-footer {
    flex-direction: row;
    align-items:    center;
    margin-top:     var(--spacing-xs);
    border-top-color:  var(--color-border);
    border-top-width:  1px;
    padding-top:    var(--spacing-xs);
}

.card-ap-cost {
    font-size:        var(--font-size-label);
    color:            var(--color-text-accent);
    -unity-font-style: bold;
    flex:             1;
}

.card-refund {
    font-size: var(--font-size-small);
    color:     var(--color-text-dim);
}
```

---

## Verification Test

In the Unity Editor:

1. Open `Assets/_Game/UI/UXML/combat-screen.uxml` in the **UI Builder**
2. Confirm the 4 zones are visible in the viewport: Phase Bar (top), Content Row (middle), Card Hand (bottom)
3. Confirm the Content Row has 3 columns: Hunter Container (left), Grid Container (centre), Monster Panel (right)
4. Confirm zero USS errors in the Console
5. Confirm the stone-panel background colour (`#1A1815`) is visible on all panels — if panels are transparent, the USS link is broken
6. Resize the UI Builder viewport — confirm panels flex correctly without overflow

No C# written. No controller. Just layout verified in UI Builder.

---

## Next Session

**File:** `_Docs/Stage_05/STAGE_05_C.md`  
**Covers:** `CombatScreenController.cs` — wiring events, refreshing hunter panels, updating stat bars, and phase label
