<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 5-A | USS Design Tokens & Stone-Panel Style System
Status: Stage 4 complete. Full campaign loop verified in
console. All Stage 4 test scripts deleted.
Task: Create the UI folder structure, tokens.uss with all
design tokens, and stone-panel.uss with the base stone
aesthetic classes. No UXML yet. No C# yet. CSS only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_05/STAGE_05_A.md

Then confirm:
- The folder structure you will create
- The 2 USS files you will create
- That you will NOT create any UXML or C# this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 5-A: USS Design Tokens & Stone-Panel Style System

**Resuming from:** Stage 4 complete  
**Done when:** `tokens.uss` and `stone-panel.uss` exist in the correct folder, import correctly in the Unity Editor with no USS errors in the Console  
**Commit:** `"5A: USS design tokens and stone-panel style system"`  
**Next session:** STAGE_05_B.md  

---

## UI Architecture Rules (Non-Negotiable)

Per `.cursorrules`:
- **UI Toolkit only.** No Unity uGUI — no `Canvas`, `Image`, `Text` components
- **All UI = UXML + USS.** No inline styles in C# controllers unless dynamically calculated (e.g. bar widths)
- **No logic in UXML/USS** — all event wiring and state updates in C# controllers in `MnM.Core.UI`
- **Stone = no rounded corners.** `border-radius: 0px` everywhere. Stone tablets don't curve.

---

## Folder Structure to Create

```
Assets/
└── _Game/
    └── UI/
        ├── UXML/          ← All .uxml screen files (Sessions 5-B onward)
        └── USS/
            ├── tokens.uss          ← Create this session
            └── stone-panel.uss     ← Create this session
```

---

## Step 1: tokens.uss

**Path:** `Assets/_Game/UI/USS/tokens.uss`

This file defines every colour, size, and spacing value used across the entire UI. All other USS files reference these variables — never hardcode values elsewhere.

```css
/* ============================================================
   Marrow & Myth — Design Tokens
   All UI colours, sizes, and spacing defined here.
   Import this file at the top of every screen USS file.
   ============================================================ */

:root {
    /* ── Palette ─────────────────────────────────────────────── */
    --color-bg-deep:          #0A0A0C;   /* Near-black background */
    --color-bg-panel:         #1A1815;   /* Stone panel background */
    --color-bg-panel-raised:  #232018;   /* Slightly lighter raised panel */
    --color-bg-panel-active:  #2A251E;   /* Selected / active panel */

    --color-border:           #3D3830;   /* Default stone border */
    --color-border-accent:    #5C5040;   /* Highlighted border */
    --color-border-danger:    #6B2020;   /* Error / collapse border */

    --color-text-primary:     #D4CCBA;   /* Main readable text */
    --color-text-dim:         #8A8070;   /* Secondary / label text */
    --color-text-accent:      #B8860B;   /* Marrow gold — headings, highlights */
    --color-text-danger:      #CC3333;   /* Collapse, loss, danger */
    --color-text-disabled:    #4A4540;   /* Greyed-out unplayable */

    --color-shell:            #4A5568;   /* Shell bar fill — grey-blue */
    --color-shell-bg:         #1E2530;   /* Shell bar background */
    --color-flesh:            #742A2A;   /* Flesh bar fill — dark red */
    --color-flesh-bg:         #2A1010;   /* Flesh bar background */
    --color-grit:             #8B7355;   /* Grit pip fill — warm bone */
    --color-grit-bg:          #2A2018;   /* Grit pip background */

    --color-aggro:            #B8860B;   /* Aggro token — Marrow gold */
    --color-loud:             #CC4B2A;   /* Loud tag — orange-red */
    --color-exposed:          #B85050;   /* Exposed part indicator */
    --color-broken:           #4A3020;   /* Broken part — dark brown */
    --color-denied:           #1A0A30;   /* Grid denied cell — dark purple */
    --color-marrow-sink:      #1A2A10;   /* Marrow sink tile — dark green */

    --color-highlight-valid:  #1A3020;   /* Valid target cell highlight */
    --color-highlight-select: #2A4030;   /* Selected cell highlight */

    /* ── Typography ─────────────────────────────────────────── */
    --font-size-xl:    20px;   /* Screen titles */
    --font-size-title: 16px;   /* Panel headers */
    --font-size-body:  13px;   /* Normal text */
    --font-size-label: 11px;   /* Small labels, zone names */
    --font-size-small: 10px;   /* Badges, tags */

    /* ── Spacing ─────────────────────────────────────────────── */
    --spacing-xs:  4px;
    --spacing-sm:  8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 40px;

    /* ── Panel Rules ─────────────────────────────────────────── */
    --panel-border-width:  2px;
    --panel-border-radius: 0px;   /* Stone = no rounded corners — ever */
    --panel-padding:       8px;

    /* ── Stat Bars ───────────────────────────────────────────── */
    --bar-height-shell: 8px;    /* Shell bars are thinner */
    --bar-height-flesh: 12px;   /* Flesh bars are taller — easier to read */
    --bar-width-full:   100%;

    /* ── Grid ────────────────────────────────────────────────── */
    --grid-cell-size:   50px;   /* Each combat grid cell */
    --grid-cell-border: 1px;
}
```

---

## Step 2: stone-panel.uss

**Path:** `Assets/_Game/UI/USS/stone-panel.uss`

The core visual language. Every UI panel in the game uses one of these classes.

```css
/* ============================================================
   Marrow & Myth — Stone Panel System
   Import tokens.uss before this file.
   ============================================================ */

/* ── Base Stone Panel ────────────────────────────────────────
   The default carved stone tablet. Used for all major panels.
   ──────────────────────────────────────────────────────────── */
.stone-panel {
    background-color: var(--color-bg-panel);
    border-color:     var(--color-border);
    border-width:     var(--panel-border-width);
    border-radius:    var(--panel-border-radius);
    padding:          var(--panel-padding);
}

/* ── Raised Stone Panel ──────────────────────────────────────
   Slightly lighter — used for nested panels and active areas.
   ──────────────────────────────────────────────────────────── */
.stone-panel--raised {
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-accent);
    border-width:     var(--panel-border-width);
    border-radius:    var(--panel-border-radius);
    padding:          var(--panel-padding);
}

/* ── Active Stone Panel ──────────────────────────────────────
   Highlighted state — selected hunter, active tab, etc.
   ──────────────────────────────────────────────────────────── */
.stone-panel--active {
    background-color: var(--color-bg-panel-active);
    border-color:     var(--color-text-accent);
    border-width:     var(--panel-border-width);
    border-radius:    var(--panel-border-radius);
    padding:          var(--panel-padding);
}

/* ── Danger Stone Panel ──────────────────────────────────────
   Collapsed hunter, critical state.
   ──────────────────────────────────────────────────────────── */
.stone-panel--danger {
    background-color: var(--color-bg-panel);
    border-color:     var(--color-border-danger);
    border-width:     var(--panel-border-width);
    border-radius:    var(--panel-border-radius);
    padding:          var(--panel-padding);
    opacity:          0.6;
}

/* ── Panel Header ────────────────────────────────────────────
   Gold heading with bottom border — every panel's title row.
   ──────────────────────────────────────────────────────────── */
.stone-panel__header {
    font-size:           var(--font-size-title);
    color:               var(--color-text-accent);
    -unity-font-style:   bold;
    border-bottom-color: var(--color-border-accent);
    border-bottom-width: 1px;
    padding-bottom:      var(--spacing-xs);
    margin-bottom:       var(--spacing-sm);
}

/* ── Stat Bar Container ──────────────────────────────────────
   Outer track for Shell and Flesh bars.
   ──────────────────────────────────────────────────────────── */
.stat-bar-track {
    flex-direction: row;
    margin-bottom:  var(--spacing-xs);
    align-items:    center;
}

/* ── Shell Bar ───────────────────────────────────────────────
   Grey-blue, thinner. Renders behind flesh visually.
   ──────────────────────────────────────────────────────────── */
.shell-bar-track {
    background-color: var(--color-shell-bg);
    height:           var(--bar-height-shell);
    flex: 1;
    border-radius:    0px;
}

.shell-bar-fill {
    background-color: var(--color-shell);
    height:           100%;
    border-radius:    0px;
    /* Width set dynamically in C# as percentage */
}

/* ── Flesh Bar ───────────────────────────────────────────────
   Dark red, taller. Colorblind note: also taller than shell.
   ──────────────────────────────────────────────────────────── */
.flesh-bar-track {
    background-color: var(--color-flesh-bg);
    height:           var(--bar-height-flesh);
    flex: 1;
    border-radius:    0px;
}

.flesh-bar-fill {
    background-color: var(--color-flesh);
    height:           100%;
    border-radius:    0px;
}

/* ── Grit Pip ────────────────────────────────────────────────
   Individual pip for Grit display (bone-warm dots).
   ──────────────────────────────────────────────────────────── */
.grit-pip {
    width:            12px;
    height:           12px;
    background-color: var(--color-grit);
    border-radius:    0px;
    margin-right:     var(--spacing-xs);
}

.grit-pip--empty {
    background-color: var(--color-grit-bg);
    border-color:     var(--color-border);
    border-width:     1px;
}

/* ── Status Effect Badge ─────────────────────────────────────
   Small label tag for Shaken, Pinned, etc.
   ──────────────────────────────────────────────────────────── */
.status-badge {
    font-size:        var(--font-size-small);
    color:            var(--color-text-danger);
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-danger);
    border-width:     1px;
    border-radius:    0px;
    padding:          2px 4px;
    margin-right:     var(--spacing-xs);
    -unity-font-style: bold;
}

/* ── Loud Tag ────────────────────────────────────────────────
   Orange-red badge on action cards that are Loud.
   ──────────────────────────────────────────────────────────── */
.loud-tag {
    font-size:        var(--font-size-small);
    color:            var(--color-loud);
    border-color:     var(--color-loud);
    border-width:     1px;
    border-radius:    0px;
    padding:          2px 4px;
    -unity-font-style: bold;
}

/* ── Exposed Tag ─────────────────────────────────────────────
   Red badge on monster parts that are Exposed.
   ──────────────────────────────────────────────────────────── */
.exposed-tag {
    font-size:        var(--font-size-small);
    color:            var(--color-exposed);
    border-color:     var(--color-exposed);
    border-width:     1px;
    border-radius:    0px;
    padding:          2px 4px;
    -unity-font-style: bold;
}

/* ── Action Button ───────────────────────────────────────────
   Primary action button — carved stone with gold border.
   ──────────────────────────────────────────────────────────── */
.action-btn {
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-accent);
    border-width:     var(--panel-border-width);
    border-radius:    0px;
    color:            var(--color-text-primary);
    font-size:        var(--font-size-body);
    padding:          var(--spacing-sm) var(--spacing-md);
    -unity-font-style: bold;
}

.action-btn:hover {
    background-color: var(--color-bg-panel-active);
    border-color:     var(--color-text-accent);
}

.action-btn:active {
    background-color: var(--color-bg-deep);
}

.action-btn--primary {
    border-color: var(--color-text-accent);
    color:        var(--color-text-accent);
}

.action-btn--danger {
    border-color: var(--color-border-danger);
    color:        var(--color-text-danger);
}

/* ── Zone Label ──────────────────────────────────────────────
   Small all-caps label for body zone names (HEAD, TORSO, etc.)
   ──────────────────────────────────────────────────────────── */
.zone-label {
    font-size:        var(--font-size-label);
    color:            var(--color-text-dim);
    -unity-font-style: bold;
    width:            48px;
    -unity-text-align: middle-left;
}

/* ── Aggro Indicator ─────────────────────────────────────────
   Gold icon shown on the hunter holding the Aggro token.
   ──────────────────────────────────────────────────────────── */
.aggro-indicator {
    color:            var(--color-aggro);
    font-size:        var(--font-size-title);
    -unity-font-style: bold;
}

/* ── Fullscreen Background ───────────────────────────────────
   Root container for every screen.
   ──────────────────────────────────────────────────────────── */
.fullscreen-bg {
    background-color: var(--color-bg-deep);
    width:            100%;
    height:           100%;
    flex-direction:   column;
}
```

---

## Verification Test

In the Unity Editor:

1. Open `Assets/_Game/UI/USS/tokens.uss` in the Project window — confirm no error icons
2. Open `Assets/_Game/UI/USS/stone-panel.uss` — confirm no error icons
3. Open Unity Console — confirm zero USS parse errors
4. In the Unity Inspector, create a temporary `PanelSettings` asset and assign `tokens.uss` as a stylesheet — confirm it loads without warnings
5. Verify folder structure: `Assets/_Game/UI/USS/` contains exactly 2 files, `Assets/_Game/UI/UXML/` exists and is empty

No gameplay. No UXML. No C#. Just clean CSS that the Editor accepts.

---

## Next Session

**File:** `_Docs/Stage_05/STAGE_05_B.md`  
**Covers:** Combat screen UXML layout — phase bar, hunter panels, grid container, monster panel, card hand
