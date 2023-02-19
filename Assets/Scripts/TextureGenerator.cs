/*
 * Constructs and returns Texture2D by size and 2D color map
 */
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    static Texture2D mapTexture2D;
    
    

    public static Texture2D textureFromColorMap(Color[] colorMap, int width, int height)
    {
        mapTexture2D = new Texture2D(width, height);
        mapTexture2D.filterMode = FilterMode.Point;
        mapTexture2D.wrapMode = TextureWrapMode.Clamp;
        mapTexture2D.SetPixels(colorMap);
        mapTexture2D.Apply();
        
        return mapTexture2D;
    }
}
