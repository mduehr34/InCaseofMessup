<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-M | All 20 Innovations & All 5 Guiding Principals
Status: Stage 7-L complete. All 30 Chronicle Events done.
Task: Create all 20 InnovationSO assets with the correct
cascade unlock tree. Create all 5 GuidingPrincipalSO assets.
Update Tutorial and Standard CampaignSO assets with correct
starting innovation sets and guiding principals.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_M.md
- Assets/_Game/Scripts/Core.Data/InnovationSO.cs
- Assets/_Game/Scripts/Core.Data/GuidingPrincipalSO.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-M: All 20 Innovations & All 5 Guiding Principals

**Resuming from:** Stage 7-L complete  
**Done when:** All 20 InnovationSO assets created with correct cascade tree; all 5 GuidingPrincipalSO assets created; INN-01 adoption cascades to INN-07 and INN-11 correctly in-game  
**Commit:** `"7M: All 20 Innovations and 5 Guiding Principals — cascade tree verified"`  
**Next session:** STAGE_07_N.md  

---

## Save Paths

- `Assets/_Game/Data/Innovations/`
- `Assets/_Game/Data/GuidingPrincipals/`

---

## All 20 InnovationSO Assets

| ID | Name | Effect | Grit Skill | Cascades To |
|---|---|---|---|---|
| INN-01 | Desperate Sprint | Spend 1 Grit: move 3 additional squares per turn | Surge | INN-07, INN-11 |
| INN-02 | Measured Hand | Spend 1 Grit: re-roll one die on any check | Steady | INN-08, INN-12 |
| INN-03 | Shoulder to Shoulder | Spend 2 Grit: adjacent hunter draws 1 card out of turn | Rally | INN-09, INN-13 |
| INN-04 | Iron Will | Spend 2 Grit: ignore one drawn Injury Card effect | Endure | INN-10, INN-14 |
| INN-05 | Bone Reading | After each successful hunt, examine 2 loot table results and choose 1 to keep | None | INN-15 |
| INN-06 | Keen Eye | Trap Zones are revealed at combat start — no longer hidden | None | INN-16 |
| INN-07 | Pursuit Tactics | Hunters moving with Surge do not trigger facing behaviors that round | None | INN-17 |
| INN-08 | Calculated Risk | When spending Grit on Steady, re-roll both dice instead of one | None | INN-18 |
| INN-09 | Unified Front | Rally costs 1 Grit instead of 2 when recipient has Aggro token | None | INN-19 |
| INN-10 | Scarred but Standing | Endure keeps the ignored Injury Card out of deck permanently | None | INN-20 |
| INN-11 | Survival Instinct | Hunters gain 1 Grit whenever they survive a hit that would have caused Flesh damage | None | None |
| INN-12 | The Patient Hunter | Once per hunt, a hunter may hold 1 card from hand to next turn instead of discarding | None | None |
| INN-13 | Blood Pact | When any hunter collapses, all adjacent hunters gain 1 Grit | None | None |
| INN-14 | Scar Tissue | Injury Cards no longer trigger immediately — hunter may choose to discard them once per draw | None | None |
| INN-15 | Marrow Sense | Hunters detect Marrow-Sink tiles before entering them — revealed at combat start | None | None |
| INN-16 | Weak Points | All hunters gain +1 Accuracy on first attack against any newly revealed or broken monster part | None | None |
| INN-17 | Pack Mentality | When 2+ hunters adjacent to same part, all attacks against it gain +1 Strength | None | None |
| INN-18 | Controlled Breathing | Once per hunter per hunt, that hunter may add +1 to any single die roll for free | None | None |
| INN-19 | Battle Rhythm | When a hunter plays 2 cards in succession from same category, draw 1 extra card | None | None |
| INN-20 | The Final Push | Once per combat: when a hunter collapses, all surviving hunters immediately gain 2 Grit and 2 AP | None | None |

**Cascade tree — addsToDeck field must reference these exact assets:**

```
INN-01.addsToDeck = [ INN-07, INN-11 ]
INN-02.addsToDeck = [ INN-08, INN-12 ]
INN-03.addsToDeck = [ INN-09, INN-13 ]
INN-04.addsToDeck = [ INN-10, INN-14 ]
INN-05.addsToDeck = [ INN-15 ]
INN-06.addsToDeck = [ INN-16 ]
INN-07.addsToDeck = [ INN-17 ]
INN-08.addsToDeck = [ INN-18 ]
INN-09.addsToDeck = [ INN-19 ]
INN-10.addsToDeck = [ INN-20 ]
INN-11 through INN-20: addsToDeck = empty
```

---

## All 5 GuidingPrincipalSO Assets

| ID | Name | Trigger | Choice A | Choice B |
|---|---|---|---|---|
| GP-01 | Life or Strength | Year 1, automatic | Life — settlement grows cautiously: +2 starting characters next campaign. | Strength — hunters are forged harder: all hunters start with +1 Strength. |
| GP-02 | Blood Price | After first permanent character death | Honor — build a memorial: unlock Artifact "The First Stone," chronicle entry. | Drive — channel grief: all remaining hunters gain +1 Accuracy permanently. |
| GP-03 | Marrow Knowledge | EVT-13 Study outcome | Study Deeper — gain 3 Innovations immediately from pool. | Seal the Knowledge — destroy 2 Innovations from pool, gain immunity to Marrow-Sink tile effects. |
| GP-04 | Legacy or Forgetting | Any character retirement | Legacy — retired hunter's name carved into settlement: +1 Grit starting value for all new characters. | Forgetting — settlement moves forward: unlock 2 random Innovations immediately. |
| GP-05 | The Suture | Year 26+, approaching end | Stand Firm — all hunters start Year 30 with maximum Grit. | Prepare — unlock all remaining locked Innovations in the pool immediately. |

---

## Update CampaignSO Assets

**Tutorial Campaign SO:**

```
startingInnovations: [ INN-01, INN-02, INN-03 ]    (3 for tutorial clarity)
guidingPrincipals:   [ GP-01 ]                       (GP-01 only)
```

**Standard Campaign SO:**

```
startingInnovations: [ INN-01, INN-02, INN-03, INN-04, INN-05, INN-06 ]    (6 base)
guidingPrincipals:   [ GP-01, GP-02, GP-03, GP-04, GP-05 ]                  (all 5)
```

---

## Verification Test

1. Start Tutorial Campaign. Innovations tab shows 3 options (INN-01, 02, 03)
2. Adopt INN-01 — pool now has INN-07 and INN-11 available (cascade)
3. Adopt INN-07 — pool now has INN-17 available (second cascade)
4. Trigger GP-01 in Year 1 settlement — Guiding Principal modal fires
5. Make choice A or B — choice recorded in resolvedGuidingPrincipalIds
6. GP-01 cannot fire again after resolution

---

## Next Session: STAGE_07_N.md
**Covers:** All 6 monster SO assets (complete data — Gaunt full, Thornback, Pack, Serpent, Pale Stag, Suture)

---
---

<!-- ============================================================
     STAGE 7-N
     ============================================================ -->

<!-- SESSION PROMPT
▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-N | All Monster SO Assets — Complete Data
Status: Stage 7-M complete. Innovations and GPs verified.
Task: Complete all 6 MonsterSO assets with full data:
stat blocks (all 3 difficulties), body parts, behavior card
references, facing tables, loot tables. The Gaunt already
has mock data — fill in the remaining fields. Build the
other 5 monsters. The Suture needs design session first.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_N.md
- Assets/_Game/Data/Monsters/Mock_GauntStandard.asset
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Data/Cards/Behavior/Gaunt/ (all Gaunt cards)

⚑ STOP AND ASK before creating The Suture's behavior deck.
  The GDD does not fully define The Suture's cards.
  Do not guess. Ask the developer to design it first.

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ -->

