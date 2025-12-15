using UnityEngine;

public class MirrorCameraManager : MonoBehaviour
{
    public Transform target; // プレイヤーやカメラ
    public Transform mirror; // 鏡のオブジェクト

    void Update()
    {
        if (target == null || mirror == null) return;

        // 鏡の法線ベクトルを取得（正規化）
        Vector3 mirrorNormal = mirror.forward.normalized;
        Vector3 toTarget = target.position - mirror.position;

        // 反射位置を計算
        Vector3 reflectedPosition = mirror.position - Vector3.Reflect(toTarget, mirrorNormal);

        // 異常値チェック
        if (!IsValidVector(reflectedPosition))
        {
            Debug.LogError("Reflected position is invalid! Resetting to mirror position.");
            reflectedPosition = mirror.position;
        }

        transform.position = reflectedPosition;

        // 鏡を基準にターゲットの向きを反転
        Vector3 reflectedDirection = Vector3.Reflect(target.forward, mirrorNormal);

        if (!IsValidVector(reflectedDirection))
        {
            Debug.LogError("Reflected direction is invalid! Resetting to forward.");
            reflectedDirection = target.forward;
        }

        transform.rotation = Quaternion.LookRotation(reflectedDirection, Vector3.up);
    }

    // ベクターが有効な値かチェック（NaN や Infinity を防ぐ）
    private bool IsValidVector(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}

