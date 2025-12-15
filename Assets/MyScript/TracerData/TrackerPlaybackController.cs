using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class TrackerPlaybackController : MonoBehaviour
{
    [Header("Tracker Data Player")]
    [SerializeField] private TrackerDataPlayer trackerDataPlayer; // TrackerDataPlayerの参照
    [SerializeField] private BoneConnector boneConnector;

    [Header("")]
    public ReplayAsObject ReplayAvatar;

    [Header("Tracker Recorder and Saver")]
    [SerializeField] private TrackerDataRecorder dataRecorder;
    [SerializeField] private TrackerDataSaver trackerSaver;
    [SerializeField] private SimpleVRIKCalibrationController calibrationController;

    [Header("reference Settings")]
    public Animator animator; // アニメーション再生用Animator
    public AnimationRecorder boneRecorder; // アニメーション記録用
    public AnimationDataSaver boneSaver; // データ保存用
    public AnimationEvaluator evaluator; // 評価用
    public RhythmAndCountdownController rhythmController; // リズムコントローラー
    public EvaluationText evaluationText; // テキスト表示

    [Header("Recoeder Button")]
    [SerializeField] private InputActionReference RecoderStartButton; // Recorderボタン
    [SerializeField] private InputActionReference RecoderStopButton;  // Stopボタン入力
    [Header("Replay Button")]
    [SerializeField] private InputActionReference ReplayStartButton; // Startボタン入力
    [SerializeField] private InputActionReference CalibrationButton; // Stopボタン入力

    [Header("Tracker for InitialPosition of Replay")]
    [SerializeField] private Transform WaistTracker;

    [Header("キャリブレーションする際の腕")]
    [SerializeField] private Transform leftArmTracker;  // 左腕のトラッカー
    [SerializeField] private Transform rightArmTracker; // 右腕のトラッカー
    private LineRenderer lineRenderer; // LineRenderer の参照



    [Header("is Evaluating")]
    public bool shouldEvaluate = false; // 評価を行うかどうかのフラグ
    public bool finalEvaluate = false; // 最終評価を行う
    private bool isCalibrationDone = false;  // キャリブレーションフラグ
    private bool isRecording = false; // 記録中かどうか
    private bool isPracticeMode = false;     // 練習モード（1回目のリプレイ中）
    private bool isPracticeModeDone = false;     // 練習モード（1回目のリプレイ中）
    private bool isReplaying = false;  //　再生中かどうか

    private float recordingDuration; // 基準データの記録時間
    private float recordingStartTime; // 記録開始時刻


    private void Start()
    {
        // コンポーネントの確認
        if (!ValidateComponents()) return;
        if (shouldEvaluate)
        {
            recordingDuration = dataRecorder.GetRecordingDuration(); // 基準データの記録時間を取得　(trackerから取得)
            Debug.Log($"recordingDuration{recordingDuration}");
            // 最初に取らないからいらない
            //  boneRecorder.SetReferenceData(boneSaver.LoadReferenceData());
        }
        if (finalEvaluate)
        {
            shouldEvaluate = true;
        }

        // GameObject に LineRenderer を追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f; // 線の太さを設定
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // デフォルトのマテリアル
        lineRenderer.positionCount = 2; // 始点と終点
    }

    private void Update()
    {
        // キャリブレーション中ならラインを表示
        if (!isCalibrationDone)
        {
            if (leftArmTracker != null && rightArmTracker != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, leftArmTracker.position);
                lineRenderer.SetPosition(1, rightArmTracker.position);
            }
        }
        else
        {
            // キャリブレーション完了後はラインを非表示
            lineRenderer.enabled = false;
        }


        // キャリブレーション中は他のボタンを押せない
        // **キャリブレーションが終わるまで、他のボタンを押せない**
        if (!isCalibrationDone)
        {
            // **キャリブレーションボタンのみ押せる**
            if (CalibrationButton.action.WasPressedThisFrame())
            {
                StartCoroutine(StartCalibrationRoutine());
            }
            return;  // **キャリブレーションが終わるまで、ここで処理を抜ける**
        }

        // リプレイ再生中
        if (isReplaying)　
        {
            // 基準データの記録時間に達したら記録を停止
            if (Time.time - recordingStartTime >= recordingDuration)
            {
                // 再生を停止
                trackerDataPlayer.StopPlayback();
                isReplaying = false;

            }
        }

        // 記録しているとき
        if (isRecording)　
        {
            // Debug.Log($"🔴 記録中... Time={Time.time:F4}");
            if (shouldEvaluate)
            {
                if (isPracticeModeDone)
                {
                    dataRecorder.RecordFrame(); // 経過時間が記録間隔を超えた場合に記録（Tracker）
                    boneRecorder.ProcessRecording(Time.deltaTime);　// 経過時間が記録間隔を超えた場合に記録（Bone）
                }
                else             // リプレイの記録　ボーンのみ
                {
                    boneRecorder.ProcessReplayRecording(Time.deltaTime);
                }
            }
            else  // 普通の記録の時は　トラッカーだけ
            {
                dataRecorder.RecordFrame(); // フレーム記録を手動で呼び出し 
            }

        }
        
        // 時間になったら記録ストップ！
        if (isRecording && shouldEvaluate)
        {
            // 基準データの記録時間に達したら記録を停止
            if (Time.time - recordingStartTime >= recordingDuration)
            {
                OnStopRecorderButtonPressed();
                isRecording = false;
                Debug.Log($"⏹ 記録終了 | Time={Time.time:F4}, Start={recordingStartTime:F4}, Duration={recordingDuration:F4}");
                Debug.Log(" StopRecord by  Update");
            }
        }

        // **練習モード中はボタンを無効化**
        if (isPracticeMode) return;

        /// 評価時
        /// キャリブレーションした後
        /// リプレイを一回以上しないと他の操作ができない
        if (shouldEvaluate)
        {
            if (!isPracticeModeDone)
            {
                if (ReplayStartButton.action.WasPressedThisFrame())
                {
                    StartCoroutine(StartPracticeReplay());  //記録するためのリプレイ
                    Debug.Log("1回目のリプレイ開始");
                }
                return;  // **キャリブレーションが終わるまで、ここで処理を抜ける**
            }
        }
        // Debug.Log("1回目のリプレイを抜けた！");


        //　レコーダースタート
        if (RecoderStartButton.action.WasPressedThisFrame() && !isRecording)
        {
            Debug.Log("▶️ 記録開始ボタン押下");
            OnRecorderButtonPressed();
            // Animationのレコーダースタート
        }

        // レコーダーストップ
        if (RecoderStopButton.action.WasPressedThisFrame() && isRecording && !shouldEvaluate)
        {
            Debug.Log("⏹ 記録停止ボタン押下");
            isRecording = false;
            OnStopRecorderButtonPressed();
        }

        // リプレイ
        if (ReplayStartButton.action.WasPressedThisFrame())
        {
            StartCoroutine(OnPlayButtonPressed());
            Debug.Log("🔁 リプレイ開始ボタン押下");
        }
    }

    private IEnumerator StartPracticeReplay()
    {
        if (isPracticeMode) yield break;  // 既に練習中なら無視
        isPracticeMode = true;  // 練習モード開始
        Debug.Log("⏳ 練習リプレイ開始（基準データを設定中）...");

        yield return StartCoroutine(OnPlayButtonPressed());  // 1回目のリプレイを再生　ここで待機？

        // **リプレイが完了したら基準データとして保存**
        Debug.Log("✅ リプレイ完了！基準のBoneデータを更新しました。");
        boneRecorder.SetReferenceData(boneSaver.LoadReferenceData());

        isPracticeMode = false;  // 練習モード終了（ボタン有効化）
        isPracticeModeDone = true; // 記録を取った。
        Debug.Log("🎯 通常モードに戻りました。");
    }


    /// <summary>
    /// Recorderボタンが押されたときの処理 (Boneの取得)
    /// </summary>
    void OnRecorderButtonPressed()
    {
        if (rhythmController != null)
        {
            if (shouldEvaluate)
            {
                // 評価するときだけ、基準データの記録時間を取得
                recordingDuration = dataRecorder.GetRecordingDuration(); // 基準データの記録時間を取得　(trackerから取得)
            }
            if (finalEvaluate)
            {
                ReplayAvatar.HideBonesAndLines();
            }
            StartCoroutine(StartRecordingWithDelay());
        }
        else
        {
            Debug.LogError("RhythmController is not assigned!");
        }
    }

    /// <summary>
    /// 記録を遅延して開始  Bone + Tracker
    /// </summary>
    IEnumerator StartRecordingWithDelay()
    {
        string[] countdown = { "3", "2", "1", "Start!" }; // カウントダウンテキスト
        double beatInterval = rhythmController.GetBeatInterval();
        double delayToNextBeat = rhythmController.GetDelayInterval();
        // yield return new WaitForSeconds(delayToNextBeat); // 次のビートに同期

        // **次のビートと同期する**
        double nextTargetBeat = AudioSettings.dspTime + delayToNextBeat;
        yield return new WaitUntil(() => AudioSettings.dspTime >= nextTargetBeat);

        foreach (string count in countdown)
        {
            evaluationText.CountDownTextChange(count); // カウントダウンを更新
            Debug.Log($"🕒 カウントダウン: {count} | 目標時間: {nextTargetBeat}");

            // **"3", "2", "1" のときのみ次のビートまで待機**
            if (count != "Start!")
            {
                nextTargetBeat += beatInterval;  // 次のビートタイミングを計算
                yield return new WaitUntil(() => AudioSettings.dspTime >= nextTargetBeat);
            }
        }

        // カウントダウン終了後に非表示
        evaluationText.CountDownTextChange("");

        // Animationをここでもう一度再生していた。

        recordingStartTime = Time.time;  // 記録時間を知るための印（スタート）
        Debug.Log($"スタート時間(記録){recordingStartTime}");
        ///
        //　ここを統合したい。
        ///


        if (shouldEvaluate)  // 評価するとき
        {
            if (!isPracticeModeDone)
            {
                //isRecording = true;　OnPlayButtonPressed()で処理されている
                StartCoroutine(OnPlayButtonPressed());
                //boneRecorder.StartRecording(); //  Bone の記録開始 OnPlayButtonPressed()で処理されている
                Debug.Log("基準データ用　記録開始（Bone）万が一！");
            }
            else　　
            {
                // isRecording = true;
                StartCoroutine(OnPlayButtonPressed()); // 1拍待ってからリプレイスタート
                // boneRecorder.StartRecording(); //  Bone の記録開始　OnPlayButtonPressed()で処理されている

                // var calibrationDataNow = calibrationController.GetCalibrationData();   //　記録時に必要となる腕の基準位置を取得andセット　
                
                // 今回は　評価するときの対象データのトラッカーは　自分の動きを再生しないのでいらない　
                // いずれいるかも
                // dataRecorder.SetCalibrationData(calibrationDataNow.calibrationHeadToWaistY, calibrationDataNow.calibrationArmYBase);
                // dataRecorder.StartRecording(shouldEvaluate);            // Trackerの記録開始

                Debug.Log("記録開始(shouldEvaluate →　リプレイ後)");
            }
        }
        else  // 普通に記録するとき
        {
            // Trackerのスタート準備
            isRecording = true;
            var calibrationDataNow = calibrationController.GetCalibrationData();　//　記録時に必要となる腕の基準位置を取得andセット
            dataRecorder.SetCalibrationData(calibrationDataNow.calibrationHeadToWaistY, calibrationDataNow.calibrationArmYBase);
            dataRecorder.StartRecording(shouldEvaluate);            // Trackerの記録開始
        }
    }

    /// <summary>
    /// 記録を停止 Bone + Tracker
    /// </summary>
    void OnStopRecorderButtonPressed()
    {
        Debug.Log(" 記録中止ボタン called ");
        if (shouldEvaluate)
        {
            if (!isPracticeModeDone)
            {
                boneRecorder.StopRecording();
                boneSaver.SaveReferenceData();   //Bone を記録する
                Debug.Log($"基準データを保存しました（リプレイより）");
                trackerDataPlayer.ResetPlayback(); // プレイヤーで一回目のプレイバックが終わったことを示す。
            }
            else
            {
                boneRecorder.StopRecording();
                // 評価を開始
                Debug.Log("結果が呼び出された");
                evaluator.EvaluateSimilarity(boneRecorder.GetSimilarityData());
                evaluationText.ShowEvaluateSimilarity();
            }
        }
        else
        {
            dataRecorder.StopRecording(shouldEvaluate);
            // 記録データを保存
            // boneSaver.SaveReferenceData();   //Bone
            trackerSaver.SaveTrackerData();    //Tracker
            trackerSaver.SaveInitialWaistPosition(); //InitialWaistPosition (Tracker)
            trackerSaver.SaveCalibrationData(calibrationController); // キャリブレーションデータ

            Debug.Log("Reference data saved.");
        }
    }


    /// <summary>
    /// 再生ボタンが押されたときの処理
    /// 1拍まってからの再生と同時にBoneも記録
    /// </summary>
    private IEnumerator OnPlayButtonPressed()
    {
        Debug.Log("トラッカーデータをロードします！");
        // 1回目の再生であったら　トラッカーデータをサーバーからロード 
        if (!isPracticeModeDone)
        {
            var loadedData = trackerSaver.LoadTrackerData();   //　トラッカーデータ
            var calibrationData = trackerSaver.LoadCalibrationData(); // キャリブレーションデータ
            Vector3 initialWaistPosition = trackerSaver.LoadInitialWaistPosition(); // 腰の初期位置
                                                                                    // 保存済みのキャリブレーションデータ
            float loadedHeadScaleY = calibrationData.headScaleY;
            float loadedPelvisScaleY = calibrationData.pelvisScaleY;
            float loadedScaleXZ = calibrationData.scaleXZ;

            var calibrationDataNow = calibrationController.GetCalibrationData();  // 現在のキャリブレーションデータ

            // データのチェック
            // キャリブレーションをしないと０が代入される
            if (calibrationDataNow.scale <= 0)
            {
                Debug.LogWarning("⚠️ ロードしたスケールが 0 以下のため、デフォルト値を適用します。");
                calibrationDataNow.scale = 1f;  // デフォルトスケール
            }

            // 現在のキャリブレーションデータ
            float currentHeadScaleY = calibrationDataNow.headScaleY;
            float currentPelvisScaleY = currentHeadScaleY;
            float currentScaleXZ = calibrationDataNow.scaleXZ;

            // **誤差を計算**
            float errorHeadY = Mathf.Abs(currentHeadScaleY - loadedHeadScaleY);
            float errorPelvisY = Mathf.Abs(currentPelvisScaleY - loadedPelvisScaleY);
            float errorXZ = Mathf.Abs(currentScaleXZ - loadedScaleXZ);

            // **誤差が5cm (0.05m) 未満なら処理を実行**
            if (errorHeadY < 0.05f && errorPelvisY < 0.05f && errorXZ < 0.05f)
            {
                Debug.Log("✅ キャリブレーション誤差が許容範囲内です！（5cm 未満）");
                // そのまま比較
            }

            else
            {
                Debug.LogWarning($"⚠ キャリブレーション誤差が大きすぎます: 頭誤差={errorHeadY * 100}cm, 腰誤差={errorPelvisY * 100}cm, XZ誤差={errorXZ * 100}cm");
                calibrationController.SetCalibrationData(calibrationDataNow); // リプレイに現在の体格を反映
                                                                              // 基準ボーンデータ、基準Trackerデータを修正する
                calibrationController.SetOrigignalCalibrationData(calibrationData); //オリジナルに適用
                Debug.Log("🔧 キャリブレーションデータを適用しました！");
            }
            if (loadedData == null)
            {
                Debug.LogError("TrackerData is not assigned!");
            }
            // 記録されたデータをTrackerDataPlayerに設定　2回目の再生だからすでにされている。
            trackerDataPlayer.SetRecordedData(loadedData, initialWaistPosition);
            trackerDataPlayer.SetCalibrationData(calibrationDataNow.calibrationHeadToWaistY, calibrationDataNow.calibrationArmYBase);

            trackerDataPlayer.SetOriginalCalibrationData(calibrationData.calibrationHeadToWaistY, calibrationData.calibrationArmYBase); //オリジナル再生時必要


            trackerDataPlayer.CalculateRatio(calibrationDataNow, calibrationData);  // 体格差の比率を計算
            Debug.Log("トラッカーデータ　セット　OK");
        }
         



        // 再生を開始
        // **リズム同期の処理**
        double delayToNextBeat = rhythmController.GetDelayInterval();
        double nextTargetBeat = AudioSettings.dspTime + delayToNextBeat;
        Debug.Log($"🎵 1拍待っています {delayToNextBeat} 秒 | 現在の DSP Time: {AudioSettings.dspTime}, 目標時間: {nextTargetBeat}");

        yield return new WaitUntil(() =>
        {
            bool condition = AudioSettings.dspTime >= nextTargetBeat;
            if (!condition)
            {
                Debug.Log($"⏳ 待機中... 現在の DSP Time: {AudioSettings.dspTime}, 目標時間: {nextTargetBeat}");
            }
            return condition;
        });

        Debug.Log($"🎵 1拍進みました！ 現在の DSP Time: {AudioSettings.dspTime}");
        //float delay = rhythmController.GetDelayInterval();  // リズムを同期するための時間を取得
        //float beatInterval = rhythmController.GetBeatInterval();
        //Debug.Log($"遅延時間　{delay}");
        //yield return new WaitForSeconds(delay); // 次のビートに同期


        ///trackerDataPlayerの初期位置をここで処理させると　リプレイアバターが移動する　（腰の位置）
        trackerDataPlayer.InitializePositon(new Vector3(WaistTracker.position.x, 0, WaistTracker.position.z));


        // リプレイスタート
        trackerDataPlayer.StartPlayback(dataRecorder.recordInterval); 
        isReplaying = true;
        recordingStartTime = Time.time;  // 記録時間を知るための印（スタート）
        if (shouldEvaluate)
        {
            if (!isPracticeModeDone) // 基準データを取っていなかったら ここで１回だけBoneデータがとられる
            {
                //基準データ（Bone）の記録時間を取得
                recordingDuration = dataRecorder.GetRecordingDuration(); // 基準データの記録時間を取得　(trackerから取得)
                recordingStartTime = Time.time;  // 記録時間を知るための印（スタート）
                isRecording = true;
                boneRecorder.StartReplayRecording(); //  Bone の記録開始
                Debug.Log("Re:基準データ用　記録開始（Bone）");

            }
            else // 2回目以降のリプレイ
            {
                recordingDuration = dataRecorder.GetRecordingDuration(); // 基準データの記録時間を取得　(trackerから取得)
                recordingStartTime = Time.time;  // 記録時間を知るための印（スタート）
                isRecording = true;
                boneRecorder.StartRecording(); //  Bone の記録開始
                Debug.Log("Re:2回目以降の記録開始（Bone）");
            }
        }
        //if (!isPracticeModeDone)  // 基準データを取っていなかったら ここで１回だけBoneデータがとられる
        //{
        //    if (rhythmController != null)
        //    {
        //        if (shouldEvaluate)
        //        {
        //            //基準データの記録時間を取得
        //            recordingDuration = dataRecorder.GetRecordingDuration(); // 基準データの記録時間を取得　(trackerから取得)
        //        }
        //        recordingStartTime = Time.time;  // 記録時間を知るための印（スタート）

        //        isRecording = true;
        //        boneRecorder.StartRecording(); //  Bone の記録開始
        //        Debug.Log("基準データ用　記録開始（Bone）");
        //    }
        //    else
        //    {
        //        Debug.LogError("RhythmController is not assigned!");
        //    }
        //}
        Debug.Log("Playback started.");
        // **レコーディングが終了するまで待機**
        while (isRecording)
        {
            yield return null; // 1フレーム待機
        }
    }

    /// <summary>
    /// 停止ボタンが押されたときの処理
    /// </summary>
    private void OnPlayStopButtonPressed()
    {
        if (trackerDataPlayer == null)
        {
            Debug.LogError("TrackerDataPlayer is not assigned!");
            return;
        }

        // 再生を停止
        trackerDataPlayer.StopPlayback();

        Debug.Log("Playback stopped.");
    }

    private IEnumerator StartCalibrationRoutine()
    {
        Debug.Log("⏳ キャリブレーション中... 他のボタンは押せません。");

        calibrationController.StartCalibration();  // キャリブレーション開始

        yield return new WaitForSeconds(1.5f);  // キャリブレーション完了を待つ (適宜調整)

        isCalibrationDone = true;  // **キャリブレーション完了フラグを true にする**
        Debug.Log("✅ キャリブレーション完了！他のボタンが押せるようになりました。");
    }



    /// <summary>
    /// 必要なコンポーネントが設定されているかを確認
    /// </summary>
    /// <returns>すべてのコンポーネントが正しく設定されていればtrue</returns>
    private bool ValidateComponents()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return false;
        }

        if (boneRecorder == null)
        {
            Debug.LogError("Recorder is not assigned!");
            return false;
        }

        if (boneSaver == null)
        {
            Debug.LogError("Saver is not assigned!");
            return false;
        }

        if (evaluator == null)
        {
            Debug.LogError("Evaluator is not assigned!");
            return false;
        }

        if (rhythmController == null)
        {
            Debug.LogError("RhythmController is not assigned!");
            return false;
        }
        if (trackerDataPlayer == null)
        {
            Debug.LogError("trackerDataPlayer is not assigned!");
            return false;
        }
        if (dataRecorder == null)
        {
            Debug.LogError("dataRecorder is not assigned!");
            return false;
        }
        if (trackerSaver == null)
        {
            Debug.LogError("trackerSaver is not assigned!");
            return false;
        }

        
        return true;
    }
}
