# Coloured 3D World Map Material Kit

This package applies solid continent colours to the supplied `map1_fbx.fbx` model. It follows the provided reference palette without adding text, labels, icons or background objects.

## One-click setup

1. Copy `Assets/WorldMapColorKit` into your Unity project's `Assets` folder.
2. Wait until Unity finishes importing the FBX, shader and editor script.
3. Run **Tools > Educational World Map > Create and Apply Continent Materials**.
4. The completed model appears as `Colored_World_Map`.
5. A reusable prefab is saved at `Assets/WorldMapColorKit/Prefabs/ColoredWorldMap.prefab`.

If the model is already in your scene, select its parent before running the command. The tool will colour the selected instance instead of creating another one.

## Colour palette

| Continent | Material | Colour |
|---|---|---|
| North America | `M_NorthAmerica_Purple` | Purple `#5D54A3` |
| South America | `M_SouthAmerica_Pink` | Pink `#E74391` |
| Europe | `M_Europe_YellowOrange` | Yellow-orange `#F9B512` |
| Africa | `M_Africa_Green` | Green `#08A56B` |
| Asia | `M_Asia_LightGreen` | Light green `#7EBC1F` |
| Australia / Oceania | `M_Oceania_RedOrange` | Red-orange `#F04C28` |
| Antarctica | `M_Antarctica_Beige` | Beige `#CCBEA4` |

## Supplied FBX mesh mapping

| Mesh | Assigned region |
|---|---|
| `Shape137` | South America |
| `Line056` | North America |
| `Rectangle002` | Europe |
| `Shape716` | Africa; both original material slots receive the same green material |
| `Line46255667` | Asia |
| `Line058` and `Line46255670` | Australia / Oceania and surrounding islands |
| `Line46255668` | Antarctica |

## Material appearance

The included shader is deliberately texture-free. It creates:

- Flat, clean continent colours matching the reference
- Slight darkening on vertical/extruded edges so the 3D shape remains visible
- No country names or continent labels
- No texture dependency from the original FBX
- Built-in Render Pipeline and URP support

You can adjust **3D Edge Shading** on any material. Set it to `0` for completely flat unshaded colour, or approximately `0.15–0.22` for subtle 3D depth.

## Important

The old FBX refers to missing external JPG textures. This kit intentionally replaces those texture references with solid Unity materials, so the missing legacy textures are no longer required.
