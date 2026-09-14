using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LabelButtonController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonVO
    {
        public Button button;
        public AudioClip voiceOver;

        [HideInInspector]
        public bool isClicked = false;
    }

    [Header("Label Buttons + VO")]
    public ButtonVO[] buttons;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Final VO")]
    public AudioClip finalVO;

    private bool finalVOPlayed = false;
    private Coroutine currentCoroutine;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;

            if (buttons[i].button != null)
                buttons[i].button.onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    private void OnButtonClicked(int index)
    {
        buttons[index].isClicked = true;
        UIFadeIn fade = buttons[index].button.GetComponentInParent<UIFadeIn>();

        if (fade != null)
            fade.FadeOut();

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(PlayButtonVO(index));
    }

    private IEnumerator PlayButtonVO(int index)
    {
        if (audioSource != null && buttons[index].voiceOver != null)
        {
            audioSource.Stop();
            audioSource.clip = buttons[index].voiceOver;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        currentCoroutine = null;

        CheckAllButtonsCompleted();
    }

    private void CheckAllButtonsCompleted()
    {
        if (finalVOPlayed)
            return;

        foreach (ButtonVO item in buttons)
        {
            if (!item.isClicked)
                return;
        }

        if (finalVO != null)
            StartCoroutine(PlayFinalVO());
    }

    private IEnumerator PlayFinalVO()
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