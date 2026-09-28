using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
public class KnowledgeSymbolManagerScript : MonoBehaviour
{
    [Header("Knowledge Symbol Objects")]
    public KnowledgeSymbolObject[] objects;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Final Conclusion VO")]
    public AudioClip finalConclusionVO;
    public AudioClip LastfinalConclusionVO;

    private int completedObjects = 0;
    private bool finalVOPlayed = false;

    public PlayableDirector playable;
    // Currently highlighted object
    private KnowledgeSymbolObject currentHighlightedObject;

    private Coroutine objectVOCoroutine;

    private void Start()
    {
        foreach (KnowledgeSymbolObject obj in objects)
        {
            if (obj != null)
            {
                obj.manager = this;
            }
        }
    }

    public void ObjectClicked(KnowledgeSymbolObject clickedObject)
    {
        if (clickedObject == null)
            return;

        if (clickedObject.alreadyClicked)
            return;

        // ------------------------------------------------
        // TURN OFF PREVIOUS OBJECT HIGHLIGHT
        // ------------------------------------------------
        if (currentHighlightedObject != null)
        {
            currentHighlightedObject.StopHighlight();
            currentHighlightedObject = null;
        }

        // Stop previous VO coroutine
        if (objectVOCoroutine != null)
        {
            StopCoroutine(objectVOCoroutine);
            objectVOCoroutine = null;
        }

        // Stop previous audio
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // ------------------------------------------------
        // MARK OBJECT AS CLICKED
        // ------------------------------------------------
        clickedObject.alreadyClicked = true;
        completedObjects++;

        // ------------------------------------------------
        // START NEW HIGHLIGHT
        // ------------------------------------------------
        clickedObject.StartHighlight();
        currentHighlightedObject = clickedObject;

        // ------------------------------------------------
        // PLAY OBJECT VO
        // ------------------------------------------------
        if (clickedObject.vo != null && audioSource != null)
        {
            audioSource.clip = clickedObject.vo;
            audioSource.Play();

            objectVOCoroutine = StartCoroutine(
                WaitForObjectVO(clickedObject)
            );
        }

        // ------------------------------------------------
        // CHECK ALL OBJECTS COMPLETED
        // ------------------------------------------------
        if (completedObjects >= objects.Length)
        {
            StartCoroutine(PlayFinalConclusion());
        }
    }

    private IEnumerator WaitForObjectVO(KnowledgeSymbolObject clickedObject)
    {
        // Wait until VO finishes
        if (audioSource != null)
        {
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }

        // ------------------------------------------------
        // VO COMPLETE → TURN OFF HIGHLIGHT
        // ------------------------------------------------
        if (clickedObject != null)
        {
            clickedObject.StopHighlight();

            if (currentHighlightedObject == clickedObject)
            {
                currentHighlightedObject = null;
            }
        }

        objectVOCoroutine = null;
    }

    private IEnumerator PlayFinalConclusion()
    {
        if (finalVOPlayed)
            yield break;

        finalVOPlayed = true;

        // Wait for current object VO to finish
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }

        // Turn off current highlight
        if (currentHighlightedObject != null)
        {
            currentHighlightedObject.StopHighlight();
            currentHighlightedObject = null;
        }

        // ------------------------------------------------
        // LAST FINAL CONCLUSION VO
        // ------------------------------------------------
        if (audioSource != null && LastfinalConclusionVO != null)
        {
            audioSource.clip = LastfinalConclusionVO;
            audioSource.Play();

            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
            playable.Resume();
        }

        // ------------------------------------------------
        // FINAL CONCLUSION VO
        // ------------------------------------------------
        //if (audioSource != null && finalConclusionVO != null)
        //{
        //    audioSource.clip = finalConclusionVO;
        //    audioSource.Play();

        //    yield return new WaitWhile(
        //        () => audioSource.isPlaying
        //    );
        //}
    }
}