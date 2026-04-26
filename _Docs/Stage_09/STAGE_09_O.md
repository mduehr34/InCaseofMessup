<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-O | Overlord: The Pale Stag Ascendant
Status: Stage 9-N complete. The Penitent overlord done.
Task: Build The Pale Stag Ascendant — the final overlord and the
campaign's ultimate hunt. It can only be hunted after Years 25–30
AND after at least one other overlord has been killed. Phase 1 is
the Pale Stag in its physical form. Phase 2 (Ascendant Form) is
triggered at 30% HP — the creature sheds its physical body and
the combat rules change dramatically.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_O.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- Phase 2 trigger is 30% HP (not 40% like other overlords)
- Ascendant Form: the physical token disappears; the creature
  becomes an "energy presence" — all attacks are Area of Effect
  (target all hunters) and the grid becomes irrelevant
- Killing the Pale Stag triggers the Victory state (CheckVictory)
- Unlocks CodexEntry_ThePaleStag and no craft set (it is the
  final reward — the campaign ends)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-O: Overlord — The Pale Stag Ascendant

**Resuming from:** Stage 9-N complete — The Penitent overlord done
**Done when:** Pale Stag MonsterSO with two-phase deck; Phase 2 "Ascendant Form" changes combat rules; killing the Pale Stag triggers Victory
**Commit:** `"9O: Overlord Pale Stag — ascendant form, AoE phase 2, victory trigger"`
**Next session:** STAGE_09_P.md

---

## The Pale Stag Ascendant — Monster Design

**Name:** The Pale Stag Ascendant
**Type:** Overlord — Final Hunt
**Tier:** Overlord (available Years 25–30; requires 1+ other overlord killed)
**Difficulty:** Nightmare

**Lore:** *(The codex entry that unlocks on victory)*
"The Pale Stag did not fight the way animals fight. It fought the way ideas fight — you can't pin it, can't outmanoeuvre it, can't predict it from last time because there is no last time. The hunters who came back described the second phase as 'it stopped pretending to be a deer.' One of them has not spoken much since. We think they saw something the others did not. We have not asked."

**Victory Condition:** Reduce the Stag to 0 Flesh in Phase 2 (the Ascendant Form). Note: you cannot kill it in Phase 1 — reducing Phase 1 HP to 0 triggers Phase 2 instead of victory.

**Phase 2 Trigger:** 30% of total Phase 1 HP. When triggered:
- All current status effects on all hunters are cleared (reset)
- The physical Stag token is removed from the grid
- Combat enters "Ascendant Form" — grid positions become irrelevant
- A new HP pool is assigned (Ascendant Form HP = 20)
- All Phase 2 attacks target ALL hunters simultaneously

---

## Ascendant Form Implementation

In `MonsterSO`, add:

```csharp
[Header("Final Overlord — Ascendant Form")]
public bool hasAscendantForm = false;
public int  ascendantFormHP  = 20;    // Separate HP pool for Phase 2
```

In `CombatManager`, override phase transition for Pale Stag:

```csharp
protected override void OnPhase2Triggered()
{
    if (_activeMonster.monsterName != "The Pale Stag Ascendant")
    {
        base.OnPhase2Triggered();
        return;
    }

    Debug.Log("[Combat] Pale Stag — ASCENDANT FORM. Physical token removed.");

    // Clear all hunter status effects
    foreach (var h in _activeCombatHunters)
        _statusSystem.ClearAll(h.hunterId);

    // Remove physical token from grid
    _monsterToken?.SetActive(false);

    // Set new HP pool
    _ascendantCurrentHP = _activeMonster.ascendantFormHP;
    _isAscendantForm    = true;

    // Show phase banner
    _hud?.ShowPhaseBanner("ASCENDANT FORM");
    GameStateManager.Instance.AddChronicleEntry(
        GameStateManager.Instance.CampaignState.currentYear,
        "The Pale Stag shed its form. What stood in the clearing was no longer an animal.");

    // Switch to phase 2 deck
    _activeDeck = _activeMonster.phase2Deck;
    ShuffleDeck();
}

// Override damage routing in Ascendant Form
public void DamageAscendantForm(int damage)
{
    if (!_isAscendantForm) return;
    _ascendantCurrentHP -= damage;
    Debug.Log($"[Combat] Ascendant Form HP: {_ascendantCurrentHP}");

    if (_ascendantCurrentHP <= 0)
        OnOverlordDefeated();
}
```

Add fields to `CombatManager`:
```csharp
private bool _isAscendantForm = false;
private int  _ascendantCurrentHP = 0;
```

---

## Parts

**Phase 1 (Physical Stag):**

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Crown Antlers | 6 | 8 | Breaking slows Phase 1 movement to 1 per card |
| Chest | 5 | 12 | Core target — reduced to 0 triggers Phase 2 |
| Flank (Left) | 4 | 8 | Breaking reduces Phase 1 card attacks by −2 damage |
| Flank (Right) | 4 | 8 | Breaking reduces Phase 1 card attacks by −2 damage |
| Hindleg | 3 | 6 | Breaking prevents Charge movement |

**Note:** Reducing Chest (or total HP) to 30% triggers Phase 2. The Stag cannot be killed in Phase 1 — it transforms instead.

**Phase 2 (Ascendant Form):**
- HP pool: 20 (displayed as a separate bar)
- No parts — damage goes directly to Ascendant HP
- Grid positions meaningless — all attacks hit all hunters

---

## Phase 1 Deck (16 cards)

Create in `Assets/_Game/Data/Monsters/PaleStag/BehaviorCards/Phase1/`.

| cardId | cardName | Notes |
|---|---|---|
| `PSA-P1-01` | White Stride | Move 2 toward aggro. |
| `PSA-P1-02` | Antler Gore | No move. 4 Flesh to aggro. Target gains Bleeding (2 Flesh/rd, 2 rds). |
| `PSA-P1-03` | Charge | Move 3 toward aggro. 3 Flesh if adjacent. Trigger: Hindleg intact. |
| `PSA-P1-04` | Pale Light | No move. All hunters lose −2 Accuracy for 1 round (blinding). |
| `PSA-P1-05` | Hooves | Move 1. 2 Flesh to all adjacent hunters. Knockback 1. |
| `PSA-P1-06` | Mark Hunter | No move. Aggro shifts to hunter with highest Luck. |
| `PSA-P1-07` | Crown Crash | No move. 5 Flesh to aggro. Trigger: Antlers intact. |
| `PSA-P1-08` | Presence | No move. All hunters gain Fear (Shaken + −1 Grit). |
| `PSA-P1-09` | Sweep | Move 1. 3 Flesh to all hunters in a 180° arc. |
| `PSA-P1-10` | Transcend Attempt | No move. If below 50% HP: phase 2 triggers immediately. Otherwise: no effect. |
| `PSA-P1-11` | Pivot and Strike | No move. Turn to face lowest-Flesh hunter. 3 Flesh to that hunter. |
| `PSA-P1-12` | White Silence | No move. All hunters lose −1 Grit permanently. isShuffle: true |
| `PSA-P1-13` | Herd Memory | No move. All hunters gain −2 Luck for 1 round. |
| `PSA-P1-14` | Trample | Move 2. 2 Flesh to each hunter in path. |
| `PSA-P1-15` | Territorial | Move 1 away from lowest-Grit hunter. |
| `PSA-P1-16` | Final Charge | Move 3. 4 Flesh to aggro. isShuffle: true |

---

## Phase 2 Deck (12 cards — All AoE)

Create in `Assets/_Game/Data/Monsters/PaleStag/BehaviorCards/Phase2/`.

**All Phase 2 cards target ALL hunters simultaneously. No single-target attacks.**

| cardId | cardName | Notes |
|---|---|---|
| `PSA-P2-01` | White Wave | 3 Flesh to ALL hunters. Cannot be Evaded. |
| `PSA-P2-02` | Dissolution | ALL hunters lose −2 to all stats for 1 round. |
| `PSA-P2-03` | Pale Fire | 4 Flesh to ALL hunters + Burning (1 Flesh/rd, 2 rds). |
| `PSA-P2-04` | Light Sear | ALL hunters gain Blinded (−3 Accuracy for 1 round). |
| `PSA-P2-05` | Memory Rip | ALL hunters gain a random Disorder. One hunter's Disorder expires after this. |
| `PSA-P2-06` | Presence Absolute | ALL hunters gain Fear and Pinned for 1 round. |
| `PSA-P2-07` | The Final Question | 5 Flesh to ALL hunters. Cannot be Evaded. isShuffle: true |
| `PSA-P2-08` | Ascendant Strike | 6 Flesh to ALL hunters. Cannot be Evaded. isShuffle: true |
| `PSA-P2-09` | White Erasure | ALL hunters gain Shaken + lose −1 Grit permanently. |
| `PSA-P2-10` | Dissolution Wave | 2 Flesh to ALL hunters. ALL hunter status effects reset to zero. |
| `PSA-P2-11` | The End of It | 3 Flesh to ALL hunters. Ascendant Form HP heals 3. isShuffle: true |
| `PSA-P2-12` | Transcended | 8 Flesh to ALL hunters. Cannot be Evaded. Victory possible after this. isShuffle: true |

---

## Pale Stag MonsterSO Asset

Create `Assets/_Game/Data/Monsters/PaleStag/PaleStag_Overlord.asset`:

```
monsterName: The Pale Stag Ascendant
monsterType: Overlord — Final Hunt
isOverlord: true
hasAscendantForm: true
ascendantFormHP: 20
difficulty: Nightmare
huntYearMin: 25
huntYearMax: 30
prerequisiteOverlordKillCount: 1    ← Add this field to MonsterSO

Parts:
  Crown Antlers: shellHP=6, fleshHP=8
  Chest: shellHP=5, fleshHP=12
  Flank (Left): shellHP=4, fleshHP=8
  Flank (Right): shellHP=4, fleshHP=8
  Hindleg: shellHP=3, fleshHP=6

victoryNodePartNames: []    ← Victory via Ascendant Form HP only
phase1Deck: [PSA-P1-01 through PSA-P1-16]
phase2Deck: [PSA-P2-01 through PSA-P2-12]
rewardCodexEntryId: CodexEntry_ThePaleStag
rewardCraftSetId: ""   ← No craft set; campaign ends on victory
startingAggroTarget: Hunter with highest Luck
```

**Add to MonsterSO:**
```csharp
public int prerequisiteOverlordKillCount = 0;
```

**Gate this hunt in `GameStateManager`:**
```csharp
public bool CanHuntMonster(MonsterSO monster)
{
    if (CampaignState.currentYear < monster.huntYearMin) return false;
    if (CampaignState.overlordKillCount < monster.prerequisiteOverlordKillCount)
        return false;
    return true;
}
```

---

## Victory Trigger

When the Pale Stag Ascendant Form HP reaches 0:

```csharp
private void OnOverlordDefeated()
{
    // ... (standard overlord defeat code from Stage 9-M)
    
    // Special case: Pale Stag victory = campaign victory
    if (_activeMonster.monsterName == "The Pale Stag Ascendant")
    {
        GameStateManager.Instance.AddChronicleEntry(
            GameStateManager.Instance.CampaignState.currentYear,
            "The Pale Stag is gone. We don't know what that means yet.");
        // Trigger victory screen
        GameStateManager.Instance.CheckVictory();
    }
}
```

---

## Verification Test

- [ ] Pale Stag MonsterSO with 5 Phase 1 parts, 16 Phase 1 cards, 12 Phase 2 cards
- [ ] `prerequisiteOverlordKillCount = 1` — hunt not available with 0 overlords killed
- [ ] Phase 1: Stag behaves as a normal (but powerful) monster
- [ ] Phase 1 Chest reduced to 30% → "ASCENDANT FORM" banner appears
- [ ] Physical token disappears from grid
- [ ] All hunter status effects cleared at Phase 2 transition
- [ ] Phase 2: all cards deal damage to ALL hunters (no single-target)
- [ ] Ascendant HP bar shows (20 HP separate from Phase 1 parts)
- [ ] Ascendant HP reaches 0 → OnOverlordDefeated → CheckVictory
- [ ] VictoryEpilogue loads
- [ ] CodexEntry_ThePaleStag unlocked
- [ ] PSA-P1-12 (White Silence) → all hunters permanently lose 1 Grit (persists post-hunt)
- [ ] No errors when Phase 2 AoE card fires and a hunter is already dead

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_P.md`
**Covers:** Craft Sets Part 1 — building out all recipe assets for the Carapace Forge, Membrane Loft, and Mire Apothecary craft sets. Each set needs 5–7 craftable items (GearSO assets) with recipes (resource costs), stat bonuses, and overlaySprite references.
