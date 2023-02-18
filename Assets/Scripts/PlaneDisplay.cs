using UnityEngine;

public class PlaneDisplay : MonoBehaviour
{
    public Renderer textureRenderer;



    public void drawTexture(Texture2D texture)
    {
        textureRenderer.material.mainTexture = texture;
    }
}
