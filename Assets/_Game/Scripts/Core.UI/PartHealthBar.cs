using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Manages the Shell and Flesh progress bars for one monster body part.
    /// Build once per part; call SetValues() whenever HP changes — avoids DOM rebuilds.
    /// </summary>
    public class PartHealthBar
    {
        private readonly string        _realName;
        private readonly Label         _partNameLabel;
        private readonly VisualElement _shellFill;
        private readonly VisualElement _fleshFill;
        private readonly Label         _shellVal;
        private readonly Label         _fleshVal;
        private readonly Label         _exposedTag;
        private readonly Label         _brokenTag;

        private readonly int _maxShell;
        private readonly int _maxFlesh;

        // Shell gold (#B8860B), Flesh dried-blood (#4A2020)
        private static readonly Color _shellColor = new Color(0.72f, 0.52f, 0.04f);
        private static readonly Color _fleshColor = new Color(0.29f, 0.13f, 0.13f);
        private static readonly Color _nameNormal  = new Color(0.83f, 0.80f, 0.73f);
        private static readonly Color _nameBroken  = new Color(0.72f, 0.20f, 0.20f);

        public PartHealthBar(VisualElement container, string partName, int maxShell, int maxFlesh)
        {
            _realName = partName;
            _maxShell = maxShell;
            _maxFlesh = maxFlesh;

            _partNameLabel = new Label();
            _partNameLabel.style.color       = new StyleColor(_nameNormal);
            _partNameLabel.style.fontSize    = 9;
            _partNameLabel.style.marginBottom = 2;
            container.Add(_partNameLabel);

            var shellRow = BuildBarRow("SHELL", _shellColor, out _shellFill, out _shellVal);
            container.Add(shellRow);

            var fleshRow = BuildBarRow("FLESH", _fleshColor, out _fleshFill, out _fleshVal);
            container.Add(fleshRow);

            // Exposed / broken tags
            var tagsRow = new VisualElement();
            tagsRow.style.flexDirection = FlexDirection.Row;
            tagsRow.style.marginBottom  = 2;

            _exposedTag = new Label("EXPOSED");
            _exposedTag.AddToClassList("exposed-tag");
            _exposedTag.style.display = DisplayStyle.None;
            tagsRow.Add(_exposedTag);

            _brokenTag = new Label("BROKEN");
            _brokenTag.AddToClassList("status-badge");
            _brokenTag.style.display = DisplayStyle.None;
            tagsRow.Add(_brokenTag);

            container.Add(tagsRow);

            var spacer = new VisualElement();
            spacer.style.height = 4;
            container.Add(spacer);
        }

        public void SetValues(int currentShell, int currentFlesh,
                              bool isRevealed = true, bool isExposed = false, bool isBroken = false)
        {
            _partNameLabel.text = isRevealed ? _realName.ToUpper() : "???";
            _partNameLabel.style.color = (isBroken && isRevealed)
                ? new StyleColor(_nameBroken)
                : new StyleColor(_nameNormal);

            float shellPct = _maxShell > 0 ? (float)currentShell / _maxShell : 0f;
            float fleshPct = _maxFlesh > 0 ? (float)currentFlesh / _maxFlesh : 0f;

            _shellFill.style.width = new StyleLength(new Length(Mathf.Clamp01(shellPct) * 100f, LengthUnit.Percent));
            _fleshFill.style.width = new StyleLength(new Length(Mathf.Clamp01(fleshPct) * 100f, LengthUnit.Percent));

            _shellVal.text = isRevealed ? currentShell.ToString() : "?";
            _fleshVal.text = isRevealed ? currentFlesh.ToString() : "?";

            _exposedTag.style.display = (isExposed && isRevealed) ? DisplayStyle.Flex : DisplayStyle.None;
            _brokenTag.style.display  = (isBroken  && isRevealed) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static VisualElement BuildBarRow(string typeLabel, Color barColor,
                                                  out VisualElement fill, out Label valLabel)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems    = Align.Center;
            row.style.marginBottom  = 2;

            var lbl = new Label(typeLabel);
            lbl.style.color    = new Color(0.54f, 0.54f, 0.54f);
            lbl.style.fontSize = 7;
            lbl.style.width    = 30;
            row.Add(lbl);

            var track = new VisualElement();
            track.style.flexGrow        = 1;
            track.style.height          = 6;
            track.style.backgroundColor = new Color(0.12f, 0.10f, 0.08f);
            track.style.marginRight     = 4;

            fill = new VisualElement();
            fill.style.height          = 6;
            fill.style.backgroundColor = new StyleColor(barColor);
            fill.style.width           = new StyleLength(new Length(100f, LengthUnit.Percent));
            fill.style.transitionDuration = new List<TimeValue> { new TimeValue(0.15f, TimeUnit.Second) };
            fill.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("width") };
            track.Add(fill);
            row.Add(track);

            valLabel = new Label("?");
            valLabel.style.color    = new Color(0.83f, 0.80f, 0.73f);
            valLabel.style.fontSize = 8;
            valLabel.style.width    = 14;
            row.Add(valLabel);

            return row;
        }
    }
}
