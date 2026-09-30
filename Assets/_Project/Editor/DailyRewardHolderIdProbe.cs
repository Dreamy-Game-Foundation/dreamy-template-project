using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class DailyRewardHolderIdProbe
{
    static DailyRewardHolderIdProbe()
    {
        EditorApplication.delayCall += Probe;
    }

    private static void Probe()
    {
        foreach (string path in new[]
        {
            "Assets/_Project/Dreamy Economy Contracts/0.1.0/Resource Holders/Prefabs/CoinHolder.prefab",
            "Assets/_Project/Dreamy Economy Contracts/0.1.0/Resource Holders/Prefabs/GemHolder.prefab"
        })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long fileId);
            Debug.Log($"[HolderIdProbe] {path} type={prefab?.GetType().Name} guid={guid} fileId={fileId}");
        }
    }
}
