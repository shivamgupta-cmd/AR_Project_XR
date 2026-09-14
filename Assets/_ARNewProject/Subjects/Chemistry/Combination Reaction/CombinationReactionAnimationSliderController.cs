using UnityEngine;
using UnityEngine.UI;

public class CombinationReactionAnimationSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Slider animationSlider;
    [SerializeField] private AnimationEventController eventController;

    [Header("Animation")]
    [SerializeField] private string animationStateName;

    [Header("Event Normalized Times")]

    [Range(0f, 1f)]
    [SerializeField] private float animationFinishedTime = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float nextAnimationFinishedTime = 0.4f;

    [Range(0f, 1f)]
    [SerializeField] private float candleTime = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float candleOffTime = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float animationFinishedOffTime = 0.95f;


    private float previousValue;

    private bool event1Triggered;
    private bool event2Triggered;
    private bool candleTriggered;
    private bool candleOffTriggered;
    private bool finalEventTriggered;


    private void Start()
    {
        animationSlider.minValue = 0f;
        animationSlider.maxValue = 1f;
        animationSlider.value = 0f;

        animator.speed = 0f;

        previousValue = 0f;

        animationSlider.onValueChanged.AddListener(OnSliderChanged);

        SetAnimationTime(0f);
    }


    private void OnSliderChanged(float value)
    {
        // Events check karo
        CheckEvents(value);

        // Animation frame change karo
        SetAnimationTime(value);

        // Previous value save karo
        previousValue = value;
    }


    private void SetAnimationTime(float normalizedTime)
    {
        animator.Play(animationStateName, 0, normalizedTime);

        animator.Update(0f);
    }


    private void CheckEvents(float currentValue)
    {
        // ==================================================
        // FORWARD
        // Slider 0 -> 1
        // ==================================================

        if (currentValue > previousValue)
        {
            // Event 1
            if (!event1Triggered &&
                previousValue < animationFinishedTime &&
                currentValue >= animationFinishedTime)
            {
                event1Triggered = true;

                Debug.Log("FORWARD: AnimationFinished");

                eventController.AnimationFinished();
            }


            // Event 2
            if (!event2Triggered &&
                previousValue < nextAnimationFinishedTime &&
                currentValue >= nextAnimationFinishedTime)
            {
                event2Triggered = true;

                Debug.Log("FORWARD: NextAnimationFinished");

                eventController.NextAnimationFinished();
            }


            // Candle ON
            if (!candleTriggered &&
                previousValue < candleTime &&
                currentValue >= candleTime)
            {
                candleTriggered = true;

                Debug.Log("FORWARD: Candle");

                eventController.Candle();
            }


            // Candle OFF
            if (!candleOffTriggered &&
                previousValue < candleOffTime &&
                currentValue >= candleOffTime)
            {
                candleOffTriggered = true;

                Debug.Log("FORWARD: Candle Off");

                eventController.Candleoff();
            }


            // Final OFF
            if (!finalEventTriggered &&
                previousValue < animationFinishedOffTime &&
                currentValue >= animationFinishedOffTime)
            {
                finalEventTriggered = true;

                Debug.Log("FORWARD: Animation Finished Off");

                eventController.AnimationFinishedoff();
            }
        }


        // ==================================================
        // BACKWARD
        // Slider 1 -> 0
        // ==================================================

        else if (currentValue < previousValue)
        {
            // --------------------------------------------------
            // Final event cross backwards
            // --------------------------------------------------

            if (previousValue >= animationFinishedOffTime &&
                currentValue < animationFinishedOffTime)
            {
                finalEventTriggered = false;

                Debug.Log("BACKWARD: AnimationFinishedoff");

                eventController.AnimationFinishedoff();
            }


            // --------------------------------------------------
            // Candle OFF event cross backwards
            // --------------------------------------------------

            if (previousValue >= candleOffTime &&
                currentValue < candleOffTime)
            {
                candleOffTriggered = false;

                Debug.Log("BACKWARD: Candleoff");

                eventController.Candleoff();
            }


            // --------------------------------------------------
            // Candle event cross backwards
            // --------------------------------------------------

            if (previousValue >= candleTime &&
                currentValue < candleTime)
            {
                candleTriggered = false;

                Debug.Log("BACKWARD: Candle");

                eventController.Candle();
            }


            // --------------------------------------------------
            // Next Animation event cross backwards
            // --------------------------------------------------

            if (previousValue >= nextAnimationFinishedTime &&
                currentValue < nextAnimationFinishedTime)
            {
                event2Triggered = false;

                Debug.Log("BACKWARD: NextAnimationFinished");

                eventController.NextAnimationFinished();
            }


            // --------------------------------------------------
            // Animation Finished cross backwards
            // --------------------------------------------------

            if (previousValue >= animationFinishedTime &&
                currentValue < animationFinishedTime)
            {
                event1Triggered = false;

                Debug.Log("BACKWARD: AnimationFinished");

                eventController.AnimationFinished();
            }
        }
    }


    private void OnDestroy()
    {
        if (animationSlider != null)
        {
            animationSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }
}