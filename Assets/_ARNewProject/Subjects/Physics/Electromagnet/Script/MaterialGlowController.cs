using UnityEngine;

public class MaterialGlowController : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Glow")]
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float glowIntensity = 5f;
    [SerializeField] private float alpha = 1f;

    [Header("Blink")]
    [SerializeField] private bool blink = true;
    [SerializeField] private float blinkInterval = 0.3f;

    [Header("Pulse")]
    [SerializeField] private bool pulse = false;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 2f;

    private Material material;
    private bool glowEnabled;
    private bool blinkState = true;
    private float blinkTimer;

    private static readonly int GlowColorID = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowIntensityID = Shader.PropertyToID("_GlowIntensity");
    private static readonly int AlphaID = Shader.PropertyToID("_Alpha");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            enabled = false;
            return;
        }

        material = targetRenderer.material;
    }

    private void Start()
    {
        GlowOff();
    }

    private void Update()
    {
        if (!glowEnabled || material == null)
            return;

        if (blink)
        {
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                blinkState = !blinkState;

                if (blinkState)
                    ApplyGlow(glowIntensity);
                else
                    ApplyGlowOff();
            }

            return;
        }

        if (pulse)
        {
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float intensity = glowIntensity + wave * pulseAmount;

            ApplyGlow(intensity);
        }
        else
        {
            ApplyGlow(glowIntensity);
        }
    }

    private void ApplyGlow(float intensity)
    {
        if (material.HasProperty(GlowColorID))
            material.SetColor(GlowColorID, glowColor);

        if (material.HasProperty(GlowIntensityID))
            material.SetFloat(GlowIntensityID, intensity);

        if (material.HasProperty(AlphaID))
            material.SetFloat(AlphaID, alpha);

        if (material.HasProperty(EmissionColorID))
        {
            material.SetColor(EmissionColorID, glowColor * intensity);
            material.EnableKeyword("_EMISSION");
        }
    }

    private void ApplyGlowOff()
    {
        if (material.HasProperty(GlowIntensityID))
            material.SetFloat(GlowIntensityID, 0f);

        if (material.HasProperty(AlphaID))
            material.SetFloat(AlphaID, 0f);

        if (material.HasProperty(EmissionColorID))
            material.SetColor(EmissionColorID, Color.black);
    }

    public void GlowOn()
    {
        if (material == null)
            return;

        glowEnabled = true;
        blinkState = true;
        blinkTimer = 0f;

        ApplyGlow(glowIntensity);
    }

    public void GlowOff()
    {
        if (material == null)
            return;

        glowEnabled = false;
        blinkTimer = 0f;

        ApplyGlowOff();
    }

    public void ToggleGlow()
    {
        if (glowEnabled)
            GlowOff();
        else
            GlowOn();
    }

    public void SetGlow(bool value)
    {
        if (value)
            GlowOn();
        else
            GlowOff();
    }
}