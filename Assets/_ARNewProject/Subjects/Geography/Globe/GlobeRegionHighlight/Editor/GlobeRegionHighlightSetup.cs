#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GlobeRegionHighlight.Editor
{
    public sealed class GlobeTextureImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/GlobeRegionHighlight/Textures/")) return;
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.maxTextureSize = 4096;

            bool dataTexture = assetPath.Contains("RegionID") || assetPath.Contains("/Masks/");
            if (dataTexture)
            {
                importer.sRGBTexture = false;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaSource = TextureImporterAlphaSource.None;
            }
            else
            {
                importer.sRGBTexture = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
            }
        }
    }

    public static class GlobeRegionHighlightSetup
    {
        private const string Root = "Assets/GlobeRegionHighlight";
        private const string BaseTexturePath = Root + "/Textures/EducationalGlobe_BaseColor_4K.png";
        private const string RegionTexturePath = Root + "/Textures/EducationalGlobe_RegionID_4K.png";
        private const string MaterialPath = Root + "/Materials/M_EducationalGlobeHighlight.mat";

        [MenuItem("Tools/Educational Globe/Apply Region Highlight Material to Selected Globe")]
        public static void ApplyToSelectedGlobe()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Educational Globe", "Select the globe sphere object in the Hierarchy first.", "OK");
                return;
            }

            Renderer renderer = Selection.activeGameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = Selection.activeGameObject.GetComponentsInChildren<Renderer>(true)
                    .FirstOrDefault(x => x.name.ToLowerInvariant().Contains("globe") || x.name.ToLowerInvariant().Contains("earth"));
            }
            if (renderer == null)
            {
                EditorUtility.DisplayDialog("Educational Globe", "No globe Renderer was found. Select the sphere mesh itself and run the command again.", "OK");
                return;
            }

            Shader shader = Shader.Find("Educational Globe/Region Highlight");
            Texture2D baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseTexturePath);
            Texture2D regionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RegionTexturePath);
            if (shader == null || baseTexture == null || regionTexture == null)
            {
                EditorUtility.DisplayDialog("Educational Globe", "Required shader or textures are still importing. Wait for Unity to finish, then try again.", "OK");
                return;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "M_EducationalGlobeHighlight" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.shader = shader;
            material.SetTexture("_MainTex", baseTexture);
            material.SetTexture("_RegionMap", regionTexture);
            material.SetFloat("_SelectedRegion", 0f);
            material.SetColor("_HighlightColor", new Color(.08f, .8f, 1.5f, 1f));
            material.SetFloat("_HighlightStrength", .8f);
            EditorUtility.SetDirty(material);

            Undo.RecordObject(renderer, "Apply Educational Globe Material");
            renderer.sharedMaterial = material;
            GlobeRegionHighlighter highlighter = renderer.GetComponent<GlobeRegionHighlighter>();
            if (highlighter == null)
                highlighter = Undo.AddComponent<GlobeRegionHighlighter>(renderer.gameObject);
            highlighter.Configure(renderer);
            EditorUtility.SetDirty(highlighter);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = renderer.gameObject;

            EditorUtility.DisplayDialog(
                "Educational Globe",
                "The 4K texture, region ID map and GlobeRegionHighlighter are now applied. Use its public Highlight methods from your UI buttons.",
                "Done");
        }
    }
}
#endif
