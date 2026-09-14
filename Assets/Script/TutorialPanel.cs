using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] Button m_PreviousBUtton,m_Nextbutton,m_closeButton;
    [SerializeField] GameObject[] m_TutorialPanel;
    //[SerializeField] ARPlaneManager m_PlaneManager;
    int currentindex = 0;


    private void Start()
    {
       // m_PlaneManager.enabled = false;
        for (int i = 0; i < m_TutorialPanel.Length; i++)
        {
            m_TutorialPanel[i].SetActive(false);
        }
        m_TutorialPanel[0].SetActive(true);
        m_PreviousBUtton.onClick.AddListener(() => PreviousPanel());
        m_Nextbutton.onClick.AddListener(()=>Nextpanel());
        m_PreviousBUtton.gameObject.SetActive(false);
        m_closeButton.onClick.AddListener(()=>Startplane());
    }
    void Nextpanel()
    {
        m_PreviousBUtton.gameObject.SetActive(true);
        for(int i = 0; i < m_TutorialPanel.Length; i++)
        {
            m_TutorialPanel[i].SetActive(false);
        }
        m_TutorialPanel[++currentindex].SetActive(true);
        if (currentindex == m_TutorialPanel.Length - 1)
        {
            m_closeButton.gameObject.SetActive(true);
            m_Nextbutton.gameObject.SetActive(false);
        }
    }

    void PreviousPanel()
    {
        m_closeButton.gameObject.SetActive(false);
        m_Nextbutton.gameObject.SetActive(true);
        for (int i = 0; i < m_TutorialPanel.Length; i++)
        {
            m_TutorialPanel[i].SetActive(false);
        }
        m_TutorialPanel[--currentindex].SetActive(true);
        if (currentindex == 0)
        {
            m_PreviousBUtton.gameObject.SetActive(false);
        }
    }
    void Startplane()
    {
       // m_PlaneManager.enabled = true;
    }
}
