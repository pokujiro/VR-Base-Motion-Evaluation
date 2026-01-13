using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class TrackerDataToFinalIK : MonoBehaviour
{
    [Header("VRIK Reference")]
    public VRIK vrik; // VRIKコンポーネント

    [Header("Tracker Transforms")]
    public Transform headTracker;
    public Transform leftHandTracker;
    public Transform rightHandTracker;
    public Transform leftFootTracker;
    public Transform rightFootTracker;
    public Transform waistTracker;

    private List<List<TrackerDataRecorder.TrackerData>> trackerData; // 記録されたトラッカーデータ
    private int currentFrame = 0; // 再生中のフレームインデックス
    public float playbackInterval = 0.1f; // 再生間隔

    private bool isPlaying = false;

    /// <summary>
    /// トラッカーデータを設定する
    /// </summary>
    public void SetTrackerData(List<List<TrackerDataRecorder.TrackerData>> data)
    {
        trackerData = data;
    }

    /// <summary>
    /// 再生を開始する
    /// </summary>
    public void StartPlayback()
    {
        if (trackerData == null || trackerData.Count == 0)
        {
            Debug.LogError("No tracker data available for playback.");
            return;
        }

        isPlaying = true;
        currentFrame = 0;
        InvokeRepeating(nameof(ApplyTrackerDataToVRIK), 0f, playbackInterval);
    }

    /// <summary>
    /// 再生を停止する
    /// </summary>
    public void StopPlayback()
    {
        isPlaying = false;
        CancelInvoke(nameof(ApplyTrackerDataToVRIK));
    }

    /// <summary>
    /// トラッカーデータをVRIKに適用する
    /// </summary>
    private void ApplyTrackerDataToVRIK()
    {
        if (!isPlaying || trackerData == null || currentFrame >= trackerData.Count)
        {
            StopPlayback();
            return;
        }

        var frameData = trackerData[currentFrame];

        foreach (var tracker in frameData)
        {
            switch (tracker.name)
            {
                case "Head":
                    ApplyTarget(headTracker, tracker);
                    vrik.solver.spine.headTarget = headTracker;
                    break;
                case "LeftHand":
                    ApplyTarget(leftHandTracker, tracker);
                    vrik.solver.leftArm.target = leftHandTracker;
                    break;
                case "RightHand":
                    ApplyTarget(rightHandTracker, tracker);
                    vrik.solver.rightArm.target = rightHandTracker;
                    break;
                case "LeftFoot":
                    ApplyTarget(leftFootTracker, tracker);
                    vrik.solver.leftLeg.target = leftFootTracker;
                    break;
                case "RightFoot":
                    ApplyTarget(rightFootTracker, tracker);
                    vrik.solver.rightLeg.target = rightFootTracker;
                    break;
                case "Waist":
                    ApplyTarget(waistTracker, tracker);
                    vrik.solver.spine.pelvisTarget = waistTracker;
                    break;
            }
        }

        currentFrame++;
    }

    /// <summary>
    /// トラッカーデータをTransformに適用する
    /// </summary>
    private void ApplyTarget(Transform targetTransform, TrackerDataRecorder.TrackerData trackerData)
    {
        if (targetTransform == null) return;

        targetTransform.position = trackerData.position;
        targetTransform.rotation = trackerData.rotation;
    }
}

