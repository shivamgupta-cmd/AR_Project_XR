#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public static class LiquidEnergySpiralBuilder {
 const string R="Assets/LiquidEnergySpiralFX", G=R+"/Generated";
 [MenuItem("Tools/Liquid Energy Spiral FX/Create Prefab")]
 public static void Build(){
  if(!AssetDatabase.IsValidFolder(G)) AssetDatabase.CreateFolder(R,"Generated");
  Material core=Mat(G+"/M_Core.mat",new Color(.65f,.95f,1,1));
  Material glow=Mat(G+"/M_Glow.mat",new Color(.1f,.65f,1,.28f));
  GameObject root=new GameObject("LiquidEnergySpiralFX");
  GameObject sp=new GameObject("Energy_Spiral"); sp.transform.SetParent(root.transform,false);
  LineRenderer gl=sp.AddComponent<LineRenderer>(); Setup(gl,glow,.10f,10);
  GameObject cg=new GameObject("Bright_Core"); cg.transform.SetParent(sp.transform,false);
  LineRenderer cl=cg.AddComponent<LineRenderer>(); Setup(cl,core,.028f,11);
  var sc=sp.AddComponent<LiquidEnergySpiral>(); sc.core=cl; sc.glow=gl;
  Particles(root.transform,core);
  Ripples(root.transform,glow);
  string path=G+"/LiquidEnergySpiralFX.prefab";
  if(AssetDatabase.LoadAssetAtPath<GameObject>(path)) AssetDatabase.DeleteAsset(path);
  PrefabUtility.SaveAsPrefabAsset(root,path); Object.DestroyImmediate(root);
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  Debug.Log("Created "+path);
 }
 static Material Mat(string path,Color c){
  Shader s=Shader.Find("Universal Render Pipeline/Particles/Unlit");
  if(!s)s=Shader.Find("Sprites/Default");
  Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,path);}
  Texture t=AssetDatabase.LoadAssetAtPath<Texture>(R+"/Textures/T_Glow.png");
  if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",t); if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",t);
  if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c); if(m.HasProperty("_Color"))m.SetColor("_Color",c);
  if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1); if(m.HasProperty("_ZWrite"))m.SetFloat("_ZWrite",0);
  m.renderQueue=3000; return m;
 }
 static void Setup(LineRenderer l,Material m,float w,int order){
  l.useWorldSpace=false;l.widthMultiplier=w;l.numCornerVertices=5;l.numCapVertices=5;l.material=m;l.sortingOrder=order;
 }
 static void Particles(Transform p,Material mat){
  GameObject g=new GameObject("Spiral_Sparkles_Bubbles");g.transform.SetParent(p,false);
  ParticleSystem ps=g.AddComponent<ParticleSystem>();var m=ps.main;m.loop=true;m.startLifetime=new ParticleSystem.MinMaxCurve(1.2f,2.8f);
  m.startSpeed=new ParticleSystem.MinMaxCurve(.02f,.09f);m.startSize=new ParticleSystem.MinMaxCurve(.012f,.055f);m.maxParticles=180;
  var e=ps.emission;e.rateOverTime=30;var s=ps.shape;s.shapeType=ParticleSystemShapeType.Donut;s.radius=.7f;s.donutRadius=.12f;
  var v=ps.velocityOverLifetime;v.enabled=true;v.orbitalY=.65f;v.y=new ParticleSystem.MinMaxCurve(.02f,.14f);
  var n=ps.noise;n.enabled=true;n.strength=.035f;n.frequency=.55f;
  var rr=g.GetComponent<ParticleSystemRenderer>();rr.material=mat;rr.renderMode=ParticleSystemRenderMode.Billboard;
 }
 static void Ripples(Transform p,Material mat){
  GameObject g=new GameObject("Base_Energy_Ripples");g.transform.SetParent(p,false);g.transform.localPosition=new Vector3(0,-.78f,0);g.transform.localRotation=Quaternion.Euler(90,0,0);
  ParticleSystem ps=g.AddComponent<ParticleSystem>();var m=ps.main;m.loop=true;m.startLifetime=2;m.startSpeed=0;m.startSize=.5f;m.maxParticles=8;
  var e=ps.emission;e.rateOverTime=.8f;var s=ps.shape;s.enabled=false;var z=ps.sizeOverLifetime;z.enabled=true;
  z.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(1,3f)));
  var rr=g.GetComponent<ParticleSystemRenderer>();rr.material=mat;rr.renderMode=ParticleSystemRenderMode.Billboard;
 }
}
#endif