using System.Collections;
using UnityEngine;

public class JCBController : MonoBehaviour
{
    [Header("JCB PARTS")]
    [SerializeField] private Transform m_jcbBackPart1;
    [SerializeField] private Transform m_jcbBackPart2;

    [Header("STONE PARTICLE")]
    [SerializeField] private ParticleSystem m_stoneParticle;

    [Header("ROTATION DURATION")]
    [SerializeField, Min(0.01f)] private float part1RotationDuration = 1.5f;
    [SerializeField, Min(0.01f)] private float part2RotationDuration = 1.5f;

    [Header("WAIT")]
    [SerializeField] private float waitAfterDrop = 0.5f;
    [SerializeField] private float waitBeforeNextLoop = 0.5f;

    private Coroutine workCoroutine;

    private void OnEnable()
    {
        if (m_jcbBackPart1 == null || m_jcbBackPart2 == null)
            return;

        m_jcbBackPart1.localRotation = Quaternion.identity;
        m_jcbBackPart2.localRotation = Quaternion.identity;

        if (m_stoneParticle != null)
            m_stoneParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        workCoroutine = StartCoroutine(WorkLoop());
    }

    private IEnumerator WorkLoop()
    {
        while (true)
        {
            yield return RotatePart(m_jcbBackPart1, Vector3.zero, part1RotationDuration);
            yield return RotatePart(m_jcbBackPart2, Vector3.zero, part2RotationDuration);
            
            yield return RotatePart(m_jcbBackPart1, new Vector3(0f, -90f, 0f), part1RotationDuration);

            if (m_stoneParticle != null)
            {
                m_stoneParticle.Clear(true);
                m_stoneParticle.Play(true);
            }

            yield return RotatePart(m_jcbBackPart2, new Vector3(90f, 0f, 0f), part2RotationDuration);

            if (m_stoneParticle != null)
                m_stoneParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (waitAfterDrop > 0f)
                yield return new WaitForSeconds(waitAfterDrop);

            if (m_stoneParticle != null)
                m_stoneParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);


            if (waitBeforeNextLoop > 0f)
                yield return new WaitForSeconds(waitBeforeNextLoop);
        }
    }

    private IEnumerator RotatePart(Transform target, Vector3 targetEulerAngles, float duration)
    {
        if (target == null)
            yield break;

        Quaternion startRotation = target.localRotation;
        Quaternion endRotation = Quaternion.Euler(targetEulerAngles);

        float elapsedTime = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            target.localRotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        target.localRotation = endRotation;
    }

    private void OnDisable()
    {
        if (workCoroutine != null)
        {
            StopCoroutine(workCoroutine);
            workCoroutine = null;
        }

        if (m_stoneParticle != null)
            m_stoneParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}