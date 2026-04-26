<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-M | Overlord: The Siltborn
Status: Stage 9-L complete. The Ironhide done.
Task: Build The Siltborn overlord monster. Overlords differ from
standard monsters: 6 breakable parts, a two-phase behavior deck
(Phase 1 and Phase 2 unlocked when HP drops below 40%), a unique
multi-node defeat condition, and campaign-level rewards on victory.
The Siltborn is a massive silted horror that emerges from deep mud.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_M.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- Overlords use MonsterSO with isOverlord=true
- Two-phase deck: phase1Deck and phase2Deck arrays on MonsterSO
  (or a single deck with cards flagged isPhase2)
- Phase transition triggers when total Flesh HP drops below 40%
- Defeat condition: all 3 "Node" parts must be destroyed (0 Flesh)
  before the Siltborn counts as defeated
- Victory unlocks CodexEntry_TheSiltborn and opens Mire Apothecary
  craft set in the settlement
- What you will NOT build (overlord sprite — separate art session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-M: Overlord — The Siltborn

**Resuming from:** Stage 9-L complete — The Ironhide done
**Done when:** Siltborn MonsterSO with two-phase deck (20 Phase 1 + 12 Phase 2 cards); multi-node defeat condition; campaign rewards fire on victory
**Commit:** `"9M: Overlord Siltborn — two-phase deck, node defeat, campaign rewards"`
**Next session:** STAGE_09_N.md

---

## Overlord System Additions

### Update MonsterSO

Add these fields to `MonsterSO.cs`:

```csharp
[Header("Overlord Settings")]
public bool isOverlord = false;

// Two-phase behavior decks
public BehaviorCardSO[] phase1Deck;
public BehaviorCardSO[] phase2Deck;

// Nodes — special parts that must ALL be destroyed to defeat the overlord
public string[] victoryNodePartNames;
// e.g. new string[] { "Left Node", "Right Node", "Core Node" }

[Header("Overlord Campaign Rewards")]
public string   rewardCodexEntryId;    // Unlocked on victory
public string   rewardCraftSetId;      // Craft set unlocked on victory
```

### Update CombatManager — Phase Transition

```csharp
private bool _inPhase2 = false;
private BehaviorCardSO[] _activeDeck;

private void CheckPhaseTransition()
{
    if (_inPhase2 || !_activeMonster.isOverlord) return;

    int totalFlesh    = 0, maxFlesh = 0;
    foreach (var part in _monsterParts)
    {
        totalFlesh += part.currentFleshHP;
        maxFlesh   += part.maxFleshHP;
    }

    if (maxFlesh > 0 && (float)totalFlesh / maxFlesh < 0.4f)
    {
        _inPhase2    = true;
        _activeDeck  = _activeMonster.phase2Deck;
        ShuffleDeck();
        Debug.Log($"[Combat] {_activeMonster.monsterName} — PHASE 2 TRIGGERED");
        // Optionally: show a visual banner
        _hud?.ShowPhaseBanner("PHASE 2");
    }
}

private bool CheckOverlordVictory()
{
    if (!_activeMonster.isOverlord) return false;
    if (_activeMonster.victoryNodePartNames == null) return false;

    foreach (var nodeName in _activeMonster.victoryNodePartNames)
    {
        var node = FindPart(nodeName);
        if (node == null || node.currentFleshHP > 0) return false;
    }
    return true;  // All nodes destroyed
}

public void OnPartDestroyed(MonsterPartState part)
{
    Debug.Log($"[Combat] Part destroyed: {part.partName}");
    if (CheckOverlordVictory())
    {
        OnOverlordDefeated();
    }
}

private void OnOverlordDefeated()
{
    Debug.Log($"[Combat] OVERLORD DEFEATED: {_activeMonster.monsterName}");

    // Chronicle entry
    GameStateManager.Instance.AddChronicleEntry(
        GameStateManager.Instance.CampaignState.currentYear,
        $"The {_activeMonster.monsterName} was brought down. The ground has been quieter since.");

    // Codex entry
    if (!string.IsNullOrEmpty(_activeMonster.rewardCodexEntryId))
        GameStateManager.Instance.UnlockCodexEntry(_activeMonster.rewardCodexEntryId);

    // Craft set (unlock in settlement)
    if (!string.IsNullOrEmpty(_activeMonster.rewardCraftSetId))
        GameStateManager.Instance.UnlockCraftSet(_activeMonster.rewardCraftSetId);

    // Overlord kill counter
    GameStateManager.Instance.CampaignState.overlordKillCount++;

    OnMonsterDefeated();
}
```

Add `UnlockCodexEntry` and `UnlockCraftSet` to `GameStateManager`:

```csharp
public void UnlockCodexEntry(string entryId)
{
    var ids = new System.Collections.Generic.List<string>(
        CampaignState.unlockedCodexEntryIds ?? new string[0]);
    if (!ids.Contains(entryId)) ids.Add(entryId);
    CampaignState.unlockedCodexEntryIds = ids.ToArray();
    Debug.Log($"[Codex] Unlocked: {entryId}");
}

public void UnlockCraftSet(string setId)
{
    var sets = new System.Collections.Generic.List<string>(
        CampaignState.unlockedCraftSetIds ?? new string[0]);
    if (!sets.Contains(setId)) sets.Add(setId);
    CampaignState.unlockedCraftSetIds = sets.ToArray();
    Debug.Log($"[CraftSet] Unlocked: {setId}");
}
```

Add `unlockedCraftSetIds` to `CampaignState`.

---

## The Siltborn — Monster Design

**Name:** The Siltborn
**Type:** Overlord — Aquatic Horror
**Tier:** Overlord (available Years 10–20, one-time fight)
**Difficulty:** Nightmare

**Lore:**  *(This is what the hunters find in the codex after the fight, not before)*
"We killed it. It took nine hunters and we came back with four. It came out of the deepest channel in the marsh and it was already dying — we think it had been dying for decades, maybe longer. The three nodes were the last parts of it that still lived. When we broke the last one, the rest of it just... settled. Into the mud. Like it had been waiting for permission."

**Victory Condition:** All three Nodes (Left Node, Right Node, Core Node) reduced to 0 Flesh HP.

**Phase 2 Trigger:** Total Flesh HP falls below 40%.

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Left Node | 5 | 8 | **Victory node** — must be destroyed |
| Right Node | 5 | 8 | **Victory node** — must be destroyed |
| Core Node | 8 | 12 | **Victory node** — must be destroyed |
| Outer Shell (Front) | 10 | 5 | Breaking exposes all nodes: −2 Shell on each node |
| Outer Shell (Rear) | 10 | 5 | Breaking: all nodes gain Exposed (attacks against nodes ignore Evasion) |
| Silt Mouth | 4 | 6 | Breaking disables Engulf and Bile Flood attacks |

---

## Phase 1 Deck (20 cards)

Create in `Assets/_Game/Data/Monsters/Siltborn/BehaviorCards/Phase1/`.

| cardId | cardName | movementEffect | attackEffect | specialEffect |
|---|---|---|---|---|
| `SLB-P1-01` | Surge | Move 1 toward aggro. | — | — |
| `SLB-P1-02` | Silt Push | Move 1. | 2 Flesh to all adjacent hunters. | Knockback 1. |
| `SLB-P1-03` | Submerge | Move to far edge of grid. | — | Untargetable until next card. |
| `SLB-P1-04` | Rise | Emerge adjacent to aggro target. | 3 Flesh to aggro target. | — |
| `SLB-P1-05` | Crush | No movement. | 4 Flesh to aggro target. | — |
| `SLB-P1-06` | Engulf | No movement. | 2 Flesh + Constrict to aggro. | Trigger: Silt Mouth intact. |
| `SLB-P1-07` | Silt Wave | No movement. | 2 Flesh to all hunters in a 2-wide arc. | Hunters hit gain Slowed. |
| `SLB-P1-08` | Node Pulse | No movement. | — | Each Node regenerates 1 Shell HP. |
| `SLB-P1-09` | Bile Spray | No movement. | 2 Flesh to aggro + Poison (3 rounds). | Trigger: Silt Mouth intact. |
| `SLB-P1-10` | Territorial | Move 1 away from lowest-Flesh hunter. | — | — |
| `SLB-P1-11` | Slam | Move 1 toward aggro. | 3 Flesh to all in path. | — |
| `SLB-P1-12` | Deep Resonance | No movement. | — | All hunters lose −1 Evasion for 1 round. |
| `SLB-P1-13` | Gravel Spit | No movement. | 1 Flesh to all hunters (ranged). | Cannot be Evaded. |
| `SLB-P1-14` | Tide Shift | Swap position with a random node (move to that node's position). | — | — |
| `SLB-P1-15` | Patience | No movement. | — | Aggro shifts to hunter with most HP. |
| `SLB-P1-16` | Shell Harden | No movement. | — | Outer Shell (Front) gains 2 Shell HP. |
| `SLB-P1-17` | Mud Slide | Move 2 toward aggro. | 1 Flesh to every hunter in path. | — |
| `SLB-P1-18` | Pressure | No movement. | — | All Poisoned hunters take 1 extra Flesh. |
| `SLB-P1-19` | Overwhelm | Move 1. | 3 Flesh to aggro, 1 Flesh to all adjacent. | — |
| `SLB-P1-20` | Submerge (isShuffle) | Move to far edge. | — | isShuffle: true |

---

## Phase 2 Deck (12 cards)

Create in `Assets/_Game/Data/Monsters/Siltborn/BehaviorCards/Phase2/`.

Phase 2 is brutal — the Siltborn's dying throes are its most dangerous state.

| cardId | cardName | movementEffect | attackEffect | specialEffect |
|---|---|---|---|---|
| `SLB-P2-01` | Desperate Surge | Move 2 toward aggro. | 4 Flesh to aggro target. | — |
| `SLB-P2-02` | Node Frenzy | No movement. | — | All 3 nodes deal 2 Flesh damage to the nearest hunter automatically. |
| `SLB-P2-03` | Bile Flood | No movement. | 3 Flesh + Venom (3 rds) to all adjacent hunters. | Trigger: Silt Mouth intact. |
| `SLB-P2-04` | Death Thrash | No movement. | 3 Flesh to all hunters within 3 spaces. | Knockback 2. |
| `SLB-P2-05` | Total Engulf | No movement. | 4 Flesh + Constrict to aggro. | Target also gains Poison (3 rounds). |
| `SLB-P2-06` | Shell Collapse | No movement. | — | All remaining Shell HP on Outer Shells is destroyed. Nodes gain Exposed. |
| `SLB-P2-07` | Mud Eruption | Move randomly 1 space. | 3 Flesh to all hunters. | Cannot be Evaded. isShuffle: true |
| `SLB-P2-08` | Final Slam | Move 1 toward aggro. | 6 Flesh to aggro target. | Cannot be Evaded. isShuffle: true |
| `SLB-P2-09` | Resonant Scream | No movement. | — | All hunters gain Fear (Shaken + −1 Grit). Hunters below half Flesh also gain Pinned. |
| `SLB-P2-10` | Node Mend (Last) | No movement. | — | Core Node regenerates 3 Flesh HP. isShuffle: true |
| `SLB-P2-11` | Relentless | Move 1 toward lowest-Flesh hunter. | 3 Flesh to that hunter. | Ignore Evasion. |
| `SLB-P2-12` | Collapse | No movement. | 2 Flesh to all. | Siltborn ends Phase 2 — isShuffle: true |

---

## Siltborn MonsterSO Asset

Create `Assets/_Game/Data/Monsters/Siltborn/Siltborn_Overlord.asset`:

```
monsterName: The Siltborn
monsterType: Overlord — Aquatic Horror
isOverlord: true
difficulty: Nightmare
huntYearMin: 10
huntYearMax: 20

Parts:
  Left Node: shellHP=5, fleshHP=8
  Right Node: shellHP=5, fleshHP=8
  Core Node: shellHP=8, fleshHP=12
  Outer Shell (Front): shellHP=10, fleshHP=5
  Outer Shell (Rear): shellHP=10, fleshHP=5
  Silt Mouth: shellHP=4, fleshHP=6

victoryNodePartNames: ["Left Node", "Right Node", "Core Node"]
phase1Deck: [SLB-P1-01 through SLB-P1-20]
phase2Deck: [SLB-P2-01 through SLB-P2-12]
rewardCodexEntryId: CodexEntry_TheSiltborn
rewardCraftSetId: Mire
startingAggroTarget: Hunter with lowest Speed
```

---

## Verification Test

- [ ] Siltborn MonsterSO with 6 parts, 20 Phase 1 cards, 12 Phase 2 cards
- [ ] victoryNodePartNames populated with all three node names
- [ ] Combat starts: Phase 1 deck active, cards drawn from phase1Deck
- [ ] Siltborn total Flesh drops below 40% → "PHASE 2 TRIGGERED" in console
- [ ] Cards now drawn from phase2Deck after transition
- [ ] Destroy Left Node and Right Node but not Core Node → NO victory yet
- [ ] Destroy Core Node → victory triggers, overlordKillCount incremented
- [ ] CodexEntry_TheSiltborn unlocked in CampaignState after victory
- [ ] Mire craft set unlocked (unlockedCraftSetIds includes "Mire")
- [ ] Chronicle entry written: "The Siltborn was brought down..."
- [ ] SLB-P1-03 (Submerge) → monster untargetable until next card
- [ ] SLB-P1-08 (Node Pulse) → all 3 nodes each gain +1 Shell HP
- [ ] SLB-P2-02 (Node Frenzy) → each living node auto-deals 2 Flesh to nearest hunter
- [ ] Silt Mouth destroyed → Engulf and Bile cards have no effect

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_N.md`
**Covers:** Overlord — The Penitent. A self-flagellating humanoid overlord that gains power from its own wounds and punishes the settlement's morality choices. Full design with two-phase deck and campaign reward.
