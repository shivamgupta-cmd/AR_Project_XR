#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ThomsonPositiveSphereBuilder
{
    const string Root = "Assets/ThomsonPositiveSphere";

    [MenuItem("Tools/Thomson Model/Create Positive Sphere Prefab")]
    public static void Build()
    {
        EnsureFolder(Root + "/Materials");
        EnsureFolder(Root + "/Prefabs");

        Material sphereMat = CreateMaterial(Root + "/Materials/PositiveSphere.mat", new Color(1f, .05f, .55f, .72f), true);
        Material chargeMat = CreateMaterial(Root + "/Materials/PositiveCharge.mat", new Color(1f, .05f, .8f, 1f), false);
        Material cutMat = CreateMaterial(Root + "/Materials/CutawaySphere.mat", new Color(1f, .05f, .55f, .16f), true);

        var root = new GameObject("Thomson_Positive_Sphere");
        var ctrl = root.AddComponent<ThomsonPositiveSphereController>();

        var solid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        solid.name = "01_Normal_Positive_Sphere";
        solid.transform.SetParent(root.transform, false);
        solid.GetComponent<Renderer>().sharedMaterial = sphereMat;
        Object.DestroyImmediate(solid.GetComponent<Collider>());

        var cutaway = new GameObject("03_No_Nucleus_Cutaway");
        cutaway.transform.SetParent(root.transform, false);
        CreateHemisphere(cutaway.transform, cutMat);
        cutaway.SetActive(false);

        var charges = new GameObject("02_3D_Positive_Charges");
        charges.transform.SetParent(root.transform, false);
        CreateCharges(charges.transform, chargeMat);
        charges.SetActive(false);

        ctrl.solidSphere = solid;
        ctrl.cutawaySphere = cutaway;
        ctrl.positiveCharges = charges;

        string prefabPath = Root + "/Prefabs/Thomson_Positive_Sphere.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Debug.Log("Created: " + prefabPath);
    }

    static void CreateCharges(Transform parent, Material mat)
    {
        Vector3[] dirs = {
            new Vector3(1,.25f,.2f), new Vector3(-1,.3f,.1f), new Vector3(.2f,1,.3f), new Vector3(.1f,-1,.4f),
            new Vector3(.2f,.25f,1), new Vector3(.3f,-.2f,-1), new Vector3(.7f,.65f,.4f), new Vector3(-.7f,.6f,.45f),
            new Vector3(.65f,-.6f,.45f), new Vector3(-.65f,-.55f,.5f), new Vector3(.55f,.45f,-.65f), new Vector3(-.55f,.4f,-.7f),
            new Vector3(.45f,-.55f,-.65f), new Vector3(-.45f,-.5f,-.7f), new Vector3(.85f,0,-.45f), new Vector3(-.85f,0,-.4f)
        };
        for(int i=0;i<dirs.Length;i++) CreatePlus(parent, dirs[i].normalized * .82f, mat, i);
    }

    static void CreatePlus(Transform parent, Vector3 pos, Material mat, int index)
    {
        var holder = new GameObject("PositiveCharge_" + (index+1).ToString("00"));
        holder.transform.SetParent(parent, false); holder.transform.localPosition = pos;
        holder.transform.localRotation = Quaternion.LookRotation(pos.normalized);
        CreateBar(holder.transform, new Vector3(.18f,.055f,.035f), mat);
        CreateBar(holder.transform, new Vector3(.055f,.18f,.035f), mat);
    }

    static void CreateBar(Transform parent, Vector3 scale, Material mat)
    {
        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.transform.SetParent(parent, false); b.transform.localScale = scale;
        b.GetComponent<Renderer>().sharedMaterial = mat; Object.DestroyImmediate(b.GetComponent<Collider>());
    }

    static void CreateHemisphere(Transform parent, Material mat)
    {
        // Back shell: scaled sphere plus a dark/open center indicator. Transparent shell makes the empty center obvious.
        var shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shell.name = "Transparent_Shell_No_Central_Nucleus";
        shell.transform.SetParent(parent, false);
        shell.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(shell.GetComponent<Collider>());

    }

    static Material CreateMaterial(string path, Color color, bool transparent)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); AssetDatabase.CreateAsset(m,path); }
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color); else m.color = color;
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * 2f); }
        if (transparent) {
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface",1);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend",0);
            m.renderQueue = 3000;
        }
        EditorUtility.SetDirty(m); return m;
    }

    static void EnsureFolder(string path)
    {
        string[] p=path.Split('/'); string cur=p[0];
        for(int i=1;i<p.Length;i++){ string next=cur+"/"+p[i]; if(!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur,p[i]); cur=next; }
    }
}
#endif
