using UnityEngine;

public class BoneErrorVisualizer : MonoBehaviour
{
    public Transform referenceBone; // 基準データのボーン
    public Transform currentBone;   // 現在のボーン
    public Material boneMaterial;   // ボーンの表示用マテリアル
    public LineRenderer lineRenderer; // ボーン間のライン

    public float positionThreshold = 0.05f; // 位置誤差の閾値
    public float rotationThreshold = 10f;   // 回転誤差の閾値
    private Color defaultColor = Color.white;

    void Update()
    {
        if (referenceBone == null || currentBone == null) return;

        // 位置誤差を計算
        float positionError = Vector3.Distance(referenceBone.position, currentBone.position);

        // 回転誤差を計算
        float rotationError = Quaternion.Angle(referenceBone.rotation, currentBone.rotation);

        // **色の変更処理**
        Color newColor = defaultColor;

        if (positionError > positionThreshold && rotationError > rotationThreshold)
        {
            newColor = Color.red; // 両方違う
        }
        else if (positionError > positionThreshold)
        {
            newColor = Color.blue; // 位置が違う
        }
        else if (rotationError > rotationThreshold)
        {
            newColor = Color.yellow; // 回転が違う
        }

        // ボーンの色変更
        if (boneMaterial != null)
        {
            boneMaterial.color = newColor;
        }

        // ラインの色変更
        if (lineRenderer != null)
        {
            lineRenderer.startColor = newColor;
            lineRenderer.endColor = newColor;
        }
    }
}
