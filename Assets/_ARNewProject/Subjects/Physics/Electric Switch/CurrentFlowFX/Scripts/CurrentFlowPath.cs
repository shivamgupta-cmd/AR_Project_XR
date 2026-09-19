using System.Collections.Generic;
using UnityEngine;

public class CurrentFlowPath : MonoBehaviour
{
    [Header("PATH - assign points in circuit order")]
    public List<Transform> points = new List<Transform>();
    public bool closedLoop = true;
    [Header("FLOW")]
    [Min(1)] public int currentDots = 55;
    public float speed = 0.35f;
    public float dotSize = 0.025f;
    public bool flowOnStart = true;
    public bool reverseDirection = false;
    [Header("VISUAL")]
    public Material currentMaterial;
    public GameObject dotPrefab;
    public Light bulbLight;
    public Renderer bulbRenderer;
    public Color bulbOnColor = new Color(1f, .75f, .12f, 1f);
    public Color bulbOffColor = new Color(.18f, .18f, .18f, 1f);
    public float bulbIntensity = 4f;

    readonly List<Transform> dots = new List<Transform>();
    float[] segLengths; float totalLength; bool flowing;
    Material bulbMat;

    void Start() { Rebuild(); SetFlow(flowOnStart); }
    void Update() { if (!flowing || totalLength <= 0) return; float t = Time.time * speed * (reverseDirection ? -1f : 1f); for (int i = 0; i < dots.Count; i++) { float u = Mathf.Repeat(t + (float)i / dots.Count, 1f); dots[i].position = EvaluateDistance(u * totalLength); } }

    public void Rebuild()
    {
        ClearDots();
        BuildLengths();
        if (points.Count < 2)
            return;
        for (int i = 0; i < currentDots; i++)
        {
            GameObject g = dotPrefab ? Instantiate(dotPrefab, transform) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = "Current_Dot_" + i.ToString("00");
            g.transform.SetParent(transform, true);
            g.transform.localScale = Vector3.one * dotSize;
            var c = g.GetComponent<Collider>();
            if (c)
                Destroy(c);
            var r = g.GetComponent<Renderer>();
            if (r && currentMaterial)
                r.sharedMaterial = currentMaterial;
            dots.Add(g.transform);
        }
    }
    void BuildLengths() { int n = points.Count; int segs = closedLoop ? n : n - 1; segLengths = new float[Mathf.Max(0, segs)]; totalLength = 0; for (int i = 0; i < segs; i++) { if (!points[i] || !points[(i + 1) % n]) continue; segLengths[i] = Vector3.Distance(points[i].position, points[(i + 1) % n].position); totalLength += segLengths[i]; } }
    Vector3 EvaluateDistance(float d) { if (points.Count < 2) return transform.position; int n = points.Count; for (int i = 0; i < segLengths.Length; i++) { if (d <= segLengths[i]) return Vector3.Lerp(points[i].position, points[(i + 1) % n].position, segLengths[i] <= 0 ? 0 : d / segLengths[i]); d -= segLengths[i]; } return points[closedLoop ? 0 : n - 1].position; }
    void ClearDots() 
    { 
        foreach (var d in dots)
            if (d)
                DestroyImmediateSafe(d.gameObject);
        dots.Clear();
    }
    void DestroyImmediateSafe(GameObject g)
    {
        if (Application.isPlaying)
            Destroy(g); else DestroyImmediate(g);
    }

    public void StartCurrent()
    { 
        SetFlow(true); 
    }
    public void StopCurrent()
    {
        SetFlow(false);
    }
    public void ToggleCurrent()
    {
        SetFlow(!flowing);
    }
    public void ReverseCurrent() 
    {
        reverseDirection = !reverseDirection;
    }
    public void SetFlow(bool on) 
    {
        flowing = on;
        if (bulbLight)
        {
            bulbLight.enabled = on; bulbLight.intensity = bulbIntensity; 
        }
        if (bulbRenderer) 
        { 
            if (!bulbMat) bulbMat = bulbRenderer.material;
            bulbMat.SetColor("_BaseColor", on ? bulbOnColor : bulbOffColor);
            bulbMat.SetColor("_EmissionColor", on ? bulbOnColor * 3f : Color.black); 
            if (on) bulbMat.EnableKeyword("_EMISSION");
            else bulbMat.DisableKeyword("_EMISSION");
        } 
    }
    public void SetSpeed(float value)
    {
        speed = value; 
    }
    public void SetDotCount(int count)
    {
        currentDots = Mathf.Max(1, count);
        Rebuild(); 
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Count < 2) return;
        Gizmos.color = Color.cyan; int n = points.Count; 
        int segs = closedLoop ? n : n - 1; 
        for (int i = 0; i < segs; i++) 
            if (points[i] && points[(i + 1) % n])
                Gizmos.DrawLine(points[i].position, points[(i + 1) % n].position);
    }
}
