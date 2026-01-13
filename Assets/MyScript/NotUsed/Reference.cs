using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reference : MonoBehaviour
{
    public DisplayBonesAsObjects referenceAvatar; // お手本アバター
    public ReplayAsObject replayAvatar; // リプレイアバター
    public Material lineMaterial; // 線のマテリアル

    private Dictionary<string, LineRenderer> boneLines = new Dictionary<string, LineRenderer>();

    // **ボーンごとの誤差を格納する辞書**
    private Dictionary<string, (float positionError, float rotationError)> boneErrors = new Dictionary<string, (float, float)>();

    // **誤差の閾値**
    public float positionThreshold = 0.15f; // 位置誤差の閾値

    /// <summary>
    /// **ボーンごとの誤差を設定**
    /// </summary>
    public void SetBoneErrors(Dictionary<string, (float positionError, float rotationError)> errors)
    {
        boneErrors = errors;
    }

    /// <summary>
    /// **位置誤差が閾値を超えた場合のみボーン同士をつなげる**
    /// </summary>
    public void ConnectBones()
    {
        Debug.Log("🔗 Connecting bones with position error filtering...");

        // すでに接続されている場合は削除
        DisconnectBones();

        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) continue;
            if (IsFingerBone(bone)) continue; // **指のボーンをスキップ**

            string boneName = bone.ToString();

            // **誤差の情報を取得**
            if (!boneErrors.ContainsKey(boneName)) continue;

            (float positionError, float rotationError) = boneErrors[boneName];

            // **位置誤差のみが閾値を超えている場合に接続**
            if (positionError >= positionThreshold && rotationError < positionThreshold)
            {
                if (referenceAvatar.boneObjects.ContainsKey(boneName) && replayAvatar.boneObjects.ContainsKey(boneName))
                {
                    GameObject lineObject = new GameObject($"Connection_{boneName}");
                    LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

                    lineRenderer.material = new Material(lineMaterial);
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                    lineRenderer.positionCount = 2;

                    boneLines[boneName] = lineRenderer;
                    Debug.Log($"✅ Connected bone (position error detected): {boneName}");
                }
            }
        }
    }

    /// <summary>
    /// **ボーンの接続を解除**
    /// </summary>
    public void DisconnectBones()
    {
        Debug.Log("❌ Disconnecting bones...");

        foreach (var line in boneLines.Values)
        {
            Destroy(line.gameObject);
        }

        boneLines.Clear();
    }

    void Update()
    {
        // **接続されているボーンをリアルタイムで更新**
        foreach (var bonePair in boneLines)
        {
            string boneName = bonePair.Key;
            LineRenderer lineRenderer = bonePair.Value;

            if (referenceAvatar.boneObjects.ContainsKey(boneName) && replayAvatar.boneObjects.ContainsKey(boneName))
            {
                lineRenderer.SetPosition(0, referenceAvatar.boneObjects[boneName].transform.position);
                lineRenderer.SetPosition(1, replayAvatar.boneObjects[boneName].transform.position);
            }
        }
    }

    /// <summary>
    /// **指のボーンかどうか判定**
    /// </summary>
    private bool IsFingerBone(HumanBodyBones bone)
    {
        return bone == HumanBodyBones.LeftThumbProximal ||
               bone == HumanBodyBones.LeftThumbIntermediate ||
               bone == HumanBodyBones.LeftThumbDistal ||
               bone == HumanBodyBones.LeftIndexProximal ||
               bone == HumanBodyBones.LeftIndexIntermediate ||
               bone == HumanBodyBones.LeftIndexDistal ||
               bone == HumanBodyBones.LeftMiddleProximal ||
               bone == HumanBodyBones.LeftMiddleIntermediate ||
               bone == HumanBodyBones.LeftMiddleDistal ||
               bone == HumanBodyBones.LeftRingProximal;
               }
}
