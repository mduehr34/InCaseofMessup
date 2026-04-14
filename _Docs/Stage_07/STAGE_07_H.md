<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-H | Weapon Cards — Fist Weapons & Spear (All Tiers)
Status: Stage 7-G complete. Gaunt behavior cards verified.
Full Gaunt fight runs with real SO assets.
Task: Create ALL ActionCardSO assets for Fist Weapons
(Tier 1–5, 18 cards) and Spear (Tier 1–5, 18 cards).
Fist Weapons are needed immediately for Tutorial testing.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_H.md
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- Asset naming: [WeaponType]_T[N]_[CardName]
  e.g. Fist_T1_Brace, Spear_T2_Interceptor
- Bare fist cards have weaponType = FistWeapon
- All cards from GDD Section 07 are faithfully reproduced
- What you will NOT create this session (other weapon types)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-H: Weapon Cards — Fist Weapons & Spear (All Tiers)

**Resuming from:** Stage 7-G complete  
**Done when:** All 36 Fist Weapon and Spear ActionCardSO assets created; Brace and Shove appear in Aldric's starting hand in play; WeaponSO assets updated  
**Commit:** `"7H: Fist Weapon and Spear ActionCardSO assets — all tiers"`  
**Next session:** STAGE_07_I.md  

---

## Save Paths

- `Assets/_Game/Data/Cards/Action/FistWeapon/`
- `Assets/_Game/Data/Cards/Action/Spear/`

---

## Fist Weapons — 18 Cards Across 5 Tiers

All fist weapon cards: `weaponType = FistWeapon`, `isLoud = false` unless noted.

### Tier 1 — 4 Cards (Starting cards — all hunters begin here)

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Fist_T1_Brace` | Brace | Reaction | 0 | 0 | When you take damage, reduce that damage by 2 Shell or 1 Flesh. Declare before damage resolves. |
| `Fist_T1_Shove` | Shove | BasicAttack | 1 | 0 | No weapon damage. Push monster 1 square back. Apply Shaken. |
| `Fist_T1_QuickJab` | Quick Jab | BasicAttack | 1 | 1 | Standard attack at -1 Strength. On hit: apply Shaken. Costs 0 net AP. |
| `Fist_T1_StrikeAndMove` | Strike and Move | Signature | 1 | 0 | Make a standard attack AND move up to 2 squares in any order. Both happen in one action. |

### Tier 2 — 4 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Fist_T2_GrappleOpener` | Grapple Opener | Opener | 1 | 1 | No damage. Apply Pinned to target. Starts combo. Costs 0 net AP. |
| `Fist_T2_HammerFist` | Hammer Fist | Opener | 1 | 0 | Attack at +1 Strength. Starts combo. |
| `Fist_T2_Deflect` | Deflect | Reaction | 0 | 0 | When targeted by a melee attack, reduce all damage by 1 Shell AND 1 Flesh this turn. |
| `Fist_T2_BodyBlow` | Body Blow | BasicAttack | 1 | 0 | Attack. On wound: apply Slowed. |

### Tier 3 — 4 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Fist_T3_FollowThrough` | Follow Through | Linker | 1 | 0 | Attack at +1 Strength. On hit: move 1 square free. Continues combo. |
| `Fist_T3_StaggeringBlow` | Staggering Blow | Linker | 1 | 1 | No damage. Apply Shaken AND Slowed. Continues combo. Costs 0 net AP. |
| `Fist_T3_ExposedStrike` | Exposed Strike | BasicAttack | 1 | 0 | Attack. If target part Shell is 0, apply Exposed. |
| `Fist_T3_Counterstrike` | Counterstrike | Reaction | 0 | 0 | When monster attacks you and misses, immediately make a free standard attack. |

### Tier 4 — 3 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Fist_T4_CrushingGrip` | Crushing Grip | Linker (strong) | 1 | 0 | Attack. If target is Pinned, auto-pass Force Check AND apply Exposed. Continues combo. |
| `Fist_T4_ThrowingArm` | Throwing Arm | Reaction | 0 | 0 | When monster moves adjacent, push it 2 squares back as a free reaction. |
| `Fist_T4_PrecisionBlow` | Precision Blow | BasicAttack | 1 | 0 | Attack. Crit threshold reduced by 1 this attack only. |

### Tier 5 — 3 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Fist_T5_FinalStrike` | Final Strike | Finisher | 1 | 0 | Attack at +2 Strength. On wound: apply Exposed permanently for rest of hunt. Ends combo. |
| `Fist_T5_SurvivorInstinct` | Survivor Instinct | Finisher (weak) | 1 | 1 | Gain 2 Grit. All adjacent hunters gain 1 Grit. Ends combo. Costs 0 net AP. |
| `Fist_T5_BreakingPoint` | Breaking Point | BasicAttack | 1 | 0 | Attack. On Shell break: deal 2 additional Flesh damage bypassing Shell. |

---

## Spear — 18 Cards Across 5 Tiers

All spear cards: `weaponType = Spear`, `isLoud = false`. Key passive: cannot attack adjacent targets.

### Tier 1 — 4 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Spear_T1_LongThrust` | Long Thrust | BasicAttack | 1 | 0 | Standard attack from 2 tiles. Cannot attack adjacent targets (Spear passive). |
| `Spear_T1_BracePosition` | Brace Position | Signature | 1 | 0 | Designate 2 adjacent squares as movement-denied for monster this round. You cannot move this turn. |
| `Spear_T1_Jab` | Jab | BasicAttack | 1 | 1 | Attack at -1 Strength from 2 tiles. On hit: apply Shaken. Costs 0 net AP. |
| `Spear_T1_SetSpear` | Set Spear | BasicAttack | 1 | 0 | Attack. If monster moved toward you this round, gains +1 Strength. |

### Tier 2 — 4 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Spear_T2_ReachOut` | Reach Out | Opener | 1 | 1 | Attack from 3 tiles (Tier 2 passive). Starts combo. Costs 0 net AP. |
| `Spear_T2_ZoneControl` | Zone Control | Opener (weak) | 1 | 1 | Designate 1 square as denied. No attack. Starts combo. Costs 0 net AP. |
| `Spear_T2_Interceptor` | Interceptor | Reaction | 0 | 0 | When monster moves toward any hunter, make a standard attack from 2 tiles before it arrives. |
| `Spear_T2_PinningThrust` | Pinning Thrust | BasicAttack | 1 | 0 | Attack from 2 tiles. On wound: apply Pinned. |

### Tier 3 — 4 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Spear_T3_SuppressingStrike` | Suppressing Strike | Linker | 1 | 0 | Attack from range. On hit: designate 1 square adjacent to struck part as denied. Continues combo. |
| `Spear_T3_Withdraw` | Withdraw | Linker (weak) | 1 | 1 | Move 2 squares away from monster. No attack. Continues combo. Costs 0 net AP. |
| `Spear_T3_Overextend` | Overextend | BasicAttack | 1 | 0 | Attack from 3 tiles at +1 Strength. You cannot move next turn. |
| `Spear_T3_DenyGround` | Deny Ground | BasicAttack | 1 | 0 | No attack. Designate 2 squares as movement-denied AND apply Exposed to one named part. |

### Tier 4 — 3 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Spear_T4_Impale` | Impale | Linker (strong) | 1 | 0 | Attack. If target Exposed, auto-pass Force Check. On wound: apply Bleeding. Continues combo. |
| `Spear_T4_CoverZone` | Cover Zone | Reaction | 0 | 0 | When monster enters a movement-denied square (forced by behavior card), make a free attack. |
| `Spear_T4_SweepingThrust` | Sweeping Thrust | BasicAttack | 1 | 0 | Attack. Hits primary target AND one adjacent part in a straight line. Resolve Force Check for each. |

### Tier 5 — 3 Cards

| Asset | cardName | category | apCost | apRefund | effectDescription |
|---|---|---|---|---|---|
| `Spear_T5_LineBreaker` | Line Breaker | Finisher | 1 | 0 | Attack. Hits every part in a straight line from your position. Each resolves Force Check separately. Ends combo. |
| `Spear_T5_DeadZone` | Dead Zone | Finisher (weak) | 1 | 1 | No attack. Designate a 3×1 line as movement-denied for the entire hunt. Ends combo. Costs 0 net AP. |
| `Spear_T5_Skewer` | Skewer | BasicAttack | 1 | 0 | Attack. On wound: monster cannot voluntarily move toward you for 2 rounds. |

---

## Update WeaponSO Assets

After creating all cards, update or create the WeaponSO for each:

**FistWeapon.asset:** `tier1Cards = [Fist_T1_Brace, Fist_T1_Shove, Fist_T1_QuickJab]`, `signatureCard = Fist_T1_StrikeAndMove`  
**Spear.asset:** `tier1Cards = [Spear_T1_LongThrust, Spear_T1_BracePosition, Spear_T1_Jab, Spear_T1_SetSpear]`, `signatureCard = Spear_T1_BracePosition`

---

## Verification Test

1. Play combat with Aldric (FistWeapon proficiency T1)
2. Vitality Phase: hand contains Brace and Shove (starting deck)
3. Play Brace as a reaction — no Precision/Force Check fires
4. Play Shove — monster pushes back 1 square, Shaken applied, Debug.Log confirms
5. Proficiency activations increment on each successful weapon use

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_I.md`  
**Covers:** Axe, Hammer/Maul, Dagger (GDD Appendix A.9), and Sword & Shield — all tiers
