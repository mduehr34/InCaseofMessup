<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-G | Codex Screen — Monsters, Artifacts, Settlement Records
Status: Stage 6-F complete. Hunt selection and travel phase
verified end-to-end. Combat loads correctly from Travel.
Task: Create codex-screen.uxml and CodexController.cs.
Three tabs: MONSTERS (locked = "???", unlocked = full info),
ARTIFACTS (locked = greyed), SETTLEMENT RECORDS (chronicle log).
Codex loads additively over Settlement.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_G.md
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/Scripts/Core.Data/MonsterSO.cs
- Assets/_Game/Scripts/Core.Data/ArtifactSO.cs
- Assets/_Game/UI/USS/settlement-shared.uss

Then confirm:
- That Codex loads ADDITIVELY (not replacing Settlement)
- That locked monsters show "???" for name — not silhouette
  art (art added in Stage 7)
- That Settlement Records tab reads directly from
  CampaignState.chronicleLog[] in settler voice
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-G: Codex Screen — All Three Tabs

**Resuming from:** Stage 6-F complete — hunt flow and travel screen verified  
**Done when:** Codex opens from Settlement; Monsters tab shows locked/unlocked states; Artifacts tab shows unlocked artifacts; Settlement Records tab shows full chronicle log; Close returns to Settlement  
**Commit:** `"6G: Codex screen — monsters, artifacts, settlement records tabs"`  
**Next session:** STAGE_06_H.md  

---

## Step 1: codex-screen.uxml

**Path:** `Assets/_Game/UI/UXML/codex-screen.uxml`

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Style src="../USS/tokens.uss"/>
    <Style src="../USS/stone-panel.uss"/>
    <Style src="../USS/settlement-shared.uss"/>
    <Style src="../USS/codex-screen.uss"/>

    <ui:VisualElement name="codex-root" class="fullscreen-bg codex-root">

        <!-- Tab Bar -->
        <ui:VisualElement name="codex-tabs" class="tab-bar stone-panel--raised codex-tab-bar">
            <ui:Label text="CODEX" class="era-year" style="margin-right:24px;"/>
            <ui:Button name="tab-monsters"  text="MONSTERS"           class="tab-btn tab-btn--active"/>
            <ui:Button name="tab-artifacts" text="ARTIFACTS"          class="tab-btn"/>
            <ui:Button name="tab-records"   text="SETTLEMENT RECORDS" class="tab-btn"/>
            <ui:VisualElement style="flex:1"/>
            <ui:Button name="btn-close"     text="CLOSE"              class="era-btn"/>
        </ui:VisualElement>

        <!-- Content Area: List + Detail -->
        <ui:VisualElement name="codex-content" class="codex-content">

            <!-- Left: Scrollable List -->
            <ui:ScrollView name="codex-list" class="codex-list stone-panel"/>

            <!-- Right: Detail Panel -->
            <ui:VisualElement name="codex-detail" class="codex-detail stone-panel">
                <ui:Label name="detail-title" text="Select an entry" class="stone-panel__header"/>
                <ui:Label name="detail-meta"  text=""                class="proficiency-label"/>
                <ui:ScrollView name="detail-body-scroll" class="detail-body-scroll">
                    <ui:Label name="detail-body" text=""             class="codex-detail-body"/>
                </ui:ScrollView>
            </ui:VisualElement>

        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

---

## Step 2: codex-screen.uss

**Path:** `Assets/_Game/UI/USS/codex-screen.uss`

```css
.codex-root       { flex-direction: column; }

.codex-tab-bar    { flex-direction: row; align-items: center; height: 60px; flex-shrink: 0; }

.codex-content    { flex: 1; flex-direction: row; min-height: 0; }

.codex-list       { width: 460px; flex-shrink: 0; margin: 2px; overflow: hidden; }

.codex-detail     { flex: 1; flex-direction: column; margin: 2px; overflow: hidden; }

.detail-body-scroll { flex: 1; }

.codex-detail-body {
    font-size:   var(--font-size-body);
    color:       var(--color-text-primary);
    white-space: normal;
}

/* ── List Entry ───────────────────────────────────────────────── */
.codex-entry {
    flex-direction: row;
    align-items:    center;
    padding:        var(--spacing-sm);
    border-bottom-color: var(--color-border);
    border-bottom-width: 1px;
}

.codex-entry:hover      { background-color: var(--color-bg-panel-raised); }
.codex-entry--selected  { background-color: var(--color-bg-panel-active); border-color: var(--color-text-accent); }
.codex-entry--locked    { opacity: 0.45; }

.codex-entry-name {
    flex:      1;
    font-size: var(--font-size-body);
    color:     var(--color-text-primary);
}

.codex-entry-tag {
    font-size:  var(--font-size-small);
    color:      var(--color-text-dim);
    margin-left: var(--spacing-sm);
}

/* ── Chronicle Records ───────────────────────────────────────── */
.chronicle-entry {
    font-size:    var(--font-size-body);
    color:        var(--color-text-primary);
    white-space:  normal;
    padding:      var(--spacing-xs) 0;
    border-bottom-color: var(--color-border);
    border-bottom-width: 1px;
}

.chronicle-divider {
    font-size:        var(--font-size-label);
    color:            var(--color-text-accent);
    -unity-font-style: bold;
    margin:           var(--spacing-sm) 0 var(--spacing-xs) 0;
}
```

---

## Step 3: CodexController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/CodexController.cs`

```csharp
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class CodexController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private CampaignSO  _campaignSO;

        private VisualElement _root;
        private ScrollView    _list;
        private Label         _detailTitle;
        private Label         _detailMeta;
        private Label         _detailBody;
        private string        _activeTab = "monsters";

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;
            _list        = _root.Q<ScrollView>("codex-list");
            _detailTitle = _root.Q<Label>("detail-title");
            _detailMeta  = _root.Q<Label>("detail-meta");
            _detailBody  = _root.Q<Label>("detail-body");

            WireTabs();
            BuildMonstersTab(); // Default
        }

        private void WireTabs()
        {
            _root.Q<Button>("tab-monsters").clicked  += () => SwitchTab("monsters");
            _root.Q<Button>("tab-artifacts").clicked += () => SwitchTab("artifacts");
            _root.Q<Button>("tab-records").clicked   += () => SwitchTab("records");
            _root.Q<Button>("btn-close").clicked     += OnClose;
        }

        private void SwitchTab(string tab)
        {
            _activeTab = tab;
            foreach (var (btn, name) in new[] {
                ("tab-monsters",  "monsters"),
                ("tab-artifacts", "artifacts"),
                ("tab-records",   "records") })
            {
                _root.Q<Button>(btn)?.EnableInClassList("tab-btn--active", name == tab);
            }

            ClearDetail();
            switch (tab)
            {
                case "monsters":  BuildMonstersTab();  break;
                case "artifacts": BuildArtifactsTab(); break;
                case "records":   BuildRecordsTab();   break;
            }
        }

        // ── Monsters Tab ─────────────────────────────────────────
        private void BuildMonstersTab()
        {
            _list.Clear();
            if (_campaignSO?.monsterRoster == null) return;

            var state = GameStateManager.Instance?.CampaignState;

            foreach (var monster in _campaignSO.monsterRoster)
            {
                if (monster == null) continue;

                // A monster is "known" if it appears in the chronicle log
                bool isKnown = state?.chronicleLog?.Any(
                    entry => entry.Contains(monster.monsterName)) ?? false;

                var entry = new VisualElement();
                entry.AddToClassList("codex-entry");
                if (!isKnown) entry.AddToClassList("codex-entry--locked");

                var nameLabel = new Label(isKnown ? monster.monsterName : "???");
                nameLabel.AddToClassList("codex-entry-name");
                entry.Add(nameLabel);

                var tierTag = new Label($"Tier {monster.materialTier}");
                tierTag.AddToClassList("codex-entry-tag");
                entry.Add(tierTag);

                if (isKnown)
                {
                    var monsterRef = monster;
                    entry.RegisterCallback<ClickEvent>(_ => ShowMonsterDetail(monsterRef));
                }

                _list.Add(entry);
            }
        }

        private void ShowMonsterDetail(MonsterSO monster)
        {
            _detailTitle.text = monster.monsterName;
            _detailMeta.text  = $"Material Tier {monster.materialTier}";

            // Settler-voice description
            var bodyParts = new System.Collections.Generic.List<string>();
            foreach (var part in monster.standardParts ?? new MonsterBodyPart[0])
                bodyParts.Add(part.partName);

            _detailBody.text =
                $"{monster.animalBasis}\n\n" +
                $"Combat: {monster.combatEmotion}\n\n" +
                $"Body parts: {string.Join(", ", bodyParts)}\n\n" +
                $"Facing: Front — {monster.frontFacing.primaryZone} first. " +
                $"Flank — {monster.flankFacing.primaryZone} first. " +
                $"Rear — {monster.rearFacing.primaryZone} first.\n\n" +
                $"Weaknesses: {string.Join(", ", monster.weaknesses?.Select(e => e.ToString()) ?? new[] { "None" })}\n" +
                $"Resistances: {string.Join(", ", monster.resistances?.Select(e => e.ToString()) ?? new[] { "None" })}";
        }

        // ── Artifacts Tab ─────────────────────────────────────────
        private void BuildArtifactsTab()
        {
            _list.Clear();
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null) return;

            if (_campaignSO == null) return;

            // All artifacts are defined in ArtifactSO assets (loaded via Resources)
            // For now, show unlocked artifact IDs from CampaignState
            if (state.unlockedArtifactIds.Length == 0)
            {
                var none = new Label("No artifacts discovered yet.");
                none.AddToClassList("chronicle-entry");
                _list.Add(none);
                return;
            }

            foreach (var artifactId in state.unlockedArtifactIds)
            {
                var artifact = Resources.Load<ArtifactSO>($"Data/Artifacts/{artifactId}");
                var entry    = new VisualElement();
                entry.AddToClassList("codex-entry");

                var nameLabel = new Label(artifact != null ? artifact.artifactName : artifactId);
                nameLabel.AddToClassList("codex-entry-name");
                entry.Add(nameLabel);

                if (artifact != null)
                {
                    var artRef = artifact;
                    entry.RegisterCallback<ClickEvent>(_ => ShowArtifactDetail(artRef));
                }

                _list.Add(entry);
            }
        }

        private void ShowArtifactDetail(ArtifactSO artifact)
        {
            _detailTitle.text = artifact.artifactName;
            _detailMeta.text  = artifact.codexCategory.ToString();
            _detailBody.text  = artifact.loreText;
        }

        // ── Settlement Records Tab ────────────────────────────────
        private void BuildRecordsTab()
        {
            _list.Clear();
            var state = GameStateManager.Instance?.CampaignState;
            if (state == null || state.chronicleLog == null) return;

            foreach (var entry in state.chronicleLog)
            {
                // Year dividers (entries starting with "---")
                if (entry.StartsWith("---"))
                {
                    var divider = new Label(entry.Replace("---", "").Trim());
                    divider.AddToClassList("chronicle-divider");
                    _list.Add(divider);
                }
                else
                {
                    var logEntry = new Label(entry);
                    logEntry.AddToClassList("chronicle-entry");
                    _list.Add(logEntry);
                }
            }

            // Records tab: detail panel shows full campaign summary
            _detailTitle.text = "Settlement Record";
            _detailMeta.text  = $"Year {state.currentYear} — {state.characters.Length} active settlers";
            _detailBody.text  =
                $"This is the record of our settlement.\n\n" +
                $"Active settlers: {state.characters.Length}\n" +
                $"Retired: {state.retiredCharacters?.Length ?? 0}\n" +
                $"Crafters built: {state.builtCrafterNames.Length}\n" +
                $"Innovations adopted: {state.adoptedInnovationIds.Length}\n" +
                $"Artifacts discovered: {state.unlockedArtifactIds.Length}\n" +
                $"Events witnessed: {state.resolvedEventIds.Length}";
        }

        private void ClearDetail()
        {
            _detailTitle.text = "Select an entry";
            _detailMeta.text  = "";
            _detailBody.text  = "";
        }

        private void OnClose()
        {
            // Codex was loaded additively — unload this scene
            SceneManager.UnloadSceneAsync("Codex");
        }
    }
}
```

Update `SettlementScreenController.OnCodexClicked()`:

```csharp
private void OnCodexClicked()
{
    SceneManager.LoadScene("Codex", LoadSceneMode.Additive);
}
```

---

## Verification Test

1. From Settlement → click CODEX button
2. Codex screen loads over Settlement (additive)
3. Monsters tab shows all monsters from CampaignSO.monsterRoster
4. Monsters not in chronicle log show "???" and are dimmed
5. Clicking an unlocked monster shows name, animal basis, body parts, facing info
6. Artifacts tab shows "No artifacts discovered yet" (expected early game)
7. Settlement Records tab shows all chronicle log entries
8. Divider entries (--- Year N begins ---) show with gold styling
9. Detail panel shows campaign summary on Records tab
10. Close button unloads Codex scene, returning to Settlement

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_H.md`  
**Covers:** Combat return flow, victory/defeat modals, end-to-end full year loop verification — Stage 6 complete
