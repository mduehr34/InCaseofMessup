<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-S | Overlord: The Suture (Deferred Design Session)
Status: Stages 9-A through 9-R complete. This is a supplementary
session that fills the design gap left in Stage 7-N, where The
Suture's MonsterSO skeleton was created but its behavior deck
was deliberately left empty (flagged "STOP AND ASK — GDD does
not fully define The Suture's cards").

Task: Complete The Suture overlord — fill the behavior deck,
confirm all parts, add self-repair mechanic, wire its node-based
defeat condition, create the Sinew craft set reward, and update
the existing Monster_Suture.asset.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_09/STAGE_09_S.md
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Data/Monsters/Monster_Suture.asset  ← existing skeleton

Then confirm:
- Monster_Suture.asset already exists with stat blocks but empty decks
- Self-repair mechanic: at monster turn start, restore 2 Flesh to
  the most-damaged non-broken part (stops when both Limb Clusters broken)
- Defeat condition: node-based — all 3 Suture Cores must reach 0 Flesh
  (same pattern as Siltborn's node system from Stage 9-M)
- Phase 2 triggers at 40% of combined Core HP
- Victory unlocks CodexEntry_TheSuture + Sinew craft set
- Run STAGE_09_R.md verification again after this session completes

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-S: Overlord — The Suture (Deferred Design Session)

**Context:** The Suture (OVR-04) was sprite-imported and given a MonsterSO stat skeleton in Stage 7-N, but its behavior deck was flagged as needing a dedicated design session because the GDD did not fully define it. This is that session.
**Resuming from:** Stage 9-R (Final Integration DoD) — this session fills the only remaining gap
**Done when:** Monster_Suture.asset fully populated; 18 Phase 1 + 12 Phase 2 behavior cards exist; self-repair mechanic resolves; 3 Suture Cores trigger overlord defeat; Sinew craft set created
**Commit:** `"9S: Overlord Suture — self-repair, node defeat, Sinew craft set"`
**Next session:** Re-run STAGE_09_R.md verification checklist with Suture added

---

## The Suture — Monster Design

**Name:** The Suture
**Type:** Overlord — Body-Horror Accumulation
**Tier:** Overlord (available Years 15–25; no kill prerequisite)
**Difficulty:** Nightmare

**Lore:** *(Codex entry unlocked on victory)*
"We had a theory, halfway through, that it was learning from us. Not tactics — something deeper. The way it moved after Petra fell looked too much like how she moved. We stopped letting it get close after that. It didn't help. You can't stop something from remembering you when it's made of people who couldn't."

**Combat Identity:**
- A mass of stitched-together limbs, bodies, and sinew pulled forward by sheer accumulated weight
- **Self-repair mechanic:** At the start of each monster turn (before card draw), The Suture restores 2 Flesh HP to its most-damaged non-broken part. This stops permanently when both Limb Clusters are broken.
- **Defeat condition:** Node-based — hunters must reduce all 3 Suture Cores (A, B, and Central Mass) to 0 Flesh. Destroying limbs weakens the Suture but does not end the hunt.
- Phase 2 trigger: 40% of combined Suture Core HP (combined = 12+12+15 = 39; trigger at ≤15 remaining)
- Killing it triggers OnOverlordDefeated with reward: CodexEntry_TheSuture + Sinew craft set unlock

---

## Self-Repair Mechanic Implementation

Add to `CombatManager`:

```csharp
// Called at start of each monster turn, before card draw
private void ResolveSutureRepair()
{
    if (_activeMonster.monsterName != "The Suture") return;

    // Check if BOTH Limb Clusters are broken — if so, repair is disabled
    var leftCluster  = FindPart("Left Limb Cluster");
    var rightCluster = FindPart("Right Limb Cluster");
    bool leftBroken  = leftCluster  == null || leftCluster.currentFleshHP  <= 0;
    bool rightBroken = rightCluster == null || rightCluster.currentFleshHP <= 0;

    if (leftBroken && rightBroken)
    {
        Debug.Log("[Combat] Suture — both Limb Clusters broken. Self-repair disabled.");
        return;
    }

    // Find most-damaged non-broken, non-Core part
    // "Most damaged" = largest gap between maxFleshHP and currentFleshHP
    MonsterPart target = null;
    int worstGap = 0;

    foreach (var part in _activeMonster.parts)
    {
        if (part.currentFleshHP <= 0) continue;              // already broken — skip
        if (IsSutureCore(part.partName))        continue;    // Cores don't self-repair
        int gap = part.maxFleshHP - part.currentFleshHP;
        if (gap > worstGap) { worstGap = gap; target = part; }
    }

    if (target == null || worstGap == 0)
    {
        Debug.Log("[Combat] Suture — no damaged parts to repair.");
        return;
    }

    int repaired = Mathf.Min(2, worstGap);
    target.currentFleshHP += repaired;
    Debug.Log($"[Combat] Suture self-repairs {repaired} Flesh on {target.partName}. " +
              $"Now {target.currentFleshHP}/{target.maxFleshHP}.");
    _hud?.RefreshPartBar(target);
}

private bool IsSutureCore(string partName) =>
    partName == "Suture Core A" ||
    partName == "Suture Core B" ||
    partName == "Central Mass";
```

Call this at the top of the monster turn, before `DrawAndResolveCard()`:

```csharp
private void BeginMonsterTurn()
{
    ResolveSutureRepair();   // ← Insert before card draw
    DrawAndResolveCard();
}
```

---

## Node-Based Defeat Condition

The Suture uses the same node system built for Siltborn in Stage 9-M.
Set the following in Monster_Suture.asset:

```
victoryNodePartNames: ["Suture Core A", "Suture Core B", "Central Mass"]
```

`CheckOverlordVictory()` (already implemented in Stage 9-M) will handle the rest:
all three named parts must be at 0 Flesh before `OnOverlordDefeated()` fires.

**Design note:** The Limb Clusters can be destroyed without ending the hunt — they simply disable self-repair. Hunters must push through to the Cores.

---

## Parts

| Part | Shell HP | Flesh HP | Break Effect |
|---|---|---|---|
| Left Limb Cluster | 4 | 10 | Breaking disables self-repair from left-side attacks; if Right also broken → all repair stops |
| Right Limb Cluster | 4 | 10 | Breaking disables self-repair from right-side attacks; if Left also broken → all repair stops |
| Suture Core A | 5 | 12 | **Defeat node** — must reach 0 Flesh to kill the Suture |
| Suture Core B | 5 | 12 | **Defeat node** — must reach 0 Flesh to kill the Suture |
| Central Mass | 8 | 15 | **Defeat node** — largest and most protected target |

**Phase 2 trigger HP calculation:**
- Combined Core HP = 12 + 12 + 15 = 39
- 40% threshold = 15.6 → trigger when combined remaining Core HP ≤ 15
- Implement: sum `currentFleshHP` of all three Cores; if ≤ 15 and not yet in Phase 2 → trigger

Add this check to the phase transition logic in `CombatManager`:

```csharp
private void CheckSuturePhaseTransition()
{
    if (_activeMonster.monsterName != "The Suture") return;
    if (_inPhase2) return;

    int coreHP = 0;
    foreach (var name in new[] { "Suture Core A", "Suture Core B", "Central Mass" })
    {
        var p = FindPart(name);
        if (p != null) coreHP += p.currentFleshHP;
    }

    // Combined Core max = 39; 40% = ~15
    if (coreHP <= 15)
    {
        Debug.Log("[Combat] Suture — Phase 2 triggered. Entering Frenzy Form.");
        _inPhase2   = true;
        _activeDeck = _activeMonster.phase2Deck;
        ShuffleDeck();
        _hud?.ShowPhaseBanner("FRENZY FORM");
        GameStateManager.Instance.AddChronicleEntry(
            GameStateManager.Instance.CampaignState.currentYear,
            "The Suture tore open. What came together again was faster. Angrier.");
    }
}
```

Call `CheckSuturePhaseTransition()` anywhere the generic `CheckPhaseTransition()` is called for overlords, but only when the active monster is The Suture.

---

## Phase 1 Deck (18 cards)

Create in `Assets/_Game/Data/Monsters/Suture/BehaviorCards/Phase1/`.

| cardId | cardName | Notes |
|---|---|---|
| `SUT-P1-01` | Drag Forward | Move 2 toward aggro. No attack. |
| `SUT-P1-02` | Limb Slash | No move. 3 Flesh to aggro. |
| `SUT-P1-03` | Suture Grasp | No move. 3 Flesh to aggro. Aggro target gains Pinned (cannot move next turn). |
| `SUT-P1-04` | Self-Stitch | No move. Restore 4 Flesh to most-damaged non-Core part. Trigger: at least one Limb Cluster intact. |
| `SUT-P1-05` | Cluster Slam | Move 1. 4 Flesh to aggro. Trigger: Left Limb Cluster intact. |
| `SUT-P1-06` | Right Sweep | No move. 2 Flesh to all hunters within 1 space. Trigger: Right Limb Cluster intact. |
| `SUT-P1-07` | Haul | Move 3 toward nearest hunter. No attack. |
| `SUT-P1-08` | Mass Pull | No move. Move aggro target 2 spaces toward The Suture (forced reposition). |
| `SUT-P1-09` | Rend | No move. 5 Flesh to aggro. isShuffle: true |
| `SUT-P1-10` | Corruption Seep | No move. All hunters within 2 spaces gain Shaken. |
| `SUT-P1-11` | Weight of Dead | No move. Aggro shifts to hunter with highest Flesh. |
| `SUT-P1-12` | Bind | No move. 2 Flesh to aggro. Aggro target cannot Evade next attack against them. |
| `SUT-P1-13` | Body Memory | No move. Restore 3 Flesh to the two most-damaged non-Core parts. isShuffle: true |
| `SUT-P1-14` | Stitch Burst | No move. 2 Flesh to all adjacent hunters. |
| `SUT-P1-15` | Heavy Drag | Move 1. 3 Flesh to aggro. Target gains Bleeding (2 Flesh/rd, 2 rds). |
| `SUT-P1-16` | Pivot Slam | No move. Aggro shifts to lowest-Grit hunter. 3 Flesh to that hunter. |
| `SUT-P1-17` | Absorb | No move. If any hunter is at 0 Flesh (dead this round): restore 5 Flesh to Central Mass. |
| `SUT-P1-18` | Pressure | No move. 3 Flesh to aggro. All hunters adjacent to aggro take 1 Flesh. isShuffle: true |

**Notes on trigger cards:**
- `SUT-P1-04` (Self-Stitch): fires the self-repair for 4 Flesh (instead of the passive 2 Flesh per turn); only if a Limb Cluster is intact
- `SUT-P1-13` (Body Memory): also manual repair but hits two parts; isShuffle forces new deck soon — hunters should break Clusters before this reshuffles
- `SUT-P1-17` (Absorb): punishes the party for losing a hunter mid-hunt; rare but brutal in long fights

---

## Phase 2 Deck (12 cards — Frenzy Form)

Create in `Assets/_Game/Data/Monsters/Suture/BehaviorCards/Phase2/`.

At Phase 2, self-repair stops (both Clusters assumed broken by this point, or the mechanic force-disables). The Suture becomes purely aggressive.

| cardId | cardName | Notes |
|---|---|---|
| `SUT-P2-01` | Frenzy Rend | No move. 6 Flesh to aggro. Cannot be Evaded. isShuffle: true |
| `SUT-P2-02` | Mass Slam | Move 1. 3 Flesh to all hunters within 2 spaces. |
| `SUT-P2-03` | Final Stitch | No move. Restore 5 Flesh to the lowest-HP surviving Suture Core. Fires at most once per hunt. isShuffle: true |
| `SUT-P2-04` | Rupture | No move. 4 Flesh to aggro + 2 Flesh to all hunters adjacent to aggro. |
| `SUT-P2-05` | Scatter | No move. Knockback 2 on ALL hunters simultaneously. |
| `SUT-P2-06` | Marrow Pull | No move. All hunters gain −1 Grit for 2 rounds. |
| `SUT-P2-07` | Tendon Tear | No move. 5 Flesh to aggro. Target gains Crippled (cannot use movement cards for 1 round). |
| `SUT-P2-08` | Consume | No move. 8 Flesh to aggro. Cannot be Evaded. isShuffle: true |
| `SUT-P2-09` | Body Horror | No move. All hunters must succeed a Grit check (roll d10 + Grit ≥ 8) or gain a random Disorder. |
| `SUT-P2-10` | Crawl Over | Move 2. 2 Flesh to all hunters in movement path. |
| `SUT-P2-11` | Last Seam | No move. 5 Flesh to all hunters adjacent to The Suture. Cannot be Evaded. isShuffle: true |
| `SUT-P2-12` | The Weight of It | No move. 7 Flesh to aggro. Target permanently loses 1 Grit (post-hunt penalty). isShuffle: true |

**Notes on Phase 2 mechanics:**
- `SUT-P2-03` (Final Stitch): the one last-gasp repair in Phase 2 — makes hunters urgently finish the Core they were targeting; mark a bool `_sutureP2StitchFired` so it only activates once
- `SUT-P2-12` (The Weight of It): permanent Grit loss survives the hunt, similar to Pale Stag's White Silence; it should be applied via `GameStateManager.GrantPermanentDebuff(hunterId, "−1 Grit")`

---

## The Suture MonsterSO — Fill Existing Skeleton

The asset already exists at:
`Assets/_Game/Data/Monsters/Monster_Suture.asset`

Open it in the Unity Inspector and set these fields (or run an Editor script to populate):

```
monsterName: The Suture
monsterType: Overlord — Body-Horror Accumulation
isOverlord: true
hasAscendantForm: false
ascendantFormHP: 0
difficulty: Nightmare
huntYearMin: 15
huntYearMax: 25
prerequisiteOverlordKillCount: 0   ← No kill prerequisite

Parts:
  Left Limb Cluster:  shellHP=4, fleshHP=10
  Right Limb Cluster: shellHP=4, fleshHP=10
  Suture Core A:      shellHP=5, fleshHP=12
  Suture Core B:      shellHP=5, fleshHP=12
  Central Mass:       shellHP=8, fleshHP=15

victoryNodePartNames: ["Suture Core A", "Suture Core B", "Central Mass"]
phase1Deck: [SUT-P1-01 through SUT-P1-18]
phase2Deck: [SUT-P2-01 through SUT-P2-12]
rewardCodexEntryId: CodexEntry_TheSuture
rewardCraftSetId: Sinew
startingAggroTarget: Hunter with highest Flesh (most life to drain)
```

---

## Sinew Craft Set

The Suture yields sinew — stitched hide and recovered body material. The Sinew craft set has a unique link bonus not covered by any other set: **+1 maximum Flesh HP per linked pair**.

### CraftSetSO Asset

Create `Assets/_Game/Data/CraftSets/Sinew_CraftSet.asset`:

```
setId: Sinew
setName: Sinew Works
linkBonusDescription: +1 max Flesh HP per linked pair of Sinew gear
perLinkMaxFleshHP: 1        ← New field on CraftSetSO (see below)
fullSetBonus: Once per hunt, when a hunter would drop to 0 Flesh, they instead drop to 1 Flesh (survives the blow)
```

### Add perLinkMaxFleshHP to CraftSetSO

```csharp
[Header("Per-Link Bonus")]
public int perLinkAccuracy    = 0;
public int perLinkEvasion     = 0;
public int perLinkToughness   = 0;
public int perLinkSpeed       = 0;
public int perLinkGrit        = 0;
public int perLinkLuck        = 0;
public int perLinkMaxFleshHP  = 0;   // ← Add this field
```

### Apply in AdjacencyBonusCalculator

In `AdjacencyBonusCalculator.Calculate()`, after counting linked pairs per set:

```csharp
// Existing stat bonuses
bonus.accuracy  += set.perLinkAccuracy  * pairCount;
bonus.evasion   += set.perLinkEvasion   * pairCount;
bonus.toughness += set.perLinkToughness * pairCount;
bonus.speed     += set.perLinkSpeed     * pairCount;
bonus.grit      += set.perLinkGrit      * pairCount;
bonus.luck      += set.perLinkLuck      * pairCount;

// New: max Flesh HP bonus
bonus.maxFleshHP += set.perLinkMaxFleshHP * pairCount;   // ← Add this line
```

### Add maxFleshHP to StatBonus

```csharp
public class StatBonus
{
    public int accuracy    = 0;
    public int evasion     = 0;
    public int toughness   = 0;
    public int speed       = 0;
    public int grit        = 0;
    public int luck        = 0;
    public int maxFleshHP  = 0;   // ← Add this field

    public bool IsEmpty =>
        accuracy == 0 && evasion == 0 && toughness == 0 &&
        speed == 0 && grit == 0 && luck == 0 && maxFleshHP == 0;
}
```

### Apply maxFleshHP Bonus in Combat

In `CombatManager`, when initializing a hunter's effective max Flesh for the hunt:

```csharp
// After calculating adjacency bonuses:
int effectiveMaxFlesh = hunter.baseFleshHP + linkBonus.maxFleshHP;
_combatState.hunterMaxFlesh[hunter.hunterId] = effectiveMaxFlesh;

// Current Flesh does NOT exceed effectiveMaxFlesh (bonus extends the pool, not instant heal)
_combatState.hunterCurrentFlesh[hunter.hunterId] =
    Mathf.Min(_combatState.hunterCurrentFlesh[hunter.hunterId], effectiveMaxFlesh);
```

### Sinew Gear Items

Create in `Assets/_Game/Data/Gear/Sinew/`:

| gearId | gearName | Slot | Stats | Special | Resource Cost |
|---|---|---|---|---|---|
| `SIN-01` | Sinew Hood | Head | +1 TOU, +1 Flesh | — | 4 Suture Hide, 2 Bone |
| `SIN-02` | Stitched Coat | Chest | +2 TOU, +2 Flesh | — | 6 Suture Hide, 3 Bone, 1 Ichor |
| `SIN-03` | Bind Wraps | Hands | +1 EVA, +1 Flesh | — | 3 Suture Hide, 1 Sinew |
| `SIN-04` | Bone-Stitch Blade | Weapon | +2 Accuracy, 3 base damage | On wound: deal 1 extra to adjacent hunter (chain damage, friend or foe) | 4 Suture Hide, 3 Bone, 2 Sinew |
| `SIN-05` | Suture Ward | Off-hand | +2 TOU | When breaking a Shell: restore 1 Flesh to wielder | 4 Suture Hide, 2 Bone, 1 Sinew |
| `SIN-06` | Marrow Cord | Amulet | +1 Grit, +1 Luck | Full Sinew set: once per hunt, when dropped to 0 Flesh, survive with 1 Flesh | 2 Suture Hide, 2 Sinew, 1 Bone |

**Resource name note:** Add "Suture Hide" and "Sinew" as new resource types to `ResourceType` enum (or string-keyed resource dictionary). These are Suture-specific materials.

### Sinew set link bonus summary:
- 1 pair linked: +1 max Flesh HP
- 2 pairs linked: +2 max Flesh HP
- 3 pairs linked: +3 max Flesh HP (full 3×3 grid with 3+ Sinew pieces)
- Full set bonus (all 6 pieces): death-survival mechanic (once per hunt)

---

## Codex Entry

Create `Assets/_Game/Data/Codex/CodexEntry_TheSuture.asset`:

```
entryId: CodexEntry_TheSuture
entryTitle: The Suture
entryBody:
"The Suture did not fight the way animals fight. It fought the way grief fights —
accumulated, patient, wearing the shapes of things you've lost.
The hunters who broke its cores say the third one bled something that wasn't blood.
We've written down what they described. We're not going to include it here."

unlockedByDefault: false
unlockCondition: kill The Suture (via OnOverlordDefeated → UnlockCodexEntry)
```

---

## Updated Overlord & Monster Counts

After this session, the complete roster is:

**Standard Monsters (8 total):**
| Monster | Stage |
|---|---|
| The Gaunt | Stage 7 (fully built) |
| Thornback | Stage 9-F |
| The Ivory Stampede | Stage 9-G |
| Bog Caller | Stage 9-H |
| The Shriek | Stage 9-I |
| The Rotmother | Stage 9-J |
| The Gilded Serpent | Stage 9-K |
| The Ironhide (The Spite) | Stage 9-L |

**Overlords (4 total):**
| Overlord | Stage | Year Range |
|---|---|---|
| The Siltborn | Stage 9-M | Years 8–16 |
| The Penitent | Stage 9-N | Years 12–22 |
| The Suture | Stage 9-S (this file) | Years 15–25 |
| The Pale Stag Ascendant | Stage 9-O | Years 25–30 |

---

## Verification Test

- [ ] Monster_Suture.asset updated with all 5 parts, 18 Phase 1 cards, 12 Phase 2 cards
- [ ] Self-repair fires at start of each monster turn (2 Flesh to most-damaged non-Core part)
- [ ] Self-repair log: `[Combat] Suture self-repairs 2 Flesh on Left Limb Cluster. Now 8/10.`
- [ ] Breaking Left Limb Cluster alone → repair still fires (Right intact)
- [ ] Breaking BOTH Limb Clusters → repair stops (`[Combat] Suture — both Limb Clusters broken. Self-repair disabled.`)
- [ ] SUT-P1-04 (Self-Stitch) fires → 4 Flesh restored to most-damaged part (not Core); disabled if both Clusters broken
- [ ] Phase 2 triggers when combined Core HP (Suture Core A + B + Central Mass) ≤ 15
- [ ] Phase 2 deck swap fires; "FRENZY FORM" banner appears
- [ ] In Phase 2: self-repair mechanic no longer fires at turn start
- [ ] SUT-P2-03 (Final Stitch) fires once; second draw does nothing (`_sutureP2StitchFired`)
- [ ] SUT-P2-12 (The Weight of It) → aggro hunter permanently loses 1 Grit after hunt
- [ ] All 3 Cores at 0 → OnOverlordDefeated fires
- [ ] CodexEntry_TheSuture unlocked in CampaignState
- [ ] Sinew craft set unlocked and appears in Settlement crafting panel
- [ ] Sinew items in crafting panel: correct costs, +Flesh stat displayed in Inspector
- [ ] Sinew link bonus: 3 Sinew pieces placed adjacently → hunter's max Flesh increases by 2 (2 orthogonal pairs)
- [ ] Full Sinew set (all 6 pieces): death-survival mechanic fires when hunter drops to 0 (once per hunt)
- [ ] CanHuntMonster(Suture): false before Year 15, true Year 15–25, false Year 26+
- [ ] No prerequisiteOverlordKillCount gate (Suture available to any settlement in Year 15–25)

---

## Post-Session: Update STAGE_09_R Checklist

After this session completes, re-open `STAGE_09_R.md` and update these items:

- Monster count: 8 standard + 4 overlords (was listed as "10 monsters" — confirm this now reads correctly)
- Behavior card count: add "The Suture: 18 Phase 1 + 12 Phase 2"
- Gear items: update from 42 to **48** (42 existing + 6 Sinew items)
- Codex entries: update from 15 to **16** (add CodexEntry_TheSuture)
- Craft sets: update from 7 to **8** (add Sinew)
- StatBonus fields: confirm `maxFleshHP` added and appears in adjacency tooltip in Settlement UI
- 30-year smoke test: add Year 15–25 Suture hunt to the Year 11–19 block

---

## Session End

This session completes the Suture overlord design. The four overlords are now fully defined:

1. The Siltborn (Years 8–16) — node-based, Silt Breath, Siltwater hazard
2. The Penitent (Years 12–22) — self-harm, desperation scaling
3. The Suture (Years 15–25) — self-repair, node cores, Sinew craft set
4. The Pale Stag Ascendant (Years 25–30) — Ascendant Form, AoE Phase 2, Victory trigger

After running the verification checklist above, re-run the STAGE_09_R.md Final Integration DoD to confirm the full campaign is complete.
