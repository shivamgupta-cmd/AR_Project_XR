using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoyalSymbolsManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button[] symbolButtons;

    /*
        0 = Royal Throne
        1 = Scepter
        2 = Royal Orb
        3 = Shield
        4 = Lion
        5 = Crown
    */

    [Header("Object Highlighters")]
    public RoyalSymbolHighlighter[] highlighters;

    [Header("Symbol Voiceovers")]
    public AudioClip[] symbolVoiceovers;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Conclusion Voiceover")]
    public AudioClip conclusionVoiceover;

    private bool[] completed;

    private bool isPlayingVO = false;
    private bool conclusionPlayed = false;


    private void Start()
    {
        completed = new bool[symbolButtons.Length];

        // Make sure all models start with their normal emission
        RestoreAllEmissions();

        // Add button listeners
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            int index = i;

            symbolButtons[i].interactable = true;

            symbolButtons[i].onClick.AddListener(
                () => OnSymbolClicked(index)
            );
        }
    }


    public void OnSymbolClicked(int index)
    {
        if (isPlayingVO)
            return;

        if (completed[index])
            return;

        if (index < 0 ||
            index >= symbolVoiceovers.Length)
            return;


        // Mark as completed
        completed[index] = true;


        // Disable all buttons during VO
        DisableAllButtons();


        // --------------------------------
        // STOP ALL PREVIOUS HIGHLIGHTS
        // --------------------------------

        StopAllHighlights();


        // --------------------------------
        // TURN OFF EMISSION OF ALL MODELS
        // --------------------------------

        DisableAllEmissions();


        // --------------------------------
        // SELECTED MODEL
        // EMISSION ON
        // HIGHLIGHT ON
        // --------------------------------

        if (highlighters[index] != null)
        {
            highlighters[index].EnableEmission();
            highlighters[index].StartHighlight();
        }


        // Play VO
        StartCoroutine(
            PlaySymbolVO(index)
        );
    }


    private IEnumerator PlaySymbolVO(int index)
    {
        isPlayingVO = true;

        AudioClip clip =
            symbolVoiceovers[index];

        if (clip != null)
        {
            audioSource.clip = clip;

            audioSource.Play();

            yield return new WaitForSeconds(
                clip.length
            );
        }


        // --------------------------------
        // VO COMPLETE
        // --------------------------------

        // Stop selected highlight
        StopAllHighlights();


        // --------------------------------
        // RESTORE ALL MODEL EMISSIONS
        // --------------------------------

        RestoreAllEmissions();


        isPlayingVO = false;


        // --------------------------------
        // CHECK ALL 6 COMPLETED
        // --------------------------------

        if (AllSymbolsCompleted())
        {
            StartCoroutine(
                PlayConclusionVO()
            );
        }
        else
        {
            // Enable remaining buttons
            EnableRemainingButtons();
        }
    }


    // =====================================
    // DISABLE ALL EMISSIONS
    // =====================================

    private void DisableAllEmissions()
    {
        for (int i = 0; i < highlighters.Length; i++)
        {
            if (highlighters[i] != null)
            {
                highlighters[i].DisableEmission();
            }
        }
    }


    // =====================================
    // RESTORE ALL EMISSIONS
    // =====================================

    private void RestoreAllEmissions()
    {
        for (int i = 0; i < highlighters.Length; i++)
        {
            if (highlighters[i] != null)
            {
                highlighters[i].RestoreOriginalEmission();
            }
        }
    }


    // =====================================
    // STOP ALL HIGHLIGHTS
    // =====================================

    private void StopAllHighlights()
    {
        for (int i = 0; i < highlighters.Length; i++)
        {
            if (highlighters[i] != null)
            {
                highlighters[i].StopHighlight();
            }
        }
    }


    // =====================================
    // DISABLE ALL BUTTONS
    // =====================================

    private void DisableAllButtons()
    {
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            symbolButtons[i].interactable = false;
        }
    }


    // =====================================
    // ENABLE REMAINING BUTTONS
    // =====================================

    private void EnableRemainingButtons()
    {
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            if (!completed[i])
            {
                symbolButtons[i].interactable = true;
            }
        }
    }


    // =====================================
    // CHECK ALL SYMBOLS
    // =====================================

    private bool AllSymbolsCompleted()
    {
        for (int i = 0; i < completed.Length; i++)
        {
            if (!completed[i])
            {
                return false;
            }
        }

        return true;
    }


    // =====================================
    // CONCLUSION VO
    // =====================================

    private IEnumerator PlayConclusionVO()
    {
        if (conclusionPlayed)
            yield break;

        conclusionPlayed = true;

        // All buttons disabled
        DisableAllButtons();

        // No highlight
        StopAllHighlights();

        // All model emissions ON
        RestoreAllEmissions();

        isPlayingVO = true;

        if (conclusionVoiceover != null)
        {
            audioSource.clip =
                conclusionVoiceover;

            audioSource.Play();

            yield return new WaitForSeconds(
                conclusionVoiceover.length
            );
        }

        isPlayingVO = false;
    }
}