using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoyalSymbolsManager : MonoBehaviour
{
    [Header("Royal Symbol Buttons")]
    public Button[] symbolButtons;
    // 0 = Throne
    // 1 = Scepter
    // 2 = Royal Orb
    // 3 = Shield
    // 4 = Lion
    // 5 = Crown

    [Header("Symbol Voiceovers")]
    public AudioClip[] symbolVoiceovers;
    // Same order as buttons

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Conclusion Voiceover")]
    public AudioClip conclusionVoiceover;

    private bool[] symbolCompleted;

    private bool isPlayingVO = false;
    private bool conclusionPlayed = false;


    private void Start()
    {
        // Create completion status for 6 symbols
        symbolCompleted = new bool[symbolButtons.Length];

        // Initially all buttons are enabled
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            int index = i;

            symbolButtons[i].interactable = true;

            symbolButtons[i].onClick.AddListener(() =>
            {
                OnSymbolClicked(index);
            });
        }
    }


    public void OnSymbolClicked(int index)
    {
        // Safety checks
        if (isPlayingVO)
            return;

        if (symbolCompleted[index])
            return;

        if (index < 0 || index >= symbolVoiceovers.Length)
            return;

        // Mark this symbol as completed
        symbolCompleted[index] = true;

        // Disable all buttons while VO is playing
        DisableAllButtons();

        // Play selected symbol VO
        StartCoroutine(PlaySymbolVoiceover(index));
    }


    private IEnumerator PlaySymbolVoiceover(int index)
    {
        isPlayingVO = true;

        AudioClip clip = symbolVoiceovers[index];

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();

            // Wait until VO finishes
            yield return new WaitForSeconds(clip.length);
        }

        isPlayingVO = false;

        // Check whether all 6 symbols are completed
        if (AllSymbolsCompleted())
        {
            StartCoroutine(PlayConclusionVoiceover());
        }
        else
        {
            // Enable only symbols which are not completed
            EnableRemainingButtons();
        }
    }


    private void DisableAllButtons()
    {
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            symbolButtons[i].interactable = false;
        }
    }


    private void EnableRemainingButtons()
    {
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            if (!symbolCompleted[i])
            {
                symbolButtons[i].interactable = true;
            }
        }
    }


    private bool AllSymbolsCompleted()
    {
        for (int i = 0; i < symbolCompleted.Length; i++)
        {
            if (!symbolCompleted[i])
            {
                return false;
            }
        }

        return true;
    }


    private IEnumerator PlayConclusionVoiceover()
    {
        // Prevent conclusion from playing again
        if (conclusionPlayed)
            yield break;

        conclusionPlayed = true;

        // Keep all buttons disabled
        DisableAllButtons();

        isPlayingVO = true;

        if (conclusionVoiceover != null)
        {
            audioSource.clip = conclusionVoiceover;
            audioSource.Play();

            yield return new WaitForSeconds(conclusionVoiceover.length);
        }

        isPlayingVO = false;
    }
}