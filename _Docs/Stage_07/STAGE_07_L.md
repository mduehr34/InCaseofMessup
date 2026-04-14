<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-L | Chronicle Events EVT-16 through EVT-30
Status: Stage 7-K complete. EVT-01 through EVT-15 verified.
EVT-01 fires Year 1. EVT-14 hooks implemented.
Task: Create EventSO assets for the remaining 15 events
covering Years 13–30. These carry the major lore arc.
All narrative text must be in settler voice — not external.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_L.md
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Data/Events/Event_EVT01.asset
  (use as reference for correct field population)

Then confirm:
- EVT-21 must gate-lock the Pale Stag — stop and ask
  how to implement this gate if unclear
- EVT-30 requires checking win/loss state — explain
  your approach before implementing
- All narrative text is in settler voice, not omniscient

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-L: Chronicle Events EVT-16 through EVT-30

**Resuming from:** Stage 7-K complete  
**Done when:** All 15 EventSO assets created; EVT-21 unlocks Pale Stag from hunt roster; EVT-30 shows correct epilogue text based on Suture fight outcome  
**Commit:** `"7L: Chronicle Events EVT-16 through EVT-30 — full event pool complete"`  
**Next session:** STAGE_07_M.md  

---

## Save Path: `Assets/_Game/Data/Events/`

---

## Years 13–20 Events (EVT-16 through EVT-22)

| ID | Asset | Name | yearMin | yearMax | mandatory | Key Content |
|---|---|---|---|---|---|---|
| EVT-16 | `Event_EVT16` | The Deep Chamber | 14 | 17 | true | First Tier 3 Crafter built triggers. Chamber contains intact mechanism. Codex: "The Old Works." narrativeText: "We thought we were digging foundations. We were not." |
| EVT-17 | `Event_EVT17` | Veteran's Rest | varies | varies | false | Retirement event. Choice A: Hopeful — new character +1 Evasion. Choice B: Warning — new character gets unique Fighting Art card. |
| EVT-18 | `Event_EVT18` | Serpent Cult | varies | varies | false | After first Gilded Serpent hunt. `monsterTag = "The Gilded Serpent"`. Choice A: Dismiss. Choice B: Investigate — unlock Artifact "Serpent Idol." `choice.artifactUnlockId = "Artifact_SerpentIdol"` |
| EVT-19 | `Event_EVT19` | The Broken Mind | 13 | 18 | false | Trigger: harsh events accumulated. Choice A: Intervene (hunter skips hunt, Disorder removed). Choice B: Watch and wait (Disorder remains, risk of worsening). |
| EVT-20 | `Event_EVT20` | Harvest Festival | 14 | 19 | false | Trigger: 3+ Crafters active. Choice A: Join (+1 Grit next hunt all hunters). Choice B: Work (craft 1 item free this phase). |
| EVT-21 | `Event_EVT21` | Pale Stag Sighting | 12 | 16 | true | Mandatory. Unlocks Pale Stag as hunt target. Codex: "The Crowned Beast." `choice.codexEntryId = "CodexEntry_PaleStag"`. narrativeText: "At the forest's edge, standing still. Its antlers catch the moonlight and hold it. Something in us knows not to move." |
| EVT-22 | `Event_EVT22` | The Record | 15 | 20 | false | Choice A: Encourage (Chronicle draws 4 instead of 1, choose 1). Choice B: Forbid (settler +1 Accuracy). |

---

## Years 21–30 Events (EVT-23 through EVT-30)

| ID | Asset | Name | yearMin | yearMax | mandatory | Key Content |
|---|---|---|---|---|---|---|
| EVT-23 | `Event_EVT23` | The Full Picture | 22 | 25 | true | After Thornback Patriarch killed. `monsterTag = "Thornback Patriarch"`. Major lore: settlers understand ruins were built deliberately. narrativeText: "We built on their bones. We always knew. I think we just didn't want to." |
| EVT-24 | `Event_EVT24` | The Last Birth | 23 | 27 | true | Mandatory if birth conditions met. Player names child. narrativeText: "What kind of world are we making this for?" |
| EVT-25 | `Event_EVT25` | The Tremors Begin | 25 | 28 | true | Ground shakes — The Suture stirs. Codex: "The Suture Stirs." All hunters gain 1-time bonus Grit next hunt. narrativeText: "Not an earthquake. Something else. Something old." |
| EVT-26 | `Event_EVT26` | Veteran's Wisdom | 22 | 28 | false | Trigger: 3+ retired characters. Choice A: Heed counsel (choose next Chronicle draw from 2 options). Choice B: Dismiss (+1 Accuracy all hunters next hunt). |
| EVT-27 | `Event_EVT27` | The Last Craft | 26 | 29 | false | Craft 1 item of choice for free — any unlocked recipe, no resource cost. narrativeText: "Let me make one more thing. Something that will outlast me." |
| EVT-28 | `Event_EVT28` | We Built This | 28 | 30 | true | Mandatory. Summary screen: all Crafters, innovations, deaths/retirements. No choices. narrativeText: "This was ours. Whatever comes next — this was ours." |
| EVT-29 | `Event_EVT29` | The Night Before | 29 | 29 | true | Mandatory Year 29. No choices. All hunters gain full Grit refill at start of Year 30 hunt. narrativeText: "One more night. One more fire. We don't speak much. We don't need to." |
| EVT-30 | `Event_EVT30` | If We Fall | 30 | 30 | true | Epilogue — two outcomes as choices. Choice A (victory): "It is over. The Suture is dead. The Marrow is quiet. For now." Choice B (defeat): "The settlement is silent. The Suture passes through. Perhaps it will be quiet now that there is nothing left to find." |

---

## EVT-21 Gate Logic

After EVT-21 resolves, the Pale Stag must be added to the hunt roster. Add to `SettlementManager.ResolveEvent()`:

```csharp
// After resolving any event, check for Pale Stag unlock
if (evt.eventId == "EVT-21")
{
    // The Pale Stag MonsterSO is in the campaign's available pool
    // but not in monsterRoster until this event fires.
    // Simplest approach: store a flag in CampaignState
    var unlocked = new System.Collections.Generic.List<string>(
        _campaign.unlockedCodexEntryIds) { "PaleStag_Unlocked" };
    _campaign.unlockedCodexEntryIds = unlocked.ToArray();
    Debug.Log("[Settlement] EVT-21 resolved — Pale Stag added to hunt roster");
}
```

Then in `HuntSelectionModal.BuildMonsterList()`, check for this flag before showing the Pale Stag.

---

## EVT-30 Win/Loss Logic

EVT-30 uses its two choices to represent the win/loss epilogue — no mechanical effect, just narrative. In `SettlementScreenController`, when Year 30 post-Suture hunt settlement loads:

```csharp
// Show EVT-30 with the correct choice pre-selected based on hunt outcome
var evt30 = FindEvent("EVT-30");
if (evt30 != null && GameStateManager.Instance.LastHuntResult.monsterName == "The Suture")
{
    bool isVictory = GameStateManager.Instance.LastHuntResult.isVictory;
    // Force-show the correct narrative by pre-selecting choice 0 (victory) or 1 (defeat)
    ShowEventModal(evt30);
    // The modal will show both choices — player selects to acknowledge
}
```

---

## Settler Voice Guide

All narrative text is written as a settler's personal record — sparse, uncertain, haunted.

✓ "These stones are too regular to be natural. Someone put them here. Long before us."  
✓ "We've been ringing a dinner bell for thirty years."  
✗ "The ruins reveal that an ancient civilization constructed this location intentionally."

Short sentences. Plain words. The settlers don't have the full picture — their language reflects that.

---

## Verification Test

- [ ] EVT-21 fires in Year 12–16 range; after resolution "PaleStag_Unlocked" flag is set
- [ ] Pale Stag does NOT appear in hunt selection before EVT-21
- [ ] EVT-25 fires in Year 25–28; "The Suture Stirs" appears in Codex
- [ ] EVT-28 fires mandatorily in Year 28–30 with no choices
- [ ] EVT-30 shows both outcome choices as acknowledgement options
- [ ] All 30 event assets exist in `Assets/_Game/Data/Events/`

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_M.md`  
**Covers:** All 20 InnovationSO assets with cascade tree + all 5 GuidingPrincipalSO assets
