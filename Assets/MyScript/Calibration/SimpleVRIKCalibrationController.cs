using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using UniGLTF.SpringBoneJobs;
using UnityEngine;
using UnityEngine.InputSystem;
using static TrackerDataSaver;

public class SimpleVRIKCalibrationController : MonoBehaviour
{
    [Header("VRIK Settings")]
    [Tooltip("Reference to the VRIK component on the avatar.")]
    public VRIK ik;
    public VRIK replayIk;
    public VRIK originalReplayIk;

    [Header("Tracker References")]
    [Tooltip("The HMD.")]
    public Transform headTracker;

    [Tooltip("(Optional) Body tracker.")]
    public Transform bodyTracker;

    [Tooltip("(Optional) Left hand tracker.")]
    public Transform leftHandTracker;

    [Tooltip("(Optional) Right hand tracker.")]
    public Transform rightHandTracker;

    [Tooltip("(Optional) Left foot tracker.")]
    public Transform leftFootTracker;

    [Tooltip("(Optional) Right foot tracker.")]
    public Transform rightFootTracker;



    private float savedScale;
    private float savedHeadScaleY;
    private float savedPelvisScaleY;
    private float SavedScaleXZ;
    private float savedLeftArmScale;
    private float savedRightArmScale;
    private float savedLeftLegScale;
    private float savedRightLegScale;
    private float armYBase;
    private float headToWaistY;

    /// <summary>
    /// **キャリブレーションを開始**
    /// - 頭、手、足のトラッカーを考慮し、手足の長さも自動調整
    /// </summary>
    public void StartCalibration()
    {
        if (ik == null || headTracker == null)
        {
            Debug.LogError("VRIK または Head Tracker がありません！");
            return;
        }

        Debug.Log("🔧 シンプルキャリブレーション開始");

        // **身長のスケール調整**

        // 一緒のはずでは？
        float avatarHeight = ik.references.head.position.y - ik.references.root.position.y; // 元の身長
        Debug.Log($"現在のアバター　頭：{ik.references.head.position.y} Root: {ik.references.root.position.y}");
        Debug.Log($" avatarHeight: {avatarHeight}");
        float userHeight = headTracker.position.y - ik.references.root.position.y; // ユーザーの身長
        Debug.Log($"現在のトラッカー　頭：{headTracker.position.y}");
        Debug.Log($" userHeight: {userHeight}");
        savedScale = userHeight / avatarHeight; // スケール倍率
        ik.references.root.localScale = Vector3.one * savedScale;
        Debug.Log($"👤 身長スケール適用: {savedScale}");

        savedHeadScaleY = headTracker.position.y;
        savedPelvisScaleY = bodyTracker.position.y;



        // VRIKのスケール設定を適用
        ik.solver.FixTransforms();  // ボーンの再適用

        // 腕の長さで調節 1
        //float avatarArmLength = Vector3.Distance(ik.references.rightHand.position, ik.references.leftHand.position);
        //float userArmLength = Vector3.Distance(rightHandTracker.position, leftHandTracker.position);
        //savedLeftArmScale = userArmLength / avatarArmLength;
        //savedRightArmScale = userArmLength / avatarArmLength;*

        // 腕の長さで調節 ２
        float avatarSholderLength = Vector3.Distance(ik.references.leftUpperArm.position, ik.references.rightUpperArm.position);
        float avatarArmLength = Vector3.Distance(ik.references.rightHand.position, ik.references.leftHand.position) - avatarSholderLength;
        float userArmLength = Vector3.Distance(rightHandTracker.position, leftHandTracker.position) * 0.94f - avatarSholderLength;
        savedLeftArmScale = userArmLength / avatarArmLength ;
        savedRightArmScale = userArmLength / avatarArmLength ;

        SavedScaleXZ = Vector3.Distance(new Vector3(rightHandTracker.position.x, 0, rightHandTracker.position.z), new Vector3(leftHandTracker.position.x, 0, leftHandTracker.position.z));

        // 適応
        ik.solver.leftArm.armLengthMlp = savedLeftArmScale;
        ik.solver.rightArm.armLengthMlp = savedRightArmScale;
        Debug.Log($"🔧 左腕 長さスケール適用: {savedLeftArmScale}");
        Debug.Log($"🔧 右腕 長さスケール適用: {savedRightArmScale}");

        // **足の長さの調整**
        savedLeftLegScale = AdjustLimbLength(ik.solver.leftLeg, leftFootTracker, "Left Leg");
        savedRightLegScale = AdjustLimbLength(ik.solver.rightLeg, rightFootTracker, "Right Leg");

        headToWaistY = headTracker.position.y - bodyTracker.position.y; // 頭 - 腰の高さ差
        armYBase = ((leftHandTracker.position.y + rightHandTracker.position.y) / 2f) - bodyTracker.position.y; // 腰から腕の基準高さ
       
        Debug.Log("✅ シンプルキャリブレーション完了");
    }

    private float AdjustLimbLength(IKSolverVR.Leg leg, Transform tracker, string limbName)
    {
        if (tracker == null) Debug.LogError("足のトラッカーない！（キャリブレーション）");

        // 足のターゲットまでの距離を計算し、スケール調整
        float targetDistance = Vector3.Distance(new Vector3(0,ik.references.pelvis.position.y,0), new Vector3(0,tracker.position.y,0));
        float boneDistance = Vector3.Distance(ik.references.pelvis.position, new Vector3 (ik.references.pelvis.position.x, 0, ik.references.pelvis.position.z));
        float scaleFactor = targetDistance / boneDistance;

        leg.legLengthMlp = scaleFactor;
        Debug.Log($"🔧 {limbName} 長さスケール適用: {scaleFactor}");

        return scaleFactor;
    }

    /// <summary>
    /// **キャリブレーションデータを取得**
    /// </summary>
    public CalibrationData GetCalibrationData()
    {
        return new CalibrationData
        {
            scale = savedScale,
            headScaleY = savedHeadScaleY,  // 頭の位置
            pelvisScaleY = savedPelvisScaleY,  // 腰の位置
            scaleXZ = SavedScaleXZ,   // Leaf Right の直接の距離
            leftArmScale = savedLeftArmScale,
            rightArmScale = savedRightArmScale,
            leftLegScale = savedLeftLegScale,
            rightLegScale = savedRightLegScale,
            calibrationHeadToWaistY = headToWaistY, //　腰から頭の高さ
            calibrationArmYBase = armYBase, // 腰から腕のy軸（平均）の高さ

        };
    }


    /// <summary>
    /// **キャリブレーションデータを適用**
    /// </summary>
    public void SetCalibrationData(CalibrationData calibrationData)
    {
        if (calibrationData == null)
        {
            Debug.LogError("⛔ キャリブレーションデータが無効です！");
            return;
        }

        savedScale = calibrationData.scale;
        savedHeadScaleY = calibrationData.headScaleY;
        savedPelvisScaleY = calibrationData.pelvisScaleY;
        SavedScaleXZ = calibrationData.scaleXZ;
        savedLeftArmScale = calibrationData.leftArmScale;
        savedRightArmScale = calibrationData.rightArmScale;
        savedLeftLegScale = calibrationData.leftLegScale;
        savedRightLegScale = calibrationData.rightLegScale;

        // **スケール適用**
        replayIk.references.root.localScale = Vector3.one * savedScale;
        replayIk.solver.leftArm.armLengthMlp = savedLeftArmScale;
        replayIk.solver.rightArm.armLengthMlp = savedRightArmScale;
        replayIk.solver.leftLeg.legLengthMlp = savedLeftLegScale;
        replayIk.solver.rightLeg.legLengthMlp = savedRightLegScale;

        Debug.Log("✅ お手本動作にキャリブレーションデータを適用しました！");
    }

    public void SetOrigignalCalibrationData(CalibrationData calibrationData)
    {
        if (calibrationData == null)
        {
            Debug.LogError("⛔ キャリブレーションデータが無効です！");
            return;
        }

        savedScale = calibrationData.scale;
        savedHeadScaleY = calibrationData.headScaleY;
        savedPelvisScaleY = calibrationData.pelvisScaleY;
        SavedScaleXZ = calibrationData.scaleXZ;
        savedLeftArmScale = calibrationData.leftArmScale;
        savedRightArmScale = calibrationData.rightArmScale;
        savedLeftLegScale = calibrationData.leftLegScale;
        savedRightLegScale = calibrationData.rightLegScale;

        // **スケール適用**
        originalReplayIk.references.root.localScale = Vector3.one * savedScale;
        originalReplayIk.solver.leftArm.armLengthMlp = savedLeftArmScale;
        originalReplayIk.solver.rightArm.armLengthMlp = savedRightArmScale;
        originalReplayIk.solver.leftLeg.legLengthMlp = savedLeftLegScale;
        originalReplayIk.solver.rightLeg.legLengthMlp = savedRightLegScale;

        Debug.Log("✅ お手本動作にキャリブレーションデータを適用しました！");
    }

}
