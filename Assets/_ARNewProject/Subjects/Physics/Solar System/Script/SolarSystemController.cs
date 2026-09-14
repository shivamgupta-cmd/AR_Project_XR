using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SolarSystemController : MonoBehaviour
{
    [Serializable]
    public class PlanetData
    {
        [Header("Planet Object")]
        public Transform planetObject;
        [Header("Label")]
        public GameObject labelCanvas;
        [Header("Button")]
        public Button planetButton;
        [Header("Voice Over")]
        public AudioClip voiceOver;
        [TextArea(2, 5)]
        public string voiceText;
        [HideInInspector] public bool isVisited;
    }
    [Header("Planets")]
    [SerializeField] private PlanetData[] planets;
    [Header("Planet Focus")]
    [SerializeField] private PlanetFocusController planetFocusController;
    [Header("Solar System Animation")]
    [SerializeField] private Animator solarSystemAnimator;
    [SerializeField] private string orbitAnimationName = "SolarSystemOrbit";
    [Header("Voice Overs")]
    [SerializeField] private AudioClip orbitIntroVO;
    [SerializeField] private AudioClip finalVO;
    private Vector3 insScale;
    public Vector3 newScale;
    [SerializeField] private float scaleDuration = 0.5f;
    private Coroutine scaleCoroutine;
    private Coroutine voCoroutine;
    private int visitedPlanetCount = 0;
    private bool introCompleted = false;
    private bool finalVOPlayed = false;
    private bool isActivityPaused = false;
    private float savedAnimatorSpeed = 1f;
    private bool[] savedButtonStates;
    private void Start()
    {
        if (solarSystemAnimator != null)
            insScale = solarSystemAnimator.transform.localPosition;
        savedButtonStates = new bool[planets.Length];
        for (int i = 0; i < planets.Length; i++)
        {
            planets[i].isVisited = false;
            if (planets[i].labelCanvas != null)
                planets[i].labelCanvas.SetActive(false);
        }
        SetupPlanetButtons();
        SetPlanetButtonsInteractable(false);
        PlayOrbitIntroduction();
    }
    private void SetupPlanetButtons()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i].planetButton == null)
                continue;
            int index = i;
            planets[i].planetButton.onClick.AddListener(() => SelectPlanet(index));
        }
    }
    public void SelectPlanet(int index)
    {
        if (!introCompleted || finalVOPlayed || isActivityPaused)
            return;
        if (index < 0 || index >= planets.Length)
            return;
        PlanetData planet = planets[index];
        if (planet.isVisited)
            return;
        planet.isVisited = true;
        visitedPlanetCount++;
        //if (planet.planetButton != null)
        //    planet.planetButton.interactable = false;
        //SetPlanetButtonsInteractable(false);
        if (planet.labelCanvas != null)
        {
            planet.labelCanvas.SetActive(true);
            UIFadeIn fade = planet.labelCanvas.GetComponent<UIFadeIn>();
            if (fade != null)
                fade.FadeOut();
        }
        if (planetFocusController != null && planet.planetObject != null)
            planetFocusController.FocusPlanet(planet.planetObject, planet.voiceText);
        Debug.Log("Visited : " + visitedPlanetCount + " / " + planets.Length);
        PlayPlanetVO(planet.voiceOver);
    }
    public void PlayOrbitIntroduction()
    {
        if (orbitIntroVO == null)
        {
            IntroComplete();
            return;
        }
        if (VOController.Instance == null)
            return;
        VOController.Instance.PlayVO(orbitIntroVO);
        voCoroutine = StartCoroutine(WaitForIntroVO());
    }
    private IEnumerator WaitForIntroVO()
    {
        yield return null;
        while (isActivityPaused || (VOController.Instance != null && VOController.Instance.IsPlaying()))
            yield return null;
        IntroComplete();
        voCoroutine = null;
    }
    private void IntroComplete()
    {
        PauseOrbitAnimation();
        introCompleted = true;
        SetPlanetButtonsInteractable(true);
    }
    private void PlayPlanetVO(AudioClip clip)
    {
        if (VOController.Instance == null)
            return;
        if (voCoroutine != null)
        {
            StopCoroutine(voCoroutine);
            voCoroutine = null;
        }
        if (clip == null)
        {
            voCoroutine = StartCoroutine(FinishPlanetWithoutVO());
            return;
        }
        VOController.Instance.PlayVO(clip);
        voCoroutine = StartCoroutine(WaitForPlanetVO());
    }
    private IEnumerator WaitForPlanetVO()
    {
        yield return null;
        while (isActivityPaused || (VOController.Instance != null && VOController.Instance.IsPlaying()))
            yield return null;
        if (planetFocusController != null)
        {
            planetFocusController.ReturnPlanet();
            while (planetFocusController.IsBusy)
                yield return null;
        }
        voCoroutine = null;
        CheckAllPlanetsVisited();
    }
    private IEnumerator FinishPlanetWithoutVO()
    {
        if (planetFocusController != null)
        {
            planetFocusController.ReturnPlanet();
            while (planetFocusController.IsBusy)
                yield return null;
        }
        voCoroutine = null;
        CheckAllPlanetsVisited();
    }
    private void CheckAllPlanetsVisited()
    {
        if (visitedPlanetCount >= planets.Length)
            PlayFinalVO();
    }
    private void PlayFinalVO()
    {
        if (finalVOPlayed)
            return;
        finalVOPlayed = true;
        ResumeOrbitAnimation();
        if (VOController.Instance != null && finalVO != null)
            VOController.Instance.PlayVO(finalVO);
    }
    private void SetPlanetButtonsInteractable(bool value)
    {
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i].labelCanvas != null)
                planets[i].labelCanvas.SetActive(value);
        }
    }
    public void PauseOrbitAnimation()
    {
        if (solarSystemAnimator == null)
            return;
        solarSystemAnimator.speed = 0f;
        SmoothScale(newScale);
    }
    public void ResumeOrbitAnimation()
    {
        if (solarSystemAnimator == null)
            return;
        solarSystemAnimator.speed = 1f;
        SmoothScale(insScale);
    }
    public void StopAllActivity()
    {
        if (isActivityPaused)
            return;
        isActivityPaused = true;
        if (solarSystemAnimator != null)
        {
            savedAnimatorSpeed = solarSystemAnimator.speed;
            solarSystemAnimator.speed = 0f;
        }
        if (VOController.Instance != null)
            VOController.Instance.PauseVO();
        if (planetFocusController != null)
            planetFocusController.PauseActivity();
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i].planetButton != null)
            {
                savedButtonStates[i] = planets[i].planetButton.interactable;
                planets[i].planetButton.interactable = false;
            }
        }
    }
    public void StartAllActivity()
    {
        if (!isActivityPaused)
            return;
        isActivityPaused = false;
        if (solarSystemAnimator != null)
            solarSystemAnimator.speed = savedAnimatorSpeed;
        if (VOController.Instance != null)
            VOController.Instance.ResumeVO();
        if (planetFocusController != null)
            planetFocusController.ResumeActivity();
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i].planetButton != null)
                planets[i].planetButton.interactable = savedButtonStates[i] && !planets[i].isVisited;
        }
    }
    public void StopVoiceOver()
    {
        if (voCoroutine != null)
        {
            StopCoroutine(voCoroutine);
            voCoroutine = null;
        }
        if (VOController.Instance != null)
            VOController.Instance.StopVO();
    }
    private void SmoothScale(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }
    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Transform target = solarSystemAnimator.transform;
        Vector3 startScale = target.localPosition;
        float time = 0f;
        while (time < scaleDuration)
        {
            if (isActivityPaused)
            {
                yield return null;
                continue;
            }
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / scaleDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            target.localPosition = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        target.localPosition = targetScale;
        scaleCoroutine = null;
    }
}