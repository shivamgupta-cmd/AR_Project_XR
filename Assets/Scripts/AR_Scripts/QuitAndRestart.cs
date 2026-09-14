using UnityEngine;
using UnityEngine.SceneManagement;  // Required for reloading the current scene

public class QuitAndRestart : MonoBehaviour
{
    // Method to quit the application
    public void QuitApplication()
    {
        Debug.Log("Application quitting...");

        // If running in the Unity editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a standalone build
        Application.Quit();
#endif
    }

    // Method to restart the application (reload current scene)
    public void RestartApplication()
    {
        Debug.Log("Restarting application...");

        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
