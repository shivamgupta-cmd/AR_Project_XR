#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class GasBurstFXBuilder
{
    const string ROOT="Assets/GasBurstFX";
    const string GEN=ROOT+"/Generated";

    [MenuItem("Tools/Gas Burst FX/Create or Refresh Prefab")]
    public static void Build()
    {
        if(!AssetDatabase.IsValidFolder(GEN)) AssetDatabase.CreateFolder(ROOT,"Generated");
        Material gas=Mat(GEN+"/M_GasCloud.mat",ROOT+"/Textures/T_GasCloud.png",new Color(.55f,.78f,1f,.42f),false);
        Material spark=Mat(GEN+"/M_GasSparkle.mat",ROOT+"/Textures/T_GasSparkle.png",new Color(.65f,.9f,1f,.8f),true);

        GameObject root=new GameObject("GasBurstFX");
        Burst(root.transform,gas);
        Core(root.transform,gas);
        Sparkles(root.transform,spark);

        string path=GEN+"/GasBurstFX.prefab";
        if(AssetDatabase.LoadAssetAtPath<GameObject>(path)) AssetDatabase.DeleteAsset(path);
        PrefabUtility.SaveAsPrefabAsset(root,path);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Created "+path);
    }

    static Material Mat(string path,string texPath,Color c,bool additive)
    {
        Shader s=Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if(!s)s=Shader.Find(additive?"Legacy Shaders/Particles/Additive":"Legacy Shaders/Particles/Alpha Blended");
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,path);} else m.shader=s;
        Texture t=AssetDatabase.LoadAssetAtPath<Texture>(texPath);
        if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",t);
        if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",t);
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
        if(m.HasProperty("_Color"))m.SetColor("_Color",c);
        if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1);
        if(m.HasProperty("_ZWrite"))m.SetFloat("_ZWrite",0);
        m.renderQueue=3000; return m;
    }

    static ParticleSystem NewPS(string name,Transform parent,Material mat)
    {
        GameObject g=new GameObject(name); g.transform.SetParent(parent,false);
        ParticleSystem ps=g.AddComponent<ParticleSystem>();
        var r=g.GetComponent<ParticleSystemRenderer>(); r.material=mat;r.renderMode=ParticleSystemRenderMode.Billboard;
        return ps;
    }

    static void Burst(Transform p,Material mat)
    {
        var ps=NewPS("Gas_Burst_Expanding_Cloud",p,mat);
        var m=ps.main;m.loop=true;m.duration=3.5f;m.startLifetime=new ParticleSystem.MinMaxCurve(2.2f,4.2f);
        m.startSpeed=new ParticleSystem.MinMaxCurve(.18f,.55f);m.startSize=new ParticleSystem.MinMaxCurve(.28f,.75f);
        m.startRotation=new ParticleSystem.MinMaxCurve(0,6.28f);m.maxParticles=65;m.simulationSpace=ParticleSystemSimulationSpace.Local;
        var e=ps.emission;e.rateOverTime=9;
        var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=.18f;
        var n=ps.noise;n.enabled=true;n.strength=new ParticleSystem.MinMaxCurve(.18f,.42f);n.frequency=.35f;n.scrollSpeed=.18f;n.damping=true;
        var sz=ps.sizeOverLifetime;sz.enabled=true;
        sz.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.25f),new Keyframe(.4f,1),new Keyframe(1,1.5f)));
        var c=ps.colorOverLifetime;c.enabled=true; Gradient g=new Gradient();
        g.SetKeys(new[]{new GradientColorKey(new Color(.65f,.85f,1),0),new GradientColorKey(new Color(.4f,.65f,1),1)},
        new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.5f,.15f),new GradientAlphaKey(.32f,.65f),new GradientAlphaKey(0,1)});c.color=g;
    }

    static void Core(Transform p,Material mat)
    {
        var ps=NewPS("Gas_Dense_Core",p,mat);
        var m=ps.main;m.loop=true;m.startLifetime=new ParticleSystem.MinMaxCurve(2.8f,5f);
        m.startSpeed=new ParticleSystem.MinMaxCurve(.04f,.16f);m.startSize=new ParticleSystem.MinMaxCurve(.22f,.55f);m.maxParticles=55;
        var e=ps.emission;e.rateOverTime=8;
        var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=.25f;
        var v=ps.velocityOverLifetime;v.enabled=true;v.y=new ParticleSystem.MinMaxCurve(.03f,.16f);
        var n=ps.noise;n.enabled=true;n.strength=.2f;n.frequency=.3f;n.scrollSpeed=.12f;n.damping=true;
    }

    static void Sparkles(Transform p,Material mat)
    {
        var ps=NewPS("Gas_Energy_Sparkles",p,mat);
        var m=ps.main;m.loop=true;m.startLifetime=new ParticleSystem.MinMaxCurve(.8f,2f);
        m.startSpeed=new ParticleSystem.MinMaxCurve(.1f,.5f);m.startSize=new ParticleSystem.MinMaxCurve(.01f,.035f);m.maxParticles=100;
        var e=ps.emission;e.rateOverTime=15;
        var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=.28f;
        var n=ps.noise;n.enabled=true;n.strength=.08f;n.frequency=.6f;
        var c=ps.colorOverLifetime;c.enabled=true;Gradient g=new Gradient();
        g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.35f,.75f,1),1)},
        new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.25f),new GradientAlphaKey(0,1)});c.color=g;
    }
}
#endif