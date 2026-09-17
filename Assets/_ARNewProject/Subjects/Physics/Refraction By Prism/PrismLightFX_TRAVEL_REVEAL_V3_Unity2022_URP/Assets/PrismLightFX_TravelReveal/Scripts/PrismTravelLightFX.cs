using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class PrismTravelLightFX : MonoBehaviour
{
    [Header("Scene Markers")]
    public Transform torchExit;
    public Transform prismEntry;
    public Transform prismExit;
    public Transform screenCenter;

    [Header("Incoming White Beam")]
    [Min(0.001f)] public float whiteBeamStartWidth = 0.025f;
    [Min(0.001f)] public float whiteBeamEndWidth = 0.018f;

    [Header("Spectrum Shape")]
    [Range(0.01f, 2f)] public float spectrumSpread = 0.35f;
    [Range(0.005f, 0.5f)] public float bandWidthAtScreen = 0.13f;
    [Range(0.001f, 0.1f)] public float bandWidthAtPrism = 0.008f;
    [Range(0f, 0.25f)] public float overlap = 0.03f;

    [Header("Look")]
    [Range(0.1f, 10f)] public float intensity = 2.5f;
    [Range(0.01f, 0.49f)] public float edgeSoftness = 0.24f;
    [Range(0.001f, 0.25f)] public float travelHeadSoftness = 0.06f;

    [Header("Travel Timing")]
    [Min(0.05f)] public float incomingTravelTime = 0.65f;
    [Min(0f)] public float prismReactionDelay = 0.12f;
    [Min(0.05f)] public float spectrumTravelTime = 0.90f;
    public AnimationCurve travelEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool playOnStart = false;

    [Header("Optional Prism Flash")]
    public ParticleSystem prismEntryGlow;
    public ParticleSystem prismExitGlow;

    [Header("Debug / Timeline")]
    [Range(0f, 1f)] public float incomingProgress = 0f;
    [Range(0f, 1f)] public float spectrumProgress = 0f;

    GameObject incomingGO;
    MeshFilter incomingMF;
    MeshRenderer incomingMR;

    GameObject spectrumRoot;
    MeshFilter[] bandMF = new MeshFilter[7];
    MeshRenderer[] bandMR = new MeshRenderer[7];

    Material whiteMat;
    Material[] bandMats = new Material[7];
    Coroutine sequenceRoutine;

    readonly Color[] colors =
    {
        new Color(1f,0.02f,0.01f,0.58f),
        new Color(1f,0.30f,0.01f,0.55f),
        new Color(1f,0.88f,0.02f,0.52f),
        new Color(0.05f,1f,0.16f,0.50f),
        new Color(0.02f,0.72f,1f,0.50f),
        new Color(0.08f,0.28f,1f,0.52f),
        new Color(0.55f,0.03f,1f,0.55f)
    };

    void OnEnable()
    {
        BuildIfNeeded();
        ApplyProgress();
        UpdateGeometry();

        if (Application.isPlaying)
        {
            if (playOnStart) PlayRefractionSequence();
            else ResetEffect();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            BuildIfNeeded();
            ApplyProgress();
            UpdateGeometry();
        }
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
            UpdateGeometry();
    }

    void BuildIfNeeded()
    {
        if (incomingGO != null) return;

        Shader shader = Shader.Find("PrismLightFX/TravelBeam");
        if (!shader)
        {
            Debug.LogError("PrismLightFX/TravelBeam shader not found.");
            return;
        }

        incomingGO = new GameObject("Incoming_White_Beam_Travel");
        incomingGO.transform.SetParent(transform, false);
        incomingMF = incomingGO.AddComponent<MeshFilter>();
        incomingMR = incomingGO.AddComponent<MeshRenderer>();
        incomingMF.sharedMesh = NewQuadMesh("IncomingBeamTravelMesh");

        whiteMat = new Material(shader);
        whiteMat.name = "WhiteBeam_Travel_Runtime";
        whiteMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.80f));
        incomingMR.sharedMaterial = whiteMat;

        spectrumRoot = new GameObject("Spectrum_Travel");
        spectrumRoot.transform.SetParent(transform, false);

        for (int i = 0; i < 7; i++)
        {
            GameObject go = new GameObject("Band_" + i);
            go.transform.SetParent(spectrumRoot.transform, false);

            bandMF[i] = go.AddComponent<MeshFilter>();
            bandMR[i] = go.AddComponent<MeshRenderer>();
            bandMF[i].sharedMesh = NewQuadMesh("SpectrumBandTravel_" + i);

            bandMats[i] = new Material(shader);
            bandMats[i].name = "SpectrumTravel_Mat_" + i;
            bandMats[i].SetColor("_BaseColor", colors[i]);
            bandMR[i].sharedMaterial = bandMats[i];
        }
    }

    Mesh NewQuadMesh(string meshName)
    {
        Mesh m = new Mesh();
        m.name = meshName;
        m.vertices = new Vector3[4];
        m.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1)
        };
        m.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        return m;
    }

    void ApplyLook(Material m)
    {
        if (!m) return;
        m.SetFloat("_Intensity", intensity);
        m.SetFloat("_EdgeSoftness", edgeSoftness);
        m.SetFloat("_HeadSoftness", travelHeadSoftness);
    }

    void ApplyProgress()
    {
        BuildIfNeeded();

        if (whiteMat)
        {
            ApplyLook(whiteMat);
            whiteMat.SetFloat("_Progress", incomingProgress);
        }

        for (int i = 0; i < bandMats.Length; i++)
        {
            if (!bandMats[i]) continue;
            ApplyLook(bandMats[i]);
            bandMats[i].SetColor("_BaseColor", colors[i]);
            bandMats[i].SetFloat("_Progress", spectrumProgress);
        }
    }

    void UpdateGeometry()
    {
        if (!torchExit || !prismEntry || !prismExit || !screenCenter) return;
        BuildIfNeeded();

        Camera cam = Camera.main;
        Vector3 viewNormal = cam ? cam.transform.forward : Vector3.forward;

        SetRibbon(
            incomingMF.sharedMesh,
            torchExit.position,
            prismEntry.position,
            whiteBeamStartWidth,
            whiteBeamEndWidth,
            viewNormal
        );

        Vector3 spreadAxis = screenCenter.up;

        for (int i = 0; i < 7; i++)
        {
            float n = (i / 6f) - 0.5f;
            Vector3 target = screenCenter.position + spreadAxis * (n * spectrumSpread);

            SetRibbon(
                bandMF[i].sharedMesh,
                prismExit.position,
                target,
                bandWidthAtPrism,
                bandWidthAtScreen + overlap,
                viewNormal
            );
        }

        ApplyProgress();
    }

    void SetRibbon(Mesh mesh, Vector3 start, Vector3 end, float startWidth, float endWidth, Vector3 viewNormal)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 side = Vector3.Cross(viewNormal, dir).normalized;

        if (side.sqrMagnitude < 0.001f)
            side = Vector3.Cross(Vector3.up, dir).normalized;

        Vector3[] v = new Vector3[4];

        v[0] = transform.InverseTransformPoint(start - side * startWidth * 0.5f);
        v[1] = transform.InverseTransformPoint(start + side * startWidth * 0.5f);
        v[2] = transform.InverseTransformPoint(end - side * endWidth * 0.5f);
        v[3] = transform.InverseTransformPoint(end + side * endWidth * 0.5f);

        mesh.vertices = v;
        mesh.RecalculateBounds();
    }

    // ------------------------------------------------------------
    // ONE-CLICK FULL SEQUENCE
    // Torch beam travels -> reaches prism -> spectrum travels outward
    // ------------------------------------------------------------
    public void PlayRefractionSequence()
    {
        if (!Application.isPlaying) return;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(RefractionSequence());
    }

    IEnumerator RefractionSequence()
    {
        ResetEffect();

        incomingGO.SetActive(true);
        spectrumRoot.SetActive(false);

        yield return AnimateIncoming();

        if (prismEntryGlow) prismEntryGlow.Play();
        if (prismExitGlow) prismExitGlow.Play();

        if (prismReactionDelay > 0f)
            yield return new WaitForSeconds(prismReactionDelay);

        spectrumRoot.SetActive(true);
        yield return AnimateSpectrum();

        sequenceRoutine = null;
    }

    IEnumerator AnimateIncoming()
    {
        incomingProgress = 0f;
        float t = 0f;

        while (t < incomingTravelTime)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / incomingTravelTime);
            incomingProgress = travelEase.Evaluate(n);
            ApplyProgress();
            yield return null;
        }

        incomingProgress = 1f;
        ApplyProgress();
    }

    IEnumerator AnimateSpectrum()
    {
        spectrumProgress = 0f;
        float t = 0f;

        while (t < spectrumTravelTime)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / spectrumTravelTime);
            spectrumProgress = travelEase.Evaluate(n);
            ApplyProgress();
            yield return null;
        }

        spectrumProgress = 1f;
        ApplyProgress();
    }

    // ------------------------------------------------------------
    // TIMELINE / SIGNAL FRIENDLY METHODS
    // ------------------------------------------------------------

    public void ResetEffect()
    {
        if (Application.isPlaying && sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        incomingProgress = 0f;
        spectrumProgress = 0f;

        BuildIfNeeded();
        if (incomingGO) incomingGO.SetActive(false);
        if (spectrumRoot) spectrumRoot.SetActive(false);

        if (prismEntryGlow)
            prismEntryGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (prismExitGlow)
            prismExitGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ApplyProgress();
    }

    public void StartIncomingTravel()
    {
        if (!Application.isPlaying) return;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        incomingGO.SetActive(true);
        spectrumRoot.SetActive(false);
        sequenceRoutine = StartCoroutine(OnlyIncomingRoutine());
    }

    IEnumerator OnlyIncomingRoutine()
    {
        yield return AnimateIncoming();
        sequenceRoutine = null;
    }

    public void StartSpectrumTravel()
    {
        if (!Application.isPlaying) return;

        spectrumRoot.SetActive(true);

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(OnlySpectrumRoutine());
    }

    IEnumerator OnlySpectrumRoutine()
    {
        yield return AnimateSpectrum();
        sequenceRoutine = null;
    }

    public void ShowCompletedEffect()
    {
        BuildIfNeeded();
        incomingGO.SetActive(true);
        spectrumRoot.SetActive(true);
        incomingProgress = 1f;
        spectrumProgress = 1f;
        ApplyProgress();
    }

    // Useful if you animate these values from another script/slider/Timeline.
    public void SetIncomingProgress(float value)
    {
        incomingProgress = Mathf.Clamp01(value);
        BuildIfNeeded();
        incomingGO.SetActive(true);
        ApplyProgress();
    }

    public void SetSpectrumProgress(float value)
    {
        spectrumProgress = Mathf.Clamp01(value);
        BuildIfNeeded();
        spectrumRoot.SetActive(true);
        ApplyProgress();
    }
}

//using System.Collections;
//using UnityEngine;

//public class PrismTravelLightFX : MonoBehaviour
//{
//    [Header("Scene Markers")]
//    public Transform torchExit;
//    public Transform prismEntry;
//    public Transform prismExit;
//    public Transform screenCenter;

//    [Header("Incoming White Beam")]
//    [Min(0.001f)] public float whiteBeamStartWidth = 0.025f;
//    [Min(0.001f)] public float whiteBeamEndWidth = 0.018f;

//    [Header("Spectrum Shape")]
//    [Range(0.01f, 2f)] public float spectrumSpread = 0.35f;
//    [Range(0.005f, 0.5f)] public float bandWidthAtScreen = 0.13f;
//    [Range(0.001f, 0.1f)] public float bandWidthAtPrism = 0.008f;
//    [Range(0f, 0.25f)] public float overlap = 0.03f;

//    [Header("Look")]
//    [Range(0.1f, 10f)] public float intensity = 2.5f;
//    [Range(0.01f, 0.49f)] public float edgeSoftness = 0.24f;
//    [Range(0.001f, 0.25f)] public float travelHeadSoftness = 0.06f;

//    [Header("Travel Timing")]
//    [Min(0.05f)] public float incomingTravelTime = 0.65f;
//    [Min(0f)] public float prismReactionDelay = 0.12f;
//    [Min(0.05f)] public float spectrumTravelTime = 0.90f;
//    public AnimationCurve travelEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
//    public bool playOnStart = false;

//    [Header("Optional Prism Flash")]
//    public ParticleSystem prismEntryGlow;
//    public ParticleSystem prismExitGlow;

//    [Header("Debug / Timeline")]
//    [Range(0f, 1f)] public float incomingProgress = 0f;
//    [Range(0f, 1f)] public float spectrumProgress = 0f;

//    GameObject incomingGO;
//    MeshFilter incomingMF;
//    MeshRenderer incomingMR;

//    GameObject spectrumRoot;
//    MeshFilter[] bandMF = new MeshFilter[7];
//    MeshRenderer[] bandMR = new MeshRenderer[7];

//    Material whiteMat;
//    Material[] bandMats = new Material[7];

//    Coroutine sequenceRoutine;

//    readonly Color[] colors =
//    {
//        new Color(1f,0.02f,0.01f,0.58f),
//        new Color(1f,0.30f,0.01f,0.55f),
//        new Color(1f,0.88f,0.02f,0.52f),
//        new Color(0.05f,1f,0.16f,0.50f),
//        new Color(0.02f,0.72f,1f,0.50f),
//        new Color(0.08f,0.28f,1f,0.52f),
//        new Color(0.55f,0.03f,1f,0.55f)
//    };

//    void Start()
//    {
//        BuildIfNeeded();
//        UpdateGeometry();

//        if (playOnStart)
//            PlayRefractionSequence();
//        else
//            ResetEffect();
//    }

//    void LateUpdate()
//    {
//        if (Application.isPlaying)
//            UpdateGeometry();
//    }

//    void OnDisable()
//    {
//        CleanupRuntimeObjects();
//    }

//    void OnDestroy()
//    {
//        CleanupRuntimeObjects();
//    }

//    void BuildIfNeeded()
//    {
//        if (!Application.isPlaying)
//            return;

//        if (incomingGO != null && spectrumRoot != null)
//            return;

//        Shader shader = Shader.Find("PrismLightFX/TravelBeam");

//        if (!shader)
//        {
//            Debug.LogError("PrismLightFX/TravelBeam shader not found.");
//            return;
//        }

//        incomingGO = new GameObject("Incoming_White_Beam_Travel");
//        incomingGO.transform.SetParent(transform, false);

//        incomingMF = incomingGO.AddComponent<MeshFilter>();
//        incomingMR = incomingGO.AddComponent<MeshRenderer>();

//        incomingMF.sharedMesh = NewQuadMesh("IncomingBeamTravelMesh");

//        whiteMat = new Material(shader);
//        whiteMat.name = "WhiteBeam_Travel_Runtime";
//        whiteMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.80f));

//        incomingMR.sharedMaterial = whiteMat;

//        spectrumRoot = new GameObject("Spectrum_Travel");
//        spectrumRoot.transform.SetParent(transform, false);

//        for (int i = 0; i < 7; i++)
//        {
//            GameObject go = new GameObject("Band_" + i);
//            go.transform.SetParent(spectrumRoot.transform, false);

//            bandMF[i] = go.AddComponent<MeshFilter>();
//            bandMR[i] = go.AddComponent<MeshRenderer>();

//            bandMF[i].sharedMesh = NewQuadMesh("SpectrumBandTravel_" + i);

//            bandMats[i] = new Material(shader);
//            bandMats[i].name = "SpectrumTravel_Mat_" + i;
//            bandMats[i].SetColor("_BaseColor", colors[i]);

//            bandMR[i].sharedMaterial = bandMats[i];
//        }
//    }

//    void CleanupRuntimeObjects()
//    {
//        if (sequenceRoutine != null)
//        {
//            StopCoroutine(sequenceRoutine);
//            sequenceRoutine = null;
//        }

//        if (incomingGO != null)
//        {
//            Destroy(incomingGO);
//            incomingGO = null;
//        }

//        if (spectrumRoot != null)
//        {
//            Destroy(spectrumRoot);
//            spectrumRoot = null;
//        }

//        if (whiteMat != null)
//        {
//            Destroy(whiteMat);
//            whiteMat = null;
//        }

//        for (int i = 0; i < bandMats.Length; i++)
//        {
//            if (bandMats[i] != null)
//            {
//                Destroy(bandMats[i]);
//                bandMats[i] = null;
//            }

//            bandMF[i] = null;
//            bandMR[i] = null;
//        }

//        incomingMF = null;
//        incomingMR = null;
//    }

//    Mesh NewQuadMesh(string meshName)
//    {
//        Mesh m = new Mesh();
//        m.name = meshName;

//        m.vertices = new Vector3[4];

//        m.uv = new Vector2[]
//        {
//            new Vector2(0,0),
//            new Vector2(0,1),
//            new Vector2(1,0),
//            new Vector2(1,1)
//        };

//        m.triangles = new int[]
//        {
//            0,2,1,
//            2,3,1
//        };

//        return m;
//    }

//    void ApplyLook(Material m)
//    {
//        if (!m)
//            return;

//        m.SetFloat("_Intensity", intensity);
//        m.SetFloat("_EdgeSoftness", edgeSoftness);
//        m.SetFloat("_HeadSoftness", travelHeadSoftness);
//    }

//    void ApplyProgress()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        if (whiteMat)
//        {
//            ApplyLook(whiteMat);
//            whiteMat.SetFloat("_Progress", incomingProgress);
//        }

//        for (int i = 0; i < bandMats.Length; i++)
//        {
//            if (!bandMats[i])
//                continue;

//            ApplyLook(bandMats[i]);
//            bandMats[i].SetColor("_BaseColor", colors[i]);
//            bandMats[i].SetFloat("_Progress", spectrumProgress);
//        }
//    }

//    void UpdateGeometry()
//    {
//        if (!Application.isPlaying)
//            return;

//        if (!torchExit || !prismEntry || !prismExit || !screenCenter)
//            return;

//        BuildIfNeeded();

//        if (incomingMF == null)
//            return;

//        Camera cam = Camera.main;
//        Vector3 viewNormal = cam ? cam.transform.forward : Vector3.forward;

//        SetRibbon(
//            incomingMF.sharedMesh,
//            torchExit.position,
//            prismEntry.position,
//            whiteBeamStartWidth,
//            whiteBeamEndWidth,
//            viewNormal
//        );

//        Vector3 spreadAxis = screenCenter.up;

//        for (int i = 0; i < 7; i++)
//        {
//            if (bandMF[i] == null)
//                continue;

//            float n = (i / 6f) - 0.5f;

//            Vector3 target =
//                screenCenter.position +
//                spreadAxis * (n * spectrumSpread);

//            SetRibbon(
//                bandMF[i].sharedMesh,
//                prismExit.position,
//                target,
//                bandWidthAtPrism,
//                bandWidthAtScreen + overlap,
//                viewNormal
//            );
//        }

//        ApplyProgress();
//    }

//    void SetRibbon(
//        Mesh mesh,
//        Vector3 start,
//        Vector3 end,
//        float startWidth,
//        float endWidth,
//        Vector3 viewNormal)
//    {
//        if (mesh == null)
//            return;

//        Vector3 dir = (end - start).normalized;
//        Vector3 side = Vector3.Cross(viewNormal, dir).normalized;

//        if (side.sqrMagnitude < 0.001f)
//            side = Vector3.Cross(Vector3.up, dir).normalized;

//        Vector3[] v = new Vector3[4];

//        v[0] = transform.InverseTransformPoint(
//            start - side * startWidth * 0.5f
//        );

//        v[1] = transform.InverseTransformPoint(
//            start + side * startWidth * 0.5f
//        );

//        v[2] = transform.InverseTransformPoint(
//            end - side * endWidth * 0.5f
//        );

//        v[3] = transform.InverseTransformPoint(
//            end + side * endWidth * 0.5f
//        );

//        mesh.vertices = v;
//        mesh.RecalculateBounds();
//    }

//    public void PlayRefractionSequence()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        if (incomingGO == null || spectrumRoot == null)
//            return;

//        if (sequenceRoutine != null)
//        {
//            StopCoroutine(sequenceRoutine);
//            sequenceRoutine = null;
//        }

//        incomingProgress = 0f;
//        spectrumProgress = 0f;

//        incomingGO.SetActive(false);
//        spectrumRoot.SetActive(false);

//        StopGlow();

//        ApplyProgress();

//        sequenceRoutine = StartCoroutine(RefractionSequence());
//    }

//    IEnumerator RefractionSequence()
//    {
//        incomingGO.SetActive(true);
//        spectrumRoot.SetActive(false);

//        yield return AnimateIncoming();

//        if (prismEntryGlow)
//            prismEntryGlow.Play();

//        if (prismExitGlow)
//            prismExitGlow.Play();

//        if (prismReactionDelay > 0f)
//            yield return new WaitForSeconds(prismReactionDelay);

//        spectrumRoot.SetActive(true);

//        yield return AnimateSpectrum();

//        sequenceRoutine = null;
//    }

//    IEnumerator AnimateIncoming()
//    {
//        incomingProgress = 0f;

//        float t = 0f;

//        while (t < incomingTravelTime)
//        {
//            t += Time.deltaTime;

//            float n = Mathf.Clamp01(t / incomingTravelTime);

//            incomingProgress = travelEase.Evaluate(n);

//            ApplyProgress();

//            yield return null;
//        }

//        incomingProgress = 1f;

//        ApplyProgress();
//    }

//    IEnumerator AnimateSpectrum()
//    {
//        spectrumProgress = 0f;

//        float t = 0f;

//        while (t < spectrumTravelTime)
//        {
//            t += Time.deltaTime;

//            float n = Mathf.Clamp01(t / spectrumTravelTime);

//            spectrumProgress = travelEase.Evaluate(n);

//            ApplyProgress();

//            yield return null;
//        }

//        spectrumProgress = 1f;

//        ApplyProgress();
//    }

//    public void ResetEffect()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        if (sequenceRoutine != null)
//        {
//            StopCoroutine(sequenceRoutine);
//            sequenceRoutine = null;
//        }

//        incomingProgress = 0f;
//        spectrumProgress = 0f;

//        if (incomingGO)
//            incomingGO.SetActive(false);

//        if (spectrumRoot)
//            spectrumRoot.SetActive(false);

//        StopGlow();

//        ApplyProgress();
//    }

//    void StopGlow()
//    {
//        if (prismEntryGlow)
//        {
//            prismEntryGlow.Stop(
//                true,
//                ParticleSystemStopBehavior.StopEmittingAndClear
//            );
//        }

//        if (prismExitGlow)
//        {
//            prismExitGlow.Stop(
//                true,
//                ParticleSystemStopBehavior.StopEmittingAndClear
//            );
//        }
//    }

//    public void StartIncomingTravel()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        if (sequenceRoutine != null)
//        {
//            StopCoroutine(sequenceRoutine);
//            sequenceRoutine = null;
//        }

//        incomingProgress = 0f;
//        spectrumProgress = 0f;

//        incomingGO.SetActive(true);
//        spectrumRoot.SetActive(false);

//        ApplyProgress();

//        sequenceRoutine = StartCoroutine(OnlyIncomingRoutine());
//    }

//    IEnumerator OnlyIncomingRoutine()
//    {
//        yield return AnimateIncoming();

//        sequenceRoutine = null;
//    }

//    public void StartSpectrumTravel()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        if (sequenceRoutine != null)
//        {
//            StopCoroutine(sequenceRoutine);
//            sequenceRoutine = null;
//        }

//        spectrumProgress = 0f;

//        spectrumRoot.SetActive(true);

//        ApplyProgress();

//        sequenceRoutine = StartCoroutine(OnlySpectrumRoutine());
//    }

//    IEnumerator OnlySpectrumRoutine()
//    {
//        yield return AnimateSpectrum();

//        sequenceRoutine = null;
//    }

//    public void ShowCompletedEffect()
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        incomingGO.SetActive(true);
//        spectrumRoot.SetActive(true);

//        incomingProgress = 1f;
//        spectrumProgress = 1f;

//        ApplyProgress();
//    }

//    public void SetIncomingProgress(float value)
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        incomingProgress = Mathf.Clamp01(value);

//        incomingGO.SetActive(true);

//        ApplyProgress();
//    }

//    public void SetSpectrumProgress(float value)
//    {
//        if (!Application.isPlaying)
//            return;

//        BuildIfNeeded();

//        spectrumProgress = Mathf.Clamp01(value);

//        spectrumRoot.SetActive(true);

//        ApplyProgress();
//    }
//}