using UnityEngine;
using System.Collections;

public class AirMaterialColorChanger : MonoBehaviour
{
    [Header("Shared Air Material")]
    [Tooltip("Wahi material drag karo jo sabhi 25 spheres par laga hai")]
    public Material airMaterial;

    [Header("CO2 Color")]
    public Color co2Color = new Color(1f, 0.25f, 0.08f, 1f);

    [Header("Transition")]
    [Range(0.1f, 5f)]
    public float transitionTime = 0.7f;

    private Color originalColor;
    private Coroutine colorCoroutine;

    void Start()
    {
        if (airMaterial != null)
        {
            // Existing blue color save kar lega
            originalColor = airMaterial.color;
        }
    }

    // Blue -> Orange/Red
    public void ChangeToCO2()
    {
        if (airMaterial == null)
            return;

        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);

        colorCoroutine = StartCoroutine(
            ChangeColorSmoothly(airMaterial.color, co2Color)
        );
    }

    // Orange/Red -> Original Blue
    public void ChangeBackToBlue()
    {
        if (airMaterial == null)
            return;

        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);

        colorCoroutine = StartCoroutine(
            ChangeColorSmoothly(airMaterial.color, originalColor)
        );
    }

    IEnumerator ChangeColorSmoothly(Color startColor, Color targetColor)
    {
        float timer = 0f;

        while (timer < transitionTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / transitionTime);

            airMaterial.color = Color.Lerp(
                startColor,
                targetColor,
                t
            );

            yield return null;
        }

        airMaterial.color = targetColor;
        colorCoroutine = null;
    }
}