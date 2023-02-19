/*
 * Displays given Texture2D on given object's renderer
 */

using UnityEngine;

public class PlaneDisplay : MonoBehaviour
{
    public Renderer textureRenderer;



    public void drawTexture(Texture2D texture)
    {
        textureRenderer.material.mainTexture = texture;
    }
}
