#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class RutherfordCompleteActivityBuilder
{
    const string R="Assets/RutherfordCompleteActivity";
    [MenuItem("Tools/Rutherford Model/Create COMPLETE Activity")]
    public static void Build()
    {
        Ensure();
        GameObject modelAsset=AssetDatabase.LoadAssetAtPath<GameObject>(R+"/Models/Ruthefords_Model.fbx");
        if(!modelAsset){EditorUtility.DisplayDialog("Missing Model","Ruthefords_Model.fbx was not found.","OK");return;}

        GameObject root=new GameObject("RUTHERFORD_COMPLETE_ACTIVITY");
        GameObject model=(GameObject)PrefabUtility.InstantiatePrefab(modelAsset,root.transform);
        model.name="Rutherford_Model";

        // IMPORTANT:
        // Imported FBX objects are Prefab instances. Unity blocks reparenting
        // Electrons_01..06, pPipe1..6, Rings, etc. while they remain linked.
        // Completely unpack the instantiated FBX before changing its hierarchy.
        PrefabUtility.UnpackPrefabInstance(
            model,
            PrefabUnpackMode.Completely,
            InteractionMode.AutomatedAction
        );

        var c=root.AddComponent<RutherfordActivityController>();
        c.nucleus=Find(model.transform,"Nucleus");
        c.electronsRoot=GroupMatches(root.transform,model.transform,"Electrons_","Electrons_Group");
        c.ringsRoot=GroupMatches(root.transform,model.transform,"pPipe","Rings_Group");
        GameObject rings=Find(model.transform,"Rings"); if(rings) rings.transform.SetParent(c.ringsRoot.transform,true);
        c.laser=Find(model.transform,"Laser");
        c.goldFoil=Find(model.transform,"Gold_foil");
        c.scatterExperiment=Find(model.transform,"Scatter_experiment");
        if(c.goldFoil)c.goldFoilRenderer=c.goldFoil.GetComponentInChildren<Renderer>();

        Material alpha=Mat("AlphaParticle_Mat","Alpha_Glow.png",new Color(1,.42f,.03f,.95f),2.3f);
        Material green=Mat("PassThrough_Mat","Alpha_Glow.png",new Color(.15f,1,.25f,.8f),1.8f);
        Material impact=Mat("Impact_Mat","Impact_Spark.png",new Color(1,.65f,.08f,.95f),2.8f);
        Material blue=Mat("NucleusPulse_Mat","Impact_Spark.png",new Color(.1f,.55f,1,.8f),2.2f);

        // Positions are local activity-space defaults; move the FX roots to match your exact imported apparatus once.
        c.alphaBeam=Beam(root.transform,"FX_01_ALPHA_INCOMING",alpha,new Vector3(-2,0,0),new Vector3(1,0,0),85,1.5f,.025f);
        c.passThrough=Beam(root.transform,"FX_02_PASS_STRAIGHT_MOST",green,new Vector3(0,0,0),new Vector3(1,0,0),65,1.6f,.022f);
        c.slightDeflection=Scatter(root.transform,"FX_03_SLIGHT_DEFLECTION_SOME",alpha,32,22f);
        c.backScatter=Scatter(root.transform,"FX_04_BACKSCATTER_VERY_FEW",impact,8,145f);
        c.foilImpact=Burst(root.transform,"FX_05_GOLD_FOIL_IMPACT",impact,24,.08f,.13f);
        c.nucleusPulse=Burst(root.transform,"FX_06_NUCLEUS_DISCOVERY_PULSE",blue,42,.04f,.18f);
        c.discoveryBurst=Burst(root.transform,"FX_07_DISCOVERY_ENERGY_RING",impact,60,.05f,.22f);

        if(c.goldFoilRenderer){
            Material gm=new Material(Shader.Find("RutherfordFX/GoldFoilPulseURP"));
            AssetDatabase.CreateAsset(gm,R+"/Materials/GoldFoil_Animated.mat");
            c.goldFoilRenderer.sharedMaterial=gm;
        }

        string p=R+"/Prefabs/Rutherford_COMPLETE_Activity.prefab";
        PrefabUtility.SaveAsPrefabAsset(root,p); Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(p);
        EditorUtility.DisplayDialog("Rutherford Activity Created",
        "The uploaded FBX was analyzed and used.\n\nDetected model parts include:\nNucleus, Nucleus1, Electrons_01..06, Rings, pPipe1..6, Laser, Gold_foil and Scatter_experiment.\n\nGenerated a complete activity prefab with alpha-particle beam, straight-through particles, deflection, backscatter, foil impact, nucleus pulse and discovery burst.\n\nOpen the prefab and align FX roots to the exact apparatus positions if required.","OK");
    }

    static GameObject Find(Transform root,string n){foreach(Transform t in root.GetComponentsInChildren<Transform>(true))if(t.name==n)return t.gameObject;return null;}
    static GameObject GroupMatches(Transform activity,Transform model,string prefix,string groupName){
        GameObject g=new GameObject(groupName);
        g.transform.SetParent(activity,false);

        // Cache the matching children BEFORE changing the hierarchy.
        Transform[] all=model.GetComponentsInChildren<Transform>(true);
        System.Collections.Generic.List<Transform> matches=
            new System.Collections.Generic.List<Transform>();

        foreach(Transform t in all)
        {
            if(t != model && t.name.StartsWith(prefix))
                matches.Add(t);
        }

        foreach(Transform t in matches)
            t.SetParent(g.transform,true);

        return g;
    }
    static ParticleSystem Beam(Transform par,string n,Material mat,Vector3 pos,Vector3 dir,float rate,float life,float size){
        GameObject g=new GameObject(n);g.transform.SetParent(par,false);g.transform.localPosition=pos;
        var p=g.AddComponent<ParticleSystem>();var m=p.main;m.loop=true;m.playOnAwake=false;m.startLifetime=life;m.startSpeed=2.4f;m.startSize=new ParticleSystem.MinMaxCurve(size*.65f,size*1.5f);m.maxParticles=350;
        var e=p.emission;e.rateOverTime=rate;var sh=p.shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=1.2f;sh.radius=.035f;
        g.transform.rotation=Quaternion.FromToRotation(Vector3.forward,dir);
        var tr=p.trails;tr.enabled=true;tr.lifetime=.18f;tr.ratio=.85f;
        var r=g.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=mat;r.trailMaterial=mat;return p;
    }
    static ParticleSystem Scatter(Transform par,string n,Material mat,float rate,float angle){
        GameObject g=new GameObject(n);g.transform.SetParent(par,false);
        var p=g.AddComponent<ParticleSystem>();var m=p.main;m.loop=true;m.playOnAwake=false;m.startLifetime=1.2f;m.startSpeed=new ParticleSystem.MinMaxCurve(1.8f,2.8f);m.startSize=new ParticleSystem.MinMaxCurve(.018f,.04f);m.maxParticles=180;
        var e=p.emission;e.rateOverTime=rate;var sh=p.shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=angle;sh.radius=.025f;
        var tr=p.trails;tr.enabled=true;tr.lifetime=.22f;tr.ratio=1;
        var r=g.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=mat;r.trailMaterial=mat;return p;
    }
    static ParticleSystem Burst(Transform par,string n,Material mat,int count,float s0,float s1){
        GameObject g=new GameObject(n);g.transform.SetParent(par,false);
        var p=g.AddComponent<ParticleSystem>();var m=p.main;m.loop=false;m.playOnAwake=false;m.startLifetime=new ParticleSystem.MinMaxCurve(.25f,.65f);m.startSpeed=new ParticleSystem.MinMaxCurve(.25f,1.1f);m.startSize=new ParticleSystem.MinMaxCurve(s0,s1);
        var e=p.emission;e.rateOverTime=0;e.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
        var sh=p.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=.08f;
        var r=g.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=mat;return p;
    }
    static Material Mat(string n,string tex,Color c,float glow){var m=new Material(Shader.Find("RutherfordFX/GlowURP"));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(R+"/Textures/"+tex));m.SetColor("_BaseColor",c);m.SetFloat("_Glow",glow);AssetDatabase.CreateAsset(m,R+"/Materials/"+n+".mat");return m;}
    static void Ensure(){foreach(string f in new[]{"Materials","Prefabs"})if(!AssetDatabase.IsValidFolder(R+"/"+f))AssetDatabase.CreateFolder(R,f);}
}
#endif