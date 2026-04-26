<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-P | Birth, Retirement & Year-End Screens
Status: Stage 8-O complete. Chronicle and Codex UI working.
Task: Build three lifecycle screens that appear at specific
campaign moments. BirthController: when a birth event fires,
show a panel letting the player name a newborn and see their
starting sprite. RetirementController: when a veteran hunter
reaches retirement age, show their history and let the player
choose to retire them or keep them at a cost. YearEndSummary
Controller: at the end of each year, show a brief recap of
the year's hunts, deaths, and crafts before the next year begins.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_P.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs

Then confirm:
- BirthController is a modal overlay on the settlement screen
- RetirementController is also a modal overlay
- YearEndSummaryController slides in before the Year banner
- All three use UIToolkit only — no new Canvas objects
- Newborn hunter names use the same GDD name pools from 8-C
- What you will NOT build (birth stat assignment UI — auto
  for MVP, player only names them)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-P: Birth, Retirement & Year-End Screens

**Resuming from:** Stage 8-O complete — Chronicle Log and Codex UI working
**Done when:** A birth event shows a naming panel for the newborn; a retiring veteran shows a history card with a choice; year-end shows a summary of the year before advancing
**Commit:** `"8P: Birth, retirement, and year-end screens — lifecycle modal overlays"`
**Next session:** STAGE_08_Q.md

---

## What These Screens Are

Your hunters age over the campaign. Important milestones happen:

- **Birth:** A baby is born to the settlement. The player names them. They grow up and become a hunter in Year 5.
- **Retirement:** A veteran hunter has survived long enough that they can retire to a safe life — or keep fighting at the risk of permanent injury.
- **Year-End Summary:** Before the banner "YEAR 3" appears, a brief recap shows what happened — hunts won, hunters lost, items crafted. This is the player's moment to feel the weight of the year.

All three are modal overlays — they appear over the settlement screen and must be dismissed before play continues.

---

## Part 1: BirthController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/BirthController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    /// <summary>
    /// Modal overlay shown when a birth event fires.
    /// Call ShowBirthPanel() from GameStateManager or SettlementScreenController
    /// when a birth event resolves.
    /// </summary>
    public class BirthController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        // Name pools — same lists from Character Creation (Stage 8-C)
        private static readonly string[] MaleNames =
        {
            "Aldric","Beorn","Cyne","Dag","Egil","Finn","Gorm","Holt",
            "Ivar","Jorund","Kell","Leif","Marn","Osric","Pell","Rand",
            "Sven","Torben"
        };
        private static readonly string[] FemaleNames =
        {
            "Eira","Freya","Gerd","Hild","Inga","Kira","Lund","Mara",
            "Nora","Orla","Pela","Runa","Sigrid","Thora","Ulla","Vala",
            "Wren","Ysa"
        };

        private VisualElement _overlay;
        private TextField     _nameField;
        private Label         _sexLabel;
        private string        _chosenSex;
        private System.Action<string, string> _onConfirm; // (name, sex)

        /// <summary>
        /// Show the birth naming panel.
        /// onConfirm receives the player's chosen name and sex so the caller
        /// can register the newborn in CampaignState.
        /// </summary>
        public void ShowBirthPanel(System.Action<string, string> onConfirm)
        {
            _onConfirm = onConfirm;
            _chosenSex = Random.value > 0.5f ? "M" : "F";
            BuildPanel();
        }

        private void BuildPanel()
        {
            var root = _uiDocument.rootVisualElement;

            // --- Dim backdrop ---
            _overlay = new VisualElement();
            _overlay.style.position       = Position.Absolute;
            _overlay.style.left = _overlay.style.top =
            _overlay.style.right = _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.78f));
            _overlay.style.alignItems      = Align.Center;
            _overlay.style.justifyContent  = Justify.Center;
            root.Add(_overlay);

            // --- Panel card ---
            var panel = new VisualElement();
            panel.style.width           = 400;
            panel.style.backgroundColor = new StyleColor(new Color(0.06f, 0.05f, 0.03f));
            panel.style.borderTopColor  = panel.style.borderBottomColor =
            panel.style.borderLeftColor = panel.style.borderRightColor =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            panel.style.borderTopWidth  = panel.style.borderBottomWidth =
            panel.style.borderLeftWidth = panel.style.borderRightWidth = 2;
            panel.style.paddingTop      = panel.style.paddingBottom =
            panel.style.paddingLeft     = panel.style.paddingRight = 24;
            _overlay.Add(panel);

            // Title
            var title = new Label("A CHILD IS BORN");
            title.style.color     = new Color(0.72f, 0.52f, 0.04f);
            title.style.fontSize  = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            panel.Add(title);

            // Flavour
            var flavour = new Label("The settlement grows. Give this child a name.");
            flavour.style.color     = new Color(0.54f, 0.54f, 0.54f);
            flavour.style.fontSize  = 10;
            flavour.style.whiteSpace = WhiteSpace.Normal;
            flavour.style.marginBottom = 20;
            panel.Add(flavour);

            // Sex label (cosmetic — randomised)
            _sexLabel = new Label(_chosenSex == "M" ? "BOY" : "GIRL");
            _sexLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            _sexLabel.style.fontSize = 10;
            _sexLabel.style.marginBottom = 8;
            panel.Add(_sexLabel);

            // Name field
            var defaultName = _chosenSex == "M"
                ? MaleNames[Random.Range(0, MaleNames.Length)]
                : FemaleNames[Random.Range(0, FemaleNames.Length)];

            _nameField = new TextField();
            _nameField.value = defaultName;
            _nameField.style.marginBottom = 20;
            _nameField.style.color = new Color(0.83f, 0.80f, 0.73f);
            _nameField.style.fontSize = 13;
            panel.Add(_nameField);

            // Confirm button
            var confirmBtn = new Button { text = "NAME THIS CHILD" };
            confirmBtn.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.25f));
            confirmBtn.style.borderTopColor   = confirmBtn.style.borderBottomColor =
            confirmBtn.style.borderLeftColor  = confirmBtn.style.borderRightColor  =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            confirmBtn.style.borderTopWidth   = confirmBtn.style.borderBottomWidth =
            confirmBtn.style.borderLeftWidth  = confirmBtn.style.borderRightWidth  = 1;
            confirmBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            confirmBtn.style.fontSize = 12;
            confirmBtn.RegisterCallback<ClickEvent>(_ => OnConfirm());
            panel.Add(confirmBtn);

            // Animate in
            StartCoroutine(FadeIn(_overlay));
        }

        private void OnConfirm()
        {
            string name = _nameField.value.Trim();
            if (string.IsNullOrEmpty(name)) name = "Unnamed";

            StartCoroutine(FadeOutAndClose(name));
        }

        private IEnumerator FadeIn(VisualElement el)
        {
            el.style.opacity = 0;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                el.style.opacity = t / 0.3f;
                yield return null;
            }
            el.style.opacity = 1;
        }

        private IEnumerator FadeOutAndClose(string name)
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                _overlay.style.opacity = 1f - t / 0.25f;
                yield return null;
            }
            _uiDocument.rootVisualElement.Remove(_overlay);
            _onConfirm?.Invoke(name, _chosenSex);
        }
    }
}
```

### Registering the Newborn in CampaignState

Add to `GameStateManager`:

```csharp
public void RegisterNewborn(string name, string sex)
{
    // Add a pending child — becomes a hunter in Year 5
    var child = new PendingChild
    {
        hunterName   = name,
        sex          = sex,
        birthYear    = CampaignState.currentYear
    };
    var list = new System.Collections.Generic.List<PendingChild>(
        CampaignState.pendingChildren ?? new PendingChild[0]);
    list.Add(child);
    CampaignState.pendingChildren = list.ToArray();
    AddChronicleEntry(CampaignState.currentYear,
        $"{name} was born to the settlement.");
    Debug.Log($"[Birth] {name} ({sex}) registered. Will hunt in Year {child.birthYear + 5}.");
}
```

Add to `CampaignState`:

```csharp
public PendingChild[] pendingChildren;
```

Add the struct (put in `Core.Data` namespace, new file `PendingChild.cs`):

```csharp
namespace MnM.Core.Data
{
    [System.Serializable]
    public struct PendingChild
    {
        public string hunterName;
        public string sex;
        public int    birthYear;   // Becomes a hunter in birthYear + 5
    }
}
```

---

## Part 2: RetirementController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/RetirementController.cs`

A hunter who has survived 7+ years of active hunting may be retired. The player can choose to:
- **RETIRE** — the hunter leaves the roster, a farewell chronicle entry is added, and the player receives a modest resource bonus (a veteran's legacy)
- **KEEP FIGHTING** — the hunter stays, but gains the permanent `Weathered` status (all checks at -1 permanently)

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class RetirementController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _overlay;
        private System.Action<bool> _onChoice; // true = retire, false = keep

        /// <summary>
        /// Show the retirement choice panel for a veteran hunter.
        /// hunterName, hunterBuild, yearsActive: pulled from HunterState.
        /// onChoice: true = player chose to retire, false = keep fighting.
        /// </summary>
        public void ShowRetirementPanel(string hunterName, string hunterBuild,
                                         int yearsActive,
                                         System.Action<bool> onChoice)
        {
            _onChoice = onChoice;
            BuildPanel(hunterName, hunterBuild, yearsActive);
        }

        private void BuildPanel(string name, string build, int years)
        {
            var root = _uiDocument.rootVisualElement;

            _overlay = new VisualElement();
            _overlay.style.position        = Position.Absolute;
            _overlay.style.left = _overlay.style.top =
            _overlay.style.right = _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.80f));
            _overlay.style.alignItems      = Align.Center;
            _overlay.style.justifyContent  = Justify.Center;
            root.Add(_overlay);

            var panel = new VisualElement();
            panel.style.width           = 440;
            panel.style.backgroundColor = new StyleColor(new Color(0.06f, 0.05f, 0.03f));
            panel.style.borderTopColor  = panel.style.borderBottomColor =
            panel.style.borderLeftColor = panel.style.borderRightColor =
                new StyleColor(new Color(0.54f, 0.54f, 0.54f));
            panel.style.borderTopWidth  = panel.style.borderBottomWidth =
            panel.style.borderLeftWidth = panel.style.borderRightWidth = 2;
            panel.style.paddingTop      = panel.style.paddingBottom =
            panel.style.paddingLeft     = panel.style.paddingRight = 28;
            _overlay.Add(panel);

            // Hunter name header
            var nameLabel = new Label(name.ToUpper());
            nameLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            nameLabel.style.fontSize = 20;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 4;
            panel.Add(nameLabel);

            var buildLabel = new Label($"{build}  ·  {years} Years Active");
            buildLabel.style.color    = new Color(0.54f, 0.54f, 0.54f);
            buildLabel.style.fontSize = 9;
            buildLabel.style.marginBottom = 20;
            panel.Add(buildLabel);

            // Flavour
            var flavour = new Label(
                $"After {years} years of hunting, {name} has earned the right to rest.\n" +
                "The bones know when they've had enough.\n\n" +
                "You may release them to a quieter life — or ask them to fight one more year.");
            flavour.style.color     = new Color(0.72f, 0.65f, 0.54f);
            flavour.style.fontSize  = 10;
            flavour.style.whiteSpace = WhiteSpace.Normal;
            flavour.style.marginBottom = 24;
            panel.Add(flavour);

            // Button row
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            panel.Add(row);

            // RETIRE button
            var retireBtn = new Button { text = $"RETIRE {name.ToUpper()}\n+3 Bone  +2 Hide" };
            retireBtn.style.width           = 180;
            retireBtn.style.whiteSpace      = WhiteSpace.Normal;
            retireBtn.style.backgroundColor = new StyleColor(new Color(0.12f, 0.10f, 0.08f));
            retireBtn.style.borderTopColor  = retireBtn.style.borderBottomColor =
            retireBtn.style.borderLeftColor = retireBtn.style.borderRightColor =
                new StyleColor(new Color(0.31f, 0.27f, 0.20f));
            retireBtn.style.borderTopWidth  = retireBtn.style.borderBottomWidth =
            retireBtn.style.borderLeftWidth = retireBtn.style.borderRightWidth = 1;
            retireBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            retireBtn.style.fontSize = 10;
            retireBtn.style.paddingTop = retireBtn.style.paddingBottom = 10;
            retireBtn.RegisterCallback<ClickEvent>(_ => Choose(true));
            row.Add(retireBtn);

            // KEEP FIGHTING button
            var keepBtn = new Button { text = $"KEEP FIGHTING\nGains: Weathered (-1 all)" };
            keepBtn.style.width           = 180;
            keepBtn.style.whiteSpace      = WhiteSpace.Normal;
            keepBtn.style.backgroundColor = new StyleColor(new Color(0.20f, 0.06f, 0.06f));
            keepBtn.style.borderTopColor  = keepBtn.style.borderBottomColor =
            keepBtn.style.borderLeftColor = keepBtn.style.borderRightColor =
                new StyleColor(new Color(0.50f, 0.15f, 0.10f));
            keepBtn.style.borderTopWidth  = keepBtn.style.borderBottomWidth =
            keepBtn.style.borderLeftWidth = keepBtn.style.borderRightWidth = 1;
            keepBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            keepBtn.style.fontSize = 10;
            keepBtn.style.paddingTop = keepBtn.style.paddingBottom = 10;
            keepBtn.RegisterCallback<ClickEvent>(_ => Choose(false));
            row.Add(keepBtn);

            StartCoroutine(FadeIn(_overlay));
        }

        private void Choose(bool retire)
        {
            StartCoroutine(FadeOutAndClose(retire));
        }

        private IEnumerator FadeIn(VisualElement el)
        {
            el.style.opacity = 0;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                el.style.opacity = t / 0.3f;
                yield return null;
            }
            el.style.opacity = 1;
        }

        private IEnumerator FadeOutAndClose(bool retire)
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                _overlay.style.opacity = 1f - t / 0.25f;
                yield return null;
            }
            _uiDocument.rootVisualElement.Remove(_overlay);
            _onChoice?.Invoke(retire);
        }
    }
}
```

### Wiring Retirement in GameStateManager

```csharp
/// <summary>
/// Call at the start of each year to check if any hunter qualifies for retirement.
/// Returns the first qualifying hunter, or null if none.
/// </summary>
public HunterState GetRetirementCandidate()
{
    if (CampaignState.hunters == null) return null;
    foreach (var h in CampaignState.hunters)
    {
        if (!h.isDead && !h.isRetired && h.yearsActive >= 7)
            return h;
    }
    return null;
}

public void RetireHunter(string hunterId, bool retire)
{
    if (CampaignState.hunters == null) return;
    foreach (var h in CampaignState.hunters)
    {
        if (h.hunterId != hunterId) continue;
        if (retire)
        {
            h.isRetired = true;
            CampaignState.bone  += 3;
            CampaignState.hide  += 2;
            AddChronicleEntry(CampaignState.currentYear,
                $"{h.hunterName} retired. The settlement honours their service.");
        }
        else
        {
            h.permanentDebuffs = AppendString(h.permanentDebuffs, "Weathered");
            AddChronicleEntry(CampaignState.currentYear,
                $"{h.hunterName} chose to keep fighting. The years are showing.");
        }
        break;
    }
}

private string AppendString(string existing, string value)
{
    if (string.IsNullOrEmpty(existing)) return value;
    return existing + "," + value;
}
```

---

## Part 3: YearEndSummaryController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/YearEndSummaryController.cs`

This screen slides up from the bottom of the settlement screen before the year banner animation. It summarises the year that just passed: hunts won/lost, hunters who died, items crafted.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class YearEndSummaryController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _panel;
        private System.Action _onClose;

        /// <summary>
        /// Show the year-end summary panel.
        /// stats: assembled by caller from CampaignState at year-end.
        /// onClose: called when the player dismisses the panel.
        /// </summary>
        public void ShowYearEndSummary(YearEndStats stats, System.Action onClose)
        {
            _onClose = onClose;
            BuildPanel(stats);
        }

        private void BuildPanel(YearEndStats stats)
        {
            var root = _uiDocument.rootVisualElement;

            // Slide-up panel from bottom
            _panel = new VisualElement();
            _panel.style.position       = Position.Absolute;
            _panel.style.left = _panel.style.right = 0;
            _panel.style.bottom         = -320; // Start off-screen
            _panel.style.height         = 280;
            _panel.style.backgroundColor = new StyleColor(new Color(0.05f, 0.04f, 0.03f, 0.98f));
            _panel.style.borderTopColor  = _panel.style.borderLeftColor =
            _panel.style.borderRightColor =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            _panel.style.borderTopWidth  = _panel.style.borderLeftWidth =
            _panel.style.borderRightWidth = 2;
            _panel.style.paddingTop      = _panel.style.paddingLeft =
            _panel.style.paddingRight    = 32;
            _panel.style.paddingBottom   = 20;
            root.Add(_panel);

            // Year label
            var yearLabel = new Label($"END OF YEAR {stats.year}");
            yearLabel.style.color    = new Color(0.72f, 0.52f, 0.04f);
            yearLabel.style.fontSize = 14;
            yearLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            yearLabel.style.marginBottom = 16;
            _panel.Add(yearLabel);

            // Stats row
            var statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.marginBottom  = 16;
            _panel.Add(statsRow);

            AddStatBlock(statsRow, "HUNTS WON",    stats.huntsWon.ToString(),    new Color(0.30f, 0.60f, 0.30f));
            AddStatBlock(statsRow, "HUNTS LOST",   stats.huntsLost.ToString(),   new Color(0.60f, 0.20f, 0.20f));
            AddStatBlock(statsRow, "HUNTERS LOST", stats.hunterDeaths.ToString(),new Color(0.60f, 0.15f, 0.15f));
            AddStatBlock(statsRow, "ITEMS CRAFTED",stats.itemsCrafted.ToString(),new Color(0.54f, 0.54f, 0.54f));

            // Deaths list
            if (stats.deadHunterNames != null && stats.deadHunterNames.Length > 0)
            {
                var deadLabel = new Label(
                    "We remember: " + string.Join(", ", stats.deadHunterNames));
                deadLabel.style.color     = new Color(0.60f, 0.20f, 0.20f);
                deadLabel.style.fontSize  = 9;
                deadLabel.style.whiteSpace = WhiteSpace.Normal;
                deadLabel.style.marginBottom = 12;
                _panel.Add(deadLabel);
            }

            // Closing line
            string closingLine = GetClosingLine(stats);
            var closing = new Label(closingLine);
            closing.style.color     = new Color(0.54f, 0.54f, 0.54f);
            closing.style.fontSize  = 9;
            closing.style.whiteSpace = WhiteSpace.Normal;
            closing.style.fontStyle  = FontStyle.Italic;
            closing.style.marginBottom = 16;
            _panel.Add(closing);

            // Continue button
            var continueBtn = new Button { text = "ADVANCE TO NEXT YEAR →" };
            continueBtn.style.alignSelf      = Align.FlexEnd;
            continueBtn.style.backgroundColor = new StyleColor(new Color(0.72f, 0.52f, 0.04f, 0.20f));
            continueBtn.style.borderTopColor  = continueBtn.style.borderBottomColor =
            continueBtn.style.borderLeftColor = continueBtn.style.borderRightColor  =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            continueBtn.style.borderTopWidth  = continueBtn.style.borderBottomWidth =
            continueBtn.style.borderLeftWidth = continueBtn.style.borderRightWidth  = 1;
            continueBtn.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            continueBtn.style.fontSize = 11;
            continueBtn.RegisterCallback<ClickEvent>(_ => Close());
            _panel.Add(continueBtn);

            StartCoroutine(SlideIn());
        }

        private void AddStatBlock(VisualElement parent, string label,
                                   string value, Color valueColor)
        {
            var block = new VisualElement();
            block.style.marginRight = 32;
            block.style.alignItems  = Align.FlexStart;
            parent.Add(block);

            var val = new Label(value);
            val.style.color    = valueColor;
            val.style.fontSize = 28;
            val.style.unityFontStyleAndWeight = FontStyle.Bold;
            block.Add(val);

            var lbl = new Label(label);
            lbl.style.color    = new Color(0.45f, 0.43f, 0.40f);
            lbl.style.fontSize = 7;
            block.Add(lbl);
        }

        private string GetClosingLine(YearEndStats stats)
        {
            if (stats.hunterDeaths > 2)
                return "The ground has taken more than we can spare.";
            if (stats.huntsWon == 0)
                return "Not a single hunt succeeded. The settlement is restless.";
            if (stats.huntsWon >= 3)
                return "A strong year. The larder is full and morale holds.";
            if (stats.itemsCrafted >= 4)
                return "The crafters worked through the cold months without complaint.";
            return "The settlement endures. That is enough.";
        }

        private void Close()
        {
            StartCoroutine(SlideOutAndClose());
        }

        private IEnumerator SlideIn()
        {
            float t = 0f, duration = 0.4f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                _panel.style.bottom = Mathf.Lerp(-320f, 0f, p);
                yield return null;
            }
            _panel.style.bottom = 0;
        }

        private IEnumerator SlideOutAndClose()
        {
            float t = 0f, duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                _panel.style.bottom = Mathf.Lerp(0f, -320f, p);
                yield return null;
            }
            _uiDocument.rootVisualElement.Remove(_panel);
            _onClose?.Invoke();
        }
    }

    [System.Serializable]
    public class YearEndStats
    {
        public int    year;
        public int    huntsWon;
        public int    huntsLost;
        public int    hunterDeaths;
        public int    itemsCrafted;
        public string[] deadHunterNames;
    }
}
```

---

## Part 4: Integration — Wiring All Three Controllers

### In SettlementScreenController

Add three serialized fields:

```csharp
[SerializeField] private BirthController          _birthController;
[SerializeField] private RetirementController     _retirementController;
[SerializeField] private YearEndSummaryController _yearEndController;
[SerializeField] private SettlementAnimationController _settlementAnim;
```

### Year-End Flow (call this from the End Year button handler)

```csharp
private void OnEndYearClicked()
{
    var gsm   = GameStateManager.Instance;
    var state = gsm.CampaignState;

    // Build stats for this year
    var stats = new YearEndStats
    {
        year           = state.currentYear,
        huntsWon       = state.yearHuntsWon,
        huntsLost      = state.yearHuntsLost,
        hunterDeaths   = state.yearHunterDeaths,
        itemsCrafted   = state.yearItemsCrafted,
        deadHunterNames = state.yearDeadHunterNames
    };

    // Show year-end summary, then year banner, then check retirement
    _yearEndController.ShowYearEndSummary(stats, () =>
    {
        int newYear = state.currentYear + 1;
        gsm.AdvanceYear();                       // increments year, resets yearly counters

        _settlementAnim.ShowYearBanner(newYear); // from Stage 8-L

        // Check for retirement candidate after a short delay
        StartCoroutine(CheckRetirementAfterBanner(newYear));
    });
}

private IEnumerator CheckRetirementAfterBanner(int newYear)
{
    yield return new WaitForSeconds(3.0f); // Let banner play

    var candidate = GameStateManager.Instance.GetRetirementCandidate();
    if (candidate != null)
    {
        _retirementController.ShowRetirementPanel(
            candidate.hunterName, candidate.buildName, candidate.yearsActive,
            retire => GameStateManager.Instance.RetireHunter(candidate.hunterId, retire));
    }
}
```

### Birth Event Trigger

When an event with the mechanical effect `"birth"` resolves, call:

```csharp
_birthController.ShowBirthPanel((name, sex) =>
{
    GameStateManager.Instance.RegisterNewborn(name, sex);
});
```

### Add to CampaignState (yearly counters)

```csharp
// Reset each year in AdvanceYear()
public int    yearHuntsWon;
public int    yearHuntsLost;
public int    yearHunterDeaths;
public int    yearItemsCrafted;
public string[] yearDeadHunterNames;
```

### GameStateManager.AdvanceYear()

```csharp
public void AdvanceYear()
{
    CampaignState.currentYear++;

    // Reset yearly counters
    CampaignState.yearHuntsWon        = 0;
    CampaignState.yearHuntsLost       = 0;
    CampaignState.yearHunterDeaths    = 0;
    CampaignState.yearItemsCrafted    = 0;
    CampaignState.yearDeadHunterNames = new string[0];

    // Age all hunters
    if (CampaignState.hunters != null)
        foreach (var h in CampaignState.hunters)
            if (!h.isDead && !h.isRetired) h.yearsActive++;

    // Check if any pending children are old enough to hunt (birthYear + 5)
    PromoteChildren();

    Debug.Log($"[Campaign] Advanced to Year {CampaignState.currentYear}");
}

private void PromoteChildren()
{
    if (CampaignState.pendingChildren == null) return;
    var remaining = new System.Collections.Generic.List<PendingChild>();
    foreach (var child in CampaignState.pendingChildren)
    {
        if (CampaignState.currentYear >= child.birthYear + 5)
        {
            // Create a hunter from the child
            var hunter = CreateHunterFromChild(child);
            AddHunterToRoster(hunter);
            AddChronicleEntry(CampaignState.currentYear,
                $"{child.hunterName} is old enough to hunt. They join the roster.");
        }
        else
        {
            remaining.Add(child);
        }
    }
    CampaignState.pendingChildren = remaining.ToArray();
}
```

---

## Verification Test

- [ ] End Year 1 → Year-End Summary panel slides up from bottom
- [ ] Summary shows correct hunts won, lost, deaths, items crafted
- [ ] Closing line changes based on the year's results (e.g., no hunts won → "Not a single hunt succeeded")
- [ ] Click "ADVANCE TO NEXT YEAR" → panel slides back down
- [ ] Year banner "YEAR 2" appears immediately after panel closes
- [ ] Veteran hunter with 7+ years → retirement panel appears after banner
- [ ] Choose RETIRE → hunter removed from roster, +3 Bone, +2 Hide added, chronicle entry written
- [ ] Choose KEEP FIGHTING → hunter stays, "Weathered" status added to their debuffs
- [ ] Trigger a birth event → birth panel appears with randomised name pre-filled
- [ ] Change name in text field → confirm → chronicle entry "X was born to the settlement"
- [ ] In Year 6, child born in Year 1 appears in the hunter roster automatically
- [ ] No null reference errors if year-end stats are all zeroes

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_Q.md`
**Covers:** Save/Load UI, Game Over screen, and Victory Epilogue — SaveLoadController (slot selection, overwrite warning), GameOverController (death screen with cause and hunter history), VictoryEpilogueController (Year 30 epilogue narrative based on campaign outcomes)
