using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gives UI buttons visible mouse-hover and mobile press feedback.
/// </summary>
public sealed class ButtonHoverFeedback : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [SerializeField] private float hoverScale = 1.035f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float animationSpeed = 15f;

    private Vector3 baseScale;
    private float targetMultiplier = 1f;
    private bool pointerInside;
    private bool pointerDown;
    private bool selected;
    private Button button;
    private Image stateImage;
    private Sprite normalSprite;

    private void Awake()
    {
        baseScale = transform.localScale;
        button = GetComponent<Button>();
        ResolveStateImage();
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            baseScale * targetMultiplier,
            1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime));
    }

    private void LateUpdate()
    {
        ApplySpriteState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        targetMultiplier = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        targetMultiplier = 1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        targetMultiplier = pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        targetMultiplier = pointerInside ? hoverScale : 1f;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    private void ResolveStateImage()
    {
        if (button == null)
        {
            return;
        }

        stateImage = button.targetGraphic as Image;

        // A disabled/wrong child Image cannot display SpriteSwap states.
        if (stateImage == null || !stateImage.enabled)
        {
            Image ownImage = GetComponent<Image>();
            if (ownImage != null)
            {
                stateImage = ownImage;
                button.targetGraphic = ownImage;
            }
        }

        if (stateImage != null)
        {
            normalSprite = stateImage.sprite;
        }
    }

    private void ApplySpriteState()
    {
        if (button == null || stateImage == null || button.transition != Selectable.Transition.SpriteSwap)
        {
            return;
        }

        Sprite nextSprite = normalSprite;
        SpriteState states = button.spriteState;

        if (!button.interactable && states.disabledSprite != null)
            nextSprite = states.disabledSprite;
        else if (pointerDown && states.pressedSprite != null)
            nextSprite = states.pressedSprite;
        else if (selected && states.selectedSprite != null)
            nextSprite = states.selectedSprite;
        else if (pointerInside && states.highlightedSprite != null)
            nextSprite = states.highlightedSprite;

        if (nextSprite != null && stateImage.sprite != nextSprite)
        {
            stateImage.sprite = nextSprite;
        }
    }

    private void OnDisable()
    {
        pointerInside = false;
        pointerDown = false;
        selected = false;
        targetMultiplier = 1f;
        transform.localScale = baseScale;

        if (stateImage != null && normalSprite != null)
        {
            stateImage.sprite = normalSprite;
        }
    }
}

/// <summary>Adds consistent feedback to every Button in every loaded scene.</summary>
internal static class ButtonHoverFeedbackInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Button[] buttons = Object.FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.GetComponent<ButtonHoverFeedback>() == null)
            {
                button.gameObject.AddComponent<ButtonHoverFeedback>();
            }
        }
    }
}
