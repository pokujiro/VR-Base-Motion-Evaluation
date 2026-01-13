using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionEvaluator : MonoBehaviour
{
    public float leftPositionError { get; private set; }
    public float rightPositionError { get; private set; }
    public float leftRotationError { get; private set; }
    public float rightRotationError { get; private set; }

    public Animator targetAnimator; // アニメーションがアタッチされたAnimator
    public Animator animator;
    public Transform referenceLeftHand; // 基準となる左手
    public Transform referenceRightHand; // 基準となる右手

    private Transform leftHandBone;
    private Transform rightHandBone;

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        // アバターの左手と右手のボーンをAnimatorから取得
        referenceLeftHand = targetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        referenceRightHand = targetAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);

        // 確認用ログ
        Debug.Log("Reference Left Hand: " + referenceLeftHand.name);
        Debug.Log("Reference Right Hand: " + referenceRightHand.name);
    }

    void Update()
    {
        // 基準ボーン（アニメーション中のボーン）の位置や回転を使用して評価可能
        //if (referenceLeftHand != null)
        //{
        //    Debug.Log($"Reference Left Hand Position: {referenceLeftHand.position}");
        //}
        //if (referenceRightHand != null)
        //{
        //    Debug.Log($"Reference Right Hand Position: {referenceRightHand.position}");
        //}


        // 評価の際に使用

        if (referenceLeftHand != null && leftHandBone != null)
        {
            // 位置の誤差を計算
            leftPositionError = Vector3.Distance(referenceLeftHand.position, leftHandBone.position);

            // 回転の誤差を計算
            leftRotationError = Quaternion.Angle(referenceLeftHand.rotation, leftHandBone.rotation);

            Debug.Log($"Left　Position Error: {leftPositionError}, Rotation Error: {leftRotationError}");
        }

        if (referenceLeftHand != null && leftHandBone != null)
        {
            // 位置の誤差を計算
            rightPositionError = Vector3.Distance(referenceRightHand.position, rightHandBone.position);

            // 回転の誤差を計算
            rightRotationError = Quaternion.Angle(referenceRightHand.rotation, rightHandBone.rotation);

            Debug.Log($"Left　Position Error: {rightPositionError}, Rotation Error: {rightRotationError}");
        }
    }
}


