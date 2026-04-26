<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-H | Card Visual Rendering System
Status: Stage 8-G complete. Combat UI polish done.
Task: Generate the stone-carved card frame art. Build
CardRenderer — a reusable component that takes any
ActionCardSO and renders it as a visual card: frame,
category colour band, weapon type icon, card name, AP cost,
and effect text. Apply to the hand display in combat.
Also render BehaviorCardSO and InnovationSO cards using
the same frame with different colour accents.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_H.md
- Assets/_Game/Scripts/Core.Data/ActionCardSO.cs
- Assets/_Game/Scripts/Core.Data/BehaviorCardSO.cs
- Assets/_Game/Scripts/Core.Data/InnovationSO.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- The card frame PNG was already imported in Stage 7-E (ui_card_frame.png)
- CardRenderer is a standalone VisualElement factory — not a MonoBehaviour
- Different card types use the same frame with different accent colours
- Behaviour cards show trigger text, not AP cost
- Effect text wraps inside the card — no overflow

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-H: Card Visual Rendering System

**Resuming from:** Stage 8-G complete — combat UI polish done
**Done when:** Hand cards in combat display as rendered card visuals with name, AP, category, and effect text; behavior cards display similarly in the monster panel; innovation cards display in the settlement
**Commit:** `"8H: Card visual rendering system — ActionCard, BehaviorCard, Innovation renders"`
**Next session:** STAGE_08_I.md

---

## Card Visual Spec

All cards share the same base frame (`ui_card_frame.png` — already imported Stage 7-E).
Cards are **160×220 px** rendered at game resolution.

**Category colour accents (top band):**
| Category | Colour |
|---|---|
| Reaction | `#2A4A6A` (cold blue) |
| BasicAttack | `#4A2020` (dried blood) |
| Opener | `#3A4A20` (dark green) |
| Linker | `#4A3A10` (dark amber) |
| Finisher | `#5A1A1A` (deep crimson) |
| Signature | `#B8860B` (marrow gold) |

---

## Step 1: Generate Category Icons

Use CoPlay `generate_or_edit_images` for each. **16×16 px, transparent bg, pixel art.**
Save to `Assets/_Game/Art/Generated/UI/CardIcons/`

| Filename | Icon Description |
|---|---|
| `icon_reaction.png` | A shield with an arrow bouncing off it. Cold blue. |
| `icon_basic_attack.png` | A simple sword or fist strike line. Bone white. |
| `icon_opener.png` | An opening door or gateway symbol. Dark green. |
| `icon_linker.png` | Two chain links connected. Amber. |
| `icon_finisher.png` | A skull or broken bone. Deep crimson. |
| `icon_signature.png` | A star or crown. Marrow gold. |
| `icon_weapon_fist.png` | A clenched fist. |
| `icon_weapon_spear.png` | Spear tip pointing up. |
| `icon_weapon_axe.png` | Axe silhouette. |
| `icon_weapon_dagger.png` | Dagger silhouette. |
| `icon_weapon_bow.png` | Bow and arrow silhouette. |
| `icon_weapon_sword.png` | Sword and shield silhouette. |
| `icon_weapon_hammer.png` | Hammer/maul silhouette. |
| `icon_weapon_greatsword.png` | Two-handed sword silhouette. |

Import all: Point (No Filter), PPU 16, Compression None

---

## Step 2: CardRenderer.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CardRenderer.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    /// <summary>
    /// Static factory — creates a VisualElement card from any card SO type.
    /// Call CardRenderer.BuildActionCard(so) to get a ready-to-add VisualElement.
    /// </summary>
    public static class CardRenderer
    {
        private static Sprite _frameSprite;

        // ── Action Cards ────────────────────────────────────────────────

        public static VisualElement BuildActionCard(ActionCardSO card, bool isPlayable = true)
        {
            var root = MakeCardShell(isPlayable);

            // Top colour band + category icon
            var band = MakeBand(card.category.ToString(), CategoryColor(card.category));
            root.Add(band);

            // Card name
            root.Add(MakeNameLabel(card.cardName));

            // Weapon type + AP cost row
            var statRow = new VisualElement();
            statRow.style.flexDirection = FlexDirection.Row;
            statRow.style.justifyContent = Justify.SpaceBetween;
            statRow.style.paddingLeft = statRow.style.paddingRight = 8;
            statRow.style.marginBottom = 4;

            var weaponLabel = new Label(card.weaponType.ToString());
            weaponLabel.style.color    = new Color(0.54f, 0.54f, 0.54f);
            weaponLabel.style.fontSize = 8;
            statRow.Add(weaponLabel);

            int netAP = card.apCost - card.apRefund;
            var apLabel = new Label($"{card.apCost} AP" + (card.apRefund > 0 ? $" ({netAP} net)" : ""));
            apLabel.style.color    = netAP <= 0 ? new Color(0.4f, 0.7f, 0.4f) : new Color(0.83f, 0.80f, 0.73f);
            apLabel.style.fontSize = 8;
            statRow.Add(apLabel);
            root.Add(statRow);

            // Loud indicator
            if (card.isLoud)
            {
                var loud = new Label("◆ LOUD");
                loud.style.color     = new Color(0.8f, 0.3f, 0.3f);
                loud.style.fontSize  = 7;
                loud.style.alignSelf = Align.Center;
                loud.style.marginBottom = 4;
                root.Add(loud);
            }

            // Effect text
            root.Add(MakeEffectText(card.effectDescription));

            // Tier indicator
            if (card.proficiencyTierRequired > 0)
            {
                var tier = new Label($"TIER {card.proficiencyTierRequired}");
                tier.style.position = Position.Absolute;
                tier.style.bottom   = 6;
                tier.style.right    = 8;
                tier.style.color    = new Color(0.72f, 0.52f, 0.04f);
                tier.style.fontSize = 7;
                root.Add(tier);
            }

            ApplyFrame(root);
            return root;
        }

        // ── Behavior Cards ──────────────────────────────────────────────

        public static VisualElement BuildBehaviorCard(BehaviorCardSO card)
        {
            var root = MakeCardShell(false);
            root.style.borderTopColor = new StyleColor(new Color(0.5f, 0.1f, 0.1f));

            root.Add(MakeBand("BEHAVIOR", new Color(0.5f, 0.1f, 0.1f)));
            root.Add(MakeNameLabel(card.cardName));

            var triggerLabel = new Label($"TRIGGER: {card.triggerCondition}");
            triggerLabel.style.color     = new Color(0.72f, 0.52f, 0.04f);
            triggerLabel.style.fontSize  = 7;
            triggerLabel.style.paddingLeft  = 8;
            triggerLabel.style.paddingRight = 8;
            triggerLabel.style.marginBottom = 4;
            triggerLabel.style.whiteSpace   = WhiteSpace.Normal;
            root.Add(triggerLabel);

            root.Add(MakeEffectText(card.effectDescription));

            if (!string.IsNullOrEmpty(card.removalCondition))
            {
                var removal = new Label($"REMOVE: {card.removalCondition}");
                removal.style.color     = new Color(0.54f, 0.54f, 0.54f);
                removal.style.fontSize  = 7;
                removal.style.paddingLeft  = 8;
                removal.style.paddingRight = 8;
                removal.style.whiteSpace   = WhiteSpace.Normal;
                root.Add(removal);
            }

            ApplyFrame(root);
            return root;
        }

        // ── Innovation Cards ────────────────────────────────────────────

        public static VisualElement BuildInnovationCard(InnovationSO inn, bool isAdopted)
        {
            var root = MakeCardShell(!isAdopted);
            if (isAdopted)
                root.style.opacity = 0.5f;

            var gold = new Color(0.72f, 0.52f, 0.04f);
            root.Add(MakeBand("INNOVATION", gold));
            root.Add(MakeNameLabel(inn.innovationName));
            root.Add(MakeEffectText(inn.effectDescription));

            if (!string.IsNullOrEmpty(inn.gritSkillName))
            {
                var skill = new Label($"GRIT SKILL: {inn.gritSkillName}");
                skill.style.color    = new Color(0.54f, 0.54f, 0.54f);
                skill.style.fontSize = 7;
                skill.style.paddingLeft = 8;
                root.Add(skill);
            }

            var adoptedLabel = new Label(isAdopted ? "✓ ADOPTED" : "ADOPT");
            adoptedLabel.style.color     = isAdopted ? new Color(0.4f, 0.7f, 0.4f) : gold;
            adoptedLabel.style.fontSize  = 8;
            adoptedLabel.style.alignSelf = Align.Center;
            adoptedLabel.style.marginTop = 8;
            root.Add(adoptedLabel);

            ApplyFrame(root);
            return root;
        }

        // ── Helpers ─────────────────────────────────────────────────────

        private static VisualElement MakeCardShell(bool interactive)
        {
            var root = new VisualElement();
            root.style.width           = 160;
            root.style.minHeight       = 220;
            root.style.flexDirection   = FlexDirection.Column;
            root.style.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
            root.style.borderTopWidth  = root.style.borderBottomWidth =
            root.style.borderLeftWidth = root.style.borderRightWidth = 2;
            root.style.borderTopColor  = root.style.borderBottomColor =
            root.style.borderLeftColor = root.style.borderRightColor =
                new StyleColor(new Color(0.31f, 0.27f, 0.20f));
            root.style.overflow = Overflow.Hidden;

            if (interactive)
            {
                root.RegisterCallback<MouseEnterEvent>(_ =>
                    root.style.borderTopColor = root.style.borderBottomColor =
                    root.style.borderLeftColor = root.style.borderRightColor =
                        new StyleColor(new Color(0.72f, 0.52f, 0.04f)));
                root.RegisterCallback<MouseLeaveEvent>(_ =>
                    root.style.borderTopColor = root.style.borderBottomColor =
                    root.style.borderLeftColor = root.style.borderRightColor =
                        new StyleColor(new Color(0.31f, 0.27f, 0.20f)));
            }
            return root;
        }

        private static VisualElement MakeBand(string text, Color color)
        {
            var band = new VisualElement();
            band.style.height          = 20;
            band.style.backgroundColor = new StyleColor(color);
            band.style.alignItems      = Align.Center;
            band.style.justifyContent  = Justify.Center;

            var label = new Label(text.ToUpper());
            label.style.color    = Color.white;
            label.style.fontSize = 8;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            band.Add(label);
            return band;
        }

        private static Label MakeNameLabel(string name)
        {
            var l = new Label(name.ToUpper());
            l.style.color       = new Color(0.83f, 0.80f, 0.73f);
            l.style.fontSize    = 11;
            l.style.paddingLeft = l.style.paddingRight = 8;
            l.style.paddingTop  = 6;
            l.style.paddingBottom = 4;
            l.style.whiteSpace  = WhiteSpace.Normal;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            return l;
        }

        private static Label MakeEffectText(string text)
        {
            var l = new Label(text);
            l.style.color       = new Color(0.70f, 0.68f, 0.62f);
            l.style.fontSize    = 8;
            l.style.paddingLeft = l.style.paddingRight = 8;
            l.style.whiteSpace  = WhiteSpace.Normal;
            l.style.flexGrow    = 1;
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
            _                        => new Color(0.20f, 0.18f, 0.14f)
        };

        private static void ApplyFrame(VisualElement root)
        {
            if (_frameSprite == null)
                _frameSprite = Resources.Load<Sprite>("Art/Generated/UI/ui_card_frame");
            if (_frameSprite == null) return;

            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = overlay.style.top = overlay.style.right = overlay.style.bottom = 0;
            overlay.style.backgroundImage     = new StyleBackground(_frameSprite);
            overlay.style.backgroundScaleMode = ScaleMode.StretchToFill;
            overlay.pickingMode = PickingMode.Ignore;
            root.Add(overlay);
        }
    }
}
```

---

## Step 3: Apply to Combat Hand

In `CombatScreenController`, when rendering the hunter's hand:

```csharp
// Replace plain label card placeholders with rendered cards:
_handContainer.Clear();
foreach (var cardSO in currentHunter.hand)
{
    var cardEl = CardRenderer.BuildActionCard(cardSO, isPlayable: true);
    cardEl.RegisterCallback<ClickEvent>(_ => OnCardClicked(cardSO));
    _handContainer.Add(cardEl);
}
```

---

## Verification Test

- [ ] Start combat — hand shows rendered cards with stone frame, category band, name, AP cost
- [ ] Reaction cards show cold blue band; Signature card shows gold band
- [ ] Hovering a card shows gold border highlight
- [ ] Monster behavior panel shows behavior cards with red "BEHAVIOR" band and trigger text
- [ ] Settlement Innovations tab shows innovation cards with gold band
- [ ] Effect text wraps cleanly inside the card — no text overflow
- [ ] Cards with net 0 AP cost show AP count in green
- [ ] LOUD cards show "◆ LOUD" indicator in red

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_I.md`
**Covers:** Screen transition animations — fade-to-black between all scene loads, slide-in panels for modals, and a consistent SceneTransitionManager singleton
