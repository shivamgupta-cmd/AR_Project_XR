#if UNITY_EDITOR
using System.IO;
using CompassLearning.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CompassLearning.VFX.Editor
{
    public static class CompassMagneticFieldBuilder
    {
        private const string Root = "Assets/CompassMagneticFieldVFX";
        private const string Generated = Root + "/Generated";
        private const string Prefabs = Root + "/Prefabs";

        [MenuItem("Tools/Compass Magnetic Field/Create Around Selected Compass")]
        public static void CreateAroundSelection()
        {
            if (!EnsureAssets()) return;
            Transform selected = Selection.activeTransform;
            GameObject go = new GameObject("Compass_MagneticField_VFX");
            Undo.RegisterCreatedObjectUndo(go, "Create Compass Magnetic Field VFX");
            if (selected != null)
            {
                go.transform.SetParent(selected, false);
                go.transform.localPosition = FindVisualCenter(selected);
                go.transform.localRotation = Quaternion.identity;
            }
            CompassMagneticFieldVFX vfx = go.AddComponent<CompassMagneticFieldVFX>();
            vfx.ConfigureMaterials(
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Lines.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Particles.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Arrows.mat"));
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        [MenuItem("Tools/Compass Magnetic Field/Rebuild Ready-To-Use Prefab")]
        public static void BuildPrefab()
        {
            if (!EnsureAssets()) return;
            GameObject go = new GameObject("Compass_MagneticField_VFX");
            CompassMagneticFieldVFX vfx = go.AddComponent<CompassMagneticFieldVFX>();
            vfx.ConfigureMaterials(
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Lines.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Particles.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Arrows.mat"));
            PrefabUtility.SaveAsPrefabAsset(go, Prefabs + "/Compass_MagneticField_VFX.prefab");
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/Compass_MagneticField_VFX.prefab");
            Debug.Log("Compass magnetic-field prefab created at " + Prefabs + "/Compass_MagneticField_VFX.prefab");
        }

        [MenuItem("Tools/Compass Magnetic Field/Upgrade Selected VFX To Complete Field")]
        public static void UpgradeSelectedEffect()
        {
            CompassMagneticFieldVFX vfx = Selection.activeGameObject == null
                ? null
                : Selection.activeGameObject.GetComponent<CompassMagneticFieldVFX>();
            if (vfx == null)
            {
                EditorUtility.DisplayDialog("Compass Magnetic Field", "Select the existing Compass_MagneticField_VFX object in the Hierarchy first.", "OK");
                return;
            }
            Undo.RecordObject(vfx, "Upgrade Compass Magnetic Field VFX");
            if (!EnsureAssets()) return;
            vfx.ConfigureMaterials(
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Lines.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Particles.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Generated + "/M_MagneticField_Arrows.mat"));
            vfx.ApplyCompleteFieldPreset();
            EditorUtility.SetDirty(vfx);
            EditorSceneManager.MarkSceneDirty(vfx.gameObject.scene);
            Debug.Log("Selected effect upgraded to the complete upright dipole field.");
        }

        [InitializeOnLoadMethod]
        private static void BuildOnceAfterImport()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/Compass_MagneticField_VFX.prefab") == null)
                    BuildPrefab();
            };
        }

        private static bool EnsureAssets()
        {
            EnsureFolder(Root, "Generated");
            EnsureFolder(Root, "Prefabs");
            Texture2D soft = AssetDatabase.LoadAssetAtPath<Texture2D>(Generated + "/T_SoftGlow.png");
            if (soft == null)
            {
                CreateSoftGlowTexture(Generated + "/T_SoftGlow.png");
                AssetDatabase.Refresh();
                soft = AssetDatabase.LoadAssetAtPath<Texture2D>(Generated + "/T_SoftGlow.png");
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(Generated + "/T_SoftGlow.png");
                importer.textureType = TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }

            Texture2D arrow = AssetDatabase.LoadAssetAtPath<Texture2D>(Generated + "/T_DirectionArrow.png");
            if (arrow == null)
            {
                CreateDirectionArrowTexture(Generated + "/T_DirectionArrow.png");
                AssetDatabase.Refresh();
                arrow = AssetDatabase.LoadAssetAtPath<Texture2D>(Generated + "/T_DirectionArrow.png");
                TextureImporter arrowImporter = (TextureImporter)AssetImporter.GetAtPath(Generated + "/T_DirectionArrow.png");
                arrowImporter.textureType = TextureImporterType.Default;
                arrowImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                arrowImporter.mipmapEnabled = false;
                arrowImporter.wrapMode = TextureWrapMode.Clamp;
                arrowImporter.filterMode = FilterMode.Bilinear;
                arrowImporter.textureCompression = TextureImporterCompression.Compressed;
                arrowImporter.SaveAndReimport();
            }

            Shader shader = Shader.Find("Compass Learning/Magnetic Field Additive");
            if (shader == null)
            {
                Debug.LogError("Magnetic Field shader is not compiled yet. Wait for Unity to finish compiling, then run the menu again.");
                return false;
            }
            // White tint preserves the component's red-violet-blue HDR vertex colours.
            CreateOrUpdateMaterial(Generated + "/M_MagneticField_Lines.mat", shader, soft, Color.white, 2.6f, 0.55f, 0.82f, 0.32f, 0.8f);
            CreateOrUpdateMaterial(Generated + "/M_MagneticField_Particles.mat", shader, soft, Color.white, 4.2f, 2.2f, 0f, 0f, 0f);
            CreateOrUpdateMaterial(Generated + "/M_MagneticField_Arrows.mat", shader, arrow, Color.white, 3.6f, 1f, 0f, 0f, 0f);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static Vector3 FindVisualCenter(Transform selected)
        {
            Renderer[] renderers = selected.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return Vector3.zero;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return selected.InverseTransformPoint(bounds.center);
        }

        private static void CreateOrUpdateMaterial(string path, Shader shader, Texture texture, Color tint, float intensity, float softness, float continuousBase, float highlightStrength, float flowSpeed)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_MainTex", texture);
            material.SetColor("_Tint", tint);
            material.SetFloat("_Intensity", intensity);
            material.SetFloat("_Softness", softness);
            material.SetFloat("_ContinuousBase", continuousBase);
            material.SetFloat("_HighlightStrength", highlightStrength);
            material.SetFloat("_FlowSpeed", flowSpeed);
            EditorUtility.SetDirty(material);
        }

        private static void CreateDirectionArrowTexture(string assetPath)
        {
            const int width = 128;
            const int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = Mathf.Abs((y + 0.5f) / height * 2f - 1f);
                bool shaft = u >= 0.08f && u <= 0.67f && v <= 0.16f;
                bool head = u >= 0.42f && v <= (1f - u) * 1.55f;
                float alpha = (shaft || head) ? 1f : 0f;
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void CreateSoftGlowTexture(string assetPath)
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / size, (y + 0.5f) / size) * 2f - Vector2.one;
                float radial = Mathf.Clamp01(1f - uv.magnitude);
                float alpha = Mathf.SmoothStep(0f, 1f, radial);
                alpha *= alpha;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
