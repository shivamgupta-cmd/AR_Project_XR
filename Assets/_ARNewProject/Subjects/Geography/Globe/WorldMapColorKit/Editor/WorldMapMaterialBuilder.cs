#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WorldMapColorKit.Editor
{
    public static class WorldMapMaterialBuilder
    {
        private const string Root = "Assets/WorldMapColorKit";
        private const string ModelPath = Root + "/Models/map1_fbx.fbx";
        private const string MaterialFolder = Root + "/Materials";
        private const string PrefabPath = Root + "/Prefabs/ColoredWorldMap.prefab";
        private const string ShaderName = "Educational World Map/Continent Solid Color";

        private enum Region
        {
            Unknown,
            NorthAmerica,
            SouthAmerica,
            Europe,
            Africa,
            Asia,
            Oceania,
            Antarctica
        }

        [MenuItem("Tools/Educational World Map/Create and Apply Continent Materials")]
        public static void CreateAndApply()
        {
            EnsureFolders();
            bool createdInstance = false;
            GameObject target = Selection.activeGameObject;

            if (target == null || EditorUtility.IsPersistent(target))
            {
                GameObject source = target != null ? target : AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                if (source == null)
                {
                    EditorUtility.DisplayDialog("World Map Color Kit", "The bundled FBX could not be found at " + ModelPath, "OK");
                    return;
                }
                target = (GameObject)PrefabUtility.InstantiatePrefab(source);
                target.name = "Colored_World_Map";
                Undo.RegisterCreatedObjectUndo(target, "Create Colored World Map");
                createdInstance = true;
            }

            Dictionary<Region, Material> materials = CreateMaterials();
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            int assigned = 0;
            var unassigned = new List<string>();

            foreach (Renderer renderer in renderers)
            {
                Region region = Classify(renderer);
                if (region == Region.Unknown)
                {
                    unassigned.Add(renderer.name);
                    continue;
                }

                Undo.RecordObject(renderer, "Assign Continent Material");
                int slotCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                renderer.sharedMaterials = Enumerable.Repeat(materials[region], slotCount).ToArray();
                assigned++;
            }

            Selection.activeGameObject = target;
            if (createdInstance || PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.NotAPrefab)
                PrefabUtility.SaveAsPrefabAsset(target, PrefabPath);

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(target);

            string message = "Applied solid continent colours to " + assigned + " mesh renderers.\n\n" +
                             "A reusable prefab was saved at:\n" + PrefabPath;
            if (unassigned.Count > 0)
                message += "\n\nUnchanged renderers: " + string.Join(", ", unassigned.Distinct());
            EditorUtility.DisplayDialog("World Map Color Kit", message, "Done");
        }

        private static Region Classify(Renderer renderer)
        {
            string objectName = renderer.name.Replace(" ", string.Empty).ToLowerInvariant();

            // Exact mesh mapping from the supplied map1_fbx model.
            if (objectName.Contains("shape137")) return Region.SouthAmerica;
            if (objectName.Contains("line056")) return Region.NorthAmerica;
            if (objectName.Contains("rectangle002")) return Region.Europe;
            if (objectName.Contains("shape716")) return Region.Africa;
            if (objectName.Contains("line46255667")) return Region.Asia;
            if (objectName.Contains("line058") || objectName.Contains("line46255670")) return Region.Oceania;
            if (objectName.Contains("line46255668")) return Region.Antarctica;

            // Fallback to the original FBX material names.
            string materialNames = string.Join(" ", renderer.sharedMaterials
                .Where(x => x != null)
                .Select(x => x.name.ToLowerInvariant()));

            if (materialNames.Contains("257")) return Region.SouthAmerica;
            if (materialNames.Contains("256")) return Region.NorthAmerica;
            if (materialNames.Contains("#25") || materialNames.EndsWith("25")) return Region.Europe;
            if (materialNames.Contains("206") || materialNames.Contains("207")) return Region.Africa;
            if (materialNames.Contains("#28") || materialNames.EndsWith("28")) return Region.Oceania;
            return Region.Unknown;
        }

        private static Dictionary<Region, Material> CreateMaterials()
        {
            return new Dictionary<Region, Material>
            {
                { Region.NorthAmerica, CreateMaterial("M_NorthAmerica_Purple", new Color32(93, 84, 163, 255)) },
                { Region.SouthAmerica, CreateMaterial("M_SouthAmerica_Pink", new Color32(231, 67, 145, 255)) },
                { Region.Europe, CreateMaterial("M_Europe_YellowOrange", new Color32(249, 181, 18, 255)) },
                { Region.Africa, CreateMaterial("M_Africa_Green", new Color32(8, 165, 107, 255)) },
                { Region.Asia, CreateMaterial("M_Asia_LightGreen", new Color32(126, 188, 31, 255)) },
                { Region.Oceania, CreateMaterial("M_Oceania_RedOrange", new Color32(240, 76, 40, 255)) },
                { Region.Antarctica, CreateMaterial("M_Antarctica_Beige", new Color32(204, 190, 164, 255)) }
            };
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(ShaderName);
            if (shader == null) shader = Shader.Find("Unlit/Color");

            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;

            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EdgeDarkness")) material.SetFloat("_EdgeDarkness", .16f);
            if (material.HasProperty("_Brightness")) material.SetFloat("_Brightness", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "WorldMapColorKit");
            if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder(Root, "Materials");
            if (!AssetDatabase.IsValidFolder(Root + "/Prefabs")) AssetDatabase.CreateFolder(Root, "Prefabs");
        }
    }
}
#endif
