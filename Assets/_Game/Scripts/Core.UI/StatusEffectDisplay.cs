using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    /// <summary>
    /// Manages the visual status effect icon strip for one hunter token.
    /// Attach multiple instances to CombatScreenController's GameObject — one per hunter.
    /// Call Initialise() from CombatScreenController.Start() after combat state is ready.
    /// </summary>
    public class StatusEffectDisplay : MonoBehaviour
    {
        // effectName → remaining visual rounds (-1 = permanent display)
        private readonly Dictionary<string, int> _activeEffects = new();

        private VisualElement  _iconStrip;
        private string         _entityId;
        private CombatManager  _combatManager;

        // Shared across all instances — sprites are immutable after load
        private static readonly Dictionary<string, Sprite> _iconCache = new();

        private static readonly string[] KnownEffects =
            { "Shaken", "Pinned", "Slowed", "Exposed", "Bleeding", "Marked", "Broken", "Inspired" };

        // ── Setup ────────────────────────────────────────────────

        public void Initialise(VisualElement iconStrip, string entityId, CombatManager combatManager)
        {
            _iconStrip     = iconStrip;
            _entityId      = entityId;
            _combatManager = combatManager;

            _iconStrip.style.flexDirection = FlexDirection.Row;

            _combatManager.OnEffectApplied += HandleEffectApplied;
            _combatManager.OnEffectRemoved += HandleEffectRemoved;
            _combatManager.OnPhaseChanged  += HandlePhaseChanged;

            PreloadIcons();
            Refresh();
            Debug.Log($"[StatusDisplay] Initialised for {entityId}");
        }

        private void OnDisable()
        {
            if (_combatManager == null) return;
            _combatManager.OnEffectApplied -= HandleEffectApplied;
            _combatManager.OnEffectRemoved -= HandleEffectRemoved;
            _combatManager.OnPhaseChanged  -= HandlePhaseChanged;
        }

        // ── Event Handlers ────────────────────────────────────────

        private void HandleEffectApplied(string entityId, string effectName, int duration)
        {
            if (entityId != _entityId) return;
            _activeEffects[effectName] = duration;
            Refresh();
        }

        private void HandleEffectRemoved(string entityId, string effectName)
        {
            if (entityId != _entityId) return;
            _activeEffects.Remove(effectName);
            Refresh();
        }

        private void HandlePhaseChanged(CombatPhase phase)
        {
            if (phase == CombatPhase.VitalityPhase)
                TickDurations();
        }

        // ── Public API (for testing or manual overrides) ──────────

        public bool HasEffect(string effectName) => _activeEffects.ContainsKey(effectName);

        public void ForceApply(string effectName, int duration)
        {
            _activeEffects[effectName] = duration;
            Refresh();
        }

        public void ForceRemove(string effectName)
        {
            _activeEffects.Remove(effectName);
            Refresh();
        }

        // ── Duration Tick ─────────────────────────────────────────

        private void TickDurations()
        {
            var toRemove = new List<string>();
            foreach (var key in new List<string>(_activeEffects.Keys))
            {
                if (_activeEffects[key] == -1) continue; // permanent visual
                _activeEffects[key]--;
                if (_activeEffects[key] <= 0)
                    toRemove.Add(key);
            }
            foreach (var key in toRemove)
                _activeEffects.Remove(key);

            Refresh();
        }

        // ── Rendering ─────────────────────────────────────────────

        private void Refresh()
        {
            if (_iconStrip == null) return;
            _iconStrip.Clear();

            foreach (var kvp in _activeEffects)
            {
                var container = new VisualElement();
                container.style.width       = 18;
                container.style.height      = 18;
                container.style.marginRight = 2;
                container.style.position    = Position.Relative;

                var icon = new VisualElement();
                icon.style.width  = 16;
                icon.style.height = 16;
                if (_iconCache.TryGetValue(kvp.Key.ToLower(), out var sprite))
                    icon.style.backgroundImage = new StyleBackground(sprite);
                container.Add(icon);

                // Duration counter — bottom-right overlay, hidden for permanent effects
                if (kvp.Value > 0)
                {
                    var dur = new Label(kvp.Value.ToString());
                    dur.style.position        = Position.Absolute;
                    dur.style.right           = 0;
                    dur.style.bottom          = 0;
                    dur.style.fontSize        = 7;
                    dur.style.color           = Color.white;
                    dur.style.backgroundColor = new Color(0f, 0f, 0f, 0.65f);
                    container.Add(dur);
                }

                _iconStrip.Add(container);
            }
        }

        // ── Resource Loading ──────────────────────────────────────

        private static void PreloadIcons()
        {
            foreach (string effect in KnownEffects)
            {
                string key  = effect.ToLower();
                if (_iconCache.ContainsKey(key)) continue;

                // Assets must be at: Assets/Resources/Art/Generated/UI/StatusIcons/status_{key}.png
                var sprite = Resources.Load<Sprite>($"Art/Generated/UI/StatusIcons/status_{key}");
                if (sprite != null)
                    _iconCache[key] = sprite;
                else
                    Debug.LogWarning($"[StatusDisplay] Icon not found: status_{key} " +
                                     $"(place PNG at Assets/Resources/Art/Generated/UI/StatusIcons/)");
            }
        }
    }
}
