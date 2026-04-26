<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-I | The Shriek Full Monster Design
Status: Stage 9-H complete. Bog Caller monster done.
Task: Create the full Shriek monster — a fast, evasive creature
that swoops across the combat grid, strikes from above, and
inflicts fear. Design lore, 5 breakable parts, 16 behavior cards,
and the MonsterSO asset. The Shriek introduces the "Dive" movement
mechanic (teleport to any cell, ignoring obstacles).

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_I.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- Dive movement is handled as a special movement string "Dive:[target]"
  parsed by CombatManager.ApplyMovement()
- Fear effect = Shaken + Grit -1 for 1 round
- The Shriek's high Evasion means attacks against it require a
  higher Accuracy roll (CombatManager already handles Evasion)
- What you will NOT build (aerial sprite — separate art session)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-I: The Shriek Full Monster Design

**Resuming from:** Stage 9-H complete — Bog Caller monster done
**Done when:** Shriek MonsterSO exists with 16 behavior cards; Dive movement resolves correctly; Fear effect applies Shaken + Grit penalty
**Commit:** `"9I: The Shriek monster — dive movement, fear effect, 16 behavior cards"`
**Next session:** STAGE_09_J.md

---

## The Shriek — Monster Design

**Name:** The Shriek
**Type:** Aerial Beast
**Tier:** Standard (hunted Years 5–10)
**Difficulty:** Veteran recommended

**Lore:** The first time hunters heard it, they thought it was the wind. The second time, they were already bleeding. The Shriek is rarely seen at rest — it exists almost entirely in motion, a dark shape crossing the sky between attacks. The settlers tried to chart its territory and found it had none. It is simply always hunting. The sound it makes just before it dives has driven two hunters permanently deaf and one permanently erratic.

**Combat Identity:**
- Very high Evasion (attacks against it are at −2 Accuracy)
- Uses "Dive" movement: can reposition to any cell on the grid
- After a Dive, it attacks with a guaranteed hit (bypasses Evasion)
- Inflicts Fear (Shaken + −1 Grit) as a persistent threat
- Punishes hunters who stand still — it picks off stationary targets
- Relatively low HP once reached — high risk/high reward to focus fire

**Dive Mechanic:**
When a card calls for "Dive," the Shriek's token is moved instantly to any valid grid cell of the AI's choosing (closest hunter). This does not trigger normal movement reactions. A Dive attack ignores the target's Evasion stat.

**Add to CombatManager.ApplyMovement():**
```csharp
if (movementEffect.StartsWith("Dive:"))
{
    // Parse target type: "Dive:Closest" or "Dive:AggroTarget"
    var diveTarget = movementEffect.Split(':')[1].Trim();
    HunterState target = diveTarget == "AggroTarget" ? GetAggroTarget() : GetClosestHunter();
    if (target == null) return;
    var pos = GetHunterPosition(target.hunterId);
    MoveMonsterToCell(pos); // Teleport, no animation path
    Debug.Log($"[Combat] Shriek dives to {pos} — next attack ignores Evasion.");
    _monsterState.nextAttackIgnoresEvasion = true;
}
```

Add `nextAttackIgnoresEvasion` bool to monster runtime state.

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Beak | 2 | 4 | Breaking disables Dive attack bonus (attacks no longer bypass Evasion) |
| Wing (Left) | 3 | 6 | Breaking limits Dive to adjacent 3 cells only |
| Wing (Right) | 3 | 6 | Breaking removes Dive movement entirely |
| Chest | 4 | 8 | Core target |
| Throat | 2 | 5 | Breaking removes Shriek ability (Fear effect) |

---

## 16 Behavior Cards

Create in `Assets/_Game/Data/Monsters/Shriek/BehaviorCards/`.

### Movement Cards (4)

**SHK-B01 — Glide**
```
movementEffect: Move 2 spaces toward the aggro target.
attackEffect: —
specialEffect: —
```

**SHK-B02 — Swoop**
```
movementEffect: Dive to aggro target's cell. Push aggro target back 1.
attackEffect: 2 Flesh damage (ignores Evasion — Dive attack).
specialEffect: —
```

**SHK-B03 — Reposition**
```
movementEffect: Dive to a cell 2 spaces away from all hunters.
attackEffect: —
specialEffect: The Shriek gains +2 Evasion until its next movement.
```

**SHK-B04 — Feint**
```
movementEffect: Move 1 space to the left of aggro target.
attackEffect: No attack.
specialEffect: Aggro token shifts to the hunter with the highest Accuracy.
  The Shriek has "read" which hunter is the biggest threat.
```

---

### Attack Cards (5)

**SHK-B05 — Talons**
```
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target.
specialEffect: If the aggro target has not moved this round: +2 damage
  (stationary targets are picked off).
```

**SHK-B06 — Shriek**
```
triggerCondition: Throat is intact (shell not broken)
movementEffect: No movement.
attackEffect: —
specialEffect: All hunters gain Fear (Shaken + −1 Grit for 1 round).
  If Throat shell is broken: this card has no effect (no sound).
```

**SHK-B07 — Death Dive**
```
movementEffect: Dive to aggro target.
attackEffect: 4 Flesh damage (ignores Evasion — Dive attack).
specialEffect: isShuffle: true
```

**SHK-B08 — Rake**
```
movementEffect: Move through a line of cells (straight line, 3 cells).
attackEffect: 1 Flesh damage to each hunter in the movement path.
specialEffect: Hunters hit gain Slowed.
```

**SHK-B09 — Pinpoint**
```
triggerCondition: Aggro target has Shaken.
movementEffect: No movement.
attackEffect: 3 Flesh damage to aggro target. Cannot be Evaded.
specialEffect: If target doesn't have Shaken: treat as Talons.
```

---

### Special Cards (5)

**SHK-B10 — Aerial**
```
movementEffect: No movement.
attackEffect: —
specialEffect: The Shriek ascends to an aerial position. Until its
  next movement card: it cannot be targeted by melee attacks.
  Ranged attacks against it have −1 Accuracy.
```

**SHK-B11 — Diving Strike**
```
triggerCondition: Shriek is in Aerial position (from SHK-B10).
movementEffect: Dive to the hunter with the lowest Evasion.
attackEffect: 5 Flesh damage (ignores Evasion). Shriek exits Aerial.
specialEffect: If Shriek is not in Aerial: treat as Swoop.
```

**SHK-B12 — Sonic Burst**
```
movementEffect: No movement.
attackEffect: 2 Flesh damage to all hunters within 3 spaces.
specialEffect: Each hunter hit loses −1 Accuracy for 1 round
  (the sound disrupts their aim).
```

**SHK-B13 — Evasion Burst**
```
movementEffect: Dive 1 cell in a random direction.
attackEffect: —
specialEffect: The Shriek gains +3 Evasion until its next attack.
  Use this when hunters are landing too many hits.
```

**SHK-B14 — Bleed Strike**
```
movementEffect: Move 1 toward aggro.
attackEffect: 2 Flesh damage to aggro target.
specialEffect: Aggro target gains Bleeding (lose 1 Flesh at start
  of each of their next 3 turns).
```

---

### Passive / Skip Cards (2)

**SHK-B15 — Circle**
```
movementEffect: Move around the aggro target clockwise 1 cell
  (rotate 90 degrees around the target).
attackEffect: —
specialEffect: The Shriek's Evasion resets to base value.
```

**SHK-B16 — Patience (Aerial)**
```
movementEffect: No movement.
attackEffect: —
specialEffect: Aggro shifts to the hunter with the lowest current Flesh.
  The Shriek will target the weakest prey. isShuffle: true
```

---

## Shriek MonsterSO Asset

Create `Assets/_Game/Data/Monsters/Shriek/Shriek_Standard.asset`:

```
monsterName: The Shriek
monsterType: Aerial Beast
difficulty: Veteran
huntYearMin: 5
huntYearMax: 10
baseEvasion: 3        ← Add baseEvasion field to MonsterSO if needed

Parts:
  Beak: shellHP=2, fleshHP=4
  Wing (Left): shellHP=3, fleshHP=6
  Wing (Right): shellHP=3, fleshHP=6
  Chest: shellHP=4, fleshHP=8
  Throat: shellHP=2, fleshHP=5

behaviorDeck: [SHK-B01 through SHK-B16]
startingAggroTarget: Hunter with highest Accuracy
```

---

## Add `baseEvasion` to MonsterSO

If not already present:

```csharp
[Header("Combat Stats")]
public int baseEvasion = 0;  // Hunters' Accuracy checks are at (hunterAccuracy - baseEvasion)
```

In `CombatManager.ResolveHunterAttack()`:

```csharp
int effectiveAccuracy = hunter.accuracy - _activeMonster.baseEvasion;
bool hit = RollD10() <= effectiveAccuracy;
```

---

## Verification Test

- [ ] Shriek MonsterSO with all 5 parts and all 16 cards in deck
- [ ] SHK-B02 (Swoop) fires → Shriek teleports to aggro target's cell
- [ ] Dive attack hits without Evasion check
- [ ] Normal attack against Shriek: hunter accuracy is reduced by 3
- [ ] SHK-B06 (Shriek) fires → all hunters gain Shaken and −1 Grit
- [ ] Throat shell broken → SHK-B06 has no effect, logged in Console
- [ ] SHK-B10 (Aerial) → Shriek enters Aerial state; melee attacks blocked
- [ ] SHK-B11 (Diving Strike) → Shriek exits Aerial and deals 5 damage
- [ ] Wing (Right) broken → Dive movement no longer available
- [ ] Wing (Left) broken → Dive limited to 3 cells
- [ ] Fear effect: Shaken applied + Grit −1 for exactly 1 round, then resets
- [ ] No Console errors when Dive target is the last living hunter

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_J.md`
**Covers:** The Rotmother — a slow, large late-game monster that spawns Rot Spawn minions each round and uses a corruption mechanic that permanently weakens hunters if left unchecked. Full design with 16 behavior cards and MonsterSO.
