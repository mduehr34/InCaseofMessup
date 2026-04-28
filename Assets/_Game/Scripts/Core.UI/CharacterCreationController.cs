using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MnM.Core.Data;
using MnM.Core.Systems;

namespace MnM.Core.UI
{
    public class CharacterCreationController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private static readonly string[] MaleNames =
        {
            "Aldric","Beorn","Cyne","Drest","Edric","Finn",
            "Garm","Hrolf","Ivar","Kenric","Leif","Mord",
            "Orm","Rolf","Sigurd","Tor","Ulf","Wulf"
        };

        private static readonly string[] FemaleNames =
        {
            "Aelith","Bryn","Cyneth","Duna","Eira","Freya",
            "Gerd","Hild","Ingrid","Kara","Lena","Mira",
            "Nessa","Runa","Sigrid","Thora","Urd","Wynn"
        };

        private static readonly (string build, string sex)[] BuildSlots =
        {
            ("Aethel","M"), ("Beorn","M"), ("Cyne","M"), ("Duna","M"),
            ("Eira","F"),   ("Freya","F"), ("Gerd","F"), ("Hild","F")
        };

        private readonly List<HunterGenerationData> _hunters = new();

        private void OnEnable()
        {
            GenerateHunters();
            BuildUI();
        }

        private void GenerateHunters()
        {
            _hunters.Clear();
            var usedM = new HashSet<string>();
            var usedF = new HashSet<string>();
            var rng   = new System.Random();

            foreach (var (build, sex) in BuildSlots)
            {
                string name = PickUnique(
                    sex == "M" ? MaleNames : FemaleNames,
                    sex == "M" ? usedM    : usedF,
                    rng);

                _hunters.Add(new HunterGenerationData
                {
                    hunterName = name,
                    buildName  = build,
                    sex        = sex,
                    spritePath = $"Art/Generated/Characters/char_{build.ToLower()}_idle_s"
                });
            }

            Debug.Log($"[CharacterCreation] Generated {_hunters.Count} hunters");
        }

        private static string PickUnique(string[] pool, HashSet<string> used, System.Random rng)
        {
            var available = System.Array.FindAll(pool, n => !used.Contains(n));
            if (available.Length == 0) return "Hunter";
            string pick = available[rng.Next(available.Length)];
            used.Add(pick);
            return pick;
        }

        private void BuildUI()
        {
            var root = _uiDocument.rootVisualElement;
            var grid = root.Q("hunters-grid");
            grid.Clear();

            for (int i = 0; i < _hunters.Count; i++)
            {
                int idx = i;
                var h   = _hunters[i];

                var card = new VisualElement();
                card.AddToClassList("hunter-card");

                // Portrait placeholder
                var portrait = new VisualElement();
                portrait.AddToClassList("hunter-card__portrait");
                var sprite = Resources.Load<Sprite>(h.spritePath);
                if (sprite != null)
                    portrait.style.backgroundImage = new StyleBackground(sprite);
                card.Add(portrait);

                // Build label (sex shown in brackets)
                var buildLabel = new Label($"{h.buildName} ({h.sex})");
                buildLabel.AddToClassList("hunter-card__build-label");
                card.Add(buildLabel);

                // Name label — click to rename
                var nameLabel = new Label(h.hunterName);
                nameLabel.AddToClassList("hunter-card__name-label");
                nameLabel.name = $"name-label-{idx}";
                nameLabel.RegisterCallback<ClickEvent>(_ => StartRename(root, idx));
                card.Add(nameLabel);

                grid.Add(card);
            }

            root.Q<Button>("btn-back")
                .RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene("CampaignSelect"));
            root.Q<Button>("btn-confirm")
                .RegisterCallback<ClickEvent>(_ => OnConfirm());
        }

        private void StartRename(VisualElement root, int idx)
        {
            var label = root.Q<Label>($"name-label-{idx}");
            if (label == null) return;

            var field = new TextField { name = $"name-field-{idx}" };
            field.value = _hunters[idx].hunterName;
            field.AddToClassList("hunter-card__name-field");

            label.parent.Add(field);
            label.parent.Remove(label);

            field.RegisterCallback<FocusOutEvent>(_ => FinishRename(root, idx, field));
            field.Q(TextField.textInputUssName)?.Focus();
        }

        private void FinishRename(VisualElement root, int idx, TextField field)
        {
            string newName = field.value.Trim();
            if (string.IsNullOrEmpty(newName)) newName = _hunters[idx].hunterName;
            _hunters[idx].hunterName = newName;

            var nameLabel = new Label(newName);
            nameLabel.AddToClassList("hunter-card__name-label");
            nameLabel.name = $"name-label-{idx}";
            nameLabel.RegisterCallback<ClickEvent>(_ => StartRename(root, idx));

            field.parent.Add(nameLabel);
            field.parent.Remove(field);
        }

        private void OnConfirm()
        {
            Debug.Log("[CharacterCreation] Confirming hunter roster");

            if (GameStateManager.Instance == null)
            {
                Debug.LogError("[CharacterCreation] GameStateManager not found — " +
                               "enter play mode from MainMenu so GSM bootstraps.");
                return;
            }

            GameStateManager.Instance.StartNewCampaign(_hunters);
            SceneManager.LoadScene("Settlement");
        }
    }
}
