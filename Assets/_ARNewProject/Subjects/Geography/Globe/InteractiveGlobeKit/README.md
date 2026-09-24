# Interactive Globe Kit — Explore Our Earth

Ready-to-use educational globe package for Unity 2022.3. It upgrades the supplied simple globe with generated latitude/longitude lines, major reference circles, poles, axis tilt, hemispheres, location pins, moving equator particles, day/night visualization, interaction and guided lesson content.

## One-click setup

1. Import/extract the package and wait for compilation.
2. Open your lesson scene.
3. Run **Tools > Interactive Globe Kit > Create Complete Interactive Globe**.
4. Use the created `Interactive_Globe_Rig` in the scene. A reusable prefab is also saved to `Assets/InteractiveGlobeKit/Prefabs/InteractiveGlobe.prefab`.

The generator runs in Edit Mode. It creates the educational overlays once and saves them. Entering Play Mode does not create another globe or duplicate the overlays.

## Included visual systems

- Latitude lines every 15 degrees
- Longitude lines every 15 degrees
- Bright Equator at 0°
- Bright Prime Meridian at 0°
- Tropic of Cancer and Tropic of Capricorn
- Arctic Circle and Antarctic Circle
- North Pole and South Pole markers
- 23.5° axis tilt and axis line
- Northern and Southern Hemisphere transparent highlights
- Coordinate-based interactive location hotspots
- Continuous moving equator particle trail
- Day/night terminator overlay and rotating-Earth demonstration
- Optional seasons/orbit demonstration script
- Camera-facing labels
- Touch drag, mouse drag, pinch zoom and mouse-wheel zoom
- Guided eight-step lesson controller and voice-over script
- Five-question JSON learning check

## Default lesson flow

1. Our Earth — introduce the globe as a spherical model.
2. Explore — rotate and zoom the model.
3. Equator and poles — show the equator, poles and 23.5° axis.
4. Latitude and longitude — display the complete coordinate grid and special circles.
5. Hemispheres — compare northern/southern and eastern/western divisions.
6. Real places — tap pins and read coordinates.
7. Day and night — show the light/dark sides while Earth rotates.
8. Review — display the important overlays together and continue to the quiz.

The complete narration text is visible in the `GlobeLessonDirector` component. You can record each line, drag the clips into the lesson steps, and connect UI Text objects for the title and instruction.

## Connect UI buttons

Add a Button `On Click()` entry, drag `Interactive_Globe_Rig` into the object field, then select a public method from `InteractiveGlobeController`:

- `SetGridVisible(bool)`
- `SetEquatorVisible(bool)`
- `SetPrimeMeridianVisible(bool)`
- `SetSpecialCirclesVisible(bool)`
- `SetAxisVisible(bool)`
- `SetHotspotsVisible(bool)`
- `SetDayNightVisible(bool)`
- `SetParticlesVisible(bool)`
- `ShowNorthernHemisphere()`
- `ShowSouthernHemisphere()`
- `ShowBothHemispheres()`
- `HideHemispheres()`
- `SetAutoRotation(bool)`
- `ResetGlobeRotation()`

For a Next/Previous lesson interface, connect buttons to `GlobeLessonDirector.NextStep()` and `PreviousStep()`.

## AR setup

### Vuforia / marker-based AR

Drag the saved `InteractiveGlobe.prefab` under your ImageTarget. Reset its local position and adjust its local scale.

### AR Foundation tracked image

When an image is detected, instantiate `InteractiveGlobe.prefab` as a child of the detected `ARTrackedImage` transform. The kit itself has no AR package dependency, so it works with either Vuforia or AR Foundation.

For touch rotation, keep the globe's SphereCollider enabled and ensure your scene has a Main Camera. Hotspots support `IPointerClickHandler`; use an EventSystem and Physics Raycaster when selecting them through UI-style rays.

## Aligning the map texture

Different Earth textures place 0° longitude at different UV positions. If the Prime Meridian is not over Greenwich, adjust `Longitude Offset` on `InteractiveGlobeController`, then use **Tools > Interactive Globe Kit > Rebuild Selected Globe Overlays**. This does not alter the globe texture.

## Performance for mobile AR / VR

- The default grid uses lightweight LineRenderers and 96-point curves.
- Disable hemisphere and day/night overlays when they are not part of the current lesson step.
- Reduce Curve Resolution to 64 for lower-end hardware, then rebuild.
- Keep the generated prefab; do not call `Rebuild()` every frame or on scene start.
- The shaders are unlit/transparent and support Built-in Render Pipeline and URP.

## Package map

- `Models/Globe_01.fbx` — supplied globe model
- `Reference/` — target visualization reference
- `Runtime/InteractiveGlobeController.cs` — overlay generator and public controls
- `Runtime/GlobeTouchRotateZoom.cs` — touch/mouse interaction
- `Runtime/GlobeDayNightController.cs` — rotation and Sun direction
- `Runtime/GlobeLessonDirector.cs` — guided lesson and narration
- `Runtime/GlobeSeasonsDemonstration.cs` — optional orbit demonstration
- `Runtime/GlobeHotspot.cs` — selectable locations
- `Shaders/` — glow, transparent overlay and terminator shaders
- `Content/GlobeQuiz.json` — five-question assessment data
- `Editor/InteractiveGlobeSetupWindow.cs` — one-click builder

## Notes

This package uses legacy `UnityEngine.UI.Text` only for optional lesson labels, so TextMeshPro is not required. You may replace those fields with TMP in your own UI. The source FBX remains your supplied asset; the kit does not modify its embedded globe or stand materials.
