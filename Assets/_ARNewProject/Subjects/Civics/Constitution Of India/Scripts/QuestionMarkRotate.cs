using System.Collections;
using UnityEngine;

public class QuestionMarkRotate : MonoBehaviour
{
    [Header("Scale Animation")]
    [SerializeField] private float scaleDuration = 0.6f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 25f;

    [Tooltip("Choose rotation axis. Example: Y = (0,1,0)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Rotation Space")]
    [SerializeField] private Space rotationSpace = Space.Self;

    private bool isRotating = false;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        // Initially hidden
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (!isRotating)
            return;

        transform.Rotate(
            rotationAxis.normalized,
            rotationSpeed * Time.deltaTime,
            rotationSpace
        );
    }

    // Call this function from Timeline / UnityEvent / another script
    public void ShowQuestionMark()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleIn());
    }

    private IEnumerator ScaleIn()
    {
        isRotating = false;

        transform.localScale = Vector3.zero;

        float time = 0f;

        while (time < scaleDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / scaleDuration);

            // Smooth animation
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, smoothT);

            yield return null;
        }

        transform.localScale = Vector3.one;

        // Start rotation only AFTER scale animation finishes
        isRotating = true;

        scaleCoroutine = null;
    }

    // Optional function to hide/reset it
    public void HideQuestionMark()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        isRotating = false;
        transform.localScale = Vector3.zero;
    }
}