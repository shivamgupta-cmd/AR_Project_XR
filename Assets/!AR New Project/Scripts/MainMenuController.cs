using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

    [Header("Topic Manager")]
    public SubjectTopicManager m_subjectTopicManager;
    public BtnClickSound btnClickSound;

    public CanvasGroup[] m_screnes;
    public int m_screnesCount;
    public float m_screnesChangeSmooth;


    [Header("buttons")]
    public Button m_physics;
    public Button m_chemistry;
    public Button m_biology;
    public Button m_history;
    public Button m_civics;
    public Button m_geography;

    [Header("Back Button")]
    public Button m_backButton;

    public TMP_Text m_subjectTopicName;

    private const string SelectedSubjectKey = "SelectedSubject";
    private const string ReturnToTopicKey = "ReturnToTopic";

    void Start()
    {
        AddHoverFeedback(m_physics);
        AddHoverFeedback(m_chemistry);
        AddHoverFeedback(m_biology);
        AddHoverFeedback(m_history);
        AddHoverFeedback(m_civics);
        AddHoverFeedback(m_geography);

        m_physics.onClick.AddListener(() => SubjectButtonClick("Physics"));
        m_chemistry.onClick.AddListener(() => SubjectButtonClick("Chemistry"));
        m_biology.onClick.AddListener(() => SubjectButtonClick("Biology"));
        m_history.onClick.AddListener(() => SubjectButtonClick("History"));
        m_civics.onClick.AddListener(() => SubjectButtonClick("Civics"));
        m_geography.onClick.AddListener(() => SubjectButtonClick("Geography"));

        m_backButton.onClick.AddListener(BackButtonClick);

        // Start Screen
        //m_screnesCount = 0;

        //UpdateBackButton();        
        GetData();

    }

    private static void AddHoverFeedback(Button button)
    {
        if (button != null && button.GetComponent<ButtonHoverFeedback>() == null)
        {
            button.gameObject.AddComponent<ButtonHoverFeedback>();
        }
    }



    void Update()
    {
        ScreneManager(m_screnesCount);
    }

    private void ScreneManager(int index)
    {

        for (int i = 0; i < m_screnes.Length; i++)
        {
            if (index == i)
            {
                m_screnes[i].alpha = Mathf.Lerp(m_screnes[i].alpha, 1f, Time.deltaTime * m_screnesChangeSmooth);
                m_screnes[i].blocksRaycasts = true;
            }
            else
            {
                m_screnes[i].alpha = Mathf.Lerp(m_screnes[i].alpha, 0f, Time.deltaTime * m_screnesChangeSmooth);
                m_screnes[i].blocksRaycasts = false;
            }
        }
    }

    private void SubjectButtonClick(string subjectName)
    {
        btnClickSound.PlayClickSound();
        m_screnesCount = 1;

        m_subjectTopicName.text = subjectName;

        m_subjectTopicManager.ShowTopics(subjectName);
        SetData(subjectName);
        UpdateBackButton();
    }


    public void SetData(string subjectName)
    {
        PlayerPrefs.SetString(SelectedSubjectKey, subjectName);
        PlayerPrefs.Save();
    }

    public void GetData()
    {
        int returnToTopic = PlayerPrefs.GetInt(ReturnToTopicKey, 0);
        string subjectName = PlayerPrefs.GetString(SelectedSubjectKey, "");

        if (returnToTopic == 1 && !string.IsNullOrEmpty(subjectName))
        {
            m_screnesCount = 1;
            m_subjectTopicName.text = subjectName;
            m_subjectTopicManager.ShowTopics(subjectName);
            PlayerPrefs.SetInt(ReturnToTopicKey, 0);
            PlayerPrefs.Save();
        }
        else
        {
            m_screnesCount = 0;
        }

        UpdateBackButton();
    }


    private void BackButtonClick()
    {
        m_screnesCount = 0;

        UpdateBackButton();
    }
    private void UpdateBackButton()
    {
        m_backButton.gameObject.SetActive(m_screnesCount == 1);
    }

    public static void SetReturnToTopic()
    {
        PlayerPrefs.SetInt(ReturnToTopicKey, 1);
        PlayerPrefs.Save();
    }
}
