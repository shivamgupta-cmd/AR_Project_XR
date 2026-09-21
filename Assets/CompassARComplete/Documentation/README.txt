COMPASS AR COMPLETE — UNITY 2022 URP

Includes your Compass.fbx, mountain panoramic skybox, world-space UI panels/sprites,
ambient particles, magnetic-field particles, North pulse, success burst, runtime lesson
controller, auto-rotate helper, material generation and a one-click demo builder.

INSTALL
1) Copy Assets/CompassARComplete into your Unity 2022 URP project.
2) Wait for compilation.
3) Run Tools > Compass AR > Create Complete Demo.
4) The builder creates COMPASS_AR_COMPLETE and saves:
   Assets/CompassARComplete/Prefabs/Compass_AR_COMPLETE.prefab
5) In CompassLessonController assign the actual magnetic needle Transform from your FBX
   to the Needle field. FBX part naming cannot be safely assumed.
6) Position North_Target where you want Magnetic North to be.
7) Put VO clips into Audio_Placeholders or your own folder and assign them in Scene Voice Clips.
8) Use public methods from Timeline/SignalReceiver:
   ResetLesson, ShowIntroduction, ShowCompassParts, ShowMagneticField, ShowDirections,
   FindNorth, Explore3D, ShowMapNavigation, StartActivity, ShowSuccess, PlayCompletePreview.

VFX INCLUDED
- Ambient_Dust
- Magnetic_Field_Particles
- North_Pulse
- Success_Burst

UI INCLUDED
- Title panel
- Info/instruction panels
- Hotspot sprite
- North glow sprite
- Checkpoint sprite
- Success sprite

NOTE
The generated skybox and UI are newly created assets inspired by the storyboard, not copied
from a commercial environment pack. For a photorealistic forest scene, add your own terrain,
trees and rocks; this package provides the lesson structure, skybox, model, UI and VFX.
