using System.Collections;
using UnityEngine;

public class ElectricalSwitchManager : MonoBehaviour
{
    public CurrentFlowPath flowPath;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    [SerializeField] private ObjectHighlighter ObjectHighlighter;
    [SerializeField] private ObjectHighlighter switchRedLightHighlighter;

    [Header("3D CAMERA")]
    [SerializeField] private bool moveCameraToSwitch = true;
    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform switchCameraPoint;
    [SerializeField, Min(0f)] private float cameraMoveDuration = 1f;
    [SerializeField, Min(0f)] private float cameraReturnDuration = 1f;

    [Header("Switch Button")]
    [SerializeField] private Transform m_switchButton;
    [SerializeField] private GameObject m_bulbLight;
    [SerializeField] private GameObject m_switchClickLable;


    [Header("Formula Panel")]
    [SerializeField] private GameObject m_formulaPanel;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    private BoxCollider switchCollider;
    private Quaternion initialSwitchRotation;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private bool hasSavedCameraPose;
    private bool canClickSwitch;
    private bool isPaused;
    private bool wasFormulaPanelActive;

    private void Awake()
    {
        if (m_switchButton != null)
        {
            switchCollider = m_switchButton.GetComponent<BoxCollider>();
            initialSwitchRotation = m_switchButton.localRotation;
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    private void Start()
    {
        StartActivity();
    }

    public void StartActivity()
    {
        if (!isActiveAndEnabled)
            return;

        if (audioSource == null ||
            !audioSource.isActiveAndEnabled ||
            m_switchButton == null ||
            switchCollider == null ||
            audioClips == null ||
            audioClips.Length < 7)
        {
            Debug.LogError(
                "Assign an active AudioSource, Switch Button with a BoxCollider, " +
                "and Audio Clips 0 to 6.",
                this
            );

            return;
        }

        for (int i = 0; i < 7; i++)
        {
            if (audioClips[i] == null)
            {
                Debug.LogError(
                    $"Assign Audio Clips element {i}.",
                    this
                );

                return;
            }
        }

        if (moveCameraToSwitch && (m_camera == null || switchCameraPoint == null))
        {
            Debug.LogError(
                "Assign the 3D Camera and Switch Camera Point.",
                this
            );

            return;
        }

        StopAllCoroutines();
        audioSource.Stop();

        isPaused = false;
        canClickSwitch = false;

        if (hasSavedCameraPose && m_camera != null)
        {
            m_camera.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation
            );
        }

        hasSavedCameraPose = false;

        m_switchButton.localRotation = initialSwitchRotation;
        switchCollider.enabled = false;

        if (ObjectHighlighter != null)
        {
            ObjectHighlighter.StopHighlight();
        }

        if (switchRedLightHighlighter != null)
        {
            switchRedLightHighlighter.StopHighlight();
        }

        if (m_bulbLight != null)
        {
            m_bulbLight.SetActive(false);
        }

        if (m_switchClickLable != null)
        {
            m_switchClickLable.SetActive(false);
        }
        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(false);
        }

        if (flowPath != null)
        {
            flowPath.enabled = false;
        }

        if (labels != null)
        {
            foreach (GameObject label in labels)
            {
                if (label != null)
                {
                    label.SetActive(false);
                }
            }
        }

        StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        PlayAudio(audioClips[0]);
        yield return WaitForVoiceOver();

        PlayAudio(audioClips[1]);
        yield return WaitForVoiceOver();

        PlayAudio(audioClips[2]);

        if (moveCameraToSwitch &&
            m_camera != null &&
            switchCameraPoint != null)
        {
            savedCameraPosition = m_camera.position;
            savedCameraRotation = m_camera.rotation;

            hasSavedCameraPose = true;

            yield return MoveCamera(
                switchCameraPoint.position,
                switchCameraPoint.rotation,
                cameraMoveDuration
            );
        }

        yield return WaitForVoiceOver();

        if (ObjectHighlighter != null)
        {
            ObjectHighlighter.StartHighlight();
        }

        if (switchCollider != null)
        {
            switchCollider.enabled = true;
        }

        if (m_switchClickLable != null)
        {
            m_switchClickLable.SetActive(true);
        }

        canClickSwitch = true;
    }

    public void ClickSwitchBtn()
    {
        if (!isActiveAndEnabled || isPaused || !canClickSwitch)
            return;

        canClickSwitch = false;

        if (ObjectHighlighter != null)
        {
            ObjectHighlighter.StopHighlight();
        }

        if (switchCollider != null)
        {
            switchCollider.enabled = false;
        }

        if (m_switchButton != null)
        {
            Vector3 angles = m_switchButton.localEulerAngles;
            angles.x = 10f;

            m_switchButton.localRotation = Quaternion.Euler(angles);
        }

        if (m_switchClickLable != null)
        {
            m_switchClickLable.SetActive(false);
        }

        if (switchRedLightHighlighter != null)
        {
            switchRedLightHighlighter.StartHighlight();
        }

        StartCoroutine(ActiveSwitchButton());
    }

    private IEnumerator ActiveSwitchButton()
    {
        PlayAudio(audioClips[3]);

        if (hasSavedCameraPose && m_camera != null)
        {
            yield return MoveCamera(
                savedCameraPosition,
                savedCameraRotation,
                cameraReturnDuration
            );

            hasSavedCameraPose = false;
        }

        yield return WaitForVoiceOver();

        PlayAudio(audioClips[4]);

        if (m_bulbLight != null)
        {
            m_bulbLight.SetActive(true);
        }

        if (flowPath != null)
        {
            flowPath.enabled = true;
        }
        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(true);
        }

        yield return WaitForVoiceOver();

        PlayAudio(audioClips[5]);
        yield return WaitForVoiceOver();
        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(false);
        }
        ShowLabels();
        PlayAudio(audioClips[6]);
    }

    private IEnumerator MoveCamera(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration)
    {
        if (m_camera == null)
            yield break;

        Vector3 startPosition = m_camera.position;
        Quaternion startRotation = m_camera.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;

            if (m_camera == null)
                yield break;

            if (isPaused)
                continue;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            m_camera.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, t),
                Quaternion.Slerp(startRotation, targetRotation, t)
            );
        }

        if (m_camera != null)
        {
            m_camera.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );
        }
    }

    public void ShowLabels()
    {
        if (labels == null)
            return;

        foreach (GameObject label in labels)
        {
            if (label != null)
            {
                label.SetActive(true);
            }
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

    private void OnDisable()
    {
        StopAllCoroutines();

        canClickSwitch = false;

        if (switchCollider != null)
        {
            switchCollider.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    //public void PauseActivity()
    //{
    //    if (!isActiveAndEnabled || isPaused)
    //        return;

    //    isPaused = true;

    //    if (audioSource != null)
    //    {
    //        audioSource.Pause();
    //    }

    //    if (switchCollider != null)
    //    {
    //        switchCollider.enabled = false;
    //    }

    //    if (m_formulaPanel != null && m_formulaPanel.activeSelf)
    //    {
    //        m_formulaPanel.SetActive(false);
    //    }
    //}

    //public void ResumeActivity()
    //{
    //    if (!isActiveAndEnabled || !isPaused)
    //        return;

    //    if (audioSource == null || !audioSource.isActiveAndEnabled)
    //    {
    //        Debug.LogWarning(
    //            "Keep the narration AudioSource active before resuming.",
    //            this
    //        );

    //        return;
    //    }

    //    audioSource.UnPause();

    //    if (switchCollider != null)
    //    {
    //        switchCollider.enabled = canClickSwitch;
    //    }

    //    if (m_formulaPanel != null && m_formulaPanel.activeSelf)
    //    {
    //        m_formulaPanel.SetActive(true);
    //    }

    //    isPaused = false;
    //}

    public void PauseActivity()
    {
        if (!isActiveAndEnabled || isPaused)
            return;

        isPaused = true;

        wasFormulaPanelActive =
            m_formulaPanel != null && m_formulaPanel.activeSelf;

        if (audioSource != null)
        {
            audioSource.Pause();
        }

        if (switchCollider != null)
        {
            switchCollider.enabled = false;
        }

        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(false);
        }
    }

    public void ResumeActivity()
    {
        if (!isActiveAndEnabled || !isPaused)
            return;

        if (audioSource == null || !audioSource.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "Keep the narration AudioSource active before resuming.",
                this
            );

            return;
        }

        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(wasFormulaPanelActive);
        }

        audioSource.UnPause();

        if (switchCollider != null)
        {
            switchCollider.enabled = canClickSwitch;
        }

        wasFormulaPanelActive = false;
        isPaused = false;
    }

}