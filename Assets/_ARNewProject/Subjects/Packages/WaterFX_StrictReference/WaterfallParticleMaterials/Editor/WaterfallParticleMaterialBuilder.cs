#if UNITY_EDITOR
using UnityEditor; using UnityEngine; using System.IO;
public static class WaterfallParticleMaterialBuilder{
const string R="Assets/WaterfallParticleMaterials",T=R+"/Textures",M=R+"/Materials";
[MenuItem("Tools/Waterfall FX/Create Particle Materials")]
public static void Build(){
 AssetDatabase.Refresh(); Ensure(M);
 Make("M_Mist", "Mist_Soft.png", new Color(.82f,.94f,1f,.55f), .8f, 1.35f);
 Make("M_Splash", "Splash_Streaks.png", new Color(.75f,.94f,1f,.9f), 1.35f, .75f);
 Make("M_Foam", "Foam_Bubbles.png", Color.white, 1.1f, .9f);
 AssetDatabase.SaveAssets();AssetDatabase.Refresh();
 Debug.Log("Created Mist, Splash and Foam particle materials.");
}
static void Make(string n,string tex,Color tint,float intensity,float soft){
 string p=M+"/"+n+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(p);
 var s=Shader.Find("WaterFX/URP Soft Particle"); if(!s){Debug.LogError("Missing WaterFX/URP Soft Particle shader");return;}
 if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,p);} else m.shader=s;
 m.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(T+"/"+tex));
 m.SetColor("_Tint",tint);m.SetFloat("_Intensity",intensity);m.SetFloat("_Softness",soft);
 m.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
 m.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
 EditorUtility.SetDirty(m);
}
static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;string par=Path.GetDirectoryName(p).Replace("\\","/");if(!AssetDatabase.IsValidFolder(par))Ensure(par);AssetDatabase.CreateFolder(par,Path.GetFileName(p));}
}
#endif