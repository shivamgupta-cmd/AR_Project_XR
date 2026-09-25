using UnityEngine;

public class RowGlowHighlighter : MonoBehaviour
{
    [Header("Assign the parent of each row")]
    public GameObject[] rows;

    [Header("URP Glow Material")]
    public Material outlineMaterial;

    [Header("Outline Shape")]
    public float horizontalPadding = 0.08f;
    public float verticalPadding = 0.08f;
    public float cornerRadius = 0.12f;
    public float frontOffset = 0.05f;
    public float lineWidth = 0.025f;

    [Header("Visibility")]
    public bool highlightAllOnStart = true;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 2f;

    [Header("Blink")]
    public bool blink = false;
    public float blinkSpeed = 2f;

    [Range(0f, 1f)]
    public float minimumBlinkBrightness = 0.05f;

    private LineRenderer[][] outlines;

    private void Start()
    {
        if (outlineMaterial == null)
        {
            Debug.LogError(
                "Assign a URP glow material to Outline Material.",
                this
            );
            enabled = false;
            return;
        }

        if (rows == null || rows.Length == 0)
        {
            Debug.LogWarning("Assign at least one row.", this);
            enabled = false;
            return;
        }

        outlines = new LineRenderer[rows.Length][];

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null)
                continue;

            Vector3 size;
            float frontZ;

            if (!GetRowSize(rows[i].transform, out size, out frontZ))
            {
                Debug.LogWarning(
                    "No MeshFilters found in row: " + rows[i].name,
                    rows[i]
                );
                continue;
            }

            outlines[i] = new LineRenderer[3];

            outlines[i][0] = CreateLine(
                rows[i].transform,
                "Outer Glow",
                lineWidth * 6f
            );

            outlines[i][1] = CreateLine(
                rows[i].transform,
                "Inner Glow",
                lineWidth * 3f
            );

            outlines[i][2] = CreateLine(
                rows[i].transform,
                "Bright Outline",
                lineWidth
            );

            Vector3[] points = CreateRoundedRectangle(
                size.x + horizontalPadding * 2f,
                size.y + verticalPadding * 2f,
                frontZ - frontOffset
            );

            foreach (LineRenderer line in outlines[i])
            {
                line.positionCount = points.Length;
                line.SetPositions(points);
                line.loop = true;
                line.gameObject.SetActive(highlightAllOnStart);
            }

            SetRowBrightness(outlines[i], 1f);
        }
    }

    private void Update()
    {
        if (outlines == null)
            return;

        float brightness = 1f;

        if (pulse)
        {
            brightness *=
                0.75f +
                0.25f * Mathf.Sin(Time.time * pulseSpeed);
        }

        if (blink)
        {
            float blinkAmount = Mathf.PingPong(
                Time.time * blinkSpeed * 2f,
                1f
            );

            brightness *= Mathf.Lerp(
                minimumBlinkBrightness,
                1f,
                blinkAmount
            );
        }

        foreach (LineRenderer[] rowLines in outlines)
        {
            if (rowLines != null)
                SetRowBrightness(rowLines, brightness);
        }
    }

    private LineRenderer CreateLine(
        Transform parent,
        string objectName,
        float width)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(parent, false);

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        // Outline positions are relative to the row parent.
        line.useWorldSpace = false;

        // Keeps the line in the parent's local XY plane.
        line.alignment = LineAlignment.TransformZ;

        // The material controls the color.
        line.sharedMaterial = outlineMaterial;

        line.widthMultiplier = width;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows = false;

        return line;
    }

    private bool GetRowSize(
        Transform parent,
        out Vector3 size,
        out float frontZ)
    {
        MeshFilter[] meshes =
            parent.GetComponentsInChildren<MeshFilter>();

        float maxX = 0f;
        float maxY = 0f;
        float minZ = float.MaxValue;
        bool foundMesh = false;

        foreach (MeshFilter meshFilter in meshes)
        {
            if (meshFilter.sharedMesh == null)
                continue;

            Bounds bounds = meshFilter.sharedMesh.bounds;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner =
                            bounds.center +
                            Vector3.Scale(
                                bounds.extents,
                                new Vector3(x, y, z)
                            );

                        Vector3 localPoint =
                            parent.InverseTransformPoint(
                                meshFilter.transform.TransformPoint(
                                    corner
                                )
                            );

                        maxX = Mathf.Max(
                            maxX,
                            Mathf.Abs(localPoint.x)
                        );

                        maxY = Mathf.Max(
                            maxY,
                            Mathf.Abs(localPoint.y)
                        );

                        minZ = Mathf.Min(
                            minZ,
                            localPoint.z
                        );

                        foundMesh = true;
                    }
                }
            }
        }

        size = new Vector3(
            maxX * 2f,
            maxY * 2f,
            0f
        );

        frontZ = minZ;
        return foundMesh;
    }

    private Vector3[] CreateRoundedRectangle(
        float width,
        float height,
        float z)
    {
        const int segmentsPerCorner = 8;

        Vector3[] points =
            new Vector3[segmentsPerCorner * 4];

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        float radius = Mathf.Min(
            cornerRadius,
            halfWidth,
            halfHeight
        );

        int index = 0;

        AddCorner(
            points, ref index,
            halfWidth - radius,
            halfHeight - radius,
            radius, 0f, 90f, z
        );

        AddCorner(
            points, ref index,
            -halfWidth + radius,
            halfHeight - radius,
            radius, 90f, 180f, z
        );

        AddCorner(
            points, ref index,
            -halfWidth + radius,
            -halfHeight + radius,
            radius, 180f, 270f, z
        );

        AddCorner(
            points, ref index,
            halfWidth - radius,
            -halfHeight + radius,
            radius, 270f, 360f, z
        );

        return points;
    }

    private void AddCorner(
        Vector3[] points,
        ref int index,
        float centerX,
        float centerY,
        float radius,
        float startAngle,
        float endAngle,
        float z)
    {
        const int segments = 8;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Lerp(
                startAngle,
                endAngle,
                i / (float)(segments - 1)
            ) * Mathf.Deg2Rad;

            points[index++] = new Vector3(
                centerX + Mathf.Cos(angle) * radius,
                centerY + Mathf.Sin(angle) * radius,
                z
            );
        }
    }

    private void SetRowBrightness(
        LineRenderer[] rowLines,
        float brightness)
    {
        SetLineOpacity(rowLines[0], 0.12f * brightness);
        SetLineOpacity(rowLines[1], 0.35f * brightness);
        SetLineOpacity(rowLines[2], brightness);
    }

    private void SetLineOpacity(
        LineRenderer line,
        float alpha)
    {
        // White keeps the color selected on outlineMaterial.
        Color tint = new Color(
            1f,
            1f,
            1f,
            Mathf.Clamp01(alpha)
        );

        line.startColor = tint;
        line.endColor = tint;
    }

    public void HighlightRow(int index)
    {
        HideAll();

        if (outlines == null ||
            index < 0 ||
            index >= outlines.Length ||
            outlines[index] == null)
        {
            return;
        }

        foreach (LineRenderer line in outlines[index])
            line.gameObject.SetActive(true);
    }

    public void HighlightAll()
    {
        if (outlines == null)
            return;

        foreach (LineRenderer[] rowLines in outlines)
        {
            if (rowLines == null)
                continue;

            foreach (LineRenderer line in rowLines)
                line.gameObject.SetActive(true);
        }
    }

    public void HideAll()
    {
        if (outlines == null)
            return;

        foreach (LineRenderer[] rowLines in outlines)
        {
            if (rowLines == null)
                continue;

            foreach (LineRenderer line in rowLines)
                line.gameObject.SetActive(false);
        }
    }
}