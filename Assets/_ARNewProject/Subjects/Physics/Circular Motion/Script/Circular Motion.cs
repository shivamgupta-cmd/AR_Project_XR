using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CircularMotion : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] clip;
    [SerializeField] private AudioSource sources;

    [Header("Car Audio")]
    [SerializeField] private AudioSource carAudioSource;
    [SerializeField] private float normalCarVolume = 0.01f;
    [SerializeField] private float maxCarVolume = 1f;

    [Header("Panels")]
    [SerializeField] private GameObject[] Panels;

    [Header("Sliders")]
    [SerializeField] private Slider Radius;
    [SerializeField] private Slider Speed;

    [Header("Rotation")]
    [SerializeField] private CircularRotateObject circularRotateObject;

    [Header("Radius Blend Shape")]
    [SerializeField] private SkinnedMeshRenderer radiusSkinnedMesh;
    [SerializeField] private string radiusBlendShapeName = "blendShape1.pCylinder5";

    private int radiusBlendShapeIndex = -1;
    private bool radiusAudioPlayed = false;

    private bool isActivityPaused = false;

    private bool narrationWasPlaying = false;
    private bool carAudioWasPlaying = false;
    private bool rotationRunning = false;
    private bool rotationWasRunning = false;

    private bool radiusWasInteractable = false;
    private bool speedWasInteractable = false;

    private void Awake()
    {
        FindBlendShape();
    }

    private void Start()
    {
        SetPanelActive(0, false);
        SetPanelActive(8, false);
        SetPanelActive(1, false);
        SetPanelActive(5, true);
        SetPanelActive(6, true);
        SetPanelActive(7, true);

        if (Radius != null)
        {
            Radius.minValue = 0f;
            Radius.maxValue = 1f;
            Radius.wholeNumbers = false;
            Radius.SetValueWithoutNotify(0.3f);
            Radius.onValueChanged.RemoveListener(OnRadiusSliderChanged);
            Radius.onValueChanged.AddListener(OnRadiusSliderChanged);
            SetRadiusBlendShape((1f - Radius.value) * 100f);
        }

        if (Speed != null)
        {
            Speed.minValue = 0f;
            Speed.maxValue = 1f;
            Speed.wholeNumbers = false;
            Speed.SetValueWithoutNotify(0f);
            Speed.onValueChanged.RemoveListener(OnSpeedSliderChanged);
            Speed.onValueChanged.AddListener(OnSpeedSliderChanged);
        }

        //    SetRadiusBlendShape((1f - Radius.value) * 100f);        
        //SetRadiusBlendShape(70f);


        if (circularRotateObject != null)
        {
            circularRotateObject.SetSpeed(38f);
            StartCarRotation();
        }

        if (carAudioSource != null)
        {
            carAudioSource.loop = true;
            carAudioSource.volume = normalCarVolume;

            if (!carAudioSource.isPlaying)
                carAudioSource.Play();
        }

        StartCoroutine(IntroManager());
    }

    private IEnumerator IntroManager()
    {
        yield return WaitWhilePaused();
        yield return PausableDelay(0.3f);

        yield return StartCoroutine(PlayAndWait(0));

        yield return WaitWhilePaused();

        StopCarRotation();

        if (carAudioSource != null)
            carAudioSource.Pause();

        SetPanelActive(2, true);
        SetPanelActive(5, false);
        SetPanelActive(6, false);

        yield return StartCoroutine(PlayAndWait(1));

        yield return WaitWhilePaused();

        SetPanelActive(2, false);
        SetPanelActive(3, true);

        yield return StartCoroutine(PlayAndWait(2));

        yield return WaitWhilePaused();

        SetPanelActive(3, false);
        SetPanelActive(4, true);

        yield return StartCoroutine(PlayAndWait(3));

        yield return WaitWhilePaused();

        SetPanelActive(4, false);
        SetPanelActive(0, true);
        SetPanelActive(1, true);
        SetPanelActive(8, true);

        StartCarRotation();

        if (carAudioSource != null)
            carAudioSource.UnPause();

        yield return StartCoroutine(PlayAndWait(4));

        yield return WaitWhilePaused();

        yield return StartCoroutine(PlayAndWait(5));
    }

    private IEnumerator PlayAndWait(int index)
    {
        yield return WaitWhilePaused();

        if (sources == null)
            yield break;

        if (clip == null)
            yield break;

        if (index < 0 || index >= clip.Length)
            yield break;

        if (clip[index] == null)
            yield break;

        sources.Stop();
        sources.clip = clip[index];
        sources.time = 0f;
        sources.Play();

        yield return null;

        while (sources.isPlaying || isActivityPaused)
            yield return null;

        yield return WaitWhilePaused();
    }

    private IEnumerator WaitWhilePaused()
    {
        while (isActivityPaused)
            yield return null;
    }

    private IEnumerator PausableDelay(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (!isActivityPaused)
                timer += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    public void PauseActivity()
    {
        if (isActivityPaused)
            return;

        SetPanelActive(0, false);
        SetPanelActive(1, false);
        SetPanelActive(8, false);

        isActivityPaused = true;

        if (sources != null)
        {
            narrationWasPlaying = sources.isPlaying;

            if (narrationWasPlaying)
                sources.Pause();
        }

        if (carAudioSource != null)
        {
            carAudioWasPlaying = carAudioSource.isPlaying;

            if (carAudioWasPlaying)
                carAudioSource.Pause();
        }

        rotationWasRunning = rotationRunning;

        if (rotationWasRunning && circularRotateObject != null)
            circularRotateObject.StopRotation();

        if (Radius != null)
        {
            radiusWasInteractable = Radius.interactable;
            Radius.interactable = false;
        }

        if (Speed != null)
        {
            speedWasInteractable = Speed.interactable;
            Speed.interactable = false;
        }
    }

    public void ResumeActivity()
    {
        if (!isActivityPaused)
            return;

        SetPanelActive(0, true);
        SetPanelActive(1, true);
        SetPanelActive(8, true);

        isActivityPaused = false;

        if (sources != null && narrationWasPlaying)
            sources.UnPause();

        if (carAudioSource != null && carAudioWasPlaying)
            carAudioSource.UnPause();

        if (rotationWasRunning && circularRotateObject != null)
        {
            circularRotateObject.StartRotation();
            rotationRunning = true;
        }

        if (Radius != null)
            Radius.interactable = radiusWasInteractable;

        if (Speed != null)
            Speed.interactable = speedWasInteractable;
    }

    private void StartCarRotation()
    {
        if (circularRotateObject == null)
            return;

        circularRotateObject.StartRotation();
        rotationRunning = true;
    }

    private void StopCarRotation()
    {
        if (circularRotateObject == null)
            return;

        circularRotateObject.StopRotation();
        circularRotateObject.ResetRotation();

        rotationRunning = false;
    }

    private void FindBlendShape()
    {
        if (radiusSkinnedMesh == null)
            return;

        if (radiusSkinnedMesh.sharedMesh == null)
            return;

        radiusBlendShapeIndex =
            radiusSkinnedMesh.sharedMesh.GetBlendShapeIndex(
                radiusBlendShapeName
            );
    }

    private void OnRadiusSliderChanged(float value)
    {
        if (isActivityPaused)
            return;

        //float blendValue = value * 100f;

        float blendValue = (1f - value) * 100f;

        SetRadiusBlendShape(blendValue);

        if (value >= 0.75f && !radiusAudioPlayed)
            radiusAudioPlayed = true;
    }

    public void SliderRadis()
    {
        if (Radius == null)
            return;

        OnRadiusSliderChanged(Radius.value);
    }

    private void SetRadiusBlendShape(float value)
    {
        if (radiusSkinnedMesh == null)
            return;

        if (radiusSkinnedMesh.sharedMesh == null)
            return;

        if (radiusBlendShapeIndex == -1)
            FindBlendShape();

        if (radiusBlendShapeIndex == -1)
            return;

        value = Mathf.Clamp(value, 0f, 100f);

        radiusSkinnedMesh.SetBlendShapeWeight(
            radiusBlendShapeIndex,
            value
        );
    }

    private void OnSpeedSliderChanged(float value)
    {
        if (isActivityPaused)
            return;

        if (circularRotateObject == null)
            return;

        float actualSpeed = Mathf.Lerp(
            38f,
            1000f,
            value
        );

        circularRotateObject.SetSpeed(actualSpeed);

        StartCarRotation();

        UpdateCarAudio(value);
    }

    private void UpdateCarAudio(float sliderValue)
    {
        if (carAudioSource == null)
            return;

        float carVolume = Mathf.Lerp(
            normalCarVolume,
            maxCarVolume,
            sliderValue
        );

        carAudioSource.volume = carVolume;

        if (!carAudioSource.isPlaying)
            carAudioSource.Play();
    }

    public void SliderSpeed()
    {
        if (Speed == null)
            return;

        OnSpeedSliderChanged(Speed.value);
    }

    private void SetPanelActive(int index, bool state)
    {
        if (Panels == null)
            return;

        if (index < 0 || index >= Panels.Length)
            return;

        if (Panels[index] == null)
            return;

        Panels[index].SetActive(state);
    }

    private void OnDestroy()
    {
        if (Radius != null)
            Radius.onValueChanged.RemoveListener(OnRadiusSliderChanged);

        if (Speed != null)
            Speed.onValueChanged.RemoveListener(OnSpeedSliderChanged);
    }
}