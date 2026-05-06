using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class Stage8L_SetupGauntCards
{
    public static void Execute()
    {
        // ── 1. Load the Gaunt MonsterSO ──────────────────────────────────
        var gaunt = AssetDatabase.LoadAssetAtPath<MonsterSO>(
            "Assets/_Game/Data/Monsters/Monster_Gaunt.asset");
        if (gaunt == null)
        {
            Debug.LogError("[8L Setup] Monster_Gaunt.asset not found");
            return;
        }

        // ── 2. Create / update each behavior card ────────────────────────
        var creep   = GetOrCreateCard("Assets/_Game/Data/Cards/Behavior/Gaunt_CreepingAdvance.asset",
                                      "Creeping Advance",    BehaviorGroup.Opening);
        var lunge   = GetOrCreateCard("Assets/_Game/Data/Cards/Behavior/Gaunt_LungeStrike.asset",
                                      "Lunge Strike",        BehaviorGroup.Opening);
        var sweep   = GetOrCreateCard("Assets/_Game/Data/Cards/Behavior/Gaunt_SweepingFlail.asset",
                                      "Sweeping Flail",      BehaviorGroup.Escalation);
        var stillness = GetOrCreateCard("Assets/_Game/Data/Cards/Behavior/Gaunt_DeadStillness.asset",
                                        "Dead Stillness",    BehaviorGroup.Opening);

        // Creeping Advance — Approach 1 cell, no attack
        creep.cardType           = BehaviorCardType.Removable;
        creep.triggerCondition   = "Always";
        creep.effectDescription  = "The Gaunt creeps one step toward its prey.";
        creep.movementPattern    = MovementPattern.Approach;
        creep.movementDistance   = 1;
        creep.attackTargetType   = AttackTargetType.None;
        creep.attackDamage       = 0;
        EditorUtility.SetDirty(creep);

        // Lunge Strike — no move, hit aggro target for 2 flesh
        lunge.cardType           = BehaviorCardType.Removable;
        lunge.triggerCondition   = "Always";
        lunge.effectDescription  = "The Gaunt lunges, driving its fang into the aggro holder.";
        lunge.movementPattern    = MovementPattern.None;
        lunge.movementDistance   = 0;
        lunge.attackTargetType   = AttackTargetType.AggroTarget;
        lunge.attackDamage       = 2;
        lunge.attackRange        = 1;
        EditorUtility.SetDirty(lunge);

        // Sweeping Flail — no move, hit all adjacent for 1 flesh
        sweep.cardType           = BehaviorCardType.Removable;
        sweep.triggerCondition   = "Always";
        sweep.effectDescription  = "The Gaunt sweeps wide, striking all hunters within reach.";
        sweep.movementPattern    = MovementPattern.None;
        sweep.movementDistance   = 0;
        sweep.attackTargetType   = AttackTargetType.AllAdjacent;
        sweep.attackDamage       = 1;
        sweep.attackRange        = 1;
        EditorUtility.SetDirty(sweep);

        // Dead Stillness — no move, no attack (pause card)
        stillness.cardType           = BehaviorCardType.Removable;
        stillness.triggerCondition   = "Always";
        stillness.effectDescription  = "The Gaunt freezes, observing. Nothing happens.";
        stillness.movementPattern    = MovementPattern.None;
        stillness.movementDistance   = 0;
        stillness.attackTargetType   = AttackTargetType.None;
        stillness.attackDamage       = 0;
        EditorUtility.SetDirty(stillness);

        // ── 3. Also update the old Mock_CreepingAdvance if it exists ─────
        var oldMock = AssetDatabase.LoadAssetAtPath<BehaviorCardSO>(
            "Assets/_Game/Data/Cards/Behavior/Mock_CreepingAdvance.asset");
        if (oldMock != null)
        {
            oldMock.movementPattern  = MovementPattern.Approach;
            oldMock.movementDistance = 1;
            oldMock.attackTargetType = AttackTargetType.None;
            oldMock.attackDamage     = 0;
            EditorUtility.SetDirty(oldMock);
        }

        // ── 4. Assign cards to Gaunt's behavior deck ─────────────────────
        gaunt.openingCards    = new BehaviorCardSO[] { creep, lunge, stillness };
        gaunt.escalationCards = new BehaviorCardSO[] { sweep };
        gaunt.apexCards       = new BehaviorCardSO[0];
        gaunt.permanentCards  = new BehaviorCardSO[0];
        EditorUtility.SetDirty(gaunt);

        // ── 5. Save all ───────────────────────────────────────────────────
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[8L Setup] Gaunt behavior cards created and assigned:\n" +
                  $"  Opening:    {creep.cardName}, {lunge.cardName}, {stillness.cardName}\n" +
                  $"  Escalation: {sweep.cardName}");
    }

    private static BehaviorCardSO GetOrCreateCard(string path, string displayName, BehaviorGroup group)
    {
        var existing = AssetDatabase.LoadAssetAtPath<BehaviorCardSO>(path);
        if (existing != null)
        {
            existing.cardName = displayName;
            existing.group    = group;
            return existing;
        }

        var card = ScriptableObject.CreateInstance<BehaviorCardSO>();
        card.cardName = displayName;
        card.group    = group;

        // Ensure directory exists
        string dir = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(dir))
        {
            string parent = System.IO.Path.GetDirectoryName(dir).Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(dir);
            AssetDatabase.CreateFolder(parent, folder);
        }

        AssetDatabase.CreateAsset(card, path);
        Debug.Log($"[8L Setup] Created {path}");
        return card;
    }
}
