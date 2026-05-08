<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-U | Chronicle Log & Codex UI
Status: Stage 8-T complete. Tutorial tooltip system working.
Task: Create CodexEntrySO data class. Build the CHRONICLE
tab in the settlement as a scrollable log of past events.
Build the CODEX tab as a grid of discovered lore entries.
Wire event resolution to automatically add chronicle entries
and unlock codex entries. Create the 15 codex entries
referenced throughout the event pool.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_U.md
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.UI/SettlementScreenController.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs

Then confirm:
- CodexEntrySO is a new ScriptableObject with id, title, bodyText, category
- Chronicle entries are plain strings stored in CampaignState
- Codex entries are unlocked by id and stored in CampaignState.unlockedCodexEntryIds
- The Chronicle tab already partially exists in SettlementScreenController
  (it may have a placeholder) — extend it, do not rebuild
- What you will NOT do (voiced/illustrated codex entries — post-MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-U: Chronicle Log & Codex UI

**Resuming from:** Stage 8-T complete — tutorial tooltips working
**Done when:** Chronicle tab shows a scrollable list of past events; Codex tab shows discovered entries as cards; events correctly unlock codex entries; all 15 referenced codex entries exist as SO assets
**Commit:** `"8U: Chronicle log and Codex UI — CodexEntrySO, chronicle scroll, codex grid"`
**Next session:** STAGE_08_V.md

---

## Part 1: CodexEntrySO

Create `Assets/_Game/Scripts/Core.Data/CodexEntrySO.cs`:

```csharp
using UnityEngine;

namespace MnM.Core.Data
{
    [CreateAssetMenu(fileName = "Codex_", menuName = "MnM/Codex Entry")]
    public class CodexEntrySO : ScriptableObject
    {
        [Header("Identity")]
        public string entryId;       // e.g. "CodexEntry_FirstRuins"
        public string entryTitle;    // e.g. "First Ruins"
        public string category;      // "History" | "Monsters" | "Marrow" | "Settlements"

        [TextArea(4, 10)]
        public string bodyText;      // The lore text shown to the player

        [Header("Unlock")]
        public string unlockedByEventId;  // e.g. "EVT-09" — for reference only
    }
}
```

**Save path:** `Assets/_Game/Data/Codex/`

---

## Part 2: Create All 15 Codex Entries

Create one CodexEntrySO asset for each. Right-click in `Assets/_Game/Data/Codex/` → Create → MnM → Codex Entry.

| Asset Name | entryId | entryTitle | category | Unlocked By |
|---|---|---|---|---|
| `Codex_WhispersBelow` | `CodexEntry_WhispersBelow` | Whispers Below | History | EVT-04 |
| `Codex_FirstRuins` | `CodexEntry_FirstRuins` | First Ruins | History | EVT-09 |
| `Codex_TheOldWorks` | `CodexEntry_TheOldWorks` | The Old Works | History | EVT-16 |
| `Codex_SerpentIdol` | `CodexEntry_SerpentIdol` | Serpent Idol | Monsters | EVT-18 |
| `Codex_MarrowExposed` | `CodexEntry_MarrowExposed` | Marrow Exposed | Marrow | EVT-13 |
| `Codex_TheSpite` | `CodexEntry_TheSpite` | The Ironhide | Monsters | EVT-21 |
| `Codex_TheFullPicture` | `CodexEntry_TheFullPicture` | The Full Picture | History | EVT-23 |
| `Codex_SutureStirs` | `CodexEntry_SutureStirs` | The Suture Stirs | Monsters | EVT-25 |
| `Codex_IvoryShard` | `CodexEntry_IvoryShard` | Ivory Shard | Monsters | EVT-12 |
| `Codex_MarrowLore` | `CodexEntry_MarrowLore` | Marrow — What We Know | Marrow | Year 1 auto |
| `Codex_SettlementLog` | `CodexEntry_SettlementLog` | How We Survive | Settlements | Year 1 auto |
| `Codex_TheSiltborn` | `CodexEntry_TheSiltborn` | The Siltborn | Monsters | Siltborn killed |
| `Codex_ThePenitent` | `CodexEntry_ThePenitent` | The Penitent | Monsters | Penitent killed |
| `Codex_ThePaleStag` | `CodexEntry_ThePaleStag` | The Pale Stag Ascendant | Monsters | Pale Stag killed |
| `Codex_TheSuture` | `CodexEntry_TheSuture` | The Suture | Monsters | Year 29 auto |

**Write body text for each** (settler voice — short, uncertain, haunted):

Example for Codex_FirstRuins:
```
"Found worked stone beneath the new crafter foundation.
Too regular to be natural. Too old to be ours.
Someone built here long before we arrived.
We try not to think about what happened to them."
```

Example for Codex_TheSpite / Ironhide:
```
"We keep finding evidence of it but never the creature itself.
The bait untouched. Prints the size of a hunter's torso.
Something has been watching us claim this land.
It does not appear concerned."
```

Write each in this voice — one paragraph, 2–5 sentences.

---

## Part 3: Chronicle & Codex UI in Settlement

Add two new tabs to the settlement screen. In the existing `SettlementScreenController.cs`, register tab handlers for `tab-chronicle` and `tab-codex`. (These tabs may already exist as stubs — wire them up properly.)

### ChronicleController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/ChronicleController.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class ChronicleController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        public void RefreshChronicle()
        {
            var root    = _uiDocument.rootVisualElement;
            var list    = root.Q<ScrollView>("chronicle-list");
            if (list == null) return;
            list.Clear();

            var state   = GameStateManager.Instance.CampaignState;
            if (state.chronicleEntries == null || state.chronicleEntries.Length == 0)
            {
                var empty = new Label("The chronicle is empty. The settlement is young.");
                empty.style.color    = new Color(0.54f, 0.54f, 0.54f);
                empty.style.fontSize = 10;
                empty.style.paddingTop = 24;
                list.Add(empty);
                return;
            }

            // Show most recent first
            for (int i = state.chronicleEntries.Length - 1; i >= 0; i--)
            {
                var entry     = state.chronicleEntries[i];
                var entryEl   = new VisualElement();
                entryEl.style.borderBottomColor = new StyleColor(new Color(0.20f, 0.18f, 0.14f));
                entryEl.style.borderBottomWidth = 1;
                entryEl.style.paddingTop        = 10;
                entryEl.style.paddingBottom     = 10;
                entryEl.style.paddingLeft       = entryEl.style.paddingRight = 8;
                entryEl.style.marginBottom      = 4;

                var yearLabel = new Label(entry.yearLabel);
                yearLabel.style.color    = new Color(0.72f, 0.52f, 0.04f);
                yearLabel.style.fontSize = 9;
                yearLabel.style.marginBottom = 4;
                entryEl.Add(yearLabel);

                var text = new Label(entry.entryText);
                text.style.color     = new Color(0.83f, 0.80f, 0.73f);
                text.style.fontSize  = 10;
                text.style.whiteSpace = WhiteSpace.Normal;
                entryEl.Add(text);

                list.Add(entryEl);
            }
        }
    }

    [System.Serializable]
    public struct ChronicleEntry
    {
        public string yearLabel;   // e.g. "Year 1"
        public string entryText;   // The narrative text
    }
}
```

Add `ChronicleEntry[]` array to `CampaignState`. Add a method to `GameStateManager`:
```csharp
public void AddChronicleEntry(int year, string text)
{
    var entry = new ChronicleController.ChronicleEntry
        { yearLabel = $"Year {year}", entryText = text };
    var list = new System.Collections.Generic.List<ChronicleController.ChronicleEntry>(
        CampaignState.chronicleEntries ?? new ChronicleController.ChronicleEntry[0]);
    list.Add(entry);
    CampaignState.chronicleEntries = list.ToArray();
}
```

### CodexController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CodexController.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CodexController : MonoBehaviour
    {
        [SerializeField] private UIDocument    _uiDocument;
        [SerializeField] private CodexEntrySO[] _allEntries;

        public void RefreshCodex()
        {
            var root  = _uiDocument.rootVisualElement;
            var grid  = root.Q("codex-grid");
            if (grid == null) return;
            grid.Clear();

            var unlocked = GameStateManager.Instance.CampaignState.unlockedCodexEntryIds ?? new string[0];

            foreach (var entry in _allEntries)
            {
                if (entry == null) continue;
                bool isUnlocked = System.Array.IndexOf(unlocked, entry.entryId) >= 0;

                var card = new VisualElement();
                card.style.width            = 180;
                card.style.minHeight        = 80;
                card.style.backgroundColor  = isUnlocked
                    ? new StyleColor(new Color(0.08f, 0.06f, 0.04f))
                    : new StyleColor(new Color(0.05f, 0.04f, 0.03f));
                card.style.borderTopColor   = card.style.borderBottomColor =
                card.style.borderLeftColor  = card.style.borderRightColor  =
                    new StyleColor(isUnlocked
                        ? new Color(0.31f, 0.27f, 0.20f)
                        : new Color(0.15f, 0.13f, 0.10f));
                card.style.borderTopWidth   = card.style.borderBottomWidth =
                card.style.borderLeftWidth  = card.style.borderRightWidth  = 1;
                card.style.paddingTop       = card.style.paddingBottom =
                card.style.paddingLeft      = card.style.paddingRight  = 10;
                card.style.marginBottom     = card.style.marginRight   = 8;

                if (isUnlocked)
                {
                    var catLabel = new Label(entry.category.ToUpper());
                    catLabel.style.color    = new Color(0.72f, 0.52f, 0.04f);
                    catLabel.style.fontSize = 7;
                    catLabel.style.marginBottom = 4;
                    card.Add(catLabel);

                    var title = new Label(entry.entryTitle);
                    title.style.color    = new Color(0.83f, 0.80f, 0.73f);
                    title.style.fontSize = 10;
                    title.style.marginBottom = 6;
                    title.style.unityFontStyleAndWeight = FontStyle.Bold;
                    card.Add(title);

                    // Preview text (first 60 chars)
                    string preview = entry.bodyText.Length > 60
                        ? entry.bodyText.Substring(0, 60) + "..."
                        : entry.bodyText;
                    var body = new Label(preview);
                    body.style.color    = new Color(0.54f, 0.54f, 0.54f);
                    body.style.fontSize = 8;
                    body.style.whiteSpace = WhiteSpace.Normal;
                    card.Add(body);

                    // Click to expand (expand in-place for now)
                    card.RegisterCallback<ClickEvent>(_ => ExpandEntry(entry));
                }
                else
                {
                    var unknown = new Label("???");
                    unknown.style.color    = new Color(0.25f, 0.22f, 0.18f);
                    unknown.style.fontSize = 14;
                    unknown.style.alignSelf = Align.Center;
                    unknown.style.marginTop = 20;
                    card.Add(unknown);
                }

                grid.Add(card);
            }
        }

        private void ExpandEntry(CodexEntrySO entry)
        {
            // Simple modal expansion — reuse SlideIn from SceneTransitionManager
            Debug.Log($"[Codex] Expand: {entry.entryTitle}");
            // TODO: build full-text modal overlay
        }
    }
}
```

---

## Part 4: Add Chronicle/Codex UXML Sections

In the settlement UXML (inside the tab content area), add:

```xml
<!-- Chronicle tab content -->
<ui:VisualElement name="tab-content-chronicle" style="display:none; flex-grow:1;">
  <ui:ScrollView name="chronicle-list" style="flex-grow:1; padding:8px;" />
</ui:VisualElement>

<!-- Codex tab content -->
<ui:VisualElement name="tab-content-codex" style="display:none; flex-grow:1;">
  <ui:VisualElement name="codex-grid" style="flex-direction:row; flex-wrap:wrap;
      padding:16px; align-content:flex-start;" />
</ui:VisualElement>
```

---

## Verification Test

- [ ] All 15 CodexEntrySO assets exist in `Assets/_Game/Data/Codex/`
- [ ] Switch to Chronicle tab in Settlement — shows "chronicle is empty" message Year 1
- [ ] Resolve EVT-01 — chronicle entry "Year 1" appears in the log
- [ ] Switch to Codex tab — shows locked "???" cards for all undiscovered entries
- [ ] EVT-09 fires (Foundation Stones) — "First Ruins" codex entry becomes visible
- [ ] Click "First Ruins" card — title and body text display
- [ ] Body text is in settler voice (short, uncertain, personal)
- [ ] `Codex_MarrowLore` and `Codex_SettlementLog` are already unlocked at campaign start

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_V.md`
**Covers:** Birth, retirement, and year-end screens — BirthController (name a newborn, show sprite), RetirementController (veteran narrative and choice), YearEndSummaryController (hunts done, deaths, crafts built)
