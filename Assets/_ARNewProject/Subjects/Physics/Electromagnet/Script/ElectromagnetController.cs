using System.Collections;
using UnityEngine;

public class ElectromagnetController : MonoBehaviour
{
    [Header("Switch Glow")]
    [SerializeField] private MaterialGlowController switchGlow;

    [Header("Audio")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Voice Overs")]
    [SerializeField] private AudioClip introVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip clickVO;
    [SerializeField] private AudioClip howItWorksVO;

    [Header("Animation")]
    [SerializeField] private Animator electromagnetAnimator;

    [Header("Switch")]
    [SerializeField] private GameObject m_switch;


    [Header("Labels")]
    [SerializeField] private GameObject[] labelsRoot;

    [Header("Switch Material Highlight")]
    [SerializeField] private Renderer switchRenderer;
    [SerializeField] private Material switchHighlightMaterial;

    [Header("Switch Rotation")]
    [SerializeField] private Transform switchTransform;
    [SerializeField] private float switchOffX = 0f;
    [SerializeField] private float switchOnX = 90f;
    [SerializeField] private float switchRotationDuration = 0.35f;

    [Header("Module")]
    [SerializeField] private bool playIntroOnStart = true;

    private Coroutine moduleCoroutine;
    private Coroutine howItWorksCoroutine;
    private Coroutine switchRotationCoroutine;

    private bool readyForSwitch;
    private bool switchActivated;
    private bool activityPaused;

    private Material originalSwitchMaterial;
    private float originalSwitchY;
    private float originalSwitchZ;
    private float previousAnimatorSpeed;
    private bool previousGlowComponentEnabled;

    private void Awake()
    {
        if (switchRenderer != null)
            originalSwitchMaterial = switchRenderer.material;

        if (switchTransform == null && switchRenderer != null)
            switchTransform = switchRenderer.transform;

        if (switchTransform != null)
        {
            Vector3 euler = switchTransform.localEulerAngles;
            originalSwitchY = euler.y;
            originalSwitchZ = euler.z;
        }
    }

    private void Start()
    {
        activityPaused = false;
        SetLabels(false);
        SetSwitchHighlight(false);
        SetSwitchRotationInstant(switchOffX);

        if (switchGlow != null)
            switchGlow.GlowOff();

        if (m_switch != null)
            m_switch.SetActive(false);

        ResetAndStopAnimation();

        if (playIntroOnStart)
            moduleCoroutine = StartCoroutine(StartModuleRoutine());
    }

    private IEnumerator StartModuleRoutine()
    {
        readyForSwitch = false;
        switchActivated = false;

        SetSwitchHighlight(false);

        yield return StartCoroutine(PlayVoiceRoutine(introVO));
        yield return StartCoroutine(PlayVoiceRoutine(setupVO));

        while (activityPaused)
            yield return null;

        readyForSwitch = true;
        SetSwitchHighlight(true);

        if (m_switch != null)
            m_switch.SetActive(true);

        moduleCoroutine = null;
    }

    public void SwitchOn()
    {
        if (activityPaused)
            return;

        if (!readyForSwitch)
            return;

        if (switchActivated)
            return;

        switchActivated = true;
        readyForSwitch = false;

        SetSwitchHighlight(false);
        SetLabels(false);

        RotateSwitchTo(switchOnX);

        if (switchGlow != null)
            switchGlow.GlowOn();

        if (m_switch != null)
            m_switch.SetActive(false);

        howItWorksCoroutine = StartCoroutine(HowItWorksRoutine());
    }

    private IEnumerator HowItWorksRoutine()
    {
        yield return StartCoroutine(PlayVoiceRoutine(clickVO));

        while (activityPaused)
            yield return null;

        RestartAnimation();

        yield return StartCoroutine(PlayVoiceRoutine(howItWorksVO));

        while (activityPaused)
            yield return null;

        SetLabels(true);

        howItWorksCoroutine = null;
    }

    private IEnumerator PlayVoiceRoutine(AudioClip clip)
    {
        if (clip == null || voiceSource == null)
            yield break;

        while (activityPaused)
            yield return null;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();

        while (true)
        {
            if (activityPaused)
            {
                yield return null;
                continue;
            }

            if (!voiceSource.isPlaying)
                break;

            yield return null;
        }

        voiceSource.clip = null;
    }

    private void RotateSwitchTo(float targetX)
    {
        if (switchTransform == null)
            return;

        if (switchRotationCoroutine != null)
            StopCoroutine(switchRotationCoroutine);

        switchRotationCoroutine = StartCoroutine(RotateSwitchRoutine(targetX));
    }

    private IEnumerator RotateSwitchRoutine(float targetX)
    {
        Quaternion startRotation = switchTransform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(
            targetX,
            originalSwitchY,
            originalSwitchZ
        );

        float time = 0f;

        while (time < switchRotationDuration)
        {
            if (activityPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;

            float t = Mathf.Clamp01(
                time / switchRotationDuration
            );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            switchTransform.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        switchTransform.localRotation = targetRotation;
        switchRotationCoroutine = null;
    }

    private void SetSwitchRotationInstant(float xRotation)
    {
        if (switchTransform == null)
            return;

        switchTransform.localRotation =
            Quaternion.Euler(
                xRotation,
                originalSwitchY,
                originalSwitchZ
            );
    }

    private void SetLabels(bool value)
    {
        if (labelsRoot == null)
            return;

        for (int i = 0; i < labelsRoot.Length; i++)
        {
            if (labelsRoot[i] != null)
                labelsRoot[i].SetActive(value);
        }
    }

    private void SetSwitchHighlight(bool value)
    {
        if (switchRenderer == null)
            return;

        if (value)
        {
            if (switchHighlightMaterial != null)
                switchRenderer.material =
                    switchHighlightMaterial;
        }
        else
        {
            if (originalSwitchMaterial != null)
                switchRenderer.material =
                    originalSwitchMaterial;
        }
    }

    public void RestartAnimation()
    {
        if (electromagnetAnimator == null)
            return;

        electromagnetAnimator.enabled = true;
        electromagnetAnimator.Rebind();
        electromagnetAnimator.Update(0f);
        electromagnetAnimator.speed = 1f;
    }

    public void ResetAndStopAnimation()
    {
        if (electromagnetAnimator == null)
            return;

        electromagnetAnimator.enabled = true;
        electromagnetAnimator.Rebind();
        electromagnetAnimator.Update(0f);
        electromagnetAnimator.speed = 0f;
    }

    public void PauseAnimation()
    {
        if (electromagnetAnimator != null)
            electromagnetAnimator.speed = 0f;
    }

    public void ResumeAnimation()
    {
        if (electromagnetAnimator != null)
            electromagnetAnimator.speed = 1f;
    }

    public void PauseActivity()
    {
        if (activityPaused)
            return;

        activityPaused = true;

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Pause();

        if (electromagnetAnimator != null)
        {
            previousAnimatorSpeed =
                electromagnetAnimator.speed;

            electromagnetAnimator.speed = 0f;
        }

        if (switchGlow != null)
        {
            previousGlowComponentEnabled =
                switchGlow.enabled;

            switchGlow.enabled = false;
        }
    }

    public void ResumeActivity()
    {
        if (!activityPaused)
            return;

        activityPaused = false;

        if (voiceSource != null)
            voiceSource.UnPause();

        if (electromagnetAnimator != null)
            electromagnetAnimator.speed =
                previousAnimatorSpeed;

        if (switchGlow != null)
            switchGlow.enabled =
                previousGlowComponentEnabled;
    }

    public void StopVoice()
    {
        if (voiceSource == null)
            return;

        voiceSource.Stop();
        voiceSource.clip = null;
    }

    public void ResetModule()
    {
        activityPaused = false;

        if (moduleCoroutine != null)
        {
            StopCoroutine(moduleCoroutine);
            moduleCoroutine = null;
        }

        if (howItWorksCoroutine != null)
        {
            StopCoroutine(howItWorksCoroutine);
            howItWorksCoroutine = null;
        }

        if (switchRotationCoroutine != null)
        {
            StopCoroutine(switchRotationCoroutine);
            switchRotationCoroutine = null;
        }

        StopVoice();

        readyForSwitch = false;
        switchActivated = false;

        SetLabels(false);
        SetSwitchHighlight(false);
        SetSwitchRotationInstant(switchOffX);

        if (switchGlow != null)
        {
            switchGlow.enabled = true;
            switchGlow.GlowOff();
        }

        ResetAndStopAnimation();

        previousAnimatorSpeed = 0f;

        if (playIntroOnStart)
            moduleCoroutine =
                StartCoroutine(StartModuleRoutine());
    }
}