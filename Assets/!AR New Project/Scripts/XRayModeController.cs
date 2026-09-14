using System.Collections.Generic;
using UnityEngine;

public class XRayModeController : MonoBehaviour
{
    [Header("X-Ray Material")]
    [SerializeField] private Material xRayMaterial;

    private Renderer[] allRenderers;

    private readonly Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();

    private bool isXRayMode = false;


    private void Awake()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in allRenderers)
        {
            originalMaterials[rend] = rend.sharedMaterials;
        }
    }


    public void ToggleXRayMode()
    {
        if (isXRayMode)
            DisableXRay();
        else
            EnableXRay();
    }


    public void EnableXRay()
    {
        if (xRayMaterial == null)
        {
            Debug.LogError("X-Ray Material assign nahi hai!");
            return;
        }

        isXRayMode = true;

        foreach (Renderer rend in allRenderers)
        {
            Material[] currentMaterials = rend.sharedMaterials;

            Material[] xrayMaterials =
                new Material[currentMaterials.Length];

            for (int i = 0; i < xrayMaterials.Length; i++)
            {
                xrayMaterials[i] = xRayMaterial;
            }

            rend.sharedMaterials = xrayMaterials;
        }
    }


    public void DisableXRay()
    {
        isXRayMode = false;

        foreach (Renderer rend in allRenderers)
        {
            if (originalMaterials.TryGetValue(rend, out Material[] materials))
            {
                rend.sharedMaterials = materials;
            }
        }
    }
}