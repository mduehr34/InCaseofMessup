<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-R | Stage 8 Final Integration & Definition of Done
Status: Stage 8-Q complete. All UI systems built.
Task: This is the final integration session for Stage 8.
Run the full end-to-end smoke test: Bootstrap → Main Menu →
Campaign Select → Character Creation → Settlement → Hunt Travel
→ Combat → Settlement return → Chronicle/Codex → Year-End →
Year 2 → Year 30 Victory. Fix any compile errors, null refs,
or broken wiring. Tag the commit as v0.8.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_R.md
- All STAGE_08_*.md files for the full picture

Then confirm:
- All scripts compile without errors
- All scene transitions use SceneTransitionManager (no raw LoadScene calls)
- All UI is UIToolkit only (no Canvas/uGUI objects)
- Save/Load round-trips cleanly (save Year 3, quit, continue → Year 3)
- Tutorial only fires on tutorial campaigns
- What you will NOT fix here (Stage 9 content gaps — those are next)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-R: Stage 8 Final Integration & Definition of Done

**Resuming from:** Stage 8-Q complete — Save/Load, Game Over, Victory Epilogue done
**Done when:** Full game flow runs end-to-end without errors; all Stage 8 verification items checked; commit tagged `v0.8`
**Commit:** `"8R: Stage 8 final integration — full flow smoke test, v0.8"`
**Next session:** STAGE_09_A.md

---

## What This Session Does

This is a verification and stitching session — no new features. You will:

1. Check that all scripts compile
2. Check all scene wiring (Build Settings order, transitions)
3. Run through the full game flow manually (or via Unity Play Mode)
4. Fix any bugs found during the smoke test
5. Make the final v0.8 commit

---

## Pre-Flight: Compile Check

In Unity, open the Console window (Window → General → Console). There should be **zero compile errors** before you do anything else.

Common issues to look for:

| Error Pattern | Likely Cause | Fix |
|---|---|---|
| `ChronicleController.ChronicleEntry` not found | ChronicleEntry struct defined inside namespace block incorrectly | Move struct to top level of namespace |
| `CampaignState` missing field | A field added in a Stage 8 session wasn't added to `CampaignState.cs` | Add the missing field |
| `SceneTransitionManager` not found | Script not in the right Assembly | Check the .asmdef includes `Core.Systems` |
| `AudioContext` missing enum value | `MainMenu` or `HuntTravel` not added | Add missing values to the enum |
| `HunterState` missing `yearsActive` | Field added in 8-P but not in the SO | Add field |

---

## CampaignState Complete Field Checklist

Open `Assets/_Game/Scripts/Core.Data/CampaignState.cs` and confirm ALL of these fields exist:

```csharp
// Campaign identity
public string   campaignName;
public string   difficulty;         // "Standard" | "Veteran" | "Nightmare"
public bool     isIronman;
public bool     isTutorialCampaign;
public int      currentYear;        // 1–30

// Hunters
public HunterState[] hunters;
public PendingChild[] pendingChildren;

// Resources
public int bone;
public int hide;
public int sinew;
public int ichor;
public int ivory;
public int membrane;
public int rotGland;

// Hunt tracking
public string   currentHuntMonsterName;
public string   currentHuntDifficulty;
public string[] resolvedEventIds;

// Chronicle & Codex
public ChronicleController.ChronicleEntry[] chronicleEntries;
public string[]                              unlockedCodexEntryIds;

// Yearly counters (reset each year)
public int    yearHuntsWon;
public int    yearHuntsLost;
public int    yearHunterDeaths;
public int    yearItemsCrafted;
public string[] yearDeadHunterNames;

// Lifetime counters
public int totalHunterDeaths;
public int overlordKillCount;
```

---

## HunterState Complete Field Checklist

Open `Assets/_Game/Scripts/Core.Data/HunterState.cs` and confirm:

```csharp
public string hunterId;
public string hunterName;
public string buildName;
public string sex;           // "M" | "F"
public int    yearsActive;   // increments each year
public bool   isDead;
public bool   isRetired;
public string permanentDebuffs;  // comma-separated, e.g. "Weathered"

// Stats
public int accuracy;
public int evasion;
public int toughness;
public int speed;
public int grit;
public int luck;
public int currentFlesh;
public int maxFlesh;

// Gear
public string[] equippedGearIds;
public string[] injuryIds;
public string[] disorderIds;
public string[] fightingArtIds;
```

---

## Scene Wiring Checklist

In Unity, open **File → Build Settings** and verify the scene order from Stage 8-Q:

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

For each scene, open it and verify the UIDocument GameObject has the correct controller component attached:

| Scene | Controller |
|---|---|
| Bootstrap | BootstrapManager |
| MainMenu | MainMenuController |
| CampaignSelect | CampaignSelectController |
| CharacterCreation | CharacterCreationController |
| Settlement | SettlementScreenController, SettlementAnimationController, BirthController, RetirementController, YearEndSummaryController, SaveLoadController |
| HuntTravel | TravelController |
| CombatScene | CombatScreenController, CombatAnimationController, CombatHUDUpdater, StatusEffectDisplay |
| GameOver | GameOverController |
| VictoryEpilogue | VictoryEpilogueController |

---

## Persistent Managers Checklist

In the **Bootstrap** scene, verify there is a single GameObject (e.g., "Managers") with ALL of these components:

- `BootstrapManager`
- `GameStateManager`
- `AudioManager`
- `SceneTransitionManager`
- `TutorialTooltipManager`

All four must have `DontDestroyOnLoad(gameObject)` in their `Awake()` methods.

There must be **no duplicate** of any of these in any other scene.

---

## Full Smoke Test — Step by Step

Run through this checklist in Unity Play Mode. Check each box only when you've confirmed it works.

### Bootstrap → Main Menu
- [ ] Press Play in Bootstrap scene → main menu loads (no black screen hang)
- [ ] Title logo and background art visible
- [ ] CONTINUE button greyed out if no save exists
- [ ] Music plays (main menu music context)
- [ ] Hovering buttons changes their appearance

### Campaign Select → Character Creation
- [ ] NEW CAMPAIGN → CampaignSelect loads with fade
- [ ] Click Tutorial card → card highlights
- [ ] Click Standard card → card highlights, Tutorial deselects
- [ ] Click VETERAN difficulty → button shows active state
- [ ] IRONMAN toggle works (checkbox state)
- [ ] CONFIRM → CharacterCreation loads
- [ ] Four hunters generated with random names
- [ ] Click a name → text field appears for renaming
- [ ] Press Enter → name confirmed
- [ ] CONFIRM HUNTERS → Settlement loads

### Settlement — Year 1
- [ ] Settlement loads — all tabs visible (HUNTERS, GEAR, CRAFTING, INNOVATIONS, EVENTS, CHRONICLE, CODEX)
- [ ] HUNTERS tab shows all 4 hunters with stats
- [ ] EVENTS tab shows available events
- [ ] Resolve an event → chronicle entry added
- [ ] CHRONICLE tab → shows entry with year label
- [ ] CODEX tab → shows locked "???" cards for all 15 entries
- [ ] Tutorial tooltips appear (if Tutorial Campaign selected)
- [ ] INNOVATIONS tab → adoption dims card, unlocks cascade cards
- [ ] CRAFTING tab → recipe list renders; craft item → forge flash animation plays
- [ ] GEAR tab → gear grid renders; equip item → gold pulse plays

### Hunt Travel → Combat
- [ ] SEND HUNTING PARTY → HuntTravel scene loads
- [ ] Wilderness background visible
- [ ] Hunt target label correct (monster name + difficulty)
- [ ] 0–3 travel events fire; each shows narrative card
- [ ] Choose event option → card fades, next event appears
- [ ] CONTINUE TO HUNT → CombatScene loads
- [ ] Phase banner appears (VITALITY PHASE)
- [ ] Hand renders card visuals (category band, name, effect text)
- [ ] Play a card → card plays animation (scale+fade), effect resolves
- [ ] Behavior card activates → gold border pulse visible
- [ ] Hit shell → gold burst at impact; hit flesh → red splatter
- [ ] Miss → grey "MISS" text floats and fades
- [ ] Monster moves → smooth lerp across grid (0.25s)
- [ ] Part breaks → container flashes red 3x
- [ ] Hunter reaches 0 flesh → shake then dark tint
- [ ] Hunt ends → return to Settlement (win or loss recorded)

### Year-End Flow
- [ ] End Year button → Year-End Summary panel slides up
- [ ] Shows correct hunts won/lost, deaths, crafts
- [ ] Closing line is contextual
- [ ] Click ADVANCE → panel slides down
- [ ] Year banner "YEAR 2" appears and fades (1.8s hold)
- [ ] Veteran hunter (7+ years) → retirement panel appears
- [ ] Choose RETIRE → chronicle entry written, resources added
- [ ] Choose KEEP FIGHTING → "Weathered" status applied

### Save/Load
- [ ] SAVE GAME → slot panel shows 3 slots
- [ ] Save to Slot 1 → slot shows campaign name + year
- [ ] Quit and reload → CONTINUE → slot panel → load Slot 1 → correct year
- [ ] Overwrite confirmation appears when clicking filled slot

### Game Over
- [ ] Force a total party kill (set all hunters `isDead = true` via a Debug button or editor)
- [ ] GameOver scene loads with fade
- [ ] "THE SETTLEMENT FALLS" title visible
- [ ] Chronicle scroll shows all entries
- [ ] MAIN MENU button returns to Main Menu

### Victory Epilogue (Year 30)
- [ ] Set `CampaignState.currentYear = 30` in editor, call `CheckVictory()`
- [ ] VictoryEpilogue scene loads
- [ ] Epilogue text fades in paragraph by paragraph
- [ ] Stats row correct
- [ ] Ending text matches campaign outcome (overlord count, death count, codex count)

---

## Bug Fix Protocol

If you find a bug during the smoke test:

1. **Identify the file** — which script or UXML is responsible?
2. **Fix it minimally** — don't refactor; just fix the broken thing
3. **Re-run only the affected test step** — no need to restart the full smoke test
4. **Log the fix** in a comment above the changed code: `// 8-R fix: [description]`
5. **Continue** with the next smoke test step

---

## Common Fixes Reference

### UIToolkit Q<> returns null
The element name in C# doesn't match the `name=""` attribute in UXML. Double-check spelling and case.

### DontDestroyOnLoad manager is null in a scene
The scene was loaded directly (e.g., pressing Play in Settlement) instead of through Bootstrap. Always play from Bootstrap for testing. Add a null guard:
```csharp
if (GameStateManager.Instance == null) { Debug.LogError("No GameStateManager!"); return; }
```

### Coroutine stops unexpectedly
If the MonoBehaviour is destroyed mid-coroutine (e.g., scene unloads), the coroutine stops. This is expected — only trigger coroutines on persistent managers or confirm the scene is still loaded.

### StyleKeyword.Initial causes ArgumentException
Unity versions before 2022.2 don't support `StyleKeyword.Initial` for all properties. Use an explicit reset color instead:
```csharp
card.style.borderTopColor = new StyleColor(new Color(0.31f, 0.27f, 0.20f)); // Reset to default
```

---

## Final Commit

Once all smoke test items are checked:

```
git add -A
git commit -m "8R: Stage 8 final integration — full flow smoke test, v0.8"
git tag v0.8
```

---

## Stage 8 Definition of Done

Check every box before moving to Stage 9:

### Scenes
- [ ] Bootstrap, MainMenu, CampaignSelect, CharacterCreation all built and connected
- [ ] Settlement, HuntTravel, CombatScene all built and connected
- [ ] GameOver and VictoryEpilogue built and connected
- [ ] Credits scene exists as a stub

### Systems
- [ ] GameStateManager (DontDestroyOnLoad) — holds CampaignState, save/load, year advance
- [ ] AudioManager (DontDestroyOnLoad) — music contexts, SFX, PlayerPrefs volume
- [ ] SceneTransitionManager (DontDestroyOnLoad) — fade transitions, all scenes use it
- [ ] TutorialTooltipManager (DontDestroyOnLoad) — 10-step tutorial, skip, PlayerPrefs progress
- [ ] SaveSystem — 3 JSON slots, load/save/delete, slot info for UI

### UI Controllers
- [ ] MainMenuController — sprite loading, button wiring, continue gating
- [ ] CampaignSelectController — card highlight, difficulty buttons, ironman toggle
- [ ] CharacterCreationController — name generation, rename field, confirm
- [ ] SettlementScreenController — all 7 tabs wired
- [ ] SettlementAnimationController — craft flash, gear pulse, innovation glow, year banner
- [ ] TravelController — travel event queue, card display, CONTINUE TO HUNT
- [ ] CombatScreenController — combat flow, hand display, card play
- [ ] CombatAnimationController — hit, miss, part break, move, collapse
- [ ] CombatHUDUpdater — round display, phase banner, hunter stats
- [ ] StatusEffectDisplay — icon strip with durations
- [ ] ChronicleController — scrollable log, year labels
- [ ] CodexController — locked/unlocked grid, click to expand
- [ ] BirthController — naming panel, newborn registration
- [ ] RetirementController — retire/keep choice, resource reward
- [ ] YearEndSummaryController — summary panel, slide-up/down
- [ ] SaveLoadController — slot select, overwrite confirm, load/save dispatch
- [ ] GameOverController — death screen, chronicle scroll, navigation
- [ ] VictoryEpilogueController — paragraph reveal, outcome-driven text, stats row

### Data
- [ ] CodexEntrySO — 15 assets in `Assets/_Game/Data/Codex/`
- [ ] CampaignState — all fields from checklist above
- [ ] HunterState — all fields from checklist above
- [ ] TutorialStep — 10 steps hardcoded in TutorialTooltipManager
- [ ] PendingChild — struct for newborns

### Stage 7R Deferred Implementations (confirm stubs are present before v0.8 commit)
These handlers were intentionally deferred in Stage 7R and must exist as `// TODO` stubs in `CombatManager.cs` so they are not forgotten before Stage 9:
- [ ] `// TODO: 7R — handle GAUNT_3PC_LOUD_SUPPRESS` — behavior cards triggered by Loud card plays reduce movement by 2; stub in the behavior card resolution switch/dispatch
- [ ] `// TODO: 7R — handle GAUNT_5PC_DEATH_CHEAT` — once per hunt, when a hunter would collapse, survive with 1 Flesh; stub in the collapse trigger path
- [ ] `// TODO: 7R — EyePendant scar intercept` — EyePendant 2pc effect intercepts injury card application; stub in the injury card resolution path

### Audio
- [ ] 6 music tracks generated (main menu, settlement early/late, travel, combat standard/overlord)
- [ ] 9 SFX generated (UI click, craft success, gear equip, innovation adopt, card draw/play/discard, hit shell, hit flesh)
- [ ] AudioManager.SetMusicContext() called correctly on each scene

### Art
- [ ] Main menu background + title logo
- [ ] Campaign select cards (Tutorial, Standard)
- [ ] Status effect icons × 8
- [ ] Card category icons × 14
- [ ] Card frame overlay
- [ ] Deck stack sprite
- [ ] Combat impact fx × 4
- [ ] Hunt travel background
- [ ] ui_card_frame.png

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_A.md`
**Covers:** Injury, Scar, Disorder & Fighting Art Card SOs — creating the ScriptableObject definitions and all relevant card assets for these hunter lifecycle card types, which are referenced in the existing card system but not yet created as assets
