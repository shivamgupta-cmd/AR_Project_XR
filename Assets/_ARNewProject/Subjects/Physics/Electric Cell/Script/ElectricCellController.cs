using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ElectricCellController : MonoBehaviour
{
    [Header("CELL OBJECTS")]
    [SerializeField] private GameObject fullCell;
    [SerializeField] private GameObject cutawayCell;

    [Header("FULL CELL ROTATION")]
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("CUTAWAY SCALE")]
    [SerializeField] private float cutawayAppearDuration = 0.7f;

    [Header("PART HIGHLIGHTERS")]
    [SerializeField] private ObjectHighlighter positiveTerminalHighlighter;
    [SerializeField] private ObjectHighlighter insulatingSealHighlighter;
    [SerializeField] private ObjectHighlighter carbonRodHighlighter;
    [SerializeField] private ObjectHighlighter electrolyteHighlighter;
    [SerializeField] private ObjectHighlighter separatorHighlighter;
    [SerializeField] private ObjectHighlighter zincContainerHighlighter;
    [SerializeField] private ObjectHighlighter negativeTerminalHighlighter;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip intro3DVO;
    [SerializeField] private AudioClip setupVO;
    [SerializeField] private AudioClip cellTwoPartsVO;
    [SerializeField] private AudioClip cellExternalPartVO;
    [SerializeField] private AudioClip labelsIntroVO;

    [Header("Lable VO")]
    [SerializeField] private AudioClip positiveTerminalVO;
    [SerializeField] private AudioClip insulatingSealVO;
    [SerializeField] private AudioClip carbonRodVO;
    [SerializeField] private AudioClip electrolyteVO;
    [SerializeField] private AudioClip separatorVO;
    [SerializeField] private AudioClip zincContainerVO;
    [SerializeField] private AudioClip negativeTerminalVO;

    [Header("Button")]
    [SerializeField] private Button positiveTerminalBtn;
    [SerializeField] private Button insulatingSealBtn;
    [SerializeField] private Button carbonRodBtn;
    [SerializeField] private Button electrolyteBtn;
    [SerializeField] private Button separatorBtn;
    [SerializeField] private Button zincContainerBtn;
    [SerializeField] private Button negativeTerminalBtn;

    [Header("LABELS")]
    [SerializeField] private GameObject[] labels;

    private bool isPaused;
    private bool rotateFullCell;

    private Vector3 cutawayOriginalScale;

    private Coroutine activityCoroutine;
    private Coroutine labelVOCoroutine;
    private ObjectHighlighter activeHighlighter;

    private void Awake()
    {
        if (cutawayCell != null)
            cutawayOriginalScale = cutawayCell.transform.localScale;
    }

    private void Start()
    {
        SetupButtonListeners();
        StartActivity();
    }






    private void SetupButtonListeners()
    {
        if (positiveTerminalBtn != null)
            positiveTerminalBtn.onClick.AddListener(() =>
                PlayLabelVO(positiveTerminalVO, positiveTerminalHighlighter, positiveTerminalBtn));

        if (insulatingSealBtn != null)
            insulatingSealBtn.onClick.AddListener(() =>
                PlayLabelVO(insulatingSealVO, insulatingSealHighlighter, insulatingSealBtn));

        if (carbonRodBtn != null)
            carbonRodBtn.onClick.AddListener(() =>
                PlayLabelVO(carbonRodVO, carbonRodHighlighter, carbonRodBtn));

        if (electrolyteBtn != null)
            electrolyteBtn.onClick.AddListener(() =>
                PlayLabelVO(electrolyteVO, electrolyteHighlighter, electrolyteBtn));

        if (separatorBtn != null)
            separatorBtn.onClick.AddListener(() =>
                PlayLabelVO(separatorVO, separatorHighlighter, separatorBtn));

        if (zincContainerBtn != null)
            zincContainerBtn.onClick.AddListener(() =>
                PlayLabelVO(zincContainerVO, zincContainerHighlighter, zincContainerBtn));

        if (negativeTerminalBtn != null)
            negativeTerminalBtn.onClick.AddListener(() =>
                PlayLabelVO(negativeTerminalVO, negativeTerminalHighlighter, negativeTerminalBtn));
    }

    private void PlayLabelVO(AudioClip clip, ObjectHighlighter highlighter, Button button)
    {
        if (labelVOCoroutine != null)
            StopCoroutine(labelVOCoroutine);

        if (button != null)
        {
            UIFadeIn fade = button.GetComponentInParent<UIFadeIn>();

            if (fade != null)
                fade.FadeOut();
        }

        if (audioSource != null)
            audioSource.Stop();

        StopAllHighlights();

        labelVOCoroutine = StartCoroutine(LabelVOSequence(clip, highlighter));
    }

    private IEnumerator LabelVOSequence(AudioClip clip, ObjectHighlighter highlighter)
    {
        activeHighlighter = highlighter;

        if (activeHighlighter != null)
            activeHighlighter.StartHighlight();

        PlayAudio(clip);

        yield return WaitForVoiceOver();

        if (activeHighlighter != null)
            activeHighlighter.StopHighlight();

        activeHighlighter = null;
        labelVOCoroutine = null;
    }







    private void Update()
    {
        if (isPaused)
            return;

        if (!rotateFullCell)
            return;

        if (fullCell == null)
            return;

        fullCell.transform.Rotate(
            rotationAxis,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    // =========================================================
    // START ACTIVITY
    // =========================================================

    public void StartActivity()
    {
        if (!isActiveAndEnabled)
            return;

        StopAllCoroutines();

        labelVOCoroutine = null;
        activeHighlighter = null;

        if (audioSource != null)
            audioSource.Stop();

        isPaused = false;

        HideLabels();
        StopAllHighlights();

        rotateFullCell = true;

        activityCoroutine = StartCoroutine(ActivitySequence());
    }

    private IEnumerator ActivitySequence()
    {
        PlayAudio(intro3DVO);
        yield return WaitForVoiceOver();

        PlayAudio(setupVO);
        yield return WaitForVoiceOver();

        StopFullCellRotation();

        if (cutawayCell != null)
        {
            cutawayCell.SetActive(true);
            cutawayCell.transform.localScale = cutawayOriginalScale;
        }

        PlayAudio(cellTwoPartsVO);

        if (cutawayCell != null)
            StartCoroutine(ScaleObject(cutawayCell.transform, cutawayOriginalScale, Vector3.zero, cutawayAppearDuration));

        yield return WaitForVoiceOver();

        PlayAudio(cellExternalPartVO);
        yield return WaitForVoiceOver();

        ShowLabels();

        PlayAudio(labelsIntroVO);
        yield return WaitForVoiceOver();
    }
    private void StopFullCellRotation()
    {
        rotateFullCell = false;

        if (fullCell != null)
            fullCell.transform.localRotation = Quaternion.identity;
    }








    private IEnumerator ScaleObject(Transform target,Vector3 startScale,Vector3 endScale,float duration)
    {
        if (target == null)
            yield break;

        if (duration <= 0f)
        {
            target.localScale = endScale;
            yield break;
        }

        target.localScale = startScale;

        float timer = 0f;

        while (timer < duration)
        {
            if (!isPaused)
            {
                timer += Time.deltaTime;

                float t =
                    Mathf.Clamp01(timer / duration);

                // Smooth animation
                t = Mathf.SmoothStep(0f, 1f, t);

                target.localScale =
                    Vector3.Lerp(
                        startScale,
                        endScale,
                        t
                    );
            }

            yield return null;
        }

        target.localScale = endScale;
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


    private void StopAllHighlights()
    {
        if (positiveTerminalHighlighter != null)
            positiveTerminalHighlighter.StopHighlight();

        if (insulatingSealHighlighter != null)
            insulatingSealHighlighter.StopHighlight();

        if (carbonRodHighlighter != null)
            carbonRodHighlighter.StopHighlight();

        if (electrolyteHighlighter != null)
            electrolyteHighlighter.StopHighlight();

        if (separatorHighlighter != null)
            separatorHighlighter.StopHighlight();

        if (zincContainerHighlighter != null)
            zincContainerHighlighter.StopHighlight();

        if (negativeTerminalHighlighter != null)
            negativeTerminalHighlighter.StopHighlight();
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
             audioSource.isPlaying)
        )
        {
            yield return null;
        }
    }

    public void PauseActivity()
    {
        if (!isActiveAndEnabled || isPaused)
            return;

        isPaused = true;

        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Pause();
        }
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
}