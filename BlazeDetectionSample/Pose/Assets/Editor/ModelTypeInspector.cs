#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Simple Editor helper that logs the main asset type of each asset under Assets/Models.
/// Use Tools > Inspect > Log Models Types to run.
/// </summary>
public static class ModelTypeInspector
{
    [MenuItem("Tools/Inspect/Log Models Types")]
    public static void LogModelTypes()
    {
        var guids = AssetDatabase.FindAssets("", new[] { "Assets/Models" });
        if (guids == null || guids.Length == 0)
        {
            Debug.Log("ModelTypeInspector: No assets found under Assets/Models.");
            return;
        }

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var t = AssetDatabase.GetMainAssetTypeAtPath(path);
            Debug.Log(path + " -> " + (t != null ? t.FullName : "null"));
        }
    }
}
#endif
