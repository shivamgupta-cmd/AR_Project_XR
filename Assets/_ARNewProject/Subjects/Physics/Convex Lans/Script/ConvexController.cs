using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConvexController : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Intro Audio")]
    [SerializeField] private AudioClip introConvexLens;

    [Header("Setup Intro")]
    [SerializeField] private AudioClip setupAudio;

    [Header("Image Formation Case")]
    [SerializeField] private AudioClip betweenF1And2F1Audio;

    [Header("Parts Audio")]
    [SerializeField] private AudioClip candleAudio;
    [SerializeField] private AudioClip convexLensAudio;
    [SerializeField] private AudioClip imageAudio;

    [Header("Intro Objects")]
    [SerializeField] private Transform introObj;
    [SerializeField] private Transform introCandle;
    [SerializeField] private Transform introLens;

    [Header("Intro Animation Settings")]
    [SerializeField] private float introScaleDuration = 0.7f;
    [SerializeField] private float introRotationSpeed = 40f;

    [Header("Main Convex Lens Object")]
    [SerializeField] private Transform convexLensAnimObj;
    [SerializeField] private float convexScaleDuration = 0.7f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationStateName = "convex lans-anim";

    [Header("Labels")]
    [SerializeField] private GameObject candleLabel;
    [SerializeField] private GameObject convexLensLabel;
    [SerializeField] private GameObject imageLabel;

    [Header("Label Buttons")]
    [SerializeField] private Button candleButton;
    [SerializeField] private Button convexLensButton;
    [SerializeField] private Button imageButton;

    [Header("Settings")]
    [SerializeField] private bool playIntroOnStart = true;

    private Coroutine mainFlowCoroutine;

    private bool rotateIntroObjects;
    private bool isActivityPaused;

    private bool audioWasPlayingBeforePause;
    private float animatorSpeedBeforePause;

    private void Start()
    {
        SetLabels(false);

        if (introObj != null)
        {
            introObj.gameObject.SetActive(true);
            introObj.localScale = Vector3.zero;
        }

        if (convexLensAnimObj != null)
        {
            convexLensAnimObj.localScale = Vector3.zero;
            convexLensAnimObj.gameObject.SetActive(false);
        }

        if (animator != null)
        {
            animator.Play(animationStateName, 0, 0f);
            animator.speed = 0f;
            animator.Update(0f);
        }

        if (candleButton != null)
            candleButton.onClick.AddListener(PlayCandle);

        if (convexLensButton != null)
            convexLensButton.onClick.AddListener(PlayConvexLens);

        if (imageButton != null)
            imageButton.onClick.AddListener(PlayImage);

        if (playIntroOnStart)
            StartMainFlow();
    }

    private void Update()
    {
        if (isActivityPaused)
            return;

        if (!rotateIntroObjects)
            return;

        if (introCandle != null)
        {
            introCandle.Rotate(
                Vector3.up,
                introRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }

        if (introLens != null)
        {
            introLens.Rotate(
                Vector3.up,
                introRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }

    public void StartMainFlow()
    {
        if (mainFlowCoroutine != null)
            StopCoroutine(mainFlowCoroutine);

        mainFlowCoroutine = StartCoroutine(MainFlow());
    }

    private IEnumerator MainFlow()
    {
        if (introObj != null)
        {
            introObj.gameObject.SetActive(true);
            introObj.localScale = Vector3.zero;
        }

        PlayAudio(introConvexLens);

        rotateIntroObjects = true;

        if (introObj != null)
        {
            yield return StartCoroutine(
                ScaleObject(
                    introObj,
                    Vector3.zero,
                    Vector3.one,
                    introScaleDuration
                )
            );
        }

        yield return StartCoroutine(
            WaitForAudioToFinish(introConvexLens)
        );

        rotateIntroObjects = false;

        if (introObj != null)
        {
            yield return StartCoroutine(
                ScaleObject(
                    introObj,
                    introObj.localScale,
                    Vector3.zero,
                    introScaleDuration
                )
            );

            introObj.gameObject.SetActive(false);
        }

        while (isActivityPaused)
            yield return null;

        if (convexLensAnimObj != null)
        {
            convexLensAnimObj.gameObject.SetActive(true);
            convexLensAnimObj.localScale = Vector3.zero;

            yield return StartCoroutine(
                ScaleObject(
                    convexLensAnimObj,
                    Vector3.zero,
                    Vector3.one,
                    convexScaleDuration
                )
            );
        }

        yield return StartCoroutine(
            PlayAudioAndWait(setupAudio)
        );

        while (isActivityPaused)
            yield return null;

        PlayAnimation();

        yield return StartCoroutine(
            PlayAudioAndWait(
                betweenF1And2F1Audio
            )
        );

        while (isActivityPaused)
            yield return null;

        SetLabels(true);

        mainFlowCoroutine = null;
    }

    private IEnumerator ScaleObject(
        Transform target,
        Vector3 startScale,
        Vector3 endScale,
        float duration
    )
    {
        if (target == null)
            yield break;

        if (duration <= 0f)
        {
            target.localScale = endScale;
            yield break;
        }

        float time = 0f;

        target.localScale = startScale;

        while (time < duration)
        {
            if (isActivityPaused)
            {
                yield return null;
                continue;
            }

            time += Time.deltaTime;

            float t = Mathf.Clamp01(
                time / duration
            );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            target.localScale = Vector3.Lerp(
                startScale,
                endScale,
                t
            );

            yield return null;
        }

        target.localScale = endScale;
    }

    private void PlayAnimation()
    {
        if (animator == null)
            return;

        animator.Play(
            animationStateName,
            0,
            0f
        );

        animator.speed = 1f;
    }

    private IEnumerator PlayAudioAndWait(AudioClip clip)
    {
        if (clip == null)
            yield break;

        if (audioSource == null)
            yield break;

        while (isActivityPaused)
            yield return null;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        yield return StartCoroutine(
            WaitForAudioToFinish(clip)
        );
    }

    private IEnumerator WaitForAudioToFinish(AudioClip clip)
    {
        if (audioSource == null)
            yield break;

        while (audioSource.clip == clip)
        {
            if (isActivityPaused)
            {
                yield return null;
                continue;
            }

            if (!audioSource.isPlaying)
                break;

            yield return null;
        }
    }

    private void SetLabels(bool value)
    {
        if (candleLabel != null)
            candleLabel.SetActive(value);

        if (convexLensLabel != null)
            convexLensLabel.SetActive(value);

        if (imageLabel != null)
            imageLabel.SetActive(value);
    }

    public void PlayCandle()
    {
        if (isActivityPaused)
            return;

        if (candleLabel != null)
        {
            UIFadeIn fade =
                candleLabel.GetComponent<UIFadeIn>();

            if (fade != null)
                fade.FadeOut();
        }

        PlayAudio(candleAudio);
    }

    public void PlayConvexLens()
    {
        if (isActivityPaused)
            return;

        if (convexLensLabel != null)
        {
            UIFadeIn fade =
                convexLensLabel.GetComponent<UIFadeIn>();

            if (fade != null)
                fade.FadeOut();
        }

        PlayAudio(convexLensAudio);
    }

    public void PlayImage()
    {
        if (isActivityPaused)
            return;

        if (imageLabel != null)
        {
            UIFadeIn fade =
                imageLabel.GetComponent<UIFadeIn>();

            if (fade != null)
                fade.FadeOut();
        }

        PlayAudio(imageAudio);
    }

    public void PlayIntro()
    {
        if (isActivityPaused)
            return;

        PlayAudio(introConvexLens);
    }

    public void PlayImageFormationIntro()
    {
        if (isActivityPaused)
            return;

        PlayAudio(setupAudio);
    }

    public void BetweenF1And2F1()
    {
        if (isActivityPaused)
            return;

        PlayAudio(betweenF1And2F1Audio);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip == null)
            return;

        if (audioSource == null)
            return;

        if (isActivityPaused)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void ActivityPause()
    {
        if (isActivityPaused)
            return;

        isActivityPaused = true;

        if (audioSource != null)
        {
            audioWasPlayingBeforePause =
                audioSource.isPlaying;

            if (audioWasPlayingBeforePause)
                audioSource.Pause();
        }

        if (animator != null)
        {
            animatorSpeedBeforePause =
                animator.speed;

            animator.speed = 0f;
        }
    }

    public void ActivityResume()
    {
        if (!isActivityPaused)
            return;

        isActivityPaused = false;

        if (audioSource != null &&
            audioWasPlayingBeforePause)
        {
            audioSource.UnPause();
        }

        if (animator != null)
        {
            animator.speed =
                animatorSpeedBeforePause;
        }

        audioWasPlayingBeforePause = false;
    }

    public void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();

        audioWasPlayingBeforePause = false;
    }

    public void ResetConvexLens()
    {
        if (mainFlowCoroutine != null)
        {
            StopCoroutine(mainFlowCoroutine);
            mainFlowCoroutine = null;
        }

        isActivityPaused = false;
        rotateIntroObjects = false;
        audioWasPlayingBeforePause = false;

        StopAudio();

        SetLabels(false);

        if (introObj != null)
        {
            introObj.gameObject.SetActive(true);
            introObj.localScale = Vector3.zero;
        }

        if (convexLensAnimObj != null)
        {
            convexLensAnimObj.localScale =
                Vector3.zero;

            convexLensAnimObj.gameObject.SetActive(false);
        }

        if (animator != null)
        {
            animator.speed = 0f;

            animator.Play(
                animationStateName,
                0,
                0f
            );

            animator.Update(0f);
        }
    }

    public void RestartConvexLens()
    {
        ResetConvexLens();
        StartMainFlow();
    }

    private void OnDestroy()
    {
        if (candleButton != null)
            candleButton.onClick.RemoveListener(PlayCandle);

        if (convexLensButton != null)
            convexLensButton.onClick.RemoveListener(PlayConvexLens);

        if (imageButton != null)
            imageButton.onClick.RemoveListener(PlayImage);
    }
}