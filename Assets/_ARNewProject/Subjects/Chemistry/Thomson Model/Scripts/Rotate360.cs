using UnityEngine;

public class Rotate360 : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Settings")]
    public RotationAxis axis = RotationAxis.Y;

    [Tooltip("Degrees per second")]
    public float rotationSpeed = 100f;

    [Header("Control")]
    public bool rotateOnStart = true;

    private bool isRotating = true;

    void Start()
    {
        isRotating = rotateOnStart;
    }

    void Update()
    {
        if (!isRotating)
            return;

        Vector3 rotation = Vector3.zero;

        switch (axis)
        {
            case RotationAxis.X:
                rotation = new Vector3(rotationSpeed * Time.deltaTime, 0f, 0f);
                break;

            case RotationAxis.Y:
                rotation = new Vector3(0f, rotationSpeed * Time.deltaTime, 0f);
                break;

            case RotationAxis.Z:
                rotation = new Vector3(0f, 0f, rotationSpeed * Time.deltaTime);
                break;
        }

        transform.localEulerAngles += rotation;
    }

    // Timeline Signal
    public void StartRotation()
    {
        isRotating = true;
    }

    // Timeline Signal
    public void StopRotation()
    {
        isRotating = false;
    }

    // Timeline Signal
    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }
}