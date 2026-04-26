<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-N | Overlord: The Penitent
Status: Stage 9-M complete. The Siltborn overlord done.
Task: Build The Penitent overlord monster — a humanoid figure
that deals damage to itself to power its attacks, grows stronger
as it takes damage, and becomes more dangerous the more hunters
have been lost in the campaign. Two-phase deck, unique self-harm
mechanic, campaign victory unlocks the Penitent codex entry and
the Ichor Works craft set.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_N.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs

Then confirm:
- The Penitent is a single-token overlord (not pack, not node-based)
- Self-harm mechanic: some cards cause the Penitent to deal X damage
  to itself to gain attack bonuses
- Desperation scaling: base damage on cards increases by 1 for every
  2 hunters who have died in the campaign (tracked in CampaignState.totalHunterDeaths)
- Phase 2 triggers at 40% HP (same as Siltborn)
- Victory unlocks CodexEntry_ThePenitent and Ichor Works craft set

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-N: Overlord — The Penitent

**Resuming from:** Stage 9-M complete — The Siltborn overlord done
**Done when:** Penitent MonsterSO exists with two-phase deck; self-harm mechanic resolves correctly; desperation scaling applies based on campaign death count
**Commit:** `"9N: Overlord Penitent — self-harm, desperation scaling, two-phase deck"`
**Next session:** STAGE_09_O.md

---

## The Penitent — Monster Design

**Name:** The Penitent
**Type:** Overlord — Humanoid Ruin
**Tier:** Overlord (available Years 12–22)
**Difficulty:** Nightmare

**Lore:** *(Post-hunt codex entry, written by a survivor)*
"It wore us on it. That's the only way I can put it. Every hunter we'd lost — it moved like it remembered them. Like it had absorbed what made them dangerous and was wearing it now. We had to keep reminding ourselves it wasn't them. By the end I'm not sure Wren was convinced."

**Combat Identity:**
- Humanoid figure — moves like a hunter, fights like a nightmare version of one
- Self-harm: can choose to deal damage to itself to supercharge its next attack
- Desperation scaling: for every 2 hunters who have died in this campaign (across all years), the Penitent's base attack damage is +1
- Phase 2 trigger: 40% total HP
- Defeat condition: reduce main body to 0 (standard, not node-based)

---

## Desperation Scaling Implementation

In `CombatManager`, before resolving any Penitent attack:

```csharp
private int GetPenitentDesperation()
{
    if (_activeMonster.monsterName != "The Penitent") return 0;
    int deaths = GameStateManager.Instance.CampaignState.totalHunterDeaths;
    int bonus  = deaths / 2;
    if (bonus > 0)
        Debug.Log($"[Combat] Penitent desperation bonus: +{bonus} damage " +
                  $"({deaths} campaign deaths).");
    return bonus;
}

// Apply in attack resolution:
int finalDamage = baseDamage + GetPenitentDesperation();
```

---

## Self-Harm Mechanic

When the Penitent plays a self-harm card, it deals damage to itself first (reducing its own Flesh HP), then gains a bonus for its next attack.

```csharp
private int _penitentSelfHarmBonus = 0;

private void PenitentSelfHarm(int selfDamage, int attackBonus)
{
    // Find the Penitent's main body part and apply self-damage
    var body = FindPart("Body");
    if (body != null)
    {
        body.currentFleshHP = Mathf.Max(0, body.currentFleshHP - selfDamage);
        Debug.Log($"[Combat] Penitent self-harms for {selfDamage}. " +
                  $"Next attack +{attackBonus} bonus.");
    }
    _penitentSelfHarmBonus += attackBonus;
    CheckPhaseTransition();
    CheckOverlordVictory();   // In case self-harm kills it (edge case)
}
```

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Iron Mask | 4 | 6 | Breaking reveals the face; all hunters gain +1 Accuracy vs Penitent |
| Body | 0 | 30 | Main target — no shell (The Penitent does not defend itself) |
| Left Arm | 3 | 8 | Breaking disables all left-hand weapon attacks |
| Right Arm | 3 | 8 | Breaking disables all right-hand weapon attacks |
| Spine Chain | 4 | 6 | Breaking disables self-harm mechanic (removes attack bonus) |

**Note:** The Penitent has NO shell on its Body — it is intentionally exposed. All damage reaches flesh directly. The challenge is its self-harm powering and its desperation scaling.

---

## Phase 1 Deck (18 cards)

Create in `Assets/_Game/Data/Monsters/Penitent/BehaviorCards/Phase1/`.

| cardId | cardName | Notes |
|---|---|---|
| `PNT-P1-01` | Advance | Move 2 toward aggro. No attack. |
| `PNT-P1-02` | Strike | No move. 4 + desperation Flesh to aggro. |
| `PNT-P1-03` | Twin Strike | No move. 2 + desp to aggro. 2 + desp to nearest non-aggro hunter. |
| `PNT-P1-04` | Self-Flagellate | No move. Deal 3 Flesh to itself. Next attack: +4 bonus damage. Trigger: Spine Chain intact. |
| `PNT-P1-05` | Relentless | Move 1 toward aggro. 3 + desp Flesh to aggro. |
| `PNT-P1-06` | Lunge | Move 2 toward aggro. 3 Flesh if adjacent after move. |
| `PNT-P1-07` | Weeping Blow | No move. 3 Flesh to aggro. Target gains Shaken. |
| `PNT-P1-08` | Iron Mask Slam | No move. 3 + desp Flesh (head attack). Target gains Stunned (skip next action). Trigger: Iron Mask intact. |
| `PNT-P1-09` | Bearing Witness | No move. All hunters gain −1 Grit for 1 round. If 3+ hunters dead in campaign: also apply Shaken. |
| `PNT-P1-10` | Grim Advance | Move 1 toward lowest-Flesh hunter. Aggro shifts to that hunter. |
| `PNT-P1-11` | Chain Lash | No move. 2 Flesh to all hunters within 2 spaces. |
| `PNT-P1-12` | Blood Surge | No move. Deal 5 Flesh to itself. +6 bonus to next attack. Trigger: Spine Chain intact. isShuffle: true |
| `PNT-P1-13` | Pivot | No move. Aggro shifts to hunter with highest Grit. |
| `PNT-P1-14` | Press | No move. 2 Flesh to aggro. Aggro target gains Pinned. |
| `PNT-P1-15` | Sorrow Strike | Move 1 toward aggro. 4 + desp Flesh to aggro. |
| `PNT-P1-16` | Endure | No move. Penitent gains +2 to all damage for 1 round (Endure stance). |
| `PNT-P1-17` | Survey | No move. Aggro moves to hunter who has dealt most damage to Penitent. |
| `PNT-P1-18` | Heavy Blow | No move. 5 Flesh to aggro. isShuffle: true |

---

## Phase 2 Deck (10 cards)

Create in `Assets/_Game/Data/Monsters/Penitent/BehaviorCards/Phase2/`.

| cardId | cardName | Notes |
|---|---|---|
| `PNT-P2-01` | Frenzy | Move 1. 5 + desp Flesh to aggro. isShuffle: true |
| `PNT-P2-02` | Death Blow | No move. 8 + desp Flesh to aggro. Cannot be Evaded. isShuffle: true |
| `PNT-P2-03` | Mass Strike | No move. 4 + desp Flesh to all hunters within 2 spaces. |
| `PNT-P2-04` | Final Flagellation | Deal 8 to itself. +8 next attack. Trigger: Spine Chain intact. isShuffle: true |
| `PNT-P2-05` | The Weight | No move. All hunters gain Fear. Hunters below half Flesh also gain Disorder — random Disorder applied. |
| `PNT-P2-06` | Relentless End | Move 2. 5 Flesh to aggro + 2 Flesh to all adjacent. |
| `PNT-P2-07` | Desperate Strike | No move. 4 Flesh to aggro + 2 Flesh to Penitent (self-harm, always fires). |
| `PNT-P2-08` | All at Once | No move. 3 Flesh to ALL hunters. Cannot be Evaded. isShuffle: true |
| `PNT-P2-09` | Last Rite | No move. All hunters lose −1 to all stats for 1 round. Log: "The Penitent calls on something old." |
| `PNT-P2-10` | Collapse | No move. 6 Flesh to aggro. isShuffle: true |

---

## Penitent MonsterSO Asset

Create `Assets/_Game/Data/Monsters/Penitent/Penitent_Overlord.asset`:

```
monsterName: The Penitent
monsterType: Overlord — Humanoid Ruin
isOverlord: true
difficulty: Nightmare
huntYearMin: 12
huntYearMax: 22

Parts:
  Iron Mask: shellHP=4, fleshHP=6
  Body: shellHP=0, fleshHP=30
  Left Arm: shellHP=3, fleshHP=8
  Right Arm: shellHP=3, fleshHP=8
  Spine Chain: shellHP=4, fleshHP=6

victoryNodePartNames: ["Body"]   ← Standard defeat: reduce Body to 0
phase1Deck: [PNT-P1-01 through PNT-P1-18]
phase2Deck: [PNT-P2-01 through PNT-P2-10]
rewardCodexEntryId: CodexEntry_ThePenitent
rewardCraftSetId: Ichor
startingAggroTarget: Hunter with highest Grit
```

---

## Verification Test

- [ ] Penitent MonsterSO with 5 parts, 18 Phase 1 cards, 10 Phase 2 cards
- [ ] Body has 0 Shell HP (takes full damage from all attacks)
- [ ] Campaign with 6 hunter deaths → Penitent attack damage +3 (6/2=3)
- [ ] Campaign with 0 deaths → no desperation bonus
- [ ] PNT-P1-04 (Self-Flagellate) fires → Penitent takes 3 Flesh; next attack has +4 bonus
- [ ] Self-harm bonus applies to very next attack card, then resets to 0
- [ ] Spine Chain broken → self-harm cards have no effect (no self-damage, no bonus)
- [ ] Phase 2 triggers at 40% of Body HP (12 HP remaining out of 30)
- [ ] PNT-P2-05 (The Weight) → all hunters gain Fear; hunters below half HP gain random Disorder
- [ ] Campaign death count changes mid-game → desperation bonus updates
- [ ] Penitent Body reaches 0 → OnOverlordDefeated fires
- [ ] CodexEntry_ThePenitent unlocked in CampaignState
- [ ] Ichor craft set unlocked

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_O.md`
**Covers:** Overlord — The Pale Stag Ascendant. The final overlord and the campaign's climactic hunt. A massive transcendent beast that calls the final reckoning. Full two-phase design, unique "Ascendant Form" Phase 2 mechanic, and campaign Victory state trigger.
