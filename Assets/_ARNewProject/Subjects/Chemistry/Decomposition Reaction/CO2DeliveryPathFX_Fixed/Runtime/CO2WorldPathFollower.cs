using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class CO2WorldPathFollower : MonoBehaviour
{
    [Header("PATH")]
    [Tooltip("Assign path points from CaCO3 to the end of the delivery tube.")]
    public Transform[] waypoints;


    [Header("BUBBLE SPEED")]
    [Tooltip("Actual movement speed of the bubbles. Lower = slower.")]
    [Min(0.001f)]
    public float bubbleSpeed = 0.20f;

    [Tooltip("Extra lifetime after the bubble reaches the end.")]
    public float lifetimeBuffer = 0.5f;


    [Header("BUBBLE SPREAD")]
    [Tooltip("How far each bubble can move away from the center path.")]
    [Range(0f, 0.05f)]
    public float spreadAmount = 0.007f;

    [Tooltip("Spread on vertical axis.")]
    [Range(0f, 1f)]
    public float verticalSpreadMultiplier = 0.5f;

    [Tooltip("Spread depth on Z axis.")]
    [Range(0f, 1f)]
    public float depthSpreadMultiplier = 0.5f;


    [Header("FLOATING MOTION")]
    [Tooltip("Small animated movement while bubbles travel.")]
    [Range(0f, 0.02f)]
    public float wobbleAmount = 0.002f;

    [Tooltip("How quickly the wobble moves.")]
    public float wobbleSpeed = 2.5f;


    [Header("PATH CALCULATION")]
    [Tooltip("Higher = more accurate path length calculation.")]
    [Range(20, 200)]
    public int pathSamples = 100;


    [Header("PLAYBACK")]
    public bool playOnStart = false;


    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private float pathLength;
    private float travelDuration;


    // =========================================================
    // UNITY
    // =========================================================

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;

        // IMPORTANT:
        // Particles and path both work in WORLD SPACE.
        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        particles =
            new ParticleSystem.Particle[
                Mathf.Max(128, main.maxParticles)
            ];

        RefreshPath();
    }


    void Start()
    {
        RefreshPath();

        if (playOnStart)
        {
            PlayCO2();
        }
        else
        {
            ps.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }
    }


    void LateUpdate()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
            return;

        if (bubbleSpeed <= 0f)
            return;

        int count =
            ps.GetParticles(particles);


        for (int i = 0; i < count; i++)
        {
            // -----------------------------
            // GET PARTICLE AGE
            // -----------------------------

            float age =
                particles[i].startLifetime -
                particles[i].remainingLifetime;


            // -----------------------------
            // SPEED CONTROL
            // -----------------------------

            // Bubble speed determines
            // how far along the path it travels.

            float distanceTravelled =
                age * bubbleSpeed;

            float normalizedDistance =
                distanceTravelled /
                Mathf.Max(0.001f, pathLength);

            float t =
                Mathf.Clamp01(
                    normalizedDistance
                );


            // -----------------------------
            // GET PATH POSITION
            // -----------------------------

            Vector3 pathPosition =
                EvaluatePath(t);


            // -----------------------------
            // UNIQUE RANDOM VALUES
            // -----------------------------

            uint seed =
                particles[i].randomSeed;

            float rx =
                RandomFromSeed(
                    seed,
                    12.9898f
                );

            float ry =
                RandomFromSeed(
                    seed,
                    78.233f
                );

            float rz =
                RandomFromSeed(
                    seed,
                    37.719f
                );


            // Convert from 0..1
            // into -1..1

            rx = rx * 2f - 1f;
            ry = ry * 2f - 1f;
            rz = rz * 2f - 1f;


            // -----------------------------
            // STATIC SPREAD
            // -----------------------------

            Vector3 spread =
                new Vector3(
                    rx * spreadAmount,

                    ry *
                    spreadAmount *
                    verticalSpreadMultiplier,

                    rz *
                    spreadAmount *
                    depthSpreadMultiplier
                );


            // -----------------------------
            // ANIMATED WOBBLE
            // -----------------------------

            float seedOffset =
                seed * 0.0001f;


            Vector3 wobble =
                new Vector3(

                    Mathf.Sin(
                        age * wobbleSpeed +
                        seedOffset
                    ),

                    Mathf.Cos(
                        age *
                        wobbleSpeed *
                        0.73f +
                        seedOffset
                    ),

                    Mathf.Sin(
                        age *
                        wobbleSpeed *
                        0.51f +
                        seedOffset
                    )

                ) * wobbleAmount;


            // -----------------------------
            // FINAL PARTICLE POSITION
            // -----------------------------

            particles[i].position =
                pathPosition +
                spread +
                wobble;


            // Script controls movement.
            particles[i].velocity =
                Vector3.zero;
        }


        ps.SetParticles(
            particles,
            count
        );
    }



    // =========================================================
    // CALCULATE PATH
    // =========================================================

    public void RefreshPath()
    {
        if (ps == null)
            ps =
                GetComponent<ParticleSystem>();


        if (waypoints == null ||
            waypoints.Length < 2)
            return;


        pathLength =
            CalculatePathLength();


        travelDuration =
            pathLength /
            Mathf.Max(
                0.001f,
                bubbleSpeed
            );


        // -----------------------------
        // AUTOMATIC PARTICLE LIFETIME
        // -----------------------------

        var main = ps.main;

        main.startLifetime =
            travelDuration +
            lifetimeBuffer;


#if UNITY_EDITOR

        if (!Application.isPlaying)
        {
            Debug.Log(
                "CO2 Path Length: "
                + pathLength.ToString("F2")
                +
                " | Travel Duration: "
                + travelDuration.ToString("F2")
                + " seconds"
            );
        }

#endif
    }



    float CalculatePathLength()
    {
        float length = 0f;

        Vector3 previous =
            EvaluatePath(0f);


        for (int i = 1;
             i <= pathSamples;
             i++)
        {
            float t =
                i /
                (float)pathSamples;


            Vector3 current =
                EvaluatePath(t);


            length +=
                Vector3.Distance(
                    previous,
                    current
                );


            previous =
                current;
        }


        return length;
    }



    // =========================================================
    // PATH
    // =========================================================

    Vector3 EvaluatePath(float t)
    {
        int n =
            waypoints.Length;


        if (n == 2)
        {
            return Vector3.Lerp(
                waypoints[0].position,
                waypoints[1].position,
                t
            );
        }


        float scaled =
            t *
            (n - 1);


        int i =
            Mathf.Min(
                Mathf.FloorToInt(
                    scaled
                ),
                n - 2
            );


        float localT =
            scaled - i;


        Vector3 p0 =
            waypoints[
                Mathf.Max(
                    i - 1,
                    0
                )
            ].position;


        Vector3 p1 =
            waypoints[i].position;


        Vector3 p2 =
            waypoints[
                i + 1
            ].position;


        Vector3 p3 =
            waypoints[
                Mathf.Min(
                    i + 2,
                    n - 1
                )
            ].position;


        float t2 =
            localT *
            localT;


        float t3 =
            t2 *
            localT;


        return
            0.5f *
            (
                (2f * p1)

                +

                (-p0 + p2) *
                localT

                +

                (
                    2f * p0 -
                    5f * p1 +
                    4f * p2 -
                    p3
                ) *
                t2

                +

                (
                    -p0 +
                    3f * p1 -
                    3f * p2 +
                    p3
                ) *
                t3
            );
    }



    // =========================================================
    // RANDOM
    // =========================================================

    float RandomFromSeed(
        uint seed,
        float multiplier
    )
    {
        float value =
            Mathf.Sin(
                seed *
                multiplier
            ) *
            43758.5453f;


        return
            value -
            Mathf.Floor(value);
    }



    // =========================================================
    // PUBLIC CONTROLS
    // =========================================================

    public void PlayCO2()
    {
        RefreshPath();

        if (!ps)
            ps =
                GetComponent<ParticleSystem>();

        ps.Play(true);
    }


    public void StopCO2()
    {
        if (ps)
        {
            ps.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmitting
            );
        }
    }


    public void ClearCO2()
    {
        if (ps)
        {
            ps.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
        }
    }



    // =========================================================
    // EDITOR
    // =========================================================

#if UNITY_EDITOR

    void OnValidate()
    {
        if (!Application.isPlaying &&
            GetComponent<ParticleSystem>() != null)
        {
            ps =
                GetComponent<ParticleSystem>();

            RefreshPath();
        }
    }


    void OnDrawGizmos()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
            return;


        Gizmos.color =
            Color.cyan;


        Vector3 previous =
            EvaluatePath(0f);


        for (int i = 1;
             i <= 100;
             i++)
        {
            float t =
                i / 100f;


            Vector3 current =
                EvaluatePath(t);


            Gizmos.DrawLine(
                previous,
                current
            );


            previous =
                current;
        }


        foreach (
            Transform waypoint
            in waypoints
        )
        {
            if (!waypoint)
                continue;


            Gizmos.DrawWireSphere(
                waypoint.position,
                0.025f
            );
        }
    }

#endif
}