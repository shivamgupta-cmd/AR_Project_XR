using UnityEngine;
using UnityEngine.Events;

public class AnimationEventController : MonoBehaviour
{
    public UnityEvent onAnimationComplete;
    public UnityEvent onNextAnimationComplete;
    public UnityEvent candle;
    public UnityEvent candleOff;
    public UnityEvent onAnimationcompleteOff;

    public void AnimationFinished()
    {
        Debug.Log("Animation Completed");
        onAnimationComplete?.Invoke();
    }
    public void NextAnimationFinished()
    {
        Debug.Log("Animation Completed");
        onNextAnimationComplete?.Invoke();
    }

    public void Candle()
    {
        Debug.Log("Animation Completed");
        candle?.Invoke();
    }

    public void Candleoff()
    {
        Debug.Log("Animation Completed");
        candleOff?.Invoke();
    }

    public void AnimationFinishedoff()
    {
        Debug.Log("Animation Completed");
        onAnimationcompleteOff?.Invoke();
    }
}