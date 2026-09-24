using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialHighlighter : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;

    [Header("HIGHLIGHT")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private bool useEmission = true;
    [SerializeField] private float emissionIntensity = 2f;

    [Header("BLINK")]
    [SerializeField] private bool useBlink = true;
    [SerializeField] private float blinkInterval = 0.3f;

    private class MaterialData
    {
        public Material material;
        public Color originalColor;
        public Color originalEmission;
        public bool hadEmission;
        public string colorProperty;
    }

    private readonly List<MaterialData> materials = new List<MaterialData>();

    private Coroutine blinkCoroutine;
    private bool isHighlighted;

    private void Awake()
    {
        CacheMaterials();
    }

    private void CacheMaterials()
    {
        materials.Clear();

        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer == null)
                continue;

            foreach (Material material in renderer.materials)
            {
                if (material == null)
                    continue;

                MaterialData data = new MaterialData();

                data.material = material;

                if (material.HasProperty("_BaseColor"))
                    data.colorProperty = "_BaseColor";
                else if (material.HasProperty("_Color"))
                    data.colorProperty = "_Color";

                if (!string.IsNullOrEmpty(data.colorProperty))
                    data.originalColor = material.GetColor(data.colorProperty);

                if (material.HasProperty("_EmissionColor"))
                    data.originalEmission = material.GetColor("_EmissionColor");

                data.hadEmission = material.IsKeywordEnabled("_EMISSION");

                materials.Add(data);
            }
        }
    }

    public void Highlight()
    {
        if (isHighlighted)
            return;

        isHighlighted = true;

        if (useBlink)
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        else
            ApplyHighlight();
    }

    public void StopHighlight()
    {
        isHighlighted = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        RestoreOriginal();
    }

    private IEnumerator BlinkRoutine()
    {
        while (isHighlighted)
        {
            ApplyHighlight();

            yield return new WaitForSeconds(blinkInterval);

            RestoreOriginal();

            yield return new WaitForSeconds(blinkInterval);
        }

        RestoreOriginal();
    }

    private void ApplyHighlight()
    {
        foreach (MaterialData data in materials)
        {
            if (data.material == null)
                continue;

            if (!string.IsNullOrEmpty(data.colorProperty))
                data.material.SetColor(
                    data.colorProperty,
                    highlightColor
                );

            if (useEmission &&
                data.material.HasProperty("_EmissionColor"))
            {
                data.material.EnableKeyword("_EMISSION");

                data.material.SetColor(
                    "_EmissionColor",
                    highlightColor * emissionIntensity
                );
            }
        }
    }

    private void RestoreOriginal()
    {
        foreach (MaterialData data in materials)
        {
            if (data.material == null)
                continue;

            if (!string.IsNullOrEmpty(data.colorProperty))
            {
                data.material.SetColor(
                    data.colorProperty,
                    data.originalColor
                );
            }

            if (data.material.HasProperty("_EmissionColor"))
            {
                data.material.SetColor(
                    "_EmissionColor",
                    data.originalEmission
                );

                if (data.hadEmission)
                    data.material.EnableKeyword("_EMISSION");
                else
                    data.material.DisableKeyword("_EMISSION");
            }
        }
    }

    public void ToggleHighlight()
    {
        if (isHighlighted)
            StopHighlight();
        else
            Highlight();
    }

    private void OnDisable()
    {
        StopHighlight();
    }
}