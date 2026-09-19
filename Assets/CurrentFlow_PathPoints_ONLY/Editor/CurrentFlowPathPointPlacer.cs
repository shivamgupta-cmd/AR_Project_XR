#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CurrentFlowPathPointPlacer
{
    [MenuItem("Tools/Current Flow/Place Existing Path Points On Wire001-003")]
    public static void Place()
    {
        GameObject root = Selection.activeGameObject;
        if (!root)
        {
            EditorUtility.DisplayDialog("Current Flow Path", "Select the Electric Switch Final root in the Hierarchy.", "OK");
            return;
        }

        Transform fx = Find(root.transform, "CURRENT_FLOW_FX");
        if (!fx)
        {
            EditorUtility.DisplayDialog("Current Flow Path", "CURRENT_FLOW_FX was not found under the selected setup.", "OK");
            return;
        }

        Transform[] wires = {
            Find(root.transform, "Wire001"),
            Find(root.transform, "Wire002"),
            Find(root.transform, "Wire003")
        };

        if (wires.Any(w => !w))
        {
            EditorUtility.DisplayDialog("Current Flow Path",
                "Wire001, Wire002 and Wire003 must all exist in the selected setup.", "OK");
            return;
        }

        // Use only the CURRENT_FLOW_FX's already-existing point transforms.
        List<Transform> points = fx.GetComponentsInChildren<Transform>(true)
            .Where(t => t != fx && (t.name.StartsWith("P") || t.name.ToLower().Contains("point")))
            .OrderBy(t => ExtractNumber(t.name)).ToList();

        if (points.Count < 2)
        {
            EditorUtility.DisplayDialog("Current Flow Path",
                "No existing path points were found inside CURRENT_FLOW_FX. This tool does not create a new FX.", "OK");
            return;
        }

        List<Vector3> path = new List<Vector3>();
        foreach (Transform wire in wires)
        {
            List<Vector3> wirePath = ExtractWireCenterline(wire);
            if (wirePath.Count == 0) continue;

            // Orient this wire section so it connects to the previous section.
            if (path.Count > 0)
            {
                float dStart = Vector3.Distance(path[path.Count-1], wirePath[0]);
                float dEnd = Vector3.Distance(path[path.Count-1], wirePath[wirePath.Count-1]);
                if (dEnd < dStart) wirePath.Reverse();
            }
            path.AddRange(wirePath);
        }

        if (path.Count < 2)
        {
            EditorUtility.DisplayDialog("Current Flow Path",
                "Could not read enough mesh/renderer geometry from the three wires.", "OK");
            return;
        }

        path = RemoveNearDuplicates(path, 0.002f);
        for (int i=0;i<points.Count;i++)
        {
            float t = points.Count == 1 ? 0 : i/(float)(points.Count-1);
            Vector3 p = SamplePolyline(path, t);
            Undo.RecordObject(points[i], "Place Current Flow Path Point");
            points[i].position = p;
            EditorUtility.SetDirty(points[i]);
        }

        EditorUtility.DisplayDialog("Current Flow Path",
            "Done. Only the existing CURRENT_FLOW_FX path points were repositioned over Wire001 → Wire002 → Wire003.", "OK");
    }

    static List<Vector3> ExtractWireCenterline(Transform wire)
    {
        var samples = new List<Vector3>();

        foreach (var mf in wire.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh) continue;
            var verts = mf.sharedMesh.vertices;
            if (verts == null || verts.Length == 0) continue;

            // Convert mesh vertices to world space.
            var world = verts.Select(v => mf.transform.TransformPoint(v)).ToList();

            // Find the dominant axis from renderer bounds and bin vertices along it.
            Bounds b = new Bounds(world[0], Vector3.zero);
            foreach(var v in world) b.Encapsulate(v);
            Vector3 size=b.size;
            int axis = size.x >= size.y && size.x >= size.z ? 0 : (size.y >= size.z ? 1 : 2);

            world.Sort((a,c)=>Axis(a,axis).CompareTo(Axis(c,axis)));
            int bins = Mathf.Clamp(Mathf.RoundToInt(world.Count/40f), 6, 30);
            for(int i=0;i<bins;i++)
            {
                int a=Mathf.FloorToInt(i*world.Count/(float)bins);
                int z=Mathf.Min(world.Count, Mathf.FloorToInt((i+1)*world.Count/(float)bins));
                if(z<=a) continue;
                Vector3 sum=Vector3.zero;
                for(int j=a;j<z;j++) sum+=world[j];
                samples.Add(sum/(z-a));
            }
        }

        // Fallback to renderer bounds centers.
        if(samples.Count < 2)
        {
            foreach(var r in wire.GetComponentsInChildren<Renderer>(true))
                samples.Add(r.bounds.center);
        }

        return samples;
    }

    static float Axis(Vector3 v,int a)=>a==0?v.x:(a==1?v.y:v.z);

    static Vector3 SamplePolyline(List<Vector3> p,float t)
    {
        if(p.Count==1)return p[0];
        float total=0;
        float[] seg=new float[p.Count-1];
        for(int i=0;i<seg.Length;i++){seg[i]=Vector3.Distance(p[i],p[i+1]);total+=seg[i];}
        if(total<=0)return p[0];
        float target=t*total, run=0;
        for(int i=0;i<seg.Length;i++)
        {
            if(run+seg[i]>=target)
            {
                float u=seg[i]<=0?0:(target-run)/seg[i];
                return Vector3.Lerp(p[i],p[i+1],u);
            }
            run+=seg[i];
        }
        return p[p.Count-1];
    }

    static List<Vector3> RemoveNearDuplicates(List<Vector3> p,float eps)
    {
        var r=new List<Vector3>();
        foreach(var v in p)
            if(r.Count==0 || Vector3.Distance(r[r.Count-1],v)>eps) r.Add(v);
        return r;
    }

    static Transform Find(Transform root,string name)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
            if(t.name.Equals(name,System.StringComparison.OrdinalIgnoreCase)) return t;
        return null;
    }

    static int ExtractNumber(string s)
    {
        string n=new string(s.Where(char.IsDigit).ToArray());
        return int.TryParse(n,out int v)?v:int.MaxValue;
    }
}
#endif