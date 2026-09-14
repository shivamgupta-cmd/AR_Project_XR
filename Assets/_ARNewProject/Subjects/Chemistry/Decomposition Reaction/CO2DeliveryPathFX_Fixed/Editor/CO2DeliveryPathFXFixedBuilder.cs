using UnityEngine;
using System.Collections.Generic;

public class FormulaObjectPathFollower : MonoBehaviour
{
    // =========================================================
    // PATH
    // =========================================================
   
    [Header("PATH")]
    [Tooltip("Assign the SAME P0-P6 points used by the bubble path.")]
    public Transform[] waypoints;


    // =========================================================
    // OBJECT
    // =========================================================

    [Header("OBJECT")]

    [Tooltip("Drag your CO2 molecule / formula prefab here.")]
    public GameObject formulaPrefab;

    [Tooltip("Total number of objects travelling continuously.")]
    [Min(1)]
    public int objectCount = 15;


    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("MOVEMENT")]

    [Tooltip("Main movement speed. Lower = slower.")]
    [Min(0.001f)]
    public float moveSpeed = 0.15f;

    [Tooltip("Random speed difference between individual objects.")]
    [Range(0f, 0.5f)]
    public float speedVariation = 0.15f;

    [Tooltip("Automatically start when scene starts.")]
    public bool playOnStart = false;


    // =========================================================
    // RANDOM FLOW
    // =========================================================

    [Header("RANDOM FLOW")]

    [Tooltip(
        "Adds randomness to initial positions. " +
        "1 = objects can begin anywhere on the full path."
    )]
    [Range(0f, 1f)]
    public float randomPathDistribution = 1f;

    [Tooltip(
        "Small forward/backward motion while travelling."
    )]
    [Range(0f, 0.05f)]
    public float forwardBackwardVariation = 0.006f;

    [Tooltip(
        "Speed of forward/backward variation."
    )]
    public float forwardBackwardSpeed = 0.7f;


    // =========================================================
    // SIDE SPREAD
    // =========================================================

    [Header("SIDE SPREAD")]

    [Tooltip(
        "How far objects can spread away from path center."
    )]
    [Range(0f, 0.05f)]
    public float spreadAmount = 0.004f;

    [Tooltip("Vertical spread amount.")]
    [Range(0f, 1f)]
    public float verticalSpreadMultiplier = 0.6f;

    [Tooltip("Depth spread amount.")]
    [Range(0f, 1f)]
    public float depthSpreadMultiplier = 0.6f;


    // =========================================================
    // WOBBLE
    // =========================================================

    [Header("FLOATING WOBBLE")]

    [Tooltip("Small natural floating movement.")]
    [Range(0f, 0.02f)]
    public float wobbleAmount = 0.001f;

    [Tooltip("Wobble movement speed.")]
    public float wobbleSpeed = 1.5f;


    // =========================================================
    // ROTATION
    // =========================================================

    [Header("ROTATION")]

    public bool keepOriginalRotation = true;

    public bool faceTravelDirection = false;

    public Vector3 rotationOffset;


    // =========================================================
    // PATH CALCULATION
    // =========================================================

    [Header("PATH CALCULATION")]

    [Range(20, 200)]
    public int pathSamples = 100;


    // =========================================================
    // INTERNAL OBJECT DATA
    // =========================================================

    private class MovingObject
    {
        public GameObject gameObject;

        // Position through the path 0-1
        public float progress;

        // Different speed for each object
        public float speedMultiplier;

        // Unique spread
        public Vector3 spread;

        // Unique animation phase
        public float phase;

        public Quaternion originalRotation;
    }


    private readonly List<MovingObject> objects =
        new List<MovingObject>();


    private float pathLength;

    private bool isPlaying;


    // =========================================================
    // UNITY
    // =========================================================

    void Start()
    {
        RefreshPath();

        CreateObjects();

        if (playOnStart)
        {
            PlayObjects();
        }
        else
        {
            SetObjectsActive(false);
        }
    }


    void Update()
    {
        if (!isPlaying)
            return;


        if (waypoints == null ||
            waypoints.Length < 2)
            return;


        if (moveSpeed <= 0f)
            return;


        UpdateObjects();
    }


    // =========================================================
    // CREATE OBJECTS
    // =========================================================

    public void CreateObjects()
    {
        ClearObjects();


        if (formulaPrefab == null)
        {
            Debug.LogWarning(
                "FormulaObjectPathFollower: " +
                "Formula Prefab is not assigned."
            );

            return;
        }


        for (int i = 0;
             i < objectCount;
             i++)
        {
            GameObject obj =
                Instantiate(
                    formulaPrefab,
                    transform
                );


            obj.name =
                formulaPrefab.name +
                "_Flow_" +
                (i + 1);


            MovingObject moving =
                new MovingObject();


            moving.gameObject =
                obj;


            // =================================================
            // IMPORTANT:
            // EACH OBJECT GETS ITS OWN PATH POSITION
            // =================================================

            float evenlyDistributed =
                i /
                (float)objectCount;


            float completelyRandom =
                Random.value;


            moving.progress =
                Mathf.Lerp(
                    evenlyDistributed,
                    completelyRandom,
                    randomPathDistribution
                );


            // =================================================
            // UNIQUE SPEED
            // =================================================

            moving.speedMultiplier =
                Random.Range(
                    1f - speedVariation,
                    1f + speedVariation
                );


            // =================================================
            // UNIQUE SIDE SPREAD
            // =================================================

            moving.spread =
                new Vector3(

                    Random.Range(
                        -spreadAmount,
                        spreadAmount
                    ),

                    Random.Range(
                        -spreadAmount,
                        spreadAmount
                    )
                    *
                    verticalSpreadMultiplier,

                    Random.Range(
                        -spreadAmount,
                        spreadAmount
                    )
                    *
                    depthSpreadMultiplier

                );


            // Unique wobble timing
            moving.phase =
                Random.Range(
                    0f,
                    Mathf.PI * 2f
                );


            moving.originalRotation =
                obj.transform.rotation;


            objects.Add(
                moving
            );


            obj.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE MOVEMENT
    // =========================================================

    void UpdateObjects()
    {
        if (pathLength <= 0.001f)
            return;


        for (int i = 0;
             i < objects.Count;
             i++)
        {
            MovingObject moving =
                objects[i];


            if (moving.gameObject == null)
                continue;


            // =================================================
            // INDEPENDENT MOVEMENT
            // =================================================

            float normalizedSpeed =
                (
                    moveSpeed *
                    moving.speedMultiplier
                )
                /
                pathLength;


            moving.progress +=
                normalizedSpeed *
                Time.deltaTime;


            // =================================================
            // LOOP CONTINUOUSLY
            // =================================================

            if (moving.progress >= 1f)
            {
                moving.progress -= 1f;

                // Give slightly new randomness
                // whenever it comes back to start.

                moving.speedMultiplier =
                    Random.Range(
                        1f - speedVariation,
                        1f + speedVariation
                    );


                moving.spread =
                    new Vector3(

                        Random.Range(
                            -spreadAmount,
                            spreadAmount
                        ),

                        Random.Range(
                            -spreadAmount,
                            spreadAmount
                        )
                        *
                        verticalSpreadMultiplier,

                        Random.Range(
                            -spreadAmount,
                            spreadAmount
                        )
                        *
                        depthSpreadMultiplier

                    );
            }


            // =================================================
            // FORWARD / BACKWARD VARIATION
            // =================================================

            float pathVariation =
                Mathf.Sin(

                    Time.time *
                    forwardBackwardSpeed +

                    moving.phase

                )
                *
                forwardBackwardVariation;


            float finalProgress =
                Mathf.Repeat(
                    moving.progress +
                    pathVariation,
                    1f
                );


            // =================================================
            // GET POSITION ON PATH
            // =================================================

            Vector3 pathPosition =
                EvaluatePath(
                    finalProgress
                );


            // =================================================
            // FLOATING WOBBLE
            // =================================================

            Vector3 wobble =
                new Vector3(

                    Mathf.Sin(
                        Time.time *
                        wobbleSpeed +
                        moving.phase
                    ),

                    Mathf.Cos(
                        Time.time *
                        wobbleSpeed *
                        0.73f +
                        moving.phase
                    ),

                    Mathf.Sin(
                        Time.time *
                        wobbleSpeed *
                        0.51f +
                        moving.phase
                    )

                )
                *
                wobbleAmount;


            // =================================================
            // FINAL POSITION
            // =================================================

            moving.gameObject.transform.position =
                pathPosition +
                moving.spread +
                wobble;


            // =================================================
            // ROTATION
            // =================================================

            if (faceTravelDirection)
            {
                float lookAhead =
                    Mathf.Repeat(
                        finalProgress +
                        0.003f,
                        1f
                    );


                Vector3 nextPosition =
                    EvaluatePath(
                        lookAhead
                    );


                Vector3 direction =
                    nextPosition -
                    pathPosition;


                if (
                    direction.sqrMagnitude >
                    0.000001f
                )
                {
                    moving.gameObject
                        .transform.rotation =

                        Quaternion.LookRotation(
                            direction.normalized
                        )
                        *
                        Quaternion.Euler(
                            rotationOffset
                        );
                }
            }

            else if (keepOriginalRotation)
            {
                moving.gameObject
                    .transform.rotation =
                    moving.originalRotation;
            }
        }
    }


    // =========================================================
    // PATH LENGTH
    // =========================================================

    public void RefreshPath()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
        {
            return;
        }


        pathLength =
            CalculatePathLength();
    }


    float CalculatePathLength()
    {
        float length =
            0f;


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
    // SAME PATH AS YOUR BUBBLES
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


        int index =
            Mathf.Min(
                Mathf.FloorToInt(
                    scaled
                ),
                n - 2
            );


        float localT =
            scaled -
            index;


        Vector3 p0 =
            waypoints[
                Mathf.Max(
                    index - 1,
                    0
                )
            ].position;


        Vector3 p1 =
            waypoints[
                index
            ].position;


        Vector3 p2 =
            waypoints[
                index + 1
            ].position;


        Vector3 p3 =
            waypoints[
                Mathf.Min(
                    index + 2,
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
                )
                *
                t2

                +

                (
                    -p0 +
                    3f * p1 -
                    3f * p2 +
                    p3
                )
                *
                t3
            );
    }


    // =========================================================
    // PLAY
    // =========================================================

    public void PlayObjects()
    {
        if (objects.Count == 0)
        {
            CreateObjects();
        }


        RefreshPath();


        isPlaying =
            true;


        SetObjectsActive(
            true
        );
    }


    // =========================================================
    // STOP
    // =========================================================

    public void StopObjects()
    {
        isPlaying =
            false;


        SetObjectsActive(
            false
        );
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetObjects()
    {
        isPlaying =
            false;


        // Redistribute everything randomly again.

        for (int i = 0;
             i < objects.Count;
             i++)
        {
            objects[i].progress =
                Random.value;
        }


        SetObjectsActive(
            false
        );
    }


    // =========================================================
    // REBUILD
    // =========================================================

    public void RebuildObjects()
    {
        bool wasPlaying =
            isPlaying;


        CreateObjects();


        if (wasPlaying)
        {
            PlayObjects();
        }
    }


    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearObjects()
    {
        for (int i = 0;
             i < objects.Count;
             i++)
        {
            if (
                objects[i].gameObject != null
            )
            {
                Destroy(
                    objects[i].gameObject
                );
            }
        }


        objects.Clear();
    }


    // =========================================================
    // ACTIVE
    // =========================================================

    void SetObjectsActive(
        bool active
    )
    {
        for (int i = 0;
             i < objects.Count;
             i++)
        {
            if (
                objects[i].gameObject != null
            )
            {
                objects[i]
                    .gameObject
                    .SetActive(
                        active
                    );
            }
        }
    }


    // =========================================================
    // PATH GIZMO
    // =========================================================

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
        {
            return;
        }


        Gizmos.color =
            Color.yellow;


        Vector3 previous =
            EvaluatePath(0f);


        for (int i = 1;
             i <= 100;
             i++)
        {
            float t =
                i /
                100f;


            Vector3 current =
                EvaluatePath(t);


            Gizmos.DrawLine(
                previous,
                current
            );


            previous =
                current;
        }
    }

#endif
}