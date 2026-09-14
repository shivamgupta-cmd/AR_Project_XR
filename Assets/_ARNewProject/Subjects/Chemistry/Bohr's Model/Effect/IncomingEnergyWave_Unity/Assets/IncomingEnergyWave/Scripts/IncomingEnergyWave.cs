using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class IncomingEnergyWave : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Transform startPoint;

    [Header("Wave")]
    [Min(8)] public int points = 48;
    public float amplitude = 0.08f;
    public float frequency = 5f;
    public float waveScrollSpeed = 10f;
    public float travelSpeed = 1.8f;
    public float width = 0.025f;
    [Range(0f, 360f)]
    public float waveAxisRotation = 0f;

    [Header("Impact")]
    public ParticleSystem impactParticles;
    public float impactDistance = 0.04f;
    public bool hideAfterImpact = true;
    public bool playOnStart = true;

    [Header("Repeat")]
    public bool autoRepeat = false;
    public float repeatDelay = 1.5f;

    [Header("Events")]
    public UnityEvent onEnergyAbsorbed;

    private LineRenderer line;
    private Vector3 origin;
    private Vector3 front;
    private float phase;
    private bool travelling;
    private float repeatTimer;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
    }

    void Start()
    {
        if (playOnStart) PlayWave();
        else line.enabled = false;
    }

    public void PlayWave()
    {
        if (target == null)
        {
            Debug.LogWarning("Assign the electron Transform to Target.", this);
            return;
        }

        origin = startPoint != null ? startPoint.position : transform.position;
        front = origin;
        phase = 0f;
        repeatTimer = 0f;
        travelling = true;

        line.enabled = true;
        line.positionCount = points;
        line.startWidth = width;
        line.endWidth = width * 0.45f;

        if (impactParticles != null)
            impactParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void StopWave()
    {
        travelling = false;
        line.enabled = false;
    }

    void Update()
    {
        if (target == null) return;

        if (travelling)
        {
            front = Vector3.MoveTowards(front, target.position, travelSpeed * Time.deltaTime);
            phase += waveScrollSpeed * Time.deltaTime;
            DrawWave(origin, front);

            if (Vector3.Distance(front, target.position) <= impactDistance)
            {
                travelling = false;

                if (impactParticles != null)
                {
                    impactParticles.transform.position = target.position;
                    impactParticles.Play();
                }

                onEnergyAbsorbed?.Invoke();
                if (hideAfterImpact) line.enabled = false;
            }
        }
        else if (autoRepeat)
        {
            repeatTimer += Time.deltaTime;
            if (repeatTimer >= repeatDelay) PlayWave();
        }
    }

    void DrawWave(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        if (direction.sqrMagnitude < 0.000001f) return;

        Vector3 forward = direction.normalized;
        Vector3 reference = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
        Vector3 side = Vector3.Cross(forward, reference).normalized;

        // Rotate the wave axis around its travelling direction
        side = Quaternion.AngleAxis(waveAxisRotation, forward) * side;

        line.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);
            Vector3 basePos = Vector3.Lerp(from, to, t);
            float taper = Mathf.Lerp(1f, 0.25f, t);
            float sine = Mathf.Sin((t * frequency * Mathf.PI * 2f) - phase);
            line.SetPosition(i, basePos + side * sine * amplitude * taper);
        }
    }
}
