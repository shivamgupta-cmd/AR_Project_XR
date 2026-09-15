using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IonicBondPairToLattice : MonoBehaviour
{
    [Header("ASSIGN ONLY THESE TWO")]
    public GameObject sodiumAtom;
    public GameObject chlorineAtom;

    [Header("Playback")]
    public bool playOnStart = false;
    public bool loop = false;
    public float loopDelay = 1.5f;

    [Header("First Pair Attraction")]
    public float attractionDuration = 1.2f;
    public float startingSeparation = 1.4f;
    public float pairSpacing = 0.35f;
    public AnimationCurve movementEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Attraction Look")]
    public bool attractionPulse = true;
    [Range(0f, 0.25f)] public float pulseAmount = 0.06f;
    public float pulseSpeed = 6f;

    [Header("Lattice")]
    [Range(2, 8)] public int latticeSize = 4;
    public float latticeSpacing = 0.38f;
    public float pairJoinDuration = 0.45f;
    public float delayBetweenPairs = 0.05f;
    public float incomingDistance = 1.2f;
    public float incomingArcHeight = 0.12f;
    public bool layerByLayer = true;
    public float layerPause = 0.18f;

    [Header("Optional FX")]
    public ParticleSystem firstPairImpactFX;
    public ParticleSystem latticeCompleteFX;

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip attractionClip;
    public AudioClip pairJoinClip;
    public AudioClip latticeCompleteClip;

    [Header("Runtime")]
    [SerializeField] private bool isPlaying;
    [SerializeField] private int currentIonCount;

    private Transform sodiumOriginalParent;
    private Transform chlorineOriginalParent;
    private Vector3 sodiumOriginalLocalPosition;
    private Vector3 chlorineOriginalLocalPosition;
    private Quaternion sodiumOriginalLocalRotation;
    private Quaternion chlorineOriginalLocalRotation;
    private Vector3 sodiumOriginalLocalScale;
    private Vector3 chlorineOriginalLocalScale;

    private readonly List<GameObject> spawnedIons = new List<GameObject>();
    private Coroutine sequenceRoutine;

    private void Awake()
    {
        CacheOriginalState();
    }

    private void Start()
    {
        ResetFormation();
        if (playOnStart) PlayFullSequence();
    }

    private void CacheOriginalState()
    {
        if (sodiumAtom != null)
        {
            sodiumOriginalParent = sodiumAtom.transform.parent;
            sodiumOriginalLocalPosition = sodiumAtom.transform.localPosition;
            sodiumOriginalLocalRotation = sodiumAtom.transform.localRotation;
            sodiumOriginalLocalScale = sodiumAtom.transform.localScale;
        }

        if (chlorineAtom != null)
        {
            chlorineOriginalParent = chlorineAtom.transform.parent;
            chlorineOriginalLocalPosition = chlorineAtom.transform.localPosition;
            chlorineOriginalLocalRotation = chlorineAtom.transform.localRotation;
            chlorineOriginalLocalScale = chlorineAtom.transform.localScale;
        }
    }

    public void PlayFullSequence()
    {
        if (!Ready()) return;
        StopSequence();
        sequenceRoutine = StartCoroutine(FullSequenceRoutine());
    }

    public void StartAttraction()
    {
        if (!Ready()) return;
        StopSequence();
        sequenceRoutine = StartCoroutine(AttractionOnlyRoutine());
    }

    public void StartLatticeFormation()
    {
        if (!Ready()) return;
        StopSequence();
        sequenceRoutine = StartCoroutine(LatticeOnlyRoutine());
    }

    public void ResetFormation()
    {
        StopSequence();

        for (int i = spawnedIons.Count - 1; i >= 0; i--)
        {
            if (spawnedIons[i] != null) Destroy(spawnedIons[i]);
        }
        spawnedIons.Clear();
        currentIonCount = 0;

        if (sodiumAtom != null)
        {
            sodiumAtom.SetActive(true);
            sodiumAtom.transform.SetParent(sodiumOriginalParent, false);
            sodiumAtom.transform.localPosition = sodiumOriginalLocalPosition;
            sodiumAtom.transform.localRotation = sodiumOriginalLocalRotation;
            sodiumAtom.transform.localScale = sodiumOriginalLocalScale;
        }

        if (chlorineAtom != null)
        {
            chlorineAtom.SetActive(true);
            chlorineAtom.transform.SetParent(chlorineOriginalParent, false);
            chlorineAtom.transform.localPosition = chlorineOriginalLocalPosition;
            chlorineAtom.transform.localRotation = chlorineOriginalLocalRotation;
            chlorineAtom.transform.localScale = chlorineOriginalLocalScale;
        }

        if (Ready()) PlaceAtStart();
        StopFX(firstPairImpactFX);
        StopFX(latticeCompleteFX);
        isPlaying = false;
    }

    public void ShowCompleteLatticeImmediately()
    {
        if (!Ready()) return;
        ResetFormation();
        BuildAllInstantly();
    }

    private IEnumerator FullSequenceRoutine()
    {
        do
        {
            ResetFormation();
            isPlaying = true;
            yield return FirstPairRoutine();
            yield return new WaitForSeconds(0.25f);
            yield return BuildLatticeRoutine();
            isPlaying = false;
            if (loop) yield return new WaitForSeconds(loopDelay);
        } while (loop);

        sequenceRoutine = null;
    }

    private IEnumerator AttractionOnlyRoutine()
    {
        isPlaying = true;
        PlaceAtStart();
        yield return FirstPairRoutine();
        isPlaying = false;
        sequenceRoutine = null;
    }

    private IEnumerator LatticeOnlyRoutine()
    {
        isPlaying = true;
        yield return BuildLatticeRoutine();
        isPlaying = false;
        sequenceRoutine = null;
    }

    private void PlaceAtStart()
    {
        Vector3 c = transform.position;
        sodiumAtom.transform.SetParent(transform, true);
        chlorineAtom.transform.SetParent(transform, true);
        sodiumAtom.transform.position = c + Vector3.left * (startingSeparation * 0.5f);
        chlorineAtom.transform.position = c + Vector3.right * (startingSeparation * 0.5f);
    }

    private IEnumerator FirstPairRoutine()
    {
        Vector3 c = transform.position;
        Vector3 naTarget = c + Vector3.left * (pairSpacing * 0.5f);
        Vector3 clTarget = c + Vector3.right * (pairSpacing * 0.5f);

        Vector3 naStart = sodiumAtom.transform.position;
        Vector3 clStart = chlorineAtom.transform.position;
        Vector3 naScale = sodiumAtom.transform.localScale;
        Vector3 clScale = chlorineAtom.transform.localScale;

        PlayClip(attractionClip);

        float t = 0f;
        while (t < attractionDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / attractionDuration);
            float e = movementEase.Evaluate(n);

            sodiumAtom.transform.position = Vector3.Lerp(naStart, naTarget, e);
            chlorineAtom.transform.position = Vector3.Lerp(clStart, clTarget, e);

            if (attractionPulse)
            {
                float p = 1f + Mathf.Sin(n * Mathf.PI * pulseSpeed) * pulseAmount * (1f - n);
                sodiumAtom.transform.localScale = naScale * p;
                chlorineAtom.transform.localScale = clScale * p;
            }
            yield return null;
        }

        sodiumAtom.transform.position = naTarget;
        chlorineAtom.transform.position = clTarget;
        sodiumAtom.transform.localScale = naScale;
        chlorineAtom.transform.localScale = clScale;
        currentIonCount = 2;

        if (firstPairImpactFX != null)
        {
            firstPairImpactFX.transform.position = (naTarget + clTarget) * 0.5f;
            firstPairImpactFX.Play();
        }
        PlayClip(pairJoinClip);
    }

    private IEnumerator BuildLatticeRoutine()
    {
        List<Slot> slots = GenerateSlots();

        int naIndex = FindClosest(slots, transform.position, true);
        Vector3 naTarget = slots[naIndex].position;
        slots.RemoveAt(naIndex);

        int clIndex = FindClosest(slots, naTarget, false);
        Vector3 clTarget = slots[clIndex].position;
        slots.RemoveAt(clIndex);

        yield return MovePair(sodiumAtom.transform, naTarget, chlorineAtom.transform, clTarget, pairJoinDuration);

        List<Slot> naSlots = new List<Slot>();
        List<Slot> clSlots = new List<Slot>();
        foreach (var s in slots)
        {
            if (s.isSodium) naSlots.Add(s); else clSlots.Add(s);
        }
        SortSlots(naSlots);
        SortSlots(clSlots);

        int count = Mathf.Max(naSlots.Count, clSlots.Count);
        int previousLayer = -1;

        for (int i = 0; i < count; i++)
        {
            Slot? ns = i < naSlots.Count ? naSlots[i] : (Slot?)null;
            Slot? cs = i < clSlots.Count ? clSlots[i] : (Slot?)null;
            int layer = ns.HasValue ? ns.Value.layer : cs.Value.layer;

            if (layerByLayer && previousLayer >= 0 && layer != previousLayer && layerPause > 0f)
                yield return new WaitForSeconds(layerPause);
            previousLayer = layer;

            GameObject na = ns.HasValue ? SpawnIncoming(sodiumAtom, ns.Value.position, true, i) : null;
            GameObject cl = cs.HasValue ? SpawnIncoming(chlorineAtom, cs.Value.position, false, i) : null;

            yield return FlyPair(na, ns.HasValue ? ns.Value.position : Vector3.zero,
                                 cl, cs.HasValue ? cs.Value.position : Vector3.zero);

            if (na != null) currentIonCount++;
            if (cl != null) currentIonCount++;
            PlayClip(pairJoinClip);

            if (delayBetweenPairs > 0f) yield return new WaitForSeconds(delayBetweenPairs);
        }

        if (latticeCompleteFX != null)
        {
            latticeCompleteFX.transform.position = transform.position;
            latticeCompleteFX.Play();
        }
        PlayClip(latticeCompleteClip);
    }

    private GameObject SpawnIncoming(GameObject source, Vector3 target, bool sodium, int index)
    {
        GameObject clone = Instantiate(source, transform);
        clone.name = (sodium ? "Na_Lattice_" : "Cl_Lattice_") + index.ToString("00");
        clone.SetActive(true);

        Vector3[] dirs = { Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        Vector3 dir = dirs[(index + (sodium ? 0 : 3)) % dirs.Length];
        clone.transform.position = target + dir * incomingDistance;
        spawnedIons.Add(clone);
        return clone;
    }

    private IEnumerator FlyPair(GameObject na, Vector3 naTarget, GameObject cl, Vector3 clTarget)
    {
        Vector3 naStart = na ? na.transform.position : Vector3.zero;
        Vector3 clStart = cl ? cl.transform.position : Vector3.zero;
        float t = 0f;

        while (t < pairJoinDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / pairJoinDuration);
            float e = movementEase.Evaluate(n);
            float arc = Mathf.Sin(e * Mathf.PI) * incomingArcHeight;

            if (na)
            {
                Vector3 p = Vector3.Lerp(naStart, naTarget, e);
                na.transform.position = p + Vector3.up * arc;
            }
            if (cl)
            {
                Vector3 p = Vector3.Lerp(clStart, clTarget, e);
                cl.transform.position = p + Vector3.up * arc;
            }
            yield return null;
        }

        if (na) na.transform.position = naTarget;
        if (cl) cl.transform.position = clTarget;
    }

    private IEnumerator MovePair(Transform a, Vector3 at, Transform b, Vector3 bt, float duration)
    {
        Vector3 a0 = a.position;
        Vector3 b0 = b.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float n = movementEase.Evaluate(Mathf.Clamp01(t / duration));
            a.position = Vector3.Lerp(a0, at, n);
            b.position = Vector3.Lerp(b0, bt, n);
            yield return null;
        }
        a.position = at;
        b.position = bt;
    }

    private List<Slot> GenerateSlots()
    {
        List<Slot> result = new List<Slot>();
        float half = (latticeSize - 1) * latticeSpacing * 0.5f;

        for (int z = 0; z < latticeSize; z++)
        for (int y = 0; y < latticeSize; y++)
        for (int x = 0; x < latticeSize; x++)
        {
            Vector3 local = new Vector3(x * latticeSpacing - half, y * latticeSpacing - half, z * latticeSpacing - half);
            result.Add(new Slot
            {
                position = transform.TransformPoint(local),
                isSodium = ((x + y + z) % 2 == 0),
                layer = z,
                distance = local.sqrMagnitude
            });
        }
        return result;
    }

    private int FindClosest(List<Slot> slots, Vector3 point, bool sodium)
    {
        int best = 0;
        float dBest = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isSodium != sodium) continue;
            float d = (slots[i].position - point).sqrMagnitude;
            if (d < dBest) { dBest = d; best = i; }
        }
        return best;
    }

    private void SortSlots(List<Slot> list)
    {
        list.Sort((a, b) =>
        {
            if (layerByLayer)
            {
                int c = a.layer.CompareTo(b.layer);
                if (c != 0) return c;
            }
            return a.distance.CompareTo(b.distance);
        });
    }

    private void BuildAllInstantly()
    {
        List<Slot> slots = GenerateSlots();
        int ni = FindClosest(slots, transform.position, true);
        sodiumAtom.transform.position = slots[ni].position;
        slots.RemoveAt(ni);
        int ci = FindClosest(slots, sodiumAtom.transform.position, false);
        chlorineAtom.transform.position = slots[ci].position;
        slots.RemoveAt(ci);

        int i = 0;
        foreach (var s in slots)
        {
            GameObject src = s.isSodium ? sodiumAtom : chlorineAtom;
            GameObject clone = Instantiate(src, s.position, src.transform.rotation, transform);
            clone.name = (s.isSodium ? "Na_Lattice_" : "Cl_Lattice_") + i.ToString("00");
            clone.SetActive(true);
            spawnedIons.Add(clone);
            i++;
        }
        currentIonCount = latticeSize * latticeSize * latticeSize;
    }

    private bool Ready()
    {
        if (sodiumAtom == null || chlorineAtom == null)
        {
            Debug.LogWarning("Assign Sodium Atom and Chlorine Atom in the Inspector.", this);
            return false;
        }
        return true;
    }

    private void StopSequence()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }
        isPlaying = false;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private static void StopFX(ParticleSystem ps)
    {
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private struct Slot
    {
        public Vector3 position;
        public bool isSodium;
        public int layer;
        public float distance;
    }
}
