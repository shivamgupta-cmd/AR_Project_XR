using System.Collections;
using UnityEngine;

public class ElectronEnergyJump : MonoBehaviour
{
    [Header("Jump Target")]
    [Tooltip("Place this point exactly on the higher orbit where the electron should land.")]
    public Transform higherOrbitPoint;

    [Header("Jump Settings")]
    public float jumpDuration = 1f;
    public float curveHeight = 0.2f;

    [Header("Glow")]
    public Renderer electronRenderer;

    [ColorUsage(true, true)]
    public Color glowColor = new Color(0f, 1.5f, 3f, 1f);

    public float glowDuration = 0.2f;

    [Header("Trail")]
    public TrailRenderer trail;

    [Header("Arrival Effect")]
    public ParticleSystem arrivalParticles;

    [Header("Higher Orbit")]
    [Tooltip("Optional. Assign only if your orbit setup uses parenting.")]
    public Transform higherOrbitParent;

    private bool isJumping = false;
    private Material electronMaterial;
    private Color originalEmissionColor = Color.black;

    void Start()
    {
        if (electronRenderer != null)
        {
            electronMaterial = electronRenderer.material;

            if (electronMaterial.HasProperty("_EmissionColor"))
                originalEmissionColor = electronMaterial.GetColor("_EmissionColor");
        }

        if (trail != null)
            trail.emitting = false;
    }

    // Connect this to your existing:
    // IncomingEnergyWave -> On Energy Absorbed
    public void AbsorbEnergyAndJump()
    {
        if (!isJumping)
            StartCoroutine(JumpSequence());
    }

    IEnumerator JumpSequence()
    {
        if (higherOrbitPoint == null)
        {
            Debug.LogWarning("ElectronEnergyJump: Assign Higher Orbit Point.", this);
            yield break;
        }

        isJumping = true;

        // 1. Glow when energy hits.
        yield return StartCoroutine(GlowElectron());

        // 2. Start trail.
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = higherOrbitPoint.position;

        float timer = 0f;

        // 3. Curved jump from CURRENT position.
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / jumpDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 currentPosition =
                Vector3.Lerp(startPosition, targetPosition, smoothT);

            float curve = Mathf.Sin(t * Mathf.PI) * curveHeight;
            currentPosition += Vector3.up * curve;

            transform.position = currentPosition;

            yield return null;
        }

        transform.position = targetPosition;

        // 4. Stop trail.
        if (trail != null)
            trail.emitting = false;

        // 5. Arrival burst.
        if (arrivalParticles != null)
        {
            arrivalParticles.transform.position = transform.position;
            arrivalParticles.Play();
        }

        // 6. Optional parent to higher orbit.
        // This script does NOT control your orbit script.
        if (higherOrbitParent != null)
            transform.SetParent(higherOrbitParent, true);

        isJumping = false;
    }

    IEnumerator GlowElectron()
    {
        if (electronMaterial == null ||
            !electronMaterial.HasProperty("_EmissionColor"))
        {
            yield return new WaitForSeconds(glowDuration);
            yield break;
        }

        electronMaterial.EnableKeyword("_EMISSION");

        float halfTime = Mathf.Max(0.01f, glowDuration * 0.5f);
        float timer = 0f;

        while (timer < halfTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfTime);

            electronMaterial.SetColor(
                "_EmissionColor",
                Color.Lerp(originalEmissionColor, glowColor, t)
            );

            yield return null;
        }

        timer = 0f;

        while (timer < halfTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfTime);

            electronMaterial.SetColor(
                "_EmissionColor",
                Color.Lerp(glowColor, originalEmissionColor, t)
            );

            yield return null;
        }

        electronMaterial.SetColor("_EmissionColor", originalEmissionColor);
    }
}
