STRICT REFERENCE WATER FX — UNITY 2022 URP

Built only around the supplied reference poster.

Texture set:
Flow_Map.png
Water_Normal.png
Foam_Texture.png
Ocean_Normal.png
Caustics_Texture.png
Noise_Texture.png
Waterfall_Texture.png
Ripple_Texture.png
Waterfall_Normal.png (required by the Waterfall material shown in the inspector)

Inspector layouts and default values mirror the supplied poster:
Custom/OceanWater
Custom/RiverWater
Custom/Waterfall

SETUP:
1. Remove previous WaterFX versions if their shader names conflict.
2. Copy Assets/WaterFX_StrictReference into the project.
3. Run Tools > Water FX Strict Reference > Create Reference Materials + Waterfall FX.
4. Assign Ocean Water.mat to ocean mesh.
5. Assign River Water.mat to river mesh.
6. Position WATERFALL_EFFECT at the river outlet.

The supplied poster is a rendered/generated reference image rather than source Unity assets.
Therefore these maps are recreated to visually resemble the exact thumbnails and intended result;
they are not extractable original source maps from the poster.
