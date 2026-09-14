using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

[DisallowMultipleComponent]
public class SceneARImageLibrary : MonoBehaviour
{
    [Header("THIS SCENE'S REFERENCES")]
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    private XRReferenceImageLibrary sceneImageLibrary;

    [Header("INITIALIZATION")]
    [SerializeField, Min(1f)]
    private float initializationTimeout = 10f;

    private IEnumerator Start()
    {
        if (trackedImageManager == null)
        {
            Debug.LogError(
                "[AR Library] Assign this scene's ARTrackedImageManager.",
                this
            );

            yield break;
        }

        if (sceneImageLibrary == null || sceneImageLibrary.count == 0)
        {
            // Do not continue tracking with an unintended library.
            trackedImageManager.enabled = false;

            Debug.LogError(
                "[AR Library] Assign a non-empty Scene Image Library.",
                this
            );

            yield break;
        }

        float deadline =
            Time.realtimeSinceStartup + initializationTimeout;

        // Wait for XR initialization without starting the camera
        // or forcing the application into AR mode.
        while (!IsImageTrackingSubsystemAvailable())
        {
            if (Time.realtimeSinceStartup >= deadline)
            {
                Debug.LogError(
                    "[AR Library] Image tracking subsystem is unavailable. " +
                    "Check XR initialization and the platform provider.",
                    this
                );

                yield break;
            }

            yield return null;
        }

        if (trackedImageManager == null)
            yield break;

        ApplySceneLibrary();
    }

    private void ApplySceneLibrary()
    {
        // Preserve the state chosen by ModelModeManager.
        bool wasEnabled = trackedImageManager.enabled;

        trackedImageManager.enabled = false;

        try
        {
            RuntimeReferenceImageLibrary runtimeLibrary =
                trackedImageManager.CreateRuntimeLibrary(
                    sceneImageLibrary
                );

            // Explicitly replace the subsystem's runtime image library.
            trackedImageManager.referenceLibrary = runtimeLibrary;

            // Do not force image tracking ON when the app is in 3D mode.
            trackedImageManager.enabled = wasEnabled;

            Debug.Log(
                $"[AR Library] Scene: {gameObject.scene.name}\n" +
                $"Assigned asset: {sceneImageLibrary.name}\n" +
                $"Runtime image count: {runtimeLibrary.count}",
                this
            );

            for (int i = 0; i < runtimeLibrary.count; i++)
            {
                Debug.Log(
                    $"[AR Library] Image: {runtimeLibrary[i].name}\n" +
                    $"GUID: {runtimeLibrary[i].guid}",
                    this
                );
            }
        }
        catch (Exception exception)
        {
            // Keep tracking disabled rather than using the wrong library.
            Debug.LogError(
                $"[AR Library] Could not apply '{sceneImageLibrary.name}'.\n" +
                exception,
                this
            );
        }
    }

    private static bool IsImageTrackingSubsystemAvailable()
    {
        XRGeneralSettings settings = XRGeneralSettings.Instance;

        if (settings == null ||
            settings.Manager == null ||
            settings.Manager.activeLoader == null)
        {
            return false;
        }

        XRImageTrackingSubsystem subsystem =
            settings.Manager.activeLoader
                .GetLoadedSubsystem<XRImageTrackingSubsystem>();

        return subsystem != null;
    }
}
