using UnityEditor;
using UnityEngine;

public class StatusIconPreviewWindow : EditorWindow
{
    private static readonly string[] Effects =
        { "shaken", "pinned", "slowed", "exposed", "bleeding", "marked", "broken", "inspired" };

    private Sprite[] _sprites;
    private string[] _status;

    [MenuItem("MnM/Status Icon Preview")]
    public static void Open()
    {
        var win = GetWindow<StatusIconPreviewWindow>("Status Icons");
        win.minSize = new Vector2(300, 260);
        win.Load();
    }

    private void Load()
    {
        _sprites = new Sprite[Effects.Length];
        _status  = new string[Effects.Length];

        for (int i = 0; i < Effects.Length; i++)
        {
            string path = $"Art/Generated/UI/StatusIcons/status_{Effects[i]}";
            _sprites[i] = Resources.Load<Sprite>(path);
            _status[i]  = _sprites[i] != null ? "OK" : "MISSING";
        }
    }

    private void OnGUI()
    {
        if (_sprites == null) Load();

        EditorGUILayout.LabelField("Status Effect Icons — Resources.Load check", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        bool allOk = true;

        for (int i = 0; i < Effects.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Icon preview
            if (_sprites[i] != null)
            {
                var tex = _sprites[i].texture;
                GUILayout.Label(tex, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("[?]", GUILayout.Width(32), GUILayout.Height(32));
                allOk = false;
            }

            // Name and status
            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = _status[i] == "OK" ? Color.green : Color.red;
            EditorGUILayout.LabelField($"status_{Effects[i]}.png", GUILayout.Width(200));
            EditorGUILayout.LabelField(_status[i], style, GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        if (allOk)
        {
            var s = new GUIStyle(EditorStyles.boldLabel);
            s.normal.textColor = Color.green;
            EditorGUILayout.LabelField("All 8 icons loaded successfully.", s);
        }
        else
        {
            var s = new GUIStyle(EditorStyles.boldLabel);
            s.normal.textColor = Color.red;
            EditorGUILayout.LabelField("One or more icons failed to load.", s);
            EditorGUILayout.HelpBox(
                "Check that PNGs are in Assets/Resources/Art/Generated/UI/StatusIcons/ " +
                "and are imported as Sprite type.", MessageType.Warning);
        }

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Reload"))
            Load();
    }
}
