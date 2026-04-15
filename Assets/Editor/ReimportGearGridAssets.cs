using UnityEditor;

public class ReimportGearGridAssets
{
    public static void Execute()
    {
        AssetDatabase.ImportAsset("Assets/_Game/UI/USS/gear-grid.uss",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/_Game/UI/UXML/gear-grid.uxml",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }
}
