using System.Collections;
using UnityEngine;

public class FlameStrengthController : MonoBehaviour
{
    [Header("FLAME REFERENCES")]
    [Tooltip("Assign your flame Particle System.")]
    public ParticleSystem flame;

    [Tooltip("Optional. Assign flame root if you also want its scale to change.")]
    public Transform flameTransform;


    [Header("STRENGTH")]
    [Range(0f, 1f)]
    public float currentStrength = 0f;

    [Tooltip("How long the flame takes to reach the requested strength.")]
    public float transitionDuration = 1f;

    public AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


    [Header("EMISSION")]
    [Tooltip("Emission rate when strength = 0.")]
    public float minimumEmission = 0f;

    [Tooltip("Emission rate when strength = 1.")]
    public float maximumEmission = 40f;


    [Header("PARTICLE SPEED")]
    public float minimumSpeed = 0.2f;
    public float maximumSpeed = 1.5f;


    [Header("PARTICLE SIZE")]
    public float minimumParticleSize = 0.1f;
    public float maximumParticleSize = 0.5f;


    [Header("FLAME SCALE")]
    public bool controlFlameScale = true;

    public Vector3 minimumScale =
        new Vector3(0.5f, 0.5f, 0.5f);

    public Vector3 maximumScale =
        new Vector3(1.2f, 1.8f, 1.2f);


    [Header("PLAY SETTINGS")]
    public bool playOnStart = false;

    private Coroutine strengthRoutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (flame == null)
            flame = GetComponent<ParticleSystem>();

        if (flame != null && flameTransform == null)
            flameTransform = flame.transform;

        if (playOnStart)
        {
            SetStrengthImmediate(currentStrength);
        }
        else
        {
            SetStrengthImmediate(0f);
        }
    }


    // =========================================================
    // MAIN FUNCTION
    // =========================================================

    /// <summary>
    /// Strength range:
    /// 0 = OFF
    /// 1 = Maximum
    ///
    /// Can be called from UnityEvent.
    /// </summary>
    public void SetStrength(float strength)
    {
        strength = Mathf.Clamp01(strength);

        if (strengthRoutine != null)
            StopCoroutine(strengthRoutine);

        strengthRoutine =
            StartCoroutine(ChangeStrengthRoutine(strength));
    }


    // =========================================================
    // SMOOTH TRANSITION
    // =========================================================

    private IEnumerator ChangeStrengthRoutine(float targetStrength)
    {
        float startingStrength = currentStrength;

        float timer = 0f;

        float duration =
            Mathf.Max(0.01f, transitionDuration);


        // Start particle system when increasing from zero
        if (targetStrength > 0f &&
            flame != null &&
            !flame.isPlaying)
        {
            flame.Play();
        }


        while (timer < duration)
        {
            timer += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(timer / duration);

            float smoothTime =
                transitionCurve.Evaluate(normalizedTime);

            currentStrength =
                Mathf.Lerp(
                    startingStrength,
                    targetStrength,
                    smoothTime
                );

            ApplyStrength(currentStrength);

            yield return null;
        }


        currentStrength = targetStrength;

        ApplyStrength(currentStrength);


        // Completely stop after smooth fade-out
        if (currentStrength <= 0.001f &&
            flame != null)
        {
            flame.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }


        strengthRoutine = null;
    }


    // =========================================================
    // APPLY
    // =========================================================

    private void ApplyStrength(float strength)
    {
        strength = Mathf.Clamp01(strength);


        // -------------------------
        // PARTICLE SYSTEM
        // -------------------------

        if (flame != null)
        {
            var emission = flame.emission;

            emission.rateOverTime =
                Mathf.Lerp(
                    minimumEmission,
                    maximumEmission,
                    strength
                );


            var main = flame.main;

            main.startSpeed =
                Mathf.Lerp(
                    minimumSpeed,
                    maximumSpeed,
                    strength
                );


            main.startSize =
                Mathf.Lerp(
                    minimumParticleSize,
                    maximumParticleSize,
                    strength
                );
        }


        // -------------------------
        // FLAME SCALE
        // -------------------------

        if (controlFlameScale &&
            flameTransform != null)
        {
            flameTransform.localScale =
                Vector3.Lerp(
                    minimumScale,
                    maximumScale,
                    strength
                );
        }
    }


    // =========================================================
    // IMMEDIATE
    // =========================================================

    public void SetStrengthImmediate(float strength)
    {
        if (strengthRoutine != null)
        {
            StopCoroutine(strengthRoutine);
            strengthRoutine = null;
        }

        currentStrength =
            Mathf.Clamp01(strength);

        if (currentStrength > 0f &&
            flame != null &&
            !flame.isPlaying)
        {
            flame.Play();
        }

        ApplyStrength(currentStrength);

        if (currentStrength <= 0.001f &&
            flame != null)
        {
            flame.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }


    // =========================================================
    // SIGNAL EMITTER FRIENDLY PRESETS
    // =========================================================

    public void FlameOff()
    {
        SetStrength(0f);
    }

    public void Flame10()
    {
        SetStrength(0.10f);
    }

    public void Flame20()
    {
        SetStrength(0.20f);
    }

    public void Flame25()
    {
        SetStrength(0.25f);
    }

    public void Flame30()
    {
        SetStrength(0.30f);
    }

    public void Flame40()
    {
        SetStrength(0.40f);
    }

    public void Flame50()
    {
        SetStrength(0.50f);
    }

    public void Flame60()
    {
        SetStrength(0.60f);
    }

    public void Flame70()
    {
        SetStrength(0.70f);
    }

    public void Flame75()
    {
        SetStrength(0.75f);
    }

    public void Flame80()
    {
        SetStrength(0.80f);
    }

    public void Flame90()
    {
        SetStrength(0.90f);
    }

    public void Flame100()
    {
        SetStrength(1f);
    }
}