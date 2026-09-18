using System.Collections;
using UnityEngine;

public class ArrowSequenceLoop : MonoBehaviour
{
    [Header("Arrows In Sequence")]
    [SerializeField] private GameObject[] arrows;

    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float stepDelay = 0.25f;
    [SerializeField, Min(0f)] private float loopDelay = 0.3f;

    [Header("Settings")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool keepPreviousArrowsOn = true;

    private Coroutine loopCoroutine;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartLoop();
        }
        else
        {
            HideAllArrows();
        }
    }

    public void StartLoop()
    {
        if (!isActiveAndEnabled)
            return;

        StopLoop();

        if (arrows == null || arrows.Length == 0)
        {
            Debug.LogWarning("Assign the arrow GameObjects.", this);
            return;
        }

        loopCoroutine = StartCoroutine(ArrowLoop());
    }

    private IEnumerator ArrowLoop()
    {
        while (true)
        {
            for (int i = 0; i < arrows.Length; i++)
            {
                if (arrows[i] == null)
                    continue;

                if (!keepPreviousArrowsOn)
                {
                    HideAllArrows();
                }

                arrows[i].SetActive(true);

                yield return new WaitForSeconds(
                    Mathf.Max(0.01f, stepDelay)
                );
            }

            HideAllArrows();

            if (loopDelay > 0f)
            {
                yield return new WaitForSeconds(loopDelay);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void StopLoop()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        HideAllArrows();
    }

    private void HideAllArrows()
    {
        if (arrows == null)
            return;

        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] != null)
            {
                arrows[i].SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        StopLoop();
    }
}