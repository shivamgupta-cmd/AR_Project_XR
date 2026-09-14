using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CopperDepositFX
{
    [DisallowMultipleComponent]
    public class CopperDepositEffect : MonoBehaviour
    {
        public enum LocalAxis { X, Y, Z }

        [Header("Target Surface")]
        [Tooltip("Renderer of the nail mesh. MeshFilter or SkinnedMeshRenderer is supported.")]
        public Renderer targetRenderer;
        public bool autoDetectAxis = true;
        public LocalAxis nailAxis = LocalAxis.Y;

        [Header("Deposit Area")]
        [Range(0f, 1f)] public float startNormalized = 0.00f;
        [Range(0f, 1f)] public float endNormalized = 0.72f;
        public bool reverseDirection = false;
        [Tooltip("Small offset OUTSIDE the real mesh surface. 0 = exactly on surface.")]
        [Range(0f, 0.08f)] public float surfaceOffset = 0.006f;

        [Header("Deposit Look")]
        [Range(20, 400)] public int blobCount = 140;

        [Range(0.002f, 0.15f)]
        public float minBlobSize = 0.018f;

        [Range(0.002f, 0.20f)]
        public float maxBlobSize = 0.045f;

        [Tooltip("Flattens blobs against the nail surface. Smaller = flatter coating.")]
        [Range(0.1f, 1f)]
        public float normalThickness = 0.35f;

        [Header("Copper Material")]
        public Material copperMaterial;

        [Header("Animation")]
        public bool playOnStart = false;
        [Min(0.1f)] public float depositionDuration = 5f;
        [Range(0f, 3.0f)] public float randomStartDelay = 0.06f;
        public AnimationCurve growCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Optional Reaction Sparkles")]
        public bool createSparkles = false;
        [Range(1, 30)] public int sparkleRate = 6;
        [Range(0.005f, 0.05f)] public float sparkleSize = 0.015f;

        [Header("Runtime")]
        [SerializeField, Range(0f, 1f)] private float progress = 0f;

        public float Progress { get { return progress; } }
        public bool IsDepositing { get; private set; }

        private readonly List<Transform> blobs = new List<Transform>();
        private readonly List<float> blobStart = new List<float>();
       
        private Transform depositRoot;
        private ParticleSystem sparkleSystem;
        private Coroutine routine;
        private LocalAxis detectedAxis;
        private Bounds meshBounds;
        private Mesh samplingMesh;
        private Transform meshTransform;
        private bool ownsBakedMesh;
        private float[] cumulativeAreas;
        private float totalTriangleArea;

        private void Awake()
        {
            EnsureBuilt();
            ApplyProgress(progress);
        }

        private void Start()
        {
            if (playOnStart) StartDeposition();
        }

        [ContextMenu("Build / Rebuild Deposit")]
        public void RebuildDeposit()
        {
            ClearGenerated();
            EnsureBuilt();
            ApplyProgress(progress);
        }

        [ContextMenu("Start Deposition")]
        public void StartDeposition()
        {
            EnsureBuilt();
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(AnimateDeposit(progress, 1f));
        }

        public void StartDeposition(float duration)
        {
            depositionDuration = Mathf.Max(0.1f, duration);
            StartDeposition();
        }

        [ContextMenu("Reset Deposit")]
        public void ResetDeposit()
        {
            EnsureBuilt();
            if (routine != null) StopCoroutine(routine);
            IsDepositing = false;
            progress = 0f;
            ApplyProgress(progress);
        }

        [ContextMenu("Complete Deposit")]
        public void CompleteDeposit()
        {
            EnsureBuilt();
            if (routine != null) StopCoroutine(routine);
            IsDepositing = false;
            progress = 1f;
            ApplyProgress(progress);
        }

        public void SetProgress(float value)
        {
            EnsureBuilt();
            progress = Mathf.Clamp01(value);
            ApplyProgress(progress);
        }

        private IEnumerator AnimateDeposit(float from, float to)
        {
            IsDepositing = true;
            if (sparkleSystem != null) sparkleSystem.Play();
            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, depositionDuration * Mathf.Abs(to - from));
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                progress = Mathf.Lerp(from, to, t);
                ApplyProgress(progress);
                yield return null;
            }
            progress = to;
            ApplyProgress(progress);
            if (sparkleSystem != null) sparkleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            IsDepositing = false;
            routine = null;
        }

        private void EnsureBuilt()
        {
            if (depositRoot != null && blobs.Count > 0) return;

            if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogWarning("CopperDepositEffect: No Renderer found on the nail.", this);
                return;
            }

            if (!PrepareSamplingMesh())
            {
                Debug.LogError("CopperDepositEffect: Nail renderer needs a readable MeshFilter mesh or a SkinnedMeshRenderer.", this);
                return;
            }

            detectedAxis = autoDetectAxis ? DetectLongestAxis(meshBounds.size) : nailAxis;
            BuildTriangleAreaTable();
          

            GameObject root = new GameObject("CopperDeposit_Generated");
            root.transform.SetParent(transform, false);
            depositRoot = root.transform;

            Random.State oldState = Random.state;
            Random.InitState(GetInstanceID());

            int made = 0;
            int attempts = 0;
            int maxAttempts = Mathf.Max(blobCount * 25, 500);

            while (made < blobCount && attempts < maxAttempts)
            {
                attempts++;
                Vector3 localPoint, localNormal;
                float axis01;
                if (!TrySampleMeshSurface(out localPoint, out localNormal, out axis01)) continue;

                float test = reverseDirection ? 1f - axis01 : axis01;
                float lo = Mathf.Min(startNormalized, endNormalized);
                float hi = Mathf.Max(startNormalized, endNormalized);
                if (test < lo || test > hi) continue;

                Vector3 worldPoint = meshTransform.TransformPoint(localPoint);
                Vector3 worldNormal = meshTransform.TransformDirection(localNormal).normalized;
                Vector3 rootPoint = transform.InverseTransformPoint(worldPoint + worldNormal * surfaceOffset);
                Vector3 rootNormal = transform.InverseTransformDirection(worldNormal).normalized;

                GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blob.name = "CopperBlob_" + made.ToString("000");
                blob.transform.SetParent(depositRoot, false);
                Collider c = blob.GetComponent<Collider>();
                if (c != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) DestroyImmediate(c); else Destroy(c);
#else
                    Destroy(c);
#endif
                }

                Renderer r = blob.GetComponent<Renderer>();

                if (r != null && copperMaterial != null)
                {
                    r.sharedMaterial = copperMaterial;
                }
                blob.transform.localPosition = rootPoint;
                blob.transform.localRotation = Quaternion.FromToRotation(Vector3.up, rootNormal) * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                float size = Random.Range(minBlobSize, maxBlobSize) * CrossSectionScale();
                float tangentStretch = Random.Range(0.75f, 1.4f);
                // Local Y is aligned with the surface normal; flatten Y to make deposit hug the nail.
                blob.transform.localScale = new Vector3(size * tangentStretch, size * normalThickness, size);

                blobs.Add(blob.transform);
                float denom = Mathf.Max(0.0001f, hi - lo);
                blobStart.Add(Mathf.Clamp01((test - lo) / denom));
                made++;
            }

            Random.state = oldState;

            if (made < blobCount)
                Debug.LogWarning("CopperDepositEffect: Could only place " + made + " of " + blobCount + " blobs. Widen Deposit Area if needed.", this);

            if (createSparkles) BuildSparkles();
        }

        private bool PrepareSamplingMesh()
        {
            ownsBakedMesh = false;
            MeshFilter mf = targetRenderer.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                samplingMesh = mf.sharedMesh;
                meshTransform = mf.transform;
                meshBounds = samplingMesh.bounds;
                return samplingMesh.isReadable;
            }

            SkinnedMeshRenderer smr = targetRenderer as SkinnedMeshRenderer;
            if (smr == null) smr = targetRenderer.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
            {
                samplingMesh = new Mesh();
                smr.BakeMesh(samplingMesh);
                ownsBakedMesh = true;
                meshTransform = smr.transform;
                meshBounds = samplingMesh.bounds;
                return true;
            }
            return false;
        }

        private void BuildTriangleAreaTable()
        {
            int[] tris = samplingMesh.triangles;
            Vector3[] verts = samplingMesh.vertices;
            int triCount = tris.Length / 3;
            cumulativeAreas = new float[triCount];
            totalTriangleArea = 0f;
            for (int i = 0; i < triCount; i++)
            {
                Vector3 a = verts[tris[i * 3]];
                Vector3 b = verts[tris[i * 3 + 1]];
                Vector3 c = verts[tris[i * 3 + 2]];
                float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                totalTriangleArea += Mathf.Max(area, 0.0000001f);
                cumulativeAreas[i] = totalTriangleArea;
            }
        }

        private bool TrySampleMeshSurface(out Vector3 point, out Vector3 normal, out float axis01)
        {
            point = Vector3.zero;
            normal = Vector3.up;
            axis01 = 0f;
            if (samplingMesh == null || totalTriangleArea <= 0f) return false;

            int[] tris = samplingMesh.triangles;
            Vector3[] verts = samplingMesh.vertices;
            Vector3[] norms = samplingMesh.normals;

            float pick = Random.value * totalTriangleArea;
            int tri = System.Array.BinarySearch(cumulativeAreas, pick);
            if (tri < 0) tri = ~tri;
            tri = Mathf.Clamp(tri, 0, cumulativeAreas.Length - 1);

            int i0 = tris[tri * 3];
            int i1 = tris[tri * 3 + 1];
            int i2 = tris[tri * 3 + 2];

            float r1 = Mathf.Sqrt(Random.value);
            float r2 = Random.value;
            float w0 = 1f - r1;
            float w1 = r1 * (1f - r2);
            float w2 = r1 * r2;

            point = verts[i0] * w0 + verts[i1] * w1 + verts[i2] * w2;

            if (norms != null && norms.Length == verts.Length)
                normal = (norms[i0] * w0 + norms[i1] * w1 + norms[i2] * w2).normalized;
            else
                normal = Vector3.Cross(verts[i1] - verts[i0], verts[i2] - verts[i0]).normalized;

            axis01 = NormalizedAxis(point);
            return true;
        }

        private float NormalizedAxis(Vector3 p)
        {
            if (detectedAxis == LocalAxis.X) return Mathf.InverseLerp(meshBounds.min.x, meshBounds.max.x, p.x);
            if (detectedAxis == LocalAxis.Y) return Mathf.InverseLerp(meshBounds.min.y, meshBounds.max.y, p.y);
            return Mathf.InverseLerp(meshBounds.min.z, meshBounds.max.z, p.z);
        }

        private LocalAxis DetectLongestAxis(Vector3 s)
        {
            if (s.x >= s.y && s.x >= s.z) return LocalAxis.X;
            if (s.y >= s.x && s.y >= s.z) return LocalAxis.Y;
            return LocalAxis.Z;
        }

        private float CrossSectionScale()
        {
            Vector3 s = meshBounds.size;
            if (detectedAxis == LocalAxis.X) return Mathf.Max(0.001f, (s.y + s.z) * 0.5f);
            if (detectedAxis == LocalAxis.Y) return Mathf.Max(0.001f, (s.x + s.z) * 0.5f);
            return Mathf.Max(0.001f, (s.x + s.y) * 0.5f);
        }

        private void ApplyProgress(float p)
        {
            for (int i = 0; i < blobs.Count; i++)
            {
                Transform b = blobs[i];
                if (b == null) continue;
                float start = blobStart[i] * 0.85f + RandomizedDelay01(i);
                float localT = Mathf.InverseLerp(start, Mathf.Min(1f, start + 0.15f), p);
                float s = growCurve.Evaluate(localT);
                b.gameObject.SetActive(s > 0.001f);
                if (s > 0.001f)
                {
                    CopperBlobScale holder = b.GetComponent<CopperBlobScale>();
                    if (holder == null)
                    {
                        holder = b.gameObject.AddComponent<CopperBlobScale>();
                        holder.originalScale = b.localScale;
                    }
                    b.localScale = holder.originalScale * Mathf.Max(0.001f, s);
                }
            }

            if (sparkleSystem != null)
            {
                var emission = sparkleSystem.emission;
                emission.rateOverTime = IsDepositing ? sparkleRate : 0f;
            }
        }

        private float RandomizedDelay01(int i)
        {
            int v = (i * 1103515245 + 12345) & 0x7fffffff;
            return ((v % 1000) / 1000f) * randomStartDelay;
        }

       

        private void BuildSparkles()
        {
            GameObject go = new GameObject("CopperReactionSparkles");
            go.transform.SetParent(depositRoot, false);
            sparkleSystem = go.AddComponent<ParticleSystem>();
            var main = sparkleSystem.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
            main.startSize = new ParticleSystem.MinMaxCurve(sparkleSize * 0.5f, sparkleSize);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.45f, 0.08f, 1f), new Color(1f, 0.78f, 0.25f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 96;
            var emission = sparkleSystem.emission;
            emission.rateOverTime = 0f;
            var shape = sparkleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
            shape.meshRenderer = targetRenderer as MeshRenderer;
            var psr = sparkleSystem.GetComponent<ParticleSystemRenderer>();
            if (psr != null) psr.sharedMaterial = copperMaterial;
        }

        private void ClearGenerated()
        {
            blobs.Clear();
            blobStart.Clear();
            sparkleSystem = null;
            Transform old = transform.Find("CopperDeposit_Generated");
            if (old != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(old.gameObject); else Destroy(old.gameObject);
#else
                Destroy(old.gameObject);
#endif
            }
            depositRoot = null;
            if (ownsBakedMesh && samplingMesh != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(samplingMesh); else Destroy(samplingMesh);
#else
                Destroy(samplingMesh);
#endif
            }
            samplingMesh = null;
            cumulativeAreas = null;
            totalTriangleArea = 0f;
        }

        private void OnDestroy()
        {
           
            if (ownsBakedMesh && samplingMesh != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(samplingMesh); else Destroy(samplingMesh);
#else
                Destroy(samplingMesh);
#endif
            }
        }
    }

    [AddComponentMenu("")]
    public class CopperBlobScale : MonoBehaviour
    {
        [HideInInspector] public Vector3 originalScale = Vector3.one;
    }
}
