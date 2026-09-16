PRISM LIGHT FX - TRAVEL REVEAL V3
Unity 2022.3 URP

WHAT CHANGED
The beam no longer appears fully from frame 1.

Sequence:
1. White light grows/travels from Torch_Exit toward Prism_Entry.
2. When it reaches the prism, optional prism flash particles can play.
3. After Prism Reaction Delay, the rainbow spectrum grows/travels from Prism_Exit toward Screen_Center.
4. The final spectrum remains visible.

SETUP
1. Import Assets/PrismLightFX_TravelReveal.
2. Tools > Prism Light FX > Create Travel Reveal Rig V3.
3. Move the four marker objects:
   Torch_Exit
   Prism_Entry
   Prism_Exit
   Screen_Center

RECOMMENDED VALUES
Incoming Travel Time: 0.55 - 0.8
Prism Reaction Delay: 0.08 - 0.18
Spectrum Travel Time: 0.75 - 1.1
Travel Head Softness: 0.04 - 0.09
Spectrum Spread: 0.30 - 0.45
Band Width At Prism: 0.006 - 0.012
Band Width At Screen: 0.10 - 0.16
Overlap: 0.02 - 0.04

TIMELINE / SIGNAL METHODS
ResetEffect()
StartIncomingTravel()
StartSpectrumTravel()
ShowCompletedEffect()

OR run the entire sequence:
PlayRefractionSequence()

MANUAL PROGRESS METHODS
SetIncomingProgress(float 0..1)
SetSpectrumProgress(float 0..1)

For your Bohr-style Signal Track:
Signal 1 -> ResetEffect
Signal 2 -> StartIncomingTravel
Signal 3 -> StartSpectrumTravel

IMPORTANT
Set Play On Start = false if Timeline/Signals control the effect.

URP
Enable Bloom in the Global Volume for the reference-style glow.
