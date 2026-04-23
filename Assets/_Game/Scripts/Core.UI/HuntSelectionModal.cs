using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using MnM.Core.Systems;
using MnM.Core.Data;

namespace MnM.Core.UI
{
    public class HuntSelectionModal
    {
        private VisualElement    _overlay;
        private CampaignSO       _campaignSO;
        private MonsterSO        _selectedMonster;
        private string           _selectedDifficulty = "Standard";
        private List<string>     _selectedHunterIds  = new List<string>();

        public void Show(VisualElement root, VisualTreeAsset modalAsset, CampaignSO campaignSO)
        {
            _campaignSO         = campaignSO;
            _selectedHunterIds  = new List<string>();
            _selectedDifficulty = "Standard";
            _selectedMonster    = null;

            _overlay = modalAsset.Instantiate();
            _overlay.style.position = Position.Absolute;
            _overlay.style.left     = 0;
            _overlay.style.top      = 0;
            _overlay.style.right    = 0;
            _overlay.style.bottom   = 0;
            root.Add(_overlay);

            BuildMonsterList();
            BuildHunterList();
            WireDifficultyButtons();

            _overlay.Q<Button>("btn-cancel-hunt").clicked  += () => root.Remove(_overlay);
            _overlay.Q<Button>("btn-confirm-hunt").clicked += () => OnConfirm(root);
        }

        private void BuildMonsterList()
        {
            var scroll = _overlay.Q<ScrollView>("monster-list");
            if (scroll == null || _campaignSO?.monsterRoster == null) return;
            var content = scroll.contentContainer;
            content.Clear();

            var campaignState = GameStateManager.Instance.CampaignState;
            foreach (var monster in _campaignSO.monsterRoster)
            {
                if (monster == null) continue;
                // The Spite is hidden until EVT-21 sets the unlock flag
                if (monster.monsterName == "The Spite" &&
                    !campaignState.unlockedCodexEntryIds.Contains("TheSpite_Unlocked"))
                    continue;

                var row = new VisualElement();
                row.AddToClassList("character-row");
                row.AddToClassList("stone-panel");

                var nameLabel = new Label(monster.monsterName);
                nameLabel.AddToClassList("character-name");
                row.Add(nameLabel);

                var tierLabel = new Label($"Tier {monster.materialTier}");
                tierLabel.AddToClassList("proficiency-label");
                row.Add(tierLabel);

                var monsterRef = monster;
                row.RegisterCallback<PointerUpEvent>(_ =>
                {
                    _selectedMonster = monsterRef;
                    RefreshMonsterSelection(content, monsterRef.monsterName);
                    Debug.Log($"[HuntSelect] Monster selected: {monsterRef.monsterName}");
                });
                content.Add(row);
            }

            // Auto-select first
            if (_campaignSO.monsterRoster.Length > 0)
            {
                _selectedMonster = _campaignSO.monsterRoster[0];
                if (_selectedMonster != null)
                    RefreshMonsterSelection(content, _selectedMonster.monsterName);
            }
        }

        private void RefreshMonsterSelection(VisualElement content, string selectedName)
        {
            foreach (var child in content.Children())
            {
                bool isSelected = child.Q<Label>()?.text == selectedName;
                child.EnableInClassList("stone-panel--active", isSelected);
            }
        }

        private void BuildHunterList()
        {
            var scroll = _overlay.Q<ScrollView>("hunter-select-list");
            if (scroll == null) return;
            var content = scroll.contentContainer;
            content.Clear();

            var state = GameStateManager.Instance.CampaignState;
            foreach (var ch in state.characters.Where(c => !c.isRetired))
            {
                // Column layout: name on top, weapon type below — readable at any width
                var row = new VisualElement();
                row.AddToClassList("stone-panel");
                row.style.flexDirection = FlexDirection.Column;
                row.style.paddingTop    = 6;
                row.style.paddingBottom = 6;
                row.style.paddingLeft   = 8;
                row.style.paddingRight  = 8;
                row.style.marginBottom  = 2;

                var nameLabel = new Label(ch.characterName);
                nameLabel.AddToClassList("character-name");
                row.Add(nameLabel);

                var weaponType = (ch.proficiencyWeaponTypes?.Length > 0) ? ch.proficiencyWeaponTypes[0] : "?";
                var tier       = (ch.proficiencyTiers?.Length > 0)        ? ch.proficiencyTiers[0]       : 0;
                var profLabel  = new Label($"Tier {tier}  ·  {weaponType}");
                profLabel.AddToClassList("proficiency-label");
                row.Add(profLabel);

                string capturedId = ch.characterId;
                row.RegisterCallback<PointerUpEvent>(_ => ToggleHunter(capturedId, row));
                content.Add(row);
            }

            // Auto-select first 4
            var first4 = state.characters.Where(c => !c.isRetired).Take(4);
            foreach (var ch in first4)
                _selectedHunterIds.Add(ch.characterId);

            RefreshHunterSelection(content);
        }

        private void ToggleHunter(string hunterId, VisualElement row)
        {
            if (_selectedHunterIds.Contains(hunterId))
            {
                _selectedHunterIds.Remove(hunterId);
                row.EnableInClassList("stone-panel--active", false);
            }
            else if (_selectedHunterIds.Count < 4)
            {
                _selectedHunterIds.Add(hunterId);
                row.EnableInClassList("stone-panel--active", true);
            }
            else
            {
                Debug.Log("[HuntSelect] Max 4 hunters already selected");
            }
        }

        private void RefreshHunterSelection(VisualElement content)
        {
            var state = GameStateManager.Instance.CampaignState;
            foreach (var child in content.Children())
            {
                var label = child.Q<Label>();
                var ch    = System.Array.Find(state.characters, c => c.characterName == label?.text);
                bool sel  = ch != null && _selectedHunterIds.Contains(ch.characterId);
                child.EnableInClassList("stone-panel--active", sel);
            }
        }

        private void WireDifficultyButtons()
        {
            var pairs = new[] {
                ("btn-diff-standard", "Standard"),
                ("btn-diff-hardened", "Hardened"),
                ("btn-diff-apex",     "Apex")
            };

            foreach (var (btnName, diff) in pairs)
            {
                string capturedDiff    = diff;
                string capturedBtnName = btnName;
                _overlay.Q<Button>(btnName).clicked += () =>
                {
                    _selectedDifficulty = capturedDiff;
                    foreach (var (b, _) in pairs)
                        _overlay.Q<Button>(b)?.EnableInClassList("tab-btn--active", b == capturedBtnName);
                    Debug.Log($"[HuntSelect] Difficulty: {capturedDiff}");
                };
            }
        }

        private void OnConfirm(VisualElement root)
        {
            if (_selectedMonster == null)
            {
                Debug.LogWarning("[HuntSelect] No monster selected");
                return;
            }
            if (_selectedHunterIds.Count == 0)
            {
                Debug.LogWarning("[HuntSelect] No hunters selected");
                return;
            }

            var state   = GameStateManager.Instance.CampaignState;
            var hunters = state.characters
                .Where(c => _selectedHunterIds.Contains(c.characterId))
                .ToArray();

            Debug.Log($"[HuntSelect] Confirming hunt: {_selectedMonster.monsterName} " +
                      $"({_selectedDifficulty}) with {hunters.Length} hunters");

            root.Remove(_overlay);
            GameStateManager.Instance.PrepareHunt(_selectedMonster, _selectedDifficulty, hunters);
        }
    }
}
