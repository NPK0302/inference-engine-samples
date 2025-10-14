#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(PoseDetectionLive))]
public class PoseDetectionLiveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PoseDetectionLive comp = (PoseDetectionLive)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Validate & Auto-Fix"))
        {
            ValidateAndFix(comp);
        }
    }

    static void ValidateAndFix(PoseDetectionLive comp)
    {
        if (comp == null) return;

        int fixesApplied = 0;
        string compPath = comp.gameObject.scene.name + ":" + comp.transform.GetHierarchyPath();

        var so = new SerializedObject(comp);

        // anchorsCSV
        if (comp.anchorsCSV == null)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/anchors.csv");
            if (asset != null)
            {
                var prop = so.FindProperty("anchorsCSV");
                if (prop != null)
                {
                    Undo.RecordObject(comp, "Auto-assign anchorsCSV");
                    prop.objectReferenceValue = asset;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned anchorsCSV for {compPath}");
                }
            }
            else
            {
                Debug.LogError($"PoseDetectionLiveEditor: {compPath} - anchorsCSV not found at Assets/Data/anchors.csv");
            }
        }

        // poseDetector
        if (comp.poseDetector == null)
        {
            var detectorPath = "Assets/Models/pose_detection.onnx";
            var modelObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(detectorPath);
            if (modelObj != null)
            {
                var prop = so.FindProperty("poseDetector");
                if (prop != null)
                {
                    Undo.RecordObject(comp, "Auto-assign poseDetector");
                    prop.objectReferenceValue = modelObj;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned poseDetector for {compPath}");
                }
            }
            else
            {
                Debug.LogWarning($"PoseDetectionLiveEditor: {compPath} - pose_detector not found at {detectorPath}");
            }
        }

        // poseLandmarker
        if (comp.poseLandmarker == null)
        {
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
                var prop = so.FindProperty("poseLandmarker");
                if (prop != null)
                {
                    Undo.RecordObject(comp, "Auto-assign poseLandmarker");
                    prop.objectReferenceValue = found;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned poseLandmarker for {compPath}");
                }
            }
            else
            {
                Debug.LogWarning($"PoseDetectionLiveEditor: {compPath} - no landmarker model found under Assets/Models");
            }
        }

        // Check other references and report
        // posePreview: find first PosePreview in scene
        if (comp.posePreview == null)
        {
            var found = FindFirstInScene<PosePreview>();
            if (found != null)
            {
                var p = so.FindProperty("posePreview");
                if (p != null)
                {
                    Undo.RecordObject(comp, "Auto-assign posePreview");
                    p.objectReferenceValue = found;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned posePreview ({found.gameObject.name}) for {compPath}");
                }
            }
            else
            {
                Debug.LogError($"PoseDetectionLiveEditor: {compPath} - posePreview is not assigned and none found in scene.");
            }
        }

        // imagePreview: find first ImagePreview in scene
        if (comp.imagePreview == null)
        {
            var found = FindFirstInScene<ImagePreview>();
            if (found != null)
            {
                var p = so.FindProperty("imagePreview");
                if (p != null)
                {
                    Undo.RecordObject(comp, "Auto-assign imagePreview");
                    p.objectReferenceValue = found;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned imagePreview ({found.gameObject.name}) for {compPath}");
                }
            }
            else
            {
                Debug.LogError($"PoseDetectionLiveEditor: {compPath} - imagePreview is not assigned and none found in scene.");
            }
        }

        // cameraCapture: find first CameraCapture in scene
        if (comp.cameraCapture == null)
        {
            var found = FindFirstInScene<CameraCapture>();
            if (found != null)
            {
                var p = so.FindProperty("cameraCapture");
                if (p != null)
                {
                    Undo.RecordObject(comp, "Auto-assign cameraCapture");
                    p.objectReferenceValue = found;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    fixesApplied++;
                    Debug.Log($"PoseDetectionLiveEditor: Auto-assigned cameraCapture ({found.gameObject.name}) for {compPath}");
                }
            }
            else
            {
                Debug.LogError($"PoseDetectionLiveEditor: {compPath} - cameraCapture is not assigned and none found in scene.");
            }
        }

        // Helper: find first component of type T in the active scene (includes inactive)
        static T FindFirstInScene<T>() where T : Component
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                var comps = root.GetComponentsInChildren<T>(true);
                if (comps != null && comps.Length > 0)
                    return comps[0];
            }
            return null;
        }

        if (fixesApplied > 0)
        {
            Debug.Log($"PoseDetectionLiveEditor: Applied {fixesApplied} automatic fix(es) to {compPath}. Remember to save the scene.");
        }
        else
        {
            Debug.Log($"PoseDetectionLiveEditor: No automatic fixes applied for {compPath}.");
        }
    }
}
#endif
