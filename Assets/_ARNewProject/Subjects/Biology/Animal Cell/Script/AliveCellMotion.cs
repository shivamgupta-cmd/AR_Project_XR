using UnityEngine;

public class AliveCellMotion : MonoBehaviour
{
    [Header("Floating Movement")]
    [SerializeField] private Vector3 moveAmount = new Vector3(0.12f, 0.08f, 0.06f);
    [SerializeField] private float moveSpeed = 0.5f;

    [Header("Gentle Rotation")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(2f, 4f, 2f);

    [Header("Breathing / Pulse")]
    [SerializeField] private bool useBreathing = true;
    [SerializeField] private float breathingAmount = 0.025f;
    [SerializeField] private float breathingSpeed = 1.2f;

    [Header("Randomize Each Cell")]
    [SerializeField] private bool randomize = true;

    private Vector3 startPosition;
    private Vector3 startScale;

    private float xOffset;
    private float yOffset;
    private float zOffset;

    private float speedMultiplier = 1f;
    private float breathingOffset;

    void Start()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;

        if (randomize)
        {
            xOffset = Random.Range(0f, 100f);
            yOffset = Random.Range(0f, 100f);
            zOffset = Random.Range(0f, 100f);

            breathingOffset = Random.Range(0f, 10f);
            speedMultiplier = Random.Range(0.75f, 1.25f);

            rotationSpeed *= Random.Range(0.7f, 1.3f);
        }
    }

    void Update()
    {
        float time = Time.time * moveSpeed * speedMultiplier;

        // Organic floating movement using Perlin Noise
        float x = (Mathf.PerlinNoise(xOffset, time) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(yOffset, time) - 0.5f) * 2f;
        float z = (Mathf.PerlinNoise(zOffset, time) - 0.5f) * 2f;

        Vector3 movement = new Vector3(
            x * moveAmount.x,
            y * moveAmount.y,
            z * moveAmount.z
        );

        transform.localPosition = startPosition + movement;

        // Gentle rotation
        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );

        // Small breathing effect
        if (useBreathing)
        {
            float pulse =
                1f +
                Mathf.Sin(
                    (Time.time + breathingOffset) *
                    breathingSpeed
                ) *
                breathingAmount;

            transform.localScale = startScale * pulse;
        }
    }
}