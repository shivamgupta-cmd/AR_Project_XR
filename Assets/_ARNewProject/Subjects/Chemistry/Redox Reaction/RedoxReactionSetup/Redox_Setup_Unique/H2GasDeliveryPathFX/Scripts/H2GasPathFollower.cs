using UnityEngine;

/// <summary>
/// Moves particle positions along a Catmull-Rom path in WORLD SPACE.
/// Attach this to the same GameObject as the travelling ParticleSystem.
/// Designed for continuous educational H2 gas visualization.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class H2GasPathFollower : MonoBehaviour
{
    [Header("Path")]
    public Transform[] pathPoints;

    [Header("Travel")]
    [Min(0.01f)] public float travelTime = 3.0f;
    public bool loop = true;

    [Header("Appearance")]
    [Range(0f, 0.08f)] public float randomPathRadius = 0.018f;
    [Range(0f, 3f)] public float wobbleAmount = 0.35f;
    [Range(0f, 10f)] public float wobbleSpeed = 3.0f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (ps == null || pathPoints == null || pathPoints.Length < 2)
            return;

        int max = Mathf.Max(ps.main.maxParticles, 8);
        if (particles == null || particles.Length < max)
            particles = new ParticleSystem.Particle[max];

        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            float totalLifetime = particles[i].startLifetime;
            if (totalLifetime <= 0.0001f) continue;

            float age = totalLifetime - particles[i].remainingLifetime;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, travelTime));

            if (loop)
                t = Mathf.Repeat(age / Mathf.Max(0.01f, travelTime), 1f);

            Vector3 pos = EvaluateWorldPath(t);

            // Stable particle-specific offset so particles don't sit on one exact line.
            float seed = particles[i].randomSeed * 0.000001f;
            Vector3 right = Vector3.right;
            Vector3 up = Vector3.up;

            float w1 = Mathf.Sin((age * wobbleSpeed) + seed * 17.1f);
            float w2 = Mathf.Cos((age * wobbleSpeed * 0.83f) + seed * 11.7f);

            Vector3 offset =
                right * w1 * randomPathRadius * wobbleAmount +
                up    * w2 * randomPathRadius * wobbleAmount;

            particles[i].position = pos + offset;
        }

        ps.SetParticles(particles, count);
    }

    public Vector3 EvaluateWorldPath(float t)
    {
        if (pathPoints.Length == 2)
            return Vector3.Lerp(pathPoints[0].position, pathPoints[1].position, t);

        int segmentCount = pathPoints.Length - 1;
        float scaled = Mathf.Clamp01(t) * segmentCount;

        int seg = Mathf.Min(Mathf.FloorToInt(scaled), segmentCount - 1);
        float localT = scaled - seg;

        Vector3 p0 = pathPoints[Mathf.Max(seg - 1, 0)].position;
        Vector3 p1 = pathPoints[seg].position;
        Vector3 p2 = pathPoints[Mathf.Min(seg + 1, pathPoints.Length - 1)].position;
        Vector3 p3 = pathPoints[Mathf.Min(seg + 2, pathPoints.Length - 1)].position;

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 +
            (-p0 + 3f*p1 - 3f*p2 + p3) * t3
        );
    }
}
