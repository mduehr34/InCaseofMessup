<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude Code to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 9-U | Limb Wound → Disorder Trigger
Status: Stage 9-T complete. Bleed and Poison counters implemented.
Limb body zones at 0 flesh already add 1 Bleed counter via
the BleedTriggered_{zone} tag system. The BleedTriggered tag is
also used as the disorder trigger signal.
Task: When a limb (LeftArm, RightArm, LeftLeg, RightLeg) reaches
0 flesh for the first time in a combat, draw one DisorderCardSO
from the campaign's disorder deck and apply it to that hunter.
The disorder carries into settlement. Display it on the hunter
panel in both combat and settlement.

Read these files before doing anything:
- CLAUDE.md
- .cursorrules
- _Docs/Stage_09/STAGE_09_U.md
- _Docs/Stage_09/STAGE_09_T.md         ← Bleed/BleedTriggered_ context
- Assets/_Game/Scripts/Core.Data/CombatState.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs
- Assets/_Game/Scripts/Core.Systems/CombatManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignSO.cs
- Assets/_Game/Scripts/Core.Data/HunterState.cs

Then confirm:
- `HunterCombatState.activeStatusEffects` string[] holds `BleedTriggered_{zone}` tags
- `HunterState.disorderIds` string[] exists and persists to campaign save
- `DisorderCardSO` does NOT yet exist — you will create it
- `CampaignSO.disorderDeck` does NOT yet exist — you will add it
- `CombatManager.CheckHunterCollapse` is where `BleedTriggered_*` tags are added (from 9-T)
- What you will NOT build: cure items for disorders (gear system, later)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 9-U: Limb Wound → Disorder Trigger

**Resuming from:** Stage 9-T complete — Bleed and Poison status counters implemented; `BleedTriggered_{zone}` tags set when a limb reaches 0 flesh
**Done when:** When a hunter's limb hits 0 flesh for the first time in a combat, one DisorderCardSO is drawn and applied to that hunter; the disorder ID is stored in `HunterState.disorderIds` and survives the campaign save; disorders are visible in the settlement hunter detail panel
**Commit:** `"9U: Limb wound disorder trigger — DisorderCardSO, campaign disorder deck, post-hunt application"`
**Next session:** STAGE_09_V.md (or next in Stage 9 sequence)

---

## Design Intent

A limb reduced to 0 flesh is a traumatic wound. The physical consequence (Bleed counter) was added in Stage 9-T. The psychological consequence is a **Disorder** — a persistent negative that follows the hunter into settlement and cannot be removed by rest alone.

Disorders are drawn from a shared campaign disorder deck, giving each one a randomized identity. This means two hunters who both take limb wounds in the same fight might end up with different disorders, creating asymmetric party stories.

**Key rules:**
- One disorder draw per limb per combat (the `BleedTriggered_{zone}` tag prevents double-triggering)
- If the campaign disorder deck is empty, no disorder is applied (pool can be exhausted over time)
- A hunter can hold at most 3 disorders (hardcoded cap — `HunterState.disorderIds.Length < 3`)
- Disorders are applied **post-combat** via `GameStateManager.PostHuntResolution`, not mid-combat, to avoid disrupting the combat loop

---

## Part 1: DisorderCardSO — New ScriptableObject

Create `Assets/_Game/Scripts/Core.Data/DisorderCardSO.cs`:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(menuName = "MnM/Hunter/Disorder", fileName = "New Disorder")]
    public class DisorderCardSO : ScriptableObject
    {
        [Header("Identity")]
        public string disorderId;          // Unique key, e.g. "FEAR_OF_DARKNESS"
        public string displayName;         // e.g. "Fear of Darkness"

        [Header("Description")]
        [TextArea] public string flavorText;
        [TextArea] public string mechanicalEffect;
        // mechanicalEffect is a plain English string read by GameStateManager and applied
        // to the hunter's stats. Examples:
        //   "accuracy -1"        → deducted from hunter's Accuracy stat
        //   "evasion -1"         → deducted from hunter's Evasion stat
        //   "grit -1"            → deducted from starting Grit for this hunter
        //   "draw 1 fewer card"  → resolved by CombatStateFactory when building the hunter's hand
        //   "always last in initiative" → resolved by CombatManager phase ordering
        // Complex or narrative effects use a tag format the Logic layer can route:
        //   "TAG:SHAKEN_ON_HUNT_START" — hunter gains Shaken at the start of every combat

        [Header("Cure")]
        [TextArea] public string cureCondition;
        // How this disorder can be removed. Examples:
        //   "Year-end: spend 1 hide in settlement"
        //   "Kill 3 monsters with this hunter"
        //   "Incurable"

        [Header("Stacking")]
        public bool isUnique;
        // If true, a hunter cannot have two copies of this disorder.
        // If false, multiple copies can stack (each applies mechanicalEffect again).
    }
}
```

---

## Part 2: CampaignSO — Add Disorder Deck

Open `Assets/_Game/Scripts/Core.Data/CampaignSO.cs`.

Add one field:

```csharp
[Header("Disorder Deck")]
public DisorderCardSO[] disorderDeck;
// Shared pool for all hunters in this campaign.
// Cards are NOT removed on draw — the same disorder can apply to multiple hunters.
// If the pool is empty, no disorder is applied.
// Populate this with the starting set defined in Part 5 of this stage.
```

---

## Part 3: Track Pending Disorders in CombatState

Disorders are applied post-combat, but CombatManager needs to record which hunters earned them during the fight. Add to `CombatState`:

```csharp
// ── Pending Disorder Records ──────────────────────────────────────
// Set by CheckHunterCollapse when a limb hits 0.
// Consumed by GameStateManager.PostHuntResolution.
public PendingDisorderRecord[] pendingDisorders;
```

Add the struct to `DataStructs.cs`:

```csharp
[Serializable]
public struct PendingDisorderRecord
{
    public string hunterId;
    public string limbZone;     // e.g. "LeftArm" — for chronicle narrative
}
```

---

## Part 4: CombatManager — Record Disorder Trigger

Open `Assets/_Game/Scripts/Core.Systems/CombatManager.cs`.

In `CheckHunterCollapse()`, **inside the limb-at-0 block added in Stage 9-T** (where `BleedTriggered_{zone}` is added), also record a pending disorder:

```csharp
// After adding BleedTriggered tag (from 9-T):
var pendingList = new List<PendingDisorderRecord>(
    CurrentState.pendingDisorders ?? new PendingDisorderRecord[0]);

// Only record if not already pending for this limb
bool alreadyPending = pendingList.Exists(
    r => r.hunterId == hunter.hunterId && r.limbZone == zone.zone);

if (!alreadyPending)
{
    pendingList.Add(new PendingDisorderRecord
    {
        hunterId = hunter.hunterId,
        limbZone = zone.zone,
    });
    CurrentState.pendingDisorders = pendingList.ToArray();
    Debug.Log($"[Combat] Disorder pending for {hunter.hunterName} — {zone.zone} at 0 flesh");
}
```

---

## Part 5: GameStateManager — PostHuntResolution Disorder Application

Open `Assets/_Game/Scripts/Core.Systems/GameStateManager.cs`.

In `PostHuntResolution(HuntResult result)`, add disorder application **after** the existing post-hunt logic (resources, chronicle entries, etc.):

```csharp
// ── Apply Pending Disorders ────────────────────────────────────────
if (result.pendingDisorders != null && result.pendingDisorders.Length > 0)
{
    var pool = _campaignData?.disorderDeck;
    if (pool == null || pool.Length == 0)
    {
        Debug.Log("[PostHunt] Disorder deck empty — no disorders applied");
    }
    else
    {
        foreach (var pending in result.pendingDisorders)
        {
            var hunterState = GetHunterState(pending.hunterId);
            if (hunterState == null) continue;

            // Cap at 3 disorders
            if ((hunterState.disorderIds?.Length ?? 0) >= 3)
            {
                Debug.Log($"[PostHunt] {hunterState.hunterName} already has 3 disorders — no new disorder");
                continue;
            }

            // Draw a random disorder from the pool
            var disorder = DrawDisorder(pool, hunterState);
            if (disorder == null)
            {
                Debug.Log($"[PostHunt] No valid disorder to draw for {hunterState.hunterName}");
                continue;
            }

            // Apply disorder
            var disorders = new List<string>(hunterState.disorderIds ?? new string[0]);
            disorders.Add(disorder.disorderId);
            hunterState.disorderIds = disorders.ToArray();

            // Chronicle entry
            AddChronicleEntry(
                CampaignState.currentYear,
                $"{hunterState.hunterName}'s {pending.limbZone.Replace("Left","left ").Replace("Right","right ")} " +
                $"was broken in the hunt. They returned with {disorder.displayName}.");

            Debug.Log($"[PostHunt] {hunterState.hunterName} gains disorder: {disorder.displayName} ({disorder.disorderId})");

            // Apply mechanical effect (stat modification)
            ApplyDisorderEffect(hunterState, disorder);
        }
    }
}
```

Add the helpers:

```csharp
private DisorderCardSO DrawDisorder(DisorderCardSO[] pool, HunterState hunter)
{
    // Filter: exclude unique disorders the hunter already has
    var candidates = new List<DisorderCardSO>();
    foreach (var d in pool)
    {
        if (d == null) continue;
        if (d.isUnique && System.Array.Exists(
            hunter.disorderIds ?? new string[0], id => id == d.disorderId)) continue;
        candidates.Add(d);
    }

    if (candidates.Count == 0) return null;
    return candidates[Random.Range(0, candidates.Count)];
}

private void ApplyDisorderEffect(HunterState hunter, DisorderCardSO disorder)
{
    string effect = (disorder.mechanicalEffect ?? "").Trim().ToLower();

    // Parse simple stat modifications
    if (TryParseStatMod(effect, "accuracy", out int accMod))
    {
        hunter.accuracy = Mathf.Max(0, hunter.accuracy + accMod);
        Debug.Log($"[Disorder] {hunter.hunterName} accuracy {accMod:+0;-0} → {hunter.accuracy}");
    }
    else if (TryParseStatMod(effect, "evasion", out int evaMod))
    {
        hunter.evasion = Mathf.Max(0, hunter.evasion + evaMod);
        Debug.Log($"[Disorder] {hunter.hunterName} evasion {evaMod:+0;-0} → {hunter.evasion}");
    }
    else if (TryParseStatMod(effect, "grit", out int gritMod))
    {
        hunter.grit = Mathf.Max(0, hunter.grit + gritMod);
        Debug.Log($"[Disorder] {hunter.hunterName} grit {gritMod:+0;-0} → {hunter.grit}");
    }
    else
    {
        // TAG: prefix or narrative effects — handled at point of use
        Debug.Log($"[Disorder] {hunter.hunterName} '{disorder.disorderId}' effect '{disorder.mechanicalEffect}' — " +
                  $"resolved at point of use (TAG or narrative effect)");
    }
}

private static bool TryParseStatMod(string effect, string stat, out int mod)
{
    mod = 0;
    if (!effect.Contains(stat)) return false;
    var match = System.Text.RegularExpressions.Regex.Match(effect, stat + @"\s*([-+]?\d+)");
    if (!match.Success) return false;
    return int.TryParse(match.Groups[1].Value, out mod);
}
```

---

## Part 6: Author Starting Disorder Cards

Create these 10 `DisorderCardSO` assets in `Assets/_Game/Data/Disorders/`:

| Asset | disorderId | displayName | mechanicalEffect | cureCondition | isUnique |
|---|---|---|---|---|---|
| `Disorder_Tremors` | TREMORS | Hand Tremors | accuracy -1 | Year-end: spend 1 hide | false |
| `Disorder_LimpingGait` | LIMPING_GAIT | Limping Gait | evasion -1 | Incurable | false |
| `Disorder_NightTerrors` | NIGHT_TERRORS | Night Terrors | draw 1 fewer card | Year-end: rest (no hunt) | true |
| `Disorder_CombatPanic` | COMBAT_PANIC | Combat Panic | grit -1 | Kill 3 monsters | true |
| `Disorder_Paranoia` | PARANOIA | Paranoia | TAG:SHAKEN_ON_HUNT_START | Incurable | true |
| `Disorder_BloodFear` | BLOOD_FEAR | Fear of Blood | TAG:SKIP_FIRST_CARD | Year-end: spend 2 hide | true |
| `Disorder_ExposedNerve` | EXPOSED_NERVE | Exposed Nerve | accuracy -1 | Kill 2 monsters | false |
| `Disorder_WeakGrip` | WEAK_GRIP | Weak Grip | evasion -1 | Year-end: spend 1 sinew | false |
| `Disorder_EchoingPain` | ECHOING_PAIN | Echoing Pain | grit -1 | Incurable | false |
| `Disorder_DeathdreamFever` | DEATHDREAM_FEVER | Deathdream Fever | TAG:RANDOM_CARD_DISCARD_ON_DRAW | Incurable | true |

**Flavor text examples:**

- **Hand Tremors:** *"The arm bone set wrong. Some nights the shaking won't stop."*
- **Limping Gait:** *"The leg healed, more or less. It will never be what it was."*
- **Night Terrors:** *"They wake before the others. They don't explain why."*
- **Combat Panic:** *"The moment the creature turns toward them, the mind goes blank."*
- **Paranoia:** *"Something followed us back. They're sure of it."*

Assign all 10 to `CampaignSO.disorderDeck` on the default `Campaign_Standard.asset`.

---

## Part 7: Settlement UI — Display Disorders

Open the settlement Hunter tab. In the hunter detail view (or alongside the `injuryIds` display), add disorder display.

In `SettlementScreenController.RefreshHunterDetail(HunterState hunter)`:

```csharp
// ── Disorders ────────────────────────────────────────────────────
var disorderContainer = _hunterDetailRoot.Q("disorder-list");
if (disorderContainer != null)
{
    disorderContainer.Clear();

    if (hunter.disorderIds == null || hunter.disorderIds.Length == 0)
    {
        var none = new Label("No disorders");
        none.AddToClassList("card-list-empty");
        disorderContainer.Add(none);
    }
    else
    {
        foreach (var id in hunter.disorderIds)
        {
            var disorder = ResolveDisorder(id);
            var row = BuildDisorderRow(disorder, id);
            disorderContainer.Add(row);
        }
    }
}
```

Add to the hunter detail UXML:
```xml
<ui:VisualElement name="disorder-list" class="card-list disorder-list" />
```

Add `BuildDisorderRow`:
```csharp
private VisualElement BuildDisorderRow(DisorderCardSO disorder, string fallbackId)
{
    var row = new VisualElement();
    row.AddToClassList("disorder-row");

    string name   = disorder != null ? disorder.displayName   : fallbackId;
    string effect = disorder != null ? disorder.mechanicalEffect : "Unknown effect";
    string cure   = disorder != null ? disorder.cureCondition  : "";

    var nameLabel = new Label(name.ToUpper());
    nameLabel.AddToClassList("disorder-name");
    row.Add(nameLabel);

    var effectLabel = new Label(effect);
    effectLabel.AddToClassList("disorder-effect");
    row.Add(effectLabel);

    if (!string.IsNullOrEmpty(cure) && cure != "Incurable")
    {
        var cureLabel = new Label($"Cure: {cure}");
        cureLabel.AddToClassList("disorder-cure");
        row.Add(cureLabel);
    }

    return row;
}

private DisorderCardSO ResolveDisorder(string disorderId)
{
    var pool = _campaignData?.disorderDeck;
    if (pool == null) return null;
    foreach (var d in pool)
        if (d != null && d.disorderId == disorderId) return d;
    return null;
}
```

Add to `settlement-screen.uss`:
```css
.disorder-list {
    margin-top: 8px;
    border-top-color: rgba(80, 30, 30, 0.50);
    border-top-width: 1px;
    padding-top: 8px;
}

.disorder-row {
    margin-bottom: 10px;
    padding: 8px;
    background-color: rgba(60, 15, 15, 0.40);
    border-color: rgba(120, 40, 40, 0.50);
    border-width: 1px;
}

.disorder-name {
    font-size: 11px;
    color: rgb(200, 80, 80);
    -unity-font-style: bold;
    margin-bottom: 4px;
}

.disorder-effect {
    font-size: 10px;
    color: rgb(160, 130, 110);
    white-space: normal;
    margin-bottom: 3px;
}

.disorder-cure {
    font-size: 9px;
    color: rgb(100, 130, 80);
    font-style: italic;
}
```

---

## Verification Test

**Limb trigger path:**
- [ ] Put Aldric's LeftArm flesh to 1 via Inspector
- [ ] Trigger `CheckHunterCollapse` (have monster attack Aldric's LeftArm for 1 damage)
- [ ] Console: `[Combat] Disorder pending for Aldric — LeftArm at 0 flesh`
- [ ] `CurrentState.pendingDisorders` array has 1 entry (verify in Inspector)
- [ ] `BleedTriggered_LeftArm` tag still set (from 9-T — no regression)

**Post-hunt application:**
- [ ] End combat (any outcome) → `PostHuntResolution` fires
- [ ] Console: `[PostHunt] Aldric gains disorder: [name] ([id])`
- [ ] `HunterState.disorderIds` has 1 entry — persists to campaign save
- [ ] Chronicle entry added: `"Aldric's left arm was broken..."`

**Settlement display:**
- [ ] Open Settlement → HUNTERS tab → click Aldric
- [ ] Disorder row visible in hunter detail panel
- [ ] Name in dark red, effect text and cure condition visible
- [ ] Hunter with no disorders shows "No disorders"

**Cap enforcement:**
- [ ] Set `disorderIds` to 3 entries via Inspector
- [ ] Trigger another limb wound
- [ ] Console: `[PostHunt] Aldric already has 3 disorders — no new disorder`

**Uniqueness:**
- [ ] `Night Terrors` (isUnique=true) applied to Aldric
- [ ] Trigger another limb wound
- [ ] Night Terrors should NOT appear again for Aldric (filtered from draw pool)

---

## Definition of Done — Stage 9-U

- [ ] `DisorderCardSO.cs` created; all fields visible in Inspector; `[CreateAssetMenu]` correct
- [ ] `CampaignSO.disorderDeck` field added
- [ ] `PendingDisorderRecord` struct added to `DataStructs.cs`
- [ ] `CombatState.pendingDisorders` field added
- [ ] `CombatManager.CheckHunterCollapse` records a `PendingDisorderRecord` when a limb hits 0 (alongside existing Bleed from 9-T)
- [ ] `GameStateManager.PostHuntResolution` draws and applies disorder from campaign pool
- [ ] Stat modification effects (`accuracy -1`, `evasion -1`, `grit -1`) applied to `HunterState`
- [ ] TAG: prefix effects logged but not applied (resolved at point of use — future stages)
- [ ] `HunterState.disorderIds` populated and survives campaign save/load round-trip
- [ ] 10 `DisorderCardSO` assets authored and assigned to `Campaign_Standard.asset.disorderDeck`
- [ ] Settlement hunter detail panel displays disorders with name, effect, cure condition
- [ ] 3-disorder cap enforced; uniqueness filtering works
- [ ] Chronicle entry written for each disorder gain
- [ ] No compile errors

---

## Next Session

**File:** `_Docs/Stage_09/STAGE_09_V.md`
**Covers:** (next in Stage 9 sequence — create this file when ready)
