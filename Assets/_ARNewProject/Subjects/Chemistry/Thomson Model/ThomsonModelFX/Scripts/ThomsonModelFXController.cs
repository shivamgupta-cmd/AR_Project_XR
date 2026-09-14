using UnityEngine;
using System.Collections;

public class ThomsonModelFXController : MonoBehaviour
{
    [Header("References")]
    public Transform atom;
    public Transform positiveSphere;
    public Transform electronsRoot;
    public ParticleSystem innerParticles;
    public ParticleSystem backgroundParticles;
    public ParticleSystem electronPulseParticles;
    public ParticleSystem chargeParticles;
    public GameObject crossSectionObject;
    public GameObject energyRings;

    [Header("Positive Sphere")]
    public Renderer positiveSphereRenderer;
    [ColorUsage(true, true)] public Color positiveColor = new Color(1.8f, 0.15f, 1.8f, 1f);
    public float maxEmission = 2.5f;
    public float pulseSpeed = 1.4f;
    public float pulseAmount = 0.25f;

    [Header("Atom Motion")]
    public bool rotateAtom = true;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 12f;
    public float floatAmount = 0.015f;
    public float floatSpeed = 1.2f;

    [Header("Electrons")]
    public float electronFloatAmount = 0.008f;
    public float electronFloatSpeed = 1.8f;
    public float electronSpinSpeed = 35f;

    Material sphereMat;
    Vector3 atomStartPos;
    Transform[] electrons;
    Vector3[] electronStartPos;
    float[] phases;
    bool fxPlaying;
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (atom == null) atom = transform;
        atomStartPos = atom.localPosition;
        if (positiveSphereRenderer != null)
        {
            sphereMat = positiveSphereRenderer.material;
            sphereMat.EnableKeyword("_EMISSION");
        }
        CacheElectrons();
        SetCrossSection(false);
        fxPlaying = false;
    }
    void Start()
    {
        PlayAllFX();
    }

    void Update()
    {
        if (!fxPlaying) return;
        float t = Time.time;
        if (rotateAtom && atom != null)
            atom.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);
        if (atom != null)
            atom.localPosition = atomStartPos + Vector3.up * (Mathf.Sin(t * floatSpeed) * floatAmount);
        if (sphereMat != null)
        {
            float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            sphereMat.SetColor(EmissionColor, positiveColor * maxEmission * pulse);
        }
        AnimateElectrons(t);
    }

    void CacheElectrons()
    {
        if (electronsRoot == null) return;
        int count = electronsRoot.childCount;
        electrons = new Transform[count];
        electronStartPos = new Vector3[count];
        phases = new float[count];
        for (int i = 0; i < count; i++)
        {
            electrons[i] = electronsRoot.GetChild(i);
            electronStartPos[i] = electrons[i].localPosition;
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void AnimateElectrons(float t)
    {
        if (electrons == null) return;
        for (int i = 0; i < electrons.Length; i++)
        {
            if (electrons[i] == null) continue;
            float p = phases[i];
            Vector3 wobble = new Vector3(
                Mathf.Sin(t * electronFloatSpeed + p),
                Mathf.Cos(t * electronFloatSpeed * .77f + p),
                Mathf.Sin(t * electronFloatSpeed * .53f + p)
            ) * electronFloatAmount;
            electrons[i].localPosition = electronStartPos[i] + wobble;
            electrons[i].Rotate(Vector3.up, electronSpinSpeed * Time.deltaTime, Space.Self);
        }
    }

    // Timeline / Signal methods
    public void PlayAllFX()
    {
        fxPlaying = true;
        PlayPS(innerParticles); PlayPS(backgroundParticles); PlayPS(chargeParticles);
        if (energyRings != null) energyRings.SetActive(true);
    }

    public void StopAllFX()
    {
        fxPlaying = false;
        StopPS(innerParticles); StopPS(backgroundParticles); StopPS(chargeParticles);
        if (energyRings != null) energyRings.SetActive(false);
    }

    public void PulseElectrons()
    {
        if (electronPulseParticles != null) electronPulseParticles.Play();
        if (electronsRoot != null) StartCoroutine(ElectronPulseRoutine());
    }

    IEnumerator ElectronPulseRoutine()
    {
        float d = .35f, timer = 0f;
        Vector3 start = electronsRoot.localScale;
        while (timer < d)
        {
            timer += Time.deltaTime;
            float k = Mathf.Sin((timer / d) * Mathf.PI);
            electronsRoot.localScale = start * (1f + .12f * k);
            yield return null;
        }
        electronsRoot.localScale = start;
    }

    public void ShowCrossSection() => SetCrossSection(true);
    public void HideCrossSection() => SetCrossSection(false);
    public void ToggleCrossSection() => SetCrossSection(crossSectionObject != null && !crossSectionObject.activeSelf);
    void SetCrossSection(bool state) { if (crossSectionObject != null) crossSectionObject.SetActive(state); }

    static void PlayPS(ParticleSystem ps) { if (ps != null) ps.Play(); }
    static void StopPS(ParticleSystem ps) { if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
}
