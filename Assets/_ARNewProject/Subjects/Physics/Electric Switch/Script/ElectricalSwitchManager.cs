using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricalSwitchManager : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] AudioSource audioSources;
    [SerializeField] GameObject[] panels;
    [SerializeField] ObjectHighlighter ObjectHighlighter;
    [SerializeField] BoxCollider BoxCollider;
    void Start()
    {
        BoxCollider.enabled = false;
        StartCoroutine(ElectricSwitch());
    }

    IEnumerator ElectricSwitch()
    {
        audioSources.PlayOneShot(audioClips[0]);
        yield return new WaitForSeconds(audioClips[0].length);
        audioSources.PlayOneShot(audioClips[1]);
        yield return new WaitForSeconds(audioClips[1].length);
        audioSources.PlayOneShot(audioClips[5]);
        yield return new WaitForSeconds(audioClips[5].length);
        ObjectHighlighter.StartHighlight();
        BoxCollider.enabled = true;
    }   
    public void ClickSwitchBtn()
    {
        StartCoroutine(ClickSwitch());
    }
    IEnumerator ClickSwitch()
    {
        ObjectHighlighter.StopHighlight();
        BoxCollider.enabled = false;
        audioSources.PlayOneShot(audioClips[2]);
        yield return new WaitForSeconds(audioClips[2].length);
        audioSources.PlayOneShot(audioClips[3]);
        yield return new WaitForSeconds(audioClips[3].length);
        audioSources.PlayOneShot(audioClips[4]);
        yield return new WaitForSeconds(audioClips[4].length);
    }
}
