#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InteractiveGlobeKit.Editor
{
    public static class InteractiveGlobeSetupWindow
    {
        private const string Root = "Assets/InteractiveGlobeKit";
        private const string ModelPath = Root + "/Models/Globe_01.fbx";
        private const string MaterialPath = Root + "/GeneratedMaterials";
        private const string PrefabPath = Root + "/Prefabs/InteractiveGlobe.prefab";

        [MenuItem("Tools/Interactive Globe Kit/Create Complete Interactive Globe")]
        public static void CreateCompleteGlobe()
        {
            EnsureFolders();
            GameObject source = Selection.activeObject as GameObject;
            if (source == null) source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("Interactive Globe Kit", "Select your globe FBX in the Project window or keep Globe_01.fbx at " + ModelPath, "OK");
                return;
            }

            GameObject rig = new GameObject("Interactive_Globe_Rig");
            Undo.RegisterCreatedObjectUndo(rig, "Create Interactive Globe");
            GameObject model = PrefabUtility.IsPartOfPrefabAsset(source)
                ? (GameObject)PrefabUtility.InstantiatePrefab(source)
                : Object.Instantiate(source);
            model.name = "Globe_Model";
            model.transform.SetParent(rig.transform, false);

            MeshFilter globeMesh = FindBestSurface(model);
            if (globeMesh == null)
            {
                Object.DestroyImmediate(rig);
                EditorUtility.DisplayDialog("Interactive Globe Kit", "No MeshFilter was found in the selected model.", "OK");
                return;
            }

            Transform pivot = new GameObject("Earth_Rotation_Pivot_23.5deg").transform;
            pivot.SetParent(rig.transform, false);
            pivot.position = globeMesh.transform.TransformPoint(globeMesh.sharedMesh.bounds.center);
            globeMesh.transform.SetParent(pivot, true);
            pivot.localRotation = Quaternion.Euler(0f, 0f, -23.5f);

            SphereCollider collider = globeMesh.GetComponent<SphereCollider>();
            if (collider == null) collider = globeMesh.gameObject.AddComponent<SphereCollider>();
            collider.center = globeMesh.sharedMesh.bounds.center;
            collider.radius = Mathf.Max(globeMesh.sharedMesh.bounds.extents.x, globeMesh.sharedMesh.bounds.extents.y, globeMesh.sharedMesh.bounds.extents.z);

            Material grid = MaterialAsset("M_Grid_Cyan", "Interactive Globe/Educational Glow", new Color(.08f, .7f, 1f, .45f), 1.35f);
            Material equator = MaterialAsset("M_Equator_Red", "Interactive Globe/Educational Glow", new Color(1f, .08f, .04f, .9f), 2.2f);
            Material prime = MaterialAsset("M_PrimeMeridian_Yellow", "Interactive Globe/Educational Glow", new Color(1f, .72f, .05f, .9f), 2f);
            Material axis = MaterialAsset("M_Axis_White", "Interactive Globe/Educational Glow", new Color(.75f, .95f, 1f, .95f), 2f);
            Material special = MaterialAsset("M_SpecialCircles_Green", "Interactive Globe/Educational Glow", new Color(.25f, 1f, .45f, .75f), 1.7f);
            Material pin = MaterialAsset("M_Hotspot_Orange", "Interactive Globe/Educational Glow", new Color(1f, .25f, .04f, 1f), 2.4f);
            Material particles = MaterialAsset("M_EquatorParticles", "Interactive Globe/Educational Glow", new Color(.1f, .95f, 1f, .85f), 2.7f);
            if (particles.HasProperty("_ParticleMode")) particles.SetFloat("_ParticleMode", 1f);
            Material hemispheres = MaterialAsset("M_HemisphereOverlay", "Interactive Globe/Transparent Overlay", new Color(.1f, .65f, 1f, .16f), 1f);
            Material dayNight = MaterialAsset("M_DayNight", "Interactive Globe/Day Night Terminator", new Color(.005f, .015f, .07f, .8f), 1f);

            GameObject sun = new GameObject("Educational_Sun", typeof(Light));
            sun.transform.SetParent(rig.transform, false);
            sun.transform.localPosition = new Vector3(5f, 1.5f, 0f);
            sun.transform.localRotation = Quaternion.Euler(25f, -90f, 0f);
            Light light = sun.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.1f; light.color = new Color(1f, .92f, .78f);

            InteractiveGlobeController controller = rig.AddComponent<InteractiveGlobeController>();
            controller.Configure(globeMesh.transform, pivot);
            controller.ConfigureMaterials(grid, equator, prime, axis, special, pin, hemispheres, dayNight, particles);
            controller.Rebuild();

            GlobeTouchRotateZoom touch = rig.AddComponent<GlobeTouchRotateZoom>();
            touch.Configure(pivot, rig.transform, collider);
            GlobeDayNightController cycle = rig.AddComponent<GlobeDayNightController>();
            Renderer terminator = globeMesh.transform.Find("Generated_Educational_Overlays/08_Day_Night/Day_Night_Terminator_Overlay")?.GetComponent<Renderer>();
            cycle.Configure(pivot, sun.transform, terminator);
            rig.AddComponent<GlobeLessonDirector>();

            Selection.activeGameObject = rig;
            PrefabUtility.SaveAsPrefabAsset(rig, PrefabPath);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(rig);
            EditorUtility.DisplayDialog("Interactive Globe Kit", "Complete globe created in the scene and saved as:\n" + PrefabPath + "\n\nThe overlays are already generated and will not duplicate when Play Mode starts.", "Done");
        }

        [MenuItem("Tools/Interactive Globe Kit/Rebuild Selected Globe Overlays")]
        public static void RebuildSelected()
        {
            InteractiveGlobeController controller = Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponentInParent<InteractiveGlobeController>();
            if (controller == null) return;
            controller.Rebuild();
            EditorUtility.SetDirty(controller.gameObject);
        }

        private static MeshFilter FindBestSurface(GameObject model)
        {
            return model.GetComponentsInChildren<MeshFilter>(true)
                .Where(x => x.sharedMesh != null)
                .OrderByDescending(x => Score(x))
                .FirstOrDefault();
        }

        private static float Score(MeshFilter filter)
        {
            Bounds b = filter.sharedMesh.bounds;
            Vector3 e = b.extents;
            float min = Mathf.Max(.0001f, Mathf.Min(e.x, Mathf.Min(e.y, e.z)));
            float max = Mathf.Max(e.x, Mathf.Max(e.y, e.z));
            float sphereScore = 10000f / (1f + Mathf.Abs(max / min - 1f));
            float nameBonus = filter.name.ToLowerInvariant().Contains("globe") || filter.name.ToLowerInvariant().Contains("earth") ? 1000000f : 0f;
            return nameBonus + sphereScore + filter.sharedMesh.vertexCount;
        }

        private static Material MaterialAsset(string name, string shaderName, Color color, float intensity)
        {
            string path = MaterialPath + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName);
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_NightColor")) material.SetColor("_NightColor", color);
            if (material.HasProperty("_Intensity")) material.SetFloat("_Intensity", intensity);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "InteractiveGlobeKit");
            if (!AssetDatabase.IsValidFolder(MaterialPath)) AssetDatabase.CreateFolder(Root, "GeneratedMaterials");
            string prefabFolder = Root + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder(Root, "Prefabs");
        }
    }
}
#endif
