using UnityEngine;
using System.Collections;

public class TestTubeHeatingEffect : MonoBehaviour
{
    [Header("HEATED MATERIAL")]
    [SerializeField] private Renderer heatRenderer;

    [Header("HEAT COLOR")]
    [ColorUsage(true, true)]
    [SerializeField]
    private Color heatColor =
        new Color(1f, 0.15f, 0.01f, 1f);

    [SerializeField] private float maxEmissionIntensity = 5f;

    [Header("TIMING")]
    [SerializeField] private float heatUpDuration = 2f;
    [SerializeField] private float coolDownDuration = 2f;

    [Header("OPTIONAL PARTICLES")]
    [SerializeField] private ParticleSystem heatParticles;

    [Header("OPTIONAL LIGHT")]
    [SerializeField] private Light heatLight;
    [SerializeField] private float maxLightIntensity = 2f;

    private Material material;

    private Coroutine heatingCoroutine;

    private float currentHeat = 0f;

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");


    private void Awake()
    {
        if (heatRenderer != null)
        {
            material = heatRenderer.material;

            material.EnableKeyword("_EMISSION");

            SetHeat(0f);
        }

        if (heatLight != null)
        {
            heatLight.intensity = 0f;
        }
    }


    // =========================================================
    // TIMELINE CALL
    // =========================================================

    private void Start()
    {
        StartHeating();
    }
    public void StartHeating()
    {
        if (heatingCoroutine != null)
        {
            StopCoroutine(heatingCoroutine);
        }

        heatingCoroutine =
            StartCoroutine(
                HeatUp()
            );

        if (heatParticles != null)
        {
            heatParticles.Play();
        }
    }


    // =========================================================
    // TIMELINE CALL
    // =========================================================

    public void StopHeating()
    {
        if (heatingCoroutine != null)
        {
            StopCoroutine(heatingCoroutine);
        }

        heatingCoroutine =
            StartCoroutine(
                CoolDown()
            );

        if (heatParticles != null)
        {
            heatParticles.Stop();
        }
    }


    // =========================================================
    // HEAT UP
    // =========================================================

    IEnumerator HeatUp()
    {
        float startHeat =
            currentHeat;

        float timer = 0f;


        while (timer < heatUpDuration)
        {
            timer +=
                Time.deltaTime;

            currentHeat =
                Mathf.Lerp(
                    startHeat,
                    1f,
                    timer / heatUpDuration
                );

            SetHeat(
                currentHeat
            );

            yield return null;
        }


        currentHeat =
            1f;

        SetHeat(
            currentHeat
        );
    }


    // =========================================================
    // COOL DOWN
    // =========================================================

    IEnumerator CoolDown()
    {
        float startHeat =
            currentHeat;

        float timer = 0f;


        while (timer < coolDownDuration)
        {
            timer +=
                Time.deltaTime;

            currentHeat =
                Mathf.Lerp(
                    startHeat,
                    0f,
                    timer / coolDownDuration
                );

            SetHeat(
                currentHeat
            );

            yield return null;
        }


        currentHeat =
            0f;

        SetHeat(
            0f
        );
    }


    // =========================================================
    // APPLY HEAT
    // =========================================================

    void SetHeat(float value)
    {
        if (material != null)
        {
            Color emission =
                heatColor *
                maxEmissionIntensity *
                value;

            material.SetColor(
                EmissionColor,
                emission
            );
        }


        if (heatLight != null)
        {
            heatLight.intensity =
                maxLightIntensity *
                value;
        }
    }
}