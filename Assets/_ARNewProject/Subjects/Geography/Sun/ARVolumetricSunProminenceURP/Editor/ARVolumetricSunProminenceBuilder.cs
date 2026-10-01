#if UNITY_EDITOR
using CompassLearning.SunVFX;
using UnityEditor;
using UnityEngine;

namespace CompassLearning.SunVFX.Editor
{
    public static class ARVolumetricSunProminenceBuilder
    {
        private const string RootPath = "Assets/ARVolumetricSunProminenceURP/";
        private const string ProminenceMaterialPath =
            RootPath + "Materials/M_AR_VolumetricSunProminence.mat";
        private const string SparkMaterialPath =
            RootPath + "Materials/M_AR_SunCoronaSpark.mat";
        private const string TexturePath =
            RootPath + "Textures/T_AR_SunProminence.png";

        [MenuItem("Tools/Compass Learning/Sun/Create Complete AR Volumetric Corona")]
        public static void CreateCompleteCorona()
        {
            Transform selectedSun = Selection.activeTransform;

            if (selectedSun == null)
            {
                EditorUtility.DisplayDialog(
                    "Select the Sun",
                    "Select the main Sun object in the Hierarchy, then run this tool again.",
                    "OK");
                return;
            }

            Material prominenceMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(ProminenceMaterialPath);
            Material sparkMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(SparkMaterialPath);
            Texture2D prominenceTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

            if (prominenceMaterial == null || prominenceTexture == null)
            {
                EditorUtility.DisplayDialog(
                    "Package assets missing",
                    "Keep the complete ARVolumetricSunProminenceURP folder directly inside Assets.",
                    "OK");
                return;
            }

            float sunRadius = CalculateLocalSunRadius(selectedSun);

            GameObject coronaRoot = new GameObject("AR Volumetric Sun Corona");
            Undo.RegisterCreatedObjectUndo(coronaRoot, "Create AR Volumetric Sun Corona");
            coronaRoot.transform.SetParent(selectedSun, false);
            coronaRoot.transform.localPosition = Vector3.zero;
            coronaRoot.transform.localRotation = Quaternion.identity;
            coronaRoot.transform.localScale = Vector3.one;

            ARVolumetricSunProminence prominence =
                Undo.AddComponent<ARVolumetricSunProminence>(coronaRoot);

            prominence.Configure(
                prominenceMaterial,
                prominenceTexture,
                sunRadius * 1.01f,
                sunRadius * 1.5f);

            CreateSparkLayer(
                coronaRoot.transform,
                sparkMaterial,
                sunRadius);

            Selection.activeGameObject = coronaRoot;
            EditorGUIUtility.PingObject(coronaRoot);

            Debug.Log(
                "AR Volumetric Sun Corona created. " +
                "The radius was fitted automatically to the selected Sun.",
                coronaRoot);
        }

        private static float CalculateLocalSunRadius(Transform selectedSun)
        {
            Renderer[] renderers = selectedSun.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
                return 1f;

            Bounds combinedBounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
                combinedBounds.Encapsulate(renderers[index].bounds);

            float worldRadius = Mathf.Max(
                combinedBounds.extents.x,
                combinedBounds.extents.y,
                combinedBounds.extents.z);

            Vector3 scale = selectedSun.lossyScale;
            float largestScale = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            return worldRadius / Mathf.Max(largestScale, 0.0001f);
        }

        private static void CreateSparkLayer(
            Transform parent,
            Material sparkMaterial,
            float sunRadius)
        {
            GameObject sparkObject = new GameObject("Corona Sparks - Mobile AR");
            Undo.RegisterCreatedObjectUndo(sparkObject, "Create Corona Sparks");
            sparkObject.transform.SetParent(parent, false);

            ParticleSystem particles = Undo.AddComponent<ParticleSystem>(sparkObject);
            ParticleSystemRenderer particleRenderer =
                sparkObject.GetComponent<ParticleSystemRenderer>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 220;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(
                sunRadius * 0.05f,
                sunRadius * 0.22f);
            main.startSize = new ParticleSystem.MinMaxCurve(
                sunRadius * 0.012f,
                sunRadius * 0.045f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.22f, 0.015f, 0.45f),
                new Color(1f, 0.75f, 0.12f, 0.9f));

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 55f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = sunRadius * 1.015f;
            shape.radiusThickness = 0.04f;
            shape.sphericalDirectionAmount = 1f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient fadeGradient = new Gradient();
            fadeGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.35f, 0.05f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.15f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime =
                particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.25f),
                    new Keyframe(0.3f, 1f),
                    new Keyframe(1f, 0f)));

            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.alignment = ParticleSystemRenderSpace.View;
            particleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            particleRenderer.receiveShadows = false;

            if (sparkMaterial != null)
                particleRenderer.sharedMaterial = sparkMaterial;
        }
    }
}
#endif
