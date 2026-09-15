using UnityEngine;

public class MultiObjectWobbleRotate : MonoBehaviour
{
    [Header("OBJECTS")]
    [Tooltip("Assign any number of objects here.")]
    public Transform[] objects;


    // =====================================================
    // WOBBLE / FLOAT
    // =====================================================

    [Header("WOBBLE / FLOAT")]
    [Tooltip("Enable or disable up/down wobble.")]
    public bool enableWobble = true;

    [Tooltip("How far the objects move up and down.")]
    public float wobbleAmount = 0.05f;

    [Tooltip("Speed of the up/down movement.")]
    public float wobbleSpeed = 2f;

    [Tooltip("If enabled, objects will not all move exactly together.")]
    public bool randomizeWobble = true;


    // =====================================================
    // ROTATION
    // =====================================================

    [Header("ROTATION")]
    [Tooltip("Enable or disable rotation.")]
    public bool enableRotation = true;

    [Tooltip("Rotation axis. Example: (0,1,0) = Y axis.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 30f;

    [Tooltip("Give each object slightly different rotation speed.")]
    public bool randomizeRotationSpeed = false;

    [Range(0f, 1f)]
    public float rotationRandomness = 0.25f;


    // =====================================================
    // START OPTIONS
    // =====================================================

    [Header("START OPTIONS")]
    [Tooltip("Automatically start enabled effects when scene starts.")]
    public bool playOnStart = true;


    // =====================================================
    // RUNTIME
    // =====================================================

    private Vector3[] startPositions;
    private float[] wobbleOffsets;
    private float[] rotationMultipliers;

    private bool isPlaying;


    private void Start()
    {
        SetupObjects();

        isPlaying = playOnStart;
    }


    private void Update()
    {
        if (!isPlaying || objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            // -------------------------------
            // WOBBLE
            // -------------------------------

            if (enableWobble)
            {
                float offset = randomizeWobble
                    ? wobbleOffsets[i]
                    : 0f;

                float y =
                    Mathf.Sin(
                        (Time.time * wobbleSpeed) + offset
                    ) * wobbleAmount;

                Vector3 newPosition = startPositions[i];

                newPosition.y += y;

                objects[i].localPosition = newPosition;
            }


            // -------------------------------
            // ROTATION
            // -------------------------------

            if (enableRotation)
            {
                float speed =
                    rotationSpeed *
                    rotationMultipliers[i];

                objects[i].Rotate(
                    rotationAxis.normalized,
                    speed * Time.deltaTime,
                    Space.Self
                );
            }
        }
    }


    // =====================================================
    // SETUP
    // =====================================================

    private void SetupObjects()
    {
        if (objects == null)
            return;

        startPositions = new Vector3[objects.Length];
        wobbleOffsets = new float[objects.Length];
        rotationMultipliers = new float[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            startPositions[i] = objects[i].localPosition;

            wobbleOffsets[i] =
                Random.Range(0f, Mathf.PI * 2f);

            if (randomizeRotationSpeed)
            {
                rotationMultipliers[i] =
                    Random.Range(
                        1f - rotationRandomness,
                        1f + rotationRandomness
                    );
            }
            else
            {
                rotationMultipliers[i] = 1f;
            }
        }
    }


    // =====================================================
    // PUBLIC FUNCTIONS
    // Timeline / Button / Unity Event
    // =====================================================

    public void StartEffects()
    {
        isPlaying = true;
    }

    public void StopEffects()
    {
        isPlaying = false;
    }


    // =====================================================
    // WOBBLE CONTROL
    // =====================================================

    public void WobbleOn()
    {
        enableWobble = true;
    }

    public void WobbleOff()
    {
        enableWobble = false;

        ResetPositions();
    }


    // =====================================================
    // ROTATION CONTROL
    // =====================================================

    public void RotationOn()
    {
        enableRotation = true;
    }

    public void RotationOff()
    {
        enableRotation = false;
    }


    // =====================================================
    // BOTH
    // =====================================================

    public void EnableBoth()
    {
        enableWobble = true;
        enableRotation = true;
        isPlaying = true;
    }

    public void DisableBoth()
    {
        enableWobble = false;
        enableRotation = false;

        ResetPositions();
    }


    // =====================================================
    // RESET
    // =====================================================

    public void ResetPositions()
    {
        if (objects == null || startPositions == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].localPosition =
                    startPositions[i];
            }
        }
    }
}