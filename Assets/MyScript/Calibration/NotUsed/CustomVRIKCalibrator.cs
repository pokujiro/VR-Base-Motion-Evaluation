using UnityEngine;
using RootMotion.FinalIK;
using static RootMotion.FinalIK.VRIKCalibrator;

public static class CustomVRIKCalibrator
{
    public static CalibrationData Calibrate(
        VRIK ik,
        VRIKCalibrator.Settings settings,
        Transform headTracker,
        Transform bodyTracker = null,
        Transform leftHandTracker = null,
        Transform rightHandTracker = null,
        Transform leftFootTracker = null,
        Transform rightFootTracker = null)
    {
        if (!ik.solver.initiated)
        {
            Debug.LogError("VRIK must be initiated before calibration.");
            return null;
        }

        if (headTracker == null)
        {
            Debug.LogError("Head tracker is required for calibration.");
            return null;
        }

        CalibrationData data = new CalibrationData();
        ik.solver.FixTransforms();

        // ==========================
        // 1️⃣ **全体のスケール調整**
        // ==========================
        Vector3 headPos = headTracker.position + headTracker.rotation * Quaternion.LookRotation(settings.headTrackerForward, settings.headTrackerUp) * settings.headOffset;
        ik.references.root.position = new Vector3(headPos.x, ik.references.root.position.y, headPos.z);

        Vector3 headForward = headTracker.rotation * settings.headTrackerForward;
        headForward.y = 0f;
        ik.references.root.rotation = Quaternion.LookRotation(headForward);

        Transform headTarget = ik.solver.spine.headTarget ?? new GameObject("Head Target").transform;
        headTarget.position = headPos;
        headTarget.rotation = ik.references.head.rotation;
        headTarget.parent = headTracker;
        ik.solver.spine.headTarget = headTarget;

        float sizeF = (headTarget.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
        ik.references.root.localScale *= sizeF * settings.scaleMlp;

        // ==========================
        // 2️⃣ **ボディのキャリブレーション**
        // ==========================
        if (bodyTracker != null)
        {
            Transform pelvisTarget = ik.solver.spine.pelvisTarget ?? new GameObject("Pelvis Target").transform;
            pelvisTarget.position = ik.references.pelvis.position;
            pelvisTarget.rotation = ik.references.pelvis.rotation;
            pelvisTarget.parent = bodyTracker;
            ik.solver.spine.pelvisTarget = pelvisTarget;

            ik.solver.spine.pelvisPositionWeight = settings.pelvisPositionWeight;
            ik.solver.spine.pelvisRotationWeight = settings.pelvisRotationWeight;

            ik.solver.plantFeet = false;
            ik.solver.spine.maxRootAngle = 180f;
        }
        else if (leftFootTracker != null && rightFootTracker != null)
        {
            ik.solver.spine.maxRootAngle = 0f;
        }

        // ==========================
        // 3️⃣ **手のキャリブレーション**
        // ==========================
        CalibrateArm(ik.solver.leftArm, leftHandTracker, ik.references.leftForearm, ik.references.leftHand, settings, true);
        CalibrateArm(ik.solver.rightArm, rightHandTracker, ik.references.rightForearm, ik.references.rightHand, settings, false);

        // ==========================
        // 4️⃣ **足のキャリブレーション**
        // ==========================
        CalibrateLeg(ik.solver.leftLeg, leftFootTracker, ik.references.leftCalf, ik.references.leftFoot, settings, true);
        CalibrateLeg(ik.solver.rightLeg, rightFootTracker, ik.references.rightCalf, ik.references.rightFoot, settings, false);

        // ==========================
        // 5️⃣ **ルートコントローラーの設定**
        // ==========================
        bool addRootController = bodyTracker != null || (leftFootTracker != null && rightFootTracker != null);
        var rootController = ik.references.root.GetComponent<VRIKRootController>();

        if (addRootController)
        {
            if (rootController == null) rootController = ik.references.root.gameObject.AddComponent<VRIKRootController>();
            rootController.Calibrate();
        }
        else
        {
            if (rootController != null) GameObject.Destroy(rootController);
        }

        ik.solver.spine.minHeadHeight = 0f;
        ik.solver.locomotion.weight = (bodyTracker == null && leftFootTracker == null && rightFootTracker == null) ? 1f : 0f;

        // キャリブレーションデータ保存
        data.scale = ik.references.root.localScale.y;
        data.head = new CalibrationData.Target(ik.solver.spine.headTarget);
        data.pelvis = new CalibrationData.Target(ik.solver.spine.pelvisTarget);
        data.leftHand = new CalibrationData.Target(ik.solver.leftArm.target);
        data.rightHand = new CalibrationData.Target(ik.solver.rightArm.target);
        data.leftFoot = new CalibrationData.Target(ik.solver.leftLeg.target);
        data.rightFoot = new CalibrationData.Target(ik.solver.rightLeg.target);

        return data;
    }

    // ==========================
    // 🟠 **手のキャリブレーション**
    // ==========================
    private static void CalibrateArm(IKSolverVR.Arm arm, Transform tracker, Transform forearm, Transform hand, VRIKCalibrator.Settings settings, bool isLeft)
    {
        if (tracker == null || hand == null) return;

        Transform handTarget = arm.target ?? new GameObject(isLeft ? "Left Hand Target" : "Right Hand Target").transform;
        handTarget.position = tracker.position + tracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp) * settings.handOffset;
        handTarget.rotation = tracker.rotation;
        handTarget.parent = tracker;
        arm.target = handTarget;
        arm.positionWeight = 1f;
        arm.rotationWeight = 1f;

        // 手の長さ調整
        float armLength = Vector3.Distance(forearm.position, tracker.position);
        float defaultArmLength = Vector3.Distance(forearm.position, hand.position);
        arm.armLengthMlp = armLength / defaultArmLength;
    }

    // ==========================
    // 🟢 **足のキャリブレーション**
    // ==========================
    private static void CalibrateLeg(IKSolverVR.Leg leg, Transform tracker, Transform calf, Transform foot, VRIKCalibrator.Settings settings, bool isLeft)
    {
        if (tracker == null || foot == null) return;

        Transform footTarget = leg.target ?? new GameObject(isLeft ? "Left Foot Target" : "Right Foot Target").transform;
        footTarget.position = tracker.position;
        footTarget.rotation = foot.rotation;
        footTarget.parent = tracker;
        leg.target = footTarget;
        leg.positionWeight = 1f;
        leg.rotationWeight = 1f;

        // 足の長さ調整
        float legLength = Vector3.Distance(calf.position, tracker.position);
        float defaultLegLength = Vector3.Distance(calf.position, foot.position);
        leg.legLengthMlp = legLength / defaultLegLength;
    }
}
