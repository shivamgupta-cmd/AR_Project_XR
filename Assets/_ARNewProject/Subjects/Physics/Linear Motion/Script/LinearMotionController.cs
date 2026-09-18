using System.Collections;
using TMPro;
using UnityEngine;

public class LinearMotionController : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] private Transform m_car;

    [Header("3D Camera")]
    [Tooltip("Assign the normal 3D camera, not the AR camera.")]
    [SerializeField] private Transform m_cameraParent;
    [SerializeField] private Transform m_camera;
    [SerializeField] private bool followCamera = true;

    [Header("Movement")]
    [Tooltip("Speed along the car parent's local +Z axis.")]
    [SerializeField, Min(0f)] private float speed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip welcomeVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip averageSpeed;
    [SerializeField] private AudioClip uniformMotion;
    [SerializeField] private AudioClip nonUniformMotion;
    [SerializeField] private AudioClip realLife;
    [SerializeField] private AudioClip finalvo;

    [Header("Formula Panel")]
    [SerializeField] private GameObject PanelParent;
    [SerializeField] private CanvasGroup[] m_panels;
    private int m_currentPanels = -1;
    [Range(1,10)][SerializeField] private float m_smoothnessSpeed;

    private bool panelTransitionComplete = true;

    [Header("Stopwatch")]
    [SerializeField] private Transform stopwatchRoot;
    [SerializeField] private TMP_Text stopwatchText;
    [SerializeField] private Vector3 stopwatchTargetScale = Vector3.one;
    [SerializeField, Min(0f)] private float stopwatchScaleDuration = 0.5f;

    private bool isInitialized;
    private bool isPaused;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private Vector3 cameraOffset;
    private Quaternion initialCameraRotation;

    private bool stopwatchStarted;
    private bool stopwatchScaleCompleted;

    private float stopwatchScaleElapsed;
    private double stopwatchElapsedTime;

    private int lastDisplayedSecond = -1;

    private void Awake()
    {
        if (m_car == null || audioSource == null)
        {
            Debug.LogError("Assign M Car and Audio Source.", this);
            enabled = false;
            return;
        }

        initialLocalPosition = m_car.localPosition;
        initialLocalRotation = m_car.localRotation;

        if (m_camera != null && (m_camera == m_car || m_car.IsChildOf(m_camera)))
        {
            m_camera = null;
            followCamera = false;
        }

        if (m_camera != null)
        {
            cameraOffset = m_camera.position - m_car.position;
            initialCameraRotation = m_camera.rotation;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.Stop();

        isInitialized = true;
    }

    private void Start()
    {
        StartActivity();
    }


    private void Update()
    {
        if (!isInitialized || isPaused)
            return;

        PanelManager(m_currentPanels);
    }

    private void PanelManager(int index)
    {
        panelTransitionComplete = true;

        if (m_panels == null)
            return;

        for (int i = 0; i < m_panels.Length; i++)
        {
            CanvasGroup panel = m_panels[i];

            if (panel == null)
                continue;

            bool selected = i == index;
            float targetAlpha = selected ? 1f : 0f;

            panel.alpha = Mathf.Lerp(
                panel.alpha,
                targetAlpha,
                Time.deltaTime * m_smoothnessSpeed
            );

            if (Mathf.Abs(panel.alpha - targetAlpha) <= 0.001f)
            {
                panel.alpha = targetAlpha;
            }
            else
            {
                panelTransitionComplete = false;
            }

            bool canInteract = selected && panel.alpha >= 0.99f;

            panel.interactable = canInteract;
            panel.blocksRaycasts = canInteract;
        }
    }

    private void ShowPanel(int index)
    {
        m_currentPanels = index;
        panelTransitionComplete = false;

        if (index >= 0 && PanelParent != null)
        {
            PanelParent.SetActive(true);
        }
    }

    private IEnumerator HidePanels()
    {
        ShowPanel(-1);

        while (isPaused || !panelTransitionComplete)
        {
            yield return null;
        }

        if (PanelParent != null)
        {
            PanelParent.SetActive(false);
        }
    }





    public void StartActivity()
    {
        if (!isInitialized || !isActiveAndEnabled || m_car == null || audioSource == null)
        {
            return;
        }

        ResetCar();

        StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        PlayAudio(welcomeVO);
        yield return WaitForVoiceOver();

        PlayAudio(setupVO);

        Coroutine movementCoroutine = StartCoroutine(MoveCarAndStopwatch());

        yield return WaitForVoiceOver();

        ShowPanel(0);
        PlayAudio(averageSpeed);
        yield return WaitForVoiceOver();

        if (stopwatchRoot != null)
        {
            stopwatchRoot.gameObject.SetActive(false);
        }

        ShowPanel(1);
        PlayAudio(uniformMotion);
        yield return WaitForVoiceOver();

        ShowPanel(2);
        speed = 20f;
        PlayAudio(nonUniformMotion);
        yield return WaitForVoiceOver();

        ShowPanel(3);
        PlayAudio(realLife);
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        yield return WaitForVoiceOver();

        yield return HidePanels();

        PlayAudio(finalvo);
        yield return WaitForVoiceOver();
    }

    public void ResetCar()
    {
        if (!isInitialized || m_car == null)
            return;

        StopActivity();

        m_car.localPosition = initialLocalPosition;
        m_car.localRotation = initialLocalRotation;

        FollowCamera();

        if (followCamera && m_camera != null)
        {
            m_camera.rotation = initialCameraRotation;
        }

        stopwatchStarted = false;
        stopwatchScaleCompleted = false;
        stopwatchScaleElapsed = 0f;
        stopwatchElapsedTime = 0d;
        lastDisplayedSecond = -1;

        if (stopwatchRoot != null)
        {
            stopwatchRoot.localScale = Vector3.zero;
            stopwatchRoot.gameObject.SetActive(false);
        }

        ShowStopwatchTime();
        //ShowPanel(4);
    }

    private IEnumerator MoveCarAndStopwatch()
    {
        while (true)
        {
            yield return null;

            if (m_car == null)
                yield break;

            if (isPaused || !m_car.gameObject.activeInHierarchy)
                continue;

            float deltaTime = Time.deltaTime;

            if (deltaTime <= 0f)
                continue;

            if (speed > 0f)
            {
                Vector3 position = m_car.localPosition;

                position.z += speed * deltaTime;

                m_car.localPosition = position;

                if (!stopwatchStarted)
                {
                    stopwatchStarted = true;

                    if (stopwatchRoot != null)
                    {
                        stopwatchRoot.gameObject.SetActive(true);
                    }
                }
            }

            if (stopwatchStarted)
            {
                TickStopwatch(deltaTime);
            }
            FollowCamera();
        }
    }

    private void TickStopwatch(float deltaTime)
    {
        if (!stopwatchScaleCompleted)
        {
            float remaining = Mathf.Max(
                0f,
                stopwatchScaleDuration - stopwatchScaleElapsed
            );

            float animationStep = Mathf.Min(deltaTime, remaining);

            stopwatchScaleElapsed += animationStep;

            deltaTime -= animationStep;

            float progress = stopwatchScaleDuration <= 0f
                ? 1f
                : Mathf.Clamp01(
                    stopwatchScaleElapsed / stopwatchScaleDuration
                );

            if (stopwatchRoot != null)
            {
                stopwatchRoot.localScale = Vector3.Lerp(
                    Vector3.zero,
                    stopwatchTargetScale,
                    Mathf.SmoothStep(0f, 1f, progress)
                );
            }

            if (stopwatchScaleElapsed < stopwatchScaleDuration)
                return;

            stopwatchScaleCompleted = true;

            if (stopwatchRoot != null)
            {
                stopwatchRoot.localScale = stopwatchTargetScale;
            }
        }

        stopwatchElapsedTime += deltaTime;

        ShowStopwatchTime();
    }

    private void ShowStopwatchTime()
    {
        if (stopwatchText == null)
            return;

        int totalSeconds = (int)stopwatchElapsedTime;

        if (totalSeconds == lastDisplayedSecond)
            return;

        lastDisplayedSecond = totalSeconds;

        stopwatchText.text =
            $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void FollowCamera()
    {
        if (followCamera &&
            m_camera != null &&
            m_car != null &&
            m_camera.gameObject.activeInHierarchy)
        {
            m_camera.position = m_car.position + cameraOffset;
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();

        audioSource.loop = false;
        audioSource.clip = clip;

        audioSource.Play();
    }

    private IEnumerator WaitForVoiceOver()
    {
        yield return null;

        while (isPaused ||
               (audioSource != null && audioSource.isPlaying))
        {
            yield return null;
        }
    }

    public void PauseMovement()
    {
        if (!isInitialized || isPaused)
            return;

        isPaused = true;

        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMovement()
    {
        if (!isInitialized || !isPaused)
            return;

        isPaused = false;

        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    private void StopActivity()
    {
        StopAllCoroutines();

        isPaused = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);

        stopwatchScaleDuration = Mathf.Max(
            0f,
            stopwatchScaleDuration
        );
    }


    public void ResetCameraPos()
    {
        m_camera.position = Vector3.zero;
        m_cameraParent.position = Vector3.zero;
    }
}