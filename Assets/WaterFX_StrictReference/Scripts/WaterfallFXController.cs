using UnityEngine;
public class WaterfallFXController:MonoBehaviour{
 public ParticleSystem mist,splash,foam;
 [Range(0,3)]public float mistIntensity=1,splashIntensity=1,splashScale=1;
 [Range(-2,2)]public float windInfluence=.2f;
 void OnValidate(){Apply();}
 public void Apply(){
  if(mist){var e=mist.emission;e.rateOverTime=45*mistIntensity;var f=mist.forceOverLifetime;f.enabled=true;f.x=windInfluence;}
  if(splash){var e=splash.emission;e.rateOverTime=70*splashIntensity;splash.transform.localScale=Vector3.one*splashScale;}
  if(foam){var e=foam.emission;e.rateOverTime=55*splashIntensity;}
 }
}