using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps all child LineRenderers hidden at startup.
/// Call PlayReveal() to animate the lines along their paths.
/// </summary>
public sealed class LineRendererDistanceReveal : MonoBehaviour
{
    public enum RevealMode
    {
        AllTogether,
        Staggered
    }

    [Header("Animation")]
    [SerializeField]
    private RevealMode revealMode = RevealMode.Staggered;

    [Tooltip("Time required for each complete line to appear.")]
    [Min(0.01f)]
    [SerializeField]
    private float lineDuration = 1.5f;

    [Tooltip("Delay between each line in Staggered mode.")]
    [Min(0f)]
    [SerializeField]
    private float staggerDelay = 0.06f;

    [SerializeField]
    private AnimationCurve revealCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Playback")]
    [Tooltip("Enable this only when the animation should start automatically.")]
    [SerializeField]
    private bool playOnEnable = false;

    [SerializeField]
    private bool useUnscaledTime = false;

    [Tooltip("Collect LineRenderers from inactive child objects too.")]
    [SerializeField]
    private bool includeInactiveChildren = false;

    private readonly List<LineData> lines = new List<LineData>();

    private Coroutine revealRoutine;
    private bool hasCachedLines;

    private sealed class LineData
    {
        public LineRenderer renderer;
        public Vector3[] positions;
        public float[] distances;
        public float totalDistance;
        public bool originalLoop;
        public bool originalEnabled;
    }

    private void Awake()
    {
        CacheLines();

        // All lines remain completely invisible at startup.
        HideLines();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            PlayReveal();
    }

    /// <summary>
    /// Starts or restarts the complete line-reveal animation.
    /// Connect this method to a Button, UnityEvent, Timeline Signal,
    /// Animation Event or another script.
    /// </summary>
    public void PlayReveal()
    {
        if (!hasCachedLines)
            CacheLines();

        StopCurrentAnimation();
        HideLines();

        revealRoutine = StartCoroutine(RevealRoutine());
    }

    /// <summary>
    /// Recollects all child LineRenderers and starts the animation.
    /// Call this after adding or rebuilding LineRenderers.
    /// </summary>
    public void RefreshAndPlay()
    {
        StopCurrentAnimation();
        CacheLines();
        HideLines();

        revealRoutine = StartCoroutine(RevealRoutine());
    }

    /// <summary>
    /// Immediately hides all collected LineRenderers.
    /// </summary>
    public void HideLines()
    {
        StopCurrentAnimation();

        foreach (LineData line in lines)
        {
            if (line.renderer == null ||
                line.positions == null ||
                line.positions.Length == 0)
            {
                continue;
            }

            line.renderer.loop = false;

            // Collapse the line at its first position.
            line.renderer.positionCount = 2;
            line.renderer.SetPosition(0, line.positions[0]);
            line.renderer.SetPosition(1, line.positions[0]);

            // Disable it so even the starting cap/dot is invisible.
            line.renderer.enabled = false;
        }
    }

    /// <summary>
    /// Immediately displays every complete line without animation.
    /// </summary>
    public void ShowImmediately()
    {
        StopCurrentAnimation();

        if (!hasCachedLines)
            CacheLines();

        foreach (LineData line in lines)
            RestoreCompleteLine(line);
    }

    /// <summary>
    /// Stops the current animation and hides all lines.
    /// </summary>
    public void StopAndHide()
    {
        StopCurrentAnimation();
        HideLines();
    }

    private void CacheLines()
    {
        lines.Clear();

        LineRenderer[] foundLines =
            GetComponentsInChildren<LineRenderer>(
                includeInactiveChildren
            );

        foreach (LineRenderer lineRenderer in foundLines)
        {
            int pointCount = lineRenderer.positionCount;

            if (pointCount < 2)
                continue;

            Vector3[] positions = new Vector3[pointCount];
            lineRenderer.GetPositions(positions);

            float[] distances = new float[pointCount];
            float totalDistance = 0f;

            for (int i = 1; i < pointCount; i++)
            {
                totalDistance += Vector3.Distance(
                    positions[i - 1],
                    positions[i]
                );

                distances[i] = totalDistance;
            }

            lines.Add(new LineData
            {
                renderer = lineRenderer,
                positions = positions,
                distances = distances,
                totalDistance = totalDistance,
                originalLoop = lineRenderer.loop,
                originalEnabled = lineRenderer.enabled
            });
        }

        hasCachedLines = true;
    }

    private IEnumerator RevealRoutine()
    {
        float finalDelay =
            revealMode == RevealMode.Staggered
                ? Mathf.Max(0, lines.Count - 1) * staggerDelay
                : 0f;

        float totalAnimationTime = finalDelay + lineDuration;
        float elapsed = 0f;

        while (elapsed < totalAnimationTime)
        {
            elapsed += useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            for (int i = 0; i < lines.Count; i++)
            {
                float delay =
                    revealMode == RevealMode.Staggered
                        ? i * staggerDelay
                        : 0f;

                float normalizedTime = Mathf.Clamp01(
                    (elapsed - delay) / lineDuration
                );

                float revealAmount = Mathf.Clamp01(
                    revealCurve.Evaluate(normalizedTime)
                );

                DrawPartialLine(lines[i], revealAmount);
            }

            yield return null;
        }

        foreach (LineData line in lines)
            RestoreCompleteLine(line);

        revealRoutine = null;
    }

    private static void DrawPartialLine(
        LineData line,
        float normalizedDistance)
    {
        if (line.renderer == null ||
            line.positions == null ||
            line.positions.Length < 2)
        {
            return;
        }

        // Keep delayed lines completely invisible.
        // This also prevents a small cap/dot at the starting position.
        if (normalizedDistance <= 0f)
        {
            line.renderer.enabled = false;
            return;
        }

        line.renderer.enabled = line.originalEnabled;
        line.renderer.loop = false;

        if (line.totalDistance <= Mathf.Epsilon)
        {
            line.renderer.positionCount = 2;
            line.renderer.SetPosition(0, line.positions[0]);
            line.renderer.SetPosition(1, line.positions[0]);
            return;
        }

        float targetDistance =
            line.totalDistance * normalizedDistance;

        int segment = FindSegment(
            line.distances,
            targetDistance
        );

        int visiblePointCount = Mathf.Clamp(
            segment + 2,
            2,
            line.positions.Length
        );

        line.renderer.positionCount = visiblePointCount;

        for (int i = 0; i <= segment; i++)
            line.renderer.SetPosition(i, line.positions[i]);

        int nextPoint = Mathf.Min(
            segment + 1,
            line.positions.Length - 1
        );

        float segmentStart = line.distances[segment];

        float segmentLength = Mathf.Max(
            Mathf.Epsilon,
            line.distances[nextPoint] - segmentStart
        );

        float segmentProgress = Mathf.Clamp01(
            (targetDistance - segmentStart) / segmentLength
        );

        Vector3 movingEndPoint = Vector3.Lerp(
            line.positions[segment],
            line.positions[nextPoint],
            segmentProgress
        );

        line.renderer.SetPosition(
            visiblePointCount - 1,
            movingEndPoint
        );
    }

    private static int FindSegment(
        float[] cumulativeDistances,
        float targetDistance)
    {
        for (int i = 0;
             i < cumulativeDistances.Length - 1;
             i++)
        {
            if (targetDistance <= cumulativeDistances[i + 1])
                return i;
        }

        return cumulativeDistances.Length - 2;
    }

    private static void RestoreCompleteLine(LineData line)
    {
        if (line.renderer == null ||
            line.positions == null)
        {
            return;
        }

        line.renderer.loop = line.originalLoop;
        line.renderer.positionCount = line.positions.Length;
        line.renderer.SetPositions(line.positions);
        line.renderer.enabled = line.originalEnabled;
    }

    private void StopCurrentAnimation()
    {
        if (revealRoutine == null)
            return;

        StopCoroutine(revealRoutine);
        revealRoutine = null;
    }
}