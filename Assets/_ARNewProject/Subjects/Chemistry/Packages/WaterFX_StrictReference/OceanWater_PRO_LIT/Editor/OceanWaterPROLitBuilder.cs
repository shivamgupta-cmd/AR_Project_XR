#if UNITY_EDITOR
using UnityEditor; using UnityEngine; using System.IO;
public static class OceanWaterPROLitBuilder{
const string R="Assets/OceanWater_PRO_LIT",M=R+"/Materials",T=R+"/Textures";
[MenuItem("Tools/Ocean Water PRO/Create Ocean Material")]
public static void Create(){AssetDatabase.Refresh();Ensure(M);Cfg(T+"/Ocean_Normal.png",true);Cfg(T+"/Ocean_Foam.png",false);var s=Shader.Find("Custom/OceanWaterPROLit");if(!s){Debug.LogError("Shader not found.");return;}string p=M+"/M_OceanWater_PRO_LIT.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,p);}else m.shader=s;m.SetTexture("_NormalMap",AssetDatabase.LoadAssetAtPath<Texture2D>(T+"/Ocean_Normal.png"));m.SetTexture("_FoamTex",AssetDatabase.LoadAssetAtPath<Texture2D>(T+"/Ocean_Foam.png"));EditorUtility.SetDirty(m);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Selection.activeObject=m;EditorGUIUtility.PingObject(m);}
static void Cfg(string p,bool normal){var i=AssetImporter.GetAtPath(p) as TextureImporter;if(!i)return;i.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;i.wrapMode=TextureWrapMode.Repeat;i.filterMode=FilterMode.Trilinear;i.mipmapEnabled=true;i.SaveAndReimport();}
static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;string par=Path.GetDirectoryName(p).Replace("\\","/");if(!AssetDatabase.IsValidFolder(par))Ensure(par);AssetDatabase.CreateFolder(par,Path.GetFileName(p));}}
#endif