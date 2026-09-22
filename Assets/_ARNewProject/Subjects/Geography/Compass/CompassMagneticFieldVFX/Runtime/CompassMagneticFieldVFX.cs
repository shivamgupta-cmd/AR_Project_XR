using System.Collections.Generic;
using UnityEngine;

namespace CompassLearning.VFX
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CompassMagneticFieldVFX : MonoBehaviour
    {
        private const string GeneratedRootName = "Generated_Magnetic_Field";

        [Header("Appearance")]
        [SerializeField] private Material fieldLineMaterial;
        [SerializeField] private Material particleMaterial;
        [SerializeField] private Material directionArrowMaterial;
        [ColorUsage(true, true)] [SerializeField] private Color northColor = new Color(1.4f, 0.04f, 0.12f, 1f);
        [ColorUsage(true, true)] [SerializeField] private Color middleColor = new Color(0.62f, 0.08f, 1.35f, 1f);
        [ColorUsage(true, true)] [SerializeField] private Color southColor = new Color(0.02f, 0.62f, 1.6f, 1f);

        [Header("Field Geometry")]
        [Range(8, 40)] [SerializeField] private int lineCount = 20;
        [Range(24, 120)] [SerializeField] private int pointsPerLine = 72;
        [Min(0.05f)] [SerializeField] private float poleDistance = 0.42f;
        [Min(0.1f)] [SerializeField] private float fieldRadius = 1.05f;
        [Min(0.001f)] [SerializeField] private float lineWidth = 0.018f;
        [Range(0f, 1f)] [SerializeField] private float lineOpacity = 0.88f;

        [Header("Particle Motion")]
        [Range(1, 16)] [SerializeField] private int particlesPerLine = 7;
        [Range(1, 8)] [SerializeField] private int directionArrowsPerLine = 3;
        [Min(0f)] [SerializeField] private float flowSpeed = 0.16f;
        [Min(0.001f)] [SerializeField] private float particleSize = 0.035f;
        [Min(0.001f)] [SerializeField] private float directionArrowSize = 0.06f;
        [SerializeField] private bool playOnAwake = true;

        [Header("Optional Compass Alignment")]
        [Tooltip("Local magnetic North direction. +Y creates the upright Earth-style field shown in the reference.")]
        [SerializeField] private Vector3 magneticNorthAxis = Vector3.up;
        [Tooltip("Moves the complete field to the visual centre of the model when its pivot is at the base.")]
        [SerializeField] private Vector3 fieldCenterOffset = Vector3.zero;
        [Range(0f, 35f)] [SerializeField] private float depthSpreadDegrees = 12f;

        private readonly List<LineRenderer> lines = new List<LineRenderer>();
        private readonly List<Vector3[]> paths = new List<Vector3[]>();
        private ParticleSystem flowParticles;
        private ParticleSystem directionParticles;
        private ParticleSystem northPoleParticles;
        private ParticleSystem southPoleParticles;
        [SerializeField, HideInInspector] private Transform generatedRoot;
        private double editorStartTime;
        private bool playing;
#if UNITY_EDITOR
        [System.NonSerialized] private bool validationRebuildQueued;
#endif

        public bool IsPlaying => playing;

        private void OnEnable()
        {
            editorStartTime = UnityEditorSafeTime();

            // The generated hierarchy is serialized with the scene, but these
            // runtime lists are not. Rebuild the caches from the saved objects
            // instead of generating a second magnetic field on entering Play Mode.
            if (!TryCacheExistingField())
            {
                if (Application.isPlaying)
                {
                    Debug.LogWarning(
                        "No baked magnetic field was found. Exit Play Mode, select " +
                        "this component, and use Rebuild Magnetic Field once, then save the scene.",
                        this);
                }
                else
                {
                    EnsureGenerated();
                }
            }

            if (playOnAwake) Play();
        }

        private void OnDisable()
        {
            if (flowParticles != null) flowParticles.Clear();
            if (directionParticles != null) directionParticles.Clear();
        }

        private void OnValidate()
        {
            lineCount = Mathf.Max(6, lineCount);
            pointsPerLine = Mathf.Max(20, pointsPerLine);
            magneticNorthAxis = magneticNorthAxis.sqrMagnitude < 0.001f ? Vector3.up : magneticNorthAxis.normalized;
            if (!isActiveAndEnabled) return;

#if UNITY_EDITOR
            // OnValidate can run inside animation, rendering, or physics callbacks.
            // Queue destruction/rebuild until after that callback ends.
            if (!Application.isPlaying)
            {
                if (validationRebuildQueued) return;
                validationRebuildQueued = true;
                UnityEditor.EditorApplication.delayCall += DelayedValidationRebuild;
                return;
            }

            // Never replace scene-authored VFX merely because Play Mode caused
            // Unity to validate the component.
            return;
#endif

#if !UNITY_EDITOR
            Rebuild();
#endif
        }

#if UNITY_EDITOR
        private void DelayedValidationRebuild()
        {
            validationRebuildQueued = false;
            if (this == null || !isActiveAndEnabled || Application.isPlaying) return;
            Rebuild();
        }
#endif

        private void Update()
        {
            if (!playing) return;
            EnsureGenerated();
            UpdateFlowParticles((float)(Application.isPlaying ? Time.timeAsDouble : UnityEditorSafeTime() - editorStartTime));
            AnimateLines((float)(Application.isPlaying ? Time.timeAsDouble : UnityEditorSafeTime()));
            UpdateMaterialAnimation();
        }

        public void Play()
        {
            playing = true;
            if (!TryCacheExistingField() && !Application.isPlaying)
                EnsureGenerated();
            if (northPoleParticles != null) northPoleParticles.Play();
            if (southPoleParticles != null) southPoleParticles.Play();
        }

        public void Stop()
        {
            playing = false;
            if (flowParticles != null) flowParticles.Clear();
            if (directionParticles != null) directionParticles.Clear();
            if (northPoleParticles != null) northPoleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (southPoleParticles != null) southPoleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        [ContextMenu("Rebuild Magnetic Field")]
        public void Rebuild()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning(
                    "Magnetic field rebuilding is disabled in Play Mode. " +
                    "Exit Play Mode and rebuild it once in Edit Mode.",
                    this);
                return;
            }

            ClearGenerated();
            EnsureGenerated();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        public void ConfigureMaterials(Material linesMaterial, Material particlesMaterial)
        {
            ConfigureMaterials(linesMaterial, particlesMaterial, particlesMaterial);
        }

        public void ConfigureMaterials(Material linesMaterial, Material particlesMaterial, Material arrowsMaterial)
        {
            fieldLineMaterial = linesMaterial;
            particleMaterial = particlesMaterial;
            directionArrowMaterial = arrowsMaterial;
            Rebuild();
        }

        public void ApplyCompleteFieldPreset()
        {
            magneticNorthAxis = Vector3.up;
            lineCount = 20;
            pointsPerLine = 72;
            poleDistance = 0.42f;
            fieldRadius = 1.05f;
            lineWidth = 0.018f;
            lineOpacity = 0.88f;
            particlesPerLine = 7;
            directionArrowsPerLine = 3;
            flowSpeed = 0.16f;
            particleSize = 0.035f;
            directionArrowSize = 0.06f;
            depthSpreadDegrees = 12f;
            Rebuild();
        }

        private void EnsureGenerated()
        {
            if (generatedRoot != null && lines.Count == lineCount && paths.Count == lineCount) return;

            if (TryCacheExistingField()) return;

            // Runtime changes cannot be saved back into the scene. Keeping this
            // guard here also prevents Update() from creating a fresh hierarchy
            // after OnEnable deliberately chose to use only the baked field.
            if (Application.isPlaying) return;

            ClearGenerated();
            generatedRoot = new GameObject(GeneratedRootName).transform;
            generatedRoot.SetParent(transform, false);
            generatedRoot.localPosition = fieldCenterOffset;

            BuildLines();
            flowParticles = CreateParticleSystem("Flowing_Energy_Particles", 256);
            ConfigureFlowRenderer(flowParticles);
            directionParticles = CreateParticleSystem("Magnetic_Direction_Arrows", 192);
            ConfigureDirectionRenderer(directionParticles);
            northPoleParticles = CreatePoleEmitter("North_Pole_Glow", magneticNorthAxis.normalized * poleDistance, northColor);
            southPoleParticles = CreatePoleEmitter("South_Pole_Glow", -magneticNorthAxis.normalized * poleDistance, southColor);
        }

        /// <summary>
        /// Reconnects the non-serialized runtime references to the magnetic field
        /// that is already saved under this component in the scene.
        /// </summary>
        private bool TryCacheExistingField()
        {
            if (generatedRoot == null)
            {
                Transform existing = transform.Find(GeneratedRootName);
                if (existing != null) generatedRoot = existing;
            }

            if (generatedRoot == null) return false;

            // A valid cache needs no further work. This keeps Update inexpensive.
            if (lines.Count == lineCount && paths.Count == lineCount &&
                flowParticles != null && directionParticles != null)
                return true;

            LineRenderer[] savedLines = generatedRoot.GetComponentsInChildren<LineRenderer>(true);
            System.Array.Sort(savedLines, (a, b) =>
                string.CompareOrdinal(a.gameObject.name, b.gameObject.name));

            if (savedLines.Length != lineCount)
                return false;

            lines.Clear();
            paths.Clear();

            for (int i = 0; i < savedLines.Length; i++)
            {
                LineRenderer line = savedLines[i];
                if (line.positionCount < 2)
                {
                    lines.Clear();
                    paths.Clear();
                    return false;
                }

                Vector3[] savedPath = new Vector3[line.positionCount];
                line.GetPositions(savedPath);
                lines.Add(line);
                paths.Add(savedPath);
            }

            flowParticles = FindSavedParticleSystem("Flowing_Energy_Particles");
            directionParticles = FindSavedParticleSystem("Magnetic_Direction_Arrows");
            northPoleParticles = FindSavedParticleSystem("North_Pole_Glow");
            southPoleParticles = FindSavedParticleSystem("South_Pole_Glow");

            bool complete = flowParticles != null && directionParticles != null &&
                            northPoleParticles != null && southPoleParticles != null;

            if (!complete)
            {
                lines.Clear();
                paths.Clear();
            }

            return complete;
        }

        private ParticleSystem FindSavedParticleSystem(string childName)
        {
            Transform child = generatedRoot.Find(childName);
            return child != null ? child.GetComponent<ParticleSystem>() : null;
        }

        private void BuildLines()
        {
            Vector3 axis = magneticNorthAxis.normalized;
            // Keep most lines in one readable presentation plane like the supplied
            // Earth reference. A small depth spread prevents a completely flat look.
            Vector3 sideA = Vector3.Cross(axis, Vector3.forward);
            if (sideA.sqrMagnitude < 0.01f) sideA = Vector3.Cross(axis, Vector3.right);
            sideA.Normalize();
            Vector3 depthAxis = Vector3.Cross(axis, sideA).normalized;
            int shellsPerSide = Mathf.CeilToInt(lineCount * 0.5f);

            for (int i = 0; i < lineCount; i++)
            {
                int shell = i / 2;
                float sideSign = (i & 1) == 0 ? 1f : -1f;
                float shell01 = shellsPerSide <= 1 ? 0f : shell / (shellsPerSide - 1f);
                float spread = Mathf.Lerp(-depthSpreadDegrees, depthSpreadDegrees, Mathf.Repeat(shell * 0.618034f, 1f));
                Quaternion depthRotation = Quaternion.AngleAxis(spread, axis);
                Vector3 lateral = depthRotation * sideA * sideSign;
                float loopRadius = fieldRadius * Mathf.Lerp(0.42f, 1.15f, shell01);
                Vector3 north = axis * poleDistance;
                Vector3 south = -axis * poleDistance;
                Vector3 c1 = north + axis * loopRadius * 0.42f + lateral * loopRadius;
                Vector3 c2 = south - axis * loopRadius * 0.42f + lateral * loopRadius;

                Vector3[] path = new Vector3[pointsPerLine];
                for (int p = 0; p < pointsPerLine; p++)
                {
                    float t = p / (pointsPerLine - 1f);
                    path[p] = CubicBezier(north, c1, c2, south, t);
                }
                paths.Add(path);

                GameObject go = new GameObject($"Field_Line_{i + 1:00}");
                go.transform.SetParent(generatedRoot, false);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = false;
                lr.positionCount = path.Length;
                lr.SetPositions(path);
                lr.widthMultiplier = lineWidth * (0.8f + 0.25f * (i % 3));
                lr.numCapVertices = 3;
                lr.numCornerVertices = 2;
                lr.textureMode = LineTextureMode.Tile;
                lr.alignment = LineAlignment.View;
                lr.sharedMaterial = fieldLineMaterial;
                lr.colorGradient = CreateLineGradient(i);
                lines.Add(lr);
            }
        }

        private Gradient CreateLineGradient(int index)
        {
            float brightness = 0.72f + 0.28f * ((index % 4) / 3f);
            Gradient g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(northColor * brightness, 0f),
                    new GradientColorKey(middleColor * brightness, 0.48f),
                    new GradientColorKey(southColor * brightness, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(lineOpacity, 0.08f),
                    new GradientAlphaKey(lineOpacity * 0.72f, 0.5f),
                    new GradientAlphaKey(lineOpacity, 0.92f),
                    new GradientAlphaKey(0f, 1f)
                });
            return g;
        }

        private ParticleSystem CreateParticleSystem(string objectName, int maxParticles)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(generatedRoot, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = maxParticles;
            main.startLifetime = 99999f;
            main.startSpeed = 0f;
            var emission = ps.emission;
            emission.enabled = false;
            return ps;
        }

        private void ConfigureFlowRenderer(ParticleSystem ps)
        {
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sharedMaterial = particleMaterial;
            renderer.sortMode = ParticleSystemSortMode.Distance;
        }

        private void ConfigureDirectionRenderer(ParticleSystem ps)
        {
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
            renderer.velocityScale = 0.12f;
            renderer.lengthScale = 1.8f;
            renderer.sharedMaterial = directionArrowMaterial != null ? directionArrowMaterial : particleMaterial;
            renderer.sortMode = ParticleSystemSortMode.Distance;
        }

        private ParticleSystem CreatePoleEmitter(string objectName, Vector3 position, Color color)
        {
            ParticleSystem ps = CreateParticleSystem(objectName, 64);
            ps.transform.localPosition = position;
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.7f, particleSize * 2.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, new Color(color.r, color.g, color.b, 0f));
            main.maxParticles = 64;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 18f;
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = poleDistance * 0.12f;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.04f;
            noise.frequency = 1.2f;
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.16f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;
            ConfigureFlowRenderer(ps);
            return ps;
        }

        private void UpdateFlowParticles(float time)
        {
            if (flowParticles == null || paths.Count == 0) return;
            int total = lineCount * particlesPerLine;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[total];
            int cursor = 0;

            for (int line = 0; line < lineCount; line++)
            {
                for (int j = 0; j < particlesPerLine; j++)
                {
                    float phase = Mathf.Repeat(time * flowSpeed + j / (float)particlesPerLine + line * 0.071f, 1f);
                    // The visible outside journey runs north-to-south. The final
                    // section returns invisibly through the centre, producing a
                    // genuinely continuous magnetic circuit with no teleport flash.
                    const float outsideFraction = 0.84f;
                    bool outside = phase < outsideFraction;
                    float pathT = outside ? phase / outsideFraction : (phase - outsideFraction) / (1f - outsideFraction);
                    Vector3 position = outside
                        ? SamplePath(paths[line], pathT)
                        : Vector3.Lerp(-magneticNorthAxis.normalized * poleDistance, magneticNorthAxis.normalized * poleDistance, pathT);
                    float pulse = 0.72f + 0.28f * Mathf.Sin((phase + time) * Mathf.PI * 2f);
                    particles[cursor].position = position;
                    particles[cursor].startSize = outside ? particleSize * pulse : 0f;
                    Color travelColor = EvaluateFieldColor(pathT);
                    travelColor.a = outside ? Mathf.Lerp(0.65f, 1f, pulse) : 0f;
                    particles[cursor].startColor = travelColor;
                    particles[cursor].remainingLifetime = 9999f;
                    particles[cursor].startLifetime = 9999f;
                    cursor++;
                }
            }
            flowParticles.SetParticles(particles, cursor);
            UpdateDirectionParticles(time);
        }

        private void UpdateDirectionParticles(float time)
        {
            if (directionParticles == null || paths.Count == 0) return;
            int total = lineCount * directionArrowsPerLine;
            ParticleSystem.Particle[] arrows = new ParticleSystem.Particle[total];
            int cursor = 0;

            for (int line = 0; line < lineCount; line++)
            {
                for (int j = 0; j < directionArrowsPerLine; j++)
                {
                    float phase = Mathf.Repeat(time * flowSpeed + j / (float)directionArrowsPerLine + line * 0.043f, 1f);
                    const float outsideFraction = 0.88f;
                    bool outside = phase < outsideFraction;
                    float pathT = outside ? phase / outsideFraction : (phase - outsideFraction) / (1f - outsideFraction);
                    Vector3 position;
                    Vector3 tangent;
                    if (outside)
                    {
                        position = SamplePath(paths[line], pathT);
                        Vector3 next = SamplePath(paths[line], Mathf.Min(pathT + 0.012f, 1f));
                        tangent = (next - position).normalized;
                    }
                    else
                    {
                        Vector3 south = -magneticNorthAxis.normalized * poleDistance;
                        Vector3 north = magneticNorthAxis.normalized * poleDistance;
                        position = Vector3.Lerp(south, north, pathT);
                        tangent = magneticNorthAxis.normalized;
                    }

                    Color arrowColor = EvaluateFieldColor(pathT);
                    arrowColor.a = outside ? 1f : 0f;
                    arrows[cursor].position = position;
                    arrows[cursor].velocity = tangent * Mathf.Max(0.25f, flowSpeed * 5f);
                    arrows[cursor].startSize = outside ? directionArrowSize : 0f;
                    arrows[cursor].startColor = arrowColor;
                    arrows[cursor].remainingLifetime = 9999f;
                    arrows[cursor].startLifetime = 9999f;
                    cursor++;
                }
            }
            directionParticles.SetParticles(arrows, cursor);
        }

        private void UpdateMaterialAnimation()
        {
            if (fieldLineMaterial != null && fieldLineMaterial.HasProperty("_FlowSpeed"))
                fieldLineMaterial.SetFloat("_FlowSpeed", Mathf.Max(0f, flowSpeed * 5f));
        }

        private void AnimateLines(float time)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == null) continue;
                float pulse = 0.86f + 0.14f * Mathf.Sin(time * 1.8f + i * 0.62f);
                lines[i].widthMultiplier = lineWidth * pulse * (0.8f + 0.25f * (i % 3));
            }
        }

        private Vector3 SamplePath(Vector3[] path, float t)
        {
            float scaled = Mathf.Clamp01(t) * (path.Length - 1);
            int a = Mathf.Min(Mathf.FloorToInt(scaled), path.Length - 2);
            return Vector3.Lerp(path[a], path[a + 1], scaled - a);
        }

        private Color EvaluateFieldColor(float t)
        {
            if (t < 0.5f) return Color.Lerp(northColor, middleColor, t * 2f);
            return Color.Lerp(middleColor, southColor, (t - 0.5f) * 2f);
        }

        private static Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float u = 1f - t;
            return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
        }

        private void ClearGenerated()
        {
            lines.Clear();
            paths.Clear();
            flowParticles = null;
            directionParticles = null;
            northPoleParticles = null;
            southPoleParticles = null;

            if (!Application.isPlaying)
            {
                // Remove any duplicate roots left by an older version of this
                // component, then create one clean baked hierarchy.
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = transform.GetChild(i);
                    if (child.name == GeneratedRootName)
                        DestroyImmediate(child.gameObject);
                }
            }
            else if (generatedRoot != null)
            {
                Destroy(generatedRoot.gameObject);
            }

            generatedRoot = null;
        }

        private static double UnityEditorSafeTime()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartupAsDouble;
#endif
        }
    }
}
