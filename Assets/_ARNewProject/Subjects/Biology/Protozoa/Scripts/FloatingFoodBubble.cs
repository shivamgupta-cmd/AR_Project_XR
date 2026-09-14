using UnityEngine;

public class FloatingFoodBubble : MonoBehaviour
{
    [Header("Movement Distance")]
    [Tooltip("Kitna Left-Right move kare")]
    [Range(0f, 0.2f)]
    public float leftRightAmount = 0.015f;

    [Tooltip("Kitna Up-Down move kare")]
    [Range(0f, 0.2f)]
    public float upDownAmount = 0.02f;


    [Header("Movement Speed")]
    [Tooltip("Left-Right speed")]
    [Range(0.01f, 2f)]
    public float leftRightSpeed = 0.25f;

    [Tooltip("Up-Down speed")]
    [Range(0.01f, 2f)]
    public float upDownSpeed = 0.20f;


    [Header("Rotation - Optional")]
    public bool useRotation = true;

    [Range(0f, 10f)]
    public float rotationSpeed = 1f;


    [Header("Random Movement")]
    [Tooltip("Har bubble ko alag movement timing milegi")]
    public bool randomStart = true;


    private Vector3 startPosition;

    private float xOffset;
    private float yOffset;


    void Start()
    {
        // Bubble ki original position
        startPosition = transform.localPosition;

        if (randomStart)
        {
            xOffset = Random.Range(0f, 10f);
            yOffset = Random.Range(0f, 10f);
        }
    }


    void Update()
    {
        // Very small Left-Right movement
        float x =
            Mathf.Sin(Time.time * leftRightSpeed + xOffset)
            * leftRightAmount;

        // Very small Up-Down movement
        float y =
            Mathf.Sin(Time.time * upDownSpeed + yOffset)
            * upDownAmount;


        // Always stay around original position
        transform.localPosition =
            startPosition + new Vector3(x, y, 0f);


        // Very slow rotation
        if (useRotation)
        {
            transform.Rotate(
                0f,
                0f,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}