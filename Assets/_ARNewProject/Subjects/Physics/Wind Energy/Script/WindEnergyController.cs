using System.Collections;
using UnityEngine;

public class WindEnergyController : MonoBehaviour
{
    public CurrentFlowPath flowPath;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip introVO;
    [SerializeField] private AudioClip windAppearsVO;
    [SerializeField] private AudioClip bladeRotationVO;
    [SerializeField] private AudioClip lookInsideVO;
    [SerializeField] private AudioClip generatorVO;
    [SerializeField] private AudioClip electricityVO;
    [SerializeField] private AudioClip finalVO;
    [SerializeField] private AudioClip clickAllLableVo;


    [Header("WIND TURBINE ROTATE PARTS")]
    [Tooltip("All parts that should rotate with the turbine blades.")]
    [SerializeField] private Transform[] turbinePart;

    [Tooltip("Rotation speed around local Z axis.")]
    [SerializeField] private float turbineRotationSpeed = 80f;

    [Tooltip("How smoothly the turbine accelerates/decelerates.")]
    [SerializeField] private float turbineAcceleration = 40f;

    [SerializeField] private bool turbineRotating = false;

    private float currentTurbineSpeed = 0f;



    [Header("WIND GENERATOR BOX")]
    [SerializeField] private Material boxMaterial;

    [Tooltip("Alpha when generator box is normal.")]
    [Range(0f, 1f)]
    [SerializeField] private float normalAlpha = 1f;

    [Tooltip("Alpha when looking inside generator.")]
    [Range(0f, 1f)]
    [SerializeField] private float transparentAlpha = 0.18f;

    [Tooltip("Time required to fade material.")]
    [SerializeField] private float transparencyDuration = 1f;


    [Header("3D CAMERA")]
    [SerializeField] private bool moveCameraToGenerator = true;
    [SerializeField] private Transform m_camera;

    [Tooltip("Camera position for viewing inside generator.")]
    [SerializeField] private Transform generatorCameraPoint;

    [SerializeField] private Transform startGeneratorCameraPoint;

    [SerializeField, Min(0f)]
    private float cameraMoveDuration = 1f;

    [SerializeField, Min(0f)]
    private float cameraReturnDuration = 1f;


    [Header("FORMULA PANEL")]
    [SerializeField] private GameObject m_formulaPanel;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;


    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private bool hasSavedCameraPose;
    private bool isPaused;
    private bool wasFormulaPanelActive;


    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.Stop();
        }

        SetMaterialAlpha(normalAlpha);
    }

    private void Start()
    {
        StartActivity();
    }

    private void Update()
    {
        UpdateTurbineRotation();
    }


    public void StartActivity()
    {
        if (!isActiveAndEnabled)
            return;

        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;

        turbineRotating = true;
        currentTurbineSpeed = 0f;

        if (hasSavedCameraPose && m_camera != null)
        {
            m_camera.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation
            );
        }

        hasSavedCameraPose = false;

        SetMaterialAlpha(normalAlpha);

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);

        if (flowPath != null)
        {
            flowPath.enabled = false;
        }

        HideLabels();

        StartCoroutine(ActivitySequence());
    }


    private IEnumerator ActivitySequence()
    {
        if (m_camera != null)
        {
            savedCameraPosition = m_camera.position;
            savedCameraRotation = m_camera.rotation;

            hasSavedCameraPose = true;
        }

        PlayAudio(introVO);
        yield return MoveCamera(
               startGeneratorCameraPoint.position,
               startGeneratorCameraPoint.rotation,
               cameraMoveDuration
        );

        yield return WaitForVoiceOver();


        PlayAudio(windAppearsVO);

        yield return WaitForVoiceOver();


        StartTurbine();

        PlayAudio(bladeRotationVO);

        yield return WaitForVoiceOver();


        PlayAudio(lookInsideVO);

        yield return MoveCamera(
            generatorCameraPoint.position,
            generatorCameraPoint.rotation,
            cameraMoveDuration
        );

        yield return FadeMaterialAlpha(
            transparentAlpha,
            transparencyDuration
        );

        yield return WaitForVoiceOver();

        PlayAudio(generatorVO);

        yield return WaitForVoiceOver();


        yield return FadeMaterialAlpha(
            normalAlpha,
            transparencyDuration
        );


        if (hasSavedCameraPose && m_camera != null)
        {
            yield return MoveCamera(
                savedCameraPosition,
                savedCameraRotation,
                cameraReturnDuration
            );

            hasSavedCameraPose = false;
        }


        PlayAudio(electricityVO);

        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(true);

        yield return WaitForVoiceOver();


        if (m_formulaPanel != null)
            m_formulaPanel.SetActive(false);




        PlayAudio(finalVO);
        if (flowPath != null)
        {
            flowPath.enabled = true;
        }

        yield return WaitForVoiceOver();
        PlayAudio(clickAllLableVo);
        turbineRotating = false;
        yield return FadeMaterialAlpha(transparentAlpha, transparencyDuration);
        ShowLabels();
    }


    private void UpdateTurbineRotation()
    {
        float targetSpeed = turbineRotating ? turbineRotationSpeed : 0f;

        currentTurbineSpeed = Mathf.MoveTowards(currentTurbineSpeed, targetSpeed, turbineAcceleration * Time.deltaTime);

        if (Mathf.Abs(currentTurbineSpeed) <= 0.001f)
            return;

        if (turbinePart == null)
            return;

        foreach (Transform part in turbinePart)
        {
            if (part == null)
                continue;

            part.Rotate(0f, 0f, currentTurbineSpeed * Time.deltaTime, Space.Self);
        }
    }


    public void StartTurbine()
    {
        turbineRotating = true;
    }

    public void StopTurbine()
    {
        turbineRotating = false;
    }

    public void SetTurbineSpeed(float speed)
    {
        turbineRotationSpeed = speed;

        if (speed > 0f)
            turbineRotating = true;
        else
            turbineRotating = false;
    }


    public void SetWindSpeed(float value)
    {
        value = Mathf.Clamp01(value);

        turbineRotationSpeed = Mathf.Lerp(0f, 200f, value);

        turbineRotating = value > 0.01f;
    }


    private IEnumerator FadeMaterialAlpha(
        float targetAlpha,
        float duration)
    {
        if (boxMaterial == null)
            yield break;

        Color startColor = boxMaterial.color;

        float startAlpha = startColor.a;

        float elapsed = 0f;

        if (duration <= 0f)
        {
            SetMaterialAlpha(targetAlpha);
            yield break;
        }

        while (elapsed < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            t = Mathf.SmoothStep(0f, 1f,t);

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            SetMaterialAlpha(alpha);

            yield return null;
        }

        SetMaterialAlpha(targetAlpha);
    }


    private void SetMaterialAlpha(float alpha)
    {
        if (boxMaterial == null)
            return;

        Color color = boxMaterial.color;

        color.a = Mathf.Clamp01(alpha);

        boxMaterial.color = color;
    }


    public void SetGeneratorTransparency(float alpha)
    {
        SetMaterialAlpha(alpha);
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

            float t = Mathf.Clamp01(elapsed / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            m_camera.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, t), Quaternion.Slerp(startRotation, targetRotation, t));

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

        while (
            isPaused ||
            (audioSource != null &&
             audioSource.isPlaying))
        {
            yield return null;
        }
    }

    public void PauseActivity()
    {
        if (!isActiveAndEnabled || isPaused)
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
        if (!isActiveAndEnabled || !isPaused)
            return;

        if (audioSource == null ||
            !audioSource.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "Keep the narration AudioSource active before resuming.",
                this
            );

            return;
        }

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

    private void OnDisable()
    {
        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        turbineRotating = false;
        currentTurbineSpeed = 0f;
    }
}