# Compass Magnetic Field VFX — Unity 2022

Ready-to-use magnetic field visualization matching storyboard panel 4 and the supplied Earth reference: complete paired dipole lobes, continuously circulating energy particles, and glowing magnetic poles.

## Install and use

1. Extract the ZIP.
2. Copy `Assets/CompassMagneticFieldVFX` into your Unity project's `Assets` folder.
3. Let Unity finish compiling. The package automatically generates its glow texture, direction-arrow texture, three materials, and the prefab.
4. Select your Compass model in the Hierarchy.
5. Choose **Tools > Compass Magnetic Field > Create Around Selected Compass**.
6. Press Play. Resize the VFX child with its Transform scale if needed.

If you already used an older version of this package, select the existing
`Compass_MagneticField_VFX` object and choose **Tools > Compass Magnetic Field > Upgrade Selected VFX To Complete Field**. This replaces the old dome layout and installs the continuous line-flow and arrow-particle materials without requiring you to rebuild the scene.

You can also drag `Assets/CompassMagneticFieldVFX/Prefabs/Compass_MagneticField_VFX.prefab` into the scene after Unity generates it.

## Orientation

The default magnetic North axis is local **+Y**, producing the upright, complete left-and-right field shown around Earth in the reference. Change **Magnetic North Axis** only if your model uses another orientation:

- +Y: `(0, 1, 0)` (default/reference appearance)
- +Z: `(0, 0, 1)`
- +X: `(1, 0, 0)`

Place the VFX object at the visual centre of the compass/globe. The one-click menu now calculates the selected model's renderer centre automatically, even when its pivot is at ground level.

## Recommended settings for the storyboard look

- Line Count: `20`
- Points Per Line: `72`
- Pole Distance: `0.42`
- Field Radius: `1.05`
- Line Width: `0.018`
- Particles Per Line: `7`
- Direction Arrows Per Line: `3`
- Flow Speed: `0.16`
- Particle Size: `0.035`
- Direction Arrow Size: `0.06`
- Depth Spread Degrees: `12`

For a strong glow in URP, enable HDR on the camera and Bloom in your existing Volume. The VFX still remains bright cyan without Bloom.

## Timeline / button methods

Call these public methods from UnityEvents, Timeline Signals, or another script:

- `Play()`
- `Stop()`
- `SetVisible(bool)`
- `Rebuild()`

## Compatibility and performance

- Unity 2022.3 LTS
- URP and Built-in Render Pipeline shader passes included
- No external packages required
- Uses one particle system for travelling energy beads, one for direction arrows, two lightweight pole emitters, and 20 Line Renderers
- VR-friendly default particle count and no expensive collision, shadows, or mesh particles

If a material is missing, run **Tools > Compass Magnetic Field > Rebuild Ready-To-Use Prefab** after Unity finishes compiling.
