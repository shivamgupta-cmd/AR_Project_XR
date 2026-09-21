#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class CompassARCompleteBuilder
{
    const string R="Assets/CompassARComplete";

    [MenuItem("Tools/Compass AR/Create Complete Demo")]
    public static void Create()
    {
        AssetDatabase.Refresh();
        CreateSkybox();

        GameObject root=new GameObject("COMPASS_AR_COMPLETE");
        var ctrl=root.AddComponent<CompassLessonController>();

        GameObject model=InstantiateModel(root.transform);
        ctrl.compassRoot=model?model.transform:null;

        GameObject vfx=new GameObject("VFX"); vfx.transform.SetParent(root.transform);
        ctrl.ambientDust=MakePS(vfx.transform,"Ambient_Dust",R+"/Textures/Particle_Dust.png",new Color(1f,.75f,.35f,.28f),70,.04f,1.6f,2.5f);
        ctrl.magneticParticles=MakePS(vfx.transform,"Magnetic_Field_Particles",R+"/Textures/Particle_Field.png",new Color(.15f,.75f,1f,.8f),45,.05f,1.0f,1.8f);
        ctrl.northPulse=MakePS(vfx.transform,"North_Pulse",R+"/Textures/Particle_Spark.png",new Color(.15f,.8f,1f,1f),18,.12f,.45f,.8f);
        ctrl.successBurst=MakePS(vfx.transform,"Success_Burst",R+"/Textures/Particle_Spark.png",new Color(.2f,1f,.4f,1f),30,.15f,.8f,1.1f);

        ctrl.magneticParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        ctrl.northPulse.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        ctrl.successBurst.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

        GameObject groups=new GameObject("STORY_GROUPS"); groups.transform.SetParent(root.transform);
        ctrl.introGroup=MakeGroup(groups.transform,"01_Introduction","WELCOME","Discover Directions in a 3D World");
        ctrl.partsGroup=MakeGroup(groups.transform,"02_Compass_Parts","MEET THE COMPASS","Tap parts to learn");
        ctrl.magneticFieldGroup=MakeGroup(groups.transform,"03_Magnetic_Field","HOW IT WORKS","Earth's Invisible Magnetic Field");
        ctrl.directionsGroup=MakeGroup(groups.transform,"04_Directions","READ DIRECTIONS","N  E  S  W");
        ctrl.mapGroup=MakeGroup(groups.transform,"05_Map_Navigation","USE WITH A MAP","Align North and Navigate");
        ctrl.activityGroup=MakeGroup(groups.transform,"06_Activity","TRY IT YOURSELF","Find the Direction");
        ctrl.successGroup=MakeGroup(groups.transform,"07_Success","YOU'RE READY!","Explore • Navigate • Discover");

        GameObject north=new GameObject("North_Target"); north.transform.SetParent(root.transform); north.transform.localPosition=new Vector3(0,0,4); ctrl.northTarget=north.transform;

        var audio=root.AddComponent<AudioSource>(); audio.playOnAwake=false; ctrl.voiceSource=audio;
        ctrl.sceneVoiceClips=new AudioClip[9];

        string prefab=R+"/Prefabs/Compass_AR_COMPLETE.prefab";
        Ensure(R+"/Prefabs");
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,prefab,InteractionMode.UserAction);
        Selection.activeGameObject=root;
        Debug.Log("Compass complete demo created. Assign the needle Transform in CompassLessonController after checking your FBX hierarchy.");
    }

    static GameObject InstantiateModel(Transform parent)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(R+"/Models/Compass.fbx");
        if(!prefab){Debug.LogError("Compass.fbx missing");return null;}
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name="Compass_3D_Model"; go.transform.SetParent(parent,false);
        return go;
    }

    static GameObject MakeGroup(Transform parent,string name,string title,string subtitle)
    {
        var g=new GameObject(name); g.transform.SetParent(parent,false);
        var canvasGO=new GameObject("WorldSpace_UI");
        canvasGO.transform.SetParent(g.transform,false);
        var canvas=canvasGO.AddComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace;
        var scaler=canvasGO.AddComponent<CanvasScaler>(); scaler.dynamicPixelsPerUnit=10;
        canvasGO.transform.localPosition=new Vector3(0,1.5f,0); canvasGO.transform.localScale=Vector3.one*.0022f;
        var rt=canvasGO.GetComponent<RectTransform>(); rt.sizeDelta=new Vector2(1100,220);

        var bg=new GameObject("Panel"); bg.transform.SetParent(canvasGO.transform,false);
        var br=bg.AddComponent<RectTransform>(); br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;
        var bi=bg.AddComponent<Image>(); bi.sprite=LoadSprite(R+"/Sprites/UI_InstructionPanel.png"); bi.type=Image.Type.Sliced;

        var txt=new GameObject("Title"); txt.transform.SetParent(bg.transform,false);
        var tr=txt.AddComponent<RectTransform>();tr.anchorMin=new Vector2(.04f,.25f);tr.anchorMax=new Vector2(.96f,.88f);tr.offsetMin=tr.offsetMax=Vector2.zero;
        var tmp=txt.AddComponent<TextMeshProUGUI>(); tmp.text=title+"\n<size=55%>"+subtitle+"</size>"; tmp.alignment=TextAlignmentOptions.Center; tmp.fontSize=54; tmp.color=Color.white;
        return g;
    }

    static ParticleSystem MakePS(Transform parent,string name,string tex,Color color,int max,float size,float speed,float life)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);
        var ps=go.AddComponent<ParticleSystem>();
        var main=ps.main; main.loop=true;main.startLifetime=life;main.startSpeed=speed;main.startSize=size;main.startColor=color;main.maxParticles=max;
        var em=ps.emission;em.rateOverTime=max/2f;
        var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=1.2f;
        var rend=ps.GetComponent<ParticleSystemRenderer>();rend.renderMode=ParticleSystemRenderMode.Billboard;
        var shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if(shader){
            var mat=new Material(shader); mat.name=name+"_MAT";
            var t=AssetDatabase.LoadAssetAtPath<Texture2D>(tex); mat.mainTexture=t;
            mat.SetColor("_BaseColor",Color.white);
            Ensure(R+"/Materials");
            string mp=R+"/Materials/"+name+"_MAT.mat";
            AssetDatabase.CreateAsset(mat,mp);rend.sharedMaterial=mat;
        }
        return ps;
    }

    static Sprite LoadSprite(string p){return AssetDatabase.LoadAssetAtPath<Sprite>(p);}

    static void CreateSkybox()
    {
        var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(R+"/Textures/Compass_Mountain_Skybox_Panorama.jpg");
        var s=Shader.Find("Skybox/Panoramic"); if(!s)return;
        string p=R+"/Materials/M_Compass_Mountain_Skybox.mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(p);
        if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,p);}
        m.SetTexture("_MainTex",tex);m.SetFloat("_Exposure",1.1f);RenderSettings.skybox=m;EditorUtility.SetDirty(m);
    }

    static void Ensure(string p)
    {
        if(AssetDatabase.IsValidFolder(p))return;
        string par=Path.GetDirectoryName(p).Replace("\\","/");
        if(!AssetDatabase.IsValidFolder(par))Ensure(par);
        AssetDatabase.CreateFolder(par,Path.GetFileName(p));
    }
}
#endif