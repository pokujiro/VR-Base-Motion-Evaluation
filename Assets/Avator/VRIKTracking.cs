using UnityEngine;
using RootMotion.FinalIK;

public class VRIKTracking : MonoBehaviour
{
    public VRIK vrik; // アバターにアタッチしたVRIKコンポーネント

    // トラッキングデバイスのTransform
    public Transform headTracker;
    public Transform leftHandTracker;
    public Transform rightHandTracker;
    public Transform hipTracker;
    public Transform leftFootTracker;
    public Transform rightFootTracker;

    void Start()
    {
        if (vrik == null)
        {
            Debug.LogError("VRIKコンポーネントが設定されていません。");
            return;
        }

        // トラッキングデバイスをVRIKのターゲットに設定
        if (headTracker != null)
            vrik.solver.spine.headTarget = headTracker;

        if (leftHandTracker != null)
            vrik.solver.leftArm.target = leftHandTracker;

        if (rightHandTracker != null)
            vrik.solver.rightArm.target = rightHandTracker;

        if (hipTracker != null)
            vrik.solver.spine.pelvisTarget = hipTracker;

        if (leftFootTracker != null)
            vrik.solver.leftLeg.target = leftFootTracker;

        if (rightFootTracker != null)
            vrik.solver.rightLeg.target = rightFootTracker;

        // 必要に応じて重み（影響度）を調整
        vrik.solver.spine.positionWeight = 1.0f;
        vrik.solver.spine.rotationWeight = 1.0f;
    }
}
