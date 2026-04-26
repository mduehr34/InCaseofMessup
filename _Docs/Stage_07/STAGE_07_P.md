<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-P | Campaign SO Assets & Tutorial Playthrough
Status: Stage 7-O complete. Gaunt craft set done. Boneworks
CrafterSO populated. Item equip and link resolver verified.
Task: Fully populate Tutorial Campaign SO and Standard
Campaign SO with all correct references. Then run a complete
3-year Tutorial Campaign playthrough to verify the full game.
Target: 45–60 minutes for the tutorial.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_P.md
- Assets/_Game/Scripts/Core.Data/CampaignSO.cs
- All completed SO assets from previous Stage 7 sessions

Then confirm:
- Tutorial Campaign contains only Gaunt and Tutorial events
- Standard Campaign does NOT include The Suture in
  monsterRoster (behavior deck not yet designed)
- What you will NOT do this session (balance testing — 7-Q)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-P: Campaign SO Assets & Tutorial Playthrough

**Resuming from:** Stage 7-O complete  
**Done when:** Tutorial Campaign SO fully populated; complete 3-year Tutorial playthrough verified; Standard Campaign SO populated and starts without errors  
**Commit:** `"7P: Tutorial and Standard Campaign SOs populated — Tutorial 3-year playthrough verified"`  
**Next session:** STAGE_07_Q.md  

---

## Tutorial Campaign SO — Complete Population

**Asset:** `Assets/_Game/Data/Campaigns/Campaign_Tutorial.asset`

```
campaignName:           Tutorial Campaign
difficulty:             Medium
campaignLengthYears:    3
startingCharacterCount: 8
baseMovement:           3
startingGrit:           3
ironmanMode:            false
retirementHuntCount:    10

monsterRoster:          [ Monster_Gaunt ]
eventPool:              [ Event_EVT01, Event_EVT02, Event_EVT03,
                          Event_EVT05, Event_EVT06 ]
startingInnovations:    [ INN-01, INN-02, INN-03 ]
crafterPool:            [ Crafter_Boneworks ]
guidingPrincipals:      [ GP-01 ]
overlordMonster:        null
overlordApproachYears:  empty

birthConditionAge:      0
```

---

## Standard Campaign SO — Complete Population

**Asset:** `Assets/_Game/Data/Campaigns/Campaign_Standard.asset`

```
campaignName:           The Standard Campaign
difficulty:             Medium
campaignLengthYears:    30
startingCharacterCount: 8
baseMovement:           3
startingGrit:           3
ironmanMode:            false
retirementHuntCount:    10

monsterRoster:          [ Monster_Gaunt, Monster_Thornback,
                          Monster_TheIvoryStampede, Monster_GildedSerpent,
                          Monster_TheSpite ]
                        ← Monster_TheSpite gate-locked behind EVT-21
                        ← Monster_Suture NOT included until
                          behavior deck is designed

eventPool:              [ all 30 Event assets — EVT-01 through EVT-30 ]
startingInnovations:    [ INN-01, INN-02, INN-03, INN-04, INN-05, INN-06 ]
crafterPool:            [ Crafter_Boneworks ]
guidingPrincipals:      [ GP-01, GP-02, GP-03, GP-04, GP-05 ]

overlordSchedule:
  OVR-01 (The Siltborn, Year 5):           approachYears: [ 3, 4 ]
  OVR-02 (The Penitent, Year 15):          approachYears: [ 12, 14 ]
  OVR-03 (Pale Stag Ascendant, Year 25):   approachYears: [ 22, 24 ]
  OVR-04 (The Suture, Year 30):            approachYears: [ 25, 27, 29 ]
                        ← CampaignSO will need OverlordScheduleEntry[]
                          struct to replace flat overlordApproachYears array
birthConditionAge:      0
```

---

## Tutorial Playthrough Checklist (Target: 45–60 minutes)

Work through this as a manual play session. Check each box as you go.

**Year 1 — Settlement:**
- [ ] 8 characters generated with GDD name pool, 4M/4F split
- [ ] EVT-01 (The First Night) fires on first settlement load — mandatory
- [ ] EVT-01 acknowledged — disappears, never fires again
- [ ] GP-01 (Life or Strength) fires during Year 1 — Guiding Principal modal shows
- [ ] GP-01 choice A or B made — recorded in resolvedGuidingPrincipalIds
- [ ] Innovations tab shows 3 options (INN-01, INN-02, INN-03)
- [ ] Adopt INN-01 — cascade: INN-07 and INN-11 added to pool
- [ ] Boneworks unlocked (auto, no cost in Tutorial)

**Year 1 — Hunt:**
- [ ] SEND HUNTING PARTY → modal opens
- [ ] Monster list shows only The Gaunt
- [ ] Select 4 hunters → HUNT
- [ ] Travel scene loads — "Hunting: The Gaunt (Standard)"
- [ ] Travel events: 0–3 events fire if any tagged "travel" in event pool
- [ ] CONTINUE TO HUNT button appears after events
- [ ] CombatScene loads

**Year 1 — Combat:**
- [ ] Phase label: "VITALITY PHASE"
- [ ] All 4 hunters have Brace and Shove in hand
- [ ] Gaunt behavior cards fire correctly (Opening group: Creeping Advance, Scent Lock, Flank Sense)
- [ ] Scent Lock triggers when Loud card is played
- [ ] Stillness is in deck but does NOT count toward win condition
- [ ] Combat ends after 7–9 rounds (expected range for bare fists Standard)
- [ ] Result modal shows victory/defeat

**Year 1 — Return:**
- [ ] Return to Settlement — loot applied (GauntFang ×2, Bone ×2, Sinew ×1)
- [ ] Hunter hunt counts incremented (+1 each)
- [ ] Era header still shows Year 1

**Year 2 — Crafting:**
- [ ] Craft Gaunt Skull Cap for one hunter
- [ ] Open Gear Grid — Skull Cap visible
- [ ] Equip Skull Cap — stats summary shows +1 Evasion

**Year 2 — Hunt:**
- [ ] Second Gaunt hunt with one hunter wearing Skull Cap
- [ ] That hunter's Evasion stat is +1 vs Year 1 baseline
- [ ] One Gaunt behavior card removed via part break — deck shrinks

**Year 3 — Innovation and End:**
- [ ] Draw Innovations — pool may now include INN-07 or INN-11 (from cascade)
- [ ] Adopt one
- [ ] Year 3 hunt completes
- [ ] END YEAR → Year 4 advance... but campaign ends at Year 3
- [ ] Tutorial completion — return to main menu or show summary

---

## Standard Campaign — Smoke Test (to Year 5)

Start a Standard Campaign and advance to Year 5 without errors:

- [ ] Year 1–2: Same as Tutorial
- [ ] Year 3+: Thornback becomes available in hunt selection
- [ ] Year 5: The Ivory Stampede becomes available
- [ ] No null reference errors in Console through Year 5
- [ ] Chronicle log accumulates entries correctly

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_Q.md`  
**Covers:** Balance pass — GDD A.13 scenarios, Gaunt fight length verification, Stage 7 final Definition of Done
