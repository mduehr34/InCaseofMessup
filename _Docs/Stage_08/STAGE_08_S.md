<!-- ============================================================
     SESSION PROMPT — copy everything between the arrows and
     paste it into Claude.ai to start this session
     ============================================================

▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

Stage 8-M | Hunt Travel Scene — Full Implementation
Status: Stage 8-R complete. Full combat loop verified against
Aldric vs Gaunt Standard with new health system.
Task: Build the Hunt Travel scene completely. Show a dark
wilderness background. Display 0–3 random travel events as
card-style panels with a narrative text and a choice or
"continue" button. After all events, show the CONTINUE
TO HUNT button that loads CombatScene. Wire travel music.

Read these files before doing anything:
- .cursorrules
- claude.md
- _Docs/Stage_08/STAGE_08_S.md
- Assets/_Game/Scripts/Core.Data/EventSO.cs
- Assets/_Game/Scripts/Core.Systems/GameStateManager.cs
- Assets/_Game/Scripts/Core.Systems/AudioManager.cs

Then confirm:
- Travel events are drawn from the eventPool filtered by
  the tag "travel" (you may need to add this tag field to EventSO)
- Events with no travel tag are never shown in travel
- If 0 travel events fire, go straight to CONTINUE TO HUNT
- Player choices in travel events are logged and stored in
  CampaignState.resolvedEventIds
- What you will NOT build (the full event system is in
  SettlementScreenController — this scene just renders events
  using the same EventSO data)

▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
============================================================ -->

# Stage 8-S: Hunt Travel Scene — Full Implementation

**Resuming from:** Stage 8-R (combat verify gate) complete — full combat loop (new health model, deployment, terrain) verified end-to-end
**Done when:** Travel scene opens after SEND HUNTING PARTY, shows 0–3 travel events with choices, then CONTINUE TO HUNT loads CombatScene
**Commit:** `"8S: Hunt travel scene — travel events, choices, CONTINUE TO HUNT button"`
**Next session:** STAGE_08_T.md

---

## What You Are Building

When the player sends hunters on a hunt, they pass through a travel scene before reaching the monster. This scene:
1. Shows a dark atmospheric wilderness background
2. Randomly draws 0–3 events tagged as "travel" from the event pool
3. Displays each as a narrative card the player must respond to
4. Then shows a CONTINUE TO HUNT button

**New developer note:** Think of this like a "road encounter" — small moments between leaving the settlement and reaching the fight.

---

## Step 1: Generate Background Art

Use CoPlay `generate_or_edit_images`:

**Travel background (640×360):**
```
Dark pixel art wilderness scene. A narrow hunter's path through
dense dead forest. Silhouetted bare trees on both sides.
Distant fog. No settlement visible. Moon partially obscured
by clouds. Distant eyes in the dark (barely visible).
Atmospheric, tense, lonely. Style: 16-bit pixel art,
ash grey, bone white, shadow black palette.
```
Save to: `Assets/_Game/Art/Generated/UI/ui_travel_bg.png`
Import settings: Sprite (2D and UI), Point (No Filter), PPU 16

---

## Step 2: Add `isTravel` to EventSO

Open `Assets/_Game/Scripts/Core.Data/EventSO.cs` and add one field:

```csharp
[Header("Travel")]
public bool isTravel = false;  // If true, this event can fire during hunt travel
```

Then open the Inspector for any travel-appropriate events and set `isTravel = true`.

**Suggested travel events** (set isTravel = true on these):
- EVT-04 (Strange Sounds)
- EVT-08 (Bone Wind)
- EVT-15 (Hard Winter — choice B forces specific monster, still valid here)

You can also create 2–3 new travel-only EventSO assets:

| Asset | Name | Narrative | Choice A | Choice B |
|---|---|---|---|---|
| `Event_TRV01` | Tracks | "Fresh tracks in the mud. Large. Recent." | Follow them (+1 Accuracy first round) | Avoid them (no effect) |
| `Event_TRV02` | Old Cairn | "Someone built this. A warning, maybe. Or a grave." | Search it (gain 1 Bone) | Leave it (nothing) |
| `Event_TRV03` | Fog | "The fog came in fast. We split up briefly." | Call out to each other (regroup, no effect) | Stay quiet (all hunters start combat with Shaken) |

---

## Step 3: Create Scene & UIDocument

**File → New Scene → Empty** → save as `Assets/HuntTravel.unity`
Add to Build Settings after Settlement.
Add UIDocument GameObject named `TravelUI`.

---

## Step 4: UXML Layout

Create `Assets/_Game/UI/HuntTravel.uxml`:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" style="width:100%; height:100%; position:relative; background-color:#0A0A0C;">

    <!-- Background art -->
    <ui:VisualElement name="bg" style="position:absolute; left:0; top:0; right:0; bottom:0;
        -unity-background-scale-mode:scale-to-fit;" />

    <!-- Dark vignette overlay -->
    <ui:VisualElement name="vignette" style="position:absolute; left:0; top:0; right:0; bottom:0;
        background-color:rgba(0,0,0,0.45); pointer-events:none;" />

    <!-- Header -->
    <ui:VisualElement style="position:absolute; top:0; left:0; right:0; height:48px;
        background-color:rgba(10,10,12,0.8); align-items:center; justify-content:center;">
      <ui:Label name="hunt-target-label" text="HUNTING: THE GAUNT (STANDARD)"
                style="color:#D4CCBA; font-size:14px;" />
    </ui:VisualElement>

    <!-- Event card area (centre) -->
    <ui:VisualElement name="event-area" style="position:absolute; top:80px; bottom:120px;
        left:0; right:0; align-items:center; justify-content:center;" />

    <!-- Continue button (shown after all events resolved) -->
    <ui:VisualElement style="position:absolute; bottom:24px; left:0; right:0;
        align-items:center;">
      <ui:Button name="btn-continue-hunt" text="CONTINUE TO HUNT →"
                 style="width:280px; height:52px; background-color:#4A2020;
                        border-color:#B8860B; border-width:2px;
                        color:#D4CCBA; font-size:16px; display:none;" />
    </ui:VisualElement>

  </ui:VisualElement>
</ui:UXML>
```

---

## Step 5: TravelController.cs

**Path:** `Assets/_Game/Scripts/Core.UI/TravelController.cs`

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class TravelController : MonoBehaviour
    {
        [SerializeField] private UIDocument  _uiDocument;
        [SerializeField] private CampaignSO  _campaignData;

        private VisualElement _eventArea;
        private Button        _continueBtn;
        private List<EventSO> _travelQueue = new();
        private int           _currentEvent = 0;

        private void OnEnable()
        {
            var root     = _uiDocument.rootVisualElement;
            _eventArea   = root.Q("event-area");
            _continueBtn = root.Q<Button>("btn-continue-hunt");

            // Background
            var bg = Resources.Load<Sprite>("Art/Generated/UI/ui_travel_bg");
            if (bg != null) root.Q("bg").style.backgroundImage = new StyleBackground(bg);

            // Hunt target label
            var state = GameStateManager.Instance.CampaignState;
            string monsterName = state.currentHuntMonsterName ?? "THE HUNT";
            root.Q<Label>("hunt-target-label").text =
                $"HUNTING: {monsterName.ToUpper()} ({state.currentHuntDifficulty.ToUpper()})";

            // Build travel event queue
            BuildEventQueue();

            // Wire continue button
            _continueBtn.RegisterCallback<ClickEvent>(_ =>
                SceneTransitionManager.Instance.LoadScene("CombatScene"));

            // Switch to travel music
            AudioManager.Instance?.SetMusicContext(AudioContext.HuntTravel);

            // Start event sequence
            StartCoroutine(RunEventSequence());
        }

        private void BuildEventQueue()
        {
            _travelQueue.Clear();
            if (_campaignData?.eventPool == null) return;

            var rng       = new System.Random();
            var candidates = new List<EventSO>();

            foreach (var evt in _campaignData.eventPool)
            {
                if (evt == null || !evt.isTravel) continue;
                if (GameStateManager.Instance.CampaignState.resolvedEventIds != null &&
                    System.Array.IndexOf(
                        GameStateManager.Instance.CampaignState.resolvedEventIds,
                        evt.eventId) >= 0) continue;
                candidates.Add(evt);
            }

            // Draw 0–3 events at random
            int count = rng.Next(0, Mathf.Min(4, candidates.Count + 1));
            while (_travelQueue.Count < count && candidates.Count > 0)
            {
                int idx = rng.Next(candidates.Count);
                _travelQueue.Add(candidates[idx]);
                candidates.RemoveAt(idx);
            }

            Debug.Log($"[Travel] {_travelQueue.Count} travel events queued");
        }

        private IEnumerator RunEventSequence()
        {
            // Brief pause before first event
            yield return new WaitForSeconds(0.8f);

            for (int i = 0; i < _travelQueue.Count; i++)
            {
                yield return ShowEvent(_travelQueue[i]);
                yield return new WaitForSeconds(0.3f);
            }

            // All events done — show Continue button
            _continueBtn.style.display = DisplayStyle.Flex;
            Debug.Log("[Travel] All events resolved — CONTINUE TO HUNT available");
        }

        private IEnumerator ShowEvent(EventSO evt)
        {
            _eventArea.Clear();

            // Build event card
            var card = new VisualElement();
            card.style.width            = 480;
            card.style.backgroundColor  = new StyleColor(new Color(0.05f, 0.04f, 0.03f, 0.95f));
            card.style.borderTopColor   = card.style.borderBottomColor =
            card.style.borderLeftColor  = card.style.borderRightColor  =
                new StyleColor(new Color(0.72f, 0.52f, 0.04f));
            card.style.borderTopWidth   = card.style.borderBottomWidth =
            card.style.borderLeftWidth  = card.style.borderRightWidth  = 2;
            card.style.paddingTop       = card.style.paddingBottom =
            card.style.paddingLeft      = card.style.paddingRight  = 24;
            card.style.opacity          = 0;

            var title = new Label(evt.eventName.ToUpper());
            title.style.color    = new Color(0.72f, 0.52f, 0.04f);
            title.style.fontSize = 14;
            title.style.marginBottom = 12;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            card.Add(title);

            var narrative = new Label(evt.narrativeText);
            narrative.style.color       = new Color(0.83f, 0.80f, 0.73f);
            narrative.style.fontSize    = 11;
            narrative.style.whiteSpace  = WhiteSpace.Normal;
            narrative.style.marginBottom = 20;
            card.Add(narrative);

            bool resolved = false;

            if (evt.choices != null && evt.choices.Length > 0)
            {
                foreach (var choice in evt.choices)
                {
                    var btn = new Button();
                    btn.text = choice.choiceText;
                    btn.style.marginBottom   = 8;
                    btn.style.backgroundColor = new StyleColor(new Color(0.12f, 0.10f, 0.08f));
                    btn.style.borderTopColor  = btn.style.borderBottomColor =
                    btn.style.borderLeftColor = btn.style.borderRightColor  =
                        new StyleColor(new Color(0.31f, 0.27f, 0.20f));
                    btn.style.borderTopWidth  = btn.style.borderBottomWidth =
                    btn.style.borderLeftWidth = btn.style.borderRightWidth  = 1;
                    btn.style.color           = new StyleColor(new Color(0.83f, 0.80f, 0.73f));
                    btn.style.fontSize        = 10;
                    btn.style.whiteSpace      = WhiteSpace.Normal;
                    var capturedChoice = choice;
                    btn.RegisterCallback<ClickEvent>(_ =>
                    {
                        resolved = true;
                        GameStateManager.Instance.ResolveEventChoice(evt, capturedChoice);
                        Debug.Log($"[Travel] {evt.eventName} → {capturedChoice.choiceText}");
                    });
                    card.Add(btn);
                }
            }
            else
            {
                var ackBtn = new Button { text = "CONTINUE" };
                ackBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    resolved = true;
                    GameStateManager.Instance.ResolveEvent(evt);
                });
                card.Add(ackBtn);
            }

            _eventArea.Add(card);

            // Fade card in
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                card.style.opacity = Mathf.Lerp(0f, 1f, t / 0.3f);
                yield return null;
            }
            card.style.opacity = 1;

            // Wait for player to resolve
            yield return new WaitUntil(() => resolved);

            // Fade card out
            t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                card.style.opacity = Mathf.Lerp(1f, 0f, t / 0.2f);
                yield return null;
            }
            _eventArea.Clear();
        }
    }
}
```

---

## Verification Test

- [ ] SEND HUNTING PARTY in Settlement → HuntTravel scene loads (with fade)
- [ ] Background wilderness art fills the screen
- [ ] Hunt target label shows correct monster and difficulty
- [ ] With 0 travel events: CONTINUE TO HUNT button appears immediately
- [ ] With travel events: event card fades in, narrative text readable
- [ ] Clicking a choice fades the card out and shows next event
- [ ] After all events resolved: CONTINUE TO HUNT button appears
- [ ] CONTINUE TO HUNT → CombatScene loads with fade
- [ ] Hunt travel music plays during this scene
- [ ] No Console errors if event pool is empty

---

## Next Session

**File:** `_Docs/Stage_08/STAGE_08_T.md`
**Covers:** Tutorial tooltip and onboarding system — step-sequenced highlight overlays that guide new players through their first settlement and first combat
