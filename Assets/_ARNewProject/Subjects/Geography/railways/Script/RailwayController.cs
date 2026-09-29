using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class RailwayController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonVO
    {
        public Button button;
        public MaterialHighlighter[] highlighter;
        public AudioClip voiceOver;
        [HideInInspector] public bool isClicked = false;
    }
    [Header("TRAIN")]
    [SerializeField] private Transform trainRunning;
    [SerializeField] private float trainStopX = 13000f;
    [Header("SIGNAL LIGHT")]
    [SerializeField] private GameObject signalLightRed;
    [SerializeField] private GameObject signalLightYellow;
    [SerializeField] private GameObject signalLightGreen;
    [SerializeField] private float yellowLightDuration = 1.5f;
    [Header("LABEL BUTTONS + VO")]
    [SerializeField] private ButtonVO[] buttons;
    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip trainStartVO;
    [SerializeField] private AudioClip trainStopVO;
    [SerializeField] private AudioClip trainInfoVO;
    [SerializeField] private AudioClip labelsIntroVO;
    [SerializeField] private AudioClip finalVO;
    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;
    private bool isPaused;
    private bool finalVOPlayed;
    private Coroutine activityCoroutine;
    private Coroutine currentCoroutine;
    private Vector3 trainStartPosition;
    //private void Awake()
    //{
    //    if (trainRunning != null)
    //        trainStartPosition = trainRunning.position;
    //}
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
        SetSignal(true, false, false);
        if (!isActiveAndEnabled)
            return;
        StopAllCoroutines();
        if (audioSource != null)
            audioSource.Stop();
        isPaused = false;
        finalVOPlayed = false;
        activityCoroutine = null;
        currentCoroutine = null;
        ResetButtons();
        StopAllHighlights();
        HideLabels();
        activityCoroutine = StartCoroutine(ActivitySequence());
    }
    private IEnumerator ActivitySequence()
    {
        PlayAudio(intro3DVO);
        yield return StartCoroutine(WaitForVoiceOver());
        PlayAudio(setupVO);
        yield return StartCoroutine(WaitForVoiceOver());
        yield return StartCoroutine(TrainController());
        PlayAudio(trainInfoVO);
        yield return StartCoroutine(WaitForVoiceOver());
        ShowLabels();
        PlayAudio(labelsIntroVO);
        yield return StartCoroutine(WaitForVoiceOver());
        activityCoroutine = null;
    }

    private IEnumerator TrainController()
    {
        if (trainRunning == null)
            yield break;

        SetSignal(false, true, false);
        yield return StartCoroutine(WaitForActivitySeconds(yellowLightDuration));

        SetSignal(false, false, true);

        Vector3 startPos = trainRunning.localPosition;
        Vector3 endPos = new Vector3(trainStopX, startPos.y, startPos.z);

        float startDuration = trainStartVO != null ? trainStartVO.length : 0f;
        float stopDuration = trainStopVO != null ? trainStopVO.length : 0f;
        float totalDuration = startDuration + stopDuration;
        float elapsedTime = 0f;

        PlayAudio(trainStartVO);

        while (elapsedTime < startDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / totalDuration);

            Vector3 pos = trainRunning.localPosition;
            pos.x = Mathf.Lerp(startPos.x, endPos.x, t);
            pos.y = startPos.y;
            pos.z = startPos.z;

            trainRunning.localPosition = pos;

            yield return null;
        }

        PlayAudio(trainStopVO);

        while (elapsedTime < totalDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / totalDuration);

            Vector3 pos = trainRunning.localPosition;
            pos.x = Mathf.Lerp(startPos.x, endPos.x, t);
            pos.y = startPos.y;
            pos.z = startPos.z;

            trainRunning.localPosition = pos;

            yield return null;
        }

        trainRunning.localPosition = endPos;

        yield return StartCoroutine(WaitForVoiceOver());

        SetSignal(false, true, false);
        yield return StartCoroutine(WaitForActivitySeconds(yellowLightDuration));

        SetSignal(true, false, false);
    }

    private void SetSignal(bool red, bool yellow, bool green)
    {
        if (signalLightRed != null)
            signalLightRed.SetActive(red);
        if (signalLightYellow != null)
            signalLightYellow.SetActive(yellow);
        if (signalLightGreen != null)
            signalLightGreen.SetActive(green);
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
    private IEnumerator WaitForActivitySeconds(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
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