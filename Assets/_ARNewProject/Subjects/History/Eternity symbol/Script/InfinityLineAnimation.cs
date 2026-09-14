using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class InfinityLineAnimation : MonoBehaviour
{
    [Header("Infinity Shape")]
    public float width = 0.6f;
    public float height = 0.37f;

    [Header("Path")]
    [Min(20)]
    public int segments = 200;

    [Header("Animation")]
    [Tooltip("Used only when VO is not assigned.")]
    public float defaultPartDuration = 2f;

    [Header("Starting Point")]
    public float startingPointDelay = 1.5f;

    [Header("Voice Over")]
    public AudioClip startingPointVO;
    public AudioClip lineMovementVO;
    public AudioClip firstLoopVO;
    public AudioClip secondLoopVO;
    public AudioClip intersectionVO;
    public AudioClip completeSymbolVO;

    [Header("Explore Parts UI")]
    public GameObject left;
    public GameObject center;
    public GameObject right;

    private LineRenderer line;
    private AudioSource audioSource;

    private Vector3[] path;

    private GameObject movingPoint;
    private Light pointLight;

    private bool animationFinished = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();

        line.useWorldSpace = false;
        line.loop = false;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        GeneratePath();

        // Hide infinity line initially
        line.positionCount = 0;

        // Hide Explore Parts buttons initially
        HideExploreButtons();

        // Create moving point
        CreateMovingPoint();

        // Start sequence
        StartCoroutine(StartExperience());
    }

    // =========================================================
    // HIDE ALL EXPLORE BUTTONS
    // =========================================================

    private void HideExploreButtons()
    {
        if (left != null)
            left.SetActive(false);

        if (center != null)
            center.SetActive(false);

        if (right != null)
            right.SetActive(false);
    }

    // =========================================================
    // MAIN EXPERIENCE
    // =========================================================

    private IEnumerator StartExperience()
    {
        // =====================================================
        // 1. STARTING POINT
        // =====================================================

        ShowStartingPoint();

        yield return new WaitForSeconds(
            startingPointDelay
        );

        // Starting Point VO
        yield return StartCoroutine(
            PlayVoice(startingPointVO)
        );


        // =====================================================
        // 2. LINE MOVEMENT
        // =====================================================

        // VO + line movement together
        yield return StartCoroutine(
            PlayVoiceAndDraw(
                lineMovementVO,
                0f,
                0.25f
            )
        );


        // =====================================================
        // 3. FIRST LOOP
        // =====================================================

        // First Loop VO + first loop drawing
        // happen together.
        yield return StartCoroutine(
            PlayVoiceAndDraw(
                firstLoopVO,
                0.25f,
                0.5f
            )
        );

        // =====================================================
        // LEFT LOOP BUTTON APPEARS
        // =====================================================

        ShowLeftButton();


        // =====================================================
        // 4. SECOND LOOP
        // =====================================================

        // Second Loop VO + second loop drawing
        // happen together.
        yield return StartCoroutine(
            PlayVoiceAndDraw(
                secondLoopVO,
                0.5f,
                1f
            )
        );

        // =====================================================
        // RIGHT LOOP BUTTON APPEARS
        // =====================================================

        ShowRightButton();


        // =====================================================
        // 5. INTERSECTION
        // =====================================================

        // Move point to center
        MovePoint(0.5f);

        // =====================================================
        // CENTER BUTTON APPEARS WHEN INTERSECTION VO STARTS
        // =====================================================

        ShowCenterButton();

        // Intersection VO
        yield return StartCoroutine(
            PlayVoice(intersectionVO)
        );

        // Intersection glow after VO
        yield return StartCoroutine(
            IntersectionEffect()
        );


        // =====================================================
        // 6. COMPLETE SYMBOL
        // =====================================================

        // Complete Symbol VO
        yield return StartCoroutine(
            PlayVoice(completeSymbolVO)
        );

        // Keep complete symbol visible
        ShowCompleteSymbol();

        animationFinished = true;
    }

    // =========================================================
    // SHOW LEFT BUTTON
    // =========================================================

    private void ShowLeftButton()
    {
        if (left != null)
        {
            left.SetActive(true);
        }
    }

    // =========================================================
    // SHOW RIGHT BUTTON
    // =========================================================

    private void ShowRightButton()
    {
        if (right != null)
        {
            right.SetActive(true);
        }
    }

    // =========================================================
    // SHOW CENTER BUTTON
    // =========================================================

    private void ShowCenterButton()
    {
        if (center != null)
        {
            center.SetActive(true);
        }
    }

    // =========================================================
    // VO + DRAW TOGETHER
    // =========================================================

    private IEnumerator PlayVoiceAndDraw(
        AudioClip clip,
        float start,
        float end)
    {
        float duration = defaultPartDuration;

        // If VO exists, animation duration
        // will be exactly same as VO duration.
        if (clip != null)
        {
            duration = clip.length;

            audioSource.Stop();

            audioSource.clip = clip;

            audioSource.Play();
        }

        float elapsed = 0f;

        // =====================================================
        // DRAW WHILE VO IS PLAYING
        // =====================================================

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float current =
                Mathf.Lerp(
                    start,
                    end,
                    normalized
                );

            DrawProgress(current);

            yield return null;
        }

        // Make sure final position is exact
        DrawProgress(end);

        // =====================================================
        // WAIT UNTIL VO REALLY FINISHES
        // =====================================================

        if (clip != null)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }
    }

    // =========================================================
    // DRAW PROGRESS
    // =========================================================

    private void DrawProgress(float progress)
    {
        progress =
            Mathf.Clamp01(progress);

        int pointCount =
            Mathf.RoundToInt(
                progress * (segments - 1)
            ) + 1;

        pointCount =
            Mathf.Clamp(
                pointCount,
                1,
                segments
            );

        line.positionCount =
            pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            line.SetPosition(
                i,
                path[i]
            );
        }

        // Moving point follows path
        MovePoint(progress);
    }

    // =========================================================
    // PLAY VOICE ONLY
    // =========================================================

    private IEnumerator PlayVoice(AudioClip clip)
    {
        if (clip == null)
            yield break;

        audioSource.Stop();

        audioSource.clip = clip;

        audioSource.Play();

        while (audioSource.isPlaying)
        {
            yield return null;
        }
    }

    // =========================================================
    // GENERATE INFINITY PATH
    // =========================================================

    private void GeneratePath()
    {
        path =
            new Vector3[segments];

        for (int i = 0; i < segments; i++)
        {
            float t =
                (float)i /
                (segments - 1) *
                Mathf.PI *
                2f;

            // First loop goes LEFT
            float x =
                -width *
                Mathf.Sin(t);

            float y =
                height *
                Mathf.Sin(2f * t);

            path[i] =
                new Vector3(
                    x,
                    y,
                    0f
                );
        }
    }

    // =========================================================
    // SHOW STARTING POINT
    // =========================================================

    private void ShowStartingPoint()
    {
        if (movingPoint == null)
            return;

        movingPoint.SetActive(true);

        MovePoint(0f);

        line.positionCount = 0;

        if (pointLight != null)
        {
            pointLight.intensity = 2f;
        }
    }

    // =========================================================
    // MOVE POINT
    // =========================================================

    private void MovePoint(float progress)
    {
        if (movingPoint == null)
            return;

        progress =
            Mathf.Clamp01(progress);

        float t =
            progress *
            Mathf.PI *
            2f;

        movingPoint.transform.localPosition =
            GetPoint(t);
    }

    // =========================================================
    // GET POINT
    // =========================================================

    public Vector3 GetPoint(float t)
    {
        float x =
            -width *
            Mathf.Sin(t);

        float y =
            height *
            Mathf.Sin(2f * t);

        return new Vector3(
            x,
            y,
            -0.02f
        );
    }

    // =========================================================
    // INTERSECTION EFFECT
    // =========================================================

    private IEnumerator IntersectionEffect()
    {
        if (pointLight == null)
            yield break;

        // Center intersection
        MovePoint(0.5f);

        float originalIntensity =
            pointLight.intensity;

        // First flash
        pointLight.intensity = 5f;

        yield return new WaitForSeconds(
            0.25f
        );

        pointLight.intensity = 1f;

        yield return new WaitForSeconds(
            0.15f
        );

        // Second flash
        pointLight.intensity = 5f;

        yield return new WaitForSeconds(
            0.25f
        );

        pointLight.intensity =
            originalIntensity;
    }

    // =========================================================
    // COMPLETE SYMBOL
    // =========================================================

    private void ShowCompleteSymbol()
    {
        if (path == null)
            return;

        line.positionCount =
            segments;

        for (int i = 0; i < segments; i++)
        {
            line.SetPosition(
                i,
                path[i]
            );
        }

        // Point back to starting center
        MovePoint(0f);

        if (pointLight != null)
        {
            pointLight.intensity = 2f;
        }
    }

    // =========================================================
    // CREATE MOVING POINT
    // =========================================================

    private void CreateMovingPoint()
    {
        movingPoint =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

        movingPoint.name =
            "Auto Moving Point";

        movingPoint.transform.SetParent(
            transform
        );

        movingPoint.transform.localScale =
            Vector3.one * 0.05f;

        // Remove collider
        Collider collider =
            movingPoint.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        movingPoint.transform.localPosition =
            GetPoint(0f);

        // =====================================================
        // POINT LIGHT
        // =====================================================

        GameObject lightObject =
            new GameObject(
                "Point Glow"
            );

        lightObject.transform.SetParent(
            movingPoint.transform
        );

        lightObject.transform.localPosition =
            Vector3.zero;

        pointLight =
            lightObject.AddComponent<Light>();

        pointLight.type =
            LightType.Point;

        pointLight.range = 1f;

        pointLight.intensity = 2f;

        pointLight.color =
            new Color(
                1f,
                0.55f,
                0.1f
            );

        // =====================================================
        // POINT MATERIAL
        // =====================================================

        Renderer renderer =
            movingPoint.GetComponent<Renderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Unlit/Color"
                );
        }

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Standard"
                );
        }

        if (shader != null)
        {
            Material material =
                new Material(shader);

            Color pointColor =
                new Color(
                    1f,
                    0.65f,
                    0.1f
                );

            if (material.HasProperty(
                "_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    pointColor
                );
            }

            if (material.HasProperty(
                "_Color"))
            {
                material.SetColor(
                    "_Color",
                    pointColor
                );
            }

            if (material.HasProperty(
                "_EmissionColor"))
            {
                material.EnableKeyword(
                    "_EMISSION"
                );

                material.SetColor(
                    "_EmissionColor",
                    pointColor * 3f
                );
            }

            renderer.material =
                material;
        }
    }

    // =========================================================
    // PLAY / PAUSE
    // =========================================================

    public void PlayPause()
    {
        if (animationFinished)
            return;

        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetAnimation()
    {
        StopAllCoroutines();

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        animationFinished = false;

        // Hide line
        if (line != null)
        {
            line.positionCount = 0;
        }

        // Hide Explore Parts buttons
        HideExploreButtons();

        // Reset moving point
        if (movingPoint != null)
        {
            movingPoint.SetActive(true);

            MovePoint(0f);
        }

        if (pointLight != null)
        {
            pointLight.intensity = 2f;
        }

        // Restart
        StartCoroutine(
            StartExperience()
        );
    }
}