using System.Collections;
using UnityEngine;

public class RutherfordActivityController : MonoBehaviour
{
    [Header("MODEL PARTS")]
    public GameObject nucleus, electronsRoot, ringsRoot, laser, goldFoil, scatterExperiment;
    [Header("VFX")]
    public ParticleSystem alphaBeam, passThrough, slightDeflection, backScatter, nucleusPulse, foilImpact, discoveryBurst;
    public Renderer goldFoilRenderer;
    [Header("SETTINGS")]
    public float electronOrbitSpeed=22f, experimentDuration=6f;
    public bool rotateElectronSystem=true;

    [Header("PREVIEW / TEST")]
    [Tooltip("Enable this to automatically preview the complete Rutherford activity when Play Mode starts.")]
    public bool playOnStart=false;

    [Tooltip("Delay before the automatic preview starts.")]
    public float playOnStartDelay=1f;

    [Tooltip("Delay between each preview step.")]
    public float previewStepDelay=2f;

    Coroutine routine;
    Coroutine previewRoutine;
    Material foilMat;

    void Awake(){ if(goldFoilRenderer) foilMat=goldFoilRenderer.material; ResetActivity(); }

    void Start()
    {
        if(playOnStart)
            PlayCompletePreview();
    }
    void Update(){ if(rotateElectronSystem && electronsRoot && electronsRoot.activeInHierarchy) electronsRoot.transform.Rotate(Vector3.up,electronOrbitSpeed*Time.deltaTime,Space.Self); }

    // ---------- COMPLETE AUTOMATIC PREVIEW ----------
    // Check Play On Start in Inspector to watch the complete activity
    // without creating Timeline signals first.
    public void PlayCompletePreview()
    {
        StopPreview();
        previewRoutine=StartCoroutine(CompletePreviewRoutine());
    }

    public void StopCompletePreview()
    {
        StopPreview();
        ResetActivity();
    }

    IEnumerator CompletePreviewRoutine()
    {
        ResetActivity();
        yield return new WaitForSeconds(Mathf.Max(0f,playOnStartDelay));

        ShowAtomicModel();
        yield return new WaitForSeconds(previewStepDelay);

        ShowEmptySpace();
        yield return new WaitForSeconds(previewStepDelay);

        RevealNucleus();
        yield return new WaitForSeconds(previewStepDelay);

        ShowGoldFoilSetup();
        yield return new WaitForSeconds(previewStepDelay);

        FireAlphaParticles();
        yield return new WaitForSeconds(Mathf.Max(previewStepDelay,experimentDuration));

        ShowPassThroughResult();
        yield return new WaitForSeconds(previewStepDelay);

        ShowDeflectionResult();
        yield return new WaitForSeconds(previewStepDelay);

        ShowBackScatterResult();
        yield return new WaitForSeconds(previewStepDelay);

        ShowDiscoveryConclusion();
        previewRoutine=null;
    }

    void StopPreview()
    {
        if(previewRoutine!=null)
        {
            StopCoroutine(previewRoutine);
            previewRoutine=null;
        }
    }

    public void ResetActivity(){
        StopRunning(); StopAllFX();
        Set(nucleus,false);Set(electronsRoot,false);Set(ringsRoot,false);Set(laser,false);Set(goldFoil,false);Set(scatterExperiment,false);Glow(0);
    }
    public void ShowAtomicModel(){Set(nucleus,true);Set(electronsRoot,true);Set(ringsRoot,true);}
    public void ShowEmptySpace(){ShowAtomicModel();Set(nucleus,false);}
    public void RevealNucleus(){Set(nucleus,true);Play(nucleusPulse);}
    public void ShowGoldFoilSetup(){Set(laser,true);Set(goldFoil,true);Set(scatterExperiment,true);Glow(.25f);}
    public void FireAlphaParticles(){StopRunning();routine=StartCoroutine(Experiment());}
    IEnumerator Experiment(){
        ShowGoldFoilSetup();Play(alphaBeam);yield return new WaitForSeconds(.55f);
        Play(foilImpact);Glow(1);Play(passThrough);yield return new WaitForSeconds(.35f);
        Play(slightDeflection);yield return new WaitForSeconds(.45f);
        Play(backScatter);yield return new WaitForSeconds(1f);
        Glow(.35f);yield return new WaitForSeconds(Mathf.Max(0,experimentDuration-2.35f));routine=null;
    }
    public void ShowPassThroughResult(){Play(passThrough);}
    public void ShowDeflectionResult(){Play(slightDeflection);}
    public void ShowBackScatterResult(){Play(backScatter);}
    public void ShowDiscoveryConclusion(){ShowAtomicModel();RevealNucleus();Play(discoveryBurst);}
    public void HighlightGoldFoil(){Glow(1);}
    public void RemoveGoldFoilHighlight(){Glow(0);}
    void Glow(float a){if(!foilMat)return;if(foilMat.HasProperty("_Glow"))foilMat.SetFloat("_Glow",a);}
    static void Set(GameObject g,bool s){if(g)g.SetActive(s);}
    static void Play(ParticleSystem p){if(p){p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);p.Play(true);}}
    static void Stop(ParticleSystem p){if(p)p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    void StopAllFX(){Stop(alphaBeam);Stop(passThrough);Stop(slightDeflection);Stop(backScatter);Stop(nucleusPulse);Stop(foilImpact);Stop(discoveryBurst);}
    void StopRunning(){if(routine!=null)StopCoroutine(routine);routine=null;}
}