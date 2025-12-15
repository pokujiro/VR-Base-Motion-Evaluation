using UnityEngine;

public class AvatarTransparency : MonoBehaviour
{
    [Range(0f, 1f)]
    public float transparency = 0.2f; // 透明度（0 = 完全透明, 1 = 不透明）

    void Start()
    {
        // すべての SkinnedMeshRenderer を再帰的に取得
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (renderers.Length > 0)
        {
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                ApplyTransparency(renderer);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ SkinnedMeshRenderer が見つかりません。アバターの階層を確認してください。");
        }
    }

    /// <summary>
    /// 透明度を適用する
    /// </summary>
    private void ApplyTransparency(SkinnedMeshRenderer renderer)
    {
        foreach (Material mat in renderer.materials)
        {
            string shaderName = mat.shader.name;

            if (shaderName.Contains("MToon"))
            {
                ApplyMToonTransparency(mat);
            }
            else if (shaderName == "Standard")
            {
                ApplyStandardTransparency(mat);
            }
            else
            {
                Debug.LogWarning($"⚠️ シェーダー '{shaderName}' は透明度に完全対応していない可能性があります。");
            }
        }
    }

    /// <summary>
    /// MToon シェーダー用の透明化処理
    /// </summary>
    private void ApplyMToonTransparency(Material mat)
    {
        // 透明化のための設定
        mat.SetFloat("_Surface", 1);          // 1 = Transparent モード
        mat.SetFloat("_BlendMode", 3);        // 3 = Alpha Blending
        mat.SetFloat("_ZWrite", 0);           // 深度書き込みをオフ
        mat.SetInt("_Cull", 0);               // 両面描画 (重要)

        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // 透明度の適用
        Color color = mat.color;
        color.a = transparency;               // 透明度を設定
        mat.color = color;

        Debug.Log($"✅ MToonシェーダーで透明度 {transparency} を適用しました。");
    }

    /// <summary>
    /// Standard シェーダー用の透明化処理
    /// </summary>
    private void ApplyStandardTransparency(Material mat)
    {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color color = mat.color;
        color.a = transparency;
        mat.color = color;

        Debug.Log($"✅ Standard シェーダーで透明度 {transparency} を適用しました。");
    }
}
