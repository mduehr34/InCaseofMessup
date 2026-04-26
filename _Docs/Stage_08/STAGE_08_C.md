<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-C | Character Creation — Hunter Roster & Naming
Status: Stage 8-B complete. Campaign select working.
Task: Create the CharacterCreation scene. Auto-generate 8
hunters from the GDD name pool. Display each hunter with
their build silhouette sprite and build name. Let the player
rename any hunter. CONFIRM calls StartNewCampaign() and
loads the Settlement scene for Year 1.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_C.md
- Assets/_Game/Scripts/Core.Data/CharacterSO.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Data/Enums.cs

Then confirm:
- 8 hunters are auto-generated: 4 male builds, 4 female builds
- Player can click any hunter name to rename it
- The build (Aethel, Beorn, etc.) is fixed — only the NAME changes
- CONFIRM calls GameStateManager.Instance.StartNewCampaign()
  which initialises campaign state and loads Settlement
- What you will NOT build this session (animation — Stage 9-C)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-C: Character Creation — Hunter Roster & Naming

**Resuming from:** Stage 8-B complete — campaign select working
**Done when:** 8 hunters generated with correct builds, player can rename any, CONFIRM starts campaign and loads Settlement
**Commit:** `"8C: Character creation scene — auto-generate hunters, rename UI, CONFIRM flow"`
**Next session:** STAGE_08_D.md

---

## What You Are Building

After choosing a campaign the player sees their starting roster of 8 hunters. Each hunter has:
- A **build** (fixed body type: Aethel, Beorn, Cyne etc.) which determines their silhouette sprite and stat modifiers
- A **player-given name** (e.g., "Aldric") — this is what shows in-game
- A **sex** (M or F) — determines which name pool to draw from

The player can rename any hunter by clicking the name and typing a new one. When happy, they press CONFIRM to begin Year 1.

**New developer note:** The "build" is like the character class — it determines appearance and starting stats. The "name" is purely flavour — the player names their hunters whatever they want.

---

## GDD Name Pools

When auto-generating, pick names at random from these pools (no duplicates):

**Male names:** Aldric, Beorn, Cyne, Drest, Edric, Finn, Garm, Hrolf, Ivar, Kenric, Leif, Mord, Orm, Rolf, Sigurd, Tor, Ulf, Wulf
**Female names:** Aelith, Bryn, Cyneth, Duna, Eira, Freya, Gerd, Hild, Ingrid, Kara, Lena, Mira, Nessa, Runa, Sigrid, Thora, Urd, Wynn

**Build assignment (fixed order, 4M / 4F):**
| Slot | Build | Sex |
|---|---|---|
| 0 | Aethel | M |
| 1 | Beorn  | M |
| 2 | Cyne   | M |
| 3 | Duna   | M |
| 4 | Eira   | F |
| 5 | Freya  | F |
| 6 | Gerd   | F |
| 7 | Hild   | F |

---

## Step 1: Create Scene

**File → New Scene → Empty** → save as `Assets/CharacterCreation.unity`
Add to Build Settings after CampaignSelect.

---

## Step 2: HunterGenerationData (simple data class)

Create `Assets/_Game/Scripts/Core.Data/HunterGenerationData.cs`:

```csharp
namespace MnM.Core.Data
{
    [System.Serializable]
    public class HunterGenerationData
    {
        public string hunterName;  // Player-given name
        public string buildName;   // Aethel, Beorn, etc.
        public string sex;         // "M" or "F"
        public string spritePath;  // e.g. "Art/Generated/Characters/char_aethel_idle_s"
    }
}
```

---

## Step 3: UXML Layout

Create `Assets/_Game/UI/CharacterCreation.uxml`:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; flex-direction:column;
      align-items:center; background-color:#0A0A0C; padding:32px;">

    <ui:Label text="YOUR HUNTERS" style="color:#D4CCBA; font-size:20px; margin-bottom:8px;" />
    <ui:Label text="Click any name to rename your hunter. Build and appearance are fixed."
              style="color:#8A8A8A; font-size:10px; margin-bottom:24px;" />

    <!-- 4 male hunters top row, 4 female bottom row -->
    <ui:VisualElement name="hunters-grid" style="flex-direction:row; flex-wrap:wrap;
        justify-content:center; gap:16px; max-width:800px; margin-bottom:32px;" />

    <ui:VisualElement style="flex-direction:row; gap:24px;">
      <ui:Button name="btn-back"    text="← BACK"    class="mnm-btn-secondary" />
      <ui:Button name="btn-confirm" text="BEGIN →"    class="mnm-btn-primary" />
    </ui:VisualElement>

  </ui:VisualElement>
</ui:UXML>
```

---

## Step 4: CharacterCreationController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CharacterCreationController.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CharacterCreationController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private static readonly string[] MaleNames =
        {
            "Aldric","Beorn","Cyne","Drest","Edric","Finn",
            "Garm","Hrolf","Ivar","Kenric","Leif","Mord"
        };
        private static readonly string[] FemaleNames =
        {
            "Aelith","Bryn","Cyneth","Duna","Eira","Freya",
            "Gerd","Hild","Ingrid","Kara","Lena","Mira"
        };

        private static readonly (string build, string sex)[] BuildSlots =
        {
            ("Aethel","M"), ("Beorn","M"), ("Cyne","M"), ("Duna","M"),
            ("Eira","F"),   ("Freya","F"), ("Gerd","F"), ("Hild","F")
        };

        private readonly List<HunterGenerationData> _hunters = new();
        private int _renamingIndex = -1;
        private TextField _activeField;

        private void OnEnable()
        {
            GenerateHunters();
            BuildUI();
        }

        private void GenerateHunters()
        {
            _hunters.Clear();
            var usedM = new HashSet<string>();
            var usedF = new HashSet<string>();
            var rng   = new System.Random();

            foreach (var (build, sex) in BuildSlots)
            {
                string name = PickUnique(sex == "M" ? MaleNames : FemaleNames,
                                         sex == "M" ? usedM : usedF, rng);
                _hunters.Add(new HunterGenerationData
                {
                    hunterName  = name,
                    buildName   = build,
                    sex         = sex,
                    spritePath  = $"Art/Generated/Characters/char_{build.ToLower()}_idle_s"
                });
            }
            Debug.Log($"[CharacterCreation] Generated {_hunters.Count} hunters");
        }

        private string PickUnique(string[] pool, HashSet<string> used, System.Random rng)
        {
            var available = System.Array.FindAll(pool, n => !used.Contains(n));
            if (available.Length == 0) return "Hunter";
            string pick = available[rng.Next(available.Length)];
            used.Add(pick);
            return pick;
        }

        private void BuildUI()
        {
            var root = _uiDocument.rootVisualElement;
            var grid = root.Q("hunters-grid");
            grid.Clear();

            for (int i = 0; i < _hunters.Count; i++)
            {
                int idx = i;
                var h   = _hunters[i];

                var card = new VisualElement();
                card.style.width           = 88;
                card.style.alignItems      = Align.Center;
                card.style.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
                card.style.borderTopColor  = card.style.borderBottomColor =
                card.style.borderLeftColor = card.style.borderRightColor  =
                    new StyleColor(new Color(0.31f, 0.27f, 0.20f));
                card.style.borderTopWidth  = card.style.borderBottomWidth =
                card.style.borderLeftWidth = card.style.borderRightWidth  = 1;
                card.style.paddingTop      = card.style.paddingBottom      = 8;
                card.style.paddingLeft     = card.style.paddingRight       = 6;

                // Sprite
                var img = new VisualElement();
                img.style.width  = 64;
                img.style.height = 64;
                img.style.marginBottom = 6;
                img.style.backgroundScaleMode = ScaleMode.ScaleToFit;
                var sprite = Resources.Load<Sprite>(h.spritePath);
                if (sprite != null)
                    img.style.backgroundImage = new StyleBackground(sprite);
                card.Add(img);

                // Build label
                var buildLabel = new Label($"{h.buildName} ({h.sex})");
                buildLabel.style.color    = new Color(0.54f, 0.54f, 0.54f);
                buildLabel.style.fontSize = 8;
                buildLabel.style.marginBottom = 4;
                card.Add(buildLabel);

                // Name label (clickable to rename)
                var nameLabel = new Label(h.hunterName);
                nameLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
                nameLabel.style.fontSize = 11;
                nameLabel.name           = $"name-label-{idx}";
                nameLabel.RegisterCallback<ClickEvent>(_ => StartRename(root, idx));
                card.Add(nameLabel);

                grid.Add(card);
            }

            // Navigation
            root.Q<Button>("btn-back")   .RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene("CampaignSelect"));
            root.Q<Button>("btn-confirm").RegisterCallback<ClickEvent>(_ => OnConfirm());
        }

        private void StartRename(VisualElement root, int idx)
        {
            // Replace label with a text field
            _renamingIndex = idx;
            var label = root.Q<Label>($"name-label-{idx}");
            if (label == null) return;

            var field = new TextField();
            field.name          = $"name-field-{idx}";
            field.value         = _hunters[idx].hunterName;
            field.style.width   = 80;
            field.style.color   = new Color(0.83f, 0.80f, 0.73f);
            field.style.fontSize = 11;
            _activeField        = field;

            label.parent.Add(field);
            label.parent.Remove(label);

            field.RegisterCallback<FocusOutEvent>(_ => FinishRename(root, idx, field));
            field.Q(TextField.textInputUssName)?.Focus();
        }

        private void FinishRename(VisualElement root, int idx, TextField field)
        {
            string newName = field.value.Trim();
            if (string.IsNullOrEmpty(newName)) newName = _hunters[idx].hunterName;
            _hunters[idx].hunterName = newName;

            var nameLabel = new Label(newName);
            nameLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            nameLabel.style.fontSize = 11;
            nameLabel.name           = $"name-label-{idx}";
            nameLabel.RegisterCallback<ClickEvent>(_ => StartRename(root, idx));

            field.parent.Add(nameLabel);
            field.parent.Remove(field);
        }

        private void OnConfirm()
        {
            Debug.Log("[CharacterCreation] Confirming hunter roster");
            GameStateManager.Instance.StartNewCampaign(_hunters);
            SceneManager.LoadScene("Settlement");
        }
    }
}
```

**Add to GameStateManager:**
```csharp
public void StartNewCampaign(List<HunterGenerationData> hunters)
{
    // Initialise CampaignState from _pendingCampaign
    // Create CharacterSO instances from HunterGenerationData
    // (Full implementation: create runtime character objects, assign to CampaignState)
    Debug.Log($"[GSM] Campaign started with {hunters.Count} hunters");
    // TODO: create CharacterSO instances and populate CampaignState.characterPool
}
```

---

## Verification Test

- [ ] CharacterCreation scene loads after clicking CONFIRM in CampaignSelect
- [ ] 8 hunter cards displayed: 4 male builds top, 4 female builds bottom
- [ ] Each card shows: build sprite, build name, sex, and player-given name
- [ ] Clicking a name shows a text field; typing and clicking away updates the name
- [ ] BACK returns to CampaignSelect
- [ ] CONFIRM logs hunter count and loads Settlement scene
- [ ] Names are unique (no two hunters share the same generated name)
- [ ] No Console errors

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_D.md`
**Covers:** Audio production — generating all 5 music tracks and 8 SFX clips via CoPlay, adding main menu music context to AudioManager, wiring all clips in the Inspector
