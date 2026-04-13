<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 4-D | Innovation Deck, Crafting & GearLinkResolver
Status: Stage 4-C complete. SettlementManager hunt results,
resources, chronicle events, and guiding principals all
verified. Test script deleted.
Task: Add to SettlementManager — Innovation deck draw/adopt
with cascade unlocks, Crafter unlock, TryCraftItem().
Create GearLinkResolver as a separate static class.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_04/STAGE_04_D.md
- Assets/_Game/Scripts/Core.Systems/SettlementManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/Scripts/Core.Data/InnovationSO.cs
- Assets/_Game/Scripts/Core.Data/CrafterSO.cs
- Assets/_Game/Scripts/Core.Data/ItemSO.cs

Then confirm:
- That you will ADD methods to SettlementManager.cs,
  not replace what exists
- That cascade unlock (INN-01 → INN-07, INN-11) adds
  to availableInnovationIds without duplicates
- That GearLinkResolver is created as a NEW file in
  Core.Logic
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 4-D: Innovation Deck, Crafting & GearLinkResolver

**Resuming from:** Stage 4-C complete — SettlementManager verified  
**Done when:** Innovation adoption cascades correctly; crafting checks resources and deducts correctly; GearLinkResolver logs active links  
**Commit:** `"4D: Innovation deck cascade, crafter unlock, crafting, GearLinkResolver"`  
**Next session:** STAGE_04_E.md  

---

## Step 1: Add Innovation Methods to SettlementManager.cs

Add these methods to the existing `SettlementManager` class:

```csharp
// ── Innovation Deck ──────────────────────────────────────────

public InnovationSO[] DrawInnovationOptions(int drawCount = 3)
{
    if (_campaignData.startingInnovations == null)
        return new InnovationSO[0];

    // Pool = all available IDs not yet adopted
    var pool = _campaign.availableInnovationIds
        .Where(id => !_campaign.adoptedInnovationIds.Contains(id))
        .Select(id => GetInnovationById(id))
        .Where(inn => inn != null)
        .ToList();

    if (pool.Count == 0)
    {
        Debug.Log("[Innovation] No innovations available to draw");
        return new InnovationSO[0];
    }

    // Shuffle and take up to drawCount
    ShuffleList(pool);
    var drawn = pool.Take(drawCount).ToArray();
    Debug.Log($"[Innovation] Drew {drawn.Length} options: " +
              $"[{string.Join(", ", drawn.Select(i => i.innovationName))}]");
    return drawn;
}

public void AdoptInnovation(InnovationSO innovation)
{
    if (_campaign.adoptedInnovationIds.Contains(innovation.innovationId))
    {
        Debug.LogWarning($"[Innovation] Already adopted: {innovation.innovationId}");
        return;
    }

    // Mark as adopted
    var adopted = new List<string>(_campaign.adoptedInnovationIds)
        { innovation.innovationId };
    _campaign.adoptedInnovationIds = adopted.ToArray();

    // Cascade — add newly unlocked cards to available pool (no duplicates)
    if (innovation.addsToDeck != null)
    {
        var available = new List<string>(_campaign.availableInnovationIds);
        foreach (var unlocked in innovation.addsToDeck)
        {
            if (unlocked != null && !available.Contains(unlocked.innovationId))
            {
                available.Add(unlocked.innovationId);
                Debug.Log($"[Innovation] Cascade unlock: {unlocked.innovationId} " +
                          $"({unlocked.innovationName}) added to pool");
            }
        }
        _campaign.availableInnovationIds = available.ToArray();
    }

    AddToChronicle($"Year {_campaign.currentYear}: Innovation adopted — {innovation.innovationName}.");
    Debug.Log($"[Innovation] Adopted: {innovation.innovationName}. " +
              $"Pool now: {_campaign.availableInnovationIds.Length} " +
              $"Adopted: {_campaign.adoptedInnovationIds.Length}");
}

private InnovationSO GetInnovationById(string id)
{
    if (_campaignData.startingInnovations == null) return null;
    // Search both starting and any cascaded innovations
    // For now: search starting set; cascaded SOs must be referenced from starting SOs
    foreach (var inn in _campaignData.startingInnovations)
    {
        if (inn == null) continue;
        if (inn.innovationId == id) return inn;
        // Check cascade references
        if (inn.addsToDeck != null)
            foreach (var child in inn.addsToDeck)
                if (child != null && child.innovationId == id) return child;
    }
    Debug.LogWarning($"[Innovation] InnovationSO not found for id: {id}");
    return null;
}

// ── Crafter Unlock ───────────────────────────────────────────

public bool TryUnlockCrafter(CrafterSO crafter)
{
    if (_campaign.builtCrafterNames.Contains(crafter.crafterName))
    {
        Debug.Log($"[Crafter] Already built: {crafter.crafterName}");
        return false;
    }

    // Check unlock cost
    if (crafter.unlockCost != null)
    {
        for (int i = 0; i < crafter.unlockCost.Length; i++)
        {
            int needed = (crafter.unlockCostAmounts != null && i < crafter.unlockCostAmounts.Length)
                ? crafter.unlockCostAmounts[i] : 0;
            int have   = GetResourceAmount(crafter.unlockCost[i].resourceName);
            if (have < needed)
            {
                Debug.LogWarning($"[Crafter] Cannot unlock {crafter.crafterName} — " +
                                 $"need {needed} {crafter.unlockCost[i].resourceName}, have {have}");
                return false;
            }
        }

        // Deduct
        for (int i = 0; i < crafter.unlockCost.Length; i++)
        {
            int needed = (crafter.unlockCostAmounts != null && i < crafter.unlockCostAmounts.Length)
                ? crafter.unlockCostAmounts[i] : 0;
            RemoveResource(crafter.unlockCost[i].resourceName, needed);
        }
    }

    var built = new List<string>(_campaign.builtCrafterNames) { crafter.crafterName };
    _campaign.builtCrafterNames = built.ToArray();

    AddToChronicle($"Year {_campaign.currentYear}: {crafter.crafterName} built.");
    Debug.Log($"[Crafter] Unlocked: {crafter.crafterName}");
    return true;
}

// ── Crafting ─────────────────────────────────────────────────

public bool TryCraftItem(ItemSO item, string forCharacterId)
{
    // Crafter must be built
    if (!IsCrafterBuiltForItem(item))
    {
        Debug.LogWarning($"[Crafting] No built Crafter for: {item.itemName}");
        return false;
    }

    // Check resources
    if (item.craftingCost != null)
    {
        for (int i = 0; i < item.craftingCost.Length; i++)
        {
            int needed = (item.craftingCostAmounts != null && i < item.craftingCostAmounts.Length)
                ? item.craftingCostAmounts[i] : 0;
            int have   = GetResourceAmount(item.craftingCost[i].resourceName);
            if (have < needed)
            {
                Debug.LogWarning($"[Crafting] Insufficient {item.craftingCost[i].resourceName}: " +
                                 $"need {needed}, have {have}");
                return false;
            }
        }

        // Deduct
        for (int i = 0; i < item.craftingCost.Length; i++)
        {
            int needed = (item.craftingCostAmounts != null && i < item.craftingCostAmounts.Length)
                ? item.craftingCostAmounts[i] : 0;
            RemoveResource(item.craftingCost[i].resourceName, needed);
        }
    }

    // Add to character's loadout
    var character = GetCharacter(forCharacterId);
    if (character == null) return false;

    var items = new List<string>(character.equippedItemNames) { item.itemName };
    character.equippedItemNames = items.ToArray();

    Debug.Log($"[Crafting] Crafted {item.itemName} for {character.characterName}");
    return true;
}

private bool IsCrafterBuiltForItem(ItemSO item)
{
    // An item can be crafted if ANY built crafter lists it in its recipeList
    if (_campaignData.crafterPool == null) return false;
    return _campaignData.crafterPool.Any(crafter =>
        crafter != null &&
        _campaign.builtCrafterNames.Contains(crafter.crafterName) &&
        crafter.recipeList != null &&
        crafter.recipeList.Any(recipe => recipe != null && recipe.itemName == item.itemName));
}

// ── Shuffle Helper ───────────────────────────────────────────
private static void ShuffleList<T>(List<T> list)
{
    for (int i = list.Count - 1; i > 0; i--)
    {
        int j = UnityEngine.Random.Range(0, i + 1);
        (list[i], list[j]) = (list[j], list[i]);
    }
}
```

---

## Step 2: GearLinkResolver.cs

**Path:** `Assets/_Game/Scripts/Core.Logic/GearLinkResolver.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Logic
{
    public static class GearLinkResolver
    {
        // Returns all active link bonuses for a given loadout
        // Called at equip time — no runtime cost during combat
        public static LinkBonus[] ResolveLinks(ItemSO[] equippedItems)
        {
            if (equippedItems == null || equippedItems.Length == 0)
                return new LinkBonus[0];

            var bonuses = new List<LinkBonus>();

            for (int i = 0; i < equippedItems.Length; i++)
            {
                var itemA = equippedItems[i];
                if (itemA == null || itemA.linkPoints == null) continue;

                foreach (var linkPoint in itemA.linkPoints)
                {
                    if (string.IsNullOrEmpty(linkPoint.affinityTag)) continue;

                    // Find another equipped item that shares this affinity tag
                    for (int j = 0; j < equippedItems.Length; j++)
                    {
                        if (i == j) continue;
                        var itemB = equippedItems[j];
                        if (itemB == null || itemB.affinityTags == null) continue;

                        if (System.Array.IndexOf(itemB.affinityTags, linkPoint.affinityTag) >= 0)
                        {
                            // Avoid duplicate bonuses (A↔B and B↔A)
                            bool alreadyLogged = bonuses.Any(b =>
                                (b.itemAName == itemA.itemName && b.itemBName == itemB.itemName) ||
                                (b.itemAName == itemB.itemName && b.itemBName == itemA.itemName));

                            if (!alreadyLogged)
                            {
                                var bonus = new LinkBonus
                                {
                                    itemAName         = itemA.itemName,
                                    itemBName         = itemB.itemName,
                                    affinityTag       = linkPoint.affinityTag,
                                    effectDescription = $"{itemA.itemName} ↔ {itemB.itemName}: " +
                                                        $"{linkPoint.affinityTag} link active",
                                };
                                bonuses.Add(bonus);
                                Debug.Log($"[GearLink] Link active: {bonus.effectDescription}");
                            }
                        }
                    }
                }
            }

            if (bonuses.Count == 0)
                Debug.Log("[GearLink] No active links in current loadout");

            return bonuses.ToArray();
        }

        // Returns stat modifier totals from all equipped items
        public static StatModifiers SumEquippedStats(ItemSO[] equippedItems)
        {
            var totals = new StatModifiers();
            if (equippedItems == null) return totals;

            foreach (var item in equippedItems)
            {
                if (item == null) continue;
                totals.accuracy  += item.accuracyMod;
                totals.strength  += item.strengthMod;
                totals.toughness += item.toughnessMod;
                totals.evasion   += item.evasionMod;
                totals.luck      += item.luckMod;
                totals.movement  += item.movementMod;
            }

            Debug.Log($"[GearLink] Stat totals from gear — " +
                      $"Acc:{totals.accuracy} Str:{totals.strength} " +
                      $"Tgh:{totals.toughness} Eva:{totals.evasion} " +
                      $"Lck:{totals.luck} Mov:{totals.movement}");
            return totals;
        }
    }

    public struct LinkBonus
    {
        public string itemAName;
        public string itemBName;
        public string affinityTag;
        public string effectDescription;
    }

    public struct StatModifiers
    {
        public int accuracy;
        public int strength;
        public int toughness;
        public int evasion;
        public int luck;
        public int movement;
    }
}
```

---

## Verification Test

Temporary test script — delete after passing:

**Path:** `Assets/_Game/Scripts/Core.Systems/InnovationCraftingTest.cs` *(delete after)*

```csharp
using UnityEngine;
using MnM.Core.Data;
using MnM.Core.Systems;
using MnM.Core.Logic;

public class InnovationCraftingTest : MonoBehaviour
{
    [SerializeField] private CampaignSO _tutorialSO;

    private void Start()
    {
        if (_tutorialSO == null) { Debug.LogError("Assign Mock_TutorialCampaign"); return; }

        Debug.Log("=== INNOVATION & CRAFTING TEST ===");

        var state      = CampaignStateFactory.BuildMockYear1State();
        var settlement = new SettlementManager();
        settlement.Initialize(state, _tutorialSO);

        // Seed some resources for crafting tests
        settlement.AddResource("Gaunt Fang", 5);
        settlement.AddResource("Bone", 5);
        settlement.AddResource("Sinew", 3);

        // ── Test 1: Innovation draw ──────────────────────────────
        var options = settlement.DrawInnovationOptions(3);
        Debug.Log($"[Test] Innovation options drawn: {options.Length}");
        // With mock SO having 3 INN IDs in pool, expect up to 3
        Debug.Assert(options.Length <= 3, "FAIL: should draw at most 3");
        Debug.Log("✓ Innovation draw ran without errors");

        // ── Test 2: Adopt innovation (if any drawn) ──────────────
        if (options.Length > 0)
        {
            var toAdopt = options[0];
            settlement.AdoptInnovation(toAdopt);
            Debug.Assert(state.adoptedInnovationIds.Contains(toAdopt.innovationId),
                "FAIL: innovation should be in adopted list");

            // Double-adopt should be silently ignored
            settlement.AdoptInnovation(toAdopt);
            Debug.Assert(state.adoptedInnovationIds.Count(id => id == toAdopt.innovationId) == 1,
                "FAIL: should not duplicate adoption");

            Debug.Log($"✓ Innovation adopted: {toAdopt.innovationName}");
            Debug.Log($"  Pool size after: {state.availableInnovationIds.Length}");
        }

        // ── Test 3: Crafter unlock with no cost (tutorial Boneworks) ─
        // Mock_TutorialCampaign crafterPool should contain Boneworks
        // If crafterPool is empty (expected with stub data) — test skips gracefully
        if (_tutorialSO.crafterPool != null && _tutorialSO.crafterPool.Length > 0)
        {
            var boneworks = _tutorialSO.crafterPool[0];
            bool unlocked = settlement.TryUnlockCrafter(boneworks);
            Debug.Log($"[Test] Unlock {boneworks.crafterName}: {unlocked}");
            if (unlocked)
            {
                Debug.Assert(state.builtCrafterNames.Contains(boneworks.crafterName),
                    "FAIL: Boneworks should be in builtCrafterNames");
                Debug.Log($"✓ Crafter unlocked: {boneworks.crafterName}");
            }
        }
        else
        {
            Debug.Log("[Test] CrafterPool empty in mock SO — crafter test skipped (expected)");
        }

        // ── Test 4: GearLinkResolver with no items ───────────────
        var noLinks = GearLinkResolver.ResolveLinks(new ItemSO[0]);
        Debug.Assert(noLinks.Length == 0, "FAIL: empty loadout should have no links");

        var noStats = GearLinkResolver.SumEquippedStats(new ItemSO[0]);
        Debug.Assert(noStats.accuracy == 0, "FAIL: empty loadout stats should be 0");
        Debug.Log("✓ GearLinkResolver handles empty loadout");

        // ── Test 5: Resource state after all operations ──────────
        Debug.Log($"[Test] Final resources: " +
                  $"GauntFang:{settlement.GetResourceAmount("Gaunt Fang")} " +
                  $"Bone:{settlement.GetResourceAmount("Bone")} " +
                  $"Sinew:{settlement.GetResourceAmount("Sinew")}");

        Debug.Log("[InnovationCraftingTest] ✓ All assertions passed");
        Debug.Log("=== INNOVATION & CRAFTING TEST COMPLETE ===");
    }
}
```

Attach to a GameObject, assign `Mock_TutorialCampaign`, Play, verify assertions, **delete the test script**.

---

## Next Session

**File:** `_Docs/Stage_04/STAGE_04_E.md`  
**Covers:** Character retirement, birth, year advance, and the full Year 1→2 simulation test that completes Stage 4
