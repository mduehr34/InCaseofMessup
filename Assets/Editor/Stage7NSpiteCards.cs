// One-shot script: fills The Spite's 17 stub behavior cards with Option B designs.
// Combat identity: wound resistance — wounds require higher Force Check threshold.
// Weakness: Fire. Resistance: Venom.
using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

public class Stage7NSpiteCards
{
    const string B = "Assets/_Game/Data/Cards/Behavior/Spite/";

    public static void Execute()
    {
        Fill("Spite_Relentless",        "Relentless",
            BehaviorGroup.Opening,
            "End of round",
            "Move 3 squares toward Aggro holder. If a wound was applied to The Spite this round, move 5 squares instead.");

        Fill("Spite_NoQuarter",         "No Quarter",
            BehaviorGroup.Opening,
            "Hunter plays a Reaction card",
            "Ignore the Reaction card's effect. Transfer Aggro to that hunter. Move 2 squares toward them.");

        Fill("Spite_Endure",            "Endure",
            BehaviorGroup.Opening,
            "End of round if The Spite has no wounds",
            "The Spite gains +1 Toughness until the start of next round.");

        Fill("Spite_WoundThrough",      "Wound Through",
            BehaviorGroup.Escalation,
            "Hunter deals a wound to The Spite",
            "Immediately move 2 squares toward that hunter. Apply Shaken to them. The wound is not cancelled.");

        Fill("Spite_IronHide",          "Iron Hide",
            BehaviorGroup.Escalation,
            "End of round if no wound was applied to The Spite this round",
            "The Spite's wound resistance threshold increases by 1 until a wound is applied (max +2, resets on wound).");

        Fill("Spite_BloodIgnore",       "Blood Ignore",
            BehaviorGroup.Escalation,
            "End of round if The Spite has 1 or more wounds",
            "Move full Movement toward Aggro holder. Standard attack at +1 Accuracy.");

        Fill("Spite_PersistentAggro",   "Persistent Aggro",
            BehaviorGroup.Escalation,
            "Hunter moves away from The Spite",
            "Transfer Aggro to that hunter. Move 3 squares toward them.");

        Fill("Spite_RelentlessCharge",  "Relentless Charge",
            BehaviorGroup.Escalation,
            "End of round if The Spite has not attacked this round",
            "Move 6 squares toward Aggro holder in a straight line. Standard attack on arrival.");

        Fill("Spite_GallBite",          "Gall Bite",
            BehaviorGroup.Escalation,
            "Hunter is in Front arc and adjacent",
            "Standard attack at +1 Strength. On hit, apply Bleeding. Venom has no effect on The Spite — Bleeding is its own toxin.");

        Fill("Spite_SpiteTurn",         "Spite Turn",
            BehaviorGroup.Escalation,
            "End of round if The Spite's back is to a hunter",
            "Immediately rotate to face that hunter. Transfer Aggro. No movement.");

        Fill("Spite_Untameable",        "Untameable",
            BehaviorGroup.Apex,
            "Hunter attempts to apply Pinned or Slowed to The Spite",
            "Status is ignored. Transfer Aggro to that hunter. The Spite moves 2 squares toward them.");

        Fill("Spite_ShedsBlood",        "Sheds Blood",
            BehaviorGroup.Apex,
            "End of round if The Spite has 2 or more wounds",
            "Move full Movement toward Aggro holder. Standard attack at +2 Accuracy and +1 Strength. The Spite does not slow down.");

        Fill("Spite_VenomRage",         "Venom Rage",
            BehaviorGroup.Apex,
            "Hunter applies Bleeding or Venom to The Spite",
            "Status is ignored. The Spite immediately makes a standard attack against that hunter at +1 Accuracy.");

        Fill("Spite_DeathDefying",      "Death Defying",
            BehaviorGroup.Apex,
            "The Spite receives a wound that would bring it to its final wound track",
            "Move 4 squares toward Aggro holder. Standard attack. Triggers once per hunt.");

        Fill("Spite_MarrowSense",       "Marrow Sense",
            BehaviorGroup.Apex,
            "End of round if any hunter has used a fire element this round",
            "Transfer Aggro to that hunter. Move full Movement toward them. +2 Accuracy on next standard attack.");

        Fill("Spite_FinalFury",         "Final Fury",
            BehaviorGroup.Apex,
            "End of round if The Spite has 3 or more wounds",
            "Move full Movement toward Aggro holder. Standard attack against all hunters in Front arc on arrival. Apply Shaken to all hit.");

        Fill("Spite_Tenacity",          "Tenacity",
            BehaviorGroup.Opening,
            "End of every round",
            "If no wound was applied to The Spite this round, increase the wound resistance threshold by 1 (max +3). Resets to base on any wound.",
            isPermanent: true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[7N-Spite] All 17 Spite behavior cards filled with Option B designs.");
    }

    static void Fill(string assetName, string cardName, BehaviorGroup group,
        string trigger, string effect, bool isPermanent = false)
    {
        string path = $"{B}{assetName}.asset";
        var card = AssetDatabase.LoadAssetAtPath<BehaviorCardSO>(path);
        if (!card) { Debug.LogWarning($"[7N-Spite] not found: {path}"); return; }

        card.cardName          = cardName;
        card.cardType          = isPermanent ? BehaviorCardType.Permanent : BehaviorCardType.Removable;
        card.group             = group;
        card.triggerCondition  = trigger;
        card.effectDescription = effect;
        EditorUtility.SetDirty(card);
    }
}
