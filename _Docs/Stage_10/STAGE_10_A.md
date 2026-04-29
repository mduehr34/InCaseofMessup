<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 10-A | Mechanical TODO Stubs — Gaunt Effects, EyePendant, Consumable UI
Status: Stage 9-S complete. Full v1.0 campaign verified.
Task: Implement the four mechanical stubs left as TODO comments
in Stage 7-R. These are the only unresolved gameplay mechanics
in the codebase. All four live in CombatManager.cs.
  1. GAUNT_3PC_LOUD_SUPPRESS — Loud card plays reduce monster movement by 2
  2. GAUNT_5PC_DEATH_CHEAT — Once per hunt collapse survival at 1 Flesh
  3. EyePendant scar intercept — discard incoming scar on confirm
  4. Consumable targeting UI — BoneSplint use from hand with adjacency check

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_10/STAGE_10_A.md
- Assets/_Game/Scripts/Core.Logic/CombatManager.cs      ← search for TODO: 7R
- Assets/_Game/Scripts/Core.Logic/ConsumableResolver.cs
- Assets/_Game/Scripts/Core.Data/DataStructs.cs         ← SetBonusEntry, LinkPoint
- Assets/_Game/Scripts/Core.Data/CombatState.cs         ← HunterCombatState

Then confirm:
- You can find all four TODO: 7R stubs by grepping for "7R"
- GAUNT_3PC_LOUD_SUPPRESS fires in the behavior card resolution path
- GAUNT_5PC_DEATH_CHEAT fires in the collapse trigger path
- EyePendant intercept fires in the injury card application path
- ConsumableResolver.ApplyBoneSplint already exists (stub from 7-R)
- What you will NOT do (art, animations, audio — those are 10-B onward)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 10-A: Mechanical TODO Stubs — Gaunt Effects, EyePendant, Consumable UI

**Resuming from:** Stage 9-S complete — all monsters, gear, and campaign systems verified
**Done when:** All four TODO: 7R stubs resolved; all five verification tests below pass
**Commit:** `"10A: Implement deferred Gaunt set effects, EyePendant intercept, consumable UI"`
**Next session:** STAGE_10_B.md

---

## Context — The Four Stubs

Stage 7-R implemented the gear resolver framework but intentionally left four effect handlers as `// TODO: 7R` comments because the behavior card resolution system wasn't yet built when Stage 7 ran. The stubs are:

1. `// TODO: 7R — handle GAUNT_3PC_LOUD_SUPPRESS`
2. `// TODO: 7R — handle GAUNT_5PC_DEATH_CHEAT`
3. `// TODO: 7R — EyePendant scar intercept`
4. `// TODO: 7R — consumable targeting UI`

Before touching anything, find all four by searching the codebase:

```
grep -rn "TODO: 7R" Assets/_Game/Scripts/
```

Confirm all four are present, then proceed section by section.

---

## Part 1 — GAUNT_3PC_LOUD_SUPPRESS

**What it does:** When a hunter plays a Loud action card (any card with `cardCategory == "Loud"`), and that hunter has the Gaunt 3-piece set bonus active, the monster's current movement target on that card is reduced by 2 (minimum 0).

**Where to implement:** In `CombatManager`, inside the behavior card resolution path — the section that reads movement instructions from the active behavior card. The 3-piece suppression is a *reaction* to a hunter action, so it fires after the hunter card resolves but before the monster card resolves that turn.

```csharp
// In CombatManager — behavior card resolution
// Replace the TODO: 7R stub with:

private void ApplyLoudSuppressIfActive(HunterCombatState attacker, BehaviorCardSO monsterCard)
{
    if (!attacker.activeGearEffectTags.Contains("GAUNT_3PC_LOUD_SUPPRESS")) return;
    if (monsterCard.movementDistance <= 0) return;

    int original = monsterCard.movementDistance;
    // Apply suppression as a runtime modifier — do NOT mutate the SO
    _activeBehaviorMovementOverride = Mathf.Max(0, original - 2);
    Debug.Log($"[GearEffect] Gaunt 3-piece: Loud suppression reduces monster move " +
              $"{original} → {_activeBehaviorMovementOverride}");
}
```

Add the field to `CombatManager`:
```csharp
private int _activeBehaviorMovementOverride = -1; // -1 = no override
```

In the movement resolution step, use the override if set:
```csharp
int moveDistance = _activeBehaviorMovementOverride >= 0
    ? _activeBehaviorMovementOverride
    : _currentBehaviorCard.movementDistance;
_activeBehaviorMovementOverride = -1; // Reset after use
```

Call `ApplyLoudSuppressIfActive` immediately after the hunter's card resolves, before monster turn begins. Pass the hunter who played the card and the *next* behavior card (peek the top of the deck).

---

## Part 2 — GAUNT_5PC_DEATH_CHEAT

**What it does:** Once per hunt, when a hunter with the Gaunt 5-piece set active would drop to 0 Flesh (collapse trigger), they instead survive with 1 Flesh on the struck body zone.

**Where to implement:** In `CombatManager`, at the collapse trigger site — the exact line where `hunterCombatState.isCollapsed = true` would be set.

```csharp
// In CombatManager — collapse trigger path
// Replace the TODO: 7R stub with:

private bool TryGaunt5PcDeathCheat(HunterCombatState hunter, string zoneName)
{
    if (!hunter.activeGearEffectTags.Contains("GAUNT_5PC_DEATH_CHEAT")) return false;
    if (hunter.spentHuntAbilities.Contains("GAUNT_5PC_DEATH_CHEAT"))    return false;

    // Find the zone and set it to 1 Flesh instead of 0
    for (int i = 0; i < hunter.bodyZones.Length; i++)
    {
        if (hunter.bodyZones[i].zone != zoneName) continue;
        hunter.bodyZones[i].fleshCurrent = 1;
        hunter.spentHuntAbilities = AppendToArray(hunter.spentHuntAbilities,
                                                   "GAUNT_5PC_DEATH_CHEAT");
        Debug.Log($"[GearEffect] Gaunt 5-piece: {hunter.hunterName} collapse prevented — " +
                  $"1 Flesh remaining on {zoneName}.");
        return true;
    }
    return false;
}
```

In the collapse trigger:
```csharp
// Before: hunter.isCollapsed = true;
// After:
if (!TryGaunt5PcDeathCheat(hunterState, struckZoneName))
{
    hunterState.isCollapsed = true;
    OnHunterCollapsed(hunterState);
}
```

---

## Part 3 — EyePendant Scar Intercept

**What it does:** When a hunter with `"Gaunt Eye Pendant"` equipped would receive a scar card, they may discard it (once per hunt). A UI confirmation modal appears; on accept the scar is not applied and the ability is marked spent.

**Where to implement:** In `CombatManager`, at the injury card application site (where `GameStateManager.GrantScar()` would be called).

```csharp
// In CombatManager — injury card application path
// Replace the TODO: 7R stub with:

private IEnumerator TryEyePendantInterceptRoutine(HunterCombatState hunter,
                                                    string scarId,
                                                    System.Action onApply,
                                                    System.Action onDiscard)
{
    bool hasEyePendant = hunter.equippedItemNames != null &&
                         System.Array.Exists(hunter.equippedItemNames,
                                             n => n == "Gaunt Eye Pendant");
    bool alreadySpent  = hunter.spentHuntAbilities != null &&
                         hunter.spentHuntAbilities.Contains("Gaunt Eye Pendant");

    if (!hasEyePendant || alreadySpent)
    {
        onApply?.Invoke();
        yield break;
    }

    // Show confirmation modal
    bool playerChose   = false;
    bool playerDiscard = false;
    _combatHUD.ShowEyePendantModal(
        hunter.hunterName,
        scarId,
        discard: () => { playerDiscard = true; playerChose = true; },
        apply:   () => { playerDiscard = false; playerChose = true; }
    );

    yield return new WaitUntil(() => playerChose);

    if (playerDiscard)
    {
        hunter.spentHuntAbilities = AppendToArray(hunter.spentHuntAbilities,
                                                   "Gaunt Eye Pendant");
        Debug.Log($"[GearEffect] EyePendant: {hunter.hunterName} discarded scar {scarId}.");
        onDiscard?.Invoke();
    }
    else
    {
        onApply?.Invoke();
    }
}
```

**UI modal in `CombatHUDUpdater`:**

```csharp
public void ShowEyePendantModal(string hunterName, string scarId,
                                 System.Action discard, System.Action apply)
{
    var root  = _uiDocument.rootVisualElement;
    var modal = root.Q("eye-pendant-modal");
    if (modal == null)
    {
        // Build it dynamically if not in UXML
        modal = new VisualElement();
        modal.name = "eye-pendant-modal";
        modal.style.position   = Position.Absolute;
        modal.style.left       = new Length(50, LengthUnit.Percent);
        modal.style.top        = new Length(50, LengthUnit.Percent);
        modal.style.translate  = new Translate(new Length(-50, LengthUnit.Percent),
                                               new Length(-50, LengthUnit.Percent));
        modal.style.backgroundColor = new Color(0.08f, 0.06f, 0.04f, 0.96f);
        modal.style.borderTopWidth = modal.style.borderBottomWidth =
        modal.style.borderLeftWidth = modal.style.borderRightWidth = 1f;
        modal.style.borderTopColor = modal.style.borderBottomColor =
        modal.style.borderLeftColor = modal.style.borderRightColor =
            new Color(0.72f, 0.52f, 0.04f);
        modal.style.paddingTop = modal.style.paddingBottom =
        modal.style.paddingLeft = modal.style.paddingRight = 16;
        root.Add(modal);
    }

    modal.Clear();
    modal.style.display = DisplayStyle.Flex;

    var title = new Label("GAUNT EYE PENDANT");
    title.style.color    = new Color(0.72f, 0.52f, 0.04f);
    title.style.fontSize = 10;
    title.style.unityFontStyleAndWeight = FontStyle.Bold;
    title.style.marginBottom = 8;
    modal.Add(title);

    var body = new Label($"{hunterName} would receive {scarId}.\nSpend Eye Pendant to discard it?");
    body.style.color      = new Color(0.83f, 0.80f, 0.73f);
    body.style.fontSize   = 9;
    body.style.whiteSpace = WhiteSpace.Normal;
    body.style.marginBottom = 12;
    modal.Add(body);

    var row = new VisualElement();
    row.style.flexDirection = FlexDirection.Row;
    row.style.justifyContent = Justify.SpaceBetween;
    modal.Add(row);

    var btnDiscard = new Button(() => { modal.style.display = DisplayStyle.None; discard?.Invoke(); });
    btnDiscard.text = "DISCARD SCAR";
    btnDiscard.style.color = new Color(0.83f, 0.80f, 0.73f);
    row.Add(btnDiscard);

    var btnApply = new Button(() => { modal.style.display = DisplayStyle.None; apply?.Invoke(); });
    btnApply.text = "ACCEPT SCAR";
    btnApply.style.color = new Color(0.45f, 0.43f, 0.40f);
    row.Add(btnApply);
}
```

---

## Part 4 — Consumable Targeting UI

**What it does:** When a hunter selects a consumable card (e.g., Bone Splint) from their hand, the UI enters "target selection" mode. Hunters within 1 grid space are highlighted; clicking one applies the effect and removes the consumable from the loadout.

**Where to implement:** In `CombatScreenController`, as an extension of the existing card-play flow. Consumables are identified by `ItemSO.isConsumable == true`.

```csharp
// In CombatScreenController — card play handler
private void OnCardClicked(ActionCardSO card)
{
    if (card.isConsumable)
    {
        StartConsumableTargeting(card);
        return;
    }
    // Existing card play logic
    PlayCard(card);
}

private ActionCardSO _pendingConsumable;

private void StartConsumableTargeting(ActionCardSO consumable)
{
    _pendingConsumable = consumable;
    _isTargetingConsumable = true;

    // Highlight valid target tokens (adjacent hunters)
    var activeHunter = _combatState.GetActiveHunter();
    foreach (var hunter in _combatState.hunters)
    {
        if (hunter == activeHunter) continue;
        bool adjacent = ConsumableResolver.IsValidConsumableTarget(
            activeHunter.gridX, activeHunter.gridY,
            hunter.gridX, hunter.gridY);
        var token = _hunterTokens[hunter.hunterId];
        token.SetHighlight(adjacent ? HighlightType.Consumable : HighlightType.None);
    }

    // Show cancel hint in HUD
    _combatHUD.ShowConsumableTargetPrompt(consumable.itemName);
}

private void OnHunterTokenClicked(HunterCombatState target)
{
    if (!_isTargetingConsumable) return;

    var activeHunter = _combatState.GetActiveHunter();
    if (!ConsumableResolver.IsValidConsumableTarget(
            activeHunter.gridX, activeHunter.gridY,
            target.gridX, target.gridY))
    {
        Debug.Log("[Consumable] Target out of range.");
        return;
    }

    // Apply effect
    ConsumableResolver.ApplyBoneSplint(target, "Torso");  // Default zone; expand later if per-zone UI is needed
    GameStateManager.Instance.RemoveItemFromHunter(activeHunter.hunterId, _pendingConsumable.itemName);

    // Clear targeting mode
    _isTargetingConsumable = false;
    _pendingConsumable     = null;
    ClearAllHighlights();
    _combatHUD.HideConsumableTargetPrompt();
}
```

Add `ShowConsumableTargetPrompt` / `HideConsumableTargetPrompt` to `CombatHUDUpdater`:

```csharp
public void ShowConsumableTargetPrompt(string itemName)
{
    var label = _uiDocument.rootVisualElement.Q<Label>("consumable-target-prompt");
    if (label == null) return;
    label.text = $"Select a target for {itemName.ToUpper()} (adjacent hunters only). Right-click to cancel.";
    label.style.display = DisplayStyle.Flex;
}

public void HideConsumableTargetPrompt()
{
    var label = _uiDocument.rootVisualElement.Q<Label>("consumable-target-prompt");
    if (label != null) label.style.display = DisplayStyle.None;
}
```

Add a `consumable-target-prompt` Label to the CombatScene UXML, positioned at the bottom of the HUD strip.

Right-click (or a Cancel button) while `_isTargetingConsumable == true` should call `CancelConsumableTargeting()` which clears the pending consumable and removes highlights.

---

## Helper — AppendToArray

If not already present in `CombatManager`, add:

```csharp
private static string[] AppendToArray(string[] existing, string value)
{
    var list = new System.Collections.Generic.List<string>(existing ?? new string[0]);
    if (!list.Contains(value)) list.Add(value);
    return list.ToArray();
}
```

---

## Verification Tests

### Test 1 — Gaunt 3-piece Loud suppress
1. Equip hunter with 3+ Gaunt pieces (grant via Debug panel → Grant Resources → Craft + Equip)
2. Confirm `activeGearEffectTags` contains `"GAUNT_3PC_LOUD_SUPPRESS"`
3. Play a Loud card (any card with `cardCategory == "Loud"`)
4. Monster's next behavior card has `movementDistance 3`
5. Expected log: `[GearEffect] Gaunt 3-piece: Loud suppression reduces monster move 3 → 1`
6. Monster moves 1 space, not 3

### Test 2 — Gaunt 5-piece collapse prevention
1. Equip hunter with all 5 Gaunt armor pieces
2. Confirm `activeGearEffectTags` contains `"GAUNT_5PC_DEATH_CHEAT"` and `spentHuntAbilities` is empty
3. Reduce hunter's active body zone to 0 via debug
4. Expected log: `[GearEffect] Gaunt 5-piece: [name] collapse prevented — 1 Flesh remaining on [zone]`
5. Hunter does NOT collapse; zone shows 1 Flesh
6. Trigger a second collapse attempt — this time hunter collapses normally (`spentHuntAbilities` blocks)

### Test 3 — EyePendant scar intercept
1. Equip hunter with `"Gaunt Eye Pendant"`; `spentHuntAbilities` empty
2. Trigger scar card draw via debug
3. Modal appears: "GAUNT EYE PENDANT — [name] would receive [scarId]. Spend Eye Pendant to discard it?"
4. Click DISCARD SCAR → scar not added to HunterState; `spentHuntAbilities` now contains `"Gaunt Eye Pendant"`
5. Trigger second scar draw → modal does NOT appear; scar applied normally

### Test 4 — Consumable targeting mode
1. Place Bone Splint in hunter's action hand (via debug or crafting)
2. Click Bone Splint card → game enters targeting mode; adjacent hunters highlighted
3. Click a hunter outside range → no effect; prompt remains
4. Click an adjacent hunter → `ConsumableResolver.ApplyBoneSplint` fires; log confirms Flesh restored
5. Bone Splint no longer in hand; targeting mode cleared

### Test 5 — No regression on existing Gaunt tests (Stage 7-R)
Run all five Stage 7-R verification tests again. All should still pass.

---

## What This Session Will NOT Do

- Art, animations, or audio (Stage 10-B onward)
- Adding new gear item types or craft sets
- Difficulty variants or settings menus

---

## Next Session

**File:** `_Docs/Stage_10/STAGE_10_B.md`
**Covers:** Injury, Disorder & Fighting Art mechanical enforcement — wiring the card data created in Stage 9-A into actual combat logic so InjurySO stat penalties apply at the start of each hunt, DisorderSO triggers fire on their conditions, and FightingArtSO effects are playable from the hunter's hand
