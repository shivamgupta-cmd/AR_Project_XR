using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CompassLearning.SunVFX
{
    /// <summary>
    /// Creates several randomly rotated annulus planes around a Sun to produce
    /// a lightweight volumetric prominence/corona effect without third-party plugins.
    /// Designed for Unity 2022 URP and mobile AR.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class ARVolumetricSunProminence : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Material sourceMaterial;
        [SerializeField] private Texture2D prominenceTexture;
        [ColorUsage(true, true)]
        [SerializeField] private Color color = new Color(1.6f, 0.28f, 0.025f, 1f);
        [Min(0f)] [SerializeField] private float brightness = 0.91f;
        [Range(0f, 1f)] [SerializeField] private float opacity = 0.85f;

        [Header("Volumetric Geometry")]
        [SerializeField] private int seed = 1082336596;
        [Range(1, 16)] [SerializeField] private int planeCount = 10;
        [Range(3, 64)] [SerializeField] private int planeDetail = 20;
        [Min(0.001f)] [SerializeField] private float radiusMin = 1f;
        [Min(0.002f)] [SerializeField] private float radiusMax = 1.5f;

        [Header("Animation")]
        [SerializeField] private Vector2 flowSpeed = new Vector2(0.015f, 0.08f);
        [Range(0f, 0.25f)] [SerializeField] private float distortionAmount = 0.04f;
        [Range(0f, 5f)] [SerializeField] private float distortionSpeed = 0.35f;
        [Range(0.1f, 8f)] [SerializeField] private float flameContrast = 1.15f;

        [Header("Plane Blending")]
        [Range(0.1f, 8f)] [SerializeField] private float fadePower = 1f;
        [Range(0.1f, 8f)] [SerializeField] private float clipPower = 2f;
        [Range(0.1f, 8f)] [SerializeField] private float outerFadePower = 1.25f;

        [Header("Render Position")]
        [Tooltip("Negative values move the effect slightly behind the Sun.")]
        [SerializeField] private float cameraOffset = -0.01f;
        [SerializeField] private Camera targetCamera;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh generatedMesh;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 baseLocalPosition;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int CenterWsId = Shader.PropertyToID("_CenterWS");
        private static readonly int RadiusMaxId = Shader.PropertyToID("_RadiusMax");
        private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
        private static readonly int DistortionAmountId = Shader.PropertyToID("_DistortionAmount");
        private static readonly int DistortionSpeedId = Shader.PropertyToID("_DistortionSpeed");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int FadePowerId = Shader.PropertyToID("_FadePower");
        private static readonly int ClipPowerId = Shader.PropertyToID("_ClipPower");
        private static readonly int OuterFadePowerId = Shader.PropertyToID("_OuterFadePower");

        public float RadiusMin => radiusMin;
        public float RadiusMax => radiusMax;

        private void OnEnable()
        {
            baseLocalPosition = transform.localPosition;
            CacheComponents();
            RebuildMesh();
            ApplyMaterialProperties();
        }

        private void LateUpdate()
        {
            ApplyCameraOffset();
            ApplyMaterialProperties();
        }

        private void OnValidate()
        {
            planeCount = Mathf.Clamp(planeCount, 1, 16);
            planeDetail = Mathf.Clamp(planeDetail, 3, 64);
            radiusMin = Mathf.Max(0.001f, radiusMin);
            radiusMax = Mathf.Max(radiusMin + 0.001f, radiusMax);

            if (!isActiveAndEnabled)
                return;

            CacheComponents();
            RebuildMesh();
            ApplyMaterialProperties();
        }

        public void Configure(
            Material material,
            Texture2D texture,
            float innerRadius,
            float outerRadius)
        {
            sourceMaterial = material;
            prominenceTexture = texture;
            radiusMin = Mathf.Max(0.001f, innerRadius);
            radiusMax = Mathf.Max(radiusMin + 0.001f, outerRadius);
            baseLocalPosition = transform.localPosition;
            RebuildMesh();
            ApplyMaterialProperties();
        }

        [ContextMenu("Rebuild Volumetric Prominence")]
        public void RebuildMesh()
        {
            CacheComponents();

            if (generatedMesh == null)
            {
                generatedMesh = new Mesh
                {
                    name = "Generated AR Volumetric Sun Prominence"
                };
                generatedMesh.MarkDynamic();
            }
            else
            {
                generatedMesh.Clear();
            }

            int verticesPerPlane = (planeDetail + 1) * 2;
            int totalVertices = verticesPerPlane * planeCount;
            int totalIndices = planeDetail * 6 * planeCount;

            List<Vector3> positions = new List<Vector3>(totalVertices);
            List<Vector3> normals = new List<Vector3>(totalVertices);
            List<Vector2> uv0 = new List<Vector2>(totalVertices);
            List<int> indices = new List<int>(totalIndices);

            Random.State previousRandomState = Random.state;
            Random.InitState(seed);

            for (int planeIndex = 0; planeIndex < planeCount; planeIndex++)
                AddPlane(Random.rotationUniform, positions, normals, uv0, indices);

            Random.state = previousRandomState;

            generatedMesh.indexFormat = totalVertices > 65535
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;

            generatedMesh.SetVertices(positions);
            generatedMesh.SetNormals(normals);
            generatedMesh.SetUVs(0, uv0);
            generatedMesh.SetTriangles(indices, 0, true);
            generatedMesh.RecalculateBounds();

            meshFilter.sharedMesh = generatedMesh;
        }

        private void AddPlane(
            Quaternion rotation,
            List<Vector3> positions,
            List<Vector3> normals,
            List<Vector2> uv0,
            List<int> indices)
        {
            int vertexOffset = positions.Count;

            for (int pointIndex = 0; pointIndex <= planeDetail; pointIndex++)
            {
                float normalizedAngle = pointIndex / (float)planeDetail;
                float angle = normalizedAngle * Mathf.PI * 2f;
                float sine = Mathf.Sin(angle);
                float cosine = Mathf.Cos(angle);

                Vector3 innerPosition = rotation * new Vector3(
                    sine * radiusMin,
                    0f,
                    cosine * radiusMin);

                Vector3 outerPosition = rotation * new Vector3(
                    sine * radiusMax,
                    0f,
                    cosine * radiusMax);

                Vector3 planeNormal = rotation * Vector3.up;

                positions.Add(innerPosition);
                positions.Add(outerPosition);
                normals.Add(planeNormal);
                normals.Add(planeNormal);

                // X moves outward from the Sun, Y moves around each plane.
                uv0.Add(new Vector2(0f, normalizedAngle * radiusMin));
                uv0.Add(new Vector2(1f, normalizedAngle * radiusMax));
            }

            for (int segmentIndex = 0; segmentIndex < planeDetail; segmentIndex++)
            {
                int vertex = vertexOffset + segmentIndex * 2;

                indices.Add(vertex);
                indices.Add(vertex + 1);
                indices.Add(vertex + 2);

                indices.Add(vertex + 3);
                indices.Add(vertex + 2);
                indices.Add(vertex + 1);
            }
        }

        private void ApplyMaterialProperties()
        {
            CacheComponents();

            if (sourceMaterial != null)
                meshRenderer.sharedMaterial = sourceMaterial;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propertyBlock);

            if (prominenceTexture != null)
                propertyBlock.SetTexture(MainTexId, prominenceTexture);

            propertyBlock.SetColor(TintId, color);
            propertyBlock.SetFloat(BrightnessId, brightness);
            propertyBlock.SetFloat(OpacityId, opacity);
            Vector3 sunCentre = transform.parent != null
                ? transform.parent.TransformPoint(baseLocalPosition)
                : transform.position;

            Vector3 worldScale = transform.lossyScale;
            float largestWorldScale = Mathf.Max(
                Mathf.Abs(worldScale.x),
                Mathf.Abs(worldScale.y),
                Mathf.Abs(worldScale.z));

            propertyBlock.SetVector(CenterWsId, sunCentre);
            propertyBlock.SetFloat(
                RadiusMaxId,
                Mathf.Max(radiusMax * largestWorldScale, 0.001f));
            propertyBlock.SetVector(FlowSpeedId, flowSpeed);
            propertyBlock.SetFloat(DistortionAmountId, distortionAmount);
            propertyBlock.SetFloat(DistortionSpeedId, distortionSpeed);
            propertyBlock.SetFloat(ContrastId, flameContrast);
            propertyBlock.SetFloat(FadePowerId, fadePower);
            propertyBlock.SetFloat(ClipPowerId, clipPower);
            propertyBlock.SetFloat(OuterFadePowerId, outerFadePower);

            meshRenderer.SetPropertyBlock(propertyBlock);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void ApplyCameraOffset()
        {
            Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
            if (cameraToUse == null)
                return;

            transform.localPosition = baseLocalPosition;

            if (Mathf.Abs(cameraOffset) <= Mathf.Epsilon)
                return;

            Vector3 directionToCamera = (
                cameraToUse.transform.position - transform.position).normalized;

            transform.position += directionToCamera * cameraOffset;
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
