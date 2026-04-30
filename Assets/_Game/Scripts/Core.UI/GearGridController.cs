using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Logic;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class GearGridController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _root;
        private VisualElement _grid3x3;
        private VisualElement[,] _cells = new VisualElement[3, 3];

        // Current character being managed
        private RuntimeCharacterState _character;

        // Item placement state
        private string _selectedItemName = null;
        private int    _selectedCellX    = -1;
        private int    _selectedCellY    = -1;

        // Grid contents: which item name is in each cell (null = empty)
        // Stored as [x + y*3] — 9 slots total
        private string[] _gridContents = new string[9];

        // ── Bootstrapper ─────────────────────────────────────────
        private void Start()
        {
            var gsm = GameStateManager.Instance;
            if (gsm == null || gsm.CampaignState == null)
            {
                Debug.LogError("[GearGrid] No GameStateManager or CampaignState — cannot open");
                return;
            }

            string charId = gsm.PendingGearGridCharacterId;
            var ch = System.Array.Find(gsm.CampaignState.characters, c => c.characterId == charId);
            if (ch == null)
            {
                Debug.LogError($"[GearGrid] Character not found: {charId}");
                return;
            }

            Open(ch);
        }

        // ── Open / Init ──────────────────────────────────────────
        public void Open(RuntimeCharacterState character)
        {
            _character = character;
            _root = _uiDocument.rootVisualElement;
            BuildGrid();
            WireButtons();
            LoadEquippedItems();
            RefreshAll();
        }

        private void WireButtons()
        {
            _root.Q<Button>("btn-close").clicked   += OnClose;
            _root.Q<Button>("btn-unequip").clicked += OnUnequip;

            _root.Q<Label>("hunter-name-header").text = _character.characterName;
            _root.Q<Label>("portrait-name").text      = _character.characterName;
            _root.Q<Label>("portrait-build").text     = $"{_character.sex} · {_character.bodyBuild}";
        }

        // ── Grid Build ───────────────────────────────────────────
        private void BuildGrid()
        {
            _grid3x3 = _root.Q<VisualElement>("gear-grid-3x3");
            if (_grid3x3 == null) return;
            _grid3x3.Clear();

            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                var cell = new VisualElement();
                cell.AddToClassList("gear-cell");

                int cx = x, cy = y;
                cell.RegisterCallback<PointerUpEvent>(_ => OnCellClicked(cx, cy));

                _cells[x, y] = cell;
                _grid3x3.Add(cell);
            }
        }

        private void LoadEquippedItems()
        {
            _gridContents = new string[9];
            if (_character.equippedItemNames == null) return;

            for (int i = 0; i < _character.equippedItemNames.Length && i < 9; i++)
                _gridContents[i] = _character.equippedItemNames[i];


        }

        // ── Cell Click ───────────────────────────────────────────
        private void OnCellClicked(int x, int y)
        {
            int idx = x + y * 3;

            if (_selectedItemName != null)
            {
                // Place selected item into this cell
                if (_selectedCellX >= 0)
                {
                    int oldIdx = _selectedCellX + _selectedCellY * 3;
                    _gridContents[oldIdx] = null;
                }
                _gridContents[idx] = _selectedItemName;
                _selectedItemName  = null;
                _selectedCellX     = -1;
                _selectedCellY     = -1;
                SaveEquippedItems();
                RefreshAll();
            }
            else if (_gridContents[idx] != null)
            {
                // Pick up item from this cell
                _selectedItemName = _gridContents[idx];
                _selectedCellX    = x;
                _selectedCellY    = y;
                ShowItemDetail(_selectedItemName);
                RefreshGridVisuals();
            }
            else
            {
                // Empty cell with no selection — deselect
                _selectedItemName = null;
                _selectedCellX    = -1;
                _selectedCellY    = -1;
                RefreshGridVisuals();
            }
        }

        private void OnUnequip()
        {
            if (_selectedCellX < 0) return;
            int idx = _selectedCellX + _selectedCellY * 3;
            _gridContents[idx] = null;
            _selectedItemName  = null;
            _selectedCellX     = -1;
            _selectedCellY     = -1;
            SaveEquippedItems();
            RefreshAll();
        }

        // ── Visuals ──────────────────────────────────────────────
        private void RefreshAll()
        {
            RefreshGridVisuals();
            RefreshStatsSummary();
        }

        private void RefreshGridVisuals()
        {
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                var cell = _cells[x, y];
                if (cell == null) continue;
                cell.Clear();

                int  idx        = x + y * 3;
                bool hasItem    = !string.IsNullOrEmpty(_gridContents[idx]);
                bool isSelected = _selectedCellX == x && _selectedCellY == y;

                cell.EnableInClassList("gear-cell--occupied", hasItem);
                cell.EnableInClassList("gear-cell--selected",  isSelected);

                if (hasItem)
                {
                    var item = LoadItemSO(_gridContents[idx]);
                    cell.EnableInClassList("gear-cell--consumable",
                        item != null && item.isConsumable);

                    var lbl = new Label(_gridContents[idx]);
                    lbl.AddToClassList("gear-cell-label");
                    cell.Add(lbl);
                }
                else
                {
                    cell.EnableInClassList("gear-cell--consumable", false);
                }
            }
        }

        private void ShowItemDetail(string itemName)
        {
            var item = LoadItemSO(itemName);

            _root.Q<Label>("item-name").text = item != null ? item.itemName : itemName;
            _root.Q<Label>("item-tier").text = item != null ? $"Tier {item.materialTier}" : "";

            if (item != null)
            {
                var statParts = new List<string>();
                if (item.accuracyMod  != 0) statParts.Add($"ACC {item.accuracyMod:+0;-0}");
                if (item.strengthMod  != 0) statParts.Add($"STR {item.strengthMod:+0;-0}");
                if (item.evasionMod   != 0) statParts.Add($"EVA {item.evasionMod:+0;-0}");
                if (item.toughnessMod != 0) statParts.Add($"TGH {item.toughnessMod:+0;-0}");
                if (item.luckMod      != 0) statParts.Add($"LCK {item.luckMod:+0;-0}");
                if (item.movementMod  != 0) statParts.Add($"MOV {item.movementMod:+0;-0}");
                _root.Q<Label>("item-stats").text   = string.Join("  ", statParts);
                _root.Q<Label>("item-special").text = item.specialEffect ?? "";
                _root.Q<Label>("set-name").text     = string.IsNullOrEmpty(item.setNameTag)
                    ? "No set" : item.setNameTag;
            }
        }

        private GearGridSlot[] BuildLoadout()
        {
            var slots = new System.Collections.Generic.List<GearGridSlot>();
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                int idx = x + y * 3;
                if (string.IsNullOrEmpty(_gridContents[idx])) continue;
                var so = LoadItemSO(_gridContents[idx]);
                if (so == null) continue;
                slots.Add(new GearGridSlot { item = so, cell = new UnityEngine.Vector2Int(x, y) });
            }
            return slots.ToArray();
        }

        private void RefreshStatsSummary()
        {
            var loadout   = BuildLoadout();
            var gearStats = GearLinkResolver.SumEquippedStats(loadout);
            var links     = GearLinkResolver.ResolveLinks(loadout);

            _root.Q<Label>("stat-accuracy").text  = $"ACC {_character.accuracy}+{gearStats.accuracy}";
            _root.Q<Label>("stat-strength").text  = $"STR {_character.strength}+{gearStats.strength}";
            _root.Q<Label>("stat-evasion").text   = $"EVA {_character.evasion}+{gearStats.evasion}";
            _root.Q<Label>("stat-toughness").text = $"TGH {_character.toughness}+{gearStats.toughness}";
            _root.Q<Label>("stat-luck").text      = $"LCK {_character.luck}+{gearStats.luck}";
            _root.Q<Label>("stat-movement").text  = $"MOV {_character.movement}+{gearStats.movement}";

            _root.Q<Label>("active-links").text = links.Length > 0
                ? $"{links.Length} link{(links.Length == 1 ? "" : "s")} active"
                : "";
        }

        private void SaveEquippedItems()
        {
            _character.equippedItemNames = _gridContents
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
        }

        private void OnClose()
        {
            MnM.Core.Systems.SceneTransitionManager.Instance.LoadScene("Settlement");
        }

        // Resources.Load fallback — Stage 7 wires a proper registry
        private ItemSO LoadItemSO(string itemName) =>
            Resources.Load<ItemSO>($"Data/Items/{itemName}");
    }
}
