using UnityEngine;
using UnityEditor;
using MnM.Core.Data;

namespace MnM.Editor
{
    public class SetupCrafterAssets
    {
        // ── Crafter definitions ───────────────────────────────────────────────
        private struct CrafterDef
        {
            public string crafterName;
            public string monsterTag;
            public string spritePath;
            public Vector2 scenePos;
        }

        private static readonly CrafterDef[] Crafters = new CrafterDef[]
        {
            new CrafterDef { crafterName = "The Ossuary",        monsterTag = "Gaunt",           spritePath = "Assets/_Game/Art/Generated/Settlement/the_ossuary.png",         scenePos = new Vector2(20,  60) },
            new CrafterDef { crafterName = "The Carapace Forge", monsterTag = "Thornback",        spritePath = "Assets/_Game/Art/Generated/Settlement/the_carapace_forge.png",  scenePos = new Vector2(120, 60) },
            new CrafterDef { crafterName = "The Mire Apothecary",monsterTag = "BogCaller",        spritePath = "Assets/_Game/Art/Generated/Settlement/the_mire_apothecary.png", scenePos = new Vector2(210, 60) },
            new CrafterDef { crafterName = "The Membrane Loft",  monsterTag = "Shriek",           spritePath = "Assets/_Game/Art/Generated/Settlement/the_membrane_loft.png",   scenePos = new Vector2(290, 60) },
            new CrafterDef { crafterName = "The Ichor Works",    monsterTag = "Spite",            spritePath = "Assets/_Game/Art/Generated/Settlement/the_ichor_works.png",     scenePos = new Vector2(370, 60) },
            new CrafterDef { crafterName = "The Auric Scales",   monsterTag = "GildedSerpent",    spritePath = "Assets/_Game/Art/Generated/Settlement/the_auric_scales.png",    scenePos = new Vector2(450, 60) },
            new CrafterDef { crafterName = "The Rot Garden",     monsterTag = "Rotmother",        spritePath = "Assets/_Game/Art/Generated/Settlement/the_rot_garden.png",      scenePos = new Vector2(540, 60) },
            new CrafterDef { crafterName = "The Ivory Hall",     monsterTag = "IvoryStampede",    spritePath = "Assets/_Game/Art/Generated/Settlement/the_ivory_hall.png",      scenePos = new Vector2(630, 60) },
        };

        public static void Execute()
        {
            FixSpriteImportSettings();
            var crafterAssets = CreateOrUpdateCrafterSOs();
            UpdateCampaignSO(crafterAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SetupCrafterAssets] Done — 8 CrafterSOs created/updated, sprites fixed, campaign pool updated.");
        }

        // ── 1. Fix import settings on all UI and Settlement sprites ──────────
        private static void FixSpriteImportSettings()
        {
            string[] folders = new[]
            {
                "Assets/_Game/Art/Generated/UI",
                "Assets/_Game/Art/Generated/Settlement"
            };

            foreach (var folder in folders)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    bool changed = false;

                    if (importer.textureType != TextureImporterType.Sprite)          { importer.textureType          = TextureImporterType.Sprite;     changed = true; }
                    if (importer.spritePixelsPerUnit != 16f)                           { importer.spritePixelsPerUnit  = 16f;                            changed = true; }
                    if (importer.filterMode != FilterMode.Point)                       { importer.filterMode           = FilterMode.Point;               changed = true; }
                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }

                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                        Debug.Log($"[SetupCrafterAssets] Fixed import settings: {path}");
                    }
                }
            }
        }

        // ── 2. Create or update CrafterSO assets ─────────────────────────────
        private static CrafterSO[] CreateOrUpdateCrafterSOs()
        {
            string outputFolder = "Assets/_Game/Data/Crafters";

            var results = new CrafterSO[Crafters.Length];

            for (int i = 0; i < Crafters.Length; i++)
            {
                var def = Crafters[i];
                string safeName = def.crafterName.Replace(" ", "");
                string assetPath = $"{outputFolder}/Crafter_{safeName}.asset";

                // Load existing or create new
                var so = AssetDatabase.LoadAssetAtPath<CrafterSO>(assetPath);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<CrafterSO>();
                    AssetDatabase.CreateAsset(so, assetPath);
                    Debug.Log($"[SetupCrafterAssets] Created: {assetPath}");
                }

                so.crafterName             = def.crafterName;
                so.monsterTag              = def.monsterTag;
                so.settlementScenePosition = def.scenePos;

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(def.spritePath);
                if (sprite != null)
                {
                    so.structureSprite = sprite;
                    Debug.Log($"[SetupCrafterAssets] Sprite assigned: {def.crafterName} → {def.spritePath}");
                }
                else
                {
                    Debug.LogWarning($"[SetupCrafterAssets] Sprite NOT FOUND: {def.spritePath}");
                }

                EditorUtility.SetDirty(so);
                results[i] = so;
            }

            return results;
        }

        // ── 3. Assign crafterPool on the campaign SO ──────────────────────────
        private static void UpdateCampaignSO(CrafterSO[] crafters)
        {
            string campaignPath = "Assets/_Game/Data/Campaigns/Mock_TutorialCampaign.asset";
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignSO>(campaignPath);
            if (campaign == null)
            {
                Debug.LogWarning($"[SetupCrafterAssets] CampaignSO not found at {campaignPath}");
                return;
            }

            campaign.crafterPool = crafters;
            EditorUtility.SetDirty(campaign);
            Debug.Log($"[SetupCrafterAssets] Updated crafterPool on {campaign.campaignName} with {crafters.Length} crafters.");
        }
    }
}
