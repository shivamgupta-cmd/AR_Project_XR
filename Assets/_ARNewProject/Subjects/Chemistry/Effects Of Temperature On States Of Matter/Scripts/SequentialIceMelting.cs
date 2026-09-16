using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialIceMelting : MonoBehaviour
{
    [Header("ICE CUBES - MELT ORDER")]
    [Tooltip("Add ice cubes here in the exact order you want them to melt.")]
    public List<Transform> iceCubes = new List<Transform>();

    [Header("ICE MELTING SETTINGS")]
    [Tooltip("Time taken for ONE ice cube to scale down completely.")]
    public float cubeMeltDuration = 1.5f;

    [Tooltip("Delay after one cube melts before the next cube starts melting.")]
    public float intervalBetweenCubes = 0.5f;

    [Header("LIQUID")]
    [Tooltip("Assign the liquid/water object.")]
    public Transform liquidObject;

    [Tooltip("Final LOCAL scale of the liquid after ALL ice cubes have melted.")]
    public Vector3 finalLiquidScale = new Vector3(1f, 1f, 1f);

    [Tooltip("Should liquid increase while each cube is shrinking?")]
    public bool smoothLiquidIncrease = true;

    [Header("OPTIONAL - LIQUID POSITION")]
    [Tooltip("Enable this if the water must also move upward while its scale increases.")]
    public bool animateLiquidPosition = false;

    [Tooltip("Final LOCAL position of liquid when all cubes have melted.")]
    public Vector3 finalLiquidPosition;

    [Header("PLAY SETTINGS")]
    public bool playOnStart = false;

    private Vector3[] originalCubeScales;

    private Vector3 initialLiquidScale;
    private Vector3 initialLiquidPosition;

    private Coroutine meltingRoutine;
    private bool initialized = false;

    private void Start()
    {
        Initialize();

        if (playOnStart)
        {
            StartMelting();
        }
    }

    // --------------------------------------------------
    // INITIALIZE
    // --------------------------------------------------

    private void Initialize()
    {
        if (initialized)
            return;

        originalCubeScales = new Vector3[iceCubes.Count];

        for (int i = 0; i < iceCubes.Count; i++)
        {
            if (iceCubes[i] != null)
            {
                originalCubeScales[i] = iceCubes[i].localScale;
            }
        }

        if (liquidObject != null)
        {
            initialLiquidScale = liquidObject.localScale;
            initialLiquidPosition = liquidObject.localPosition;
        }

        initialized = true;
    }

    // --------------------------------------------------
    // CALL THIS TO START MELTING
    // --------------------------------------------------

    public void StartMelting()
    {
        Initialize();

        if (meltingRoutine != null)
        {
            StopCoroutine(meltingRoutine);
        }

        meltingRoutine = StartCoroutine(MeltingSequence());
    }

    // --------------------------------------------------
    // COMPLETE MELTING SEQUENCE
    // --------------------------------------------------

    private IEnumerator MeltingSequence()
    {
        if (iceCubes.Count == 0)
        {
            Debug.LogWarning("No ice cubes assigned.");
            yield break;
        }

        for (int i = 0; i < iceCubes.Count; i++)
        {
            Transform currentCube = iceCubes[i];

            if (currentCube == null)
                continue;

            yield return StartCoroutine(
                MeltSingleCube(currentCube, i)
            );

            // Wait before next cube melts
            if (intervalBetweenCubes > 0f)
            {
                yield return new WaitForSeconds(intervalBetweenCubes);
            }
        }

        // Make sure final liquid values are exact
        if (liquidObject != null)
        {
            liquidObject.localScale = finalLiquidScale;

            if (animateLiquidPosition)
            {
                liquidObject.localPosition = finalLiquidPosition;
            }
        }

        meltingRoutine = null;

        Debug.Log("Ice melting completed.");
    }

    // --------------------------------------------------
    // MELT ONE ICE CUBE
    // --------------------------------------------------

    private IEnumerator MeltSingleCube(
        Transform cube,
        int cubeIndex)
    {
        Vector3 cubeStartScale = cube.localScale;

        // Calculate how much water should exist
        // after THIS cube has completely melted.

        float previousLiquidProgress =
            (float)cubeIndex / iceCubes.Count;

        float targetLiquidProgress =
            (float)(cubeIndex + 1) / iceCubes.Count;

        Vector3 liquidStartScale = Vector3.Lerp(
            initialLiquidScale,
            finalLiquidScale,
            previousLiquidProgress
        );

        Vector3 liquidTargetScale = Vector3.Lerp(
            initialLiquidScale,
            finalLiquidScale,
            targetLiquidProgress
        );

        Vector3 liquidStartPosition = initialLiquidPosition;
        Vector3 liquidTargetPosition = initialLiquidPosition;

        if (animateLiquidPosition)
        {
            liquidStartPosition = Vector3.Lerp(
                initialLiquidPosition,
                finalLiquidPosition,
                previousLiquidProgress
            );

            liquidTargetPosition = Vector3.Lerp(
                initialLiquidPosition,
                finalLiquidPosition,
                targetLiquidProgress
            );
        }

        float timer = 0f;

        while (timer < cubeMeltDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / cubeMeltDuration
            );

            // Smooth animation instead of linear movement
            float smoothT = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            // -----------------------------
            // ICE SHRINKING
            // -----------------------------

            cube.localScale = Vector3.Lerp(
                cubeStartScale,
                Vector3.zero,
                smoothT
            );

            // -----------------------------
            // LIQUID INCREASING
            // -----------------------------

            if (liquidObject != null &&
                smoothLiquidIncrease)
            {
                liquidObject.localScale =
                    Vector3.Lerp(
                        liquidStartScale,
                        liquidTargetScale,
                        smoothT
                    );

                if (animateLiquidPosition)
                {
                    liquidObject.localPosition =
                        Vector3.Lerp(
                            liquidStartPosition,
                            liquidTargetPosition,
                            smoothT
                        );
                }
            }

            yield return null;
        }

        // Ensure cube is exactly zero
        cube.localScale = Vector3.zero;

        // If smooth liquid animation is disabled,
        // increase liquid when cube finishes.
        if (liquidObject != null)
        {
            liquidObject.localScale =
                liquidTargetScale;

            if (animateLiquidPosition)
            {
                liquidObject.localPosition =
                    liquidTargetPosition;
            }
        }
    }

    // --------------------------------------------------
    // RESET
    // --------------------------------------------------

    public void ResetMelting()
    {
        Initialize();

        if (meltingRoutine != null)
        {
            StopCoroutine(meltingRoutine);
            meltingRoutine = null;
        }

        // Restore all ice cubes
        for (int i = 0; i < iceCubes.Count; i++)
        {
            if (iceCubes[i] != null)
            {
                iceCubes[i].localScale =
                    originalCubeScales[i];

                iceCubes[i].gameObject.SetActive(true);
            }
        }

        // Restore water
        if (liquidObject != null)
        {
            liquidObject.localScale =
                initialLiquidScale;

            liquidObject.localPosition =
                initialLiquidPosition;
        }
    }

    // --------------------------------------------------
    // INSTANTLY COMPLETE MELTING
    // --------------------------------------------------

    public void CompleteMeltingImmediately()
    {
        Initialize();

        if (meltingRoutine != null)
        {
            StopCoroutine(meltingRoutine);
            meltingRoutine = null;
        }

        foreach (Transform cube in iceCubes)
        {
            if (cube != null)
            {
                cube.localScale = Vector3.zero;
            }
        }

        if (liquidObject != null)
        {
            liquidObject.localScale =
                finalLiquidScale;

            if (animateLiquidPosition)
            {
                liquidObject.localPosition =
                    finalLiquidPosition;
            }
        }
    }
}