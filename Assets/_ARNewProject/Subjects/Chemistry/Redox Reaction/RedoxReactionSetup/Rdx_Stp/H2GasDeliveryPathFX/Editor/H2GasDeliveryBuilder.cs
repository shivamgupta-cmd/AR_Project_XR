#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class H2GasDeliveryBuilder : EditorWindow
{
    [MenuItem("Tools/H2 Gas FX/Create Delivery Path Rig")]
    public static void Open()
    {
        GetWindow<H2GasDeliveryBuilder>("H2 Gas Delivery");
    }

    void OnGUI()
    {
        GUILayout.Label("H2 GAS DELIVERY PATH FX", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Creates a path-based H2 gas visualization similar to your CO2 bubble delivery setup.\n\n" +
            "Move the path markers to match the real cylinder hose and tube inlet in your scene.",
            MessageType.Info);

        if (GUILayout.Button("CREATE H2 DELIVERY RIG", GUILayout.Height(42)))
            Build();
    }

    static void Build()
    {
        const string generated = "Assets/H2GasDeliveryPathFX/Generated";
        EnsureFolder(generated);
        EnsureFolder(generated + "/Materials");

        Shader shader = Shader.Find("H2GasDeliveryPathFX/URPSoftGas");
        if (shader == null)
        {
            EditorUtility.DisplayDialog(
                "H2 Gas FX",
                "The included H2 shader has not compiled yet. Wait for Unity to finish compiling and try again.",
                "OK");
            return;
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/H2GasDeliveryPathFX/Textures/H2_SoftParticle.png");

        if (tex == null)
        {
            EditorUtility.DisplayDialog(
                "H2 Gas FX",
                "H2 particle texture is missing.",
                "OK");
            return;
        }

        string matPath = generated + "/Materials/M_H2_Gas.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
            AssetDatabase.DeleteAsset(matPath);

        Material mat = new Material(shader);
        mat.name = "M_H2_Gas";
        mat.SetTexture("_BaseMap", tex);
        mat.SetColor("_BaseColor", new Color(0.80f,0.95f,1.0f,0.42f));
        mat.SetFloat("_EmissionStrength", 1.15f);
        AssetDatabase.CreateAsset(mat, matPath);

        GameObject old = GameObject.Find("H2_Delivery_Path_FX");
        if (old != null) DestroyImmediate(old);

        GameObject root = new GameObject("H2_Delivery_Path_FX");

        // ---------------- Path Markers ----------------
        GameObject pathRoot = new GameObject("PATH_POINTS_MOVE_THESE");
        pathRoot.transform.SetParent(root.transform, false);

        Transform p0 = Marker("P0_H2_Cylinder_Outlet", pathRoot.transform, new Vector3(-1.25f,0.55f,0));
        Transform p1 = Marker("P1_Regulator_Hose", pathRoot.transform, new Vector3(-0.95f,0.55f,0));
        Transform p2 = Marker("P2_Hose_Down_Bend", pathRoot.transform, new Vector3(-0.75f,0.18f,0));
        Transform p3 = Marker("P3_Hose_Bottom", pathRoot.transform, new Vector3(-0.45f,0.05f,0));
        Transform p4 = Marker("P4_TestTube_Inlet", pathRoot.transform, new Vector3(-0.12f,0.22f,0));
        Transform p5 = Marker("P5_Inside_Reaction_Tube", pathRoot.transform, new Vector3(0.55f,0.22f,0));

        // ---------------- Travelling Gas ----------------
        GameObject gasObj = new GameObject("H2_Gas_Travel_Path");
        gasObj.transform.SetParent(root.transform, false);

        ParticleSystem ps = gasObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 3.2f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.045f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.86f,0.97f,1f,0.28f),
            new Color(0.70f,0.92f,1f,0.50f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 110;

        var emission = ps.emission;
        emission.rateOverTime = 14f;

        var shape = ps.shape;
        shape.enabled = false;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.012f;
        noise.frequency = 0.65f;
        noise.scrollSpeed = 0.20f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.78f,0.94f,1f),0f),
                new GradientColorKey(new Color(0.92f,0.98f,1f),1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f,0f),
                new GradientAlphaKey(0.46f,0.12f),
                new GradientAlphaKey(0.36f,0.82f),
                new GradientAlphaKey(0f,1f)
            });
        color.color = g;

        var renderer = gasObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = mat;
        renderer.sortingFudge = 0.1f;

        H2GasPathFollower follower = gasObj.AddComponent<H2GasPathFollower>();
        follower.pathPoints = new Transform[] { p0,p1,p2,p3,p4,p5 };
        follower.travelTime = 3.0f;
        follower.loop = true;
        follower.randomPathRadius = 0.018f;
        follower.wobbleAmount = 0.55f;
        follower.wobbleSpeed = 3.2f;

        H2GasDeliveryController controller = root.AddComponent<H2GasDeliveryController>();
        controller.travellingGas = ps;
        controller.playOnStart = false;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        string prefabPath = generated + "/H2_Delivery_Path_FX.prefab";

        PrefabUtility.SaveAsPrefabAssetAndConnect(
            root,
            prefabPath,
            InteractionMode.UserAction);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        EditorUtility.DisplayDialog(
            "H2 Gas FX",
            "Created:\n\n" + prefabPath +
            "\n\nMove the objects inside PATH_POINTS_MOVE_THESE so they follow your cylinder hose and reaction tube.",
            "OK");
    }

    static Transform Marker(string name, Transform parent, Vector3 localPosition)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent,false);
        g.transform.localPosition = localPosition;
        return g.transform;
    }

    static void EnsureFolder(string path)
    {
        path = path.Replace("\\","/").TrimEnd('/');
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i=1;i<parts.Length;i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current,parts[i]);
            current = next;
        }
    }
}
#endif
