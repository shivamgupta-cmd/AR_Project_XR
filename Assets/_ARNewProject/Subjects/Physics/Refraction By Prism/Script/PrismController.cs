using System.Collections;
using UnityEngine;

public class PrismController : MonoBehaviour
{
    [Header("PRISM OBJECTS")]
    [SerializeField] private Transform introPrism;
    [SerializeField] private Transform animatedPrism;

    [Header("PRISM SCALE ANIMATION")]
    [SerializeField] private float prismAppearDuration = 1f;
    [SerializeField] private float prismDisappearDuration = 0.8f;

    [Header("INTRO PRISM ROTATION")]
    [SerializeField] private Vector3 introRotationAxis = Vector3.up;
    [SerializeField] private float introRotationSpeed = 35f;

    [Header("ANIMATOR")]
    [SerializeField] private Animator prismAnimator;

    [Header("TORCH")]
    [SerializeField] private Light torchLight;

    [Header("FORMULA PANEL")]
    [SerializeField] private CanvasGroup formulaPanelCanvasGroup;
    [SerializeField] private float formulaFadeDuration = 0.6f;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip introVO;
    [SerializeField] private AudioClip finalVO;

    [Header("TIMING")]
    [SerializeField] private float delayAfterIntro = 0.3f;
    [SerializeField] private float delayBeforeFinalVO = 0.5f;

    private Vector3 animatedPrismOriginalScale;
    private Quaternion introPrismOriginalRotation;

    private Coroutine mainSequenceCoroutine;
    private Coroutine formulaCoroutine;
    private Coroutine introRotationCoroutine;

    private bool rotateIntroPrism;
    private bool isPaused;

    private void Awake()
    {
        if (animatedPrism != null)
            animatedPrismOriginalScale = animatedPrism.localScale;

        if (introPrism != null)
        {
            introPrismOriginalRotation = introPrism.localRotation;
            introPrism.localScale = Vector3.zero;
        }

        if (animatedPrism != null)
            animatedPrism.localScale = Vector3.zero;
    }

    private void Start()
    {
        PrepareModule();

        mainSequenceCoroutine =
            StartCoroutine(ModuleSequence());
    }

    private void PrepareModule()
    {
        isPaused = false;

        if (introPrism != null)
        {
            introPrism.gameObject.SetActive(true);
            introPrism.localScale = Vector3.zero;
            introPrism.localRotation = introPrismOriginalRotation;
        }

        if (animatedPrism != null)
        {
            animatedPrism.localScale = Vector3.zero;
            animatedPrism.gameObject.SetActive(false);
        }

        if (prismAnimator != null)
        {
            prismAnimator.enabled = false;
            prismAnimator.speed = 1f;
        }

        if (torchLight != null)
            torchLight.enabled = false;

        if (formulaPanelCanvasGroup != null)
        {
            formulaPanelCanvasGroup.alpha = 0f;
            formulaPanelCanvasGroup.interactable = false;
            formulaPanelCanvasGroup.blocksRaycasts = false;
            formulaPanelCanvasGroup.gameObject.SetActive(false);
        }

        HideLabels();
    }

    private IEnumerator ModuleSequence()
    {
        if (introPrism != null)
        {
            introPrism.gameObject.SetActive(true);
            introPrism.localScale = Vector3.zero;

            rotateIntroPrism = true;

            introRotationCoroutine =
                StartCoroutine(RotateIntroPrism());

            yield return StartCoroutine(
                ScaleObject(
                    introPrism,
                    Vector3.zero,
                    Vector3.one,
                    prismAppearDuration
                )
            );
        }

        if (introVO != null)
        {
            PlayAudio(introVO);

            yield return StartCoroutine(
                WaitForActivitySeconds(introVO.length)
            );
        }

        if (delayAfterIntro > 0f)
        {
            yield return StartCoroutine(
                WaitForActivitySeconds(delayAfterIntro)
            );
        }

        if (introPrism != null)
        {
            yield return StartCoroutine(
                ScaleObject(
                    introPrism,
                    introPrism.localScale,
                    Vector3.zero,
                    prismDisappearDuration
                )
            );

            rotateIntroPrism = false;

            if (introRotationCoroutine != null)
            {
                StopCoroutine(introRotationCoroutine);
                introRotationCoroutine = null;
            }

            introPrism.gameObject.SetActive(false);
        }

        if (animatedPrism != null)
        {
            animatedPrism.gameObject.SetActive(true);
            animatedPrism.localScale = Vector3.zero;

            yield return StartCoroutine(
                ScaleObject(
                    animatedPrism,
                    Vector3.zero,
                    animatedPrismOriginalScale,
                    prismAppearDuration
                )
            );
        }

        TurnTorchOn();

        StartPrismAnimation();

        if (delayBeforeFinalVO > 0f)
        {
            yield return StartCoroutine(
                WaitForActivitySeconds(delayBeforeFinalVO)
            );
        }

        if (finalVO != null)
        {
            PlayAudio(finalVO);

            yield return StartCoroutine(
                WaitForActivitySeconds(finalVO.length)
            );
        }

        ShowLabels();

        mainSequenceCoroutine = null;
    }

    private IEnumerator RotateIntroPrism()
    {
        while (rotateIntroPrism)
        {
            if (!isPaused && introPrism != null)
            {
                introPrism.Rotate(
                    introRotationAxis.normalized,
                    introRotationSpeed * Time.deltaTime,
                    Space.Self
                );
            }

            yield return null;
        }
    }

    private IEnumerator ScaleObject(
        Transform target,
        Vector3 from,
        Vector3 to,
        float duration
    )
    {
        if (target == null)
            yield break;

        target.localScale = from;

        if (duration <= 0f)
        {
            target.localScale = to;
            yield break;
        }

        float time = 0f;

        while (time < duration)
        {
            if (!isPaused)
            {
                time += Time.deltaTime;

                float t = Mathf.Clamp01(time / duration);

                t = Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

                target.localScale =
                    Vector3.LerpUnclamped(
                        from,
                        to,
                        t
                    );
            }

            yield return null;
        }

        target.localScale = to;
    }

    private IEnumerator WaitForActivitySeconds(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if (!isPaused)
                time += Time.deltaTime;

            yield return null;
        }
    }

    public void ShowFormulaAndLabels()
    {
        if (formulaCoroutine != null)
            StopCoroutine(formulaCoroutine);

        formulaCoroutine =
            StartCoroutine(ShowFormulaAndLabelsRoutine());
    }

    private IEnumerator ShowFormulaAndLabelsRoutine()
    {
        ShowLabels();

        if (formulaPanelCanvasGroup != null)
        {
            formulaPanelCanvasGroup.gameObject.SetActive(true);

            formulaPanelCanvasGroup.interactable = false;
            formulaPanelCanvasGroup.blocksRaycasts = false;

            yield return StartCoroutine(
                FadeCanvasGroup(
                    formulaPanelCanvasGroup,
                    formulaPanelCanvasGroup.alpha,
                    1f,
                    formulaFadeDuration
                )
            );

            formulaPanelCanvasGroup.interactable = true;
            formulaPanelCanvasGroup.blocksRaycasts = true;
        }

        formulaCoroutine = null;
    }

    public void HideFormulaAndLabels()
    {
        if (formulaCoroutine != null)
            StopCoroutine(formulaCoroutine);

        formulaCoroutine =
            StartCoroutine(HideFormulaAndLabelsRoutine());
    }

    private IEnumerator HideFormulaAndLabelsRoutine()
    {
        HideLabels();

        if (formulaPanelCanvasGroup != null)
        {
            formulaPanelCanvasGroup.interactable = false;
            formulaPanelCanvasGroup.blocksRaycasts = false;

            yield return StartCoroutine(
                FadeCanvasGroup(
                    formulaPanelCanvasGroup,
                    formulaPanelCanvasGroup.alpha,
                    0f,
                    formulaFadeDuration
                )
            );

            formulaPanelCanvasGroup.gameObject.SetActive(false);
        }

        formulaCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float from,
        float to,
        float duration
    )
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.alpha = from;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float time = 0f;

        while (time < duration)
        {
            if (!isPaused)
            {
                time += Time.deltaTime;

                float t = Mathf.Clamp01(time / duration);

                t = Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        from,
                        to,
                        t
                    );
            }

            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void ShowLabels()
    {
        if (labels == null)
            return;

        foreach (GameObject label in labels)
        {
            if (label != null)
                label.SetActive(true);
        }
    }

    public void HideLabels()
    {
        if (labels == null)
            return;

        foreach (GameObject label in labels)
        {
            if (label != null)
                label.SetActive(false);
        }
    }

    public void TurnTorchOn()
    {
        if (torchLight != null)
            torchLight.enabled = true;
    }

    public void TurnTorchOff()
    {
        if (torchLight != null)
            torchLight.enabled = false;
    }

    public void StartPrismAnimation()
    {
        if (prismAnimator == null)
            return;

        prismAnimator.enabled = true;
        prismAnimator.speed = 1f;
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PauseActivity()
    {
        if (isPaused)
            return;

        isPaused = true;

        if (audioSource != null)
            audioSource.Pause();

        if (prismAnimator != null &&
            prismAnimator.enabled)
        {
            prismAnimator.speed = 0f;
        }
    }

    public void ResumeActivity()
    {
        if (!isPaused)
            return;

        isPaused = false;

        if (audioSource != null)
            audioSource.UnPause();

        if (prismAnimator != null &&
            prismAnimator.enabled)
        {
            prismAnimator.speed = 1f;
        }
    }
}