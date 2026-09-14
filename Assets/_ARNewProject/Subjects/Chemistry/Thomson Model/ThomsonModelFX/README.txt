THOMSON MODEL FX — COMPLETE MATERIAL + TEXTURE PACK

Target: Unity 2022.3 LTS + URP

INCLUDED
- Scripts: controller, electron highlight, ring animation
- Custom URP shaders
- 6 PNG textures:
  T_PositiveSpherePattern.png
  T_EnergyRing.png
  T_SoftCircle.png
  T_Sparkle.png
  T_ElectronGlow.png
  T_EnergyNoise.png
- Automatic material generation:
  M_PositiveSphere
  M_EnergyRing
  M_PositiveParticles
  M_Sparkle
  M_BackgroundSparkles
  M_ElectronBurst
- Complete FX rig prefab generator

INSTALL
1. Copy Assets/ThomsonModelFX into your Unity project Assets folder.
2. Wait for Unity to compile/import.
3. Run: Tools > Thomson Model FX > Create Complete FX Rig
4. Generated prefab: Assets/ThomsonModelFX/Generated/ThomsonModelFX_Rig.prefab
5. Generated materials: Assets/ThomsonModelFX/Generated/Materials

ASSIGN YOUR MODEL
On ThomsonModelFXController assign:
- Atom
- Positive Sphere
- Positive Sphere Renderer
- Electrons Root
- Optional Cross Section Object

For the positive sphere, assign Generated/Materials/M_PositiveSphere.mat to your sphere renderer.

TIMELINE METHODS
PlayAllFX()
StopAllFX()
PulseElectrons()
ShowCrossSection()
HideCrossSection()
ToggleCrossSection()

NOTE
Materials are generated inside Unity so shader references remain valid in any project and do not depend on fragile pre-generated GUIDs.
