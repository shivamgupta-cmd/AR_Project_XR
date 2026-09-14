using UnityEngine;
using System.Collections;

public class ObjectFadeTransparent : MonoBehaviour
{
    public Renderer targetRenderer;

    public float fadeDuration = 1f;

    private Material mat;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // Instance material banega, original material affect nahi hoga
        mat = targetRenderer.material;
    }

    // Is function ko Button / Event / dusri script se call karo
    public void StartFade()
    {
        StopAllCoroutines();
        StartCoroutine(FadeObject());
    }

    IEnumerator FadeObject()
    {
        // URP Lit Material ko Transparent karo
        mat.SetFloat("_Surface", 1);

        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color color = mat.color;

        float startAlpha = color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = time / fadeDuration;

            color.a = Mathf.Lerp(startAlpha, 0f, t);

            mat.color = color;

            yield return null;
        }

        color.a = 0f;
        mat.color = color;
    }
}