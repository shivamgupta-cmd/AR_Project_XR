# Educational Globe 4K Texture and Region Highlight System

This package replaces a blurry globe texture with a clean 4096 × 2048 equirectangular educational map. It also supplies a region ID texture, twelve individual masks, a cross-pipeline shader and a controller for highlighting continents and oceans independently.

## Install and apply

1. Copy `Assets/GlobeRegionHighlight` into your Unity project's `Assets` folder.
2. Wait for Unity to finish importing.
3. In the Hierarchy, select the spherical Earth mesh—not the stand or the full rig.
4. Select **Tools > Educational Globe > Apply Region Highlight Material to Selected Globe**.
5. Unity creates `M_EducationalGlobeHighlight.mat`, assigns both textures and adds `GlobeRegionHighlighter`.

## Selectable regions

| ID | Region | UI method |
|---:|---|---|
| 0 | No highlight | `ClearHighlight()` |
| 1 | Asia | `HighlightAsia()` |
| 2 | Africa | `HighlightAfrica()` |
| 3 | North America | `HighlightNorthAmerica()` |
| 4 | South America | `HighlightSouthAmerica()` |
| 5 | Europe | `HighlightEurope()` |
| 6 | Australia / Oceania | `HighlightAustraliaOceania()` |
| 7 | Antarctica | `HighlightAntarctica()` |
| 8 | Pacific Ocean | `HighlightPacificOcean()` |
| 9 | Atlantic Ocean | `HighlightAtlanticOcean()` |
| 10 | Indian Ocean | `HighlightIndianOcean()` |
| 11 | Arctic Ocean | `HighlightArcticOcean()` |
| 12 | Southern Ocean | `HighlightSouthernOcean()` |

## Connect a UI button

1. Select the Button.
2. Add an entry under `On Click()`.
3. Drag the globe sphere containing `GlobeRegionHighlighter` into the field.
4. Select `GlobeRegionHighlighter > HighlightAsia()` or another named method.

Use `HighlightById(int)` when a lesson manager stores the region as a number.

## Fix texture orientation

Every sphere model can use a different UV orientation. Open `M_EducationalGlobeHighlight.mat` and use:

- **Longitude Texture Offset** to rotate the map horizontally around the sphere.
- **Flip Texture Horizontally** if east and west are reversed.
- **Flip Texture Vertically** if the North and South Poles are reversed.

Adjust only the material; you do not need to modify the texture files or globe mesh.

## Shader controls

- **Highlight Color** controls the selected region's glow colour.
- **Highlight Strength** controls how strongly the selected region changes colour.
- **Pulse Speed** controls the animated glow.
- **Base Brightness** changes the complete globe brightness.
- **Rim Color/Strength/Power** controls the atmospheric edge glow.
- **Selected Region ID** can be changed manually for testing.

## Texture import settings

The included editor importer configures the textures automatically:

- Base texture: sRGB, bilinear filtering, mipmaps and high-quality compression.
- Region ID/masks: non-sRGB, point filtering, no mipmaps and no compression.

Do not enable compression or bilinear filtering on `EducationalGlobe_RegionID_4K.png`; changing its pixel values can make the shader select the wrong region.

## Included files

- `EducationalGlobe_BaseColor_4K.png` — visible 4K globe texture.
- `EducationalGlobe_RegionID_4K.png` — shader selection data.
- `EducationalGlobe_TexturePreview.png` — region legend and preview.
- `Masks/` — separate black-and-white masks for all twelve regions.
- `GlobeRegionHighlight.shader` — Built-in and URP highlight shader.
- `GlobeRegionHighlighter.cs` — named UI and lesson-control methods.
- `GlobeRegionHighlightSetup.cs` — one-click material setup and import settings.

## Mobile AR and VR performance

The 4K base texture produces the sharpest result. For a low-memory mobile device, set its Unity **Max Size** to `2048`; keep the Region ID texture at 4096 or 2048 with Point filtering and no compression. Do not add thin country-border lines because they can shimmer in VR.

## Geographic data

Land and country geometry is derived from Natural Earth public-domain vector data. Ocean regions use conventional educational basin divisions and are intended for classroom highlighting rather than maritime legal boundaries.
