using System.Collections;
using UnityEngine;

public class LabelController : MonoBehaviour
{

    public static LabelController instance;

    [SerializeField] private CanvasGroup[] labels;
    [SerializeField] private float aftarClickDelay = 1f;
    [SerializeField] private float fadeDuration = 0.3f;

    private void Awake()
    {
        instance = this;

        if (labels == null || labels.Length == 0)
            labels = GetComponentsInChildren<CanvasGroup>(true);
        foreach (CanvasGroup label in labels)
        {
            label.alpha = 0f;
            label.blocksRaycasts = false;
            label.interactable = false;
        }
    }
    private void Start()
    {
        
    }

    public void ShowAllLabels()
    {
        foreach (CanvasGroup label in labels)
            StartCoroutine(FadeLabel(label, 1f));
    }

    public void HideAllLabels()
    {
        foreach (CanvasGroup label in labels)
            StartCoroutine(FadeLabel(label, 0f));
    }

    private IEnumerator FadeLabel(CanvasGroup canvasGroup, float targetAlpha)
    {
        yield return new WaitForSecondsRealtime(aftarClickDelay);

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        if (targetAlpha == 0f)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                time / fadeDuration
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 1f)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
}