using UnityEngine;

public class AvatarTracking : MonoBehaviour
{
    public Transform headTracker;
    public Transform leftHandTracker;
    public Transform rightHandTracker;
    public Transform hipTracker;

    public Transform headBone;
    public Transform leftHandBone;
    public Transform rightHandBone;
    public Transform hipBone;

    void Update()
    {
        // トラッキングデータをボーンに反映
        if (headBone && headTracker)
        {
            headBone.position = headTracker.position;
            headBone.rotation = headTracker.rotation;
        }

        if (leftHandBone && leftHandTracker)
        {
            leftHandBone.position = leftHandTracker.position;
            leftHandBone.rotation = leftHandTracker.rotation;
        }

        if (rightHandBone && rightHandTracker)
        {
            rightHandBone.position = rightHandTracker.position;
            rightHandBone.rotation = rightHandTracker.rotation;
        }

        if (hipBone && hipTracker)
        {
            hipBone.position = hipTracker.position;
            hipBone.rotation = hipTracker.rotation;
        }
    }
}
