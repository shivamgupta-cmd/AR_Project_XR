using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Smoothly fades UI Images and text in or out.
/// Call FadeIn() or FadeOut() from a Button, Animation Event, or any UnityEvent.
/// </summary>
public class UIFadeIn : MonoBehaviour
{
    [Tooltip("Images and text to fade. Supports Image, UI Text, and TextMeshProUGUI.")]
    [SerializeField] private Graphic[] targets;

    [Min(0f)]
    [SerializeField] private float duration = 2f;

    [SerializeField] private AnimationCurve fadeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Fade automatically whenever this GameObject becomes active.")]
    [SerializeField] private bool fadeOnEnable;

    [Tooltip("Keep fading even when Time.timeScale is zero.")]
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine fadeRoutine;

    private void Reset()
    {
        targets = GetComponentsInChildren<Graphic>(true);
    }

    private void OnEnable()
    {
        if (fadeOnEnable)
        {
            FadeIn();
        }
    }

    /// <summary>Starts the fade again from zero alpha.</summary>
    public void FadeIn()
    {
        StopCurrentFade();
        fadeRoutine = StartCoroutine(FadeInRoutine());
    }

    /// <summary>Fades from full alpha to zero, then deactivates this GameObject.</summary>
    public void FadeOut()
    {
        StopCurrentFade();
        fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    /// <summary>Immediately makes all assigned UI elements transparent.</summary>
    public void SetTransparent()
    {
        StopCurrentFade();
        SetAlpha(0f);
    }

    /// <summary>Immediately makes all assigned UI elements fully visible.</summary>
    public void SetVisible()
    {
        StopCurrentFade();
        SetAlpha(1f);
    }

    private IEnumerator FadeInRoutine()
    {
        SetAlpha(0f);

        if (duration <= 0f)
        {
            SetAlpha(1f);
            fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            SetAlpha(fadeCurve.Evaluate(progress));
            yield return null;
        }

        SetAlpha(1f);
        fadeRoutine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
        SetAlpha(1f);

        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                SetAlpha(1f - fadeCurve.Evaluate(progress));
                yield return null;
            }
        }

        SetAlpha(0f);
        fadeRoutine = null;
        gameObject.SetActive(false);
    }

    private void StopCurrentFade()
    {
        if (fadeRoutine == null)
        {
            return;
        }

        StopCoroutine(fadeRoutine);
        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            Graphic target = targets[i];
            if (target == null)
            {
                continue;
            }

            Color color = target.color;
            color.a = alpha;
            target.color = color;
        }
    }
}
