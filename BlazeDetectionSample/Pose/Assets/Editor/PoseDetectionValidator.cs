#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Editor utility to validate PoseDetectionLive components in the current scene
public static class PoseDetectionValidator
{
    [MenuItem("Tools/Pose Detection/Validate Scene...")]
    public static void ValidateScene()
    {
        var fixesApplied = 0;
        var errorsFound = 0;

        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        var components = new List<PoseDetectionLive>();
        foreach (var root in roots)
        {
            components.AddRange(root.GetComponentsInChildren<PoseDetectionLive>(true));
        }

        foreach (var comp in components)
        {
            if (comp == null) continue;

            // Check anchorsCSV
            if (comp.anchorsCSV == null)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/anchors.csv");
                if (asset != null)
                {
                    Undo.RecordObject(comp, "Auto-assign anchorsCSV");
                    comp.anchorsCSV = asset;
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                }
                else
                {
                    Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - anchorsCSV is missing and no Assets/Data/anchors.csv found.");
                    errorsFound++;
                }
            }

            // Try to auto-assign model assets from Assets/Models without referencing ModelAsset type
            if (comp.poseDetector == null)
            {
                var detectorPath = "Assets/Models/pose_detection.onnx";
                var modelObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(detectorPath);
                if (modelObj != null)
                {
                    // Assign via SerializedObject to avoid compile-time dependency on ModelAsset
                    var so = new SerializedObject(comp);
                    var prop = so.FindProperty("poseDetector");
                    if (prop != null)
                    {
                        Undo.RecordObject(comp, "Auto-assign poseDetector");
                        prop.objectReferenceValue = modelObj;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);
                        fixesApplied++;
                    }
                    else
                    {
                        Debug.LogWarning($"PoseDetectionValidator: {GetPath(comp.gameObject)} - couldn't find serialized property 'poseDetector' to assign.");
                    }
                }
                else
                {
                    Debug.LogWarning($"PoseDetectionValidator: {GetPath(comp.gameObject)} - poseDetector not assigned and {detectorPath} not found as an asset.");
                }
            }

            if (comp.poseLandmarker == null)
            {
                // Try a few common filenames for the landmarker
                string[] candidatePaths = new string[] {
                    "Assets/Models/pose_landmarks_detector_full.onnx",
                    "Assets/Models/pose_landmarks_detector_heavy.onnx",
                    "Assets/Models/pose_landmarks_detector_lite.onnx"
                };
                UnityEngine.Object found = null;
                foreach (var p in candidatePaths)
                {
                    var m = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
                    if (m != null) { found = m; break; }
                }
                if (found != null)
                {
                    var so = new SerializedObject(comp);
                    var prop = so.FindProperty("poseLandmarker");
                    if (prop != null)
                    {
                        Undo.RecordObject(comp, "Auto-assign poseLandmarker");
                        prop.objectReferenceValue = found;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);
                        fixesApplied++;
                    }
                    else
                    {
                        Debug.LogWarning($"PoseDetectionValidator: {GetPath(comp.gameObject)} - couldn't find serialized property 'poseLandmarker' to assign.");
                    }
                }
                else
                {
                    Debug.LogWarning($"PoseDetectionValidator: {GetPath(comp.gameObject)} - poseLandmarker not assigned and no known model files found under Assets/Models.");
                }
            }

            // Check and try to auto-assign other references (posePreview, imagePreview, cameraCapture)
            var soComp = new SerializedObject(comp);

            if (comp.posePreview == null)
            {
                var found = FindFirstInScene<PosePreview>();
                if (found != null)
                {
                    var prop = soComp.FindProperty("posePreview");
                    if (prop != null)
                    {
                        Undo.RecordObject(comp, "Auto-assign posePreview");
                        prop.objectReferenceValue = found;
                        soComp.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);
                        fixesApplied++;
                        Debug.Log($"PoseDetectionValidator: Auto-assigned posePreview ({found.gameObject.name}) for {GetPath(comp.gameObject)}");
                    }
                }
                else
                {
                    Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - posePreview is not assigned and none found in scene.");
                    errorsFound++;
                }
            }

            if (comp.imagePreview == null)
            {
                var found = FindFirstInScene<ImagePreview>();
                if (found != null)
                {
                    var prop = soComp.FindProperty("imagePreview");
                    if (prop != null)
                    {
                        Undo.RecordObject(comp, "Auto-assign imagePreview");
                        prop.objectReferenceValue = found;
                        soComp.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);
                        fixesApplied++;
                        Debug.Log($"PoseDetectionValidator: Auto-assigned imagePreview ({found.gameObject.name}) for {GetPath(comp.gameObject)}");
                    }
                }
                else
                {
                    Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - imagePreview is not assigned and none found in scene.");
                    errorsFound++;
                }
            }

            if (comp.cameraCapture == null)
            {
                var found = FindFirstInScene<CameraCapture>();
                if (found != null)
                {
                    var prop = soComp.FindProperty("cameraCapture");
                    if (prop != null)
                    {
                        Undo.RecordObject(comp, "Auto-assign cameraCapture");
                        prop.objectReferenceValue = found;
                        soComp.ApplyModifiedProperties();
                        EditorUtility.SetDirty(comp);
                        fixesApplied++;
                        Debug.Log($"PoseDetectionValidator: Auto-assigned cameraCapture ({found.gameObject.name}) for {GetPath(comp.gameObject)}");
                    }
                }
                else
                {
                    Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - cameraCapture is not assigned and none found in scene.");
                    errorsFound++;
                }
            }
            if (comp.poseDetector == null)
            {
                Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - poseDetector is not assigned.");
                errorsFound++;
            }
            if (comp.poseLandmarker == null)
            {
                Debug.LogError($"PoseDetectionValidator: {GetPath(comp.gameObject)} - poseLandmarker is not assigned.");
                errorsFound++;
            }
        }

        AssetDatabase.SaveAssets();
        if (fixesApplied > 0)
            Debug.Log($"PoseDetectionValidator: Applied {fixesApplied} automatic fix(es). Remember to save the scene.");
        if (errorsFound == 0 && fixesApplied == 0)
            Debug.Log("PoseDetectionValidator: No issues found.");
    }

    static string GetPath(GameObject go)
    {
        return go.scene.name + ":" + go.transform.GetHierarchyPath();
    }

    // Helper: find first component of type T in the active scene (includes inactive)
    static T FindFirstInScene<T>() where T : Component
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            var comps = root.GetComponentsInChildren<T>(true);
            if (comps != null && comps.Length > 0)
                return comps[0];
        }
        return null;
    }
}

// Helper extension to get a transform's hierarchy path
public static class TransformExtensions
{
    public static string GetHierarchyPath(this Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
#endif
