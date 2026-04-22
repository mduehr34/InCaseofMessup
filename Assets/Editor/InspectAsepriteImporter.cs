using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class InspectAsepriteImporter
{
    public static void Execute()
    {
        string assetPath = "Assets/_Game/Art/Generated/Characters/Aldric/aldric_sheet.aseprite";
        var importer = AssetImporter.GetAtPath(assetPath);
        var sb = new StringBuilder();

        if (importer == null) { Debug.LogError("[Inspect] No importer found"); return; }

        sb.AppendLine("IMPORTER TYPE: " + importer.GetType().FullName);
        sb.AppendLine("---");

        var so = new SerializedObject(importer);
        var prop = so.GetIterator();

        while (prop.NextVisible(true))
        {
            string name = prop.propertyPath.ToLower();
            if (name.Contains("anim") || name.Contains("tag") || name.Contains("clip")
                || name.Contains("generate") || name.Contains("frame") || name.Contains("layer"))
            {
                string val = prop.propertyType == SerializedPropertyType.Boolean ? prop.boolValue.ToString()
                           : prop.propertyType == SerializedPropertyType.Integer  ? prop.intValue.ToString()
                           : prop.propertyType == SerializedPropertyType.String   ? prop.stringValue
                           : prop.propertyType == SerializedPropertyType.Enum     ? prop.enumValueIndex.ToString()
                           : prop.propertyType.ToString();
                sb.AppendLine($"{prop.propertyPath} = {val}");
            }
        }

        sb.AppendLine("--- SUB-ASSETS ---");
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var a in subAssets)
            sb.AppendLine($"  {a.name}  ({a.GetType().Name})");

        string outPath = "Assets/Editor/aseprite_inspect.txt";
        File.WriteAllText(outPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[Inspect] Written to " + outPath);
    }
}
