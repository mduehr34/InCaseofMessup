using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class TravelScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument      _uiDocument;
        [SerializeField] private VisualTreeAsset _eventModalAsset;
        [SerializeField] private CampaignSO      _campaignSO;
        [Header("Art")]
        [SerializeField] private Sprite          _travelBg;

        private VisualElement  _root;
        private Queue<EventSO> _travelEvents = new Queue<EventSO>();
        private int            _totalEvents;

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            var gsm = GameStateManager.Instance;
            if (gsm?.SelectedMonster == null)
            {
                Debug.LogError("[Travel] No hunt prepared in GameStateManager");
                return;
            }

            // Travel music
            AudioManager.Instance?.SetMusicContext(AudioContext.HuntTravel);

            // Background art
            var pathVisual = _root.Q("path-visual");
            if (pathVisual != null && _travelBg != null)
                pathVisual.style.backgroundImage = new StyleBackground(_travelBg);

            // Header
            _root.Q<Label>("hunt-target-label").text =
                $"HUNTING: {gsm.SelectedMonster.monsterName.ToUpper()} ({gsm.SelectedDifficulty.ToUpper()})";

            // Hunter condition bar
            BuildHunterConditionBar(gsm.CampaignState, gsm.SelectedHunters);

            // Draw travel events
            DrawTravelEvents(gsm.CampaignState);

            // Wire Continue button
            _root.Q<Button>("btn-continue-hunt").clicked += OnContinueToHunt;

            // Show first event (or reveal Continue immediately if no travel events)
            ShowNextEvent();
        }

        // ── Travel Event Queue ────────────────────────────────────
        private void DrawTravelEvents(CampaignState state)
        {
            _travelEvents.Clear();

            if (_campaignSO?.eventPool == null)
            {
                Debug.LogWarning("[Travel] CampaignSO or eventPool is null — skipping travel events");
                return;
            }

            // Events are eligible if flagged isTravel, unseen, and within year range
            var eligible = _campaignSO.eventPool
                .Where(e => e != null
                    && e.isTravel
                    && !state.resolvedEventIds.Contains(e.eventId)
                    && state.currentYear >= e.yearRangeMin
                    && state.currentYear <= e.yearRangeMax)
                .OrderBy(_ => Random.value)
                .Take(3);

            foreach (var evt in eligible)
                _travelEvents.Enqueue(evt);

            _totalEvents = _travelEvents.Count;
            UpdateEventsRemaining();

            Debug.Log($"[Travel] {_totalEvents} travel event(s) queued for " +
                      $"Year {state.currentYear}");
        }

        private void ShowNextEvent()
        {
            UpdateEventsRemaining();

            if (_travelEvents.Count == 0)
            {
                // All events resolved — reveal Continue button
                _root.Q<Button>("btn-continue-hunt").style.display = DisplayStyle.Flex;
                Debug.Log("[Travel] All travel events resolved — Continue button shown");
                return;
            }

            var evt = _travelEvents.Dequeue();
            ShowEventModal(evt);
        }

        // ── Event Modal ───────────────────────────────────────────
        private void ShowEventModal(EventSO evt)
        {
            if (_eventModalAsset == null)
            {
                Debug.LogWarning("[Travel] Event modal UXML asset not assigned — skipping event");
                ShowNextEvent();
                return;
            }

            var overlay = _eventModalAsset.Instantiate();
            overlay.style.position = Position.Absolute;
            overlay.style.left     = 0;
            overlay.style.top      = 0;
            overlay.style.right    = 0;
            overlay.style.bottom   = 0;
            _root.Add(overlay);

            overlay.Q<Label>("event-id").text        = evt.eventId;
            overlay.Q<Label>("event-name").text      = evt.eventName;
            overlay.Q<Label>("event-narrative").text = evt.narrativeText;

            bool isMandatory = evt.isMandatory || evt.choices == null || evt.choices.Length == 0;
            overlay.Q<Label>("event-mandatory").style.display =
                isMandatory ? DisplayStyle.Flex : DisplayStyle.None;

            var choicesEl = overlay.Q<VisualElement>("event-choices");
            var ackBtn    = overlay.Q<Button>("btn-acknowledge");

            if (isMandatory)
            {
                choicesEl.style.display = DisplayStyle.None;
                ackBtn.style.display    = DisplayStyle.Flex;
                ackBtn.clicked += () =>
                {
                    ApplyTravelEventEffect(evt, -1);
                    _root.Remove(overlay);
                    ShowNextEvent();
                };
            }
            else
            {
                choicesEl.style.display = DisplayStyle.Flex;
                ackBtn.style.display    = DisplayStyle.None;

                void Resolve(int choiceIdx)
                {
                    ApplyTravelEventEffect(evt, choiceIdx);
                    _root.Remove(overlay);
                    ShowNextEvent();
                }

                var btnA = overlay.Q<Button>("btn-choice-a");
                var btnB = overlay.Q<Button>("btn-choice-b");

                if (evt.choices.Length > 0)
                {
                    btnA.text = $"A: {evt.choices[0].outcomeText}";
                    btnA.clicked += () => Resolve(0);
                }

                if (evt.choices.Length > 1)
                {
                    btnB.text = $"B: {evt.choices[1].outcomeText}";
                    btnB.clicked += () => Resolve(1);
                }
                else
                {
                    btnB.style.display = DisplayStyle.None;
                }
            }

            Debug.Log($"[Travel] Showing event: {evt.eventId} — {evt.eventName}");
        }

        // ── Effect Application ────────────────────────────────────
        private void ApplyTravelEventEffect(EventSO evt, int choiceIndex)
        {
            // Mark event as resolved in campaign state
            var state    = GameStateManager.Instance.CampaignState;
            var resolved = new List<string>(state.resolvedEventIds) { evt.eventId };
            state.resolvedEventIds = resolved.ToArray();

            // Per .cursorrules: do NOT interpret mechanicalEffect strings — log for manual review
            bool hasChoice = evt.choices != null && choiceIndex >= 0 && choiceIndex < evt.choices.Length;
            if (hasChoice && !string.IsNullOrEmpty(evt.choices[choiceIndex].mechanicalEffect))
            {
                Debug.Log($"[Travel] Event effect (apply manually if needed): " +
                          $"{evt.choices[choiceIndex].mechanicalEffect}");
            }

            Debug.Log($"[Travel] Event resolved: {evt.eventId}");
        }

        // ── UI Helpers ────────────────────────────────────────────
        private void UpdateEventsRemaining()
        {
            var label = _root.Q<Label>("events-remaining");
            if (label == null) return;
            int remaining = _travelEvents.Count;
            label.text = remaining == 0
                ? "All events resolved"
                : $"{remaining} event{(remaining == 1 ? "" : "s")} remain";
        }

        private void BuildHunterConditionBar(CampaignState state, RuntimeCharacterState[] hunters)
        {
            var bar = _root.Q<VisualElement>("hunter-condition-bar");
            if (bar == null || hunters == null) return;
            bar.Clear();

            foreach (var h in hunters)
            {
                var strip = new VisualElement();
                strip.AddToClassList("character-row");
                strip.AddToClassList("stone-panel");
                strip.style.flexGrow = 1;

                var nameLabel = new Label(h.characterName);
                nameLabel.AddToClassList("character-name");
                strip.Add(nameLabel);

                if (h.injuryCardNames != null && h.injuryCardNames.Length > 0)
                {
                    var injLabel = new Label($"\u2691 {h.injuryCardNames.Length}");
                    injLabel.AddToClassList("injury-indicator");
                    strip.Add(injLabel);
                }

                bar.Add(strip);
            }
        }

        // ── Navigation ────────────────────────────────────────────
        private void OnContinueToHunt()
        {
            Debug.Log("[Travel] All travel events resolved — loading CombatScene");
            GameStateManager.Instance.BeginCombat();
        }
    }
}
