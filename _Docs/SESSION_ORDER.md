# Marrow & Myth — Master Session Order

> **Last updated:** After Stage 8-L complete + combat system rework (6 new sessions inserted as 8-M through 8-R; original 8-M through 8-R shifted to 8-S through 8-X; 9-T/U added; 10-M/N inserted before 10-L)
>
> **Key change from original plan:** After completing the 8-L monster execution engine, playtesting revealed the shell/flesh HP system created frustrating combat pacing. Six sessions were inserted immediately after 8-L:
> - **8-M** (new): Data model rework — shell/flesh HP removed; behavior deck IS the monster's health; wound location deck introduced
> - **8-N** (new): Runtime implementation of the new health model — `BehaviorDeck`/`WoundDeck` wrappers, rebuilt `MonsterAI`, wound resolution, Gaunt SO assets
> - **8-O** (new): Hunter Deployment Phase — player places hunters in a spawn zone before combat
> - **8-P** (new): Active Hunter Selection — player clicks any non-acted hunter token to switch who acts
> - **8-Q** (new): Combat Terrain — obstacle and bonus terrain cells
> - **8-R** (new): Combat Loop Integration Gate — full Aldric vs Gaunt pass verifying the complete new system
> - The original 8-M through 8-R sessions were renumbered to 8-S through 8-X
> - **9-T** (new): Bleed & Poison status counter system
> - **9-U** (new): Limb Wound → Disorder trigger (persistent negative effect carried into settlement)
> - **10-M/10-N**: Combat and Settlement animations (originally 8-K and 8-L before the first reorganization) — moved here from their original positions; executed before 10-L (final ship)

---

## Stage 8 — Core Systems & UI

| Session | File | Topic | Status |
|---------|------|--------|--------|
| 8-A | STAGE_08_A.md | Bootstrap Scene & Game Manager | ✅ Done |
| 8-B | STAGE_08_B.md | Scene Architecture & Build Settings | ✅ Done |
| 8-C | STAGE_08_C.md | Character Creation Screen | ✅ Done |
| 8-D | STAGE_08_D.md | Settlement Screen Core | ✅ Done |
| 8-E | STAGE_08_E.md | Settlement Event System | ✅ Done |
| 8-F | STAGE_08_F.md | Settlement Gear & Crafting | ✅ Done |
| 8-G | STAGE_08_G.md | Combat UI Polish — Part HP Bars, AP/Grit | ✅ Done |
| 8-H | STAGE_08_H.md | Card Visual Rendering System | ✅ Done |
| 8-I | STAGE_08_I.md | SceneTransitionManager | ✅ Done |
| 8-J | STAGE_08_J.md | Card Play & Draw Animations | ✅ Done |
| 8-K | STAGE_08_K.md | Hunter Movement UI — Grid Click, Range Highlight, Facing | ✅ Done |
| 8-L | STAGE_08_L.md | Monster Action Execution Engine (first pass — superseded by 8-M) | ✅ Done |
| 8-M | STAGE_08_M.md | Monster Health Rework — Behavior Deck as Life & Wound Locations | ✅ Done |
| 8-N | STAGE_08_N.md | New Combat Runtime — BehaviorDeck, Wound Resolution, MonsterAI Rebuild | ✅ Done |
| **8-O** | **STAGE_08_O.md** | **Hunter Deployment Phase — Per-Monster Spawn Zones** | ⬜ Next |
| **8-P** | **STAGE_08_P.md** | **Active Hunter Selection — Player Chooses Who Acts** | ⬜ |
| **8-Q** | **STAGE_08_Q.md** | **Combat Terrain — Obstacle and Bonus Squares** | ⬜ |
| **8-R** | **STAGE_08_R.md** | **Combat Loop Integration Gate — full Aldric vs Gaunt pass (new system)** | ⬜ |
| 8-S | STAGE_08_S.md | Hunt Travel Scene — Travel Events, CONTINUE TO HUNT | ⬜ |
| 8-T | STAGE_08_T.md | Tutorial Tooltip & Onboarding System | ⬜ |
| 8-U | STAGE_08_U.md | Chronicle Log & Codex UI | ⬜ |
| 8-V | STAGE_08_V.md | Birth, Retirement & Year-End Screens | ⬜ |
| 8-W | STAGE_08_W.md | Save/Load UI, Game Over & Victory Epilogue | ⬜ |
| 8-X | STAGE_08_X.md | Stage 8 Final Integration & DoD (tag v0.8) | ⬜ |

### Stage 8 Reorganization Notes

**First reorganization (after 8-J):**
- **8-K was:** Combat Action Animations → **moved to 10-M**
- **8-L was:** Settlement UI Animations → **moved to 10-N**
- **8-K is now:** Hunter Movement UI
- **8-L is now:** Monster Action Execution Engine (first pass)

**Second reorganization (after 8-L):**
- Playtesting the 8-L execution engine revealed the shell/flesh HP system was frustrating — combat pacing felt arbitrary and monsters died too quickly or survived too long depending on which body parts were targeted
- **8-M** (new, immediately after 8-L): Replaces the data model — shell/flesh HP removed entirely; behavior cards are the monster's only health pool; a separate shuffled wound location deck determines Force Check targets and resource rewards; no escalation groups — all cards for a difficulty are one unified deck
- **8-N** (new, after 8-M): Implements the runtime side — `BehaviorDeck`/`WoundDeck` wrappers, Fisher-Yates pool-based deck construction, rebuilt `MonsterAI.ExecuteCard` with 6-step sub-phase flow and Grit windows, full wound resolution logic, defeat condition
- **8-O** (new, after 8-N): Hunter deployment phase — player places hunters in a spawn zone before VitalityPhase; replaces hardcoded starting positions
- **8-P** (new, after 8-O): Active hunter selection — player clicks any non-acted hunter token to switch who they're acting as
- **8-Q** (new, after 8-P): Combat terrain — obstacle cells (block movement) and bonus terrain cells (accuracy/defense modifiers)
- **8-R** (Combat Verify gate, after 8-Q): Tests the full new system (wound location draws, behavior deck health, deployment, terrain) before Stage 9 content layers on top
- **8-S through 8-X** continue in original order (formerly 8-M through 8-R) after the verify gate

---

## Stage 9 — Content, Events & Systems Depth

| Session | File | Topic |
|---------|------|--------|
| 9-A | STAGE_09_A.md | Second Monster — The Wailing Hollowed |
| 9-B | STAGE_09_B.md | The Ivory Stampede (pack monster) |
| 9-C | STAGE_09_C.md | Status Effects — Slowed, Shaken, Stunned, Pinned |
| 9-D | STAGE_09_D.md | Hunter Injuries & Disorders |
| 9-E | STAGE_09_E.md | Gear Set Bonuses |
| 9-F | STAGE_09_F.md | Settlement Innovation Tree |
| 9-G | STAGE_09_G.md | Chronicle Events Batch 1 (Years 1–10) |
| 9-H | STAGE_09_H.md | Chronicle Events Batch 2 (Years 11–20) |
| 9-I | STAGE_09_I.md | Chronicle Events Batch 3 (Years 21–30) |
| 9-J | STAGE_09_J.md | Overlord Arrival — The Pale Court |
| 9-K | STAGE_09_K.md | Overlord Combat Mechanics |
| 9-L | STAGE_09_L.md | Third Monster — The Ashen Crawler |
| 9-M | STAGE_09_M.md | Fourth Monster — The Veil Serpent |
| 9-N | STAGE_09_N.md | Hunter Lifecycle — Aging, Veteran Cards |
| 9-O | STAGE_09_O.md | Gear Crafting Tree Completion |
| 9-P | STAGE_09_P.md | Settlement Buildings — Second Tier |
| 9-Q | STAGE_09_Q.md | Aggro & Threat Mechanics Depth |
| 9-R | STAGE_09_R.md | Stage 9 Smoke Test — Full Campaign Simulation |
| 9-S | STAGE_09_S.md | Stage 9 Final Integration & DoD (tag v0.9) |
| **9-T** | **STAGE_09_T.md** | **Bleed & Poison — Status Counter System (stacking, tick damage, limb-wound trigger, cure API)** |
| **9-U** | **STAGE_09_U.md** | **Limb Wound → Disorder Trigger (DisorderCardSO, campaign disorder deck, post-hunt application)** |

---

## Stage 10 — Polish, Balance & Ship

Execute in order: **10-A → 10-B → ... → 10-K → 10-M → 10-N → 10-L**

The filename letters (K, M, N, then L at the end) do not sort alphabetically into execution order. Follow the table below, not filename sort. The `Next session:` pointers in each file are correct.

| Session | File | Topic |
|---------|------|--------|
| 10-A | STAGE_10_A.md | Audio Pass — Combat & Settlement SFX |
| 10-B | STAGE_10_B.md | Audio Pass — Music & Ambient Loops |
| 10-C | STAGE_10_C.md | Art Pass — Settlement Scene Backgrounds |
| 10-D | STAGE_10_D.md | Art Pass — Hunter Portrait Variants |
| 10-E | STAGE_10_E.md | Art Pass — Monster Illustrations |
| 10-F | STAGE_10_F.md | Art Pass — Gear & Behavior Card Art |
| 10-G | STAGE_10_G.md | Accessibility — Colorblind Modes, Font Scaling |
| 10-H | STAGE_10_H.md | Controller & Input Remapping |
| 10-I | STAGE_10_I.md | Save Migration & Version Compat |
| 10-J | STAGE_10_J.md | Credits Scene |
| 10-K | STAGE_10_K.md | Balance Pass — Economy, Monster HP, Encounter Pacing |
| **10-M** | **STAGE_10_M.md** | **Combat Action Animations — Hit Flash, Part Break, Collapse Pulse** |
| **10-N** | **STAGE_10_N.md** | **Settlement UI Animations — Craft Pulse, Gear Flash, Year Banner** |
| 10-L | STAGE_10_L.md | Final Integration & Ship — DoD, Windows Build, v1.0-gold Tag |

---

## Summary — What Changed and Why

### First Reorganization (after 8-J)
The card and settlement animation work originally planned for 8-K and 8-L was postponed to Stage 10 (now 10-M and 10-N) because the combat mechanics needed to be verified working before adding visual polish on top. Hunter Movement UI was prioritized as 8-K and Monster Execution Engine as 8-L.

### Second Reorganization (after 8-L)
The first-pass monster execution engine (8-L) used a shell/flesh HP system split across body parts. Playtesting revealed this created frustrating combat:
- Hunters targeted whichever part had the most shell, creating repetitive optimal lines
- Monster "deaths" felt arbitrary — depended heavily on which parts were targeted, not round count
- The body part system added UI complexity (many bars to track) without adding interesting decisions

The solution, validated in design before implementation:
1. **Behavior cards ARE the monster's health.** Defeating the monster means drawing the wound location deck repeatedly until the behavior deck is empty. Each hit is meaningful — one card off the monster's health pool, and you see exactly which card was removed.
2. **Wound location deck** determines the Force Check difficulty and any special effects on each hit — drawn randomly, creating unpredictability while still being legible (you can see the wound deck if you have abilities to look ahead).
3. **No escalation groups.** A single shuffled deck per difficulty; harder difficulties have more and stronger cards.
4. **8-M** (data model only, no runtime), **8-N** (runtime), then **8-O/P/Q** (remaining combat UX features) before the Combat Verify gate (**8-R**).
