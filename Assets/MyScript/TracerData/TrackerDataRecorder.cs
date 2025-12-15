using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// インスペクターからそれぞれのトラッカーの actionを代入する
/// ボタンを押すと、インターバルごとに記録がとられる。
/// 記録データをJSON形式で保存およびロードする機能を追加。
/// </summary>
public class TrackerDataRecorder : MonoBehaviour
{
    [Header("Tracker Transforms")]
    [SerializeField] private Transform HeadTracker;
    [SerializeField] private Transform LeftHandTracker;
    [SerializeField] private Transform RightHandTracker;
    [SerializeField] private Transform LeftFootTracker;
    [SerializeField] private Transform RightFootTracker;
    [SerializeField] private Transform WaistTracker;

    [Header("Settings")]
    [SerializeField] public float recordInterval = 0.1f; // 取得間隔（秒）

    private float timeSinceLastRecord = 0f;
    private Vector3 initialWaistPosition; // 腰の初期位置
    private bool isInitialized = false; // 初期位置が設定されたか

    private float recordingStartTime; // 記録開始時間
    private float recordingEvaluateStartTime; // 記録開始時間

    public float recordingEvaluateDuration { get; private set; } // 記録時間を保存(対象データ)
    public float recordingDuration { get; private set; } // 記録時間を保存 
                                                         // { get; private set; }: これはプロパティのアクセサー（アクセス方法）を定義しています：
                                                         // get: 誰でもこの値を読み取ることができます
                                                         // private set: このクラス内からのみ値を変更できます（外部からは変更不可）

    private const string RecordingDurationKey = "RecordingDuration"; // PlayerPrefs用のキー
                                                                     // const は定数を表し、この値は変更できません

    [System.Serializable]
    public class TrackerData
    {
        public string name; // トラッカーの名前（例: "Head", "LeftHand"）
        public Vector3 position; // 腰を基準にした相対位置
        public Quaternion rotation;
    }

    private List<List<TrackerData>> recordedData = new List<List<TrackerData>>(); // フレームごとのトラッカーデータ
    private bool isRecording = false;

    // キャリブレーション時のデータ（腕の基準値を保存）
    private float calibrationArmYBase;
    private float calibrationHeadToWaistY;


    private void Start()
    {
        // 基準データの記録時間をロード
        // PlayerPrefs.GetFloat() は保存されているデータを読み込むメソッドです
        // 第1引数 RecordingDurationKey: データを識別するためのキー
        // 第2引数 0f: データが存在しない場合のデフォルト値（この場合は0秒）
        recordingDuration = PlayerPrefs.GetFloat(RecordingDurationKey, 0f); 
        Debug.Log($"Loaded recording duration: {recordingDuration:F2} seconds");
    }


    public float GetRecordingDuration()
    {
        return recordingDuration;
    }

    /// <summary>
    /// 記録を開始
    /// </summary>
    public void StartRecording(bool isEvaluate)
    {
        isRecording = true;
        recordedData.Clear();
        timeSinceLastRecord = 0f;

        // 初回のみ、腰の初期位置を設定
        initialWaistPosition = WaistTracker.position;
        Debug.Log($"リプレイ腰の初期位置{initialWaistPosition}");
        if (initialWaistPosition.y == 0)
        {
            Debug.LogError("記録ミス");

        }

        isInitialized = true;
        if (!isEvaluate)  
        {
            recordingStartTime = Time.time; // 記録開始時間を保存
        }
        else  // 評価時
        {
            recordingEvaluateStartTime = Time.time; // 記録開始時間を保存(対象データ)
        }
        Debug.Log($"Recording started. Initial waist position set: {initialWaistPosition}");
    }

    /// <summary>
    /// 記録を停止
    /// </summary>
    public void StopRecording(bool isEvaluate)
    {
        isRecording = false;
        if (!isEvaluate)  // 基準データ記録時
        {
            recordingDuration = Time.time - recordingStartTime; // 基準データの記録時間を計算
            // 記録時間を保存
            // `PlayerPrefs.SetFloat()` でデータを保存します
            // -第1引数: 保存するデータのキー
            // -第2引数: 保存する値
            PlayerPrefs.SetFloat(RecordingDurationKey, recordingDuration);
            PlayerPrefs.Save();
            Debug.Log($"Tracker Recording stopped. Duration saved: {recordingDuration:F2} seconds");
        }
        else  // 評価時
        {
            recordingEvaluateDuration = Time.time - recordingEvaluateStartTime; // 評価対象データの記録時間を計算
            Debug.Log($"Evaluation Tracker recording stopped. Duration: {recordingEvaluateDuration:F2} seconds");
        }
        Debug.Log($"Tracer Recording stopped. Total frames recorded: {recordedData.Count}");
    }

    /// <summary>
    /// キャリブレーション時の腕の基準値をセット
    /// </summary>
    public void SetCalibrationData(float headToWaistY, float armYBase)
    {
        calibrationHeadToWaistY = headToWaistY; // 頭 - 腰の高さ差
        calibrationArmYBase = armYBase; // 腰から腕の基準高さ
        Debug.Log($"📏 キャリブレーションデータセット: ArmYBase={calibrationArmYBase}, HeadToWaist={calibrationHeadToWaistY}");
    }

    /// <summary>
    /// フレームごとのデータを記録
    /// </summary>
    public void RecordFrame()
    {
        if (!isRecording || !isInitialized) return;

        // **現在の時間を取得**　デバック用
        float currentTime = Time.time;

        // このインターバル計算が呼び出すことによってうまくいかないかも。
        // これまでは　update関数　で回していた。
        timeSinceLastRecord += Time.deltaTime;
        if (timeSinceLastRecord >= recordInterval)
        {
            // **デバッグ: フレーム記録開始**
            Debug.Log($"📝 フレーム記録開始: Time={currentTime:F4}, 経過時間={timeSinceLastRecord:F4}");

            var frameData = new List<TrackerData>
            {
                CreateTrackerData(HeadTracker, "Head"),
                CreateTrackerData(LeftHandTracker, "LeftHand", true),
                CreateTrackerData(RightHandTracker, "RightHand", true),
                CreateTrackerData(LeftFootTracker, "LeftFoot"),
                CreateTrackerData(RightFootTracker, "RightFoot"),
                CreateTrackerData(WaistTracker, "Waist")
                // ここに　フレーム　を入れることになるかも
            };

            recordedData.Add(frameData);

            // **デバッグ: 記録データの確認**
            // Debug.Log($"📌 記録フレーム {recordedData.Count} 件目 | 記録間隔={recordInterval}s");
            // Debug.Log($"Frame recorded. Total frames: {recordedData.Count}");
            // Debug.Log($"Frame recorded. {recordedData}");
            timeSinceLastRecord = 0f;
        }
    }
    private TrackerData CreateTrackerData(Transform tracker, string name, bool isArm = false)
    {
        var position = tracker.position - initialWaistPosition;

        // 腕の場合、補正を適用
        if (isArm)
        {
            position.y = CalculateArmYCorrection(tracker.position.y, WaistTracker.position.y, HeadTracker.position.y);
        }

        var rotation = tracker.rotation;
        return new TrackerData { name = name, position = position, rotation = rotation };
    }

    /// <summary>
    /// 腕のY軸位置を補正する
    /// </summary>
    private float CalculateArmYCorrection(float currentArmY, float currentWaistY, float currentHeadY)
    {
        // 記録時の頭-腰の比率
        float currentHeadToWaistY = currentHeadY - currentWaistY;
        float heightRatio = currentHeadToWaistY / calibrationHeadToWaistY; // 記録時の高さの比率

        // 補正された基準点を計算
        float correctedBaseY = (calibrationArmYBase * heightRatio) + currentWaistY;

        // 腕のY軸誤差を計算
        float correctedArmY = currentArmY - correctedBaseY;

        // Debug.Log($"🖐 腕Y軸補正: OriginalY={currentArmY}, BaseY={correctedBaseY}, CorrectedY={correctedArmY}");
        return correctedArmY;
    }

    /// <summary>
    /// 記録されたデータを取得
    /// </summary>
    public List<List<TrackerData>> GetRecordedData()
    {
        return recordedData;
    }

    public Vector3 GetInitialPositions()
    {
        return initialWaistPosition;
    }

}
