#if UNITY_EDITOR
using UnityEditor;using UnityEngine;using System.IO;
public static class WaterFXStrictBuilder{
 const string R="Assets/WaterFX_StrictReference",M=R+"/Materials",T=R+"/Textures";
 [MenuItem("Tools/Water FX Strict Reference/Create Reference Materials + Waterfall FX")]
 public static void Build(){
  AssetDatabase.Refresh();Ensure(M);
  var o=Mat(M+"/Ocean Water.mat","Custom/OceanWater");Set(o,"_FoamTex","Foam_Texture.png");Set(o,"_NormalMap","Ocean_Normal.png");Set(o,"_RippleTex","Ripple_Texture.png");Set(o,"_CausticsTex","Caustics_Texture.png");
  var r=Mat(M+"/River Water.mat","Custom/RiverWater");Set(r,"_FlowMap","Flow_Map.png");Set(r,"_NormalMap","Water_Normal.png");Set(r,"_FoamTex","Foam_Texture.png");
  var w=Mat(M+"/Waterfall.mat","Custom/Waterfall");Set(w,"_WaterfallTex","Waterfall_Texture.png");Set(w,"_NormalMap","Waterfall_Normal.png");Set(w,"_NoiseTex","Noise_Texture.png");
  if(!GameObject.Find("WATERFALL_EFFECT")){
   var root=new GameObject("WATERFALL_EFFECT");var q=GameObject.CreatePrimitive(PrimitiveType.Quad);q.name="Waterfall";q.transform.SetParent(root.transform,false);q.GetComponent<MeshRenderer>().sharedMaterial=w;Object.DestroyImmediate(q.GetComponent<Collider>());
   var mist=PS(root.transform,"Mist",new Vector3(0,-.42f,.02f),45,.12f,.12f,1.4f,ParticleSystemShapeType.Cone);
   var splash=PS(root.transform,"Splash",new Vector3(0,-.5f,.03f),70,.045f,.8f,.65f,ParticleSystemShapeType.Hemisphere);
   var foam=PS(root.transform,"Foam",new Vector3(0,-.5f,.04f),55,.07f,.05f,1.1f,ParticleSystemShapeType.Circle);
   var c=root.AddComponent<WaterfallFXController>();c.mist=mist;c.splash=splash;c.foam=foam;c.Apply();Selection.activeGameObject=root;
  }
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Strict reference Water FX created.");
 }
 static Material Mat(string p,string sn){var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m)return m;var s=Shader.Find(sn);if(!s){Debug.LogError("Missing shader "+sn);return null;}m=new Material(s);AssetDatabase.CreateAsset(m,p);return m;}
 static void Set(Material m,string p,string f){if(!m)return;var t=AssetDatabase.LoadAssetAtPath<Texture2D>(T+"/"+f);m.SetTexture(p,t);EditorUtility.SetDirty(m);}
 static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;var par=Path.GetDirectoryName(p).Replace("\\","/");if(!AssetDatabase.IsValidFolder(par))Ensure(par);AssetDatabase.CreateFolder(par,Path.GetFileName(p));}
 static ParticleSystem PS(Transform par,string n,Vector3 pos,float rate,float size,float speed,float life,ParticleSystemShapeType st){var g=new GameObject(n);g.transform.SetParent(par,false);g.transform.localPosition=pos;var p=g.AddComponent<ParticleSystem>();var ma=p.main;ma.loop=true;ma.startLifetime=life;ma.startSize=size;ma.startSpeed=speed;ma.maxParticles=400;var e=p.emission;e.rateOverTime=rate;var sh=p.shape;sh.shapeType=st;sh.radius=.18f;return p;}
}
#endif