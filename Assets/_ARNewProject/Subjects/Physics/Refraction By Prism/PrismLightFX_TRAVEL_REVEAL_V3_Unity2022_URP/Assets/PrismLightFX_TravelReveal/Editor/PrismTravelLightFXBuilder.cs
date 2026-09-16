#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PrismTravelLightFXBuilder
{
    [MenuItem("Tools/Prism Light FX/Create Travel Reveal Rig V3")]
    static void Create()
    {
        GameObject root = new GameObject("Prism_Light_FX_TRAVEL_V3");
        PrismTravelLightFX fx = root.AddComponent<PrismTravelLightFX>();

        fx.torchExit = Point("Torch_Exit", root.transform, new Vector3(-1.5f, 0, 0));
        fx.prismEntry = Point("Prism_Entry", root.transform, new Vector3(-0.15f, 0, 0));
        fx.prismExit = Point("Prism_Exit", root.transform, new Vector3(0.15f, 0, 0));
        fx.screenCenter = Point("Screen_Center", root.transform, new Vector3(1.6f, 0, 0));

        Undo.RegisterCreatedObjectUndo(root, "Create Prism Travel Light FX");
        Selection.activeGameObject = root;

        Debug.Log(
            "Prism Travel Reveal V3 created. " +
            "Place Torch_Exit, Prism_Entry, Prism_Exit and Screen_Center. " +
            "Call PlayRefractionSequence() to preview the complete animation."
        );
    }

    static Transform Point(string n, Transform p, Vector3 localPos)
    {
        GameObject g = new GameObject(n);
        g.transform.SetParent(p);
        g.transform.localPosition = localPos;
        return g.transform;
    }
}
#endif