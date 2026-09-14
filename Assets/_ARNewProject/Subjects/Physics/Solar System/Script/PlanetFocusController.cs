using System.Collections;
using TMPro;
using UnityEngine;

public class PlanetFocusController : MonoBehaviour
{
    [Header("Target Position")]
    [SerializeField] private Transform targetPoint;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.6f;

    [Header("Target Scale")]
    [SerializeField] private Vector3 targetScale = new Vector3(10f, 10f, 10f);

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("VO Text UI")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TMP_Text voText;

    private Transform currentObject;
    private Transform originalParent;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Coroutine moveCoroutine;

    private bool isReturning = false;
    private bool isPaused = false;
    private bool isFocused = false;

    public bool IsBusy
    {
        get { return moveCoroutine != null; }
    }

    private void Start()
    {
        if (textPanel != null)
            textPanel.SetActive(false);
    }

    private void Update()
    {
        if (currentObject == null)
            return;

        if (isPaused || isReturning)
            return;

        if (isFocused)
        {
            // TargetPoint ka child hai,
            // isliye center par lock rahega
            currentObject.localPosition = Vector3.zero;

            // Continuous rotation
            currentObject.Rotate(
                rotationAxis.normalized,
                rotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }

    public void FocusPlanet(Transform planetObject, string narrationText)
    {
        if (planetObject == null)
        {
            Debug.LogWarning("Planet Object Missing!");
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("Target Point Missing!");
            return;
        }

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (currentObject != null)
        {
            ForceRestore();
        }

        currentObject = planetObject;

        // Original values save
        originalParent = currentObject.parent;
        originalLocalPosition = currentObject.localPosition;
        originalLocalRotation = currentObject.localRotation;
        originalLocalScale = currentObject.localScale;

        // Planet ko TargetPoint ka child banao
        // true = current world position preserve
        currentObject.SetParent(targetPoint, true);

        if (voText != null)
            voText.text = narrationText;

        if (textPanel != null)
            textPanel.SetActive(true);

        isFocused = false;
        isReturning = false;

        moveCoroutine = StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 startLocalPosition = currentObject.localPosition;
        Vector3 startLocalScale = currentObject.localScale;

        float time = 0f;

        while (time < moveDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            // TargetPoint center ki taraf move
            currentObject.localPosition = Vector3.Lerp(
                startLocalPosition,
                Vector3.zero,
                t
            );

            // Smooth scale
            currentObject.localScale = Vector3.Lerp(
                startLocalScale,
                targetScale,
                t
            );

            // Move hote hue bhi rotate
            currentObject.Rotate(
                rotationAxis.normalized,
                rotationSpeed * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        currentObject.localPosition = Vector3.zero;
        currentObject.localScale = targetScale;

        isFocused = true;
        moveCoroutine = null;

        Debug.Log(currentObject.name + " Reached Target Point");
    }

    public void ReturnPlanet()
    {
        if (currentObject == null)
            return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isFocused = false;
        isReturning = true;

        // Original parent me wapas daalo
        // World position abhi preserve hogi
        currentObject.SetParent(originalParent, true);

        moveCoroutine = StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        Vector3 startPosition = currentObject.localPosition;
        Quaternion startRotation = currentObject.localRotation;
        Vector3 startScale = currentObject.localScale;

        float time = 0f;

        while (time < moveDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            currentObject.localPosition = Vector3.Lerp(
                startPosition,
                originalLocalPosition,
                t
            );

            currentObject.localRotation = Quaternion.Slerp(
                startRotation,
                originalLocalRotation,
                t
            );

            currentObject.localScale = Vector3.Lerp(
                startScale,
                originalLocalScale,
                t
            );

            yield return null;
        }

        currentObject.localPosition = originalLocalPosition;
        currentObject.localRotation = originalLocalRotation;
        currentObject.localScale = originalLocalScale;

        if (textPanel != null)
            textPanel.SetActive(false);

        currentObject = null;
        originalParent = null;

        isReturning = false;
        isFocused = false;
        moveCoroutine = null;

        Debug.Log("Planet Returned To Original Position");
    }

    public void PauseActivity()
    {
        isPaused = true;
    }

    public void ResumeActivity()
    {
        isPaused = false;
    }

    private void ForceRestore()
    {
        if (currentObject == null)
            return;

        if (originalParent != null)
            currentObject.SetParent(originalParent, false);

        currentObject.localPosition = originalLocalPosition;
        currentObject.localRotation = originalLocalRotation;
        currentObject.localScale = originalLocalScale;

        currentObject = null;
        originalParent = null;

        isFocused = false;
        isReturning = false;

        if (textPanel != null)
            textPanel.SetActive(false);
    }
}