using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace MnM.Core.UI
{
    /// <summary>
    /// Single-row layout per part — fits all parts without scrolling:
    ///   [NAME    ] [S ###-- 3/5] [F ##--- 2/5]
    /// Bar is capped at 5 chars (scaled when max > 5).
    /// </summary>
    public class PartHealthBar
    {
        private readonly string _realName;
        private readonly int    _maxShell;
        private readonly int    _maxFlesh;

        private readonly Label _partNameLabel;
        private readonly Label _shellLabel;
        private readonly Label _fleshLabel;

        public static readonly Color ColShell   = new Color(0.72f, 0.52f, 0.04f);
        public static readonly Color ColFlesh   = new Color(0.55f, 0.18f, 0.18f);
        private static readonly Color ColNameOn  = new Color(0.83f, 0.80f, 0.73f);
        private static readonly Color ColNameOff = new Color(0.72f, 0.20f, 0.20f);

        private const int NameWidth  = 68;
        private const int BarCap     = 5;

        public PartHealthBar(VisualElement container, string partName, int maxShell, int maxFlesh)
        {
            _realName = partName;
            _maxShell = maxShell;
            _maxFlesh = maxFlesh;

            // Single flex-row — all content on one line
            container.style.flexDirection  = FlexDirection.Row;
            container.style.alignItems     = Align.Center;
            container.style.paddingLeft    = 4;
            container.style.paddingRight   = 4;
            container.style.paddingTop     = 3;
            container.style.paddingBottom  = 3;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = new StyleColor(new Color(0.20f, 0.17f, 0.13f));

            // Part name — fixed width
            _partNameLabel = new Label();
            _partNameLabel.style.color    = new StyleColor(ColNameOn);
            _partNameLabel.style.fontSize = 9;
            _partNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _partNameLabel.style.width    = NameWidth;
            _partNameLabel.style.flexShrink = 0;
            container.Add(_partNameLabel);

            // Shell — grows to fill half remaining width
            _shellLabel = new Label();
            _shellLabel.style.color    = new StyleColor(ColShell);
            _shellLabel.style.fontSize = 9;
            _shellLabel.style.flexGrow = 1;
            container.Add(_shellLabel);

            // Flesh — grows to fill other half
            _fleshLabel = new Label();
            _fleshLabel.style.color    = new StyleColor(ColFlesh);
            _fleshLabel.style.fontSize = 9;
            _fleshLabel.style.flexGrow = 1;
            container.Add(_fleshLabel);
        }

        public void SetValues(int currentShell, int currentFlesh,
                              bool isRevealed = true, bool isExposed = false, bool isBroken = false)
        {
            string name = isRevealed ? _realName.ToUpper() : "???";
            if (isExposed && isRevealed) name += " EXP";
            if (isBroken  && isRevealed) name += " BRK";
            _partNameLabel.text = name;
            _partNameLabel.style.color = (isBroken && isRevealed)
                ? new StyleColor(ColNameOff)
                : new StyleColor(ColNameOn);

            if (isRevealed)
            {
                _shellLabel.text = $"S {Bar(currentShell, _maxShell)} {currentShell}/{_maxShell}";
                _fleshLabel.text = $"F {Bar(currentFlesh, _maxFlesh)} {currentFlesh}/{_maxFlesh}";
            }
            else
            {
                _shellLabel.text = "S ???";
                _fleshLabel.text = "F ???";
            }
        }

        private static string Bar(int current, int max)
        {
            int display = Mathf.Min(max, BarCap);
            int filled  = max <= BarCap
                ? Mathf.Clamp(current, 0, max)
                : Mathf.RoundToInt((float)current / max * BarCap);

            var sb = new StringBuilder(display);
            for (int i = 0; i < display; i++)
                sb.Append(i < filled ? '#' : '-');
            return sb.ToString();
        }
    }
}
