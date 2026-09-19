using System.Collections.Generic;
using UnityEngine;

public class MeshWaypointGenerator : MonoBehaviour
{
    [Header("MESH")]
    [Tooltip("Assign your Wire MeshFilter here")]
    public MeshFilter targetMesh;

    [Header("WAYPOINT SETTINGS")]
    [Min(2)]
    public int waypointCount = 20;

    [Tooltip("Parent for generated waypoints. If empty, one will be created.")]
    public Transform waypointParent;

    [Header("POINT SETTINGS")]
    public string waypointName = "Point_";

    [Tooltip("Small offset from mesh center if required")]
    public Vector3 positionOffset;

    [Header("DEBUG")]
    public bool showPath = true;

    private List<Transform> generatedPoints = new List<Transform>();


    // =========================================================
    // GENERATE
    // =========================================================

    [ContextMenu("Generate Waypoints")]
    public void GenerateWaypoints()
    {
        if (targetMesh == null)
        {
            Debug.LogError("Please assign Target Mesh.");
            return;
        }

        Mesh mesh = targetMesh.sharedMesh;

        if (mesh == null)
        {
            Debug.LogError("Target Mesh has no mesh.");
            return;
        }

        ClearWaypoints();

        // Create parent automatically
        if (waypointParent == null)
        {
            GameObject parentObject =
                new GameObject("Generated_Waypoints");

            parentObject.transform.SetParent(
                transform,
                false
            );

            waypointParent = parentObject.transform;
        }

        Vector3[] vertices = mesh.vertices;

        if (vertices.Length < 2)
        {
            Debug.LogError("Mesh does not have enough vertices.");
            return;
        }


        // =====================================================
        // GET WORLD VERTICES
        // =====================================================

        List<Vector3> worldVertices =
            new List<Vector3>();

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldPosition =
                targetMesh.transform.TransformPoint(vertex);

            worldVertices.Add(worldPosition);
        }


        // =====================================================
        // FIND LONGEST MESH AXIS
        // =====================================================

        Bounds bounds =
            targetMesh.GetComponent<Renderer>().bounds;

        Vector3 size = bounds.size;

        int axis;

        if (size.x >= size.y &&
            size.x >= size.z)
        {
            axis = 0;
        }
        else if (size.y >= size.x &&
                 size.y >= size.z)
        {
            axis = 1;
        }
        else
        {
            axis = 2;
        }


        // =====================================================
        // SORT VERTICES ALONG WIRE
        // =====================================================

        worldVertices.Sort(
            (a, b) =>
            GetAxis(a, axis)
            .CompareTo(GetAxis(b, axis))
        );


        // =====================================================
        // DIVIDE MESH INTO SECTIONS
        // =====================================================

        for (int i = 0; i < waypointCount; i++)
        {
            float normalized =
                i / (float)(waypointCount - 1);

            int startIndex =
                Mathf.FloorToInt(
                    normalized *
                    (worldVertices.Count - 1)
                );

            int range =
                Mathf.Max(
                    1,
                    worldVertices.Count /
                    waypointCount
                );

            int endIndex =
                Mathf.Min(
                    worldVertices.Count,
                    startIndex + range
                );


            // =================================================
            // FIND CENTER OF THIS MESH SECTION
            // =================================================

            Vector3 center =
                Vector3.zero;

            int amount = 0;

            for (int j = startIndex;
                 j < endIndex;
                 j++)
            {
                center += worldVertices[j];
                amount++;
            }

            if (amount > 0)
                center /= amount;


            // =================================================
            // CREATE POINT
            // =================================================

            GameObject point =
                new GameObject(
                    waypointName +
                    i.ToString("00")
                );

            point.transform.SetParent(
                waypointParent
            );

            point.transform.position =
                center + positionOffset;

            generatedPoints.Add(
                point.transform
            );
        }


        Debug.Log(
            "Generated " +
            generatedPoints.Count +
            " waypoints on mesh: " +
            targetMesh.name
        );
    }


    // =========================================================
    // CLEAR
    // =========================================================

    [ContextMenu("Clear Waypoints")]
    public void ClearWaypoints()
    {
        generatedPoints.Clear();

        if (waypointParent == null)
            return;

        for (int i =
             waypointParent.childCount - 1;
             i >= 0;
             i--)
        {
            GameObject child =
                waypointParent
                .GetChild(i)
                .gameObject;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child);
            else
                Destroy(child);
#else
            Destroy(child);
#endif
        }
    }


    // =========================================================
    // AXIS
    // =========================================================

    private float GetAxis(
        Vector3 position,
        int axis)
    {
        if (axis == 0)
            return position.x;

        if (axis == 1)
            return position.y;

        return position.z;
    }


    // =========================================================
    // DRAW PATH
    // =========================================================

    private void OnDrawGizmos()
    {
        if (!showPath ||
            waypointParent == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0;
             i < waypointParent.childCount;
             i++)
        {
            Transform point =
                waypointParent.GetChild(i);

            Gizmos.DrawSphere(
                point.position,
                0.01f
            );

            if (i <
                waypointParent.childCount - 1)
            {
                Transform next =
                    waypointParent.GetChild(i + 1);

                Gizmos.DrawLine(
                    point.position,
                    next.position
                );
            }
        }
    }
}