using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BauxiteController : MonoBehaviour
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
    [SerializeField] private Transform generatorCameraPoint;
    [SerializeField, Min(0f)] private float cameraMoveDuration = 1f;
    [SerializeField, Min(0f)] private float cameraReturnDuration = 1f;

    [Header("BAUXITE")]
    [SerializeField] private Transform bauxite;

    [Header("BAUXITE PATH")]
    [SerializeField] private Transform[] bauxitePathPoints;
    [SerializeField] private Transform bauxiteTargetPoint;
    [SerializeField, Min(0.1f)] private float bauxiteMoveDuration = 5f;
    [SerializeField, Min(0f)] private float bauxiteRotationTurns = 2f;
    [SerializeField] private Vector3 bauxiteRotationAxis = Vector3.up;
    [SerializeField] private Vector3 bauxiteAppearStartScale = Vector3.zero;

    [Header("LABEL BUTTONS + VO")]
    [SerializeField] private ButtonVO[] buttons;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip appearVO;
    [SerializeField] private AudioClip BauxiteVO;
    [SerializeField] private AudioClip labelsIntroVO;
    [SerializeField] private AudioClip finalVO;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private bool hasSavedCameraPose;

    private Vector3 bauxiteStartPosition;
    private Quaternion bauxiteStartRotation;
    private Vector3 bauxiteStartScale;

    private bool isPaused;
    private bool finalVOPlayed;

    private Coroutine activityCoroutine;
    private Coroutine currentCoroutine;

    private void Start()
    {
        if (bauxite != null)
        {
            bauxiteStartPosition = bauxite.position;
            bauxiteStartRotation = bauxite.rotation;
            bauxiteStartScale = bauxite.localScale;
        }

        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;

                if (buttons[i].button != null)
                    buttons[i].button.onClick.AddListener(() => OnButtonClicked(index));
            }
        }

        StartActivity();
    }

    public void StartActivity()
    {
        if (!isActiveAndEnabled)
            return;

        StopAllCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        if (m_camera != null)
        {
            savedCameraPosition = m_camera.position;
            savedCameraRotation = m_camera.rotation;
            hasSavedCameraPose = true;
        }

        if (bauxite != null)
        {
            bauxite.position = bauxiteStartPosition;
            bauxite.rotation = bauxiteStartRotation;
            bauxite.localScale = bauxiteAppearStartScale;
        }

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
        yield return StartCoroutine(MoveBauxiteThroughPath());
        yield return WaitForVoiceOver();

        PlayAudio(BauxiteVO);

        if (moveCameraToGenerator && m_camera != null && generatorCameraPoint != null)
            yield return MoveCamera(generatorCameraPoint.position, generatorCameraPoint.rotation, cameraMoveDuration);

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

    private IEnumerator MoveBauxiteThroughPath()
    {
        if (bauxite == null || bauxiteTargetPoint == null)
            yield break;

        List<Vector3> path = new List<Vector3>();

        path.Add(bauxiteStartPosition);

        if (bauxitePathPoints != null)
        {
            for (int i = 0; i < bauxitePathPoints.Length; i++)
            {
                if (bauxitePathPoints[i] != null)
                    path.Add(bauxitePathPoints[i].position);
            }
        }

        path.Add(bauxiteTargetPoint.position);

        Quaternion startRotation = bauxiteStartRotation;
        Quaternion targetRotation = bauxiteTargetPoint.rotation;

        Vector3 startScale = bauxiteAppearStartScale;
        Vector3 targetScale = bauxiteTargetPoint.localScale;

        Vector3 rotationAxis = bauxiteRotationAxis;

        if (rotationAxis.sqrMagnitude <= 0.001f)
            rotationAxis = Vector3.up;

        rotationAxis.Normalize();

        bauxite.position = bauxiteStartPosition;
        bauxite.rotation = startRotation;
        bauxite.localScale = startScale;

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, bauxiteMoveDuration);

        while (elapsed < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            bauxite.position = GetSmoothPathPosition(path, smoothT);

            bauxite.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                smoothT
            );

            Quaternion targetBlend = Quaternion.Slerp(
                startRotation,
                targetRotation,
                smoothT
            );

            float spinAngle = 360f * bauxiteRotationTurns * smoothT;

            Quaternion spinRotation = Quaternion.AngleAxis(
                spinAngle,
                rotationAxis
            );

            bauxite.rotation = targetBlend * spinRotation;

            yield return null;
        }

        bauxite.position = bauxiteTargetPoint.position;
        bauxite.rotation = bauxiteTargetPoint.rotation;
        bauxite.localScale = bauxiteTargetPoint.localScale;
    }

    private Vector3 GetSmoothPathPosition(List<Vector3> points, float t)
    {
        if (points == null || points.Count == 0)
            return bauxiteStartPosition;

        if (points.Count == 1)
            return points[0];

        if (points.Count == 2)
            return Vector3.Lerp(points[0], points[1], t);

        int segmentCount = points.Count - 1;

        float scaledT = t * segmentCount;

        int segment = Mathf.FloorToInt(scaledT);

        if (segment >= segmentCount)
            segment = segmentCount - 1;

        float localT = scaledT - segment;

        Vector3 p0 = points[Mathf.Max(segment - 1, 0)];
        Vector3 p1 = points[segment];
        Vector3 p2 = points[Mathf.Min(segment + 1, points.Count - 1)];
        Vector3 p3 = points[Mathf.Min(segment + 2, points.Count - 1)];

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f *
        (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
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

        PlayAudio(finalVO);
    }

    private void ResetButtons()
    {
        if (buttons == null)
            return;

        foreach (ButtonVO item in buttons)
            item.isClicked = false;
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

            m_camera.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetPosition, t),
                Quaternion.Slerp(startRotation, targetRotation, t)
            );

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

        if (audioSource != null &&
            audioSource.isActiveAndEnabled &&
            audioSource.clip != null)
        {
            audioSource.UnPause();
        }
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