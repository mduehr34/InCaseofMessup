<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 6-A | GameStateManager & Scene Scaffold
Status: Stage 5 complete. Combat screen fully playable.
All Stage 5 verification passed.
Task: Create GameStateManager (singleton, DontDestroyOnLoad),
all Unity scene files, and the USS shared across all non-combat
screens. No screen controllers yet — infrastructure only.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_06/STAGE_06_A.md
- Assets/_Game/Scripts/Core.Systems/SaveManager.cs
- Assets/_Game/Scripts/Core.Systems/CampaignInitializer.cs
- Assets/_Game/Scripts/Core.Data/CampaignState.cs
- Assets/_Game/UI/USS/tokens.uss

Then confirm:
- The files you will create
- That GameStateManager uses DontDestroyOnLoad
- That scene names match exactly: MainMenu, Settlement,
  Travel, CombatScene (already exists)
- What you will NOT touch this session

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 6-A: GameStateManager & Scene Scaffold

**Resuming from:** Stage 5 complete — combat screen playable  
**Done when:** GameStateManager persists across scene loads; all 4 scenes exist in Build Settings; `settlement-shared.uss` created with no USS errors  
**Commit:** `"6A: GameStateManager singleton, scene scaffold, settlement-shared.uss"`  
**Next session:** STAGE_06_B.md  

---

## Step 1: GameStateManager.cs

**Path:** `Assets/_Game/Scripts/Core.Systems/GameStateManager.cs`

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using MnM.Core.Data;

namespace MnM.Core.Systems
{
    public class GameStateManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────
        public static GameStateManager Instance { get; private set; }

        // ── State ────────────────────────────────────────────────
        public CampaignState CampaignState { get; private set; }
        public CombatState   CombatState   { get; private set; }
        public HuntResult    LastHuntResult { get; private set; }

        // Hunt selection — stored between Settlement and Travel scenes
        public MonsterSO              SelectedMonster    { get; private set; }
        public string                 SelectedDifficulty { get; private set; }
        public RuntimeCharacterState[] SelectedHunters   { get; private set; }

        // ── Lifecycle ────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GSM] GameStateManager initialized — persisting across scenes");
        }

        // ── Campaign Lifecycle ────────────────────────────────────
        public void StartNewCampaign(CampaignSO campaignData)
        {
            CampaignState = CampaignInitializer.CreateNewCampaign(campaignData);
            Debug.Log($"[GSM] New campaign started: {campaignData.campaignName} " +
                      $"Year:{CampaignState.currentYear}");
            SceneManager.LoadScene("Settlement");
        }

        public void LoadCampaign(CampaignState state)
        {
            CampaignState = state;
            Debug.Log($"[GSM] Campaign loaded. Year:{state.currentYear}");
            SceneManager.LoadScene("Settlement");
        }

        // ── Hunt Flow ────────────────────────────────────────────
        public void PrepareHunt(
            MonsterSO monster,
            string difficulty,
            RuntimeCharacterState[] hunters)
        {
            SelectedMonster    = monster;
            SelectedDifficulty = difficulty;
            SelectedHunters    = hunters;

            // Build CombatState from current campaign + selected hunt parameters
            CombatState = CampaignInitializer.BuildCombatState(
                CampaignState, monster, difficulty, hunters);

            Debug.Log($"[GSM] Hunt prepared: {monster.monsterName} ({difficulty}) " +
                      $"with {hunters.Length} hunters");
            SceneManager.LoadScene("Travel");
        }

        public void BeginCombat()
        {
            // Travel scene calls this after travel events are resolved
            Debug.Log("[GSM] Beginning combat — loading CombatScene");
            SceneManager.LoadScene("CombatScene");
        }

        public void ReturnFromHunt(HuntResult result)
        {
            LastHuntResult = result;
            CampaignState.pendingHuntResult = result;
            SaveManager.Save(CampaignState);
            Debug.Log($"[GSM] Returning from hunt. Victory:{result.isVictory}");
            SceneManager.LoadScene("Settlement");
        }

        // ── Navigation Helpers ───────────────────────────────────
        public void GoToMainMenu()
        {
            // Save before leaving (if campaign active)
            if (CampaignState != null)
                SaveManager.Save(CampaignState);
            SceneManager.LoadScene("MainMenu");
        }

        public void OpenCodex()
        {
            // Codex can be opened from Settlement — loads additively
            SceneManager.LoadScene("Codex", LoadSceneMode.Additive);
        }

        // ── State Validation ─────────────────────────────────────
        public bool HasActiveCampaign => CampaignState != null;
        public bool HasSave           => SaveManager.HasSave();
    }
}
```

---

## Step 2: Scene Setup

Create these Unity scenes (File → New Scene → Empty, then save):

| Scene Name | Path | Purpose |
|---|---|---|
| `MainMenu` | `Assets/_Game/Scenes/MainMenu.unity` | Title screen |
| `Settlement` | `Assets/_Game/Scenes/Settlement.unity` | Between-hunt phase |
| `Travel` | `Assets/_Game/Scenes/Travel.unity` | Pre-combat travel events |
| `Codex` | `Assets/_Game/Scenes/Codex.unity` | Loaded additively |

`CombatScene` already exists from Stage 5.

**In each scene, add a `GameStateManagerBootstrap` GameObject:**

```csharp
// Assets/_Game/Scripts/Core.Systems/GameStateManagerBootstrap.cs
// Add this to the FIRST scene (MainMenu) only.
// GameStateManager persists from there.
using UnityEngine;
using MnM.Core.Systems;

public class GameStateManagerBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject _gameStateManagerPrefab;

    private void Awake()
    {
        if (GameStateManager.Instance == null)
        {
            if (_gameStateManagerPrefab != null)
                Instantiate(_gameStateManagerPrefab);
            else
            {
                var go = new GameObject("GameStateManager");
                go.AddComponent<GameStateManager>();
            }
        }
    }
}
```

**Add all scenes to Build Settings** (File → Build Settings → Add Open Scenes):
- `MainMenu` (index 0)
- `Settlement` (index 1)
- `Travel` (index 2)
- `CombatScene` (index 3)
- `Codex` (index 4)

---

## Step 3: settlement-shared.uss

Shared CSS used by Settlement, Main Menu, Codex, and Travel screens.

**Path:** `Assets/_Game/UI/USS/settlement-shared.uss`

```css
/* ============================================================
   Marrow & Myth — Settlement Shared Styles
   Used by: MainMenu, Settlement, Codex, Travel screens.
   Import after tokens.uss and stone-panel.uss.
   ============================================================ */

/* ── Era / Year Header Bar ──────────────────────────────────── */
.era-bar {
    height:          60px;
    flex-direction:  row;
    align-items:     center;
    padding-left:    var(--spacing-md);
    padding-right:   var(--spacing-md);
    flex-shrink:     0;
}

.era-year {
    font-size:        var(--font-size-xl);
    color:            var(--color-text-accent);
    -unity-font-style: bold;
    margin-right:     var(--spacing-sm);
}

.era-name {
    font-size:  var(--font-size-body);
    color:      var(--color-text-dim);
    flex:       1;
    -unity-font-style: italic;
}

.era-btn {
    background-color: transparent;
    border-color:     var(--color-border-accent);
    border-width:     1px;
    border-radius:    0px;
    color:            var(--color-text-dim);
    font-size:        var(--font-size-label);
    padding:          var(--spacing-xs) var(--spacing-sm);
}

/* ── Tab Bar ─────────────────────────────────────────────────── */
.tab-bar {
    flex-direction: row;
    border-bottom-color: var(--color-border);
    border-bottom-width: 1px;
    margin-bottom:  var(--spacing-sm);
}

.tab-btn {
    flex:             1;
    background-color: transparent;
    border-width:     0;
    border-radius:    0px;
    color:            var(--color-text-dim);
    font-size:        var(--font-size-label);
    padding:          var(--spacing-sm);
    border-bottom-color: transparent;
    border-bottom-width: 2px;
}

.tab-btn--active {
    color:               var(--color-text-accent);
    border-bottom-color: var(--color-text-accent);
    border-bottom-width: 2px;
    -unity-font-style:   bold;
}

.tab-btn:hover {
    color: var(--color-text-primary);
}

.tab-content {
    flex: 1;
    overflow: hidden;
}

/* ── Action Bar ──────────────────────────────────────────────── */
.action-bar {
    height:          80px;
    flex-direction:  row;
    align-items:     center;
    justify-content: center;
    padding:         0 var(--spacing-lg);
    flex-shrink:     0;
}

.action-bar .action-btn {
    margin: 0 var(--spacing-sm);
    min-width: 180px;
    height:    48px;
}

/* ── Modal Overlay ───────────────────────────────────────────── */
.modal-overlay {
    position:         absolute;
    left:             0;
    top:              0;
    right:            0;
    bottom:           0;
    background-color: rgba(0, 0, 0, 0.78);
    align-items:      center;
    justify-content:  center;
}

.modal-panel {
    width:      800px;
    max-height: 560px;
    flex-direction: column;
}

/* ── Character Row ───────────────────────────────────────────── */
.character-row {
    flex-direction: row;
    align-items:    center;
    margin-bottom:  var(--spacing-xs);
    padding:        var(--spacing-sm);
}

.character-name {
    font-size:        var(--font-size-body);
    color:            var(--color-text-primary);
    -unity-font-style: bold;
    flex:             1;
}

.injury-indicator {
    font-size:  var(--font-size-label);
    color:      var(--color-text-danger);
    margin-right: var(--spacing-sm);
}

.proficiency-label {
    font-size:  var(--font-size-small);
    color:      var(--color-text-dim);
    margin-right: var(--spacing-xs);
}

.small-btn {
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-accent);
    border-width:     1px;
    border-radius:    0px;
    color:            var(--color-text-dim);
    font-size:        var(--font-size-small);
    padding:          3px var(--spacing-sm);
}

/* ── Main Menu Specific ──────────────────────────────────────── */
.title-block {
    align-items:     center;
    justify-content: center;
    flex:            1;
}

.title-marrow {
    font-size:        72px;
    color:            var(--color-text-accent);
    -unity-font-style: bold;
}

.title-myth {
    font-size:        48px;
    color:            var(--color-text-primary);
    -unity-font-style: bold;
}

.title-subtitle {
    font-size:  var(--font-size-body);
    color:      var(--color-text-dim);
    margin-top: var(--spacing-sm);
    -unity-font-style: italic;
}

.menu-buttons {
    align-items: center;
    padding:     var(--spacing-lg);
    margin:      var(--spacing-lg) var(--spacing-xl);
}

.menu-btn {
    width:            320px;
    height:           52px;
    margin-bottom:    var(--spacing-sm);
    font-size:        var(--font-size-title);
    -unity-font-style: bold;
    background-color: var(--color-bg-panel-raised);
    border-color:     var(--color-border-accent);
    border-width:     2px;
    border-radius:    0px;
    color:            var(--color-text-primary);
}

.menu-btn--secondary {
    color:        var(--color-text-dim);
    border-color: var(--color-border);
    font-size:    var(--font-size-body);
    height:       40px;
}

.menu-btn:hover {
    border-color: var(--color-text-accent);
    color:        var(--color-text-accent);
}

.version-label {
    font-size:        var(--font-size-small);
    color:            var(--color-text-dim);
    position:         absolute;
    bottom:           var(--spacing-sm);
    right:            var(--spacing-sm);
}
```

---

## Verification Test

1. Confirm `GameStateManager.cs` compiles with no errors
2. Create a `GameStateManager` prefab from a GameObject with the component attached
3. Play the `MainMenu` scene — confirm `GameStateManager.Instance` is not null in Console (add a `Debug.Log(GameStateManager.Instance)` to any test script temporarily)
4. Confirm all 5 scenes appear in Build Settings
5. Open `settlement-shared.uss` in Project window — confirm no USS error icons
6. Confirm `GameStateManager.HasSave` returns false when no save exists

---

## Next Session

**File:** `_Docs/Stage_06/STAGE_06_B.md`  
**Covers:** Main menu UXML + controller, campaign select UXML + controller
