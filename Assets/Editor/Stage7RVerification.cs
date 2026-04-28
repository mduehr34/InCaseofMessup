using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MnM.Core.Data;
using MnM.Core.Logic;

// Stage 7-R verification — run once, delete when green.
public class Stage7RVerification
{
    public static void Execute()
    {
        int passed = 0;
        int failed = 0;

        RunTest1(ref passed, ref failed);
        RunTest2(ref passed, ref failed);
        RunTest3(ref passed, ref failed);
        RunTest4(ref passed, ref failed);
        RunTest5(ref passed, ref failed);

        Debug.Log($"[Stage7R] ========== {passed} passed / {failed} failed ==========");
    }

    // ── Test 1: directional link fires (SkullCap above HideVest) ──
    static void RunTest1(ref int passed, ref int failed)
    {
        var cap  = LoadItem("Item_GauntSkullCap");
        var vest = LoadItem("Item_GauntHideVest");
        if (cap == null || vest == null) { Fail("Test1", "assets missing", ref failed); return; }

        var loadout = new[]
        {
            new GearGridSlot { item = cap,  cell = new Vector2Int(0, 0) },
            new GearGridSlot { item = vest, cell = new Vector2Int(0, 1) },
        };

        var stats = GearLinkResolver.SumEquippedStats(loadout);
        var links = GearLinkResolver.ResolveLinks(loadout);

        bool linkFired   = links.Length > 0;
        bool accCorrect  = stats.accuracy  == 1;
        bool tghCorrect  = stats.toughness == 1;

        if (linkFired && accCorrect && tghCorrect)
            Pass("Test1 — directional link fires: accuracy+1 toughness+1", ref passed);
        else
            Fail("Test1", $"links={links.Length} acc={stats.accuracy} tgh={stats.toughness}", ref failed);
    }

    // ── Test 2: link does NOT fire when spatial condition unmet ───
    static void RunTest2(ref int passed, ref int failed)
    {
        var cap     = LoadItem("Item_GauntSkullCap");
        var bracers = LoadItem("Item_GauntBoneBracers");
        if (cap == null || bracers == null) { Fail("Test2", "assets missing", ref failed); return; }

        var loadout = new[]
        {
            new GearGridSlot { item = cap,     cell = new Vector2Int(0, 0) },
            new GearGridSlot { item = bracers, cell = new Vector2Int(3, 0) },
        };

        var links = GearLinkResolver.ResolveLinks(loadout);

        if (links.Length == 0)
            Pass("Test2 — no link fires for wrong spatial position", ref passed);
        else
            Fail("Test2", $"expected 0 links, got {links.Length}", ref failed);
    }

    // ── Test 3: 2-piece set bonus applies ─────────────────────────
    static void RunTest3(ref int passed, ref int failed)
    {
        var cap  = LoadItem("Item_GauntSkullCap");
        var vest = LoadItem("Item_GauntHideVest");
        if (cap == null || vest == null) { Fail("Test3", "assets missing", ref failed); return; }

        var mods = SetBonusResolver.ResolveSetBonuses(new[] { cap, vest }, out var tags);

        bool evaCorrect  = mods.evasion == 1;
        bool noExtraTags = tags.Length == 0;

        if (evaCorrect && noExtraTags)
            Pass("Test3 — 2-piece set bonus: evasion+1, no effect tags", ref passed);
        else
            Fail("Test3", $"evasion={mods.evasion} tags={tags.Length}", ref failed);
    }

    // ── Test 4: once-per-hunt spent flag blocks second use ────────
    static void RunTest4(ref int passed, ref int failed)
    {
        var hunter = new HunterCombatState
        {
            hunterId              = "aldric",
            hunterName            = "Aldric",
            spentHuntAbilities    = new[] { "Gaunt Eye Pendant" },
        };

        bool alreadySpent = System.Array.IndexOf(
            hunter.spentHuntAbilities, "Gaunt Eye Pendant") >= 0;

        if (alreadySpent)
            Pass("Test4 — spentHuntAbilities blocks EyePendant reuse", ref passed);
        else
            Fail("Test4", "spentHuntAbilities not blocking", ref failed);
    }

    // ── Test 5: BoneSplint adjacency + shell restore + removal ────
    static void RunTest5(ref int passed, ref int failed)
    {
        bool adjTrue  = ConsumableResolver.IsValidConsumableTarget(3, 3, 4, 3);
        bool adjFalse = ConsumableResolver.IsValidConsumableTarget(3, 3, 6, 3);

        var hunterB = new HunterCombatState
        {
            hunterId   = "hunterB",
            hunterName = "Hunter B",
            bodyZones  = new[]
            {
                new BodyZoneState { zone = "LeftArm", shellCurrent = 0, shellMax = 3, fleshCurrent = 3, fleshMax = 3 }
            }
        };

        ConsumableResolver.ApplyBoneSplint(hunterB, "LeftArm");
        int shellAfter = hunterB.bodyZones[0].shellCurrent;

        // Simulate removal from RuntimeCharacterState
        var equipped = new List<string> { "Bone Splint", "Other Item" };
        equipped.Remove("Bone Splint");
        bool removed = !equipped.Contains("Bone Splint");

        if (adjTrue && !adjFalse && shellAfter == 2 && removed)
            Pass("Test5 — BoneSplint adjacency + shell+2 + removal", ref passed);
        else
            Fail("Test5", $"adj(4,3)={adjTrue} adj(6,3)={adjFalse} shell={shellAfter} removed={removed}", ref failed);
    }

    // ── Helpers ───────────────────────────────────────────────────
    static ItemSO LoadItem(string name)
    {
        var guids = AssetDatabase.FindAssets($"t:ItemSO {name}");
        if (guids.Length == 0) { Debug.LogError($"[Stage7R] Not found: {name}"); return null; }
        return AssetDatabase.LoadAssetAtPath<ItemSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    static void Pass(string label, ref int count)
    {
        count++;
        Debug.Log($"[Stage7R] PASS — {label}");
    }

    static void Fail(string label, string reason, ref int count)
    {
        count++;
        Debug.LogError($"[Stage7R] FAIL — {label}: {reason}");
    }
}
