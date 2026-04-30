using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    /// <summary>
    /// Static factory — creates a VisualElement card from any card SO type.
    /// Icons and frame require assets in Assets/Resources/Art/Generated/UI/.
    /// Degrades gracefully when assets are missing.
    /// </summary>
    public static class CardRenderer
    {
        private const string ResourcesBase = "Art/Generated/UI/CardIcons/";

        private static Sprite _frameSprite;
        private static bool   _iconsLoaded;
        private static readonly Dictionary<string, Sprite> _iconCache = new();

        // ── Icon key maps ─────────────────────────────────────────────────

        private static readonly Dictionary<CardCategory, string> CategoryIconKeys = new()
        {
            { CardCategory.Reaction,    "icon_reaction"    },
            { CardCategory.BasicAttack, "icon_basic_attack"},
            { CardCategory.Opener,      "icon_opener"      },
            { CardCategory.Linker,      "icon_linker"      },
            { CardCategory.Finisher,    "icon_finisher"    },
            { CardCategory.Signature,   "icon_signature"   },
        };

        // Sword arrived named ui_weapon_sword — mapped explicitly here.
        private static readonly Dictionary<WeaponType, string> WeaponIconKeys = new()
        {
            { WeaponType.FistWeapon,     "icon_weapon_fist"       },
            { WeaponType.Dagger,         "icon_weapon_dagger"     },
            { WeaponType.SwordAndShield, "icon_weapon_sword"      },
            { WeaponType.Axe,            "icon_weapon_axe"        },
            { WeaponType.HammerMaul,     "icon_weapon_hammer"     },
            { WeaponType.Spear,          "icon_weapon_spear"      },
            { WeaponType.Greatsword,     "icon_weapon_greatsword" },
            { WeaponType.Bow,            "icon_weapon_bow"        },
        };

        // ── Icon preload ──────────────────────────────────────────────────

        private static void EnsureIconsLoaded()
        {
            if (_iconsLoaded) return;
            _iconsLoaded = true;

            foreach (var kvp in CategoryIconKeys)
                LoadIcon(kvp.Value);

            foreach (var kvp in WeaponIconKeys)
                LoadIcon(kvp.Value);
        }

        private static void LoadIcon(string key)
        {
            if (_iconCache.ContainsKey(key)) return;
            var sprite = Resources.Load<Sprite>(ResourcesBase + key);
            if (sprite != null)
                _iconCache[key] = sprite;
            else
                Debug.LogWarning($"[CardRenderer] Icon not found: {ResourcesBase}{key} " +
                                 $"(move PNGs to Assets/Resources/{ResourcesBase})");
        }

        private static Sprite GetIcon(string key)
        {
            _iconCache.TryGetValue(key, out var sprite);
            return sprite;
        }

        // ── Action Cards ─────────────────────────────────────────────────

        public static VisualElement BuildActionCard(ActionCardSO card, bool isPlayable = true)
        {
            EnsureIconsLoaded();

            var root = MakeCardShell(isPlayable);

            CategoryIconKeys.TryGetValue(card.category, out var catIconKey);
            root.Add(MakeBand(card.category.ToString(), CategoryColor(card.category),
                catIconKey != null ? GetIcon(catIconKey) : null));

            root.Add(MakeNameLabel(card.cardName));

            // Weapon icon + type label + AP cost row
            var statRow = new VisualElement();
            statRow.style.flexDirection  = FlexDirection.Row;
            statRow.style.justifyContent = Justify.SpaceBetween;
            statRow.style.alignItems     = Align.Center;
            statRow.style.paddingLeft    = 8;
            statRow.style.paddingRight   = 8;
            statRow.style.marginBottom   = 4;

            var weaponLeft = new VisualElement();
            weaponLeft.style.flexDirection = FlexDirection.Row;
            weaponLeft.style.alignItems    = Align.Center;

            WeaponIconKeys.TryGetValue(card.weaponType, out var wpnIconKey);
            var wpnSprite = wpnIconKey != null ? GetIcon(wpnIconKey) : null;
            if (wpnSprite != null)
            {
                var wpnIcon = new VisualElement();
                wpnIcon.style.width        = 14;
                wpnIcon.style.height       = 14;
                wpnIcon.style.marginRight  = 3;
                wpnIcon.style.backgroundImage = new StyleBackground(wpnSprite);
                wpnIcon.style.backgroundSize  = new BackgroundSize(
                    new Length(100, LengthUnit.Percent), new Length(100, LengthUnit.Percent));
                weaponLeft.Add(wpnIcon);
            }

            var weaponLabel = new Label(card.weaponType.ToString());
            weaponLabel.style.color    = new Color(0.54f, 0.54f, 0.54f);
            weaponLabel.style.fontSize = 10;
            weaponLeft.Add(weaponLabel);
            statRow.Add(weaponLeft);

            int    netAP  = card.apCost - card.apRefund;
            string apText = $"{card.apCost} AP" + (card.apRefund > 0 ? $" ({netAP} net)" : "");
            var apLabel = new Label(apText);
            apLabel.style.color    = netAP <= 0
                ? new Color(0.4f, 0.7f, 0.4f)
                : new Color(0.83f, 0.80f, 0.73f);
            apLabel.style.fontSize = 10;
            statRow.Add(apLabel);
            root.Add(statRow);

            if (card.isLoud)
            {
                var loud = new Label("◆ LOUD");
                loud.style.color        = new Color(0.8f, 0.3f, 0.3f);
                loud.style.fontSize     = 10;
                loud.style.alignSelf    = Align.Center;
                loud.style.marginBottom = 4;
                root.Add(loud);
            }

            root.Add(MakeEffectText(card.effectDescription));

            if (card.proficiencyTierRequired > 0)
            {
                var tier = new Label($"TIER {card.proficiencyTierRequired}");
                tier.style.position = Position.Absolute;
                tier.style.bottom   = 6;
                tier.style.right    = 8;
                tier.style.color    = new Color(0.72f, 0.52f, 0.04f);
                tier.style.fontSize = 10;
                root.Add(tier);
            }

            // ApplyFrame(root); — re-enable once a transparent-centre frame PNG is ready
            return root;
        }

        // ── Behavior Cards ───────────────────────────────────────────────

        public static VisualElement BuildBehaviorCard(BehaviorCardSO card)
        {
            EnsureIconsLoaded();

            var behaviorRed = new Color(0.5f, 0.1f, 0.1f);
            var root = MakeCardShell(false);
            root.style.borderTopColor    = new StyleColor(behaviorRed);
            root.style.borderBottomColor = new StyleColor(behaviorRed);
            root.style.borderLeftColor   = new StyleColor(behaviorRed);
            root.style.borderRightColor  = new StyleColor(behaviorRed);

            root.Add(MakeBand("BEHAVIOR", behaviorRed, null));
            root.Add(MakeNameLabel(card.cardName));

            var triggerLabel = new Label($"TRIGGER: {card.triggerCondition}");
            triggerLabel.style.color        = new Color(0.72f, 0.52f, 0.04f);
            triggerLabel.style.fontSize     = 10;
            triggerLabel.style.paddingLeft  = 8;
            triggerLabel.style.paddingRight = 8;
            triggerLabel.style.marginBottom = 4;
            triggerLabel.style.whiteSpace   = WhiteSpace.Normal;
            root.Add(triggerLabel);

            root.Add(MakeEffectText(card.effectDescription));

            if (!string.IsNullOrEmpty(card.removalCondition))
            {
                var removal = new Label($"REMOVE: {card.removalCondition}");
                removal.style.color        = new Color(0.54f, 0.54f, 0.54f);
                removal.style.fontSize     = 10;
                removal.style.paddingLeft  = 8;
                removal.style.paddingRight = 8;
                removal.style.whiteSpace   = WhiteSpace.Normal;
                root.Add(removal);
            }

            // ApplyFrame(root); — re-enable once a transparent-centre frame PNG is ready
            return root;
        }

        // ── Innovation Cards ─────────────────────────────────────────────

        public static VisualElement BuildInnovationCard(InnovationSO inn, bool isAdopted)
        {
            EnsureIconsLoaded();

            var root = MakeCardShell(!isAdopted);
            if (isAdopted)
                root.style.opacity = 0.5f;

            var gold = new Color(0.72f, 0.52f, 0.04f);
            root.Add(MakeBand("INNOVATION", gold, null));
            root.Add(MakeNameLabel(inn.innovationName));
            root.Add(MakeEffectText(inn.effect));

            if (!string.IsNullOrEmpty(inn.gritSkillUnlocked))
            {
                var skill = new Label($"GRIT SKILL: {inn.gritSkillUnlocked}");
                skill.style.color       = new Color(0.54f, 0.54f, 0.54f);
                skill.style.fontSize    = 10;
                skill.style.paddingLeft = 8;
                root.Add(skill);
            }

            var statusLabel = new Label(isAdopted ? "✓ ADOPTED" : "ADOPT");
            statusLabel.style.color     = isAdopted ? new Color(0.4f, 0.7f, 0.4f) : gold;
            statusLabel.style.fontSize  = 10;
            statusLabel.style.alignSelf = Align.Center;
            statusLabel.style.marginTop = 8;
            root.Add(statusLabel);

            // ApplyFrame(root); — re-enable once a transparent-centre frame PNG is ready
            return root;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static VisualElement MakeCardShell(bool interactive)
        {
            var root = new VisualElement();
            root.style.width             = 220;
            root.style.minHeight         = 0;
            root.style.flexDirection     = FlexDirection.Column;
            root.style.backgroundColor   = new Color(0.08f, 0.06f, 0.04f);
            root.style.borderTopWidth    = 2;
            root.style.borderBottomWidth = 2;
            root.style.borderLeftWidth   = 2;
            root.style.borderRightWidth  = 2;
            var defaultBorder = new StyleColor(new Color(0.31f, 0.27f, 0.20f));
            root.style.borderTopColor    = defaultBorder;
            root.style.borderBottomColor = defaultBorder;
            root.style.borderLeftColor   = defaultBorder;
            root.style.borderRightColor  = defaultBorder;
            root.style.overflow          = Overflow.Hidden;

            if (interactive)
            {
                var hoverBorder   = new StyleColor(new Color(0.72f, 0.52f, 0.04f));
                var restoreBorder = new StyleColor(new Color(0.31f, 0.27f, 0.20f));
                root.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    root.style.borderTopColor    = hoverBorder;
                    root.style.borderBottomColor = hoverBorder;
                    root.style.borderLeftColor   = hoverBorder;
                    root.style.borderRightColor  = hoverBorder;
                });
                root.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    root.style.borderTopColor    = restoreBorder;
                    root.style.borderBottomColor = restoreBorder;
                    root.style.borderLeftColor   = restoreBorder;
                    root.style.borderRightColor  = restoreBorder;
                });
            }
            return root;
        }

        // Band with optional left-aligned icon and centered text
        private static VisualElement MakeBand(string text, Color color, Sprite icon)
        {
            var band = new VisualElement();
            band.style.height          = 28;
            band.style.backgroundColor = new StyleColor(color);
            band.style.flexDirection   = FlexDirection.Row;
            band.style.alignItems      = Align.Center;
            band.style.paddingLeft     = 6;
            band.style.paddingRight    = 6;

            if (icon != null)
            {
                var iconEl = new VisualElement();
                iconEl.style.width        = 18;
                iconEl.style.height       = 18;
                iconEl.style.marginRight  = 4;
                iconEl.style.flexShrink   = 0;
                iconEl.style.backgroundImage = new StyleBackground(icon);
                iconEl.style.backgroundSize  = new BackgroundSize(
                    new Length(100, LengthUnit.Percent), new Length(100, LengthUnit.Percent));
                band.Add(iconEl);
            }

            var label = new Label(text.ToUpper());
            label.style.color                   = Color.white;
            label.style.fontSize                = 11;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.flexGrow                = 1;
            band.Add(label);
            return band;
        }

        private static Label MakeNameLabel(string name)
        {
            var l = new Label(name.ToUpper());
            l.style.color                   = new Color(0.83f, 0.80f, 0.73f);
            l.style.fontSize                = 15;
            l.style.paddingLeft             = 8;
            l.style.paddingRight            = 8;
            l.style.paddingTop              = 8;
            l.style.paddingBottom           = 6;
            l.style.whiteSpace              = WhiteSpace.Normal;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            return l;
        }

        private static Label MakeEffectText(string text)
        {
            var l = new Label(text);
            l.style.color        = new Color(0.70f, 0.68f, 0.62f);
            l.style.fontSize     = 11;
            l.style.paddingLeft  = 8;
            l.style.paddingRight = 8;
            l.style.paddingTop   = 4;
            l.style.whiteSpace   = WhiteSpace.Normal;
            l.style.flexGrow     = 1;
            return l;
        }

        private static Color CategoryColor(CardCategory cat) => cat switch
        {
            CardCategory.Reaction    => new Color(0.16f, 0.29f, 0.42f),
            CardCategory.BasicAttack => new Color(0.29f, 0.13f, 0.13f),
            CardCategory.Opener      => new Color(0.23f, 0.29f, 0.13f),
            CardCategory.Linker      => new Color(0.29f, 0.23f, 0.06f),
            CardCategory.Finisher    => new Color(0.35f, 0.10f, 0.10f),
            CardCategory.Signature   => new Color(0.72f, 0.52f, 0.04f),
            _                        => new Color(0.20f, 0.18f, 0.14f),
        };

        private static void ApplyFrame(VisualElement root)
        {
            if (_frameSprite == null)
                _frameSprite = Resources.Load<Sprite>("Art/Generated/UI/ui_card_frame");
            if (_frameSprite == null) return;

            var overlay = new VisualElement();
            overlay.style.position        = Position.Absolute;
            overlay.style.left            = 0;
            overlay.style.top             = 0;
            overlay.style.right           = 0;
            overlay.style.bottom          = 0;
            overlay.style.backgroundImage = new StyleBackground(_frameSprite);
            overlay.style.backgroundSize  = new BackgroundSize(
                new Length(100, LengthUnit.Percent), new Length(100, LengthUnit.Percent));
            overlay.pickingMode           = PickingMode.Ignore;
            root.Add(overlay);
        }
    }
}
