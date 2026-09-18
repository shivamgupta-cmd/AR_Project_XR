using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CyanOuterGlow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Empty = this GameObject renderer")]
    public Renderer targetRenderer;

    [Header("Children")]
    [Tooltip("ON = Parent + all children + children's children")]
    public bool includeChildren = false;

    [Header("Glow Settings")]
    [ColorUsage(true, true)]
    public Color glowColor = new Color(0f, 0.8f, 1f, 1f);

    [Range(0.001f, 0.1f)]
    public float outlineWidth = 0.015f;

    [Range(0f, 10f)]
    public float glowIntensity = 2f;

    // All generated glow objects
    private List<GameObject> glowObjects =
        new List<GameObject>();

    private List<Renderer> glowRenderers =
        new List<Renderer>();

    private Material glowMaterial;

    private bool created = false;

    // =====================================================
    // SCRIPT ON
    // =====================================================

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        CreateGlowIfNeeded();
        SetGlowState(true);
    }

    // =====================================================
    // SCRIPT OFF
    // =====================================================

    private void OnDisable()
    {
        SetGlowState(false);
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (!enabled)
            return;

        CreateGlowIfNeeded();
        SetGlowState(true);
    }

    // =====================================================
    // CREATE ALL GLOWS
    // =====================================================

    private void CreateGlowIfNeeded()
    {
        if (created)
            return;

        CreateGlowMaterial();

        if (glowMaterial == null)
            return;

        // =============================================
        // INCLUDE ALL CHILDREN
        // =============================================

        if (includeChildren)
        {
            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rend in renderers)
            {
                if (rend == null)
                    continue;

                // Don't create glow for already generated glow
                if (rend.gameObject.name.EndsWith("_OuterGlow"))
                    continue;

                CreateGlowForRenderer(rend);
            }
        }

        // =============================================
        // ONLY TARGET OBJECT
        // =============================================

        else
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer == null)
            {
                Debug.LogWarning(
                    "CyanOuterGlow: Renderer not found on " +
                    gameObject.name
                );

                return;
            }

            CreateGlowForRenderer(targetRenderer);
        }

        created = true;
    }

    // =====================================================
    // DETECT RENDERER TYPE
    // =====================================================

    private void CreateGlowForRenderer(Renderer source)
    {
        if (source == null)
            return;

        // ---------------------------------------------
        // NORMAL MESH
        // ---------------------------------------------

        MeshRenderer meshRenderer =
            source as MeshRenderer;

        if (meshRenderer != null)
        {
            CreateMeshGlow(meshRenderer);
            return;
        }

        // ---------------------------------------------
        // SKINNED MESH
        // ---------------------------------------------

        SkinnedMeshRenderer skinnedRenderer =
            source as SkinnedMeshRenderer;

        if (skinnedRenderer != null)
        {
            CreateSkinnedGlow(skinnedRenderer);
        }
    }

    // =====================================================
    // NORMAL MESH GLOW
    // =====================================================

    private void CreateMeshGlow(MeshRenderer source)
    {
        MeshFilter sourceFilter =
            source.GetComponent<MeshFilter>();

        if (sourceFilter == null)
            return;

        GameObject glowObject =
            new GameObject(
                source.gameObject.name + "_OuterGlow"
            );

        glowObject.transform.SetParent(
            source.transform,
            false
        );

        glowObject.transform.localPosition =
            Vector3.zero;

        glowObject.transform.localRotation =
            Quaternion.identity;

        glowObject.transform.localScale =
            Vector3.one *
            (1f + outlineWidth);

        // --------------------------
        // Mesh Filter
        // --------------------------

        MeshFilter newFilter =
            glowObject.AddComponent<MeshFilter>();

        newFilter.sharedMesh =
            sourceFilter.sharedMesh;

        // --------------------------
        // Mesh Renderer
        // --------------------------

        MeshRenderer newRenderer =
            glowObject.AddComponent<MeshRenderer>();

        SetMaterials(
            newRenderer,
            source.sharedMaterials.Length
        );

        ConfigureRenderer(newRenderer);

        // Store references
        glowObjects.Add(glowObject);
        glowRenderers.Add(newRenderer);
    }

    // =====================================================
    // SKINNED MESH GLOW
    // =====================================================

    private void CreateSkinnedGlow(
        SkinnedMeshRenderer source)
    {
        GameObject glowObject =
            new GameObject(
                source.gameObject.name + "_OuterGlow"
            );

        glowObject.transform.SetParent(
            source.transform.parent,
            false
        );

        glowObject.transform.localPosition =
            source.transform.localPosition;

        glowObject.transform.localRotation =
            source.transform.localRotation;

        glowObject.transform.localScale =
            source.transform.localScale *
            (1f + outlineWidth);

        SkinnedMeshRenderer newRenderer =
            glowObject.AddComponent<SkinnedMeshRenderer>();

        newRenderer.sharedMesh =
            source.sharedMesh;

        newRenderer.rootBone =
            source.rootBone;

        newRenderer.bones =
            source.bones;

        newRenderer.localBounds =
            source.localBounds;

        SetMaterials(
            newRenderer,
            source.sharedMaterials.Length
        );

        ConfigureRenderer(newRenderer);

        // Store references
        glowObjects.Add(glowObject);
        glowRenderers.Add(newRenderer);
    }

    // =====================================================
    // SET MATERIALS
    // =====================================================

    private void SetMaterials(
        Renderer renderer,
        int materialCount)
    {
        // Safety
        if (materialCount <= 0)
            materialCount = 1;

        Material[] mats =
            new Material[materialCount];

        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = glowMaterial;
        }

        renderer.sharedMaterials = mats;
    }

    // =====================================================
    // CREATE CYAN MATERIAL
    // =====================================================

    private void CreateGlowMaterial()
    {
        if (glowMaterial != null)
            return;

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
        {
            Debug.LogError(
                "CyanOuterGlow: URP Unlit shader not found!"
            );

            return;
        }

        glowMaterial =
            new Material(shader);

        glowMaterial.name =
            "Runtime_CyanOuterGlow";

        UpdateGlowMaterial();

        // -----------------------------------------
        // FRONT FACE CULL
        // Only outside shell visible
        // -----------------------------------------

        if (glowMaterial.HasProperty("_Cull"))
        {
            glowMaterial.SetFloat(
                "_Cull",
                (float)CullMode.Front
            );
        }

        glowMaterial.renderQueue = 2999;
    }

    // =====================================================
    // UPDATE GLOW COLOR
    // =====================================================

    private void UpdateGlowMaterial()
    {
        if (glowMaterial == null)
            return;

        Color finalColor =
            glowColor * glowIntensity;

        if (glowMaterial.HasProperty("_BaseColor"))
        {
            glowMaterial.SetColor(
                "_BaseColor",
                finalColor
            );
        }

        if (glowMaterial.HasProperty("_Color"))
        {
            glowMaterial.SetColor(
                "_Color",
                finalColor
            );
        }
    }

    // =====================================================
    // GLOW ON / OFF
    // =====================================================

    private void SetGlowState(bool state)
    {
        // Enable / Disable Renderers
        for (int i = 0; i < glowRenderers.Count; i++)
        {
            if (glowRenderers[i] != null)
            {
                glowRenderers[i].enabled = state;
            }
        }

        // Enable / Disable Objects
        for (int i = 0; i < glowObjects.Count; i++)
        {
            if (glowObjects[i] != null)
            {
                glowObjects[i].SetActive(state);
            }
        }
    }

    // =====================================================
    // RENDERER SETTINGS
    // =====================================================

    private void ConfigureRenderer(Renderer rend)
    {
        rend.shadowCastingMode =
            ShadowCastingMode.Off;

        rend.receiveShadows = false;

        rend.lightProbeUsage =
            LightProbeUsage.Off;

        rend.reflectionProbeUsage =
            ReflectionProbeUsage.Off;
    }

    // =====================================================
    // DESTROY GENERATED GLOW
    // =====================================================

    private void OnDestroy()
    {
        for (int i = 0; i < glowObjects.Count; i++)
        {
            if (glowObjects[i] != null)
            {
                Destroy(glowObjects[i]);
            }
        }

        glowObjects.Clear();
        glowRenderers.Clear();

        if (glowMaterial != null)
        {
            Destroy(glowMaterial);
        }
    }
}