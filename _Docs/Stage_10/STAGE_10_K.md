<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-K | Balance Pass — Economy, Monster HP, Encounter Pacing
Status: Stage 10-J complete. Credits scene done. All content built.
Task: This is a pure balancing session. No new features. Using
the Debug Campaign Panel (built in Stage 9-R), simulate campaign
states at Years 1, 5, 10, 15, 20, 25, and 30. Verify that:
  - Monster HP targets are met (round counts in expected ranges)
  - Resources accumulate at a satisfying pace (not trivially easy,
    not gated for too long)
  - Overlord gates feel appropriately late-game
  - Lifecycle cards (injuries, disorders) feel impactful but not
    immediately campaign-ending
  - Winning and losing both feel like they took the right amount
    of time

Document every change you make. Each change is a small targeted
edit to a SO asset or a constant in GameStateManager.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_K.md
- _Docs/Stage_07/STAGE_07_Q.md    ← original balance targets
- _Docs/Stage_09/STAGE_09_R.md    ← smoke test checklist (reference)
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Editor/DebugCampaignPanel.cs

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-K: Balance Pass — Economy, Monster HP, Encounter Pacing

**Resuming from:** Stage 10-J complete — credits scene done; all systems built
**Done when:** All balance targets in the table below pass simulation; all adjustments documented; balance commit made
**Commit:** `"10K: Balance pass — HP tuning, resource economy, encounter gates adjusted"`
**Next session:** STAGE_10_L.md

---

## Balance Targets (from Stage 7-Q)

These are the designer's intended round counts and resource economy targets. Use these as the pass/fail criteria for every test.

| Metric | Target | Acceptable Range |
|--------|--------|-----------------|
| Thornback kill — round count | 8 rounds | 6–10 |
| Standard monster (any) Year 10 — round count | 10 rounds | 8–13 |
| Overlord fight — round count | 15 rounds | 12–18 |
| Pale Stag Ascendant — round count | 18 rounds | 15–22 |
| First craft item available | Year 2–3 | Year 1–4 |
| First overlord-tier gear available | Year 8–12 | Year 6–14 |
| Hunters with 1+ injuries by Year 10 | 2 of 4 | 1–4 |
| Hunters retired by Year 20 | 1–2 | 0–3 |
| Hunters with disorders by Year 20 | 1–2 | 0–3 |
| Total hunter deaths across 30 years | 4–8 | 2–12 |

---

## Part 1 — Pre-Test Setup

Before testing, read Stage 7-Q to refresh on original HP values. Then open the game in Play Mode and load the Debug Campaign Panel (**MnM → Debug Campaign Panel**).

Run through each scenario below. For each, document:
- What the actual round count or resource count was
- Whether it passed or failed the target
- What change (if any) was made to fix it

---

## Part 2 — Monster HP Simulation

### Test Procedure for Each Monster

Use this method to estimate round counts without running the full combat:

1. Open Debug Campaign Panel
2. Set year to the monster's first available year
3. Grant 20 of each resource; craft and equip a typical mid-tier loadout for 4 hunters
4. Start a hunt against the target monster
5. Count the rounds until the monster dies under "average" play (play all cards optimally)

**Expected hunter loadout by test year:**
- Year 1: no gear (base stats only)
- Year 5: Carapace 2-piece equipped (2 hunters)
- Year 10: Carapace or Membrane 5-piece (2 hunters); basic gear (2 hunters)
- Year 15+: one full set + Mire or Ichor pieces mixed

### HP Tuning Reference Table

If a monster dies too fast (< lower bound), increase its Shell or Flesh HP. If too slow (> upper bound), decrease.

| Monster | Total HP (all parts) | Adjustment rule |
|---------|---------------------|-----------------|
| Thornback | ~55 | +4 if kills in <6 rounds; -4 if >10 |
| Bog Caller | ~60 | Same range as Thornback |
| Shriek | ~45 | Lower HP acceptable (high evasion compensates) |
| Rotmother | ~70 | +6 if <8 rounds; -6 if >13 |
| Gilded Serpent | ~65 | Scale reflection adds effective HP; lower base if needed |
| Ironhide | ~75 | Armoured; high Shell means fewer Flesh hits — may seem hard to kill |
| Ivory Stampede | ~50 (alpha + 2 flankers) | Pack HP is distributed; alpha death ends hunt |
| Siltborn | ~100 (nodes: 35+35+30) | Node system — all 3 nodes must hit 0; overlord tier |
| Penitent | ~90 | Self-harm restores some HP; effective HP lower than listed |
| Suture | ~100 (Cores: 12+12+15 + limb clusters) | Self-repair means fights last longer by design |
| Pale Stag Phase 1 | ~80 | Transition at 30% HP; Phase 1 should feel achievable |
| Pale Stag Ascendant | ~50 | Grid-irrelevant AoE; should feel desperate and short |

### How to Apply HP Changes

Open the MonsterSO asset in Unity Inspector and adjust `shellHP` and `fleshHP` on individual parts. Do NOT write editor scripts for this — direct Inspector edits are faster and clearer.

For every change, log it here in the format:
```
CHANGE: Thornback Left Flank shellHP 6 → 8  (fight ended in 5 rounds, too fast)
```

---

## Part 3 — Resource Economy Simulation

### Test Procedure

1. Set year to 1 (debug panel)
2. Grant 0 resources
3. Simulate: win 1 hunt per year with standard resource reward (`GameStateManager.GrantHuntResources`)
4. After Year 2, check: can the player afford the cheapest Carapace item?
5. After Year 8, check: is the Mire Apothecary crafting set unlocked and at least 1 item affordable?
6. After Year 15, check: can the player maintain a full 5-piece set?

### Resource Reward Constants

Resource rewards per hunt are defined in `GameStateManager`. Find the method `GrantHuntResources(string monsterName)` (or equivalent). Typical values:

```csharp
// Target values (adjust if economy is off):
// Standard hunt: 3 Bone, 2 Hide, 1 Sinew, 0 Ichor
// Successful overlord: 5 Bone, 3 Hide, 2 Sinew, 2 Ichor (+ set-specific material)
```

If the player can craft their first item before Year 2 → reduce resource rewards by 20%.
If the player cannot craft anything by Year 4 → increase by 20%.

### Craft Cost Tuning

The cheapest Carapace item (Bone Cleaver, CAR-01) should cost:
- **Target:** 3 Bone, 1 Hide — affordable after 1–2 successful hunts
- If it costs more, reduce the recipe

The most expensive full-set item (any 5-piece or off-hand overlord-tier item) should cost:
- **Target:** 6–8 materials, at least 2 of which require the specific monster resource
- If it's cheaper, add 1–2 of the monster-specific material

---

## Part 4 — Lifecycle Card Frequency

### Injury/Scar Distribution

Injuries are applied when a hunter survives a part reduction to 0 Flesh (saved by shell protection). Check the injury application code path to confirm the roll that triggers an injury.

**Target:** Average 1 injury per 4–5 hunts per hunter.

If injuries apply too frequently (every hunt), add a random chance gate:
```csharp
// In CombatManager — injury application
if (Random.value > 0.6f)  // 40% chance per 0-Flesh event
    GameStateManager.Instance.GrantInjury(hunter.hunterId, RollRandomInjury());
```

If injuries never occur in practice, remove the chance gate and always apply on a 0-Flesh save.

### Disorder Distribution

Disorders are granted post-hunt if the hunter was Shaken 3+ times or the hunt lasted 15+ rounds.

**Target:** Average 1 disorder per hunter per 8–10 hunts (roughly once per 2 years).

Check the post-hunt resolution code. If no disorder-granting code exists, add it to `PostHuntResolution`:

```csharp
// In GameStateManager.PostHuntResolution(HuntResult result):
foreach (var hunter in result.participatingHunters)
{
    // Grant disorder on severe exposure
    if (result.shakenCountForHunter(hunter.hunterId) >= 3 ||
        result.roundCount > 14)
    {
        if (Random.value > 0.7f && hunter.disorderIds.Length < 3)
        {
            string disorderId = RollRandomDisorder();
            GrantDisorder(hunter.hunterId, disorderId);
            AddChronicleEntry(CampaignState.currentYear,
                $"{hunter.hunterName} returned changed. Something in the dark left a mark.");
        }
    }
}
```

### Fighting Art Unlock Rate

Fighting Arts unlock when a hunter reaches the required `unlockYear`. Verify in `SettlementScreenController.HuntersTab()` that when a hunter hits `yearsActive == art.unlockYear`, a notification or indicator appears.

**Target:** First fighting art available on a hunter at Year 2–3.

FA-01 (Trample, unlockYear 3) should be the first art available. Confirm Aldric (or the oldest hunter) has it available by Year 3.

---

## Part 5 — Overlord Gate Tuning

Confirm overlord year gates are correct in each MonsterSO:

| Overlord | Current gate | Target |
|----------|-------------|--------|
| Siltborn | Years 8–16 | ✓ Stays at 8 (after some standard kills) |
| Penitent | Years 12–22 | ✓ Stays at 12 |
| Suture | Years 15–25 | ✓ Stays at 15 |
| Pale Stag Ascendant | Years 25–30 | ✓ + requires 1+ overlord kill |

If the player reaches Year 8 and the Siltborn is available before they've killed 3–4 standard monsters, the pacing is wrong. Verify `CanHuntMonster(monster, campaignState)` checks both year AND any prerequisiteOverlordKillCount.

The Pale Stag's `prerequisiteOverlordKillCount = 1` is load-bearing — confirm this is set. If the player can fight the Pale Stag with zero overlord kills, the endgame collapses.

---

## Part 6 — Retirement and Death Pacing

Retirement triggers when `hunter.yearsActive >= 7` and the player chooses to retire them.

**Target:** Settle on a soft pressure system — at 7 years, retirement is available but not mandatory. At 10 years, a "Weathered" debuff applies (-1 to all stats). At 13 years, the hunter dies of old age unless retired.

If the aging system only does "Weathered" at 7 years with no further pressure, add the following to `GameStateManager.AdvanceYear()`:

```csharp
foreach (var hunter in CampaignState.hunters)
{
    if (hunter.isDead || hunter.isRetired) continue;
    hunter.yearsActive++;

    if (hunter.yearsActive == 7)
    {
        // Offer retirement — retirement panel opens in Settlement
        CampaignState.pendingRetirementHunterIds =
            AppendToArray(CampaignState.pendingRetirementHunterIds, hunter.hunterId);
    }
    else if (hunter.yearsActive == 10)
    {
        if (!hunter.permanentDebuffs.Contains("Weathered"))
        {
            hunter.permanentDebuffs += (string.IsNullOrEmpty(hunter.permanentDebuffs) ? "" : ",")
                                     + "Weathered";
            AddChronicleEntry(CampaignState.currentYear,
                $"{hunter.hunterName} grows weathered. The years show.");
        }
    }
    else if (hunter.yearsActive >= 13)
    {
        // Hunter dies of old age
        hunter.isDead = true;
        CampaignState.totalHunterDeaths++;
        AddChronicleEntry(CampaignState.currentYear,
            $"{hunter.hunterName} passed quietly, their body finally done. The settlement mourns.");
    }
}
```

---

## Part 7 — Full Campaign Simulation (Abbreviated)

Using the Debug Panel, run this compressed campaign check:

1. Start new Standard campaign → 4 hunters, Year 1
2. Grant resource equivalent of 3 Year-1 hunts: `bone=9, hide=6, sinew=3`
3. Craft 1 Carapace item → confirm available and affordable
4. Jump to Year 5 → simulate 4 hunts → confirm 1–2 injuries
5. Jump to Year 10 → confirm 2–3 craft sets unlocked (Carapace + 2 standard kills)
6. Jump to Year 12 → confirm Penitent available in hunt selection
7. Kill Penitent → confirm Ichor Works unlocked
8. Jump to Year 25 → simulate 1 overlord kill
9. Confirm Pale Stag available (1 overlord kill prerequisite met)
10. Run Pale Stag fight → Phase 2 triggers at ~30% HP → Ascendant form kills or is killed
11. Victory epilogue → correct tier based on campaign state

Document any step that fails and the fix applied.

---

## Balance Change Log

Fill in this table as you go. Every change made this session must be documented here so it can be reverted if needed.

| Asset | Field | Old Value | New Value | Reason |
|-------|-------|-----------|-----------|--------|
| (fill in during session) | | | | |

---

## Definition of Done — Stage 10-K

- [ ] All 8 standard monsters pass round count targets (±2 of target)
- [ ] All 4 overlords pass round count targets
- [ ] Resource economy: first craft available Year 2–3 verified
- [ ] Resource economy: overlord-tier gear available Year 8–12 verified
- [ ] Injury frequency: at least 1 injury per hunter by Year 10 in simulation
- [ ] Retirement pressure: at least 1 retirement event fires in Year 7–15 simulation
- [ ] Pale Stag prerequisite gate: 0 overlord kills → Pale Stag NOT in hunt selection
- [ ] Balance change log filled with at least 3 documented changes (if zero changes needed, document "no change required" for each area)

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_M.md`
**Covers:** Combat Action Animations — hit flash, part break, collapse pulse; all USS transitions, no logic changes
