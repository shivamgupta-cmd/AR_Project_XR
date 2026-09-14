using UnityEngine;
using UnityEngine.SceneManagement; // For scene management

public class SceneChanger : MonoBehaviour
{
    // Function to load a scene by its name
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Alternatively, load a scene by index
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
