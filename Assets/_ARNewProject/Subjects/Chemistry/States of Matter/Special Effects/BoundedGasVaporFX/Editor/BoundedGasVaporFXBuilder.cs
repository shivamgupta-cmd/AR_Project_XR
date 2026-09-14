#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class BoundedGasVaporFXBuilder
{
    const string Root = "Assets/BoundedGasVaporFX";
    const string Gen = Root + "/Generated";
    const string Tex = Root + "/Textures/Gas_Cloud_Soft.png";

    [InitializeOnLoadMethod]
    static void AutoBuild()
    {
        EditorApplication.delayCall += Build;
    }

    [MenuItem("Tools/Bounded Gas Vapor FX/Create or Refresh Prefab")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Root)) return;
        if (!AssetDatabase.IsValidFolder(Gen))
            AssetDatabase.CreateFolder(Root, "Generated");

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        string matPath = Gen + "/M_BoundedGasVapor.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null) {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        } else mat.shader = shader;

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Tex);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
        Color tint = new Color(0.88f,0.93f,1f,0.20f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface",1);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite",0);
        mat.renderQueue=3000;
        EditorUtility.SetDirty(mat);

        GameObject root = new GameObject("BoundedGasVapor");
        CreateLayer(root.transform, "Cloud_Core", mat, 0.31f, 13f, 65, .18f,.34f, 4f,6f, .006f, .035f);
        CreateLayer(root.transform, "Cloud_Wisps", mat, 0.37f, 10f, 50, .10f,.22f, 3f,5f, .012f, .055f);

        PrefabUtility.SaveAsPrefabAsset(root, Gen + "/BoundedGasVapor.prefab");
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Bounded Gas Vapor FX ready: " + Gen + "/BoundedGasVapor.prefab");
    }

    static void CreateLayer(Transform parent,string name,Material mat,float radius,float rate,int max,
        float minSize,float maxSize,float minLife,float maxLife,float speed,float noiseStrength)
    {
        GameObject go=new GameObject(name);
        go.transform.SetParent(parent,false);
        ParticleSystem ps=go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer rr=go.GetComponent<ParticleSystemRenderer>();
        rr.renderMode=ParticleSystemRenderMode.Billboard;
        rr.material=mat;
        rr.sortMode=ParticleSystemSortMode.Distance;

        var m=ps.main;
        m.loop=true; m.playOnAwake=true; m.duration=5;
        m.startLifetime=new ParticleSystem.MinMaxCurve(minLife,maxLife);
        m.startSpeed=new ParticleSystem.MinMaxCurve(0,speed);
        m.startSize=new ParticleSystem.MinMaxCurve(minSize,maxSize);
        m.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
        m.maxParticles=max;
        m.simulationSpace=ParticleSystemSimulationSpace.Local;
        m.gravityModifier=0;
        m.startColor=new ParticleSystem.MinMaxGradient(
            new Color(.85f,.91f,1f,.10f), new Color(1f,1f,1f,.23f));

        var e=ps.emission; e.enabled=true; e.rateOverTime=rate;

        var s=ps.shape; s.enabled=true;
        s.shapeType=ParticleSystemShapeType.Sphere;
        s.radius=radius;
        s.radiusThickness=1f;

        // Tiny internal motion only; no upward stream.
        var v=ps.velocityOverLifetime; v.enabled=true;
        v.space=ParticleSystemSimulationSpace.Local;
        v.x=new ParticleSystem.MinMaxCurve(-.005f,.005f);
        v.y=new ParticleSystem.MinMaxCurve(-.003f,.006f);
        v.z=new ParticleSystem.MinMaxCurve(-.005f,.005f);

        var n=ps.noise; n.enabled=true; n.separateAxes=true;
        n.strengthX=new ParticleSystem.MinMaxCurve(noiseStrength*.75f,noiseStrength);
        n.strengthY=new ParticleSystem.MinMaxCurve(noiseStrength*.45f,noiseStrength*.75f);
        n.strengthZ=new ParticleSystem.MinMaxCurve(noiseStrength*.75f,noiseStrength);
        n.frequency=.38f; n.scrollSpeed=.08f; n.damping=true;
        n.quality=ParticleSystemNoiseQuality.High;

        var col=ps.colorOverLifetime; col.enabled=true;
        Gradient g=new Gradient();
        g.SetKeys(
            new[]{new GradientColorKey(new Color(.86f,.92f,1),0),
                  new GradientColorKey(Color.white,.5f),
                  new GradientColorKey(new Color(.86f,.92f,1),1)},
            new[]{new GradientAlphaKey(0,0),
                  new GradientAlphaKey(.25f,.16f),
                  new GradientAlphaKey(.18f,.68f),
                  new GradientAlphaKey(0,1)});
        col.color=g;

        var sz=ps.sizeOverLifetime; sz.enabled=true;
        AnimationCurve curve=new AnimationCurve(
            new Keyframe(0,.55f),new Keyframe(.45f,1f),new Keyframe(1,1.12f));
        sz.size=new ParticleSystem.MinMaxCurve(1,curve);

        var rot=ps.rotationOverLifetime; rot.enabled=true;
        rot.z=new ParticleSystem.MinMaxCurve(-.18f,.18f);
    }
}
#endif
