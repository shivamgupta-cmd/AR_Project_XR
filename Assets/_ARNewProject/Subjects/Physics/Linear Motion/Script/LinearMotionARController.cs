using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMotionARController : MonoBehaviour
{
    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip arIntroVO;
    [SerializeField] private AudioClip clickLabels;

    private bool isPaused;

    void Start()
    {
        if (labels == null)
            return;

        foreach (GameObject label in labels)
        {
            if (label != null)
                label.SetActive(false);
        }
    }


    public void StartARActivity()
    {

        StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        PlayAudio(arIntroVO);
        yield return WaitForVoiceOver();

        PlayAudio(clickLabels);

        yield return WaitForVoiceOver();

        ShowLabels();
    }


    public void ShowLabels()
    {
        if (labels == null)
            return;

        foreach (GameObject label in labels)
        {
            if (label != null)
                label.SetActive(true);
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();

        audioSource.loop = false;
        audioSource.clip = clip;

        audioSource.Play();
    }

    private IEnumerator WaitForVoiceOver()
    {
        yield return null;

        while (isPaused ||
               (audioSource != null && audioSource.isPlaying))
        {
            yield return null;
        }
    }
}
