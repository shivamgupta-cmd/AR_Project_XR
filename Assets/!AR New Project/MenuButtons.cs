using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    public AudioClip m_audioClipBtnClick;
    public AudioSource m_audioSourceBtnClick;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    void StartGame()
    {
        PlayBtnClickClip();
        SceneManager.LoadScene("Main Menu AR_New"); // apna scene name
    }

    void ExitGame()
    {
        PlayBtnClickClip();
        Application.Quit();
        Debug.Log("Application Quit");
    }
    public void PlayBtnClickClip()
    {
        m_audioSourceBtnClick.clip = m_audioClipBtnClick;
        m_audioSourceBtnClick.Play();

    }
}