using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PetrolCanController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonVO
    {
        public Button button;
        public MaterialHighlighter[] highlighter;
        public AudioClip voiceOver;
        [HideInInspector] public bool isClicked = false;
    }

    [Header("3D OBJECT")]
    [SerializeField] private Transform petrolCan;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float moveDuration = 1f;

    [Header("PETROL CAN CAP")]
    [SerializeField] private Transform petrolCanCap;
    [SerializeField] private float capOpenY = 0.5f;
    [SerializeField] private float capMoveDuration = 0.7f;

    [Header("3D CAMERA")]
    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform generatorCameraPoint;
    [SerializeField, Min(0f)] private float cameraMoveDuration = 1f;
    [SerializeField, Min(0f)] private float cameraReturnDuration = 1f;

    [Header("PETROL CAN BODY MATERIAL")]
    [SerializeField] private Material boxMaterial;
    [Range(0f, 1f)][SerializeField] private float normalAlpha = 1f;
    [Range(0f, 1f)][SerializeField] private float transparentAlpha = 0.18f;
    [SerializeField] private float transparencyDuration = 1f;

    [Header("LABEL BUTTONS + VO")]
    [SerializeField] private ButtonVO[] buttons;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip petrolCanInfoVO;
    [SerializeField] private AudioClip labelsIntroVO;
    [SerializeField] private AudioClip finalVO;

    [Header("LABELS")]
    [SerializeField] private GameObject labels;

    private Vector3 savedCapLocalPosition;
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private bool hasSavedCameraPose;
    private bool isPaused;
    private bool finalVOPlayed;
    private Coroutine activityCoroutine;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (petrolCanCap != null)
            savedCapLocalPosition = petrolCanCap.localPosition;
    }

    private void Start()
    {
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
        StopAllCoroutines();
        activityCoroutine = null;
        currentCoroutine = null;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        isPaused = false;
        finalVOPlayed = false;
        if (petrolCanCap != null)
            petrolCanCap.localPosition = savedCapLocalPosition;
        SetMaterialAlpha(normalAlpha);
        ResetButtons();
        StopAllHighlights();
        HideLabels();
        activityCoroutine = StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        if (m_camera != null)
        {
            savedCameraPosition = m_camera.position;
            savedCameraRotation = m_camera.rotation;
            hasSavedCameraPose = true;
        }
        PlayAudio(intro3DVO);
        yield return StartCoroutine(WaitForVoiceOver());
        yield return StartCoroutine(MoveToTarget(petrolCan, targetPoint, moveDuration));
        PlayAudio(setupVO);
        yield return StartCoroutine(WaitForVoiceOver());
        PlayAudio(petrolCanInfoVO);
        yield return StartCoroutine(FadeMaterialAlpha(transparentAlpha, transparencyDuration));
        if (generatorCameraPoint != null)
            yield return StartCoroutine(MoveCamera(generatorCameraPoint.position, generatorCameraPoint.rotation, cameraMoveDuration));
        yield return StartCoroutine(WaitForVoiceOver());
        yield return StartCoroutine(FadeMaterialAlpha(normalAlpha, transparencyDuration));
        if (hasSavedCameraPose)
            yield return StartCoroutine(MoveCamera(savedCameraPosition, savedCameraRotation, cameraReturnDuration));
        ShowLabels();
        PlayAudio(labelsIntroVO);
        yield return StartCoroutine(OpenCapOnlyY());
        yield return StartCoroutine(FadeMaterialAlpha(transparentAlpha, transparencyDuration));
        yield return StartCoroutine(WaitForVoiceOver());
        activityCoroutine = null;
    }

    private IEnumerator MoveToTarget(Transform obj, Transform target, float duration)
    {
        if (obj == null || target == null)
            yield break;
        Vector3 startPosition = obj.position;
        Quaternion startRotation = obj.rotation;
        Vector3 startScale = obj.localScale;
        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;
        Vector3 targetScale = target.localScale;
        if (duration <= 0f)
        {
            obj.position = targetPosition;
            obj.rotation = targetRotation;
            obj.localScale = targetScale;
            yield break;
        }
        float elapsedTime = 0f;
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
            obj.position = Vector3.Lerp(startPosition, targetPosition, t);
            obj.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            obj.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        obj.position = targetPosition;
        obj.rotation = targetRotation;
        obj.localScale = targetScale;
    }

    private IEnumerator OpenCapOnlyY()
    {
        if (petrolCanCap == null)
            yield break;
        float startY = petrolCanCap.position.y;
        float elapsed = 0f;
        if (capMoveDuration <= 0f)
        {
            Vector3 pos = petrolCanCap.position;
            pos.y = capOpenY;
            petrolCanCap.position = pos;
            yield break;
        }
        while (elapsed < capMoveDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / capMoveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = petrolCanCap.position;
            pos.y = Mathf.Lerp(startY, capOpenY, t);
            petrolCanCap.position = pos;
            yield return null;
        }
        Vector3 finalPosition = petrolCanCap.position;
        finalPosition.y = capOpenY;
        petrolCanCap.position = finalPosition;
    }

    private IEnumerator CloseCapOnlyY()
    {
        if (petrolCanCap == null)
            yield break;
        float startLocalY = petrolCanCap.localPosition.y;
        float targetLocalY = savedCapLocalPosition.y;
        float elapsed = 0f;
        if (capMoveDuration <= 0f)
        {
            Vector3 pos = petrolCanCap.localPosition;
            pos.y = targetLocalY;
            petrolCanCap.localPosition = pos;
            yield break;
        }
        while (elapsed < capMoveDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / capMoveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = petrolCanCap.localPosition;
            pos.y = Mathf.Lerp(startLocalY, targetLocalY, t);
            petrolCanCap.localPosition = pos;
            yield return null;
        }
        Vector3 finalPosition = petrolCanCap.localPosition;
        finalPosition.y = targetLocalY;
        petrolCanCap.localPosition = finalPosition;
    }

    private IEnumerator FadeMaterialAlpha(float targetAlpha, float duration)
    {
        if (boxMaterial == null)
            yield break;
        Color startColor = boxMaterial.color;
        float startAlpha = startColor.a;
        if (duration <= 0f)
        {
            SetMaterialAlpha(targetAlpha);
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);
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

    private void OnButtonClicked(int index)
    {
        if (isPaused)
            return;
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
        yield return StartCoroutine(CloseCapOnlyY());
        yield return StartCoroutine(FadeMaterialAlpha(normalAlpha, transparencyDuration));
        PlayAudio(finalVO);
        yield return StartCoroutine(WaitForVoiceOver());
    }

    private void ResetButtons()
    {
        if (buttons == null)
            return;
        foreach (ButtonVO item in buttons)
            item.isClicked = false;
    }

    public void ShowLabels()
    {
        if (labels != null)
            labels.SetActive(true);
    }

    public void HideLabels()
    {
        if (labels != null)
            labels.SetActive(false);
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
}