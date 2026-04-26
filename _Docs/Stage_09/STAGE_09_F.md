<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-F | Thornback Behavior Cards
Status: Stage 9-E complete. Gear adjacency and link bonuses working.
Task: Create all 16 BehaviorCardSO assets for the Thornback monster.
The Thornback is an armoured quadruped with crystalline dorsal spines.
Its behavior deck emphasizes trampling approaches, body-spinning area
attacks, and entrenched counterattacks. Wire all cards into the
existing Thornback MonsterSO (if it exists) or create the MonsterSO.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_F.md
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Systems/MonsterAI.cs

Then confirm:
- BehaviorCardSO fields: cardId, cardName, triggerCondition,
  movementEffect, attackEffect, specialEffect
- MonsterSO has a behaviorDeck field (BehaviorCardSO[] or List)
- MonsterAI draws and resolves cards from the deck each round
- The Thornback is a Year 1-4 Standard hunt target (not an overlord)
- What you will NOT build (Thornback sprite generation — in 9-K)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-F: Thornback Behavior Cards

**Resuming from:** Stage 9-E complete — gear adjacency and link bonus logic working
**Done when:** All 16 Thornback behavior cards exist as SO assets; Thornback MonsterSO references the full deck; MonsterAI can draw and resolve each card
**Commit:** `"9F: Thornback behavior deck — 16 cards, MonsterSO wired"`
**Next session:** STAGE_09_G.md

---

## The Thornback — Monster Design

**Name:** The Thornback
**Type:** Beast
**Tier:** Standard (hunted Years 1–4)
**Difficulty:** Standard

**Lore:** A stocky quadruped the size of a draught horse, with a low-slung head and a ridge of crystalline spines running from neck to tail. The spines catch light at dawn and dusk — settlers call it "the creature that glitters before it charges." It cannot retreat easily once its spines are deployed, making it dangerous to circle around it but exploitable if you can stay in front.

**Combat Identity:**
- Charges and tramples when far away
- Locks into a "spine stance" when surrounded (AoE threat)
- Retaliates hard when hit on the spine dorsal (vulnerable to front, dangerous from sides)
- Slow but relentless — hunters who stop moving get pushed

**Parts:**
| Part | Shell HP | Flesh HP | Notes |
|---|---|---|---|
| Crown (head) | 4 | 6 | Breaking shell staggers; breaking flesh grants +1 Accuracy all hunters |
| Chest | 6 | 8 | Core target |
| Spine Crest (back) | 8 | 4 | Breaking shell triggers Spine Break special; very high shell |
| Foreleg | 4 | 5 | Breaking slows the Thornback (−1 Speed) |
| Hindleg | 4 | 5 | Breaking removes Trample movement ability |

---

## Behavior Card Design

The Thornback's 16-card deck is structured as follows:

| Card Type | Count |
|---|---|
| Move (approach / charge) | 5 |
| Attack | 5 |
| Special (stance, spin, retaliate) | 4 |
| Passive / Skip | 2 |

---

## Part 1: BehaviorCardSO — Verify Fields

Confirm `BehaviorCardSO.cs` has at minimum:

```csharp
public string cardId;
public string cardName;

[TextArea(2,4)]
public string triggerCondition;    // When this card is "active" (always, if blood, etc.)

[TextArea(2,4)]
public string movementEffect;      // How the monster moves this turn

[TextArea(2,4)]
public string attackEffect;        // How the monster attacks this turn

[TextArea(2,4)]
public string specialEffect;       // Any additional special rule this turn

public bool   isShuffle;           // If true, shuffle deck after this card is drawn
```

If any field is missing, add it now.

---

## Part 2: All 16 Thornback Behavior Cards

Create in `Assets/_Game/Data/Monsters/Thornback/BehaviorCards/`.

Right-click → Create → MnM → Behavior Card.

---

### Movement Cards (5)

**Card 01 — Creeping Advance**
```
cardId: TBK-B01
cardName: Creeping Advance
triggerCondition: Always
movementEffect: Move 1 space toward the aggro target.
attackEffect: No attack.
specialEffect: —
```

**Card 02 — Lumber**
```
cardId: TBK-B02
cardName: Lumber
triggerCondition: Always
movementEffect: Move 2 spaces toward the aggro target.
  If the Thornback enters a hunter's space, that hunter
  is pushed back 1 space (Knockback).
attackEffect: No attack.
specialEffect: —
```

**Card 03 — Trample Charge**
```
cardId: TBK-B03
cardName: Trample Charge
triggerCondition: Aggro target is 3+ spaces away
movementEffect: Move 3 spaces directly toward the aggro target,
  ignoring occupied spaces (pass through, apply Knockback to
  every hunter in path).
attackEffect: At end of movement: 2 Flesh damage to aggro target.
specialEffect: —
```

**Card 04 — Pivot**
```
cardId: TBK-B04
cardName: Pivot
triggerCondition: Always
movementEffect: Rotate to face the hunter with the lowest Flesh.
  No position change.
attackEffect: No attack.
specialEffect: The Thornback now threatens that hunter —
  next card drawn is resolved as if this hunter is the aggro target.
```

**Card 05 — Pin Ground**
```
cardId: TBK-B05
cardName: Pin Ground
triggerCondition: Any hunter is adjacent
movementEffect: No movement.
attackEffect: All adjacent hunters gain Pinned (cannot move next round).
specialEffect: The Thornback's Spine Crest gains +2 Shell HP until
  it next moves.
```

---

### Attack Cards (5)

**Card 06 — Gore**
```
cardId: TBK-B06
cardName: Gore
triggerCondition: Always
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target.
  If aggro target has no Shell remaining (on any part): +1 damage.
specialEffect: —
```

**Card 07 — Head Slam**
```
cardId: TBK-B07
cardName: Head Slam
triggerCondition: Always
movementEffect: Move 1 space toward aggro target.
attackEffect: 2 Flesh damage to aggro target.
  If this hits: aggro target gains Shaken.
specialEffect: —
```

**Card 08 — Spine Lash**
```
cardId: TBK-B08
cardName: Spine Lash
triggerCondition: Any hunter is behind the Thornback
  (in the two cells directly behind its current facing)
movementEffect: No movement.
attackEffect: 2 Flesh damage to all hunters behind the Thornback.
  This attack cannot be Evaded (the spines sweep too fast).
specialEffect: If Spine Crest shell is broken: +2 damage to this attack.
```

**Card 09 — Foreleg Stomp**
```
cardId: TBK-B09
cardName: Foreleg Stomp
triggerCondition: Always
movementEffect: No movement.
attackEffect: 1 Flesh damage to each hunter in cells directly
  in front of the Thornback.
specialEffect: Hunters hit are pushed 1 space away (Knockback).
```

**Card 10 — Crushing Weight**
```
cardId: TBK-B10
cardName: Crushing Weight
triggerCondition: Aggro target is adjacent
movementEffect: No movement.
attackEffect: 4 Flesh damage to aggro target.
  If this would reduce the target below 0 Flesh: they are
  Crippled (cannot use movement cards next round).
specialEffect: isShuffle: true (deck reshuffled after this card).
```

---

### Special Cards (4)

**Card 11 — Spine Deploy**
```
cardId: TBK-B11
cardName: Spine Deploy
triggerCondition: Always
movementEffect: No movement.
attackEffect: No attack.
specialEffect: The Thornback enters Spine Stance until its next
  movement card. In Spine Stance: any hunter who moves adjacent
  to the Thornback takes 2 automatic Flesh damage (spine graze).
  The Thornback's Spine Crest shell regenerates 1 HP.
```

**Card 12 — Berserk Spin**
```
cardId: TBK-B12
cardName: Berserk Spin
triggerCondition: Spine Crest shell is broken
movementEffect: No movement.
attackEffect: 2 Flesh damage to all hunters within 2 spaces.
specialEffect: Each hunter hit gains Shaken.
  After this card: the Thornback collapses into Stunned state
  (skips its next card draw — resolve the following card as "No action").
```

**Card 13 — Enrage**
```
cardId: TBK-B13
cardName: Enrage
triggerCondition: Any part's Flesh HP is broken (reduced to 0)
movementEffect: Move 1 space toward the hunter who last attacked.
attackEffect: 3 Flesh damage to that hunter.
specialEffect: If the Thornback has no broken flesh parts,
  this card has no effect (treat as Creeping Advance instead).
```

**Card 14 — Blood Frenzy**
```
cardId: TBK-B14
cardName: Blood Frenzy
triggerCondition: The Thornback's own Flesh total is below 50%
movementEffect: Move 2 spaces toward the closest hunter.
attackEffect: 3 Flesh damage to the closest hunter.
  If the Thornback moves at least 1 space this card: +1 damage.
specialEffect: isShuffle: true
```

---

### Passive / Skip Cards (2)

**Card 15 — Stillness**
```
cardId: TBK-B15
cardName: Stillness
triggerCondition: Always
movementEffect: No movement.
attackEffect: No attack.
specialEffect: The Thornback surveys the field. Its aggro token
  moves to the hunter with the fewest remaining Flesh HP.
```

**Card 16 — Recuperate**
```
cardId: TBK-B16
cardName: Recuperate
triggerCondition: No hunter is adjacent
movementEffect: No movement.
attackEffect: No attack.
specialEffect: The Thornback regains 2 HP to its Chest Flesh.
  If any hunter IS adjacent, this card has no effect
  (treat as Creeping Advance instead).
```

---

## Part 3: Thornback MonsterSO

Create or update `Assets/_Game/Data/Monsters/Thornback/Thornback_Standard.asset`:

```
monsterName: The Thornback
monsterType: Beast
difficulty: Standard
huntYearMin: 1
huntYearMax: 4

Parts:
  Crown: shellHP=4, fleshHP=6
  Chest: shellHP=6, fleshHP=8
  Spine Crest: shellHP=8, fleshHP=4
  Foreleg: shellHP=4, fleshHP=5
  Hindleg: shellHP=4, fleshHP=5

behaviorDeck: [TBK-B01 through TBK-B16, all 16 cards]
startingAggroTarget: hunter with highest Grit
```

---

## Part 4: Verify MonsterAI Resolves Each Card Type

In `MonsterAI.cs`, confirm the `ResolveCard()` method handles:

```csharp
private void ResolveCard(BehaviorCardSO card)
{
    Debug.Log($"[MonsterAI] {_monster.monsterName} resolves: {card.cardName}");

    // Check trigger condition — if not met, some cards have fallback behavior
    // (This is checked via string matching for MVP; replace with enum in Stage 9-R)
    bool triggerMet = EvaluateTrigger(card.triggerCondition);

    if (!triggerMet)
    {
        Debug.Log($"[MonsterAI] Trigger not met for {card.cardName} — skipping");
        return;
    }

    // Movement
    if (!string.IsNullOrEmpty(card.movementEffect))
        ApplyMovement(card.movementEffect);

    // Attack
    if (!string.IsNullOrEmpty(card.attackEffect))
        ApplyAttack(card.attackEffect);

    // Special
    if (!string.IsNullOrEmpty(card.specialEffect))
        ApplySpecial(card.specialEffect);

    // Shuffle if flagged
    if (card.isShuffle)
        ShuffleDeck();
}
```

Add `Debug.Log` calls inside `EvaluateTrigger()` as required by the CLAUDE.md workflow:

```csharp
private bool EvaluateTrigger(string condition)
{
    if (string.IsNullOrEmpty(condition) || condition == "Always")
    {
        Debug.Log("[MonsterAI] Trigger: Always — condition met");
        return true;
    }
    // Add specific condition checks here as they are needed
    Debug.Log($"[MonsterAI] Trigger: '{condition}' — evaluated (stub returns true)");
    return true; // Stub: always true for MVP
}
```

---

## Verification Test

- [ ] All 16 Thornback behavior card assets exist in the correct folder
- [ ] Each card has a non-empty cardId and cardName
- [ ] Thornback MonsterSO `behaviorDeck` array shows 16 cards in Inspector
- [ ] Play combat against the Thornback — each round a card is drawn and logged
- [ ] `[MonsterAI] The Thornback resolves: Creeping Advance` appears in Console
- [ ] TBK-B03 (Trample Charge) fires — monster moves 3 spaces
- [ ] TBK-B11 (Spine Deploy) fires — Spine Stance special effect logged
- [ ] TBK-B10 (Crushing Weight) fires — deck reshuffled after (check Console log)
- [ ] TBK-B15 (Stillness) — aggro token reassigns to lowest Flesh hunter
- [ ] No NullReferenceException when drawing from deck
- [ ] Deck shuffles at start of each hunt (not mid-hunt unless isShuffle=true)

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_G.md`
**Covers:** Ivory Stampede Pack Behavior Cards — the unique PackMonsterSO design for The Ivory Stampede, which uses a coordinated herd of 3 tokens that share one behavior deck and activate in sequence
