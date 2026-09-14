using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Question
{
    public string questionText;
    public string[] options;
    public int correctAnswerIndex;

    public AudioClip questionVoice;   
    public AudioClip correctAnswerVoice;
}

public class QuizManager : MonoBehaviour
{
    public ModelModeManager modelModeManager;

    [Header("Questions")]
    public Question[] questions;
    private int currentQuestionIndex = 0;

    [Header("Panels")]
    public GameObject m_quizPanel;
    public GameObject m_gameOverPanel;

    [Header("UI")]
    public TMP_Text questionText;
    public Button[] optionTexts;  
    public TMP_Text scoreText;

    private int attemptCount = 0;
    private int score = 0;
    private bool questionFinished = false;

    public Sprite m_green;
    public Sprite m_red;
    public Sprite m_default;

    //public Color green;
    //public Color red;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip wrongAnswerVoice;
    public AudioClip assessmentCompleteVoice;

    [Header("Buttons")]
    public Button m_quitButton;
    public Button m_playAgainButton;

    void Start()
    {
        m_quitButton.gameObject.SetActive(false);
        m_playAgainButton.gameObject.SetActive(false);
        m_quizPanel.SetActive(true);
        m_gameOverPanel.SetActive(false);
        for (int i = 0; i < optionTexts.Length; i++)
        {
            int index = i; 
            optionTexts[i].onClick.AddListener(() => SelectAnswer(index));
        }
        //LoadQuestion();
        UpdateScore();

        m_quitButton.onClick.AddListener(OnClickQuitButton);
        m_playAgainButton.onClick.AddListener(OnClickAssessmentButton);
    }

    public void SelectAnswer(int selectedIndex)
    {
        if (questionFinished) return;

        attemptCount++;
        int correctIndex = questions[currentQuestionIndex].correctAnswerIndex;

        Button clickedButton = optionTexts[selectedIndex];

        if (selectedIndex == correctIndex)
        {
            score += (attemptCount == 1) ? 2 : 1;
            //clickedButton.GetComponent<Image>().color = green;
            questionFinished = true;
            UpdateScore();

            PlayCorrectAnswerVoice();
            DisableAllButtons();

            // Apply this after disabling the buttons because Unity's
            // Disabled Sprite state otherwise overwrites the green sprite.
            clickedButton.GetComponent<Image>().overrideSprite = m_green;

            StartCoroutine(LoadNextQuestionAfterVoice());
        }
        else
        {
            //clickedButton.GetComponent<Image>().color = red;
           // clickedButton.enabled = false;
            clickedButton.interactable = false;
            clickedButton.GetComponent<Image>().overrideSprite = m_red;

            PlayWrongAnswerVoice();

            if (attemptCount >= 3)
            {
                questionFinished = true;
                DisableAllButtons();
                PlayCorrectAnswerVoice();
               StartCoroutine(AutoCorrect(correctIndex));

            }
        }
    }

    public void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            modelModeManager.CloseQuiz();
            //m_quizPanel.SetActive(false);
            //m_gameOverPanel.SetActive(true);

            //m_quitButton.gameObject.SetActive(false);
            //m_playAgainButton.gameObject.SetActive(false);

            //UpdateScore(true);

            //PlayAssessmentCompleteVoice();
            ////questionText.text = "Quiz Finished!";
            //DisableAllButtons();

            //StartCoroutine(EnableButtonsAfterAssessmentVoice());
            return;
        }

        attemptCount = 0;
        questionFinished = false;

        Question q = questions[currentQuestionIndex];
        questionText.text = q.questionText;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].GetComponentInChildren<TMP_Text>().text = q.options[i];
            //optionTexts[i].GetComponent<Image>().color = Color.white;
            Image optionImage = optionTexts[i].GetComponent<Image>();
            optionImage.sprite = m_default;
            optionImage.overrideSprite = null;
            optionTexts[i].enabled = true;
            optionTexts[i].interactable = true; 
        }

        PlayQuestionVoice();
        EnableAllButtons();
    }

    public void StartQuiz()
    {
        StopAllCoroutines();
        currentQuestionIndex = 0;
        attemptCount = 0;
        score = 0;
        questionFinished = false;

        if (m_quizPanel != null)
            m_quizPanel.SetActive(true);

        if (m_gameOverPanel != null)
            m_gameOverPanel.SetActive(false);

        UpdateScore();
        LoadQuestion();
    }

    public void StopQuizAudio()
    {
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }


    void PlayQuestionVoice()
    {
        AudioClip clip = questions[currentQuestionIndex].questionVoice;
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void PlayCorrectAnswerVoice()
    {
        AudioClip clip = questions[currentQuestionIndex].correctAnswerVoice;
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void PlayWrongAnswerVoice()
    {
        if (wrongAnswerVoice != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = wrongAnswerVoice;
            audioSource.Play();
        }
    }

    void PlayAssessmentCompleteVoice()
    {
        if (assessmentCompleteVoice != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = assessmentCompleteVoice;
            audioSource.Play();
        }
    }

    IEnumerator EnableButtonsAfterAssessmentVoice()
    {
        if (audioSource != null)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        m_quitButton.gameObject.SetActive(true);
        m_playAgainButton.gameObject.SetActive(true);
        m_quitButton.interactable = true;
        m_playAgainButton.interactable = true;
    }


    IEnumerator AutoCorrect(int correctIndex)
    {
        yield return new WaitForSeconds(.5f);
        //optionTexts[correctIndex].GetComponent<Image>().color = green;
        optionTexts[correctIndex].GetComponent<Image>().overrideSprite = m_green;
        yield return new WaitForSeconds(.3f);
        //optionTexts[correctIndex].GetComponent<Image>().color = Color.white;
        optionTexts[correctIndex].GetComponent<Image>().overrideSprite = m_default;
        yield return new WaitForSeconds(.3f);
        //optionTexts[correctIndex].GetComponent<Image>().color = green;
        optionTexts[correctIndex].GetComponent<Image>().overrideSprite = m_green;
        yield return new WaitForSeconds(.3f);
        //optionTexts[correctIndex].GetComponent<Image>().color = Color.white;
        optionTexts[correctIndex].GetComponent<Image>().overrideSprite = m_default;
        yield return new WaitForSeconds(.3f);
        //optionTexts[correctIndex].GetComponent<Image>().color = green;
        optionTexts[correctIndex].GetComponent<Image>().overrideSprite = m_green;
        yield return new WaitForSeconds(1f);

                StartCoroutine(LoadNextQuestionAfterVoice());
    }

    IEnumerator LoadNextQuestionAfterVoice()
    {
        if (audioSource != null)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }

        currentQuestionIndex++;
        LoadQuestion();
    }

    void UpdateScore(bool isGameOver = false)
    {
        int maxScore = questions.Length * 2;

        if (isGameOver)
            scoreText.text = "Score : " + score + " / " + maxScore;
        else
            scoreText.text = "Score : " + score;
    }


    void DisableAllButtons()
    {
        foreach (Button btn in optionTexts)
        {
            btn.interactable = false;
           // btn.enabled = false;
        }
    }

    void EnableAllButtons()
    {
        foreach (Button btn in optionTexts)
        {
            btn.interactable = true;
           // btn.enabled = true;
        }
    }

    private void OnClickQuitButton()
    {
        Application.Quit();
    }
    private void OnClickAssessmentButton()
    {
        SceneManager.LoadScene(0);
    }
}
