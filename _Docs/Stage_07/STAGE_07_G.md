<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-G | Gaunt Behavior Cards — All SO Assets
Status: Stage 7-F complete. Animation frames and AudioManager
done.
Task: Create all BehaviorCardSO assets for The Gaunt —
Standard (10), Hardened additions (3), and Apex additions (3).
These are the canonical template all other monsters follow.
Then update Mock_GauntStandard MonsterSO to reference them.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_G.md
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs
- Assets/_Game/Data/Monsters/Mock_GauntStandard.asset

Then confirm:
- Asset naming: Gaunt_[CardName] — e.g. Gaunt_ScentLock
- All removal condition strings match EXACTLY what
  PartResolver uses to look them up by name
- Stillness is Permanent type (never removed)
- Apex Predator is SingleTrigger type
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-G: Gaunt Behavior Cards — All SO Assets

**Resuming from:** Stage 7-F complete  
**Done when:** All 16 Gaunt BehaviorCardSO assets created; Mock_GauntStandard MonsterSO updated to reference Standard deck; full Gaunt fight runs correctly with real behavior cards  
**Commit:** `"7G: All Gaunt behavior card SO assets — full Gaunt fight verified"`  
**Next session:** STAGE_07_H.md  

---

## Asset Save Path

All behavior cards: `Assets/_Game/Data/Cards/Behavior/Gaunt/`

---

## Standard Deck — 9 Removable + 1 Permanent

### Opening Group (Cards 1–3)

| Asset Name | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|
| `Gaunt_CreepingAdvance` | End of round | Move 3 squares toward Aggro holder | Removable | Right Flank Shell break |
| `Gaunt_ScentLock` | Hunter plays a Loud card | Move 4 squares toward that hunter. Transfer Aggro. Apply Shaken. | Removable | Throat Shell break |
| `Gaunt_FlankSense` | Hunter moves to Rear arc | Immediately rotate to face that hunter. Transfer Aggro. | Removable | Tail Shell break |

### Escalation Group (Cards 4–7)

| Asset Name | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|
| `Gaunt_TremorRead` | Hunter spends Grit to Surge | Rotate to face that hunter. Transfer Aggro. Next standard attack gains +1 Accuracy. | Removable | Head Flesh wound 1 |
| `Gaunt_Lunge` | End of round | Move 6 squares straight toward Aggro holder. Hunters in path make Evasion check (difficulty 5) or lose next Move action. | Removable | Hind Legs Shell break |
| `Gaunt_TheHowl` | Gaunt Torso first wounded (once per fight) | All hunters apply Shaken for 2 rounds. Cannot trigger again. | Removable | Throat Shell break |
| `Gaunt_PackMemory` | End of round | Rotate to face hunter furthest from Aggro holder. Next standard attack targets that hunter instead. | Removable | Head Flesh wound 2 |

### Apex Group (Cards 8–9, enter on first part break)

| Asset Name | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|
| `Gaunt_Frenzy` | Any hunter Flesh reaches 0 | Move 4 squares toward nearest hunter. All Shaken statuses refresh duration. | Removable | Torso Flesh wound |
| `Gaunt_ThroatLock` | Hunter plays their Signature card | Apply Pinned to that hunter. Gaunt rotates to face them. Next standard attack targets Throat specifically. | Removable | Left Flank Shell break |

### Permanent (Always present)

| Asset Name | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|
| `Gaunt_Stillness` | End of every round | Rotate to face the hunter who played the most Loud cards this round. No other effect. | Permanent | Cannot be removed |

---

## Hardened Additions (3 new cards)

| Asset Name | Group | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|---|
| `Gaunt_BloodFrenzy` | Escalation | Hunter plays their Signature card | Move 4 squares toward that hunter. Transfer Aggro. | Removable | Head Flesh wound 1 (shifts TremorRead to wound 2, PackMemory to wound 3) |
| `Gaunt_SavageLunge` | Apex | End of round if Gaunt moved 6+ squares this round | Make standard attack against Aggro holder at +1 Accuracy. | Removable | Hind Legs Flesh wound |
| `Gaunt_PackInstinct` | Apex | Two or more hunters are adjacent to each other | Rotate to face the cluster. Next standard attack hits both adjacent hunters simultaneously. | Removable | Left Flank Flesh wound |

---

## Apex Additions (3 new cards)

| Asset Name | Group | Trigger | Effect | Type | Removal Condition |
|---|---|---|---|---|---|
| `Gaunt_DeathSilence` | Escalation | Hunter plays any card with AP Refund (weak cards) | Rotate to face that hunter. Transfer Aggro. Next standard attack gains +2 Accuracy. | Removable | Throat Flesh wound 1 |
| `Gaunt_MarrowHunger` | Apex | End of round if 3+ parts are broken | Gaunt gains +2 Movement and +1 Accuracy until end of next round. Triggers once per hunt. | Removable | Torso Flesh wound 2 |
| `Gaunt_ApexPredator` | Single Trigger | Start of Round 5 (automatic) | All hunters lose 1 Grit. All Reaction cards in all hunters' hands are discarded without effect. | SingleTrigger | Cannot be removed early — fires at Round 5 |

---

## Updating Mock_GauntStandard MonsterSO

After creating all card assets, open `Mock_GauntStandard.asset` and populate:

**openingCards (3):** Gaunt_CreepingAdvance, Gaunt_ScentLock, Gaunt_FlankSense  
**escalationCards (4):** Gaunt_TremorRead, Gaunt_Lunge, Gaunt_TheHowl, Gaunt_PackMemory  
**apexCards (2):** Gaunt_Frenzy, Gaunt_ThroatLock  
**permanentCards (1):** Gaunt_Stillness  

---

## Verification Test

Play the combat scene with Mock_GauntStandard:

- [ ] Vitality Phase: Aldric draws cards — Debug.Log shows hand
- [ ] Monster Phase Round 1: Draws from Opening group — one of the first 3 cards
- [ ] Scent Lock triggers when Aldric plays a Loud card
- [ ] Stillness appears in permanent list but does NOT count toward win condition
- [ ] After 9 removable cards removed: Debug.Log "MONSTER DEFEATED" fires immediately
- [ ] Apex Predator logs at start of Round 5 if deck is in Apex mode

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_H.md`  
**Covers:** All weapon Action Card SO assets — Fist Weapons all 5 tiers (priority: needed for Tutorial)
