using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiButtonVOManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonVO
    {
        public Button button;
        public AudioClip voiceOver;

        [HideInInspector]
        public bool isClicked = false;
    }

    [Header("Buttons + VO")]
    public ButtonVO[] buttons;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Final VO")]
    public AudioClip finalVO;

    private bool finalVOPlayed = false;
    private Coroutine currentCoroutine;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;

            if (buttons[i].button != null)
            {
                buttons[i].button.onClick.AddListener(() => OnButtonClicked(index));
            }
        }
    }

    void OnButtonClicked(int index)
    {
        buttons[index].isClicked = true;
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(PlayButtonVO(index));
    }

    IEnumerator PlayButtonVO(int index)
    {
        if (audioSource != null && buttons[index].voiceOver != null)
        {
            audioSource.Stop();
            audioSource.clip = buttons[index].voiceOver;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        CheckAllButtonsCompleted();
    }

    void CheckAllButtonsCompleted()
    {
        if (finalVOPlayed)
            return;

        foreach (ButtonVO item in buttons)
        {
            if (!item.isClicked)
                return;
        }
        if (finalVO != null)
        {
            StartCoroutine(PlayFinalVO());

        }

    }

    IEnumerator PlayFinalVO()
    {
        finalVOPlayed = true;

        while (audioSource != null && audioSource.isPlaying)
            yield return null;

        if (audioSource != null && finalVO != null)
        {
            audioSource.clip = finalVO;
            audioSource.Play();
        }
    }
}
