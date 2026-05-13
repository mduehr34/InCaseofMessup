using UnityEditor;

public class RefreshAssets
{
    public static void Execute()
    {
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("[RefreshAssets] AssetDatabase.Refresh() complete");
    }
}
