using UnityEngine;
using UnityEngine.UI;

public class AnimationSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Slider animationSlider;

    [Header("Animation")]
    [SerializeField] private string animationStateName;

    private void Start()
    {
        if (animationSlider != null)
        {
            animationSlider.minValue = 0f;
            animationSlider.maxValue = 1f;
            animationSlider.value = 0f;
            animationSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        if (animator != null)
        {
            animator.speed = 0f;
        }
        SetAnimationTime(0f);
    }
    

    private void OnSliderChanged(float value)
    {
        SetAnimationTime(value);
    }

    private void SetAnimationTime(float normalizedTime)
    {
        animator.Play(animationStateName, 0, normalizedTime);

        animator.Update(0f);
    }

    private void OnDestroy()
    {
        if (animationSlider != null)
        {
            animationSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }
}