#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public static class RealisticRollingBoilBuilder {
 const string R="Assets/RealisticRollingBoilFX";
 [MenuItem("Tools/Realistic Rolling Boil/Create Reference-Style Boiling Prefab")]
 public static void Build(){
  Ensure();
  Material bubble=Mat("Bubble_Clear","Bubble_Ring.png",new Color(.9f,.98f,1,.55f),1);
  Material foam=Mat("Surface_Highlights","Surface_Foam.png",new Color(1,1,1,.48f),1.2f);
  Material steam=Mat("Steam_Wisp","Steam_Wisp.png",new Color(.93f,.96f,.97f,.30f),1.5f);
  Material steam2=Mat("Steam_Haze","Steam_Wisp.png",new Color(.9f,.94f,.96f,.14f),1.9f);
  Material drop=Mat("Condensation_Drop","Droplet.png",new Color(.85f,.96f,1,.55f),1);

  GameObject root=new GameObject("REALISTIC_ROLLING_BOIL_FX");
  var c=root.AddComponent<RealisticRollingBoilController>();
  GameObject water=GameObject.CreatePrimitive(PrimitiveType.Cylinder);water.name="Water_Surface_DEMO";
  water.transform.SetParent(root.transform,false);water.transform.localPosition=new Vector3(0,.20f,0);water.transform.localScale=new Vector3(.62f,.20f,.62f);
  Object.DestroyImmediate(water.GetComponent<Collider>());
  Material wm=new Material(Shader.Find("RealisticRollingBoilFX/BoilingWaterURP"));wm.SetColor("_BaseColor",new Color(.72f,.88f,.94f,.48f));
  AssetDatabase.CreateAsset(wm,R+"/Materials/Boiling_Water.mat");water.GetComponent<Renderer>().sharedMaterial=wm;
  c.waterMesh=water.transform;c.waterRenderer=water.GetComponent<Renderer>();

  GameObject full=new GameObject("FULL_WATER_STATE");full.transform.SetParent(root.transform,false);full.transform.localPosition=water.transform.localPosition;full.transform.localScale=water.transform.localScale;
  GameObject low=new GameObject("LOW_WATER_STATE");low.transform.SetParent(root.transform,false);low.transform.localPosition=new Vector3(0,.035f,0);low.transform.localScale=new Vector3(.62f,.035f,.62f);
  c.fullWaterState=full.transform;c.lowWaterState=low.transform;

  c.bottomBubbles=PS(root.transform,"01_Bottom_Micro_Bubbles",bubble,new Vector3(0,.05f,0),55,.9f,.012f,.035f,.36f,.48f,0);
  c.rollingBubbles=PS(root.transform,"02_Rolling_Large_Bubbles",bubble,new Vector3(0,.07f,0),38,.8f,.025f,.075f,.52f,.47f,0);
  c.surfaceBursts=PS(root.transform,"03_Surface_Bubble_Bursts",bubble,new Vector3(0,.39f,0),30,.32f,.035f,.095f,.16f,.48f,1);
  c.surfaceFoam=PS(root.transform,"04_Surface_White_Highlights",foam,new Vector3(0,.405f,0),26,.38f,.025f,.07f,.05f,.48f,1);
  c.steamNear=Steam(root.transform,"05_Steam_Near_Surface",steam,new Vector3(0,.44f,0),20,2.4f,.12f,.28f,.35f,.5f);
  c.steamHigh=Steam(root.transform,"06_Steam_Rising_Haze",steam2,new Vector3(0,.55f,0),10,4.0f,.28f,.55f,.28f,.56f);
  c.condensation=PS(root.transform,"07_Reverse_Condensation_Drops",drop,new Vector3(0,.78f,0),14,1.5f,.014f,.03f,-.18f,.48f,2);

  string path=R+"/Prefabs/REALISTIC_ROLLING_BOIL_FX.prefab";PrefabUtility.SaveAsPrefabAsset(root,path);Object.DestroyImmediate(root);
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(path);
  EditorUtility.DisplayDialog("Rolling Boil FX","Created reference-style effect.\n\nThe video uses a strong rolling boil: dense clear bubbles, repeated surface breaks, bright surface highlights, and soft continuous steam. The prefab is built around those layers.\n\nUse your own water mesh by assigning Water Mesh and moving the FX layers to its surface.","OK");
 }
 static ParticleSystem PS(Transform par,string n,Material mat,Vector3 pos,float rate,float life,float s0,float s1,float speed,float radius,int mode){
  GameObject g=new GameObject(n);g.transform.SetParent(par,false);g.transform.localPosition=pos;var p=g.AddComponent<ParticleSystem>();
  var m=p.main;m.loop=true;m.playOnAwake=false;m.startLifetime=new ParticleSystem.MinMaxCurve(life*.75f,life*1.2f);m.startSpeed=new ParticleSystem.MinMaxCurve(speed*.65f,speed*1.25f);m.startSize=new ParticleSystem.MinMaxCurve(s0,s1);m.maxParticles=500;m.simulationSpace=ParticleSystemSimulationSpace.Local;
  var e=p.emission;e.rateOverTime=rate;var sh=p.shape;sh.shapeType=ParticleSystemShapeType.Circle;sh.radius=radius;sh.radiusThickness=1;sh.rotation=new Vector3(-90,0,0);
  var v=p.velocityOverLifetime;v.enabled=true;v.x=new ParticleSystem.MinMaxCurve(-.08f,.08f);v.z=new ParticleSystem.MinMaxCurve(-.08f,.08f);v.y=mode==2?new ParticleSystem.MinMaxCurve(-.45f,-.18f):new ParticleSystem.MinMaxCurve(speed*.35f,speed);
  if(mode==1){var sz=p.sizeOverLifetime;sz.enabled=true;sz.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.45f,1.15f),new Keyframe(1,0)));}
  var r=g.GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Billboard;r.sharedMaterial=mat;return p;
 }
 static ParticleSystem Steam(Transform par,string n,Material mat,Vector3 pos,float rate,float life,float s0,float s1,float speed,float radius){
  var p=PS(par,n,mat,pos,rate,life,s0,s1,speed,radius,0);
  var no=p.noise;no.enabled=true;no.strength=new ParticleSystem.MinMaxCurve(.08f,.22f);no.frequency=.3f;no.scrollSpeed=.2f;no.damping=true;
  var sz=p.sizeOverLifetime;sz.enabled=true;sz.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.25f),new Keyframe(.35f,1),new Keyframe(1,1.65f)));
  var col=p.colorOverLifetime;col.enabled=true;Gradient gr=new Gradient();gr.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.9f,.95f,.98f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.6f,.2f),new GradientAlphaKey(0,1)});col.color=gr;
  return p;
 }
 static Material Mat(string n,string tex,Color col,float pow){var m=new Material(Shader.Find("RealisticRollingBoilFX/SoftFXURP"));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(R+"/Textures/"+tex));m.SetColor("_BaseColor",col);m.SetFloat("_Power",pow);AssetDatabase.CreateAsset(m,R+"/Materials/"+n+".mat");return m;}
 static void Ensure(){foreach(string f in new[]{"Materials","Prefabs"})if(!AssetDatabase.IsValidFolder(R+"/"+f))AssetDatabase.CreateFolder(R,f);}
}
#endif