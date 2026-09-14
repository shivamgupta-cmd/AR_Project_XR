using UnityEngine;
using System.Collections;

public class ObjectHighlighter : MonoBehaviour
{
    private Material mat;
    private Color originalEmissionColor;
    private Coroutine blinkRoutine;

    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float maxIntensity = 1.2f;
    public float blinkSpeed = 1f;

    void Awake()
    {
        mat = GetComponent<Renderer>().material;

        if (mat.HasProperty("_EmissionColor"))
        {
            originalEmissionColor = mat.GetColor("_EmissionColor");
        }
    }

    public void StartHighlight()
    {
        mat.EnableKeyword("_EMISSION");

        if (blinkRoutine == null)
        {
            blinkRoutine = StartCoroutine(BlinkRoutine());
        }
    }

    public void StopHighlight()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Reset emission
        mat.SetColor("_EmissionColor", originalEmissionColor);
    }

    IEnumerator BlinkRoutine()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * blinkSpeed;

            // Smooth ping-pong (0 → 1 → 0)
            float intensity = Mathf.PingPong(time, 1f);

            Color finalColor = highlightColor * Mathf.Lerp(0f, maxIntensity, intensity);
            mat.SetColor("_EmissionColor", finalColor);

            yield return null;
        }
    }
}