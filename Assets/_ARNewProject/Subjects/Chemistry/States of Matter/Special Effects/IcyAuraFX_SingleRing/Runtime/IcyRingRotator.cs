using UnityEngine;

public class IcyRingRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 22f;
    [SerializeField] private Vector3 localAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(localAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
