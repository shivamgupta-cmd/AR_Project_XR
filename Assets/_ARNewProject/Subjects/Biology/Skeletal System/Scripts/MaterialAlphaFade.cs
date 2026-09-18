using System.Collections;
using UnityEngine;

public class MaterialAlphaFade : MonoBehaviour
{
    [Header("Material")]
    [Tooltip("Material you want to fade")]
    public Material targetMaterial;

    [Header("Alpha Settings")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.5f;

    [Range(0f, 1f)]
    public float startAlpha = 0f;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Start Settings")]
    public bool setAlphaZeroOnStart = true;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("Target Material is not assigned!");
            return;
        }

        SetupTransparentMaterial();

        if (setAlphaZeroOnStart)
            SetAlpha(startAlpha);
    }

    // ==============================
    // 0 -> Target Alpha
    // ==============================
    public void FadeIn()
    {
        StartFade(targetAlpha);
    }

    // ==============================
    // Current Alpha -> 0
    // ==============================
    public void FadeOut()
    {
        StartFade(0f);
    }

    // ==============================
    // Directly set Target Alpha
    // ==============================
    public void ShowImmediately()
    {
        SetAlpha(targetAlpha);
    }

    // ==============================
    // Directly set Alpha = 0
    // ==============================
    public void HideImmediately()
    {
        SetAlpha(0f);
    }

    private void StartFade(float newAlpha)
    {
        if (targetMaterial == null)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            FadeToAlpha(newAlpha)
        );
    }

    private IEnumerator FadeToAlpha(float newAlpha)
    {
        Color color = targetMaterial.color;

        float currentAlpha = color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            color.a = Mathf.Lerp(
                currentAlpha,
                newAlpha,
                t
            );

            targetMaterial.color = color;

            yield return null;
        }

        color.a = newAlpha;
        targetMaterial.color = color;

        fadeCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (targetMaterial == null)
            return;

        Color color = targetMaterial.color;
        color.a = alpha;

        targetMaterial.color = color;
    }

    // ==============================
    // URP Lit -> Transparent
    // ==============================
    private void SetupTransparentMaterial()
    {
        if (!targetMaterial.HasProperty("_Surface"))
            return;

        targetMaterial.SetFloat("_Surface", 1f);

        targetMaterial.SetOverrideTag(
            "RenderType",
            "Transparent"
        );

        targetMaterial.SetInt(
            "_SrcBlend",
            (int)UnityEngine.Rendering.BlendMode.SrcAlpha
        );

        targetMaterial.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        targetMaterial.SetInt("_ZWrite", 0);

        targetMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        targetMaterial.renderQueue = 3000;
    }
}