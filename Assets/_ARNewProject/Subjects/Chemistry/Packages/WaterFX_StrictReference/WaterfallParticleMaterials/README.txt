WATERFALL PARTICLE MATERIALS — Unity 2022 URP

The uploaded WATERFALL_EFFECT.prefab was inspected:
Splash ParticleSystemRenderer material = None
Foam ParticleSystemRenderer material = None
Mist ParticleSystemRenderer material = None
That is why those particles render pink.

Import this folder, then run:
Tools > Waterfall FX > Create Particle Materials

Assign:
Splash Renderer > Material = M_Splash
Foam Renderer > Material = M_Foam
Mist Renderer > Material = M_Mist

Included textures:
Splash_Streaks.png
Foam_Bubbles.png
Mist_Soft.png

Included shader:
WaterFX/URP Soft Particle

For Particle Systems, Renderer > Render Mode should normally remain Billboard.
