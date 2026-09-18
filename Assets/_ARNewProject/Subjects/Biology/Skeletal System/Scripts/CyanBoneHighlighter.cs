using UnityEngine;

public class CyanBoneHighlighter : MonoBehaviour
{
    [Header("Target")]
    public Renderer targetRenderer;

    [Header("Highlight Settings")]
    [ColorUsage(true, true)]
    public Color highlightColor = new Color(0f, 0.8f, 1f, 1f);

    [Range(0f, 10f)]
    public float emissionIntensity = 2f;

    private Material[] materials;
    private Color[] originalBaseColors;
    private Color[] originalEmissionColors;

    private bool initialized = false;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning(
                "CyanBoneHighlighter: Renderer not found on " + gameObject.name
            );
            return;
        }

        // Creates separate material instances
        // so other bones using same material are not affected.
        materials = targetRenderer.materials;

        originalBaseColors = new Color[materials.Length];
        originalEmissionColors = new Color[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];

            // Save original base color
            if (mat.HasProperty("_BaseColor"))
            {
                originalBaseColors[i] =
                    mat.GetColor("_BaseColor");
            }
            else if (mat.HasProperty("_Color"))
            {
                originalBaseColors[i] =
                    mat.GetColor("_Color");
            }

            // Save original emission
            if (mat.HasProperty("_EmissionColor"))
            {
                originalEmissionColors[i] =
                    mat.GetColor("_EmissionColor");
            }
        }

        initialized = true;
    }

    // ==========================================
    // SCRIPT ENABLED = HIGHLIGHT ON
    // ==========================================

    private void OnEnable()
    {
        Initialize();

        if (!initialized)
            return;

        SetHighlight();
    }

    // ==========================================
    // SCRIPT DISABLED = HIGHLIGHT OFF
    // ==========================================

    private void OnDisable()
    {
        if (!initialized)
            return;

        RestoreOriginal();
    }

    private void SetHighlight()
    {
        Color emission =
            highlightColor * emissionIntensity;

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];

            // Base Color
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    highlightColor
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    highlightColor
                );
            }

            // Emission
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");

                mat.SetColor(
                    "_EmissionColor",
                    emission
                );
            }
        }
    }

    private void RestoreOriginal()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];

            // Restore Base Color
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    originalBaseColors[i]
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    originalBaseColors[i]
                );
            }

            // Restore Emission
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor(
                    "_EmissionColor",
                    originalEmissionColors[i]
                );
            }
        }
    }
}