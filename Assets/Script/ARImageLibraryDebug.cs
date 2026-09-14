using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ARImageLibraryDebug : MonoBehaviour
{
    [Header("Assign a visible UI TextMeshPro text")]
    [SerializeField] private TMP_Text debugText;

    private float nextRefreshTime;
    private string previousReport = string.Empty;

    private void Update()
    {
        // Refresh once per second, including when Time.timeScale is zero.
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + 1f;

        var managers = FindObjectsByType<ARTrackedImageManager>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        StringBuilder report = new StringBuilder();

        report.AppendLine(
            $"Current scene: {SceneManager.GetActiveScene().name}"
        );

        int activeManagerCount = 0;

        foreach (ARTrackedImageManager manager in managers)
        {
            if (!manager.isActiveAndEnabled)
                continue;

            activeManagerCount++;

            report.AppendLine();
            report.AppendLine(
                $"Manager: {manager.name} | ID: {manager.GetInstanceID()}"
            );

            report.AppendLine(
                $"Manager scene: {manager.gameObject.scene.name}"
            );

            bool subsystemRunning =
                manager.subsystem != null &&
                manager.subsystem.running;

            report.AppendLine($"Subsystem running: {subsystemRunning}");

            var library = manager.referenceLibrary;

            if (library == null)
            {
                report.AppendLine("Library: NULL");
            }
            else
            {
                report.AppendLine($"Library image count: {library.count}");

                for (int i = 0; i < library.count; i++)
                {
                    report.AppendLine(
                        $"Library image: {library[i].name}"
                    );
                }
            }

            int detectedCount = 0;

            foreach (ARTrackedImage image in manager.trackables)
            {
                if (image == null)
                    continue;

                detectedCount++;

                report.AppendLine(
                    $"Detected: {image.referenceImage.name}" +
                    $" | State: {image.trackingState}"
                );
            }

            if (detectedCount == 0)
                report.AppendLine("Detected: None");
        }

        report.AppendLine();
        report.AppendLine($"Active image managers: {activeManagerCount}");

        string message = report.ToString();

        if (debugText != null)
            debugText.text = message;

        // Only log when the report changes.
        if (message != previousReport)
        {
            Debug.Log(message, this);
            previousReport = message;
        }
    }
}