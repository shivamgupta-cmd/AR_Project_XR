COPPER DEPOSIT FX V2 - REAL MESH SURFACE VERSION
================================================

WHAT CHANGED
------------
V1 estimated the nail as an ellipse/cylinder using renderer bounds. On tapered or irregular nails,
this could place copper blobs floating around the nail.

V2 samples REAL triangles from your nail mesh. Every copper blob is positioned on the actual nail
surface and offset only slightly along the real surface normal.

FAST SETUP
----------
1. Import package.
2. Select the nail GameObject.
3. Tools > Copper Deposit FX > Setup Selected Nail.
4. Press Play.

IMPORTANT
---------
For a MeshFilter mesh, Read/Write must be enabled in the model import settings.
Select the nail FBX/model in Project > Inspector > Model > Read/Write Enabled = ON > Apply.
SkinnedMeshRenderer is baked automatically at runtime.

GOOD STARTING SETTINGS
----------------------
Surface Offset      : 0.003 - 0.008
Blob Count          : 100 - 160
Min Blob Size       : 0.012 - 0.025
Max Blob Size       : 0.03 - 0.055
Normal Thickness    : 0.20 - 0.40
Create Sparkles     : OFF (turn ON only if you want reaction particles)

If the wrong portion gets coated, toggle Reverse Direction.
