#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Create a ModelAsset ScriptableObject for a selected .onnx file using reflection.
/// Menu: Tools > Inference > Create ModelAsset From ONNX
/// </summary>
public static class CreateModelAssetFromOnnx
{
    [MenuItem("Tools/Inference/Create ModelAsset From ONNX", priority = 200)]
    public static void CreateForSelectedOnnx()
    {
        var obj = Selection.activeObject;
        if (obj == null)
        {
            Debug.LogError("Select a .onnx file in the Project window first.");
            return;
        }

        var path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("Please select a .onnx file (e.g. Assets/Models/pose_detection.onnx).");
            return;
        }

        // Find the ModelAsset type via loaded assemblies
        Type modelAssetType = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ModelAsset" || x.FullName == "Unity.InferenceEngine.ModelAsset");
                if (t != null)
                {
                    modelAssetType = t;
                    break;
                }
            }
            catch { }
        }

        if (modelAssetType == null)
        {
            Debug.LogError("ModelAsset type not found. Is the Inference Engine package (com.unity.ai.inference) installed?");
            return;
        }

        // Check if a ModelAsset already exists at that path
        var existing = AssetDatabase.LoadAssetAtPath(path, modelAssetType);
        if (existing != null)
        {
            Debug.Log("A ModelAsset already exists for this path: " + path);
            Selection.activeObject = existing;
            return;
        }

        // Create a ScriptableObject instance of the ModelAsset type
        var instance = ScriptableObject.CreateInstance(modelAssetType);
        if (instance == null)
        {
            Debug.LogError("Failed to create instance of ModelAsset type.");
            return;
        }

        // Attempt to find a field or property that can hold the ONNX asset (assign the main ONNX asset)
        var onnxAsset = AssetDatabase.LoadMainAssetAtPath(path) as UnityEngine.Object;

        bool assigned = false;
        // Try fields
        var fields = modelAssetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType))
            {
                try
                {
                    f.SetValue(instance, onnxAsset);
                    assigned = true;
                    break;
                }
                catch { }
            }
        }

        // Try properties if not assigned
        if (!assigned)
        {
            var props = modelAssetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (!p.CanWrite) continue;
                if (typeof(UnityEngine.Object).IsAssignableFrom(p.PropertyType))
                {
                    try
                    {
                        p.SetValue(instance, onnxAsset);
                        assigned = true;
                        break;
                    }
                    catch { }
                }
            }
        }

        // Create asset next to the ONNX file, with .asset extension
        var assetPath = path.Substring(0, path.Length - ".onnx".Length) + ".asset"; // e.g. Assets/Models/pose_detection.asset
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(instance, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (assigned)
            Debug.Log($"Created ModelAsset at {assetPath} and assigned ONNX reference.");
        else
            Debug.LogWarning($"Created ModelAsset at {assetPath} but could not find a suitable field/property to assign the ONNX asset. You may need to assign it manually in the Inspector.");

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = instance;
    }
}
#endif
