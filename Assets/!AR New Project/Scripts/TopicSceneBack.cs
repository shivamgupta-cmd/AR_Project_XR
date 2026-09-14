using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopicSceneBack : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        backButton.onClick.AddListener(Back);
    }

    private void Back()
    {
        MainMenuController.SetReturnToTopic();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(Back);
    }
}