#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ThomsonModelFXBuilder
{
    const string Root = "Assets/ThomsonModelFX/Generated";
    const string MatRoot = "Assets/ThomsonModelFX/Generated/Materials";
    const string TexRoot = "Assets/ThomsonModelFX/Textures/";

    [MenuItem("Tools/Thomson Model FX/Create Complete FX Rig")]
    public static void Build()
    {
        EnsureFolder("Assets/ThomsonModelFX/Generated");
        EnsureFolder(MatRoot);

        Material ringMat = CreateAdditiveMaterial("M_EnergyRing", "T_EnergyRing.png", new Color(0.15f,0.55f,1f,1f), 3.5f);
        Material sparkleMat = CreateAdditiveMaterial("M_Sparkle", "T_Sparkle.png", new Color(0.55f,0.25f,1f,1f), 3.0f);
        Material positiveParticleMat = CreateAdditiveMaterial("M_PositiveParticles", "T_SoftCircle.png", new Color(1f,0.15f,0.95f,1f), 2.6f);
        Material bgMat = CreateAdditiveMaterial("M_BackgroundSparkles", "T_Sparkle.png", new Color(0.15f,0.45f,1f,1f), 2.2f);
        Material electronBurstMat = CreateAdditiveMaterial("M_ElectronBurst", "T_ElectronGlow.png", new Color(0.1f,0.6f,1f,1f), 2.8f);
        CreatePositiveSphereMaterial();

        GameObject root = new GameObject("ThomsonModelFX_Rig");
        var controller = root.AddComponent<ThomsonModelFXController>();

        GameObject rings = new GameObject("Energy_Rings"); rings.transform.SetParent(root.transform,false); controller.energyRings=rings;
        for(int i=0;i<3;i++){
            var r=GameObject.CreatePrimitive(PrimitiveType.Quad); r.name="EnergyRing_"+(i+1); r.transform.SetParent(rings.transform,false);
            r.transform.localRotation = Quaternion.Euler(90f,0f,0f);
            r.transform.localScale=new Vector3(2.1f+i*.30f,2.1f+i*.30f,1f);
            Object.DestroyImmediate(r.GetComponent<Collider>());
            r.GetComponent<Renderer>().sharedMaterial = ringMat;
            var rot=r.AddComponent<EnergyRingRotator>(); rot.speed=(i%2==0?1:-1)*(18+i*8); rot.pulseAmount=.025f;
        }

        controller.innerParticles = MakeParticles("Inner_Positive_Charge_Particles", root.transform, 70, .025f, 1.3f, new Color(1f,.1f,1f,1f), .02f, positiveParticleMat, .8f);
        controller.backgroundParticles = MakeParticles("Background_Sparkles", root.transform, 45, .018f, 2.5f, new Color(.3f,.5f,1f,1f), .015f, bgMat, 2.2f);
        controller.chargeParticles = MakeParticles("Positive_Charge_Sparkles", root.transform, 35, .02f, 1.8f, new Color(1f,.2f,.8f,1f), .012f, sparkleMat, .85f);
        controller.electronPulseParticles = MakeParticles("Electron_Tap_Burst", root.transform, 0, .05f, .45f, new Color(.1f,.6f,1f,1f), .025f, electronBurstMat, .25f);
        var em=controller.electronPulseParticles.emission; em.rateOverTime=0; em.SetBursts(new[]{new ParticleSystem.Burst(0,12)});

        string path=Root+"/ThomsonModelFX_Rig.prefab";
        PrefabUtility.SaveAsPrefabAsset(root,path); Object.DestroyImmediate(root); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Debug.Log("Thomson Model FX created with materials and textures: "+path);
    }

    static ParticleSystem MakeParticles(string name, Transform parent, float rate, float size, float life, Color color, float speed, Material mat, float radius)
    {
        GameObject g=new GameObject(name); g.transform.SetParent(parent,false); var ps=g.AddComponent<ParticleSystem>();
        var main=ps.main; main.loop=true; main.playOnAwake=false; main.startLifetime=life; main.startSpeed=speed; main.startSize=size; main.startColor=color; main.simulationSpace=ParticleSystemSimulationSpace.Local; main.maxParticles=300;
        var em=ps.emission; em.rateOverTime=rate;
        var shape=ps.shape; shape.enabled=true; shape.shapeType=ParticleSystemShapeType.Sphere; shape.radius=radius;
        var noise=ps.noise; noise.enabled=true; noise.strength=.04f; noise.frequency=.5f; noise.scrollSpeed=.2f;
        var rend=ps.GetComponent<ParticleSystemRenderer>(); rend.renderMode=ParticleSystemRenderMode.Billboard; rend.sharedMaterial=mat;
        return ps;
    }

    static Material CreateAdditiveMaterial(string name, string textureName, Color color, float intensity)
    {
        string path=MatRoot+"/"+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){ Shader s=Shader.Find("ThomsonModelFX/AdditiveTexture"); m=new Material(s); AssetDatabase.CreateAsset(m,path); }
        Texture2D t=AssetDatabase.LoadAssetAtPath<Texture2D>(TexRoot+textureName); m.SetTexture("_MainTex",t); m.SetColor("_Color",color); m.SetFloat("_Intensity",intensity); EditorUtility.SetDirty(m); return m;
    }

    static void CreatePositiveSphereMaterial()
    {
        string path=MatRoot+"/M_PositiveSphere.mat"; Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){ m=new Material(Shader.Find("ThomsonModelFX/PositiveSphereTextured")); AssetDatabase.CreateAsset(m,path); }
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(TexRoot+"T_PositiveSpherePattern.png"));
        m.SetColor("_BaseColor",new Color(.38f,.02f,.58f,.32f)); m.SetColor("_EmissionColor",new Color(1.5f,.06f,2.2f,1f)); m.SetFloat("_FresnelPower",3.2f); m.SetFloat("_PatternStrength",.65f); m.SetFloat("_Alpha",.32f); EditorUtility.SetDirty(m);
    }

    static void EnsureFolder(string p){ if(!Directory.Exists(p)) Directory.CreateDirectory(p); }
}
#endif
