
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Thermometer3DController : MonoBehaviour
{
    // =========================================================
    // CALIBRATION POINT
    // =========================================================

    [Serializable]
    public class TemperatureReferencePoint
    {
        [Tooltip("Temperature represented by this reference point.")]
        public float temperature = 0f;

        [Tooltip("Place this Transform at the exact physical position " +
                 "where the 3D slider should be for this temperature.")]
        public Transform referencePoint;
    }

    // =========================================================
    // TEMPERATURE EVENT
    // =========================================================

    [Serializable]
    public class TemperatureEvent
    {
        public string name = "Boiling";

        public float temperature = 100f;

        public float tolerance = 0.5f;

        public bool fireOnce = true;

        public UnityEvent onReached;

        [HideInInspector]
        public bool fired;
    }

    // =========================================================
    // 3D THERMOMETER
    // =========================================================

    [Header("3D THERMOMETER")]

    [Tooltip("Assign your actual moving 3D thermometer slider / mercury.")]
    public Transform thermometerSlider;


    // =========================================================
    // TEMPERATURE REFERENCE POINTS
    // =========================================================

    [Header("TEMPERATURE CALIBRATION POINTS")]

    [Tooltip(
        "Add physical reference points on your thermometer.\n\n" +
        "Example:\n" +
        "0 C   -> Reference_0\n" +
        "25 C  -> Reference_25\n" +
        "50 C  -> Reference_50\n" +
        "100 C -> Reference_100"
    )]
    public List<TemperatureReferencePoint> temperatureReferencePoints
        = new List<TemperatureReferencePoint>();


    // =========================================================
    // TEMPERATURE
    // =========================================================

    [Header("TEMPERATURE")]

    public float currentTemperature = 20f;

    [Tooltip("Time taken to automatically move to target temperature.")]
    public float automaticMoveDuration = 2f;

    [Tooltip("Smooth automatic thermometer movement.")]
    public AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);


    // =========================================================
    // MANUAL CONTROL
    // =========================================================

    [Header("MANUAL 3D CONTROL")]

    [Tooltip(
        "If enabled, manually moving the physical 3D slider " +
        "will update the temperature."
    )]
    public bool allowManualSliderMovement = true;

    [Tooltip(
        "Keep the manually moved slider constrained between " +
        "your calibration points."
    )]
    public bool clampSliderToTrack = true;


    // =========================================================
    // FLAME
    // =========================================================

    [Header("FLAME")]

    [Tooltip("Assign candleFlame_fx (2).")]
    public ParticleSystem flame;

    [Tooltip("Optional flame root Transform.")]
    public Transform flameTransform;

    [Range(0f, 2f)]
    public float minimumFlameStrength = 0.25f;

    [Range(0f, 3f)]
    public float maximumFlameStrength = 1.5f;


    // =========================================================
    // EVENTS
    // =========================================================
    //[Header("TEMPERATURE EVENT CONTROL")]
    //public bool automaticallyTriggerTemperatureEvents = true;

    [Header("TEMPERATURE EVENTS")]

    public List<TemperatureEvent> temperatureEvents
        = new List<TemperatureEvent>();


    [Header("GENERAL EVENTS")]

    public UnityEvent<float> onTemperatureChanged;


    // =========================================================
    // START
    // =========================================================

    [Header("PLAY")]

    public bool applyTemperatureOnStart = true;


    // =========================================================
    // PRIVATE
    // =========================================================

    private Coroutine moveRoutine;

    private bool automaticMovement;

    private float baseEmission;
    private float baseSpeed;
    private float baseSize;

    private Vector3 baseFlameScale = Vector3.one;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        SortReferencePoints();

        CacheFlame();
    }


    private void Start()
    {
        if (applyTemperatureOnStart)
        {
            SetTemperatureImmediate(currentTemperature);
        }
    }


    private void Update()
    {
        if (!automaticMovement &&
            allowManualSliderMovement &&
            thermometerSlider != null &&
            HasValidReferences())
        {
            ReadTemperatureFrom3DSlider();
        }
    }


    // =========================================================
    // SORT REFERENCES
    // =========================================================

    public void SortReferencePoints()
    {
        temperatureReferencePoints.Sort(
            (a, b) => a.temperature.CompareTo(b.temperature)
        );
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private bool HasValidReferences()
    {
        if (temperatureReferencePoints == null)
            return false;

        int validCount = 0;

        foreach (TemperatureReferencePoint point
                 in temperatureReferencePoints)
        {
            if (point != null &&
                point.referencePoint != null)
            {
                validCount++;
            }
        }

        return validCount >= 2;
    }


    // =========================================================
    // GET MIN TEMPERATURE
    // =========================================================

    private float GetMinimumTemperature()
    {
        SortReferencePoints();

        foreach (TemperatureReferencePoint point
                 in temperatureReferencePoints)
        {
            if (point.referencePoint != null)
            {
                return point.temperature;
            }
        }

        return 0f;
    }


    // =========================================================
    // GET MAX TEMPERATURE
    // =========================================================

    private float GetMaximumTemperature()
    {
        SortReferencePoints();

        for (int i = temperatureReferencePoints.Count - 1;
             i >= 0;
             i--)
        {
            if (temperatureReferencePoints[i].referencePoint != null)
            {
                return temperatureReferencePoints[i].temperature;
            }
        }

        return 100f;
    }


    // =========================================================
    // GET POSITION FOR TEMPERATURE
    // =========================================================

    private Vector3 GetPositionForTemperature(float temperature)
    {
        SortReferencePoints();

        List<TemperatureReferencePoint> validPoints =
            new List<TemperatureReferencePoint>();

        foreach (TemperatureReferencePoint point
                 in temperatureReferencePoints)
        {
            if (point.referencePoint != null)
            {
                validPoints.Add(point);
            }
        }


        if (validPoints.Count == 0)
        {
            return thermometerSlider != null
                ? thermometerSlider.position
                : Vector3.zero;
        }


        // ---------------------------------------------
        // Only one point
        // ---------------------------------------------

        if (validPoints.Count == 1)
        {
            return validPoints[0].referencePoint.position;
        }


        // ---------------------------------------------
        // Below minimum
        // ---------------------------------------------

        if (temperature <= validPoints[0].temperature)
        {
            return validPoints[0].referencePoint.position;
        }


        // ---------------------------------------------
        // Above maximum
        // ---------------------------------------------

        if (temperature >=
            validPoints[validPoints.Count - 1].temperature)
        {
            return validPoints[
                validPoints.Count - 1
            ].referencePoint.position;
        }


        // ---------------------------------------------
        // Find surrounding points
        // ---------------------------------------------

        for (int i = 0;
             i < validPoints.Count - 1;
             i++)
        {
            TemperatureReferencePoint lower =
                validPoints[i];

            TemperatureReferencePoint upper =
                validPoints[i + 1];


            if (temperature >= lower.temperature &&
                temperature <= upper.temperature)
            {
                float t = Mathf.InverseLerp(
                    lower.temperature,
                    upper.temperature,
                    temperature
                );


                return Vector3.Lerp(
                    lower.referencePoint.position,
                    upper.referencePoint.position,
                    t
                );
            }
        }


        return validPoints[0].referencePoint.position;
    }


    // =========================================================
    // AUTOMATIC TEMPERATURE
    // =========================================================

    public void SetTemperature(float temperature)
    {
        MoveToTemperature(temperature);
    }


    public void MoveToTemperature(float temperature)
    {
        if (!HasValidReferences())
        {
            Debug.LogWarning(
                "Thermometer3DController: " +
                "Please assign at least 2 Temperature Reference Points."
            );

            return;
        }


        float min = GetMinimumTemperature();
        float max = GetMaximumTemperature();

        temperature = Mathf.Clamp(
            temperature,
            min,
            max
        );


        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }


        moveRoutine =
            StartCoroutine(
                MoveTemperatureRoutine(temperature)
            );
    }


    // =========================================================
    // IMMEDIATE
    // =========================================================

    public void SetTemperatureImmediate(float temperature)
    {
        if (!HasValidReferences())
            return;


        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }


        moveRoutine = null;

        automaticMovement = false;


        float min = GetMinimumTemperature();
        float max = GetMaximumTemperature();


        temperature = Mathf.Clamp(
            temperature,
            min,
            max
        );


        ApplyTemperatureValue(
            temperature
        );


        MoveSliderToTemperature(
            temperature
        );
    }


    // =========================================================
    // SMOOTH MOVEMENT
    // =========================================================

    private IEnumerator MoveTemperatureRoutine(
        float targetTemperature)
    {
        automaticMovement = true;


        float startTemperature =
            currentTemperature;


        Vector3 startPosition =
            thermometerSlider.position;


        Vector3 targetPosition =
            GetPositionForTemperature(
                targetTemperature
            );


        float timer = 0f;


        float duration =
            Mathf.Max(
                0.01f,
                automaticMoveDuration
            );


        while (timer < duration)
        {
            timer += Time.deltaTime;


            float normalizedTime =
                Mathf.Clamp01(
                    timer / duration
                );


            float smoothTime =
                movementCurve.Evaluate(
                    normalizedTime
                );


            // -----------------------------------------
            // Move physical 3D slider
            // -----------------------------------------

            thermometerSlider.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothTime
                );


            // -----------------------------------------
            // Temperature
            // -----------------------------------------

            float temperature =
                Mathf.Lerp(
                    startTemperature,
                    targetTemperature,
                    smoothTime
                );


            ApplyTemperatureValue(
                temperature
            );


            yield return null;
        }


        thermometerSlider.position =
            targetPosition;


        ApplyTemperatureValue(
            targetTemperature
        );


        automaticMovement = false;

        moveRoutine = null;
    }


    // =========================================================
    // MOVE SLIDER IMMEDIATELY
    // =========================================================

    private void MoveSliderToTemperature(
        float temperature)
    {
        if (thermometerSlider == null)
            return;


        thermometerSlider.position =
            GetPositionForTemperature(
                temperature
            );
    }


    // =========================================================
    // MANUAL 3D SLIDER
    // =========================================================

    public void ReadTemperatureFrom3DSlider()
    {
        if (!HasValidReferences() ||
            thermometerSlider == null)
        {
            return;
        }


        SortReferencePoints();


        List<TemperatureReferencePoint> validPoints =
            new List<TemperatureReferencePoint>();


        foreach (TemperatureReferencePoint point
                 in temperatureReferencePoints)
        {
            if (point.referencePoint != null)
            {
                validPoints.Add(point);
            }
        }


        Vector3 sliderPosition =
            thermometerSlider.position;


        float bestDistance =
            float.MaxValue;


        float calculatedTemperature =
            currentTemperature;


        Vector3 closestTrackPosition =
            sliderPosition;


        // ------------------------------------------------
        // Check every segment
        // ------------------------------------------------

        for (int i = 0;
             i < validPoints.Count - 1;
             i++)
        {
            Vector3 a =
                validPoints[i]
                .referencePoint.position;


            Vector3 b =
                validPoints[i + 1]
                .referencePoint.position;


            Vector3 ab =
                b - a;


            float lengthSquared =
                ab.sqrMagnitude;


            if (lengthSquared <= 0.000001f)
                continue;


            float t =
                Vector3.Dot(
                    sliderPosition - a,
                    ab
                ) / lengthSquared;


            t =
                Mathf.Clamp01(t);


            Vector3 projectedPosition =
                a + ab * t;


            float distance =
                Vector3.Distance(
                    sliderPosition,
                    projectedPosition
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;


                closestTrackPosition =
                    projectedPosition;


                calculatedTemperature =
                    Mathf.Lerp(
                        validPoints[i].temperature,
                        validPoints[i + 1].temperature,
                        t
                    );
            }
        }


        // Keep slider on thermometer path
        if (clampSliderToTrack)
        {
            thermometerSlider.position =
                closestTrackPosition;
        }


        ApplyTemperatureValue(
            calculatedTemperature
        );
    }


    // =========================================================
    // APPLY TEMPERATURE
    // =========================================================

    private void ApplyTemperatureValue(
        float temperature)
    {
        float previous =
            currentTemperature;


        currentTemperature =
            Mathf.Clamp(
                temperature,
                GetMinimumTemperature(),
                GetMaximumTemperature()
            );


        UpdateFlame();


        CheckEvents(
            previous,
            currentTemperature
        );
        //if (automaticallyTriggerTemperatureEvents)
        //{
        //    CheckEvents(
        //        previous,
        //        currentTemperature
        //    );
        //}

        onTemperatureChanged?.Invoke(
            currentTemperature
        );
    }


    // =========================================================
    // FLAME
    // =========================================================

    private void CacheFlame()
    {
        if (!flame &&
            flameTransform)
        {
            flame =
                flameTransform
                .GetComponent<ParticleSystem>();
        }


        if (flame)
        {
            var emission =
                flame.emission;


            baseEmission =
                emission.rateOverTime.constant;


            var main =
                flame.main;


            baseSpeed =
                main.startSpeed.constant;


            baseSize =
                main.startSize.constant;


            if (!flameTransform)
            {
                flameTransform =
                    flame.transform;
            }
        }


        if (flameTransform)
        {
            baseFlameScale =
                flameTransform.localScale;
        }
    }


    // =========================================================
    // UPDATE FLAME
    // =========================================================

    private void UpdateFlame()
    {
        if (!flame &&
            !flameTransform)
        {
            return;
        }


        float t =
            Mathf.InverseLerp(
                GetMinimumTemperature(),
                GetMaximumTemperature(),
                currentTemperature
            );


        float strength =
            Mathf.Lerp(
                minimumFlameStrength,
                maximumFlameStrength,
                t
            );


        if (flame)
        {
            var emission =
                flame.emission;


            emission.rateOverTime =
                baseEmission * strength;


            var main =
                flame.main;


            main.startSpeed =
                baseSpeed *
                Mathf.Lerp(
                    0.65f,
                    1.35f,
                    t
                );


            main.startSize =
                baseSize *
                Mathf.Lerp(
                    0.7f,
                    1.25f,
                    t
                );
        }


        if (flameTransform)
        {
            flameTransform.localScale =
                baseFlameScale *
                Mathf.Lerp(
                    0.65f,
                    1.35f,
                    t
                );
        }
    }


    // =========================================================
    // TEMPERATURE EVENTS
    // =========================================================

    private void CheckEvents(
        float previous,
        float now)
    {
        foreach (TemperatureEvent e
                 in temperatureEvents)
        {
            if (e == null ||
                e.onReached == null)
            {
                continue;
            }


            if (e.fireOnce &&
                e.fired)
            {
                continue;
            }


            bool crossedUp =
                previous < e.temperature &&
                now >= e.temperature;


            bool crossedDown =
                previous > e.temperature &&
                now <= e.temperature;


            bool near =
                Mathf.Abs(
                    now - e.temperature
                )
                <=
                Mathf.Max(
                    0.01f,
                    e.tolerance
                );


            if (crossedUp ||
                crossedDown ||
                near)
            {
                e.fired = true;

                e.onReached.Invoke();
            }
        }
    }


    // =========================================================
    // RESET EVENTS
    // =========================================================

    public void ResetTemperatureEvents()
    {
        foreach (TemperatureEvent e
                 in temperatureEvents)
        {
            if (e != null)
            {
                e.fired = false;
            }
        }
    }


    // =========================================================
    // SIGNAL EMITTER PRESETS
    // =========================================================

    public void SetMinus20()
    {
        MoveToTemperature(-20f);
    }

    public void Set0()
    {
        MoveToTemperature(0f);
    }

    public void Set10()
    {
        MoveToTemperature(10f);
    }

    public void Set20()
    {
        MoveToTemperature(20f);
    }

    public void Set25()
    {
        MoveToTemperature(25f);
    }

    public void Set30()
    {
        MoveToTemperature(30f);
    }

    public void Set40()
    {
        MoveToTemperature(40f);
    }

    public void Set50()
    {
        MoveToTemperature(50f);
    }

    public void Set60()
    {
        MoveToTemperature(60f);
    }

    public void Set70()
    {
        MoveToTemperature(70f);
    }

    public void Set75()
    {
        MoveToTemperature(75f);
    }

    public void Set80()
    {
        MoveToTemperature(80f);
    }

    public void Set90()
    {
        MoveToTemperature(90f);
    }

    public void Set100()
    {
        MoveToTemperature(100f);
    }

    public void Set110()
    {
        MoveToTemperature(110f);
    }

    public void Set120()
    {
        MoveToTemperature(120f);
    }


    // =========================================================
    // INCREASE / DECREASE
    // =========================================================

    public void Increase5()
    {
        MoveToTemperature(
            currentTemperature + 5f
        );
    }


    public void Decrease5()
    {
        MoveToTemperature(
            currentTemperature - 5f
        );
    }


    public void Increase10()
    {
        MoveToTemperature(
            currentTemperature + 10f
        );
    }


    public void Decrease10()
    {
        MoveToTemperature(
            currentTemperature - 10f
        );
    }


    // =========================================================
    // GIZMOS
    // =========================================================

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (temperatureReferencePoints == null)
            return;


        // Draw calibration points
        for (int i = 0;
             i < temperatureReferencePoints.Count;
             i++)
        {
            TemperatureReferencePoint point =
                temperatureReferencePoints[i];


            if (point == null ||
                point.referencePoint == null)
            {
                continue;
            }


            Gizmos.DrawWireSphere(
                point.referencePoint.position,
                0.01f
            );


            // Draw track between points
            if (i <
                temperatureReferencePoints.Count - 1)
            {
                TemperatureReferencePoint next =
                    temperatureReferencePoints[i + 1];


                if (next != null &&
                    next.referencePoint != null)
                {
                    Gizmos.DrawLine(
                        point.referencePoint.position,
                        next.referencePoint.position
                    );
                }
            }
        }
    }

#endif
}