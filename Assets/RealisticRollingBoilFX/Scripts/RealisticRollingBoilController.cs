using System.Collections;
using UnityEngine;

public class RealisticRollingBoilController : MonoBehaviour
{
    [Header("YOUR WATER")]
    public Transform waterMesh;
    public Renderer waterRenderer;
    public Transform fullWaterState;
    public Transform lowWaterState;

    [Header("REFERENCE-MATCHED FX")]
    public ParticleSystem bottomBubbles;
    public ParticleSystem rollingBubbles;
    public ParticleSystem surfaceBursts;
    public ParticleSystem surfaceFoam;
    public ParticleSystem steamNear;
    public ParticleSystem steamHigh;
    public ParticleSystem condensation;

    [Header("BOIL CONTROL")]
    [Range(0,1)] public float boilIntensity;
    public float heatUpSeconds=3f;
    public float evaporateSeconds=12f;
    public float condenseSeconds=10f;
    public bool lowerWaterWhileBoiling=true;

    Coroutine routine;
    Vector3 fullP,lowP,fullS,lowS;

    void Awake(){CacheWater(); Apply(0); StopAllParticles();}
    void CacheWater(){
        if(!waterMesh)return;
        fullP=fullWaterState?fullWaterState.localPosition:waterMesh.localPosition;
        fullS=fullWaterState?fullWaterState.localScale:waterMesh.localScale;
        if(lowWaterState){lowP=lowWaterState.localPosition;lowS=lowWaterState.localScale;}
        else {lowS=new Vector3(fullS.x,fullS.y*.12f,fullS.z);lowP=fullP-new Vector3(0,(fullS.y-lowS.y)*.5f,0);}
    }

    // Timeline / Signal friendly
    public void StartRollingBoil(){StopRoutine();routine=StartCoroutine(HeatToBoil());}
    public void BoilAndEvaporate(){StopRoutine();routine=StartCoroutine(Evaporate());}
    public void ReverseCondenseToWater(){StopRoutine();routine=StartCoroutine(Condense());}
    public void StopEffect(){StopRoutine();boilIntensity=0;Apply(0);StopAllParticles();}
    public void SetCalm(){SetBoilIntensity(0);}
    public void SetSimmer(){SetBoilIntensity(.35f);}
    public void SetMediumBoil(){SetBoilIntensity(.65f);}
    public void SetFullRollingBoil(){SetBoilIntensity(1f);}

    public void SetBoilIntensity(float v){boilIntensity=Mathf.Clamp01(v);Apply(boilIntensity);}

    IEnumerator HeatToBoil(){
        float s=boilIntensity,t=0;
        while(t<heatUpSeconds){t+=Time.deltaTime;SetBoilIntensity(Mathf.Lerp(s,1,Mathf.SmoothStep(0,1,t/heatUpSeconds)));yield return null;}
        SetBoilIntensity(1);routine=null;
    }
    IEnumerator Evaporate(){
        yield return HeatToBoil();
        float t=0;
        while(t<evaporateSeconds){
            t+=Time.deltaTime;float n=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/evaporateSeconds));
            SetBoilIntensity(1);
            if(lowerWaterWhileBoiling&&waterMesh){waterMesh.localPosition=Vector3.Lerp(fullP,lowP,n);waterMesh.localScale=Vector3.Lerp(fullS,lowS,n);}
            yield return null;
        }
        if(waterMesh)waterMesh.gameObject.SetActive(false);
        Stop(bottomBubbles);Stop(rollingBubbles);Stop(surfaceBursts);Stop(surfaceFoam);
        Rate(steamNear,1);Rate(steamHigh,1);routine=null;
    }
    IEnumerator Condense(){
        Stop(bottomBubbles);Stop(rollingBubbles);Stop(surfaceBursts);Stop(surfaceFoam);
        Play(steamNear);Play(steamHigh);Play(condensation);
        if(waterMesh)waterMesh.gameObject.SetActive(true);
        float t=0;
        while(t<condenseSeconds){
            t+=Time.deltaTime;float n=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/condenseSeconds));
            Rate(steamNear,1-n);Rate(steamHigh,1-n);Rate(condensation,Mathf.Sin(n*Mathf.PI));
            if(waterMesh){waterMesh.localPosition=Vector3.Lerp(lowP,fullP,n);waterMesh.localScale=Vector3.Lerp(lowS,fullS,n);}
            yield return null;
        }
        Stop(steamNear);Stop(steamHigh);Stop(condensation);boilIntensity=0;Apply(0);routine=null;
    }

    void Apply(float v){
        // Rolling boil in reference: lots of small rising bubbles + frequent larger surface breaks + soft steam.
        Set(bottomBubbles, v, .15f, 1f);
        Set(rollingBubbles, v, .28f, 1f);
        Set(surfaceBursts, v, .42f, 1f);
        Set(surfaceFoam, v, .5f, 1f);
        Set(steamNear, v, .22f, .85f);
        Set(steamHigh, v, .38f, .75f);
        if(waterRenderer && waterRenderer.material.HasProperty("_Boil"))
            waterRenderer.material.SetFloat("_Boil",v);
    }
    static void Set(ParticleSystem p,float v,float threshold,float max){
        if(!p)return; float n=Mathf.InverseLerp(threshold,1,v);
        if(n>.001f)Play(p); else Stop(p); Rate(p,n*max);
    }
    static void Rate(ParticleSystem p,float m){if(!p)return;var e=p.emission;e.rateOverTimeMultiplier=Mathf.Max(0,m);}
    static void Play(ParticleSystem p){if(p&&!p.isPlaying)p.Play();}
    static void Stop(ParticleSystem p){if(p)p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    void StopRoutine(){if(routine!=null)StopCoroutine(routine);routine=null;}
    void StopAllParticles(){Stop(bottomBubbles);Stop(rollingBubbles);Stop(surfaceBursts);Stop(surfaceFoam);Stop(steamNear);Stop(steamHigh);Stop(condensation);}
}