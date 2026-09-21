#if UNITY_EDITOR
using UnityEditor;
public class CompassSpritePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.Contains("Assets/CompassARComplete/Sprites/")) return;
        var i=(TextureImporter)assetImporter;
        i.textureType=TextureImporterType.Sprite;
        i.spriteImportMode=SpriteImportMode.Single;
        i.alphaIsTransparency=true;
        i.mipmapEnabled=false;
    }
}
#endif