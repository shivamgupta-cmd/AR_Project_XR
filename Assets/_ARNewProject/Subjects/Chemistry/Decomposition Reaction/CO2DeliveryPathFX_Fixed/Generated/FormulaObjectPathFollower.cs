using UnityEngine;
using System.Collections.Generic;

public class FormulaObjectPathFollower : MonoBehaviour
{
    [Header("PATH")]
    public Transform[] waypoints;

    [Header("OBJECT")]
    public GameObject formulaPrefab;

    [Tooltip("Maximum objects that can exist at the same time.")]
    [Min(1)]
    public int objectCount = 20;

    [Header("CONTINUOUS SPAWNING")]
    [Tooltip("Time between new objects spawning from P0.")]
    [Min(0.01f)]
    public float spawnInterval = 0.25f;

    [Tooltip("Random variation in spawn timing.")]
    [Range(0f, 1f)]
    public float spawnIntervalRandomness = 0.25f;

    [Header("MOVEMENT")]
    [Tooltip("World units travelled per second.")]
    [Min(0.001f)]
    public float moveSpeed = 0.15f;

    [Range(0f, 0.5f)]
    public float speedVariation = 0.15f;

    public bool playOnStart = false;

    [Header("SIDE SPREAD")]
    [Range(0f, 0.05f)]
    public float spreadAmount = 0.004f;

    [Range(0f, 1f)]
    public float verticalSpreadMultiplier = 0.6f;

    [Range(0f, 1f)]
    public float depthSpreadMultiplier = 0.6f;

    [Header("FLOATING WOBBLE")]
    [Range(0f, 0.02f)]
    public float wobbleAmount = 0.001f;

    public float wobbleSpeed = 1.5f;

    [Header("FORWARD / BACKWARD RANDOMNESS")]
    [Range(0f, 0.03f)]
    public float forwardBackwardVariation = 0.002f;

    public float forwardBackwardSpeed = 0.7f;

    [Header("ROTATION")]
    public bool keepOriginalRotation = true;
    public bool faceTravelDirection = false;
    public Vector3 rotationOffset;

    [Header("PATH CALCULATION")]
    [Range(20, 200)]
    public int pathSamples = 100;


    // =========================================================
    // INTERNAL OBJECT
    // =========================================================

    private class MovingObject
    {
        public GameObject gameObject;

        public bool active;

        public float progress;

        public float speedMultiplier;

        public Vector3 spread;

        public float phase;

        public Quaternion originalRotation;
    }


    private readonly List<MovingObject> objects =
        new List<MovingObject>();


    private float pathLength;

    private bool isPlaying;

    private float spawnTimer;

    private float nextSpawnTime;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        RefreshPath();

        CreatePool();

        if (playOnStart)
        {
            PlayObjects();
        }
        else
        {
            HideAllObjects();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (!isPlaying)
            return;

        if (waypoints == null ||
            waypoints.Length < 2)
            return;


        // ---------------------------------------------
        // CONTINUOUS SPAWNER
        // ---------------------------------------------

        spawnTimer += Time.deltaTime;


        if (spawnTimer >= nextSpawnTime)
        {
            SpawnObject();

            spawnTimer = 0f;

            CalculateNextSpawnTime();
        }


        // ---------------------------------------------
        // MOVE ACTIVE OBJECTS
        // ---------------------------------------------

        UpdateObjects();
    }


    // =========================================================
    // CREATE OBJECT POOL
    // =========================================================

    void CreatePool()
    {
        ClearObjects();


        if (formulaPrefab == null)
        {
            Debug.LogWarning(
                "Formula Prefab is not assigned."
            );

            return;
        }


        for (int i = 0; i < objectCount; i++)
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


            moving.originalRotation =
                obj.transform.rotation;


            moving.active =
                false;


            obj.SetActive(false);


            objects.Add(
                moving
            );
        }
    }


    // =========================================================
    // SPAWN ONE OBJECT
    // =========================================================

    void SpawnObject()
    {
        MovingObject moving =
            GetAvailableObject();


        // No free object in pool.
        if (moving == null)
            return;


        // ALWAYS START FROM P0
        moving.progress =
            0f;


        moving.active =
            true;


        // ---------------------------------------------
        // RANDOM SPEED
        // ---------------------------------------------

        moving.speedMultiplier =
            Random.Range(
                1f - speedVariation,
                1f + speedVariation
            );


        // ---------------------------------------------
        // RANDOM SPREAD
        // ---------------------------------------------

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


        // ---------------------------------------------
        // RANDOM WOBBLE PHASE
        // ---------------------------------------------

        moving.phase =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );


        // ---------------------------------------------
        // PUT EXACTLY AT START
        // ---------------------------------------------

        moving.gameObject
            .transform.position =

            EvaluatePath(0f) +
            moving.spread;


        moving.gameObject
            .transform.rotation =
            moving.originalRotation;


        moving.gameObject
            .SetActive(true);
    }


    // =========================================================
    // FIND UNUSED OBJECT
    // =========================================================

    MovingObject GetAvailableObject()
    {
        for (int i = 0;
             i < objects.Count;
             i++)
        {
            if (!objects[i].active)
            {
                return objects[i];
            }
        }


        return null;
    }


    // =========================================================
    // UPDATE ACTIVE OBJECTS
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


            if (!moving.active)
                continue;


            if (moving.gameObject == null)
                continue;


            // ---------------------------------------------
            // MOVE FORWARD
            // ---------------------------------------------

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
            // REACHED P6
            // =================================================

            if (moving.progress >= 1f)
            {
                // REMOVE THIS OBJECT.
                // Spawner will reuse it again from P0.

                moving.active =
                    false;


                moving.progress =
                    0f;


                moving.gameObject
                    .SetActive(false);


                continue;
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


            // Clamp instead of Repeat!
            // This prevents an object near P0 from
            // suddenly appearing near P6.

            float finalProgress =
                Mathf.Clamp01(
                    moving.progress +
                    pathVariation
                );


            // =================================================
            // PATH POSITION
            // =================================================

            Vector3 pathPosition =
                EvaluatePath(
                    finalProgress
                );


            // =================================================
            // WOBBLE
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

            moving.gameObject
                .transform.position =

                pathPosition +
                moving.spread +
                wobble;


            // =================================================
            // ROTATION
            // =================================================

            if (faceTravelDirection)
            {
                float lookAhead =
                    Mathf.Clamp01(
                        finalProgress +
                        0.003f
                    );


                Vector3 nextPosition =
                    EvaluatePath(
                        lookAhead
                    );


                Vector3 direction =
                    nextPosition -
                    pathPosition;


                if (direction.sqrMagnitude >
                    0.000001f)
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
    // NEXT SPAWN TIME
    // =========================================================

    void CalculateNextSpawnTime()
    {
        float random =
            Random.Range(
                1f - spawnIntervalRandomness,
                1f + spawnIntervalRandomness
            );


        nextSpawnTime =
            Mathf.Max(
                0.01f,
                spawnInterval * random
            );
    }


    // =========================================================
    // PATH LENGTH
    // =========================================================

    public void RefreshPath()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
            return;


        pathLength =
            CalculatePathLength();
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
    // TIMELINE - PLAY
    // =========================================================

    public void PlayObjects()
    {
        RefreshPath();


        // Start fresh.
        HideAllObjects();


        spawnTimer =
            0f;


        nextSpawnTime =
            0f;


        isPlaying =
            true;


        // Spawn first object immediately.
        SpawnObject();


        CalculateNextSpawnTime();
    }


    // =========================================================
    // TIMELINE - STOP
    // =========================================================

    public void StopObjects()
    {
        isPlaying =
            false;
    }


    // =========================================================
    // STOP + REMOVE EVERYTHING
    // =========================================================

    public void StopAndClearObjects()
    {
        isPlaying =
            false;


        HideAllObjects();
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetObjects()
    {
        isPlaying =
            false;


        spawnTimer =
            0f;


        HideAllObjects();
    }


    // =========================================================
    // HIDE POOL
    // =========================================================

    void HideAllObjects()
    {
        for (int i = 0;
             i < objects.Count;
             i++)
        {
            objects[i].active =
                false;


            objects[i].progress =
                0f;


            if (objects[i].gameObject != null)
            {
                objects[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // CLEAR POOL
    // =========================================================

    void ClearObjects()
    {
        for (int i = 0;
             i < objects.Count;
             i++)
        {
            if (objects[i].gameObject != null)
            {
                Destroy(
                    objects[i].gameObject
                );
            }
        }


        objects.Clear();
    }


    // =========================================================
    // GIZMO
    // =========================================================

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (waypoints == null ||
            waypoints.Length < 2)
            return;


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