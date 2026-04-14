<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-J | Weapon Cards — Greatsword & Bow (All Tiers)
Status: Stage 7-I complete. Axe, Hammer, Dagger, Sword done.
Task: Create all ActionCardSO assets for Greatsword (18 cards)
and Bow (18 cards) across all 5 tiers. This completes all
8 weapon types.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_J.md
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-J: Weapon Cards — Greatsword & Bow (All Tiers)

**Resuming from:** Stage 7-I complete  
**Done when:** All 36 Greatsword and Bow cards created; all 8 weapon WeaponSO assets populated with correct tier card arrays  
**Commit:** `"7J: Greatsword and Bow card assets — all weapon types complete"`  
**Next session:** STAGE_07_K.md  

---

## Save Paths

- `Assets/_Game/Data/Cards/Action/Greatsword/`
- `Assets/_Game/Data/Cards/Action/Bow/`

---

## Greatsword — GDD Section 07

Key passives: Hits TWO parts simultaneously (facing-determined). Both resolve Force Check on same roll.

**Tier 1 — 4 Cards**

| Asset | cardName | category | apCost | refund | effectDescription |
|---|---|---|---|---|---|
| `Greatsword_T1_SweepingCut` | Sweeping Cut | BasicAttack | 1 | 0 | Attack. Hits two parts simultaneously (facing-determined). Both resolve Force Check on same roll. |
| `Greatsword_T1_MeasuredSwing` | Measured Swing | BasicAttack (weak) | 1 | 1 | Attack one part only at -1 Strength. Costs 0 net AP. |
| `Greatsword_T1_BrutalArc` | Brutal Arc | BasicAttack | 1 | 0 | Attack. Both struck parts take +1 Strength damage on arc hit. |
| `Greatsword_T1_WideArc` | Wide Arc | Signature | 1 | 0 | Attack primary target AND one adjacent part simultaneously. Both resolve Force Check. |

**Tier 2 — 4 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Greatsword_T2_DrivingOpener` | Driving Opener | Opener | Attack primary part. Push monster 1 square. Starts combo. |
| `Greatsword_T2_ClearTheField` | Clear the Field | Opener (weak) | No damage. Move 2 squares in any direction free. Starts combo. Costs 0 net AP. |
| `Greatsword_T2_InterruptingSlash` | Interrupting Slash | Reaction | When monster targets an adjacent hunter: make a free standard sweep attack (2 parts). |
| `Greatsword_T2_SweepingStrike` | Sweeping Strike | BasicAttack | Attack. Hits 3 parts in arc on crit (normal = 2). |

**Tier 3 — 4 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Greatsword_T3_MomentumLinker` | Momentum Linker | Linker | Attack 2 parts. If both parts hit, apply Slowed. Continues combo. |
| `Greatsword_T3_GroundControl` | Ground Control | Linker (weak) | No attack. Designate 3 squares as movement-denied. Continues combo. Costs 0 net AP. |
| `Greatsword_T3_ArcOfDespair` | Arc of Despair | BasicAttack | Attack. Both struck parts take +1 Flesh damage on wound. |
| `Greatsword_T3_PressureStrike` | Pressure Strike | BasicAttack | Attack. On Shell break on either part: apply Exposed to the other part. |

**Tier 4 — 3 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Greatsword_T4_ExecutionArc` | Execution Arc | Linker (strong) | Attack 2 parts. If either part Exposed: auto-pass Force Check on that part. Continues combo. |
| `Greatsword_T4_Riposte` | Riposte | Reaction | When you take damage: immediately make a free Sweeping Cut. |
| `Greatsword_T4_DoubleDown` | Double Down | BasicAttack | Attack same part twice. Second hit gains +1 Strength if first hit succeeded. |

**Tier 5 — 3 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Greatsword_T5_TidalStrike` | Tidal Strike | Finisher | Attack. Hits ALL parts in front arc. Each resolves Force Check separately. Ends combo. |
| `Greatsword_T5_LastSweep` | Last Sweep | Finisher (weak) | No attack. All hunters adjacent to monster gain 3 Grit. Ends combo. Costs 0 net AP. |
| `Greatsword_T5_TheEndingBlow` | The Ending Blow | BasicAttack | Attack 2 parts. On wound on either: that part becomes permanently Exposed. |

---

## Bow — GDD Section 07

Key passives: Range 3–6 tiles. Cannot attack if monster is adjacent. Proximity behavior cards never trigger from this hunter.

**Tier 1 — 4 Cards**

| Asset | cardName | category | apCost | refund | effectDescription |
|---|---|---|---|---|---|
| `Bow_T1_LoosedArrow` | Loosed Arrow | BasicAttack | 1 | 0 | Standard ranged attack from 3–6 tiles. Cannot attack adjacent targets. |
| `Bow_T1_NockAndDraw` | Nock and Draw | BasicAttack (weak) | 1 | 1 | Ranged attack at -1 Accuracy. On hit: apply Shaken. Costs 0 net AP. |
| `Bow_T1_ControlledShot` | Controlled Shot | BasicAttack | 1 | 0 | Ranged attack. Choose target part specifically (no facing table — player picks). |
| `Bow_T1_MarkedTarget` | Marked Target | Signature | 1 | 0 | No damage. Mark a part — all attacks against it gain +1 Accuracy this round. |

**Tier 2 — 4 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Bow_T2_PinningShot` | Pinning Shot | Opener | Ranged attack. On hit: apply Pinned. Starts combo. |
| `Bow_T2_RapidFire` | Rapid Fire | Opener (weak) | Make 2 ranged attacks at -1 Accuracy each. Starts combo. Costs 0 net AP. |
| `Bow_T2_DisengageShot` | Disengage Shot | Reaction | When monster moves adjacent: move 3 squares away free AND make a ranged attack. |
| `Bow_T2_ExposedFlank` | Exposed Flank | BasicAttack | Ranged attack. If attacking from Rear arc: gains +2 Accuracy bonus. |

**Tier 3 — 4 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Bow_T3_SuppressingFire` | Suppressing Fire | Linker | Ranged attack. On hit: monster cannot target you next Monster Phase. Continues combo. |
| `Bow_T3_TacticalRetreat` | Tactical Retreat | Linker (weak) | Move 4 squares away free. No attack. Continues combo. Costs 0 net AP. |
| `Bow_T3_HeadShot` | Head Shot | BasicAttack | Ranged attack. If target is Head part: crit threshold -1 this attack. |
| `Bow_T3_VitalShot` | Vital Shot | BasicAttack | Ranged attack. On wound: the wounded part loses 1 additional Flesh. |

**Tier 4 — 3 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Bow_T4_ArrowBarrage` | Arrow Barrage | Linker (strong) | Make 3 ranged attacks at separate parts at -1 Accuracy each. Continues combo. |
| `Bow_T4_CoverFire` | Cover Fire | Reaction | When any hunter targeted: make a free ranged attack against monster (may distract). |
| `Bow_T4_ExecutionShot` | Execution Shot | BasicAttack | Ranged attack. If target Exposed AND part Shell=0: auto-crit. |

**Tier 5 — 3 Cards**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Bow_T5_FinalVolley` | Final Volley | Finisher | Make ranged attacks against ALL parts. Each resolves separately. Ends combo. |
| `Bow_T5_SignalFlare` | Signal Flare | Finisher (weak) | No damage. All hunters gain 1 Grit AND next round all Reaction cards cost 0 AP. Ends combo. Costs 0 net AP. |
| `Bow_T5_DeadEye` | Dead Eye | BasicAttack | Ranged attack. Ignore all accuracy penalties this attack (Shaken, arc, etc.). On crit: apply Exposed. |

---

## Verify All 8 WeaponSO Assets

After all cards are created, confirm each WeaponSO has:
- Correct `tier1Cards` through `tier5Cards` arrays populated
- Correct `signatureCard` reference
- Correct `isAlwaysLoud` (only HammerMaul = true)
- Correct `range` (Bow = 3–6, Spear = 2, all others = 0/adjacent)

---

## Verification Test

- [ ] All 36 card assets exist
- [ ] All 8 WeaponSO assets populated with complete tier arrays
- [ ] Equipping Bow on a hunter: Proximity behavior cards should not trigger
- [ ] Equipping Greatsword: Sweeping Cut hits 2 parts on both Precision and Force checks

---

## Next Session: STAGE_07_K.md
**Covers:** Chronicle Events EVT-01 through EVT-15 as EventSO assets

---
---

<!-- ============================================================
     STAGE 7-K
     ============================================================ -->

<!-- SESSION PROMPT
▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-K | Chronicle Events EVT-01 through EVT-15
Status: Stage 7-J complete. All 8 weapon types done.
Task: Create EventSO assets for all 15 events from
GDD Years 1–12. EVT-01 is mandatory and must fire Year 1.
All events must have correct yearRangeMin/Max and tags.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_K.md
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ -->

