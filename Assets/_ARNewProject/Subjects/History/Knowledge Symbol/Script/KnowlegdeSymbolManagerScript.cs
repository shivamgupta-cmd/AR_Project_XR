using UnityEngine;
using System.Collections;

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

        clickedObject.alreadyClicked = true;

        completedObjects++;

        // Start highlight
        clickedObject.StartHighlight();

        // Play VO assigned to this object
        if (clickedObject.vo != null && audioSource != null)
        {
            audioSource.Stop();

            audioSource.clip = clickedObject.vo;

            audioSource.Play();
        }

        // Check if all objects are clicked
        if (completedObjects >= objects.Length)
        {
            StartCoroutine(PlayFinalConclusion());
        }
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

        // Play final conclusion VO
        if (audioSource != null && finalConclusionVO != null &&  LastfinalConclusionVO != null)
        {
            audioSource.clip = LastfinalConclusionVO;
            audioSource.Play();
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
            audioSource.clip = finalConclusionVO;
            audioSource.Play();
        }
    }
}