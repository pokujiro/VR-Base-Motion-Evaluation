using UnityEngine;

public class AvatarBoneAccess : MonoBehaviour
{
    public Animator animator;

    private Transform headBone;
    private Transform leftHandBone;
    private Transform rightHandBone;
    private Transform hipBone;

    void Start()
    {
        // ボーン情報の取得
        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);

        Debug.Log("ボーン情報取得完了");
        Debug.Log("lefthandの情報:" + leftHandBone);
    }
}
