<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-W | Save/Load UI, Game Over & Victory Epilogue
Status: Stage 8-V complete. Birth, retirement, year-end screens working.
Task: Build the Save/Load slot selection screen, Game Over screen
(triggered on a Total Party Kill or Ironman death), and Victory
Epilogue screen (Year 30 reached). Each is a full-screen overlay
or scene. Save/Load uses 3 save slots stored as JSON on disk.
Game Over shows the dead hunter's name, cause of death, and the
campaign's chronicle. Victory shows a generated text epilogue
based on campaign outcomes.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_W.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.UI/MainMenuController.cs

Then confirm:
- Save slots are JSON files in Application.persistentDataPath
- Slot file names: save_slot_0.json, save_slot_1.json, save_slot_2.json
- Game Over is a full scene (not a modal) — loads "GameOver" scene
- Victory Epilogue is also a full scene — loads "VictoryEpilogue" scene
- Ironman death on any hunter = Game Over immediately
- Standard campaign: Game Over only when ALL hunters are dead
- What you will NOT build (cloud save, cross-platform sync — post-MVP)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-W: Save/Load UI, Game Over & Victory Epilogue

**Resuming from:** Stage 8-V complete — Birth, retirement, and year-end screens working
**Done when:** Save/Load slot screen is functional; Game Over triggers correctly and shows full history; Year 30 victory loads epilogue with outcome-driven narrative
**Commit:** `"8W: Save/Load slots, Game Over screen, Victory Epilogue"`
**Next session:** STAGE_08_X.md

---

## Part 1: Save System

**New developer note:** Unity doesn't automatically save your game. We store the entire `CampaignState` object as a JSON text file on the player's computer. `Application.persistentDataPath` is a folder Unity provides for each game — it exists on every platform and doesn't get deleted when the game updates.

### SaveSystem.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/SaveSystem.cs`

```csharp
using System.IO;
using UnityEngine;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public static class SaveSystem
    {
        private const int SlotCount = 3;

        private static string SlotPath(int slot) =>
            Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

        // ── Save ──────────────────────────────────────────────────────────

        public static void Save(int slot, CampaignState state)
        {
            string json = JsonUtility.ToJson(state, prettyPrint: true);
            File.WriteAllText(SlotPath(slot), json);
            Debug.Log($"[Save] Slot {slot} saved to {SlotPath(slot)}");
        }

        // ── Load ──────────────────────────────────────────────────────────

        public static CampaignState Load(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Save] No save found at slot {slot}");
                return null;
            }
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<CampaignState>(json);
        }

        // ── Slot Info (for UI display) ────────────────────────────────────

        public static SaveSlotInfo GetSlotInfo(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo { isEmpty = true, slot = slot };

            try
            {
                string json = File.ReadAllText(path);
                var state   = JsonUtility.FromJson<CampaignState>(json);
                return new SaveSlotInfo
                {
                    isEmpty      = false,
                    slot         = slot,
                    campaignName = state.campaignName ?? "Campaign",
                    year         = state.currentYear,
                    isIronman    = state.isIronman,
                    savedAt      = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm")
                };
            }
            catch
            {
                return new SaveSlotInfo { isEmpty = true, slot = slot };
            }
        }

        // ── Delete ────────────────────────────────────────────────────────

        public static void Delete(int slot)
        {
            string path = SlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
            Debug.Log($"[Save] Slot {slot} deleted");
        }

        public static bool HasAnySave()
        {
            for (int i = 0; i < SlotCount; i++)
                if (File.Exists(SlotPath(i))) return true;
            return false;
        }

        public static int SlotCount_ => SlotCount;
    }

    [System.Serializable]
    public class SaveSlotInfo
    {
        public bool   isEmpty;
        public int    slot;
        public string campaignName;
        public int    year;
        public bool   isIronman;
        public string savedAt;
    }
}
```

### Wiring Save into GameStateManager

Add:

```csharp
public int CurrentSaveSlot { get; private set; } = 0;

public void SaveGame(int slot)
{
    CurrentSaveSlot = slot;
    SaveSystem.Save(slot, CampaignState);
}

public void LoadGame(int slot)
{
    var state = SaveSystem.Load(slot);
    if (state == null) return;
    CurrentSaveSlot = slot;
    CampaignState   = state;
    Debug.Log($"[GameState] Loaded slot {slot} — Year {state.currentYear}");
}
```

---

## Part 2: Save/Load Slot UI

**New developer note:** This screen appears when the player clicks CONTINUE or SAVE GAME. It shows three slots side by side. Empty slots say "Empty". Filled slots show the campaign name, year, and save date.

### SaveLoadController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/SaveLoadController.cs`

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public enum SaveLoadMode { Save, Load }

    public class SaveLoadController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private SaveLoadMode _mode;
        private VisualElement _overlay;
        private System.Action _onClose;

        /// <summary>Show save/load slot selection over the current screen.</summary>
        public void Show(SaveLoadMode mode, System.Action onClose = null)
        {
            _mode    = mode;
            _onClose = onClose;
            BuildUI();
        }

        private void BuildUI()
        {
            var root = _uiDocument.rootVisualElement;

            _overlay = new VisualElement();
            _overlay.style.position        = Position.Absolute;
            _overlay.style.left = _overlay.style.top =
            _overlay.style.right = _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.85f));
            _overlay.style.alignItems      = Align.Center;
            _overlay.style.justifyContent  = Justify.Center;
            root.Add(_overlay);

            var panel = new VisualElement();
            panel.style.width           = 600;
            panel.style.backgroundColor = new StyleColor(new Color(0.06f, 0.05f, 0.03f));
            panel.style.borderTopColor  = panel.style.borderBottomColor =
            panel.style.borderLeftColor = panel.style.borderRightColor =
                new StyleColor(new Color(0.31f, 0.27f, 0.20f));
            panel.style.borderTopWidth  = panel.style.borderBottomWidth =
            panel.style.borderLeftWidth = panel.style.borderRightWidth = 2;
            panel.style.paddingTop      = panel.style.paddingBottom =
            panel.style.paddingLeft     = panel.style.paddingRight = 28;
            _overlay.Add(panel);

            // Title
            string title = _mode == SaveLoadMode.Save ? "SAVE GAME" : "LOAD GAME";
            var titleLabel = new Label(title);
            titleLabel.style.color    = new Color(0.72f, 0.52f, 0.04f);
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 20;
            panel.Add(titleLabel);

            // Slot row
            var slotRow = new VisualElement();
            slotRow.style.flexDirection  = FlexDirection.Row;
            slotRow.style.justifyContent = Justify.SpaceBetween;
            slotRow.style.marginBottom   = 20;
            panel.Add(slotRow);

            for (int i = 0; i < 3; i++)
            {
                int capturedSlot = i;
                var info = SaveSystem.GetSlotInfo(i);
                slotRow.Add(BuildSlotCard(info, capturedSlot));
            }

            // Cancel button
            var cancelBtn = new Button { text = "CANCEL" };
            cancelBtn.style.alignSelf       = Align.FlexEnd;
            cancelBtn.style.color           = new StyleColor(new Color(0.54f, 0.54f, 0.54f));
            cancelBtn.style.fontSize        = 10;
            cancelBtn.style.backgroundColor = StyleKeyword.None;
            cancelBtn.style.borderTopWidth  = cancelBtn.style.borderBottomWidth =
            cancelBtn.style.borderLeftWidth = cancelBtn.style.borderRightWidth = 0;
            cancelBtn.RegisterCallback<ClickEvent>(_ => Close());
            panel.Add(cancelBtn);
        }

        private VisualElement BuildSlotCard(SaveSlotInfo info, int slot)
        {
            var card = new VisualElement();
            card.style.width           = 160;
            card.style.minHeight       = 120;
            card.style.backgroundColor = new StyleColor(new Color(0.08f, 0.07f, 0.05f));
            card.style.borderTopColor  = card.style.borderBottomColor =
            card.style.borderLeftColor = card.style.borderRightColor =
                new StyleColor(new Color(0.20f, 0.18f, 0.14f));
            card.style.borderTopWidth  = card.style.borderBottomWidth =
            card.style.borderLeftWidth = card.style.borderRightWidth = 1;
            card.style.paddingTop      = card.style.paddingBottom =
            card.style.paddingLeft     = card.style.paddingRight = 12;
            card.style.cursor          = new StyleCursor(StyleKeyword.Auto);

            var slotLabel = new Label($"SLOT {slot + 1}");
            slotLabel.style.color    = new Color(0.45f, 0.43f, 0.40f);
            slotLabel.style.fontSize = 8;
            slotLabel.style.marginBottom = 8;
            card.Add(slotLabel);

            if (info.isEmpty)
            {
                var emptyLabel = new Label("Empty");
                emptyLabel.style.color    = new Color(0.30f, 0.28f, 0.24f);
                emptyLabel.style.fontSize = 12;
                card.Add(emptyLabel);

                if (_mode == SaveLoadMode.Save)
                    card.RegisterCallback<ClickEvent>(_ => OnSlotClicked(slot, isEmpty: true));
            }
            else
            {
                var campaignName = new Label(info.campaignName.ToUpper());
                campaignName.style.color    = new Color(0.83f, 0.80f, 0.73f);
                campaignName.style.fontSize = 11;
                campaignName.style.unityFontStyleAndWeight = FontStyle.Bold;
                campaignName.style.marginBottom = 4;
                card.Add(campaignName);

                var yearLabel = new Label($"Year {info.year}");
                yearLabel.style.color    = new Color(0.72f, 0.52f, 0.04f);
                yearLabel.style.fontSize = 10;
                yearLabel.style.marginBottom = 4;
                card.Add(yearLabel);

                if (info.isIronman)
                {
                    var ironLabel = new Label("IRONMAN");
                    ironLabel.style.color    = new Color(0.60f, 0.20f, 0.20f);
                    ironLabel.style.fontSize = 7;
                    ironLabel.style.marginBottom = 4;
                    card.Add(ironLabel);
                }

                var dateLabel = new Label(info.savedAt);
                dateLabel.style.color    = new Color(0.35f, 0.33f, 0.28f);
                dateLabel.style.fontSize = 8;
                card.Add(dateLabel);

                card.RegisterCallback<ClickEvent>(_ => OnSlotClicked(slot, isEmpty: false));

                // Hover effect
                card.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    card.style.borderTopColor  = card.style.borderBottomColor =
                    card.style.borderLeftColor = card.style.borderRightColor =
                        new StyleColor(new Color(0.72f, 0.52f, 0.04f));
                });
                card.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    card.style.borderTopColor  = card.style.borderBottomColor =
                    card.style.borderLeftColor = card.style.borderRightColor =
                        new StyleColor(new Color(0.20f, 0.18f, 0.14f));
                });
            }

            return card;
        }

        private void OnSlotClicked(int slot, bool isEmpty)
        {
            if (_mode == SaveLoadMode.Load)
            {
                if (isEmpty) return; // Can't load empty slot
                GameStateManager.Instance.LoadGame(slot);
                Close();
                SceneTransitionManager.Instance.LoadScene("Settlement");
            }
            else // Save
            {
                if (!isEmpty)
                {
                    // Show overwrite confirmation
                    ShowOverwriteConfirm(slot);
                }
                else
                {
                    GameStateManager.Instance.SaveGame(slot);
                    Close();
                }
            }
        }

        private void ShowOverwriteConfirm(int slot)
        {
            // Build a small confirm dialog on top of the current overlay
            var root    = _uiDocument.rootVisualElement;
            var confirm = new VisualElement();
            confirm.style.position        = Position.Absolute;
            confirm.style.left = confirm.style.top =
            confirm.style.right = confirm.style.bottom = 0;
            confirm.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.6f));
            confirm.style.alignItems      = Align.Center;
            confirm.style.justifyContent  = Justify.Center;
            root.Add(confirm);

            var box = new VisualElement();
            box.style.width           = 300;
            box.style.backgroundColor = new StyleColor(new Color(0.08f, 0.06f, 0.04f));
            box.style.borderTopColor  = box.style.borderBottomColor =
            box.style.borderLeftColor = box.style.borderRightColor =
                new StyleColor(new Color(0.60f, 0.20f, 0.20f));
            box.style.borderTopWidth  = box.style.borderBottomWidth =
            box.style.borderLeftWidth = box.style.borderRightWidth = 2;
            box.style.paddingTop      = box.style.paddingBottom =
            box.style.paddingLeft     = box.style.paddingRight = 20;
            confirm.Add(box);

            var msg = new Label($"Overwrite Slot {slot + 1}?");
            msg.style.color    = new Color(0.83f, 0.80f, 0.73f);
            msg.style.fontSize = 12;
            msg.style.marginBottom = 16;
            box.Add(msg);

            var row = new VisualElement();
            row.style.flexDirection  = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            box.Add(row);

            var yes = new Button { text = "OVERWRITE" };
            yes.style.color    = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
            yes.style.fontSize = 10;
            yes.RegisterCallback<ClickEvent>(_ =>
            {
                root.Remove(confirm);
                GameStateManager.Instance.SaveGame(slot);
                Close();
            });
            row.Add(yes);

            var no = new Button { text = "CANCEL" };
            no.style.color    = new StyleColor(new Color(0.54f, 0.54f, 0.54f));
            no.style.fontSize = 10;
            no.RegisterCallback<ClickEvent>(_ => root.Remove(confirm));
            row.Add(no);
        }

        private void Close()
        {
            if (_overlay != null && _overlay.panel != null)
                _uiDocument.rootVisualElement.Remove(_overlay);
            _onClose?.Invoke();
        }
    }
}
```

---

## Part 3: Game Over Scene

**New developer note:** The "GameOver" scene is a separate Unity scene — like a different room the game enters when things go wrong. Create it via **File → New Scene → Empty**, save as `Assets/GameOver.unity`, and add it to Build Settings.

### GameOver.uxml

**Path:** `Assets/_Game/UI/GameOver.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; background-color:#0A0A0C;
      align-items:center; justify-content:flex-start; padding-top:48px;">

    <!-- Title -->
    <ui:Label name="gameover-title" text="THE SETTLEMENT FALLS"
              style="color:#4A2020; font-size:32px; -unity-font-style:bold;
                     margin-bottom:8px;" />
    <ui:Label name="gameover-subtitle" text="Year 7 · The last hunter fell to the Gaunt"
              style="color:#545454; font-size:11px; margin-bottom:40px;" />

    <!-- Chronicle scroll -->
    <ui:ScrollView name="chronicle-scroll"
                   style="width:560px; height:280px; border-color:#1E1A12;
                          border-width:1px; padding:16px; margin-bottom:32px;" />

    <!-- Buttons -->
    <ui:VisualElement style="flex-direction:row; margin-top:16px;">
      <ui:Button name="btn-main-menu" text="MAIN MENU"
                 style="width:180px; height:48px; margin-right:16px;
                        background-color:#150D0D; border-color:#4A2020;
                        border-width:2px; color:#D4CCBA; font-size:13px;" />
      <ui:Button name="btn-new-run" text="NEW CAMPAIGN"
                 style="width:180px; height:48px;
                        background-color:#0D1510; border-color:#1E3A1E;
                        border-width:2px; color:#D4CCBA; font-size:13px;" />
    </ui:VisualElement>

  </ui:VisualElement>
</ui:UXML>
```

### GameOverController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/GameOverController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.UI;

namespace MnM.Core.UI
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root     = _uiDocument.rootVisualElement;
            var state    = GameStateManager.Instance.CampaignState;

            // Subtitle: cause of death context
            string subtitle = BuildSubtitle(state);
            root.Q<Label>("gameover-subtitle").text = subtitle;

            // Chronicle scroll — show all entries
            var scroll = root.Q<ScrollView>("chronicle-scroll");
            scroll.Clear();
            if (state.chronicleEntries != null)
            {
                for (int i = state.chronicleEntries.Length - 1; i >= 0; i--)
                {
                    var entry = state.chronicleEntries[i];
                    var row   = new VisualElement();
                    row.style.marginBottom = 10;

                    var year = new Label(entry.yearLabel);
                    year.style.color    = new Color(0.45f, 0.43f, 0.40f);
                    year.style.fontSize = 8;
                    row.Add(year);

                    var text = new Label(entry.entryText);
                    text.style.color     = new Color(0.72f, 0.65f, 0.54f);
                    text.style.fontSize  = 10;
                    text.style.whiteSpace = WhiteSpace.Normal;
                    row.Add(text);

                    scroll.Add(row);
                }
            }

            // Buttons
            root.Q<Button>("btn-main-menu").RegisterCallback<ClickEvent>(_ =>
                SceneTransitionManager.Instance.LoadScene("MainMenu"));

            root.Q<Button>("btn-new-run").RegisterCallback<ClickEvent>(_ =>
                SceneTransitionManager.Instance.LoadScene("CampaignSelect"));

            // Delete ironman save on game over
            if (state.isIronman)
                SaveSystem.Delete(GameStateManager.Instance.CurrentSaveSlot);

            // Dramatic fade in
            StartCoroutine(FadeIn(root));
        }

        private string BuildSubtitle(CampaignState state)
        {
            string year = $"Year {state.currentYear}";
            if (state.isIronman)
                return $"{year} · Ironman ended.";
            return $"{year} · All hunters have fallen.";
        }

        private IEnumerator FadeIn(VisualElement root)
        {
            root.style.opacity = 0;
            yield return new WaitForSeconds(0.5f);
            float t = 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                root.style.opacity = Mathf.Clamp01(t);
                yield return null;
            }
        }
    }
}
```

### Triggering Game Over from GameStateManager

```csharp
/// <summary>
/// Call after any hunter death. Checks for total party kill or ironman loss.
/// </summary>
public void CheckGameOver(string deadHunterName)
{
    if (CampaignState.isIronman)
    {
        // Any death = game over
        Debug.Log($"[GameOver] Ironman — {deadHunterName} died.");
        SceneTransitionManager.Instance.LoadScene("GameOver");
        return;
    }

    // Standard: check if all hunters dead
    bool allDead = true;
    if (CampaignState.hunters != null)
        foreach (var h in CampaignState.hunters)
            if (!h.isDead) { allDead = false; break; }

    if (allDead)
    {
        Debug.Log("[GameOver] Total party kill.");
        SceneTransitionManager.Instance.LoadScene("GameOver");
    }
}
```

---

## Part 4: Victory Epilogue Scene

**New developer note:** At Year 30, the campaign ends in victory. The epilogue scene shows a narrative text that changes based on what the player accomplished — did they kill all three overlords? Did they lose many hunters? Did they uncover many codex entries? This is the game's ending screen.

### VictoryEpilogue.uxml

**Path:** `Assets/_Game/UI/VictoryEpilogue.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; background-color:#0A0A0C;
      align-items:center; padding-top:64px; padding-left:80px; padding-right:80px;">

    <ui:Label name="victory-title" text="THE SETTLEMENT STANDS"
              style="color:#B8860B; font-size:28px; -unity-font-style:bold;
                     margin-bottom:12px;" />
    <ui:Label name="victory-year" text="YEAR 30"
              style="color:#545454; font-size:10px; margin-bottom:40px;" />

    <!-- Scrollable epilogue text -->
    <ui:ScrollView name="epilogue-scroll"
                   style="width:640px; height:320px; margin-bottom:32px;" />

    <!-- Stats row -->
    <ui:VisualElement name="stats-row"
                      style="flex-direction:row; margin-bottom:40px;" />

    <ui:Button name="btn-credits" text="VIEW CREDITS"
               style="width:200px; height:48px;
                      background-color:rgba(184,134,11,0.15);
                      border-color:#B8860B; border-width:2px;
                      color:#D4CCBA; font-size:14px;" />

  </ui:VisualElement>
</ui:UXML>
```

### VictoryEpilogueController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/VictoryEpilogueController.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class VictoryEpilogueController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            var root  = _uiDocument.rootVisualElement;
            var state = GameStateManager.Instance.CampaignState;

            // Epilogue text — driven by outcomes
            string epilogue = BuildEpilogueText(state);
            var scroll = root.Q<ScrollView>("epilogue-scroll");
            scroll.Clear();

            // Reveal paragraphs with a typewriter-style stagger
            StartCoroutine(RevealEpilogue(scroll, epilogue));

            // Stats row
            var statsRow = root.Q("stats-row");
            AddStat(statsRow, "YEARS SURVIVED",   "30",
                              new Color(0.72f, 0.52f, 0.04f));
            AddStat(statsRow, "OVERLORDS SLAIN",  state.overlordKillCount.ToString(),
                              new Color(0.72f, 0.52f, 0.04f));
            AddStat(statsRow, "HUNTERS LOST",     state.totalHunterDeaths.ToString(),
                              new Color(0.60f, 0.20f, 0.20f));
            AddStat(statsRow, "CODEX ENTRIES",
                              (state.unlockedCodexEntryIds?.Length ?? 0).ToString(),
                              new Color(0.54f, 0.54f, 0.54f));

            // Credits button
            root.Q<Button>("btn-credits").RegisterCallback<ClickEvent>(_ =>
                SceneTransitionManager.Instance.LoadScene("Credits"));

            // Delete save — campaign is complete
            SaveSystem.Delete(GameStateManager.Instance.CurrentSaveSlot);

            StartCoroutine(FadeIn(root));
        }

        private string BuildEpilogueText(CampaignState state)
        {
            // Determine ending tier based on outcomes
            bool killedAllOverlords = state.overlordKillCount >= 3;
            bool lowDeaths          = state.totalHunterDeaths <= 4;
            bool highCodex          = (state.unlockedCodexEntryIds?.Length ?? 0) >= 12;

            if (killedAllOverlords && lowDeaths)
                return EPILOGUE_TRIUMPH;
            if (killedAllOverlords)
                return EPILOGUE_VICTORY;
            if (highCodex)
                return EPILOGUE_SCHOLAR;
            return EPILOGUE_SURVIVOR;
        }

        private const string EPILOGUE_TRIUMPH =
            "Thirty years.\n\n" +
            "The deep things are gone — or at least quiet. The settlement's walls are real now, " +
            "not just hope stacked on bone. Children who never knew the first lean years look at the " +
            "hunters' hall and see legend.\n\n" +
            "The elders do not correct them.\n\n" +
            "It cost what it cost. Those names are in the stone now. " +
            "That is enough. That will have to be enough.\n\n" +
            "The land does not forgive. But it remembers those who endured it.";

        private const string EPILOGUE_VICTORY =
            "Thirty years.\n\n" +
            "The great terrors have been brought low. Not without cost. Never without cost. " +
            "The settlement carries its dead carefully — names spoken at the year fire, " +
            "tools kept even when broken.\n\n" +
            "There are still things in the dark. There always will be.\n\n" +
            "But the settlement stands. And that was never certain.";

        private const string EPILOGUE_SCHOLAR =
            "Thirty years.\n\n" +
            "The chronicle is long and the codex nearly full. " +
            "Someone will read it someday and understand what this land is and was — " +
            "why the bones glow, what the old structures were for, " +
            "what the creatures wanted.\n\n" +
            "Or they will read it and remain as confused as the settlers always were. " +
            "That is also a kind of knowledge.\n\n" +
            "The settlement stands. The record stands. That is more than most manage.";

        private const string EPILOGUE_SURVIVOR =
            "Thirty years.\n\n" +
            "You endured. The settlement endured. " +
            "Not triumphant — the word would feel wrong in your mouth. " +
            "But present. Standing. That matters.\n\n" +
            "The land broke some of you. Took names you still can't say aloud. " +
            "You're still here anyway.\n\n" +
            "Thirty years is a lifetime in a place like this. " +
            "You spent it well.";

        private IEnumerator RevealEpilogue(ScrollView scroll, string fullText)
        {
            // Split on double newlines into paragraphs
            string[] paragraphs = fullText.Split(new[] { "\n\n" },
                System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var para in paragraphs)
            {
                var label = new Label(para.Replace("\\n", "\n"));
                label.style.color       = new Color(0.72f, 0.65f, 0.54f);
                label.style.fontSize    = 11;
                label.style.whiteSpace  = WhiteSpace.Normal;
                label.style.marginBottom = 16;
                label.style.opacity     = 0;
                scroll.Add(label);

                // Fade paragraph in
                float t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    label.style.opacity = t / 0.5f;
                    yield return null;
                }
                label.style.opacity = 1;
                yield return new WaitForSeconds(0.8f);
            }
        }

        private void AddStat(VisualElement parent, string label,
                              string value, Color color)
        {
            var block = new VisualElement();
            block.style.marginRight  = 40;
            block.style.alignItems   = Align.FlexStart;
            parent.Add(block);

            var val = new Label(value);
            val.style.color    = color;
            val.style.fontSize = 24;
            val.style.unityFontStyleAndWeight = FontStyle.Bold;
            block.Add(val);

            var lbl = new Label(label);
            lbl.style.color    = new Color(0.40f, 0.38f, 0.34f);
            lbl.style.fontSize = 7;
            block.Add(lbl);
        }

        private IEnumerator FadeIn(VisualElement root)
        {
            root.style.opacity = 0;
            yield return new WaitForSeconds(1.0f);
            float t = 0f;
            while (t < 1.5f)
            {
                t += Time.deltaTime;
                root.style.opacity = Mathf.Clamp01(t / 1.5f);
                yield return null;
            }
        }
    }
}
```

### Triggering Victory from GameStateManager

```csharp
/// <summary>Call at the end of Year 30 settlement phase.</summary>
public void CheckVictory()
{
    if (CampaignState.currentYear >= 30)
    {
        Debug.Log("[Victory] Year 30 reached — loading epilogue.");
        SceneTransitionManager.Instance.LoadScene("VictoryEpilogue");
    }
}
```

Add to `CampaignState`:

```csharp
public int overlordKillCount;
public int totalHunterDeaths;
```

Increment `totalHunterDeaths` wherever a hunter death is recorded; increment `overlordKillCount` when an overlord is killed.

---

## Part 5: Build Settings — Final Scene Order

**In Unity:** File → Build Settings → Scenes In Build. Drag scenes to match this order exactly:

| Index | Scene |
|---|---|
| 0 | `Bootstrap` |
| 1 | `MainMenu` |
| 2 | `CampaignSelect` |
| 3 | `CharacterCreation` |
| 4 | `Settlement` |
| 5 | `HuntTravel` |
| 6 | `CombatScene` |
| 7 | `GameOver` |
| 8 | `VictoryEpilogue` |
| 9 | `Credits` *(stub — add an empty scene for now)* |

---

## Verification Test

- [ ] Click SAVE GAME in settlement → slot panel appears showing 3 slots
- [ ] Click empty slot → game saved, panel closes
- [ ] Click filled slot → overwrite confirmation dialog appears
- [ ] Confirm overwrite → slot updated with current year
- [ ] Click CONTINUE on Main Menu → Load panel appears with saved campaigns
- [ ] Click a filled Load slot → Settlement loads at correct year
- [ ] Cancel on Save/Load panel → panel closes, game continues
- [ ] All hunters die → `CheckGameOver()` fires → GameOver scene loads
- [ ] GameOver scene shows "THE SETTLEMENT FALLS" with correct year subtitle
- [ ] Chronicle scroll shows all entries most-recent-first
- [ ] MAIN MENU button → returns to main menu
- [ ] NEW CAMPAIGN button → loads CampaignSelect
- [ ] Ironman save deleted automatically on Game Over
- [ ] Reach Year 30 (test via `GameStateManager.Instance.CampaignState.currentYear = 30`) → Victory epilogue loads
- [ ] Epilogue text matches campaign outcome (all 3 overlords killed → TRIUMPH ending)
- [ ] Stats row shows correct overlord kills, hunter deaths, codex count
- [ ] Paragraphs fade in one by one

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_X.md`
**Covers:** Stage 8 Final Integration & Definition of Done — full end-to-end smoke test from Bootstrap through Victory Epilogue; all Stage 8 verification checklists re-run; commit tag `v0.8`
