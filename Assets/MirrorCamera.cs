using UnityEngine;

public class MirrorCamera : MonoBehaviour
{
    public RenderTexture mirrorTexture; // Inspector ‚Å RenderTexture ‚ğƒZƒbƒg

    void Start()
    {
        if (mirrorTexture == null)
        {
            Debug.LogError("MirrorTexture ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.targetTexture = mirrorTexture;
        }
    }
}
