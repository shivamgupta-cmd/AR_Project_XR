using System.Collections;
using UnityEngine;

public class ChildScaleAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float scaleDuration = 0.4f;
    [SerializeField] private float delayBetweenChildren = 0.15f;

    [Header("Pop Effect")]
    [SerializeField] private bool usePopEffect = true;
    [SerializeField] private float popScale = 1.15f;

    private Transform[] children;

    private void Awake()
    {
        children = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
            children[i].localScale = Vector3.zero;
        }
    }

    public void PopupLocation()
    {
        StartCoroutine(AnimateAllChildren());
    }

    private IEnumerator AnimateAllChildren()
    {
        foreach (Transform child in children)
        {
            StartCoroutine(ScaleChild(child));
            yield return new WaitForSeconds(delayBetweenChildren);
        }
    }

    private IEnumerator ScaleChild(Transform child)
    {
        Vector3 targetScale = Vector3.one;

        if (usePopEffect)
        {
            float timer = 0f;

            while (timer < scaleDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / scaleDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                child.localScale =
                    Vector3.Lerp(Vector3.zero, Vector3.one * popScale, t);

                yield return null;
            }

            timer = 0f;
            float settleDuration = scaleDuration * 0.35f;

            while (timer < settleDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / settleDuration);

                child.localScale =
                    Vector3.Lerp(Vector3.one * popScale, targetScale, t);

                yield return null;
            }
        }
        else
        {
            float timer = 0f;

            while (timer < scaleDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / scaleDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                child.localScale =
                    Vector3.Lerp(Vector3.zero, targetScale, t);

                yield return null;
            }
        }

        child.localScale = targetScale;
    }
}
