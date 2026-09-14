using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class InfinityTubeMesh : MonoBehaviour
{
    [Header("Infinity Shape")]
    public float width = 3f;
    public float height = 2f;

    [Header("Tube")]
    public float tubeRadius = 0.18f;

    [Header("Quality")]
    public int pathSegments = 200;
    public int tubeSegments = 16;

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices =
            new Vector3[pathSegments * tubeSegments];

        Vector3[] normals =
            new Vector3[pathSegments * tubeSegments];

        Vector2[] uv =
            new Vector2[pathSegments * tubeSegments];

        int[] triangles =
            new int[pathSegments * tubeSegments * 6];

        // -------------------------------
        // Generate vertices
        // -------------------------------

        for (int i = 0; i < pathSegments; i++)
        {
            float t =
                (float)i / pathSegments *
                Mathf.PI * 2f;

            Vector3 center = GetInfinityPoint(t);

            Vector3 tangent =
                GetInfinityTangent(t).normalized;

            // Since the infinity is lying on XZ plane
            Vector3 up = Vector3.up;

            Vector3 side =
                Vector3.Cross(up, tangent).normalized;

            Vector3 normal =
                Vector3.Cross(tangent, side).normalized;

            for (int j = 0; j < tubeSegments; j++)
            {
                float angle =
                    (float)j / tubeSegments *
                    Mathf.PI * 2f;

                Vector3 offset =
                    side * Mathf.Cos(angle) * tubeRadius +
                    normal * Mathf.Sin(angle) * tubeRadius;

                int index =
                    i * tubeSegments + j;

                vertices[index] =
                    center + offset;

                normals[index] =
                    offset.normalized;

                uv[index] =
                    new Vector2(
                        (float)i / pathSegments,
                        (float)j / tubeSegments
                    );
            }
        }

        // -------------------------------
        // Generate triangles
        // -------------------------------

        int triangleIndex = 0;

        for (int i = 0; i < pathSegments; i++)
        {
            int next =
                (i + 1) % pathSegments;

            for (int j = 0; j < tubeSegments; j++)
            {
                int nextTube =
                    (j + 1) % tubeSegments;

                int a =
                    i * tubeSegments + j;

                int b =
                    next * tubeSegments + j;

                int c =
                    next * tubeSegments + nextTube;

                int d =
                    i * tubeSegments + nextTube;

                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = b;
                triangles[triangleIndex++] = c;

                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // -------------------------------
    // Infinity position
    // -------------------------------

    Vector3 GetInfinityPoint(float t)
    {
        float x =
            width * Mathf.Sin(t);

        float z =
            height * Mathf.Sin(2f * t);

        return new Vector3(x, 0f, z);
    }

    // -------------------------------
    // Infinity tangent
    // -------------------------------

    Vector3 GetInfinityTangent(float t)
    {
        float dx =
            width * Mathf.Cos(t);

        float dz =
            height * 2f * Mathf.Cos(2f * t);

        return new Vector3(
            dx,
            0f,
            dz
        );
    }
}