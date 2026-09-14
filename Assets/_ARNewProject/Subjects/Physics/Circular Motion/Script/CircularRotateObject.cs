using UnityEngine;

public class CircularRotateObject : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 38f;

    public bool IsRotating { get; private set; } = true;

    private Quaternion startingRotation;

    void Start()
    {
        // Starting rotation save
        startingRotation = transform.rotation;
    }

    void Update()
    {
        if (IsRotating)
        {
            //Debug.Log("rotationSpeed " + rotationSpeed);
            transform.Rotate(0f,-rotationSpeed * Time.deltaTime,0f);
        }
    }

    // =========================================================
    // START ROTATION
    // =========================================================

    public void StartRotation()
    {
        IsRotating = true;
    }

    // =========================================================
    // STOP ROTATION
    // =========================================================

    public void StopRotation()
    {
        IsRotating = false;
    }

    // =========================================================
    // SET SPEED
    // =========================================================

    public void SetSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    // =========================================================
    // RESET ROTATION
    // =========================================================

    public void ResetRotation()
    {
        transform.rotation = startingRotation;
    }
}