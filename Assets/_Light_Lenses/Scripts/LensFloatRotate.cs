using UnityEngine;

public class LensFloatRotate : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.08f;
    public float floatSpeed = 1f;

    [Header("Rotation")]
    public float rotationSpeed = 20f;

    private Vector3 startPos;

    public bool IsRotate = true;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        if (IsRotate)
        {
            // Rotation
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
