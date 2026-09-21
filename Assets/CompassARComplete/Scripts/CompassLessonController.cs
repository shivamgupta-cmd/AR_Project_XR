using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CompassLessonController : MonoBehaviour
{
    [Header("Compass")]
    public Transform compassRoot;
    public Transform needle;
    public Transform northTarget;
    public float needleTurnSpeed = 4f;
    public bool animateNeedle = true;

    [Header("Scene Groups")]
    public GameObject introGroup;
    public GameObject partsGroup;
    public GameObject magneticFieldGroup;
    public GameObject directionsGroup;
    public GameObject mapGroup;
    public GameObject activityGroup;
    public GameObject successGroup;

    [Header("VFX")]
    public ParticleSystem ambientDust;
    public ParticleSystem magneticParticles;
    public ParticleSystem northPulse;
    public ParticleSystem successBurst;

    [Header("Audio")]
    public AudioSource voiceSource;
    public AudioClip[] sceneVoiceClips;

    [Header("Preview")]
    public bool playOnStart;
    public float previewStepDelay = 3f;

    Coroutine preview;

    void Start(){ if(playOnStart) PlayCompletePreview(); }

    void Update()
    {
        if(!animateNeedle || needle==null || northTarget==null) return;
        Vector3 dir = northTarget.position - needle.position;
        dir.y = 0;
        if(dir.sqrMagnitude < .0001f) return;
        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        needle.rotation = Quaternion.Slerp(needle.rotation,target,Time.deltaTime*needleTurnSpeed);
    }

    public void ResetLesson()
    {
        StopPreview();
        SetOnly(null);
        if(ambientDust) ambientDust.Play();
        if(magneticParticles) magneticParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(northPulse) northPulse.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if(successBurst) successBurst.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void ShowIntroduction(){ SetOnly(introGroup); PlayVO(0); }
    public void ShowCompassParts(){ SetOnly(partsGroup); PlayVO(1); }
    public void ShowMagneticField(){ SetOnly(magneticFieldGroup); if(magneticParticles) magneticParticles.Play(); PlayVO(2); }
    public void ShowDirections(){ SetOnly(directionsGroup); PlayVO(3); }
    public void FindNorth(){ SetOnly(directionsGroup); if(northPulse) northPulse.Play(); PlayVO(4); }
    public void Explore3D(){ SetOnly(partsGroup); PlayVO(5); }
    public void ShowMapNavigation(){ SetOnly(mapGroup); PlayVO(6); }
    public void StartActivity(){ SetOnly(activityGroup); PlayVO(7); }
    public void ShowSuccess(){ SetOnly(successGroup); if(successBurst) successBurst.Play(); PlayVO(8); }

    void SetOnly(GameObject active)
    {
        GameObject[] all={introGroup,partsGroup,magneticFieldGroup,directionsGroup,mapGroup,activityGroup,successGroup};
        foreach(var g in all) if(g) g.SetActive(g==active);
    }

    void PlayVO(int i)
    {
        if(!voiceSource || sceneVoiceClips==null || i<0 || i>=sceneVoiceClips.Length || !sceneVoiceClips[i]) return;
        voiceSource.Stop(); voiceSource.clip=sceneVoiceClips[i]; voiceSource.Play();
    }

    public void PlayCompletePreview(){ StopPreview(); preview=StartCoroutine(Preview()); }
    public void StopPreview(){ if(preview!=null){StopCoroutine(preview);preview=null;} }
    IEnumerator Preview()
    {
        ShowIntroduction(); yield return new WaitForSeconds(previewStepDelay);
        ShowCompassParts(); yield return new WaitForSeconds(previewStepDelay);
        ShowMagneticField(); yield return new WaitForSeconds(previewStepDelay);
        ShowDirections(); yield return new WaitForSeconds(previewStepDelay);
        FindNorth(); yield return new WaitForSeconds(previewStepDelay);
        Explore3D(); yield return new WaitForSeconds(previewStepDelay);
        ShowMapNavigation(); yield return new WaitForSeconds(previewStepDelay);
        StartActivity(); yield return new WaitForSeconds(previewStepDelay);
        ShowSuccess(); preview=null;
    }
}