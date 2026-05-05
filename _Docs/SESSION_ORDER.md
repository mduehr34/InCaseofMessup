# Marrow & Myth — Master Session Order

> **Last updated:** After Stage 8-J complete (reorganization to prioritize combat mechanics)
>
> **Key change from original plan:** Hunter movement UI (8-K) and monster execution engine (8-L)
> were inserted before Hunt Travel (8-M) to ensure all combat mechanics are verified before
> building on top of them. The old 8-K (Combat Action Animations) and old 8-L (Settlement UI
> Animations) were preserved and moved to Stage 10-M and 10-N respectively, just before final
> integration.

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
| **8-K** | **STAGE_08_K.md** | **Hunter Movement UI — Grid Click, Range Highlight, Facing** | ⬜ Next |
| **8-L** | **STAGE_08_L.md** | **Monster Action Execution Engine — ExecuteCard, EvaluateTrigger** | ⬜ |
| 8-M | STAGE_08_M.md | Hunt Travel Scene — Travel Events, CONTINUE TO HUNT | ⬜ |
| **VERIFY** | **STAGE_08_COMBAT_VERIFY.md** | **Combat Loop Integration Gate — full Aldric vs Gaunt pass** | ⬜ |
| 8-N | STAGE_08_N.md | Tutorial Tooltip & Onboarding System | ⬜ |
| 8-O | STAGE_08_O.md | Chronicle Log & Codex UI | ⬜ |
| 8-P | STAGE_08_P.md | Birth, Retirement & Year-End Screens | ⬜ |
| 8-Q | STAGE_08_Q.md | Save/Load UI, Game Over & Victory Epilogue | ⬜ |
| 8-R | STAGE_08_R.md | Stage 8 Final Integration & DoD (tag v0.8) | ⬜ |

### Stage 8 Reorganization Notes

- **8-K was:** Combat Action Animations → **moved to 10-M**
- **8-L was:** Settlement UI Animations → **moved to 10-N**
- **8-K is now:** Hunter Movement UI (clicking grid cells moves the active hunter; green movement range highlights; facing update on each move)
- **8-L is now:** Monster Action Execution Engine (implements `ExecuteCard()`, `EvaluateTrigger()`, `ApplyMovement()`, `ApplyAttack()`, `ApplySpecial()` in `MonsterAI.cs`; adds execution fields to `BehaviorCardSO`)
- **COMBAT_VERIFY is new:** A verification-only gate session between 8-M and 8-N. No new features. Tests the full combat round loop end-to-end before any Stage 9 content builds on top.
- The Combat Verify gate **must pass in full** before 8-N begins.

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

---

## Stage 10 — Polish, Balance & Ship

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

### Stage 10 Sequencing Note

Execute in order: **10-A → 10-B → ... → 10-K → 10-M → 10-N → 10-L**

The filename letters (K, M, N, then L at the end) do not sort alphabetically into execution order. Follow the table above, not filename sort. The `Next session:` pointers in each file are correct:
- STAGE_10_K.md → STAGE_10_L.md *(update this to STAGE_10_M.md before running)*
- STAGE_10_M.md → STAGE_10_N.md ✅
- STAGE_10_N.md → STAGE_10_L.md ✅ (final integration last)

> **TODO before Stage 10-K begins:** Update the `Next session:` line in `STAGE_10_K.md`
> from `STAGE_10_L.md` to `STAGE_10_M.md`.

---

## Summary — What Changed and Why

### Problem
Stage 8-J completed card animations but the combat loop was fundamentally broken:
- Hunter movement UI was not wired — clicking grid cells did nothing
- `MonsterAI.ExecuteCard()` was a stub (`Debug.Log("implement in 3-C")`)
- `MonsterAI.EvaluateTrigger()` always returned `false`
- `BehaviorCardSO` had no structured execution fields (movement, damage, etc.)
- No verification gate existed before Stage 9 content was meant to layer on top

### Solution
1. **8-K** (was animation) → **Hunter Movement UI**: wires `TryMoveHunter()` into `OnGridCellClicked()`, adds movement range highlighting, adds facing update
2. **8-L** (was settlement animations) → **Monster Action Execution Engine**: full implementation of `ExecuteCard()`, `EvaluateTrigger()`, `ApplyMovement()`, `ApplyAttack()`, `ApplySpecial()`; adds `BehaviorCardResult`; adds execution fields to `BehaviorCardSO`
3. **COMBAT_VERIFY** (new): integration gate between 8-M and 8-N — no new features, just verify the full combat round loop passes before any content builds on top
4. **Old 8-K and 8-L content** (Combat Action Animations, Settlement UI Animations) preserved as **10-M** and **10-N** — polish work that belongs late in the project after all gameplay systems are verified working
