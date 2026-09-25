using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AridSoilController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonVO
    {
        public Button button;
        public MaterialHighlighter[] highlighter;
        public AudioClip voiceOver;

        [HideInInspector] public bool isClicked = false;
    }

    [Header("3D CAMERA")]
    [SerializeField] private bool moveCameraToGenerator = true;
    [SerializeField] private Transform m_camera;

    [Tooltip("Camera position for viewing inside generator.")]
    [SerializeField] private Transform generatorCameraPoint;


    [Header("Label Buttons + VO")]
    public ButtonVO[] buttons;

    [SerializeField] private Transform aridSoil;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip appearVO;
    [SerializeField] private AudioClip aridSoilAppearVO;
    [SerializeField] private AudioClip miniExplanationVO;
    [SerializeField] private AudioClip labelsIntroVO;
    [SerializeField] private AudioClip finalVO;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    [SerializeField, Min(0f)]
    private float cameraMoveDuration = 1f;

    [SerializeField, Min(0f)]
    private float cameraReturnDuration = 1f;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private bool hasSavedCameraPose;

    private bool isPaused;
    private bool finalVOPlayed;
    private Coroutine activityCoroutine;
    private Coroutine currentCoroutine;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;

            if (buttons[i].button != null)
                buttons[i].button.onClick.AddListener(() => OnButtonClicked(index));
        }

        StartActivity();
    }

    public void StartActivity()
    {
        if (m_camera != null)
        {
            savedCameraPosition = m_camera.position;
            savedCameraRotation = m_camera.rotation;

            hasSavedCameraPose = true;
        }

        if (!isActiveAndEnabled)
            return;

        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;
        finalVOPlayed = false;
        currentCoroutine = null;

        ResetButtons();
        StopAllHighlights();
        HideLabels();

        activityCoroutine = StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        PlayAudio(intro3DVO);
        yield return WaitForVoiceOver();

        PlayAudio(setupVO);
        yield return WaitForVoiceOver();

        PlayAudio(appearVO);
        StartCoroutine(AridSoilAppear(-4.4f, 0f));
        yield return WaitForVoiceOver();

        PlayAudio(aridSoilAppearVO);
        yield return MoveCamera(generatorCameraPoint.position, generatorCameraPoint.rotation, cameraMoveDuration);
        yield return WaitForVoiceOver();

        PlayAudio(miniExplanationVO);
        yield return WaitForVoiceOver();

        if (hasSavedCameraPose && m_camera != null)
        {
            yield return MoveCamera(savedCameraPosition, savedCameraRotation, cameraReturnDuration);

            hasSavedCameraPose = false;
        }

        ShowLabels();

        PlayAudio(labelsIntroVO);
        yield return WaitForVoiceOver();

        activityCoroutine = null;
    }

    private IEnumerator AridSoilAppear(float startYPos, float endYPos)
    {
        if (aridSoil == null)
            yield break;

        float duration = appearVO != null ? appearVO.length : 1f;
        float elapsedTime = 0f;

        Vector3 currentPosition = aridSoil.localPosition;

        Vector3 startPosition = new Vector3(
            currentPosition.x,
            startYPos,
            currentPosition.z
        );

        Vector3 endPosition = new Vector3(
            currentPosition.x,
            endYPos,
            currentPosition.z
        );

        aridSoil.localPosition = startPosition;

        while (elapsedTime < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            aridSoil.localPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                t
            );

            yield return null;
        }

        aridSoil.localPosition = endPosition;
    }

    private void OnButtonClicked(int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Length)
            return;

        buttons[index].isClicked = true;

        if (buttons[index].button != null)
        {
            UIFadeIn fade = buttons[index].button.GetComponentInParent<UIFadeIn>();

            if (fade != null)
                fade.FadeOut();
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (audioSource != null)
            audioSource.Stop();

        StopAllHighlights();

        currentCoroutine = StartCoroutine(PlayButtonVO(index));
    }

    private IEnumerator PlayButtonVO(int index)
    {
        StartHighlight(index);

        if (audioSource != null && buttons[index].voiceOver != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = buttons[index].voiceOver;
            audioSource.Play();

            while (isPaused || audioSource.isPlaying)
                yield return null;
        }

        StopHighlight(index);

        currentCoroutine = null;

        CheckAllButtonsCompleted();
    }

    private void StartHighlight(int index)
    {
        if (buttons[index].highlighter == null)
            return;

        foreach (MaterialHighlighter highlighter in buttons[index].highlighter)
        {
            if (highlighter != null)
                highlighter.Highlight();
        }
    }

    private void StopHighlight(int index)
    {
        if (buttons[index].highlighter == null)
            return;

        foreach (MaterialHighlighter highlighter in buttons[index].highlighter)
        {
            if (highlighter != null)
                highlighter.StopHighlight();
        }
    }

    private void StopAllHighlights()
    {
        if (buttons == null)
            return;

        foreach (ButtonVO buttonVO in buttons)
        {
            if (buttonVO.highlighter == null)
                continue;

            foreach (MaterialHighlighter highlighter in buttonVO.highlighter)
            {
                if (highlighter != null)
                    highlighter.StopHighlight();
            }
        }
    }

    private void CheckAllButtonsCompleted()
    {
        if (finalVOPlayed || buttons == null || buttons.Length == 0)
            return;

        foreach (ButtonVO item in buttons)
        {
            if (!item.isClicked)
                return;
        }

        StartCoroutine(PlayFinalVO());
    }

    private IEnumerator PlayFinalVO()
    {
        finalVOPlayed = true;

        StopAllHighlights();

        while (isPaused || (audioSource != null && audioSource.isPlaying))
            yield return null;

        if (audioSource != null && finalVO != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = finalVO;
            audioSource.Play();
        }
    }

    private void ResetButtons()
    {
        if (buttons == null)
            return;

        foreach (ButtonVO item in buttons)
        {
            item.isClicked = false;
        }
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (m_camera == null)
            yield break;

        Vector3 startPosition = m_camera.position;
        Quaternion startRotation = m_camera.rotation;

        if (duration <= 0f)
        {
            m_camera.SetPositionAndRotation(targetPosition, targetRotation);

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

        m_camera.SetPositionAndRotation(targetPosition, targetRotation);
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

        while (isPaused || (audioSource != null && audioSource.isPlaying))
            yield return null;
    }

    public void PauseActivity()
    {
        if (!isActiveAndEnabled || isPaused)
            return;

        isPaused = true;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Pause();
    }

    public void ResumeActivity()
    {
        if (!isActiveAndEnabled || !isPaused)
            return;

        isPaused = false;

        if (audioSource != null && audioSource.isActiveAndEnabled && audioSource.clip != null)
            audioSource.UnPause();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        StopAllHighlights();

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;
        currentCoroutine = null;
        activityCoroutine = null;
    }
}