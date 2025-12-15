using UnityEngine;
using RootMotion.FinalIK;

public class CostomVRIKCalibrationController : MonoBehaviour
{
    [Header("VRIK Settings")]
    [Tooltip("Reference to the VRIK component on the avatar.")]
    public VRIK ik;

    [Tooltip("The settings for VRIK calibration.")]
    public VRIKCalibrator.Settings settings;

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

    [Header("Calibration Data")]
    public VRIKCalibrator.CalibrationData calibrationData = new VRIKCalibrator.CalibrationData();

    private bool isCalibrated = false;

    void Update()
    {
        // キャリブレーション開始（Cキー）
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCalibration();
        }

        // 再キャリブレーション（Dキー）
        if (Input.GetKeyDown(KeyCode.D))
        {
            ApplySavedCalibration();
        }

        // スケールのみ再調整（Sキー）
        if (Input.GetKeyDown(KeyCode.S))
        {
            RecalibrateScale();
        }
    }

    /// <summary>
    /// **キャリブレーションを開始**
    /// - 頭、手、足のトラッカーを考慮し、手足の長さも自動調整
    /// </summary>
    public void StartCalibration()
    {
        if (ik == null)
        {
            Debug.LogError("VRIK is not assigned!");
            return;
        }

        if (headTracker == null)
        {
            Debug.LogError("Head tracker is required for calibration.");
            return;
        }

        Debug.Log("Starting Calibration...");

        // キャリブレーション実行
        calibrationData = CustomVRIKCalibrator.Calibrate(
            ik,
            settings,
            headTracker,
            bodyTracker,
            leftHandTracker,
            rightHandTracker,
            leftFootTracker,
            rightFootTracker
        );

        if (calibrationData != null)
        {
            isCalibrated = true;
            Debug.Log("Calibration Successful.");
        }
        else
        {
            Debug.LogError("Calibration Failed.");
        }
    }

    /// <summary>
    /// **保存したキャリブレーションデータを適用**
    /// - 事前に `StartCalibration()` を実行してデータを保存しておく必要がある
    /// </summary>
    public void ApplySavedCalibration()
    {
        if (!isCalibrated)
        {
            Debug.LogError("No calibration data found. Please calibrate first.");
            return;
        }

        Debug.Log("Applying saved calibration...");

        // キャリブレーションデータを適用
        VRIKCalibrator.Calibrate(ik, calibrationData, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);

        Debug.Log("Calibration data applied.");
    }

    /// <summary>
    /// **スケールのみ再調整**
    /// - アバターのサイズを再調整する
    /// </summary>
    public void RecalibrateScale()
    {
        if (!isCalibrated)
        {
            Debug.LogError("Avatar must be calibrated first.");
            return;
        }

        Debug.Log("Recalibrating scale...");

        // スケールのみ再調整
        VRIKCalibrator.RecalibrateScale(ik, calibrationData, settings);

        Debug.Log("Scale recalibrated.");
    }
}
