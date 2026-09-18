using UnityEngine;

public class VirusRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 25f;

    [Tooltip("Choose rotation axis. Example: Y = (0,1,0)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("Rotation Space")]
    public Space rotationSpace = Space.Self;

    void Update()
    {
        transform.Rotate(
            rotationAxis.normalized,
            rotationSpeed * Time.deltaTime,
            rotationSpace
        );
    }
}