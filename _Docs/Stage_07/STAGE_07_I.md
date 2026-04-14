<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 7-I | Weapon Cards — Axe, Hammer/Maul, Dagger, Sword & Shield
Status: Stage 7-H complete. Fist Weapon and Spear card assets
done. WeaponSO assets updated. Tutorial combat verified.
Task: Create all ActionCardSO assets for Axe (18 cards),
Hammer/Maul (18 cards), Dagger (18 cards from GDD A.9),
and Sword & Shield (18 cards) across all 5 tiers each.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_07/STAGE_07_I.md
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- All Hammer/Maul cards have isLoud = true
- Dagger cards exactly match GDD Appendix A.9
- What you will NOT create this session (Greatsword, Bow)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 7-I: Weapon Cards — Axe, Hammer/Maul, Dagger, Sword & Shield

**Resuming from:** Stage 7-H complete  
**Done when:** 72 ActionCardSO assets created (4 weapons × 18 cards each); all WeaponSO assets updated with correct tier arrays  
**Commit:** `"7I: Axe, Hammer/Maul, Dagger, Sword & Shield card assets — all tiers"`  
**Next session:** STAGE_07_J.md  

---

## Save Paths

- `Assets/_Game/Data/Cards/Action/Axe/`
- `Assets/_Game/Data/Cards/Action/HammerMaul/`
- `Assets/_Game/Data/Cards/Action/Dagger/`
- `Assets/_Game/Data/Cards/Action/SwordAndShield/`

---

## Dagger — GDD Appendix A.9 (Canonical — reproduce exactly)

Key passives: Full Accuracy bonus from Rear arc. Crit threshold permanently -1 at all tiers. -2 Accuracy penalty from Front arc. `isLoud = false` for all.

Asset naming: `Dagger_T[N]_[CardName]`

**Tier 1 — 4 Cards:** `Dagger_T1_QuickSlash` (×2 — intentional duplicate), `Dagger_T1_GlancingCut`, `Dagger_T1_ShadowStep` (Signature)  
**Tier 2 — 4 Cards:** `Dagger_T2_FirstBlood` (Opener), `Dagger_T2_Feint` (Opener weak), `Dagger_T2_Vanish` (Reaction), `Dagger_T2_FindTheGap`  
**Tier 3 — 4 Cards:** `Dagger_T3_TwistTheBlade` (Linker), `Dagger_T3_BleedShadow` (Linker weak), `Dagger_T3_DoubleStrike`, `Dagger_T3_SlipBehind`  
**Tier 4 — 3 Cards:** `Dagger_T4_Rupture` (Linker strong), `Dagger_T4_GhostStep` (Reaction), `Dagger_T4_Precision`  
**Tier 5 — 3 Cards:** `Dagger_T5_KillingEdge` (Finisher), `Dagger_T5_DeathMark` (Finisher weak), `Dagger_T5_ShadowFlurry`

> ⚑ Full effect descriptions are in GDD Appendix A.9. Copy them exactly — do not paraphrase.

---

## Axe — GDD Section 07

Key passives: Shell hits count as 2 Shell damage. No Flesh modifier. `isAlwaysLoud = true` on WeaponSO. Asset naming: `Axe_T[N]_[CardName]`

**Tier 1 — 4 Cards:**

| Asset | cardName | category | apCost | refund | isLoud | effectDescription |
|---|---|---|---|---|---|---|
| `Axe_T1_ShieldSplitter` | Shield Splitter | BasicAttack | 1 | 0 | Yes | Attack. Shell hit counts as 2 Shell damage (Axe passive). |
| `Axe_T1_Cleave` | Cleave | BasicAttack | 1 | 0 | Yes | Attack primary part AND one adjacent part. Resolve both separately. |
| `Axe_T1_HeavySwing` | Heavy Swing | BasicAttack (weak) | 1 | 1 | Yes | Attack at +1 Strength. Costs 0 net AP. |
| `Axe_T1_ChopAndBeat` | Chop and Beat | Signature | 1 | 0 | Yes | Attack the same part twice. Resolve each hit separately. |

**Tier 2 — 4 Cards:**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Axe_T2_ArmbreakOpener` | Armbreak Opener | Opener | Attack. On Shell break: remove 1 additional behavior card (player chooses which). Starts combo. |
| `Axe_T2_WeighIn` | Weigh In | Opener (weak) | No attack. Move 1 square free. Starts combo. Costs 0 net AP. |
| `Axe_T2_ReactiveChop` | Reactive Chop | Reaction | When any part breaks mid-combat: make a free standard attack against that part immediately. |
| `Axe_T2_CrackTheShell` | Crack the Shell | BasicAttack | Attack. On Shell break: deal 1 Flesh damage bypassing remaining Shell. |

**Tier 3 — 4 Cards:**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Axe_T3_Splinter` | Splinter | Linker | Attack. If target part Shell > 0: Shell damage counts as ×2. Continues combo. |
| `Axe_T3_GrindDown` | Grind Down | Linker (weak) | No damage. Apply Slowed. Continues combo. Costs 0 net AP. |
| `Axe_T3_ShatteringBlow` | Shattering Blow | BasicAttack | Attack. On Shell break: remove 1 behavior card of your choice from the monster deck. |
| `Axe_T3_RelentlessChop` | Relentless Chop | BasicAttack | Attack the same part twice at -1 Strength each. Resolve separately. |

**Tier 4 — 3 Cards:**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Axe_T4_ExposingSplit` | Exposing Split | Linker (strong) | Attack. If Shell = 0: apply Exposed AND deal 1 Flesh bypass. Continues combo. |
| `Axe_T4_Reverb` | Reverb | Reaction | When any part breaks: make a free attack against one adjacent part immediately. |
| `Axe_T4_ArmourBane` | Armour Bane | BasicAttack | Attack. Shell damage counts as ×2 this attack (regardless of other modifiers). |

**Tier 5 — 3 Cards:**

| Asset | cardName | category | effectDescription |
|---|---|---|---|
| `Axe_T5_Demolish` | Demolish | Finisher | Attack. If part is Exposed: deal double Flesh damage on wound. Ends combo. |
| `Axe_T5_LastRites` | Last Rites | Finisher (weak) | No attack. Grant 2 Grit to all hunters. Ends combo. Costs 0 net AP. |
| `Axe_T5_OverkillBlow` | Overkill Blow | BasicAttack | Attack. If Shell break occurs mid-hit: deal full Flesh damage in addition to Shell break. |

---

## Hammer/Maul — GDD Section 07

Key passives: Every attack is Loud regardless of card. On Shell hit: deal 1 Flesh damage (partial bypass). -1 Movement while equipped. `isLoud = true` on ALL cards. Asset naming: `Hammer_T[N]_[CardName]`

**Tier 1:** `Hammer_T1_ThunderingBlow` (BasicAttack), `Hammer_T1_WindUp` (Opener, +2 Str bonus next attack), `Hammer_T1_Shove` (Opener weak, push 2 + Shaken), `Hammer_T1_Brace` (Reaction, reduce damage)  
**Tier 2:** `Hammer_T2_GroundSlam` (Linker, +1 Str, on wound Slowed), `Hammer_T2_Momentum` (Linker weak, move 3 straight, hunters in path Evasion 5 or Shaken), `Hammer_T2_SmashThrough` (BasicAttack, if Shell=0 deal +1 Flesh), `Hammer_T2_Stagger` (BasicAttack, on wound: target loses next Move)  
**Tier 3:** `Hammer_T3_SeeingRed` (Opener, +2 Str all attacks this turn, ends on miss), `Hammer_T3_BuildingForce` (Opener weak, +1 Str stacks until used), `Hammer_T3_Aftershock` (Reaction, monster moves adjacent: free attack), `Hammer_T3_EarthShaker` (BasicAttack, attack all parts in adjacent row)  
**Tier 4:** `Hammer_T4_PileDriver` (Linker strong, Exposed: auto-pass + double Flesh), `Hammer_T4_BraceForImpact` (Reaction, reduce all damage by 3 this turn), `Hammer_T4_BoneShatter` (BasicAttack, on Shell break: permanently Exposed)  
**Tier 5:** `Hammer_T5_Annihilate` (Finisher, double Flesh on wound), `Hammer_T5_RallyingCrash` (Finisher weak, all hunters in 3 squares +2 Grit), `Hammer_T5_Devastation` (BasicAttack, Shell hit: 2 Flesh, Flesh wound: Slowed + Shaken)

---

## Sword & Shield — GDD Section 07

Key passives: Only 1 attack per turn always. Shield absorbs 1 Shell hit per round free. No Strength modifier ever. Asset naming: `Sword_T[N]_[CardName]`

**Tier 1:** `Sword_T1_GuardedStrike`, `Sword_T1_ShieldBash` (push 1 + Shaken), `Sword_T1_ShieldBlock` (Signature/Reaction — interrupt + absorb 1 Shell hit), `Sword_T1_Parry` (Reaction — negate all damage one attack)  
**Tier 2:** `Sword_T2_RallyingDefense` (Opener, grant 1 Grit adjacent), `Sword_T2_ControlledStrike` (Opener weak), `Sword_T2_CounterThrust` (Reaction, after ShieldBlock: free attack), `Sword_T2_PressForward`  
**Tier 3:** `Sword_T3_FormationStrike` (Linker, if adjacent hunter: both attack free), `Sword_T3_HoldTheLine` (Linker weak, denied + Pinned immunity), `Sword_T3_ShieldWall` (Reaction, adjacent hunters reduce damage 1 Shell), `Sword_T3_CoverAlly` (redirect attack from adjacent hunter to yourself)  
**Tier 4:** `Sword_T4_BreachingStrike` (Linker strong, Shell=0: +1 Flesh bypass + Exposed), `Sword_T4_IronWill` (Reaction, once per hunt: survive collapse with 1 Flesh), `Sword_T4_Advance` (attack + push + move into square)  
**Tier 5:** `Sword_T5_JusticeStrike` (Finisher, hunter collapsed: double damage), `Sword_T5_LastStand` (Finisher weak, +3 Grit all + Shell restore 1), `Sword_T5_ExecuteOrder` (3+ parts Exposed: auto-crit)

---

## Verification Test

- [ ] All 72 card assets created across 4 weapon folders
- [ ] Dagger cards exactly match GDD Appendix A.9 (open both and compare)
- [ ] All Hammer cards have `isLoud = true`
- [ ] Sword & Shield: `proficiencyTierRequired` set correctly on each card
- [ ] All WeaponSO assets have tier1–5 arrays populated

---

## Next Session

**File:** `_Docs/Stage_07/STAGE_07_J.md`  
**Covers:** Greatsword and Bow — all tiers (completes all 8 weapon types)
