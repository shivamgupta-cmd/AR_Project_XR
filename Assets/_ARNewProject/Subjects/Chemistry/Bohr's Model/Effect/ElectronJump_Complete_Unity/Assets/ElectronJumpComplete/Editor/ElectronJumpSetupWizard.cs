#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ElectronJumpSetupWizard
{
    const string GeneratedFolder = "Assets/ElectronJumpComplete/Generated";

    [MenuItem("Tools/Bohr Electron Jump/Setup Selected Electron")]
    public static void SetupSelectedElectron()
    {
        GameObject electron = Selection.activeGameObject;

        if (electron == null)
        {
            EditorUtility.DisplayDialog(
                "Bohr Electron Jump",
                "Select your Electron GameObject first.",
                "OK"
            );
            return;
        }

        EnsureGeneratedFolder();

        ElectronEnergyJump jump = electron.GetComponent<ElectronEnergyJump>();
        if (jump == null)
            jump = electron.AddComponent<ElectronEnergyJump>();

        Renderer electronRenderer = electron.GetComponent<Renderer>();
        if (electronRenderer == null)
            electronRenderer = electron.GetComponentInChildren<Renderer>();

        jump.electronRenderer = electronRenderer;

        // -------------------------
        // TRAIL RENDERER
        // -------------------------
        TrailRenderer trail = electron.GetComponent<TrailRenderer>();
        if (trail == null)
            trail = electron.AddComponent<TrailRenderer>();

        Material trailMaterial = CreateTrailMaterial();

        trail.material = trailMaterial;
        trail.time = 0.4f;
        trail.minVertexDistance = 0.01f;
        trail.startWidth = 0.035f;
        trail.endWidth = 0f;
        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;
        trail.numCapVertices = 4;
        trail.numCornerVertices = 2;
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.emitting = false;

        Gradient trailGradient = new Gradient();
        trailGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.15f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.1f, 0.45f, 1f), 0.65f),
                new GradientColorKey(new Color(0.35f, 0.05f, 1f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.75f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = trailGradient;

        jump.trail = trail;

        // -------------------------
        // ARRIVAL PARTICLES
        // -------------------------
        Transform existingArrival = electron.transform.Find("ElectronArrivalParticles");
        GameObject arrivalGO;

        if (existingArrival != null)
            arrivalGO = existingArrival.gameObject;
        else
        {
            arrivalGO = new GameObject("ElectronArrivalParticles");
            arrivalGO.transform.SetParent(electron.transform, false);
        }

        ParticleSystem arrival = arrivalGO.GetComponent<ParticleSystem>();
        if (arrival == null)
            arrival = arrivalGO.AddComponent<ParticleSystem>();

        ParticleSystemRenderer psRenderer =
            arrivalGO.GetComponent<ParticleSystemRenderer>();

        psRenderer.material = CreateParticleMaterial();

        var main = arrival.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.075f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.2f, 1f, 1f, 1f),
            new Color(0.35f, 0.15f, 1f, 1f)
        );
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = arrival.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 22)
        });

        var shape = arrival.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.035f;

        var colorOverLifetime = arrival.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient particleGradient = new Gradient();
        particleGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.5f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.1f, 0.4f, 1f), 0.6f),
                new GradientColorKey(new Color(0.45f, 0.08f, 1f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = particleGradient;

        var sizeOverLifetime = arrival.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.25f),
            new Keyframe(0.2f, 1f),
            new Keyframe(1f, 0f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        arrival.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        jump.arrivalParticles = arrival;

        // -------------------------
        // HIGHER ORBIT POINT
        // -------------------------
        GameObject point = GameObject.Find("HigherOrbitPoint");
        if (point == null)
        {
            point = new GameObject("HigherOrbitPoint");
            point.transform.position = electron.transform.position + Vector3.up * 0.5f;
        }

        jump.higherOrbitPoint = point.transform;

        EditorUtility.SetDirty(electron);
        EditorUtility.SetDirty(jump);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = point;

        EditorUtility.DisplayDialog(
            "Bohr Electron Jump",
            "Setup complete.\n\n" +
            "Created/configured:\n" +
            "- ElectronEnergyJump\n" +
            "- TrailRenderer\n" +
            "- Glow trail material\n" +
            "- Arrival particle burst\n" +
            "- HigherOrbitPoint\n\n" +
            "Now move HigherOrbitPoint onto the exact higher shell position.",
            "OK"
        );
    }

    static void EnsureGeneratedFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ElectronJumpComplete"))
            AssetDatabase.CreateFolder("Assets", "ElectronJumpComplete");

        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            AssetDatabase.CreateFolder("Assets/ElectronJumpComplete", "Generated");
    }

    static Material CreateTrailMaterial()
    {
        string path = GeneratedFolder + "/ElectronJumpTrail.mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Particles/Additive") ??
            Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.name = "ElectronJumpTrail";

        Color glow = new Color(0.1f, 0.8f, 2.5f, 1f);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", glow);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", glow);

        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f);

        if (mat.HasProperty("_ZWrite"))
            mat.SetFloat("_ZWrite", 0f);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static Material CreateParticleMaterial()
    {
        string path = GeneratedFolder + "/ElectronArrivalParticles.mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Particles/Additive") ??
            Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.name = "ElectronArrivalParticles";

        Color glow = new Color(0.15f, 0.85f, 2.5f, 1f);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", glow);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", glow);

        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f);

        if (mat.HasProperty("_ZWrite"))
            mat.SetFloat("_ZWrite", 0f);

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
#endif
