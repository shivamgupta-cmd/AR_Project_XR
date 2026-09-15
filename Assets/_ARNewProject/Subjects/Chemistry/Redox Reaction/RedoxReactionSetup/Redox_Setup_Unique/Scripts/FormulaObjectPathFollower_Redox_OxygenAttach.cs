using UnityEngine;
using System.Collections.Generic;

public class FormulaObjectPathFollower_Redox_OxygenAttach : MonoBehaviour
{
    [Header("PATH")]
    public Transform[] waypoints;

    [Header("OBJECT")]
    public GameObject formulaPrefab;
    [Min(1)] public int objectCount = 20;

    [Header("CONTINUOUS SPAWNING")]
    [Min(0.01f)] public float spawnInterval = 0.25f;
    [Range(0f, 1f)] public float spawnIntervalRandomness = 0.25f;

    [Header("MOVEMENT")]
    [Min(0.001f)] public float moveSpeed = 0.15f;
    [Range(0f, 0.5f)] public float speedVariation = 0.15f;
    public bool playOnStart = false;

    [Header("SIDE SPREAD")]
    [Range(0f, 0.05f)] public float spreadAmount = 0.004f;
    [Range(0f, 1f)] public float verticalSpreadMultiplier = 0.6f;
    [Range(0f, 1f)] public float depthSpreadMultiplier = 0.6f;

    [Header("FLOATING WOBBLE")]
    [Range(0f, 0.02f)] public float wobbleAmount = 0.001f;
    public float wobbleSpeed = 1.5f;

    [Header("FORWARD / BACKWARD RANDOMNESS")]
    [Range(0f, 0.03f)] public float forwardBackwardVariation = 0.002f;
    public float forwardBackwardSpeed = 0.7f;

    [Header("ROTATION")]
    public bool keepOriginalRotation = true;
    public bool faceTravelDirection = false;
    public Vector3 rotationOffset;

    [Header("PATH CALCULATION")]
    [Range(20, 200)] public int pathSamples = 100;

    [Header("OXYGEN ATTACHMENT")]
    public bool enableOxygenAttachment = true;
    [Tooltip("Place this transform at the exact point where oxygen should attach.")]
    public Transform oxygenAttachPoint;
    [Tooltip("Assign oxygen objects already present in the tube.")]
    public GameObject[] oxygenParticles;
    [Tooltip("Where oxygen sits after becoming child of hydrogen.")]
    public Vector3 oxygenLocalPosition = new Vector3(0.03f, 0f, 0f);
    public Vector3 oxygenLocalEuler = Vector3.zero;
    [Range(0f, 0.1f)] public float oxygenAttachTolerance = 0.005f;
    public bool resetOxygenAtPathEnd = true;

    class MovingObject
    {
        public GameObject gameObject;
        public bool active;
        public float progress;
        public float speedMultiplier;
        public Vector3 spread;
        public float phase;
        public Quaternion originalRotation;
        public GameObject attachedOxygen;
        public bool oxygenAttached;
    }

    class OxygenData
    {
        public GameObject gameObject;
        public Transform originalParent;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;
        public bool originalActive;
        public bool inUse;
    }

    readonly List<MovingObject> objects = new List<MovingObject>();
    readonly List<OxygenData> oxygenPool = new List<OxygenData>();

    float pathLength;
    float oxygenAttachProgress = 0.5f;
    bool isPlaying;
    float spawnTimer;
    float nextSpawnTime;

    void Start()
    {
        RefreshPath();
        CacheOxygenParticles();
        CreatePool();

        if (playOnStart) PlayObjects();
        else HideAllObjects();
    }

    void Update()
    {
        if (!isPlaying || waypoints == null || waypoints.Length < 2) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnTime)
        {
            SpawnObject();
            spawnTimer = 0f;
            CalculateNextSpawnTime();
        }

        UpdateObjects();
    }

    void CreatePool()
    {
        ClearObjects();
        if (formulaPrefab == null) return;

        for (int i = 0; i < objectCount; i++)
        {
            GameObject obj = Instantiate(formulaPrefab, transform);
            obj.name = formulaPrefab.name + "_Flow_" + (i + 1);

            MovingObject m = new MovingObject();
            m.gameObject = obj;
            m.originalRotation = obj.transform.rotation;
            obj.SetActive(false);
            objects.Add(m);
        }
    }

    void CacheOxygenParticles()
    {
        oxygenPool.Clear();
        if (oxygenParticles == null) return;

        foreach (GameObject oxygen in oxygenParticles)
        {
            if (oxygen == null) continue;

            OxygenData data = new OxygenData();
            data.gameObject = oxygen;
            data.originalParent = oxygen.transform.parent;
            data.originalPosition = oxygen.transform.position;
            data.originalRotation = oxygen.transform.rotation;
            data.originalScale = oxygen.transform.localScale;
            data.originalActive = oxygen.activeSelf;
            data.inUse = false;
            oxygenPool.Add(data);
        }
    }

    void SpawnObject()
    {
        MovingObject m = GetAvailableObject();
        if (m == null) return;

        if (m.attachedOxygen != null) ResetSpecificOxygen(m.attachedOxygen);

        m.progress = 0f;
        m.active = true;
        m.oxygenAttached = false;
        m.attachedOxygen = null;
        m.speedMultiplier = Random.Range(1f - speedVariation, 1f + speedVariation);
        m.spread = new Vector3(
            Random.Range(-spreadAmount, spreadAmount),
            Random.Range(-spreadAmount, spreadAmount) * verticalSpreadMultiplier,
            Random.Range(-spreadAmount, spreadAmount) * depthSpreadMultiplier);
        m.phase = Random.Range(0f, Mathf.PI * 2f);
        m.gameObject.transform.position = EvaluatePath(0f) + m.spread;
        m.gameObject.transform.rotation = m.originalRotation;
        m.gameObject.SetActive(true);
    }

    MovingObject GetAvailableObject()
    {
        foreach (MovingObject m in objects) if (!m.active) return m;
        return null;
    }

    OxygenData GetAvailableOxygen()
    {
        foreach (OxygenData o in oxygenPool)
            if (o.gameObject != null && !o.inUse) return o;
        return null;
    }

    void UpdateObjects()
    {
        if (pathLength <= 0.001f) return;

        foreach (MovingObject m in objects)
        {
            if (!m.active || m.gameObject == null) continue;

            float normalizedSpeed = (moveSpeed * m.speedMultiplier) / pathLength;
            m.progress += normalizedSpeed * Time.deltaTime;

            if (m.progress >= 1f)
            {
                if (resetOxygenAtPathEnd && m.attachedOxygen != null)
                    ResetSpecificOxygen(m.attachedOxygen);

                m.attachedOxygen = null;
                m.oxygenAttached = false;
                m.active = false;
                m.progress = 0f;
                m.gameObject.SetActive(false);
                continue;
            }

            if (enableOxygenAttachment && !m.oxygenAttached && oxygenAttachPoint != null &&
                m.progress >= Mathf.Max(0f, oxygenAttachProgress - oxygenAttachTolerance))
            {
                AttachOxygen(m);
            }

            float pathVariation = Mathf.Sin(Time.time * forwardBackwardSpeed + m.phase) * forwardBackwardVariation;
            float finalProgress = Mathf.Clamp01(m.progress + pathVariation);
            Vector3 pathPosition = EvaluatePath(finalProgress);

            Vector3 wobble = new Vector3(
                Mathf.Sin(Time.time * wobbleSpeed + m.phase),
                Mathf.Cos(Time.time * wobbleSpeed * 0.73f + m.phase),
                Mathf.Sin(Time.time * wobbleSpeed * 0.51f + m.phase)) * wobbleAmount;

            m.gameObject.transform.position = pathPosition + m.spread + wobble;

            if (faceTravelDirection)
            {
                float lookAhead = Mathf.Clamp01(finalProgress + 0.003f);
                Vector3 dir = EvaluatePath(lookAhead) - pathPosition;
                if (dir.sqrMagnitude > 0.000001f)
                    m.gameObject.transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(rotationOffset);
            }
            else if (keepOriginalRotation)
            {
                m.gameObject.transform.rotation = m.originalRotation;
            }
        }
    }

    void AttachOxygen(MovingObject m)
    {
        OxygenData oxygen = GetAvailableOxygen();
        if (oxygen == null) return;

        oxygen.inUse = true;
        m.oxygenAttached = true;
        m.attachedOxygen = oxygen.gameObject;

        Transform t = oxygen.gameObject.transform;
        Vector3 originalScale = t.localScale;
        t.SetParent(m.gameObject.transform, false);
        t.localPosition = oxygenLocalPosition;
        t.localRotation = Quaternion.Euler(oxygenLocalEuler);
        t.localScale = originalScale;
        oxygen.gameObject.SetActive(true);
    }

    void ResetSpecificOxygen(GameObject oxygenObject)
    {
        foreach (OxygenData oxygen in oxygenPool)
        {
            if (oxygen.gameObject != oxygenObject) continue;

            Transform t = oxygen.gameObject.transform;
            t.SetParent(oxygen.originalParent, true);
            t.position = oxygen.originalPosition;
            t.rotation = oxygen.originalRotation;
            t.localScale = oxygen.originalScale;
            oxygen.gameObject.SetActive(oxygen.originalActive);
            oxygen.inUse = false;
            return;
        }
    }

    public void ResetAllOxygen()
    {
        foreach (OxygenData oxygen in oxygenPool)
        {
            if (oxygen.gameObject == null) continue;
            Transform t = oxygen.gameObject.transform;
            t.SetParent(oxygen.originalParent, true);
            t.position = oxygen.originalPosition;
            t.rotation = oxygen.originalRotation;
            t.localScale = oxygen.originalScale;
            oxygen.gameObject.SetActive(oxygen.originalActive);
            oxygen.inUse = false;
        }

        foreach (MovingObject m in objects)
        {
            m.attachedOxygen = null;
            m.oxygenAttached = false;
        }
    }

    void CalculateNextSpawnTime()
    {
        float random = Random.Range(1f - spawnIntervalRandomness, 1f + spawnIntervalRandomness);
        nextSpawnTime = Mathf.Max(0.01f, spawnInterval * random);
    }

    public void RefreshPath()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        pathLength = CalculatePathLength();
        CalculateOxygenAttachProgress();
    }

    void CalculateOxygenAttachProgress()
    {
        if (oxygenAttachPoint == null || waypoints == null || waypoints.Length < 2) return;

        float bestDistance = float.MaxValue;
        float bestProgress = 0f;
        int samples = Mathf.Max(pathSamples * 2, 100);

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float d = (EvaluatePath(t) - oxygenAttachPoint.position).sqrMagnitude;
            if (d < bestDistance)
            {
                bestDistance = d;
                bestProgress = t;
            }
        }

        oxygenAttachProgress = bestProgress;
    }

    float CalculatePathLength()
    {
        float length = 0f;
        Vector3 previous = EvaluatePath(0f);
        for (int i = 1; i <= pathSamples; i++)
        {
            float t = i / (float)pathSamples;
            Vector3 current = EvaluatePath(t);
            length += Vector3.Distance(previous, current);
            previous = current;
        }
        return length;
    }

    Vector3 EvaluatePath(float t)
    {
        int n = waypoints.Length;
        if (n == 2) return Vector3.Lerp(waypoints[0].position, waypoints[1].position, t);

        float scaled = t * (n - 1);
        int index = Mathf.Min(Mathf.FloorToInt(scaled), n - 2);
        float localT = scaled - index;

        Vector3 p0 = waypoints[Mathf.Max(index - 1, 0)].position;
        Vector3 p1 = waypoints[index].position;
        Vector3 p2 = waypoints[index + 1].position;
        Vector3 p3 = waypoints[Mathf.Min(index + 2, n - 1)].position;

        float t2 = localT * localT;
        float t3 = t2 * localT;

        return 0.5f * ((2f * p1) + (-p0 + p2) * localT +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    public void PlayObjects()
    {
        RefreshPath();
        HideAllObjects();
        ResetAllOxygen();
        spawnTimer = 0f;
        nextSpawnTime = 0f;
        isPlaying = true;
        SpawnObject();
        CalculateNextSpawnTime();
    }

    public void StopObjects()
    {
        isPlaying = false;
    }

    public void StopAndClearObjects()
    {
        isPlaying = false;
        HideAllObjects();
        ResetAllOxygen();
    }

    public void ResetObjects()
    {
        isPlaying = false;
        spawnTimer = 0f;
        HideAllObjects();
        ResetAllOxygen();
    }

    void HideAllObjects()
    {
        foreach (MovingObject m in objects)
        {
            if (m.attachedOxygen != null) ResetSpecificOxygen(m.attachedOxygen);
            m.attachedOxygen = null;
            m.oxygenAttached = false;
            m.active = false;
            m.progress = 0f;
            if (m.gameObject != null) m.gameObject.SetActive(false);
        }
    }

    void ClearObjects()
    {
        foreach (MovingObject m in objects)
            if (m.gameObject != null) Destroy(m.gameObject);
        objects.Clear();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        Vector3 previous = EvaluatePath(0f);
        for (int i = 1; i <= 100; i++)
        {
            Vector3 current = EvaluatePath(i / 100f);
            Gizmos.DrawLine(previous, current);
            previous = current;
        }

        if (oxygenAttachPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(oxygenAttachPoint.position, 0.025f);
        }
    }
#endif
}
