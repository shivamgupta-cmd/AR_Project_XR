ELECTRON JUMP COMPLETE - UNITY
==============================

THIS PACKAGE DOES NOT INCLUDE IncomingEnergyWave.cs.
Keep your existing IncomingEnergyWave exactly as it is.

INCLUDED / AUTO-CREATED
-----------------------
- ElectronEnergyJump.cs
- TrailRenderer setup
- Glowing Trail material
- Arrival ParticleSystem
- Arrival particle material
- HigherOrbitPoint helper
- Editor setup wizard

INSTALL
-------
Copy:
Assets/ElectronJumpComplete
into your Unity project's Assets folder.

AUTOMATIC SETUP
---------------
1. In Unity, select your Electron GameObject.
2. Go to:
   Tools > Bohr Electron Jump > Setup Selected Electron

The tool automatically:
- Adds ElectronEnergyJump
- Adds/configures TrailRenderer
- Creates glowing trail material
- Creates/configures ElectronArrivalParticles
- Creates HigherOrbitPoint
- Assigns the references

3. After setup, move HigherOrbitPoint to the EXACT place on the higher orbit
   where the electron should land.

CONNECT TO YOUR EXISTING ENERGY WAVE
------------------------------------
Select your existing IncomingEnergyWave object.

In:
On Energy Absorbed

Click +

Drag the Electron GameObject into the event.

Select:
ElectronEnergyJump > AbsorbEnergyAndJump()

RESULT
------
Energy wave hits electron
-> electron glows
-> trail begins
-> electron jumps from its current position
-> electron reaches HigherOrbitPoint
-> arrival particles burst
-> optional parenting to HigherOrbitParent

ORBIT SCRIPT
------------
This package does NOT pause, stop, enable, disable, or modify your orbit script.

If you want parenting after the jump:
Assign Higher Orbit Parent in ElectronEnergyJump.

If your own orbit script handles the new orbit another way:
Leave Higher Orbit Parent empty.

URP
---
For a stronger glow, enable Bloom in your Volume/Post Processing.
