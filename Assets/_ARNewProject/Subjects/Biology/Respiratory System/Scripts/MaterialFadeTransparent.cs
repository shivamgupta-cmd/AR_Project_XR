using UnityEngine;
using System.Collections;

public class MaterialFadeTransparent : MonoBehaviour
{
    [Header("Lung Materials")]
    public Material material1;
    public Material material2;

    [Header("Transparency")]
    [Tooltip("1 = Fully Opaque, 0 = Fully Transparent")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.3f;

    [Header("Fade Settings")]
    [Range(0.1f, 5f)]
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    // Timeline / Button / Event se call karo
    public void FadeTransparent()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    // Ending me lungs ko wapas opaque karne ke liye
    public void FadeOpaque()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(1f));
    }

    IEnumerator FadeTo(float target)
    {
        SetupTransparentMaterial(material1);
        SetupTransparentMaterial(material2);

        float startAlpha1 = GetAlpha(material1);
        float startAlpha2 = GetAlpha(material2);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth transition
            t = Mathf.SmoothStep(0f, 1f, t);

            SetAlpha(
                material1,
                Mathf.Lerp(startAlpha1, target, t)
            );

            SetAlpha(
                material2,
                Mathf.Lerp(startAlpha2, target, t)
            );

            yield return null;
        }

        SetAlpha(material1, target);
        SetAlpha(material2, target);

        fadeCoroutine = null;
    }

    void SetupTransparentMaterial(Material mat)
    {
        if (mat == null)
            return;

        // URP Lit -> Transparent
        mat.SetFloat("_Surface", 1f);

        mat.SetOverrideTag("RenderType", "Transparent");

        mat.SetInt(
            "_SrcBlend",
            (int)UnityEngine.Rendering.BlendMode.SrcAlpha
        );

        mat.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        mat.SetInt("_ZWrite", 0);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        mat.renderQueue =
            (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    float GetAlpha(Material mat)
    {
        if (mat == null)
            return 1f;

        return mat.color.a;
    }

    void SetAlpha(Material mat, float alpha)
    {
        if (mat == null)
            return;

        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
}