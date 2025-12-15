using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using static TrackerDataSaver;
using static TrackerDataRecorder;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using System;



/// <summary>
///  コルーチン　PlayBack関数で
/// </summary>
public class TrackerDataPlayer : MonoBehaviour
{
    [Header("Playback Settings (Objects to be moved)")] // 動かす対象のオブジェクト
    [SerializeField] private Transform headTransform; // 頭のトラッカーのTransform　
    [SerializeField] private Transform leftHandTransform; // 左手のトラッカーのTransform
    [SerializeField] private Transform rightHandTransform; // 右手のトラッカーのTransform
    [SerializeField] private Transform leftFootTransform; // 左足のトラッカーのTransform
    [SerializeField] private Transform rightFootTransform; // 右足のトラッカーのTransform
    [SerializeField] private Transform waistTransform; // 腰のトラッカーのTransform

    [Header("Playback Settings for OriginalReplay(Objects to be moved)")] // 動かす対象のオブジェクト
    [SerializeField] private Transform headTransformOriginal; // 頭のトラッカーのTransform　
    [SerializeField] private Transform leftHandTransformOriginal; // 左手のトラッカーのTransform
    [SerializeField] private Transform rightHandTransformOriginal; // 右手のトラッカーのTransform
    [SerializeField] private Transform leftFootTransformOriginal; // 左足のトラッカーのTransform
    [SerializeField] private Transform rightFootTransformOriginal; // 右足のトラッカーのTransform
    [SerializeField] private Transform waistTransformOriginal; // 腰のトラッカーのTransform

    [Header("Global Transform Adjustments")]
    [SerializeField] private Vector3 globalPositionOffset = Vector3.zero; // 全体の平行移動オフセット
    [SerializeField] private Vector3 globalRotationOffset = Vector3.zero; // 全体の回転オフセット
    [SerializeField] private Transform mirrorBaseTransform; // アバターのTransform（基準となるオブジェクト）

    [Header("Playback Settings (Objects to be moved)")] // 動かす対象のオブジェクト
    public bool applayInitialWaistPosition = true;
    [SerializeField] private Vector3 InitialReplayPosition;

    [Header("元データ再生")]
    public bool isOriginal = false;
    private Vector3  originalWaistPosition;
    //　再生する際に必要となる腕の基準位置
    private float recordedOriginalHandBaselineY;
    private float recordedOriginalHeadToWaistY;

    [Header("Calibration Ratio")]
    private float headRatio = 1.5f;
    private float pelvisRatio = 1.5f;
    private float armRatio = 1.5f;
    private float legRatio = 1.5f;

    private List<List<TrackerDataRecorder.TrackerData>> recordedData; // 記録されたトラッカーデータ
    private bool isPlaying = false; // 再生中かどうかを示すフラグ
    private float playbackInterval; // データを再生する間隔（秒）　レコーダーのインターバルと同じでなければならない
    private Vector3 InitialwaistPosition;

    //　再生する際に必要となる腕の基準位置
    private float recordedHandBaselineY;
    private float recordedHeadToWaistY;

    private int frameIndex = 0;       // 再生するフレームのインデックス
    private float nextFrameTime = 0f; // 次のフレームを再生する時間
    private double lastFrameTime = 0f; // 最後にフレームを更新した時間
    float timeSinceLastPlayback;


        // 再生時
        // 各フレームごとに保存するトラッカーのオフセットデータ（位置 + 回転）
    [System.Serializable]
    private struct TrackerOffsetData
    {
        public Vector3 position;
        public Quaternion rotation;

        public TrackerOffsetData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }
    private Dictionary<int, Dictionary<string, TrackerOffsetData>> savedOffsetsPerFrame = new Dictionary<int, Dictionary<string, TrackerOffsetData>>();
    private bool isFirstPlayback = true; // 最初の再生かどうか


    void Start()
    {
        // 初期位置の記録 最初はトラッカーの位置が反映されていない
        //InitialReplayPosition = headTransform.transform.position;
        Debug.Log($"動かす対象のオブジェクト　初期位置：{InitialReplayPosition}");

    }

    /// <summary>
    /// 初期位置（スタート位置）を指定
    /// </summary>
    public void InitializePositon(Vector3 InitilPosition) 
    {
        if (applayInitialWaistPosition)
        {
            InitialReplayPosition = InitilPosition;
        }
    }   

    /// <summary>
    /// 記録されたデータを設定
    /// </summary>
    /// <param name="data">記録されたトラッカーデータのリスト</param>
    public void SetRecordedData(List<List<TrackerDataRecorder.TrackerData>> data, Vector3 InitialWaistPosition)
    {
        recordedData = data; // 記録データをセット
        InitialwaistPosition = InitialWaistPosition;
        Debug.Log($"InitialwaistPosition: {InitialwaistPosition}");
        InitialwaistPosition = new Vector3(0, InitialWaistPosition.y, 0);
        originalWaistPosition = new Vector3(0, InitialWaistPosition.y, 0);
    }

    public void SetCalibrationData(float headToWaistY, float armYBase)
    {
        recordedHandBaselineY = armYBase; // 頭 - 腰の高さ差
        recordedHeadToWaistY = headToWaistY; // 腰から腕の基準高さ
        Debug.Log($"📏 キャリブレーションデータセット(再生時): ArmYBase={recordedHandBaselineY}, HeadToWaist={recordedHeadToWaistY}");
    }

    public void SetOriginalCalibrationData(float headToWaistY, float armYBase)
    {
        recordedOriginalHandBaselineY = armYBase; // 頭 - 腰の高さ差
        recordedOriginalHeadToWaistY = headToWaistY; // 腰から腕の基準高さ
        Debug.Log($"📏 キャリブレーションデータセット(オリジナル）: ArmYBase={recordedHandBaselineY}, HeadToWaist={recordedHeadToWaistY}");
    }

    /// <summary>
    /// 現在と基準データの体格差の比率を計算
    /// </summary>
    /// <param name="currentData"></param>
    /// <param name="referenceData"></param>
    public void CalculateRatio(CalibrationData currentData, CalibrationData referenceData)
    {
        // 変更したためここでおかしくなるかも
        headRatio = (currentData.headScaleY- currentData.pelvisScaleY) / (referenceData.headScaleY - referenceData.pelvisScaleY);
        Debug.Log($"headRatio = {currentData.headScaleY} / {referenceData.headScaleY};");
        Debug.Log("頭の比率を変えたからおかしくなるかも");
        pelvisRatio = currentData.pelvisScaleY / referenceData.pelvisScaleY;
        Debug.Log($"pelvisRatio = {currentData.pelvisScaleY} / {referenceData.pelvisScaleY};");
        armRatio = currentData.scaleXZ / referenceData.scaleXZ;
        Debug.Log($"armRatio = {currentData.scaleXZ} / {referenceData.scaleXZ};");
        legRatio = currentData.pelvisScaleY / referenceData.pelvisScaleY;
        Debug.Log($"legRatio = {currentData.pelvisScaleY} / {referenceData.pelvisScaleY};");

        InitialwaistPosition.y *= legRatio; // 初期腰位置を腰の高さの比率に合わせる

        Debug.Log($"頭の比率: {headRatio} \n" +
            $"腰の比率: {pelvisRatio} \n" +
            $"腕の比率: {armRatio} \n" +
            $"足の比率: {legRatio}");

    }


    /// <summary>
    /// データの再生を開始
    /// </summary>
    public void StartPlayback(float playBackInterval)
    {
        // 再生データが存在しない場合はエラーを表示
        if (recordedData == null || recordedData.Count == 0)
        {
            Debug.LogError("No data to play."); // データがない場合のエラーログ
            return;
        }

        playbackInterval = playBackInterval;

        // 再生が開始されていない場合のみ再生を開始
        if (!isPlaying)
        {
            timeSinceLastPlayback = 0f; // ✅ 再生開始時刻を記録
            frameIndex = 0;
            isPlaying = true;                     // 再生フラグを設定
        }
    }

    /// <summary>
    /// データの再生を停止
    /// </summary>
    public void StopPlayback()
    {
        //isPlaying = false; // 再生フラグを解除
        //StopAllCoroutines(); // 再生中のコルーチンを全て停止

        // すべてのコルーチンを停止するので　リズムと合わせたときにエラーが起きるかもしれない
        //
        //
        //public void StopPlayback()

        isPlaying = false; // 再生フラグを解除
        frameIndex = 0;    // インデックスをリセット
        Debug.Log("Playback stopped.");
    }

    /// <summary>
    /// 記録データを順番に再生するコルーチン
    /// </summary>
    //private IEnumerator Playback()
    //{
    //    foreach (var frameData in recordedData) // フレームごとのデータをループ
    //    {
    //        if (!isPlaying) yield break; // 再生フラグが解除されていたら終了

    //        foreach (var tracker in frameData) // 各トラッカーのデータを適用
    //        {
    //            ApplyTrackerData(tracker); // トラッカーのデータを適用
    //        }

    //        yield return new WaitForSeconds(playbackInterval); // 再生間隔を待機 // WaitForSeconds(playbackInterval)は正確ではない
    //    }

    //    Debug.Log("Playback finished."); // 再生完了ログ
    //    isPlaying = false; // 再生フラグを解除
    //}
    //private IEnumerator Playback()
    //{
    //    float startTime = Time.time;  // 再生開始時間
    //    int frameIndex = 0; // 現在のフレームインデックス
    //    float interval = playbackInterval; // 目標間隔（0.1秒など）

    //    while (frameIndex < recordedData.Count)
    //    {
    //        if (!isPlaying) yield break; // 再生フラグが解除されていたら終了

    //        float targetTime = startTime + (frameIndex * interval); // 次のフレームの目標時間
    //        float currentTime = Time.time;

    //        if (currentTime >= targetTime) // 目標時間に達したら再生
    //        {
    //            var frameData = recordedData[frameIndex];

    //            // **同じフレーム内で基準値を取得**
    //            float frameHeadY = 0f;
    //            float frameWaistY = 0f;
    //            float frameHeadToWaistY = 1f;

    //            foreach (var tracker in frameData)
    //            {
    //                if (tracker.name == "Head") frameHeadY = tracker.position.y;
    //                if (tracker.name == "Waist") frameWaistY = tracker.position.y;
    //            }
    //            frameHeadToWaistY = frameHeadY - frameWaistY; // そのフレームの頭と腰の距離

    //            foreach (var tracker in frameData) // 各トラッカーのデータを適用
    //            {
    //                ApplyTrackerData(tracker, frameHeadY, frameWaistY, frameHeadToWaistY);
    //            }

    //            frameIndex++; // 次のフレームへ
    //        }

    //        yield return null; // 毎フレーム更新してチェック
    //    }

    //    Debug.Log("Playback finished.");
    //    isPlaying = false;
    //}

       // 現在のフレームインデックス

    private void Update()
    {
        if (!isPlaying || frameIndex >= recordedData.Count) return;

        // 経過時間を加算
        timeSinceLastPlayback += Time.deltaTime;

        // **再生インターバルに達したらフレームを再生**
        if (timeSinceLastPlayback >= playbackInterval)
        {
            var frameData = recordedData[frameIndex];


            if (!isFirstPlayback)
            {
                ApplySavedOffsets(frameIndex);


                if (isOriginal)
                {
                    float frameHeadY = 0f;
                    float frameWaistY = 0f;

                    foreach (var tracker in frameData)
                    {
                        if (tracker.name == "Head") frameHeadY = tracker.position.y + originalWaistPosition.y;
                        if (tracker.name == "Waist") frameWaistY = tracker.position.y + originalWaistPosition.y;
                    }
                    float frameHeadToWaistY = frameHeadY - frameWaistY;

                    foreach (var tracker in frameData)
                    {
           
                        ApplyOriginalReplayTrackerData(tracker, frameHeadY, frameWaistY, frameHeadToWaistY, frameIndex);
                    }

                }
            }
            else
            {
                float frameHeadY = 0f;
                float frameWaistY = 0f;

                foreach (var tracker in frameData)
                {
                    if (tracker.name == "Head") frameHeadY = tracker.position.y * headRatio + InitialwaistPosition.y;
                    if (tracker.name == "Waist") frameWaistY = tracker.position.y * pelvisRatio + InitialwaistPosition.y;
                }

                float frameHeadToWaistY = frameHeadY - frameWaistY;

                foreach (var tracker in frameData)
                {
                    ApplyTrackerData(tracker, frameHeadY, frameWaistY, frameHeadToWaistY, frameIndex);
                }
            }

            frameIndex++;

            // デバッグ用：フレーム間隔の確認
            Debug.Log($"▶️ フレーム再生: Time={Time.time:F4}, 経過時間={timeSinceLastPlayback:F4}");

            // 経過時間をリセット
            timeSinceLastPlayback = 0f;
        }
    }

    private void ApplyOriginalReplayTrackerData(TrackerDataRecorder.TrackerData trackerData, float frameHeadY, float frameWaistY, float frameHeadToWaistY, int frameIndex)
    {
        Transform targetTransform = GetOriginalTargetTransform(trackerData.name);
        if (targetTransform == null) return;

        Vector3 adjustedPosition = trackerData.position;  // 各トラッカーの移動距離を代入

        // 基準値位置（ワールド座標）
        float currentHandBaselineY = recordedOriginalHandBaselineY * (frameHeadToWaistY / recordedOriginalHeadToWaistY) + frameWaistY;
        //Debug.Log($"currentHandBaselineY{currentHandBaselineY}");

        if (trackerData.name == "Head")
        {
            // 適応　記録された腰の位置（補正あり）＋　移動距離　＋　初期位置
            targetTransform.position = originalWaistPosition + adjustedPosition + InitialReplayPosition;
            targetTransform.rotation = trackerData.rotation;
        }

        else if (trackerData.name == "Waist")
        {
            // 適応　記録された腰の位置（補正あり）＋　移動距離　＋　初期位置
            targetTransform.position = originalWaistPosition + adjustedPosition + InitialReplayPosition;
            targetTransform.rotation = trackerData.rotation;

        }
        else if (trackerData.name == "LeftHand")
        {
            //Debug.Log($"{trackerData.name}保存");
            adjustedPosition.x += InitialReplayPosition.x;    // 実際の位置
            adjustedPosition.z += InitialReplayPosition.z;
            adjustedPosition.y = (trackerData.position.y) + currentHandBaselineY; // 実際の位置
            // 適応
            targetTransform.position = adjustedPosition;
            targetTransform.rotation = trackerData.rotation;
        }
        else if (trackerData.name == "RightHand")
        {
            //Debug.Log($"{trackerData.name}保存");
            adjustedPosition.x += InitialReplayPosition.x;    // 実際の位置
            adjustedPosition.z += InitialReplayPosition.z;
            adjustedPosition.y = (trackerData.position.y) + currentHandBaselineY; // 実際の位置
            // 適応
            targetTransform.position = adjustedPosition;
            targetTransform.rotation = trackerData.rotation;
        }
        else if (trackerData.name == "LeftFoot")
        {
            // 適応　記録された腰の位置（補正あり）＋　移動距離　＋　初期位置
            targetTransform.position = originalWaistPosition + adjustedPosition + InitialReplayPosition;
            targetTransform.rotation = trackerData.rotation;
            Debug.Log($"左足（オリジナル）{targetTransform.position}");
        }
        else if (trackerData.name == "RightFoot")
        {
            // 適応　記録された腰の位置（補正あり）＋　移動距離　＋　初期位置
            targetTransform.position = originalWaistPosition + adjustedPosition + InitialReplayPosition;
            targetTransform.rotation = trackerData.rotation;
        }
    }




    /// <summary>
    /// トラッカーデータを適用してTransformを更新（スケール補正適用）
    /// </summary>
    /// <param name="trackerData">トラッカーの位置と回転データ</param>
    private void ApplyTrackerData(TrackerDataRecorder.TrackerData trackerData, float frameHeadY, float frameWaistY, float frameHeadToWaistY, int frameIndex)
    {
        Transform targetTransform = GetTargetTransform(trackerData.name);
        if (targetTransform == null) return;

        Vector3 adjustedPosition = trackerData.position;  // 各トラッカーの移動距離を代入

        // **再生時の基準値を計算**　　基準の位置（ワールド座標）
        float currentHandBaselineY = recordedHandBaselineY * (frameHeadToWaistY / recordedHeadToWaistY) + frameWaistY;
        // Debug.Log($"recordedHandBaselineY: {recordedHandBaselineY}, frameHeadToWaistY:{frameHeadToWaistY}, recordedHeadToWaistY{recordedHeadToWaistY}, frameWaistY: {frameWaistY}");




        // **1回目の再生時に移動距離を保存**  腰の位置を考慮した
        if (isFirstPlayback)
        {
            Debug.Log("1回目の再生（計算してます）");
            if (trackerData.name == "Head")
            {
               // Debug.Log($"{trackerData.name}保存");

                if (!savedOffsetsPerFrame.ContainsKey(frameIndex)) // 空の入れ物を作成
                {
                    savedOffsetsPerFrame[frameIndex] = new Dictionary<string, TrackerOffsetData>();
                }

                //　どっちがいいか検証
                adjustedPosition *= headRatio;
                // adjustedPosition.y *= headRatio;

                // 適応　記録された腰の位置（補正あり）＋　移動距離　＋　初期位置
                targetTransform.position = InitialwaistPosition + adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;

                // 保存
                //        savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                //            targetTransform.position, // 移動距離を保存
                //　　　　targetTransform.rotation // 回転を保存
                //);
                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

                //        // 保存
                //        savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                //            new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
                //　　　　targetTransform.rotation // 回転を保存
                //);

            }
            else if (trackerData.name == "Waist")
            {
                //Debug.Log($"{trackerData.name}保存");
                adjustedPosition.y *= pelvisRatio;

                // 適応
                targetTransform.position = InitialwaistPosition + adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;

                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

            }
            else if (trackerData.name == "LeftHand")
            {
                //Debug.Log($"{trackerData.name}保存");
                adjustedPosition.x *= armRatio + InitialwaistPosition.x ;    // 実際の位置
                adjustedPosition.z *= armRatio + InitialwaistPosition.z ;
                adjustedPosition.y = (trackerData.position.y * armRatio) + currentHandBaselineY; // 実際の位置

                // 適応
                targetTransform.position = adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;

                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

            }
            else if (trackerData.name == "RightHand")
            {
                //Debug.Log($"{trackerData.name}保存");
                adjustedPosition.x *= armRatio + InitialwaistPosition.x;    // 実際の位置
                                                                            //Debug.Log($"InitialwaistPosition.x.{InitialwaistPosition.x}");
                adjustedPosition.z *= armRatio + InitialwaistPosition.z;
                adjustedPosition.y = (trackerData.position.y * armRatio) + currentHandBaselineY; // 実際の位置
                                                                                                 // Debug.Log($"armRatio:{armRatio},currentHandBaselineY:{currentHandBaselineY}, InitialwaistTransform.y:{InitialwaistTransform.y}frameWaistY: {frameWaistY}");

                // 適応
                targetTransform.position = adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;

                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

            }
            else if (trackerData.name == "LeftFoot")
            {
                //Debug.Log($"{trackerData.name}保存");
                adjustedPosition.y *= pelvisRatio;

                // 適応
                targetTransform.position = InitialwaistPosition + adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;

                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

            }
            else if (trackerData.name == "RightFoot")
            {
                //Debug.Log($"{trackerData.name}保存");
                adjustedPosition.y *= pelvisRatio;

                // 適応
                targetTransform.position = InitialwaistPosition + adjustedPosition + InitialReplayPosition;
                targetTransform.rotation = trackerData.rotation;
                // 保存
                savedOffsetsPerFrame[frameIndex][trackerData.name] = new TrackerOffsetData(
                    new Vector3(targetTransform.position.x - InitialwaistPosition.x - InitialReplayPosition.x, targetTransform.position.y, targetTransform.position.z - InitialwaistPosition.z - InitialReplayPosition.z), // 移動距離を保存
    　　　　　　　　targetTransform.rotation // 回転を保存
　　　　　　　　);

            }
        }
    }

    private void ApplySavedOffsets(int frameIndex)
    {
        if (!savedOffsetsPerFrame.ContainsKey(frameIndex)) return;

        foreach (var trackerEntry in savedOffsetsPerFrame[frameIndex])
        {
            string trackerName = trackerEntry.Key;
            TrackerOffsetData offsetData = trackerEntry.Value;

            Transform targetTransform = GetTargetTransform(trackerName);
            if (targetTransform == null) continue;

            // **保存されたオフセットを適用**
            targetTransform.position = offsetData.position +  InitialReplayPosition;
            targetTransform.rotation = offsetData.rotation;
        }
    }



    /// <summary>
    /// この関数を呼びだすと
    /// </summary>
    public void ResetPlayback()
    {
        isFirstPlayback = false;
        isPlaying = false; // 再生フラグを解除
        frameIndex = 0;    // インデックスをリセット
        Debug.Log("Playback stopped.");
    }

    /// <summary>
    /// トラッカー名に対応するTransformを取得
    /// </summary>
    private Transform GetTargetTransform(string trackerName)
    {
        switch (trackerName)
        {
            case "Head": return headTransform;
            case "LeftHand": return leftHandTransform;
            case "RightHand": return rightHandTransform;
            case "LeftFoot": return leftFootTransform;
            case "RightFoot": return rightFootTransform;
            case "Waist": return waistTransform;
            default: return null;
        }
    }

    /// <summary>
    /// トラッカー名に対応するTransformを取得
    /// </summary>
    private Transform GetOriginalTargetTransform(string trackerName)
    {
        switch (trackerName)
        {
            case "Head": return headTransformOriginal;
            case "LeftHand": return leftHandTransformOriginal;
            case "RightHand": return rightHandTransformOriginal;
            case "LeftFoot": return leftFootTransformOriginal;
            case "RightFoot": return rightFootTransformOriginal;
            case "Waist": return waistTransformOriginal;
            default: return null;
        }
    }
}
