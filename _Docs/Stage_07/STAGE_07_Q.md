<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-Q | Final Balance Pass & Stage 7 Definition of Done
Status: Stage 7-P complete. Tutorial 3-year playthrough
verified. Standard Campaign starts to Year 5 without errors.
All art sprites imported with Point filtering verified.
Task: Run the GDD A.13 balance scenarios. Verify audio
context switching. Complete the Stage 7 Definition of Done
checklist. Document what remains for future sessions.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_Q.md
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs
- Assets/_Game/Data/Monsters/Monster_Gaunt.asset

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-Q: Final Balance Pass & Stage 7 Definition of Done

**Resuming from:** Stage 7-P complete  
**Done when:** All GDD A.13 balance scenarios pass within expected ranges; AudioManager verified; Stage 7 Definition of Done fully checked off  
**Commit:** `"7Q: Balance pass complete — Stage 7 and full game complete"`  

---

## GDD A.13 Balance Scenarios

Run each scenario **3 times** and record average round count and collapse count. If averages fall outside the expected range by more than 2 rounds, review behavior card composition and stat blocks.

### Scenario 1: Gaunt Standard — 4 Bare Fist Hunters

```
Setup:
- 4 hunters: Aethel, Eira, Beorn, Freya (all bare fists, Tier 1)
- Gaunt Standard stat block
- No gear, no innovations, starting Grit: 3
- Aldric holds Aggro

Expected: ~8 rounds, 1 hunter collapse likely, 2 possible
```

| Run | Rounds | Collapses | Pass? |
|---|---|---|---|
| 1 | | | |
| 2 | | | |
| 3 | | | |
| **Average** | | | |

**Apex Predator check (Gaunt Apex only):** Fires at Round 5. All Reaction cards discarded. All hunters lose 1 Grit. Confirm this fires correctly:
- [ ] Apex Predator is SingleTrigger type
- [ ] Fires at start of Round 5 (check MonsterAI trigger: "Start of Round 5")
- [ ] All Reaction cards in all hunter hands discarded
- [ ] Grit decremented by 1 for each hunter

### Scenario 2: Gaunt Hardened — Partial Tier 1 Gear

```
Setup:
- 4 hunters with mix of Gaunt Skull Cap and Gaunt Hide Vest
- Gaunt Hardened stat block
- Tier 2 weapon proficiency on 2 hunters

Expected: ~10 rounds, 2 collapses likely
```

| Run | Rounds | Collapses | Pass? |
|---|---|---|---|
| 1 | | | |
| 2 | | | |
| 3 | | | |

### Scenario 3: Gaunt Apex — Full Tier 1 Set, Tier 3–4 Proficiency

```
Setup:
- 4 hunters with full Gaunt Hunter's Set (5 piece)
- Gaunt Apex stat block
- Tier 3–4 weapon proficiency on all hunters

Expected: ~12 rounds, 2–3 collapses
Note: Apex Predator fires Round 5 removing all Reactions — plan for it
```

| Run | Rounds | Collapses | Pass? |
|---|---|---|---|
| 1 | | | |
| 2 | | | |
| 3 | | | |

---

## Tuning Guidance

If fights are running significantly short (< 6 rounds Standard):
- Increase Gaunt Shell durability by 1 per part
- Or reduce hunter attack frequency (reduce hand size to 1 for early tiers)

If fights are running too long (> 12 rounds Standard):
- Increase Gaunt Accuracy or Strength by 1
- Or add 1 more Escalation behavior card to speed up pressure

Do NOT change the win condition or the behavior card group structure — those are architectural.

---

## AudioManager Verification

```
Context switch tests:
[ ] Load Settlement → AudioManager.SetContextForYear(1) fires → SettlementEarly music
[ ] Load CombatScene → SetMusicContext(CombatStandard) → combat music starts
[ ] Hunter collapses → PlayDeathSting() → music cuts 2s → fades back in
[ ] Monster defeated → PlaySFX("MonsterDefeated") → no error
[ ] Year 12+ Settlement → SetContextForYear(13) → SettlementLate music
[ ] Audio context crossfades smoothly (no hard cuts except death sting)
```

---

## Stage 7 Definition of Done — Final Checklist

Work through this entire list. Every box must be checked before Stage 7 is considered complete.

**Content:**
- [ ] All 6 monster SOs complete (Suture stat blocks only — behavior deck pending design)
- [ ] All 8 weapon types — 18 cards each = 144 ActionCardSO assets total
- [ ] All 16 Gaunt behavior card SO assets (Standard 10, Hardened +3, Apex +3)
- [ ] All 30 Chronicle Events as EventSO assets; EVT-01 mandatory Year 1; EVT-14 fires on death
- [ ] All 20 Innovations with correct cascade tree; INN-01 cascades to INN-07 and INN-11
- [ ] All 5 Guiding Principals; GP-01 fires Year 1; GP-03 triggered by EVT-13 choice B
- [ ] Gaunt Boneworks craft set: 5 armor, 4 weapons, 2 accessories, 1 consumable
- [ ] Tutorial Campaign SO and Standard Campaign SO fully populated

**Art:**
- [ ] All 8 character idle sprites imported — Point (No Filter) applied to all
- [ ] All 8 standard monster sprites + 4 overlord sprites imported — Point (No Filter) applied to all
- [ ] UI elements imported: stone panel texture, card frame, buttons, aggro token
- [ ] All 5 settlement structure sprites imported — Boneworks appears in scene when built
- [ ] Aldric animation frames imported and sliced: Idle ×2, Walk ×4, Attack ×3, Collapse ×2

**Audio:**
- [ ] AudioManager compiles; context switches log correctly
- [ ] Music crossfades on context change (no hard cuts)
- [ ] Death sting: 2s silence then fade-in
- [ ] SFX: shell hit, flesh hit, miss, card play, part break, collapse, monster defeated

**Gameplay:**
- [ ] Tutorial Campaign: 3-year playthrough completes in 45–60 minutes
- [ ] Standard Campaign: starts and reaches Year 5 without Console errors
- [ ] GDD A.13 Gaunt Standard balance: ~8 rounds, 1–2 collapses
- [ ] GDD A.13 Gaunt Hardened balance: ~10 rounds, ~2 collapses
- [ ] No uGUI components in any scene

---

## What Remains After Stage 7 (Design Sessions Required)

These items are intentionally deferred — they require design decisions before implementation can begin:

| Item | Status | Next Step |
|---|---|---|
| The Suture behavior deck | Stat blocks created, deck empty | Design session with developer |
| Remaining Crafter buildings (Herbalist, Forge, Tannery, Armory) | spriteAssetPath not yet set | Design item lists per crafter |
| Thornback/Pack/Serpent/Stag craft item sets | Not yet created | Design items following Gaunt template |
| Character animation frames (7 remaining builds) | Aldric done; 7 more needed | Follow Stage 7-F template per build |
| SFX audio clips | AudioManager wired, clips unassigned | Source or commission audio |
| Settings screen | Controller remapping, volume sliders | Post-MVP |
| Online co-op networking | JSON state is ready | Post-MVP |
| Hard / Easy campaign difficulty variants | Medium done | Post-MVP |

---

## Final Commit

```
"Stage 7 complete — full game verified: Tutorial and Standard Campaign, 
all Gaunt content, art pipeline, AudioManager, balance pass passed"
```

Congratulations. Marrow & Myth is playable.
