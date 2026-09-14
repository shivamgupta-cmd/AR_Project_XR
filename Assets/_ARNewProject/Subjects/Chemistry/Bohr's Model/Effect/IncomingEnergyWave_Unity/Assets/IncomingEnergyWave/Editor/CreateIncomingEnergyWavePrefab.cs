#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateIncomingEnergyWavePrefab
{
    [MenuItem("Tools/Incoming Energy Wave/Create Ready Prefab")]
    public static void CreatePrefab()
    {
        const string folder = "Assets/IncomingEnergyWaveGenerated";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "IncomingEnergyWaveGenerated");

        Material mat = CreateMaterial(folder);

        GameObject root = new GameObject("IncomingEnergyWave");
        LineRenderer line = root.AddComponent<LineRenderer>();
        IncomingEnergyWave wave = root.AddComponent<IncomingEnergyWave>();

        line.material = mat;
        line.useWorldSpace = true;
        line.positionCount = 48;
        line.startWidth = 0.025f;
        line.endWidth = 0.012f;
        line.numCapVertices = 4;
        line.textureMode = LineTextureMode.Stretch;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        GameObject impactGO = new GameObject("ImpactParticles");
        impactGO.transform.SetParent(root.transform);
        ParticleSystem impact = impactGO.AddComponent<ParticleSystem>();
        ParticleSystemRenderer pr = impactGO.GetComponent<ParticleSystemRenderer>();
        pr.material = mat;

        var main = impact.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.85f, 0.1f, 1f),
            new Color(1f, 0.2f, 0.02f, 1f)
        );
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = impact.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });

        var shape = impact.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.025f;

        wave.impactParticles = impact;

        string path = folder + "/IncomingEnergyWave.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    static Material CreateMaterial(string folder)
    {
        string path = folder + "/IncomingEnergyGlow.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;

        Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Particles/Additive");
        if (s == null) s = Shader.Find("Unlit/Color");

        m = new Material(s);
        m.name = "IncomingEnergyGlow";

        if (m.HasProperty("_BaseColor"))
            m.SetColor("_BaseColor", new Color(1f, 0.55f, 0.02f, 1f));
        if (m.HasProperty("_Color"))
            m.SetColor("_Color", new Color(1f, 0.55f, 0.02f, 1f));

        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
#endif
