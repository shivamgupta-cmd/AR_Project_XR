using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubjectTopicManager : MonoBehaviour
{
    public BtnClickSound btnClickSound;
    public SceneChange sceneChange;

    [Serializable]
    public class TopicData
    {
        public string topicName;
        public Sprite topicLogo;
    }

    [Serializable]
    public class SubjectData
    {
        public string subjectName;
        public List<TopicData> topics = new List<TopicData>();
    }


    [Header("All Subjects")]
    public List<SubjectData> subjects = new List<SubjectData>();


    [Header("Topic Button Spawn")]
    public Button topicButtonPrefab;

    public Transform topicButtonParent;


    public void ShowTopics(string subjectName)
    {
        ClearOldButtons();

        SubjectData selectedSubject = subjects.Find(x => x.subjectName.Equals(subjectName, StringComparison.OrdinalIgnoreCase));

        if (selectedSubject == null)
        {
            Debug.LogError("Subject not found : " + subjectName);
            return;
        }

        foreach (TopicData topic in selectedSubject.topics)
        {
            CreateTopicButton(selectedSubject, topic);
        }
    }


    private void CreateTopicButton(SubjectData subject,TopicData topic)
    {
        Button newButton = Instantiate(topicButtonPrefab, topicButtonParent);

        if (newButton.GetComponent<ButtonHoverFeedback>() == null)
        {
            newButton.gameObject.AddComponent<ButtonHoverFeedback>();
        }

        newButton.gameObject.name = topic.topicName + "_Button";


        Transform logoTransform = newButton.transform.Find("Icon");

        if (logoTransform != null)
        {
            Image logoImage = logoTransform.GetComponent<Image>();

            // Keep the prefab's default topic icon when this topic doesn't
            // have a custom logo assigned in the Inspector.
            if (logoImage != null && topic.topicLogo != null)
            {
                logoImage.sprite = topic.topicLogo;
            }
        }


        TMP_Text topicText = newButton.GetComponentInChildren<TMP_Text>(true);

        if (topicText != null)
        {
            topicText.text = topic.topicName;
        }


        string subjectName = subject.subjectName;
        string topicName = topic.topicName;

        newButton.onClick.RemoveAllListeners();

        newButton.onClick.AddListener(() => TopicButtonClick(subjectName, topicName));
    }


    private void TopicButtonClick(string subjectName, string topicName)
    {
        Debug.Log(  "Subject : " + subjectName + " | Topic : " + topicName);
        btnClickSound.PlayClickSound();
        sceneChange.Scenechange(topicName);

        // Yahan baad me:
        // AR Model Load
        // Scene Change
        // Topic Screen Open
        // etc.
    }


    private void ClearOldButtons()
    {
        for (int i = topicButtonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(topicButtonParent.GetChild(i).gameObject);
        }
    }
}
