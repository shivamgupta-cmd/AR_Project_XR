using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

namespace ARLearn.Editor
{
    /// <summary>One-time rollback for the AR setup accidentally applied to the two shell scenes.</summary>
    public static class ARProjectRollback
    {
        private const string DoneKey = "ARLearn.ARProjectRollback.ShellScenesRestored";

        [InitializeOnLoadMethod]
        private static void RunOnceAfterCompile()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(DoneKey, false) || Application.isBatchMode)
                    return;
                RestoreShellScenes();
                SessionState.SetBool(DoneKey, true);
            };
        }

        [MenuItem("AR Learn/Restore AR Loading and Main Menu")]
        public static void RestoreShellScenes()
        {
            var original = SceneManager.GetActiveScene().path;
            var scenes = new[]
            {
                "Assets/!AR New Project/Scenes/AR Loading.unity",
                "Assets/!AR New Project/Scenes/Main Menu AR_New.unity"
            };

            foreach (var path in scenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var origin = UnityEngine.Object.FindObjectOfType<XROrigin>(true);
                if (origin != null && origin.name == "XR Origin (AR Rig)")
                {
                    var camera = origin.Camera;
                    if (camera != null)
                    {
                        camera.transform.SetParent(null, true);
                        RemoveIfPresent<ARCameraManager>(camera.gameObject);
                        RemoveIfPresent<ARCameraBackground>(camera.gameObject);
                        RemoveIfPresent<TrackedPoseDriver>(camera.gameObject);
                    }
                    UnityEngine.Object.DestroyImmediate(origin.gameObject);
                }

                var session = UnityEngine.Object.FindObjectOfType<ARSession>(true);
                if (session != null && session.name == "AR Session")
                    UnityEngine.Object.DestroyImmediate(session.gameObject);

                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            Debug.Log("[AR Rollback] Restored AR Loading and Main Menu AR_New.");
            System.IO.File.WriteAllText("Logs/ARRollbackReport.txt",
                "Restored AR Loading and Main Menu AR_New at " + DateTime.Now.ToString("O"));
        }

        private static void RemoveIfPresent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null) UnityEngine.Object.DestroyImmediate(component);
        }
    }
}
