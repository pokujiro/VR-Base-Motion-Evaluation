using UnityEngine;

public class MirrorTransform : MonoBehaviour
{
    public Transform original; // ミラーリングする元のオブジェクト
    private Vector3 initialOriginalPosition; // 元オブジェクトの初期位置
    private Vector3 initialMirroredPosition; // ミラーオブジェクトの初期位置
    private Quaternion initialRotationOffset; // 初期回転のオフセット
    private bool isInitialized = false;

    void Start()
    {
        if (original == null)
        {
            Debug.LogError("Original Transform is not assigned!");
            return;
        }

        // 初期位置を記録
        initialOriginalPosition = original.position;
        initialMirroredPosition = transform.position;

        // 初期回転のオフセットを記録
        initialRotationOffset = Quaternion.Inverse(MirrorRotation(original.rotation)) * transform.rotation;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || original == null) return;

        // 位置のミラーリング
        Vector3 mirroredMovement = MirrorPosition(original.position - initialOriginalPosition);
        transform.position = initialMirroredPosition + mirroredMovement;

        // 回転のミラーリング + Y軸180°回転
        transform.rotation = MirrorRotation(original.rotation) * initialRotationOffset;
    }

    // 位置のミラーリング（YZ平面対称）
    private Vector3 MirrorPosition(Vector3 movement)
    {
        return new Vector3(movement.x, movement.y, -movement.z);
    }

    // 回転のミラーリング（YZ平面対称）+ Y軸180°回転
    private Quaternion MirrorRotation(Quaternion originalRotation)
    {
        // 1. YZ 平面でのミラーリング（Z 軸反転）
        Quaternion mirrored = new Quaternion(originalRotation.x, originalRotation.y, -originalRotation.z, -originalRotation.w);

        // 2. Y 軸 180° 回転を適用
        return Quaternion.Euler(0, 180, 0) * mirrored;
    }
}


