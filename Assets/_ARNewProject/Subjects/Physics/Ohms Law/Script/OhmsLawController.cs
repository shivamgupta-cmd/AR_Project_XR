using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OhmsLawController : MonoBehaviour
{
    [Header("CURRENT FLOW")]
    [SerializeField] private CurrentFlowPath[] flowPath;

    [SerializeField] private ObjectHighlighter ObjectHighlighter;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip circuitSetupVO;
    [SerializeField] private AudioClip switchVO;
    [SerializeField] private AudioClip switchClickVO;
    [SerializeField] private AudioClip currentFlowVO;
    [SerializeField] private AudioClip nowUseSliderVO;
    [SerializeField] private AudioClip formulaVO;
    [SerializeField] private AudioClip labelsIntroVO;

    [Header("3D CAMERA")]
    [SerializeField] private bool moveCameraToCircuit = true;
    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform circuitCameraPoint;
    [SerializeField, Min(0f)] private float cameraMoveDuration = 1f;
    [SerializeField, Min(0f)] private float cameraReturnDuration = 1f;

    [Header("Switch Button")]
    [SerializeField] private Transform m_switchButton;
    [SerializeField] private GameObject m_bulbLight;
    [SerializeField] private GameObject m_switchClickLable;

    private BoxCollider switchCollider;
    private Quaternion initialSwitchRotation;

    [Header("FORMULA PANEL")]
    [SerializeField] private GameObject m_formulaPanel;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    [Header("OHMS LAW SLIDER")]
    [SerializeField] private Slider voltageSlider;
    [SerializeField] private GameObject sliderBg;
    [SerializeField] private float minVoltage = 2f;
    [SerializeField] private float maxVoltage = 12f;
    [SerializeField] private float defaultVoltage = 6f;
    [SerializeField] private float resistance = 6f;

    [Header("DC VOLTS SELECTOR")]
    [SerializeField] private Transform dcVoltsSelector;
    [SerializeField] private Vector3 dcSelectorRotationAxis = Vector3.up;
    [SerializeField] private float dcSelectorMinAngle = 0f;
    [SerializeField] private float dcSelectorMaxAngle = 180f;

    [Header("VOLTMETER")]
    [SerializeField] private Transform voltmeterNeedle;
    [SerializeField] private Vector3 voltmeterRotationAxis = Vector3.forward;
    [SerializeField] private float voltmeterMinAngle = 45f;
    [SerializeField] private float voltmeterMaxAngle = -45f;

    [Header("AMMETER")]
    [SerializeField] private Transform ammeterNeedle;
    [SerializeField] private Vector3 ammeterRotationAxis = Vector3.forward;
    [SerializeField] private float ammeterMinAngle = 45f;
    [SerializeField] private float ammeterMaxAngle = -45f;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private Quaternion dcSelectorBaseRotation;
    private Quaternion voltmeterBaseRotation;
    private Quaternion ammeterBaseRotation;

    private Coroutine activityCoroutine;

    private bool switchIsOn;
    private bool hasSavedCameraPose;
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

        if (dcVoltsSelector != null)
            dcSelectorBaseRotation = dcVoltsSelector.localRotation;

        if (voltmeterNeedle != null)
            voltmeterBaseRotation = voltmeterNeedle.localRotation;

        if (ammeterNeedle != null)
            ammeterBaseRotation = ammeterNeedle.localRotation;

        if (m_bulbLight != null)
        {
            m_bulbLight.SetActive(false);
        }

        if (m_switchClickLable != null)
        {
            m_switchClickLable.SetActive(false);
        }

        if (switchCollider != null)
        {
            switchCollider.enabled = false;
        }
        if (sliderBg != null)
        {
            sliderBg.SetActive(false);
        }
    }

    private void Start()
    {
        SetupSlider();
        StartActivity();
    }

    private void SetupSlider()
    {
        if (voltageSlider == null)
            return;

        voltageSlider.minValue = minVoltage;
        voltageSlider.maxValue = maxVoltage;
        voltageSlider.wholeNumbers = true;

        voltageSlider.onValueChanged.RemoveListener(OnVoltageChanged);
        voltageSlider.onValueChanged.AddListener(OnVoltageChanged);

        voltageSlider.SetValueWithoutNotify(defaultVoltage);

        //OnVoltageChanged(defaultVoltage);
    }

    private void OnVoltageChanged(float voltage)
    {
        if (resistance <= 0f)
            return;

        voltage = Mathf.Clamp(
            voltage,
            minVoltage,
            maxVoltage
        );

        float current = voltage / resistance;

        UpdateDCVoltsSelector(voltage);
        UpdateVoltmeter(voltage);
        UpdateAmmeter(current);
    }

    private void UpdateDCVoltsSelector(float voltage)
    {
        if (dcVoltsSelector == null)
            return;

        float t = Mathf.InverseLerp(
            minVoltage,
            maxVoltage,
            voltage
        );

        float angle = Mathf.Lerp(
            dcSelectorMinAngle,
            dcSelectorMaxAngle,
            t
        );

        dcVoltsSelector.localRotation =
            dcSelectorBaseRotation *
            Quaternion.AngleAxis(
                angle,
                dcSelectorRotationAxis.normalized
            );
    }

    private void UpdateVoltmeter(float voltage)
    {
        if (voltmeterNeedle == null)
            return;

        float t = Mathf.InverseLerp(
            minVoltage,
            maxVoltage,
            voltage
        );

        float angle = Mathf.Lerp(
            voltmeterMinAngle,
            voltmeterMaxAngle,
            t
        );

        voltmeterNeedle.localRotation =
            voltmeterBaseRotation *
            Quaternion.AngleAxis(
                angle,
                voltmeterRotationAxis.normalized
            );
    }

    private void UpdateAmmeter(float current)
    {
        if (ammeterNeedle == null)
            return;

        float minCurrent = minVoltage / resistance;
        float maxCurrent = maxVoltage / resistance;

        float t = Mathf.InverseLerp(
            minCurrent,
            maxCurrent,
            current
        );

        float angle = Mathf.Lerp(
            ammeterMinAngle,
            ammeterMaxAngle,
            t
        );

        ammeterNeedle.localRotation =
            ammeterBaseRotation *
            Quaternion.AngleAxis(
                angle,
                ammeterRotationAxis.normalized
            );
    }

    public void SetVoltage(float voltage)
    {
        voltage = Mathf.Clamp(voltage, minVoltage, maxVoltage);

        if (voltageSlider != null)
        {
            voltageSlider.SetValueWithoutNotify(voltage);
        }

        OnVoltageChanged(voltage);
    }

    public void SetDefaultVoltage()
    {
        SetVoltage(defaultVoltage);
    }

    public void StartActivity()
    {
        if (!isActiveAndEnabled)
            return;

        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;

        if (hasSavedCameraPose && m_camera != null)
        {
            m_camera.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation
            );
        }

        hasSavedCameraPose = false;

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);

        HideLabels();

        StopCurrentFlow();

        StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        if (m_camera != null)
        {
            savedCameraPosition =
                m_camera.position;

            savedCameraRotation =
                m_camera.rotation;

            hasSavedCameraPose = true;
        }

        PlayAudio(intro3DVO);

        yield return WaitForVoiceOver();

        PlayAudio(circuitSetupVO);

        yield return WaitForVoiceOver();

        if (moveCameraToCircuit && m_camera != null && circuitCameraPoint != null)
        {
            yield return MoveCamera(circuitCameraPoint.position, circuitCameraPoint.rotation, cameraMoveDuration);
        }

        PlayAudio(switchVO);
        yield return WaitForVoiceOver();
        if (ObjectHighlighter != null)
        {
            ObjectHighlighter.StartHighlight();

            if (m_switchClickLable != null)
            {
                m_switchClickLable.SetActive(true);
            }

            if (switchCollider != null)
            {
                switchCollider.enabled = true;
            }
        }
    }

    public void SwitchClicked()
    {
        PlayAudio(switchClickVO);
        if (switchIsOn)
            return;

        switchIsOn = true;

        if (switchCollider != null)
        {
            switchCollider.enabled = false;
        }

        if (ObjectHighlighter != null)
        {
            ObjectHighlighter.StopHighlight();
        }

        if (m_switchButton != null)
        {
            Vector3 angles = m_switchButton.localEulerAngles;
            angles.x = 40f;

            m_switchButton.localRotation = Quaternion.Euler(angles);
        }
        if (m_bulbLight != null)
        {
            m_bulbLight.SetActive(true);
        }

        if (m_switchClickLable != null)
        {
            m_switchClickLable.SetActive(false);
        }

        if (activityCoroutine != null)
            StopCoroutine(activityCoroutine);

        activityCoroutine = StartCoroutine(SwitchSequence());
    }

    private IEnumerator SwitchSequence()
    {
        if (moveCameraToCircuit && hasSavedCameraPose && m_camera != null)
        {
            yield return MoveCamera(
                savedCameraPosition,
                savedCameraRotation,
                cameraReturnDuration
            );
        }

        hasSavedCameraPose = false;

        SetDefaultVoltage();

        StartCurrentFlow();

        yield return WaitForVoiceOver();
        PlayAudio(currentFlowVO);

        yield return WaitForVoiceOver();

        PlayAudio(nowUseSliderVO);
        if (sliderBg != null)
        {
            sliderBg.SetActive(true);
        }
        yield return WaitForVoiceOver();
        PlayAudio(formulaVO);

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(true);

        yield return WaitForVoiceOver();

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);

        ShowLabels();

        PlayAudio(labelsIntroVO);

        yield return WaitForVoiceOver();
        
    }

    public void StartCurrentFlow()
    {
        if (flowPath == null)
            return;

        foreach (CurrentFlowPath path in flowPath)
        {
            if (path != null)
            {
                path.enabled = true;
            }
        }
    }

    public void StopCurrentFlow()
    {
        if (flowPath == null)
            return;

        foreach (CurrentFlowPath path in flowPath)
        {
            if (path != null)
            {
                path.enabled = false;
            }
        }
    }

    private IEnumerator MoveCamera(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration)
    {
        if (m_camera == null)
            yield break;

        Vector3 startPosition =
            m_camera.position;

        Quaternion startRotation =
            m_camera.rotation;

        if (duration <= 0f)
        {
            m_camera.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (m_camera == null)
                yield break;

            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            m_camera.SetPositionAndRotation(
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                ),
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                )
            );

            yield return null;
        }

        m_camera.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );
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

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null ||
            clip == null)
            return;

        audioSource.Stop();

        audioSource.loop = false;

        audioSource.clip = clip;

        audioSource.Play();
    }

    private IEnumerator WaitForVoiceOver()
    {
        yield return null;

        while (
            isPaused ||
            (audioSource != null &&
             audioSource.isPlaying)
        )
        {
            yield return null;
        }
    }

    public void PauseActivity()
    {
        if (!isActiveAndEnabled ||
            isPaused)
            return;

        isPaused = true;

        wasFormulaPanelActive =
            m_formulaPanel != null &&
            m_formulaPanel.activeSelf;

        if (audioSource != null)
            audioSource.Pause();

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);
    }

    public void ResumeActivity()
    {
        if (!isActiveAndEnabled ||
            !isPaused)
            return;

        if (audioSource == null ||
            !audioSource.isActiveAndEnabled)
            return;

        if (m_formulaPanel != null)
        {
            m_formulaPanel.SetActive(
                wasFormulaPanelActive
            );
        }

        audioSource.UnPause();

        wasFormulaPanelActive = false;

        isPaused = false;
    }

    public void ResetActivity()
    {
        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;

        StopCurrentFlow();

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);

        HideLabels();

        SetDefaultVoltage();

        if (hasSavedCameraPose &&
            m_camera != null)
        {
            m_camera.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation
            );
        }

        hasSavedCameraPose = false;
    }

    private void OnDestroy()
    {
        if (voltageSlider != null)
        {
            voltageSlider.onValueChanged.RemoveListener(
                OnVoltageChanged
            );
        }
    }
}