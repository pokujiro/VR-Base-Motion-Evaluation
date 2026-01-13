using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayBonesAsGizmos : MonoBehaviour
{
    public Animator animator; // アバターのAnimatorコンポーネント
    public Color boneColor = Color.green; // ボーンの色

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        // アバター内のSkinnedMeshRenderer（メッシュ用）だけを無効化
        foreach (var skinnedRenderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skinnedRenderer.enabled = false;
        }

        Debug.Log("All skinned meshes are now hidden. Only bones are visible.");
    }

    void OnDrawGizmos()
    {
        if (animator == null) return;

        Gizmos.color = boneColor;

        // HumanBodyBonesのすべてのボーンを描画
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) continue;

            Transform boneTransform = animator.GetBoneTransform(bone);

            if (boneTransform != null)
            {
                // ボーンの位置を小さな球として描画
                Gizmos.DrawSphere(boneTransform.position, 0.02f);

                // ボーンの親子関係をラインで描画
                if (boneTransform.parent != null)
                {
                    Gizmos.DrawLine(boneTransform.position, boneTransform.parent.position);
                }
            }
        }
    }
}

