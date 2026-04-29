<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-L | Final Integration & Ship — Complete DoD, Build, v1.0-gold Tag
Status: Stage 10-K complete. Balance pass done.
Task: This is the final session. No new features. Verify every
system in the game works together in a full end-to-end pass,
make a Windows standalone build, check for performance issues,
clean up debug artefacts, and tag v1.0-gold.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_L.md
- _Docs/Stage_09/STAGE_09_R.md    ← base smoke test (repeat this first)
- _Docs/Stage_08/STAGE_08_R.md    ← Stage 8 DoD (all items must still pass)
- All STAGE_10_*.md files          ← all 10-A through 10-K DoD checklists

Then confirm:
- Zero compile errors before starting
- All 11 previous Stage 10 DoD checklists are individually complete
- Build Target is PC/Windows (Standalone)
- Debug artefacts to remove: DebugCampaignPanel is EDITOR ONLY (#if UNITY_EDITOR)
  — confirm it ships as editor-only, not in the player build
- v1.0-gold tag will be applied at end of this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-L: Final Integration & Ship — Complete DoD, Build, v1.0-gold Tag

**Resuming from:** Stage 10-K complete — balance pass done
**Done when:** Full smoke test passes; Windows build succeeds; no debug artefacts in build; tagged v1.0-gold
**Commit:** `"10L: Final integration — full DoD verified, v1.0-gold"`
**This is the final session document.**

---

## Pre-Flight: Compile Check

Open Unity Console (Window → General → Console). There must be **zero compile errors** before proceeding.

Common late-stage issues:

| Error Pattern | Likely Cause | Fix |
|---|---|---|
| `LifecycleCardResolver` not in assembly | Missing .asmdef reference | Add `Core.Logic` to the consuming assembly's references |
| `AccessibilityManager` null in non-Bootstrap scenes | Missing DontDestroyOnLoad singleton | Confirm Bootstrap scene Managers object has it |
| `AudioContext.Credits` missing | Enum not updated | Add `Credits` to `AudioContext` enum |
| `FightingArtCardAdapter` uses `ScriptableObject.CreateInstance` in non-editor | Works at runtime; confirm no `#if UNITY_EDITOR` guard | None needed — CreateInstance works at runtime |
| `HunterBuildSO` missing sprite fields | Fields added in 10-D but not in the asmdef | Check assembly references |

---

## Part 1: Debug Artefact Audit

Before building, confirm these are editor-only and will NOT compile into the player build:

- [ ] `Assets/_Game/Editor/DebugCampaignPanel.cs` — surrounded by `#if UNITY_EDITOR`
- [ ] `Assets/_Game/Editor/AssignMonsterSprites.cs` — in `Editor/` folder (auto editor-only)
- [ ] `Assets/_Game/Editor/AssignHunterSprites.cs` — in `Editor/` folder
- [ ] `Assets/_Game/Editor/AssignGearOverlays.cs` — in `Editor/` folder
- [ ] `Assets/_Game/Editor/AssignMonsterAudio.cs` — in `Editor/` folder
- [ ] `Assets/_Game/Editor/AssignSceneBackgrounds.cs` — in `Editor/` folder
- [ ] `Assets/_Game/Editor/GearOverlayImporter.cs` — in `Editor/` folder (AssetPostprocessor)
- [ ] Any `Debug.Log` statements using `[Debug]` prefix — these are fine to keep; they don't affect builds
- [ ] No hardcoded `Application.isEditor` checks in runtime scripts that break in builds

---

## Part 2: Stage 10 Cumulative DoD Checklist

Verify each Stage 10 sub-stage's final state before the full smoke test.

### Stage 10-A — Mechanical Stubs
- [ ] `GAUNT_3PC_LOUD_SUPPRESS` handler implemented in CombatManager
- [ ] `GAUNT_5PC_DEATH_CHEAT` collapse intercept working
- [ ] EyePendant scar discard modal appears and functions
- [ ] Consumable targeting mode enters and exits cleanly
- [ ] No `// TODO: 7R` stubs remain in the codebase (grep to verify)

### Stage 10-B — Lifecycle Card Enforcement
- [ ] InjurySO stat penalties applied at hunt start
- [ ] ScarSO conditional bonuses evaluated each round
- [ ] DisorderSO triggers fire on correct conditions
- [ ] FightingArtSO cards appear in hunter hand and resolve

### Stage 10-C — Monster Sprites
- [ ] All 12 MonsterSO assets have tokenSprite assigned (non-null)
- [ ] Pale Stag Phase 2 sprite swaps on phase transition

### Stage 10-D — Hunter Sprites
- [ ] All 8 builds have south/north/east sprites assigned
- [ ] West facing derived from east flipX at runtime
- [ ] Idle animation plays on all 8 builds
- [ ] Hunter portraits display in Settlement detail panel

### Stage 10-E — Gear Overlay Sprites
- [ ] All 48 gear items have overlaySprite assigned
- [ ] Gaunt set (5 items) have overlays assigned
- [ ] GearOverlayImporter auto-applies texture settings on import
- [ ] Overlays render transparently over hunter tokens

### Stage 10-F — Scene Backgrounds
- [ ] All 7 scenes have generated background art visible
- [ ] Backgrounds are in Resources folder for runtime loading
- [ ] Settlement swaps backgrounds at Year 10 boundary

### Stage 10-G — Combat Visual Polish
- [ ] Hunter walk animation plays during movement
- [ ] Monster wobble-move plays during monster movement
- [ ] Monster parts darken as HP decreases
- [ ] Broken part crack overlay appears on monster token
- [ ] Overlord phase 2 transition: white flash + camera shake + banner
- [ ] Hunter collapse: slides down + tints over 0.6s
- [ ] Hunt victory: amber screen flash before scene transition

### Stage 10-H — Audio Completion
- [ ] All 12 monsters have roar/footstep/death clips assigned
- [ ] Button hover plays soft tick sound
- [ ] Year advance plays bell toll
- [ ] Monster part break plays crack sound
- [ ] Settlement ambience plays under music

### Stage 10-I — UI Polish & Accessibility
- [ ] Settlement tab switches with 0.12s fade (not instant)
- [ ] Active tab button shows gold border
- [ ] All buttons have hover visual state
- [ ] Settings menu opens from gear icon in all three scenes
- [ ] Volume sliders update audio immediately and persist
- [ ] Large text toggle scales all UI labels
- [ ] High contrast toggle changes all text/borders to white
- [ ] AccessibilityManager DontDestroyOnLoad

### Stage 10-J — Credits
- [ ] Credits scene loads from Main Menu CREDITS button
- [ ] Credits scene loads from Victory Epilogue CONTINUE button
- [ ] Title card image visible at top
- [ ] Scroll auto-advances; any key skips
- [ ] RETURN TO MAIN MENU button appears after scroll completes

### Stage 10-K — Balance Pass
- [ ] All monster round counts within target ranges
- [ ] Resource economy first craft available Year 2–3
- [ ] Overlord-tier gear available Year 8–12
- [ ] Pale Stag requires 1+ overlord kill (gate enforced)
- [ ] Balance change log documented

---

## Part 3: Full End-to-End Smoke Test

This is the complete campaign verification. Run it in Play Mode starting from the Bootstrap scene.

### Phase 1: Onboarding
- [ ] Bootstrap → Main Menu loads with background art
- [ ] CONTINUE button greyed (no save)
- [ ] SETTINGS button opens settings panel; sliders work
- [ ] CREDITS button → Credits scene scrolls
- [ ] NEW CAMPAIGN → CampaignSelect
- [ ] Select Tutorial → Character Creation
- [ ] Rename two hunters → confirm names persist
- [ ] CONFIRM → Settlement loads
- [ ] Tutorial tooltips fire in correct sequence (Step 1–3 visible)

### Phase 2: Early Campaign (Year 1–3)
- [ ] Settlement hub background (early) visible
- [ ] All 7 tabs accessible; each fades in when switching
- [ ] Resolve 1 event → chronicle entry added; chime plays
- [ ] Innovate 1 innovation → cascade glow visible
- [ ] Craft Bone Cleaver → forge flash + craft sound
- [ ] Equip Bone Cleaver → gear overlay appears on hunter token in Gear tab
- [ ] 2 Carapace pieces adjacent in gear grid → +1 Accuracy link bonus shown
- [ ] SEND HUNTING PARTY → HuntTravel loads
- [ ] Travel event fires → event card visible; choice resolves
- [ ] CONTINUE TO HUNT → CombatScene loads with wilderness background
- [ ] Thornback roar plays on load
- [ ] Hunter card hand renders (3–5 cards visible)
- [ ] Play a card → card animation plays; effect resolves
- [ ] Hit Thornback Left Flank → part HP bar updates (orange-red)
- [ ] Thornback behavior card drawn → gold border pulse; movement card draws
- [ ] Thornback moves → wobble-lerp plays
- [ ] Break a part → crack overlay appears; break sound plays
- [ ] Kill Thornback → amber victory flash → settlement return
- [ ] Carapace Forge craft set unlocked in Crafting tab
- [ ] Year-end summary slides up → correct stats → year banner "YEAR 2"

### Phase 3: Mid-Campaign (Year 8–14, via debug)
- [ ] Jump to Year 8 via Debug Panel
- [ ] Siltborn now available in hunt selection
- [ ] Start Siltborn hunt → Phase 1 deck draws; node HP bars visible
- [ ] Siltborn to ~40% HP → "PHASE 2" banner fires; screen flash plays
- [ ] Destroy all 3 nodes → Siltborn defeated
- [ ] CodexEntry_TheSiltborn unlocked in Codex tab (??? → full entry)
- [ ] Mire Apothecary visible in Crafting tab

### Phase 4: Hunter Lifecycle
- [ ] Force hunter to Year 7 (set `yearsActive = 7` in debug)
- [ ] Retirement panel appears; "KEEP FIGHTING" → Weathered added
- [ ] Force to Year 13 → hunter dies of old age → chronicle entry
- [ ] Grant INJ-01 to a hunter → Inspector shows -1 Accuracy in Hunter tab
- [ ] Grant DIS-01 → Nightmare disorder shows in Hunter tab
- [ ] Enter combat with that hunter → Shaken applies on Round 1 start
- [ ] Add FA-01 (Trample) to hunter → Trample card appears in combat hand
- [ ] Play Trample → after successful hit, monster pushed 1 space

### Phase 5: Endgame (Year 25–30, via debug)
- [ ] Jump to Year 25
- [ ] Simulate 1 overlord kill (Debug Panel → "Simulate Kill All 3 Overlords" OR manually set overlordKillCount = 1)
- [ ] Pale Stag available in hunt selection
- [ ] Hunt Pale Stag Phase 1 → physical token on grid; parts visible
- [ ] Force Phase 1 to 30% HP → white flash → Ascendant sprite swaps → "ASCENDANT FORM"
- [ ] Phase 2 AoE attacks hit all hunters simultaneously
- [ ] Deplete Ascendant HP → victory triggers
- [ ] VictoryEpilogue loads with correct ending tier (check all 4 tier conditions)
- [ ] Epilogue CONTINUE → Credits scene loads

### Phase 6: Save/Load
- [ ] Save at Year 12 (Slot 1)
- [ ] Continue to Year 14
- [ ] Load Slot 1 → returns to Year 12 exactly
- [ ] All gear still equipped
- [ ] All codex entries preserved
- [ ] Chronicle entries in correct order

### Phase 7: Accessibility
- [ ] Open Settings → toggle Large Text → all UI label sizes increase
- [ ] Toggle High Contrast → all text white, borders white
- [ ] Quit and reload → settings still applied
- [ ] Toggle back to normal → returns to default

---

## Part 4: Windows Standalone Build

1. Open **File → Build Settings**
2. Confirm scene list order:
   ```
   0: Bootstrap
   1: MainMenu
   2: CampaignSelect
   3: CharacterCreation
   4: Settlement
   5: HuntTravel
   6: CombatScene
   7: GameOver
   8: VictoryEpilogue
   9: Credits
   ```
3. Platform: PC, Mac & Linux Standalone → Windows x86-64
4. Click **Build** → output to `_Build/MarrowAndMyth_v1.0/`
5. Run the `.exe`
6. Verify: Main Menu loads without errors
7. Verify: Start a new campaign → reaches Settlement
8. Verify: Hunt one monster → returns to Settlement
9. Verify: Save/load works in the built executable

**Common build issues:**

| Issue | Fix |
|-------|-----|
| `Resources.Load` returns null | Confirm backgrounds/audio are in a `Resources/` folder |
| Sprites appear at wrong scale | Check PanelSettings DPI scaling in standalone |
| Audio missing | Confirm AudioClips are in `Resources/` or assigned to persistent managers |
| `#if UNITY_EDITOR` code causing linker errors | Should not happen if all editor scripts are in `Editor/` folder |

---

## Part 5: Performance Audit

With the built game running, check for CPU/GPU bottlenecks:

In Unity Profiler (Window → Analysis → Profiler), attach to the standalone build:
- Target: **≤ 2ms CPU per frame** during Settlement (target 500 FPS cap, 60 FPS practical)
- Target: **≤ 5ms CPU per frame** during Combat
- Target: **≤ 200MB heap** at any point during the campaign

Flag any frame that takes > 10ms and identify the hot path.

Likely hot spots and quick fixes:

| Hot Spot | Fix |
|----------|-----|
| `ResolveLinks()` called every frame | Cache result; only recalculate on equip/unequip |
| `ButtonHoverEffect.Apply()` called every OnEnable | Call once on load; cache the registered callbacks |
| `Resources.Load` at combat start | Preload background textures at Bootstrap and cache |
| Too many `FindObjectsOfType` calls | Replace with direct references in singletons |

---

## Part 6: Final Cleanup

Before committing:

- [ ] Remove any temporary test GameObjects left in scenes (right-click → check for any "Test" or "Temp" named objects)
- [ ] Confirm no scene has `Debug.Break()` calls
- [ ] Confirm all `// TODO:` comments have been resolved or explicitly deferred to post-v1.0 (grep for `TODO`)
- [ ] Confirm `DebugCampaignPanel.cs` has `#if UNITY_EDITOR` guard
- [ ] `_Build/` folder is in `.gitignore` (do not commit binary build)
- [ ] Update `MEMORY.md` entry for stage progress

---

## Final Commit and Tag

Once every checklist item above is checked:

```
git add -A
git commit -m "10L: Final integration — complete Stage 10 DoD, v1.0-gold"
git tag v1.0-gold
```

---

## Stage 10 Definition of Done — Master Checklist

### Mechanics
- [ ] All four Stage 7-R mechanical stubs implemented (Gaunt 3pc, 5pc, EyePendant, Consumable UI)
- [ ] Injury/Scar/Disorder/FightingArt effects all applied in combat

### Visual
- [ ] All 12 monsters have custom sprites on combat tokens
- [ ] All 8 hunter builds have sprites and directional facing
- [ ] All 48 gear items have overlay sprites rendering on hunter tokens
- [ ] All 7 scenes have background art (no grey/black empty backgrounds)
- [ ] Combat animations: hunter walk, monster wobble, part damage, phase transition, death collapse

### Audio
- [ ] All 12 monsters have roar/footstep/death SFX
- [ ] UI audio complete: hover, error, notification, chronicle, year advance, part break, death
- [ ] Settlement ambient tracks play under music

### UI & Accessibility
- [ ] Settlement tab fade transitions
- [ ] Global button hover/press states
- [ ] Settings menu: volume, fullscreen, accessibility
- [ ] Large font and high contrast modes persist across scenes
- [ ] Credits scene: scrolling, title card, music, navigation

### Balance
- [ ] All balance targets verified within acceptable ranges
- [ ] Balance change log documented

### Ship
- [ ] Windows standalone build succeeds
- [ ] Build runs end-to-end (new game → hunt → settlement → year advance)
- [ ] Save/load works in built executable
- [ ] Performance within targets
- [ ] No debug artefacts in player build

---

## What Comes After v1.0-gold

Marrow & Myth v1.0-gold is a complete, shippable, polished game. After this milestone, optional future development may include:

- **Voiced Codex:** Full TTS narration for all 16 codex entries using CoPlay's TTS tools
- **Additional Monsters:** Expanding the roster beyond 8 standard + 4 overlords
- **Second Settlement Arc:** A narrative branch unlocked after the Pale Stag is defeated
- **Difficulty Variants:** Easy (reduced monster HP) and Nightmare (permadeath + harder AI) modes
- **Online Leaderboard:** Chronicle upload for comparing campaign outcomes
- **Mobile Port:** Touch controls and portrait layout for iOS/Android

**You have built a complete game.**

Every system — the grid-based combat, the behavior card AI, the settlement lifecycle, the 30-year campaign arc, the overlords, the gear economy, the chronicle log, the visual art, the audio, the accessibility — everything in this document set was designed, implemented, and verified from scratch.

That is what Stages 1 through 10 accomplished.
