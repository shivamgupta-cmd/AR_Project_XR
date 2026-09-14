using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public float rotationSpeed = 30f;

    private bool isRotating = false;

    private void Update()
    {
        if (isRotating)
        {
            transform.Rotate(
                0f,
                rotationSpeed * Time.deltaTime,
                0f
            );
        }
    }

    public void StartRotation()
    {
        isRotating = true;
    }

    public void StopRotation()
    {
        isRotating = false;
    }
}