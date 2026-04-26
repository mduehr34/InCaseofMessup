<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-R | Stage 9 Final Integration & Full Campaign DoD
Status: Stage 9-Q complete. All 42 gear items done.
Task: This is the final integration session. Run the complete
full-campaign smoke test from Year 1 through Year 30 (using a
fast-forward debug mode). Verify all monsters, gear sets, codex
entries, overlords, and progression systems work end-to-end.
Fix any remaining issues. Tag the milestone commit v1.0.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_R.md
- All STAGE_09_*.md files
- _Docs/Stage_08/STAGE_08_R.md (for Stage 8 DoD reference)

Then confirm:
- A debug cheat panel exists (or will be created here) to fast-
  forward the campaign to specific years
- All 15 codex entries can be unlocked through normal play
- All 7 craft sets unlock through their correct conditions
- All 3 overlords are available in their correct year ranges
- Killing the Pale Stag triggers the victory epilogue
- v1.0 commit is clean — no untracked editor scripts or test files

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-R: Stage 9 Final Integration & Full Campaign DoD

**Resuming from:** Stage 9-Q complete — all 42 gear items done
**Done when:** Full 30-year campaign verified end-to-end; all systems connected; tagged v1.0
**Commit:** `"9R: Stage 9 final integration — full campaign DoD, v1.0"`
**This is the final session document.**

---

## Part 1: Debug Cheat Panel

**New developer note:** Testing a 30-year campaign by playing normally would take hours. The debug cheat panel lets you fast-forward to specific states for testing without affecting the shipped game. It is an **Editor-only tool** that appears in a separate Unity window — it is never visible to players.

**Path:** `Assets/_Game/Editor/DebugCampaignPanel.cs`

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Editor
{
    public class DebugCampaignPanel : EditorWindow
    {
        [MenuItem("MnM/Debug Campaign Panel")]
        public static void ShowWindow()
            => GetWindow<DebugCampaignPanel>("MnM Debug");

        private int  _targetYear     = 10;
        private bool _killAllOverlords = false;
        private string _unlockCodexId  = "";
        private string _unlockSetId    = "";

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug tools.", MessageType.Info);
                return;
            }

            var gsm = GameStateManager.Instance;
            if (gsm == null)
            {
                EditorGUILayout.HelpBox("GameStateManager not found.", MessageType.Warning);
                return;
            }

            var state = gsm.CampaignState;

            EditorGUILayout.LabelField("CAMPAIGN STATE", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Year: {state?.currentYear}");
            EditorGUILayout.LabelField($"Overlord Kills: {state?.overlordKillCount}");
            EditorGUILayout.LabelField($"Total Deaths: {state?.totalHunterDeaths}");
            EditorGUILayout.LabelField($"Codex Unlocked: {state?.unlockedCodexEntryIds?.Length ?? 0}/15");
            EditorGUILayout.Space();

            // Year jump
            EditorGUILayout.LabelField("Jump to Year", EditorStyles.boldLabel);
            _targetYear = EditorGUILayout.IntSlider(_targetYear, 1, 30);
            if (GUILayout.Button($"Set Year to {_targetYear}"))
            {
                if (state != null) state.currentYear = _targetYear;
                Debug.Log($"[Debug] Year set to {_targetYear}");
            }
            EditorGUILayout.Space();

            // Resources
            EditorGUILayout.LabelField("Grant Resources", EditorStyles.boldLabel);
            if (GUILayout.Button("Grant 20 of Each Resource"))
            {
                if (state != null)
                {
                    state.bone = state.hide = state.sinew = state.ichor =
                    state.ivory = state.membrane = state.rotGland = 20;
                }
                Debug.Log("[Debug] Resources granted.");
            }
            EditorGUILayout.Space();

            // Overlords
            EditorGUILayout.LabelField("Overlord Kills", EditorStyles.boldLabel);
            if (GUILayout.Button("Simulate Kill All 3 Overlords"))
            {
                if (state != null) state.overlordKillCount = 3;
                gsm.UnlockCraftSet("Mire");
                gsm.UnlockCraftSet("Ichor");
                gsm.UnlockCodexEntry("CodexEntry_TheSiltborn");
                gsm.UnlockCodexEntry("CodexEntry_ThePenitent");
                Debug.Log("[Debug] All 3 overlords marked as killed.");
            }
            EditorGUILayout.Space();

            // Craft sets
            EditorGUILayout.LabelField("Unlock Craft Set", EditorStyles.boldLabel);
            _unlockSetId = EditorGUILayout.TextField("Set ID:", _unlockSetId);
            if (GUILayout.Button("Unlock"))
            {
                if (!string.IsNullOrEmpty(_unlockSetId))
                    gsm.UnlockCraftSet(_unlockSetId);
            }
            EditorGUILayout.Space();

            // Codex
            EditorGUILayout.LabelField("Unlock Codex Entry", EditorStyles.boldLabel);
            _unlockCodexId = EditorGUILayout.TextField("Entry ID:", _unlockCodexId);
            if (GUILayout.Button("Unlock"))
            {
                if (!string.IsNullOrEmpty(_unlockCodexId))
                    gsm.UnlockCodexEntry(_unlockCodexId);
            }
            EditorGUILayout.Space();

            // Unlock all codex
            if (GUILayout.Button("Unlock ALL 15 Codex Entries"))
            {
                string[] all = {
                    "CodexEntry_WhispersBelow", "CodexEntry_FirstRuins",
                    "CodexEntry_TheOldWorks", "CodexEntry_SerpentIdol",
                    "CodexEntry_MarrowExposed", "CodexEntry_TheSpite",
                    "CodexEntry_TheFullPicture", "CodexEntry_SutureStirs",
                    "CodexEntry_IvoryShard", "CodexEntry_MarrowLore",
                    "CodexEntry_SettlementLog", "CodexEntry_TheSiltborn",
                    "CodexEntry_ThePenitent", "CodexEntry_ThePaleStag",
                    "CodexEntry_TheSuture"
                };
                foreach (var id in all) gsm.UnlockCodexEntry(id);
                Debug.Log("[Debug] All 15 codex entries unlocked.");
            }
            EditorGUILayout.Space();

            // Game Over / Victory
            EditorGUILayout.LabelField("Trigger End States", EditorStyles.boldLabel);
            if (GUILayout.Button("Force Game Over"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
            if (GUILayout.Button("Force Victory Epilogue"))
            {
                if (state != null) state.currentYear = 30;
                gsm.CheckVictory();
            }
        }
    }
}
#endif
```

**To open:** In Unity → **MnM → Debug Campaign Panel**. Only works in Play Mode.

---

## Part 2: Asset Existence Checklist

Run these checks in the Unity Project window before starting the smoke test.

### Monsters (10 total)
- [ ] `Thornback_Standard.asset`
- [ ] `IvoryStampede_Standard.asset`
- [ ] `BogCaller_Standard.asset`
- [ ] `Shriek_Standard.asset`
- [ ] `Rotmother_Nightmare.asset`
- [ ] `GildedSerpent_Standard.asset`
- [ ] `Ironhide_Standard.asset`
- [ ] `Siltborn_Overlord.asset`
- [ ] `Penitent_Overlord.asset`
- [ ] `PaleStag_Overlord.asset`

### Behavior Cards
- [ ] Thornback: 16 cards in `Monsters/Thornback/BehaviorCards/`
- [ ] Ivory Stampede: 12 cards in `Monsters/IvoryStampede/BehaviorCards/`
- [ ] Bog Caller: 16 cards in `Monsters/BogCaller/BehaviorCards/`
- [ ] Shriek: 16 cards in `Monsters/Shriek/BehaviorCards/`
- [ ] Rotmother: 16 cards in `Monsters/Rotmother/BehaviorCards/`
- [ ] Gilded Serpent: 16 cards in `Monsters/GildedSerpent/BehaviorCards/`
- [ ] Ironhide: 16 cards in `Monsters/Ironhide/BehaviorCards/`
- [ ] Siltborn Phase 1: 20 cards; Phase 2: 12 cards
- [ ] Penitent Phase 1: 18 cards; Phase 2: 10 cards
- [ ] Pale Stag Phase 1: 16 cards; Phase 2: 12 cards

### Gear (42 total)
- [ ] Carapace Forge: 6 items (`GEAR-CAR-01` through `GEAR-CAR-06`)
- [ ] Membrane Loft: 6 items (`GEAR-MEM-01` through `GEAR-MEM-06`)
- [ ] Mire Apothecary: 6 items (`GEAR-MIR-01` through `GEAR-MIR-06`)
- [ ] Ichor Works: 6 items (`GEAR-ICH-01` through `GEAR-ICH-06`)
- [ ] Auric Scales: 6 items (`GEAR-AUR-01` through `GEAR-AUR-06`)
- [ ] Rot Garden: 6 items (`GEAR-ROT-01` through `GEAR-ROT-06`)
- [ ] Ivory Hall: 6 items (`GEAR-IVY-01` through `GEAR-IVY-06`)

### Codex (15 entries)
- [ ] All 15 `Codex_*.asset` files in `Assets/_Game/Data/Codex/`

### Craft Sets (7 CraftSetSO assets)
- [ ] `CraftSet_Carapace.asset`
- [ ] `CraftSet_Membrane.asset`
- [ ] `CraftSet_Ichor.asset`
- [ ] `CraftSet_Auric.asset`
- [ ] `CraftSet_Rot.asset`
- [ ] `CraftSet_Ivory.asset`
- [ ] `CraftSet_Mire.asset`

### Lifecycle Cards
- [ ] Injuries: 10 assets in `Assets/_Game/Data/Injuries/`
- [ ] Scars: 8 assets in `Assets/_Game/Data/Scars/`
- [ ] Disorders: 8 assets in `Assets/_Game/Data/Disorders/`
- [ ] Fighting Arts: 10 assets in `Assets/_Game/Data/FightingArts/`

---

## Part 3: Full Campaign Smoke Test

Use the Debug Campaign Panel for all year-jump tests.

### Years 1–5: Early Campaign

- [ ] Start new Standard campaign → 4 hunters generated
- [ ] Year 1: Codex_MarrowLore and Codex_SettlementLog are already unlocked
- [ ] Hunt Thornback (Standard) → behavior cards fire; parts breakable
- [ ] Resolve 3 settlement events → 3 chronicle entries visible
- [ ] Craft Bone Cleaver (Carapace Forge) → resources deducted; item in inventory
- [ ] Equip Bone Cleaver → overlay visible on hunter token
- [ ] Carapace Helm + Chest adjacent in gear grid → +1 Toughness link bonus shown
- [ ] Year-end summary correct; year banner shows "YEAR 2"
- [ ] A veteran hunter (force-age to 7 years via debug) → retirement panel appears

### Years 6–10: Mid Campaign

- [ ] Jump to Year 6 via debug
- [ ] Hunt Bog Caller → mist zone created; hunter in mist gains Poison
- [ ] Hunt Shriek → Dive attack fires; bypasses Evasion; Fear effect applies
- [ ] Kill Gilded Serpent → Auric Scales craft set unlocked in Crafting tab
- [ ] Kill Ivory Stampede → Ivory Hall unlocked
- [ ] Kill Rotmother → Rot Garden unlocked; check Corruption if 3+ Spawn survived
- [ ] Craft Rot Spear → Blight Spear -1 Shell on hit confirmed in console
- [ ] EVT-09 fires (Foundation Stones) → Codex_FirstRuins unlocked; visible in Codex tab

### Years 11–19: Late Campaign

- [ ] Jump to Year 12
- [ ] Siltborn available in hunt selection (overlord, year 10+)
- [ ] Hunt Siltborn Phase 1 → draws from phase1Deck
- [ ] Force Siltborn to 40% HP → Phase 2 triggers; "PHASE 2" banner
- [ ] Destroy all 3 Nodes → Siltborn defeated
- [ ] CodexEntry_TheSiltborn unlocked; Mire Apothecary craft set available
- [ ] Hunt Penitent → desperation scaling with 0 deaths vs 6 deaths; verify +3 damage
- [ ] Penitent self-harms (SLF-Flagellate) → takes Flesh damage, +4 next attack
- [ ] Penitent defeated → CodexEntry_ThePenitent unlocked; Ichor Works available

### Years 20–30: Endgame

- [ ] Jump to Year 25
- [ ] Pale Stag Ascendant NOT available if 0 overlords killed (gate check)
- [ ] Simulate 1 overlord kill → Pale Stag becomes available
- [ ] Hunt Pale Stag Phase 1 → physical form on grid; antler and flank parts breakable
- [ ] Force to 30% HP → Ascendant Form triggers
- [ ] Physical token removed; grid irrelevant; AoE attacks hit all hunters
- [ ] Ascendant Form HP depletes to 0 → Victory triggered
- [ ] VictoryEpilogue loads with correct ending tier:
  - [ ] All 3 overlords killed, ≤4 deaths → TRIUMPH epilogue
  - [ ] All 3 overlords, >4 deaths → VICTORY epilogue
  - [ ] 12+ codex, not all overlords → SCHOLAR epilogue
  - [ ] Other → SURVIVOR epilogue
- [ ] CodexEntry_ThePaleStag unlocked
- [ ] overlordKillCount and totalHunterDeaths correct in stats row

---

## Part 4: Save/Load Round-Trip Verification

- [ ] Save at Year 8 (slot 1)
- [ ] Continue playing to Year 10
- [ ] Load slot 1 → returns to Year 8 state exactly
- [ ] All equipped gear still equipped
- [ ] All unlocked codex entries still unlocked
- [ ] All unlocked craft sets still unlocked
- [ ] Chronicle entries preserved

---

## Part 5: Final Bug Fix Protocol

For any bug found:

1. Note the file and line
2. Fix minimally
3. Add comment `// 9-R fix: [description]`
4. Re-run the specific test step that failed
5. Do NOT proceed to the commit until all test items are checked

---

## Full Definition of Done — Stage 9

### Data

- [ ] 10 monsters with complete behavior decks (160 total behavior cards)
- [ ] 3 overlords with two-phase decks
- [ ] 42 gear items across 7 craft sets
- [ ] 7 CraftSetSO assets with link and full-set bonuses
- [ ] 15 codex entries with lore text in settler voice
- [ ] 36 lifecycle cards (10 Injuries, 8 Scars, 8 Disorders, 10 Fighting Arts)

### Mechanics

- [ ] Phase transition (40% HP → Phase 2) working for all overlords
- [ ] Pale Stag Phase 2 transition at 30% HP
- [ ] Ascendant Form (grid-irrelevant AoE) working
- [ ] Node-based defeat condition (Siltborn) working
- [ ] Counterattack on miss (Ironhide) working
- [ ] Scale Reflection (Gilded Serpent) working
- [ ] Dive movement (Shriek) working
- [ ] Rot Spawn spawning and corruption mechanic working
- [ ] Mist Zone Poison application working
- [ ] Pack AI alpha promotion working (Ivory Stampede)
- [ ] Gear adjacency link bonuses calculating correctly
- [ ] Gear overlay sprites drawing on combat tokens
- [ ] 4-way directional sprites and facing logic working
- [ ] Animator controllers for all 8 builds working

### Campaign Flow

- [ ] Birth → naming → chronicle entry → child joins roster in Year N+5
- [ ] Retirement check each year; veteran choice with consequences
- [ ] Year-end summary → year banner → next year
- [ ] Game Over on TPK or Ironman death
- [ ] Victory on Pale Stag defeat (or Year 30 reached)
- [ ] All 4 epilogue endings reachable based on campaign outcomes
- [ ] Save/load round-trips cleanly for all campaign states

### UI

- [ ] All 7 settlement tabs functional (Hunters, Gear, Crafting, Innovations, Events, Chronicle, Codex)
- [ ] Crafting panel shows correct sets per unlock state
- [ ] Codex shows locked/unlocked cards correctly
- [ ] Chronicle scrollable, most-recent-first
- [ ] Hunter detail panel shows injuries, scars, disorders, fighting arts
- [ ] Gear grid shows adjacency link bonuses

---

## Final Commit

Once all smoke test items and DoD boxes are checked:

```
git add -A
git commit -m "9R: Stage 9 final integration — full campaign DoD, v1.0"
git tag v1.0
```

---

## What Comes Next

Marrow & Myth v1.0 represents a complete, playable 30-year campaign. After this milestone, future development may include:

- **Art Polish:** Generating all sprite assets (monster sprites, gear overlays, settlement backgrounds) using CoPlay's image generation tools
- **Audio Completion:** Any music tracks or SFX not yet generated
- **Balance Pass:** A dedicated balancing session (monster HP tuning, resource economy, event frequency)
- **Voiced Codex:** Full text-to-speech for codex entries (TTS generation via CoPlay)
- **Additional Content:** Stage 10 may cover post-campaign challenges, new monster variants, or a second settlement story arc

**You have built a game.**

Every system — the combat grid, the behavior card AI, the settlement lifecycle, the 30-year campaign arc, the overlords, the gear economy, the chronicle log — everything in this document set represents a complete design and implementation path, specified precisely enough that Claude can build it from scratch.

That is what this document set is for. You are not a new developer anymore.
