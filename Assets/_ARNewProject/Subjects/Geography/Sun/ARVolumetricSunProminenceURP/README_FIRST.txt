AR VOLUMETRIC SUN PROMINENCE - UNITY 2022 URP

PURPOSE
This package recreates the supplied multi-plane prominence/corona effect without
Space Graphics Toolkit, CW Common, VFX Graph, or any other dependency. It uses
standard Unity mesh generation, one lightweight URP shader, and an optional
mobile-friendly Particle System spark layer.

COMPATIBILITY
- Unity 2022.3 LTS
- Universal Render Pipeline (URP)
- AR Foundation / ARCore / ARKit scenes
- Android mobile AR
- Built-in Particle System

INSTALLATION
1. Copy the included Assets/ARVolumetricSunProminenceURP folder into the Assets
   folder of your Unity project.
2. Wait for Unity to finish compiling and importing.
3. Select the main Sun object in the Hierarchy.
4. Run:
   Tools > Compass Learning > Sun > Create Complete AR Volumetric Corona
5. The tool automatically measures the selected Sun and creates the complete
   corona and spark effect as a child.

REFERENCE-MATCHED DEFAULTS
- Seed: 1082336596
- Plane Count: 10
- Plane Detail: 20
- Inner-to-outer radius ratio: 1 : 1.5
- Brightness: 0.91
- Edge Fade Power: 1
- Near Clip Power: 2
- Camera Offset: -0.01

IMPORTANT
Do not apply the prominence material directly to a UV Sphere. The component
generates 10 randomly rotated annulus planes. This multi-plane geometry is what
creates the full volumetric flame appearance visible from changing AR angles.

AR PERFORMANCE
The default mesh contains only 420 vertices and 400 triangles. The optional
spark system is capped at 220 particles. These defaults are designed to remain
lightweight for mobile AR.

GLOW
The effect uses additive HDR colours and works without Bloom. If your AR camera
setup supports URP post-processing, enable Bloom for a stronger glow.

TUNING
- Radius Min: moves the flame base toward or away from the Sun surface.
- Radius Max: controls total flame length.
- Plane Count: 8-10 recommended for Android AR.
- Plane Detail: 16-24 recommended.
- Flow Speed: controls continuous flame movement.
- Distortion Amount/Speed: controls flame bending.
- Fade Power: removes hard plane edges.
- Clip Power: prevents the near-facing planes from obscuring the Sun.
