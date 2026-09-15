using System.Collections;
using UnityEngine;


public class RoyalSymbolHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float maxIntensity = 1.2f;
    public float blinkSpeed = 1f;

    private Renderer[] renderers;
    private Material[] materials;
    private Color[] originalEmissionColors;
    private bool[] hadEmission;

    private Coroutine blinkRoutine;

    private void Awake()
    {
        // Get all renderers including child objects
        renderers = GetComponentsInChildren<Renderer>(true);

        materials = new Material[renderers.Length];
        originalEmissionColors = new Color[renderers.Length];
        hadEmission = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Creates material instance so original asset is not modified
            materials[i] = renderers[i].material;

            if (materials[i].HasProperty("_EmissionColor"))
            {
                hadEmission[i] = materials[i].IsKeywordEnabled("_EMISSION");
                originalEmissionColors[i] =
                    materials[i].GetColor("_EmissionColor");
            }
        }
    }

    public void StartHighlight()
    {
        if (blinkRoutine != null)
            return;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null &&
                materials[i].HasProperty("_EmissionColor"))
            {
                materials[i].EnableKeyword("_EMISSION");
            }
        }

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void StopHighlight()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        RestoreOriginalEmission();
    }

    private IEnumerator BlinkRoutine()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * blinkSpeed;

            float intensity = Mathf.PingPong(time, 1f);

            float emissionIntensity =
                Mathf.Lerp(0f, maxIntensity, intensity);

            Color finalColor =
                highlightColor * emissionIntensity;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null &&
                    materials[i].HasProperty("_EmissionColor"))
                {
                    materials[i].SetColor(
                        "_EmissionColor",
                        finalColor
                    );
                }
            }

            yield return null;
        }
    }

    public void RestoreOriginalEmission()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null ||
                !materials[i].HasProperty("_EmissionColor"))
                continue;

            materials[i].SetColor(
                "_EmissionColor",
                originalEmissionColors[i]
            );

            if (hadEmission[i])
                materials[i].EnableKeyword("_EMISSION");
            else
                materials[i].DisableKeyword("_EMISSION");
        }
    }

    public void EnableEmission()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null &&
                materials[i].HasProperty("_EmissionColor"))
            {
                materials[i].EnableKeyword("_EMISSION");

                materials[i].SetColor(
                    "_EmissionColor",
                    originalEmissionColors[i]
                );
            }
        }
    }

    public void DisableEmission()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null &&
                materials[i].HasProperty("_EmissionColor"))
            {
                materials[i].SetColor(
                    "_EmissionColor",
                    Color.black
                );

                materials[i].DisableKeyword("_EMISSION");
            }
        }
    }
}
