using UnityEngine;

public class Rotate : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Settings")]
    public RotationAxis axis = RotationAxis.Y;

    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 50f;

    [Header("Control")]
    public bool rotateOnStart = true;

    private bool isRotating;

    private void Start()
    {
        isRotating = rotateOnStart;
    }

    private void Update()
    {
        if (!isRotating)
            return;

        Vector3 rotateAxis = Vector3.zero;

        switch (axis)
        {
            case RotationAxis.X:
                rotateAxis = Vector3.right;
                break;

            case RotationAxis.Y:
                rotateAxis = Vector3.up;
                break;

            case RotationAxis.Z:
                rotateAxis = Vector3.forward;
                break;
        }

        transform.Rotate(rotateAxis * rotationSpeed * Time.deltaTime);
    }

    // Call this from Timeline Signal
    public void StartRotation()
    {
        isRotating = true;
    }

    // Call this from Timeline Signal
    public void StopRotation()
    {
        isRotating = false;
    }

    // Toggle rotation
    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }

    // Check current state
    public bool IsRotating()
    {
        return isRotating;
    }
}