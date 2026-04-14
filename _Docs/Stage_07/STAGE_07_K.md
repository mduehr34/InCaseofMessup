<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-K | Chronicle Events EVT-01 through EVT-15
Status: Stage 7-J complete. All 8 weapon types done.
144 ActionCardSO assets verified.
Task: Create EventSO assets for the first 15 Chronicle Events
covering Years 1–12. EVT-01 must fire in Year 1. EVT-14
must fire after any permanent character death.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_K.md
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

Then confirm:
- Asset naming: Event_EVT01 through Event_EVT15
- EVT-01 has isMandatory = true and yearRangeMin/Max = 1/1
- EVT-14 has isMandatory = true and is triggered by death,
  not by year range — explain how you will handle the trigger
- What you will NOT create this session (EVT-16 through EVT-30)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-K: Chronicle Events EVT-01 through EVT-15

**Resuming from:** Stage 7-J complete  
**Done when:** All 15 EventSO assets created; EVT-01 fires automatically in Year 1 settlement; EVT-14 fires after any permanent character death  
**Commit:** `"7K: Chronicle Events EVT-01 through EVT-15 as SO assets"`  
**Next session:** STAGE_07_L.md  

---

## Save Path: `Assets/_Game/Data/Events/`

Asset naming convention: `Event_EVT01`, `Event_EVT02`, etc.

---

## Years 1–5 Events (EVT-01 through EVT-08)

| ID | Asset | Name | yearMin | yearMax | mandatory | Choices | Notes |
|---|---|---|---|---|---|---|---|
| EVT-01 | `Event_EVT01` | The First Night | 1 | 1 | true | None | Mandatory Year 1. narrativeText: "The darkness is absolute. Someone has to go first." Chronicle entry: "Year 1." |
| EVT-02 | `Event_EVT02` | The Naming | 1 | 2 | false | A: Name a child (birth new character) / B: Focus on survival (gain 2 Bone) | |
| EVT-03 | `Event_EVT03` | First Blood | 1 | 3 | false | A: Honor the fallen (chronicle entry) / B: Use the loss as fuel (all hunters +1 Grit next hunt) | Trigger: after first hunt |
| EVT-04 | `Event_EVT04` | Strange Sounds | 2 | 4 | false | A: Investigate (unlock Codex entry "Whispers Below") / B: Ignore (nothing) | |
| EVT-05 | `Event_EVT05` | The Crafter's Offer | 2 | 5 | false | A: Accept (unlock one Crafter free, no resource cost) / B: Decline (gain 3 generic resources) | |
| EVT-06 | `Event_EVT06` | A Child Is Born | 2 | 5 | true | None (mandatory if birth conditions met) | narrativeText: "Life continues despite everything." |
| EVT-07 | `Event_EVT07` | The Lost Hunter | 3 | 6 | false | A: Search (2 hunters unavailable next hunt, gain 1 Fighting Art card) / B: Mourn (-1 Accuracy all hunters 1 hunt) | |
| EVT-08 | `Event_EVT08` | Bone Wind | 3 | 6 | false | A: Take shelter (no effect) / B: Hunt through it (all hunters start next hunt Shaken) | seasonTag: "winter" |

---

## Years 6–12 Events (EVT-09 through EVT-15)

| ID | Asset | Name | yearMin | yearMax | mandatory | Choices | Notes |
|---|---|---|---|---|---|---|---|
| EVT-09 | `Event_EVT09` | Foundation Stones | 6 | 8 | true | None | Mandatory first time Tier 1 Crafter built. Dig reveals worked stone. Codex: "First Ruins." narrativeText: "These stones are too regular to be natural. Someone put them here. Long before us." |
| EVT-10 | `Event_EVT10` | The Old Tools | 7 | 10 | false | A: Reverse engineer (unlock 1 random Innovation immediately) / B: Smelt down (gain 3 Dense Bone equivalent materials) | |
| EVT-11 | `Event_EVT11` | Pack Survivor | varies | varies | false | A: Hunt it down (gain 2 Pack materials) / B: Leave it (30% chance triggers EVT-12) | `monsterTag = "The Pack"`. Fires after any Pack hunt. |
| EVT-12 | `Event_EVT12` | The Lone Wolf | varies | varies | false | A: Attempt contact (Codex unlock "Pack Shard", wolf moves on) / B: Kill it (gain 3 Pack materials) | Triggered by EVT-11 Choice B (30% chance). |
| EVT-13 | `Event_EVT13` | Marrow Seep | 8 | 12 | false | A: Seal it (costs 3 Dense Bone, no further effect) / B: Study it (unlock Codex "Marrow Exposed", trigger GP-03) | `choice.guidingPrincipalTrigger = "GP-03"` on Choice B |
| EVT-14 | `Event_EVT14` | The Grief | varies | varies | true | None | Mandatory after any permanent character death. mechanicalEffect: "One hunter chosen by player gains Disorder: Grief (-1 Accuracy for 3 hunts, auto-removes)." narrativeText: "Grief comes. It always comes." |
| EVT-15 | `Event_EVT15` | Hard Winter | 6 | 10 | false | A: Ration supplies (lose 4 generic resources) / B: Hunt early (next hunt forces specific monster) | `seasonTag = "winter"` |

---

## EVT-14 Trigger Hook

EVT-14 is triggered by permanent character death, not year range. Add this to `SettlementManager`:

```csharp
public void OnPermanentCharacterDeath(string characterId)
{
    var evt14 = System.Array.Find(
        _campaignData.eventPool, e => e.eventId == "EVT-14");
    if (evt14 == null || _campaign.resolvedEventIds.Contains("EVT-14")) return;

    // Mark as pending — Settlement screen shows it next load
    var pending = new System.Collections.Generic.List<string>(
        _campaign.resolvedEventIds);
    // Store "EVT-14-PENDING" as a special flag the Settlement screen checks
    // Simple implementation: add to activeGuidingPrincipalIds with special prefix
    Debug.Log("[Settlement] EVT-14 (The Grief) queued — character death");
}
```

> ⚑ The full mechanical effect of Grief Disorder (adding a card to a hunter's deck) requires clarification before implementing. Log it and ask the developer which hunter receives it and how the deck card is created.

---

## Verification Test

1. Start new campaign — EVT-01 fires in Year 1 settlement (isMandatory = true, yearRangeMin/Max = 1/1)
2. Acknowledge EVT-01 — resolvedEventIds contains "EVT-01", it never fires again
3. Reach Year 6–8 — EVT-09 fires (Foundation Stones); "First Ruins" appears in Codex
4. Open Inspector on Event_EVT13 — Choice B `guidingPrincipalTrigger` is set to "GP-03"
5. Confirm all 15 assets exist in `Assets/_Game/Data/Events/`

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_L.md`  
**Covers:** Chronicle Events EVT-16 through EVT-30 — the lore revelation arc and campaign endgame
