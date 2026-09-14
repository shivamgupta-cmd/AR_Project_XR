using UnityEngine;

public class CellWallTabletShake : MonoBehaviour
{
    [Header("Cell Wall")]
    public Transform cellWall;

    [Header("Shake Detection")]
    public float shakeThreshold = 1.5f;

    [Header("Shake Amount")]
    public float positionShakeAmount = 0.015f;
    public float rotationShakeAmount = 2f;

    [Header("Return Speed")]
    public float returnSpeed = 8f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 lastAcceleration;

    void Start()
    {
        if (cellWall != null)
        {
            originalPosition = cellWall.localPosition;
            originalRotation = cellWall.localRotation;
        }

        lastAcceleration = Input.acceleration;
    }

    void Update()
    {
        if (cellWall == null)
            return;

        Vector3 currentAcceleration = Input.acceleration;

        // Gravity ko mostly ignore karne ke liye
        // previous frame se acceleration difference check kar rahe hain
        Vector3 accelerationDifference =
            currentAcceleration - lastAcceleration;

        float shakeStrength = accelerationDifference.magnitude;

        if (shakeStrength > shakeThreshold)
        {
            ShakeCellWall(shakeStrength);
        }
        else
        {
            ReturnToOriginal();
        }

        lastAcceleration = currentAcceleration;
    }

    void ShakeCellWall(float strength)
    {
        // Shake ko limit rakho
        strength = Mathf.Clamp(strength, 0f, 3f);

        Vector3 randomPosition =
            Random.insideUnitSphere *
            positionShakeAmount *
            strength;

        cellWall.localPosition =
            originalPosition + randomPosition;

        float xRotation =
            Random.Range(-rotationShakeAmount, rotationShakeAmount)
            * strength;

        float yRotation =
            Random.Range(-rotationShakeAmount, rotationShakeAmount)
            * strength;

        float zRotation =
            Random.Range(-rotationShakeAmount, rotationShakeAmount)
            * strength;

        cellWall.localRotation =
            originalRotation *
            Quaternion.Euler(
                xRotation,
                yRotation,
                zRotation
            );
    }

    void ReturnToOriginal()
    {
        cellWall.localPosition =
            Vector3.Lerp(
                cellWall.localPosition,
                originalPosition,
                Time.deltaTime * returnSpeed
            );

        cellWall.localRotation =
            Quaternion.Slerp(
                cellWall.localRotation,
                originalRotation,
                Time.deltaTime * returnSpeed
            );
    }
}