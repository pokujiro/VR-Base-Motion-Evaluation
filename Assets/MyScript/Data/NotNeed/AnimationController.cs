using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// アニメーションを再生し、記録と評価を管理するコントローラ
/// void Update　１
/// </summary>
public class AnimationController : MonoBehaviour
{
    [Header("Settings")]
    public Animator animator; // アニメーション再生用Animator
    public AnimationRecorder recorder; // アニメーション記録用
    public AnimationDataSaver saver; // データ保存用
    public AnimationEvaluator evaluator; // 評価用
    public RhythmAndCountdownController rhythmController; // リズムコントローラー

    [Header("UI Elements")]
    public Text countdownText; // カウントダウン表示用のテキスト
    public Button startButton; // スタートボタン
    public Button stopButton; // ストップボタン

    [Header("is Evaluating")]
    public bool shouldEvaluate = false; // 評価を行うかどうかのフラグ

    //private bool isRecording = false; // 記録中かどうか
    //private bool animationFinished = false; // アニメーション終了状態
    //private float recordingDuration; // 基準データの記録時間
    //private float recordingStartTime; // 記録開始時刻

    void Start()
    {
        //// コンポーネントの確認
        //if (!ValidateComponents()) return;
        //// ボタンにリスナーを追加
        ////if (startButton != null)
        ////    startButton.onClick.AddListener(OnStartButtonPressed);

        //if (stopButton != null)
        //    stopButton.onClick.AddListener(OnStopButtonPressed);

        //// 最初はスタートボタンのみ表示
        //ToggleButtons(true);
    }

    //void Update()
    //{
    //    // 記録開始！
    //    if (isRecording && shouldEvaluate)
    //    {
    //        // 基準データの記録時間に達したら記録を停止
    //        if (Time.time - recordingStartTime >= recordingDuration)
    //        {
    //            StopRecording();
    //            animationFinished = true; // アニメーション終了フラグを設定
    //            Debug.Log($"Time.time: {Time.time}, recordingStartTime: {recordingStartTime}, recordingDuration: {recordingDuration}");
    //            Debug.Log(" StopRecord by  Update");
    //        }
    //    }

    //    if (animationFinished)
    //    {
    //        Debug.Log("Animation has finished!");

    //        // アニメーション終了後の処理
    //        animationFinished = false; // 処理後にリセット
    //    }
    //}

    /// <summary>
    /// ボタンの表示を切り替える
    /// </summary>
    /// <param name="showStart">スタートボタンを表示するかどうか</param>
    void ToggleButtons(bool showStart)
    {
        if (startButton != null)
            startButton.gameObject.SetActive(showStart);

        if (stopButton != null)
            stopButton.gameObject.SetActive(!showStart);
    }

    /// <summary>
    /// スタートボタンが押されたときの処理
    /// </summary>
    //void OnStartButtonPressed()
    //{
    //    if (rhythmController != null)
    //    {
    //        if (shouldEvaluate)
    //        {
    //            // 評価するときだけ、基準データの記録時間を取得
    //            recordingDuration = recorder.GetRecordingDuration(); // 基準データの記録時間を取得
    //        }
    //        float delay = rhythmController.GetDelayInterval();  // リズムを同期するための時間を取得
    //        StartCoroutine(StartRecordingWithDelay(delay));
    //    }
    //    else
    //    {
    //        Debug.LogError("RhythmController is not assigned!");
    //    }

    //    // ボタンを切り替え（ストップボタンを表示）
    //    ToggleButtons(false);
    //}

    /// <summary>
    /// 記録を遅延して開始
    /// </summary>
    //IEnumerator StartRecordingWithDelay(float delay)
    //{
    //    string[] countdown = { "3", "2", "1", "スタート!" }; // カウントダウンテキスト
    //    float beatInterval = rhythmController.GetBeatInterval();
    //    yield return new WaitForSeconds(delay); // 次のビートに同期
    //    foreach (string count in countdown)
    //    {
    //        countdownText.text = count; // カウントダウンを更新
    //        yield return new WaitForSeconds(beatInterval); // テンポに合わせて表示
    //        Debug.Log($" beatInterval is {beatInterval} ");
    //    }
    //    countdownText.text = ""; // カウントダウン終了後に非表示

    //    // Animator のステートを確認して再生
    //    int layerIndex = 0; // Animator のレイヤー
    //    if (animator.HasState(layerIndex, Animator.StringToHash("Boxing")))
    //    {
    //        animator.Play("Boxing", layerIndex, 0f); // "Boxing" ステートを再生
    //        Debug.Log("Animation Boxing started.");
    //    }
    //    else
    //    {
    //        Debug.LogError("State 'Boxing' not found in Animator Controller.");
    //    }

    //    recordingStartTime = Time.time;
    //    StartRecording();
    //    Debug.Log("記録開始　by AnimationController");

    //}

    /// <summary>
    /// ストップボタンが押されたときの処理
    /// </summary>
    //void OnStopButtonPressed()
    //{
    //    StopRecording();
    //    Debug.Log("記録終了　by AnimationController");

    //    // ボタンを切り替え（スタートボタンを表示）
    //    ToggleButtons(true);
    //}

    /// <summary>
    /// 記録を開始
    /// </summary>
    //void StartRecording()
    //{
    //    if (recorder == null)
    //    {
    //        Debug.LogError("Recorder is not assigned!");
    //        return;
    //    }

    //    recorder.StartRecording(shouldEvaluate);
    //    isRecording = true;
    //    Debug.Log("Recording started. (Animation Controller)");
    //}

    /// <summary>
    /// 記録を停止
    ///// </summary>
    //void StopRecording()
    //{
    //    ToggleButtons(true);
    //    if (recorder == null || saver == null || evaluator == null)
    //    {
    //        Debug.LogError("Recorder, Saver, or Evaluator is not assigned!");
    //        return;
    //    }

    //    recorder.StopRecording(shouldEvaluate);
    //    isRecording = false;

    //    // 評価フラグの確認
    //    if (shouldEvaluate)
    //    {
    //        // 評価を開始
    //        evaluator.EvaluateSimilarity();
    //    }
    //    else
    //    {
    //        // 記録データを保存
    //        saver.SaveReferenceData();
    //        Debug.Log("Reference data saved.");
    //    }
    //}

    /// <summary>
    /// 必要なコンポーネントが設定されているかを確認
    /// </summary>
    /// <returns>すべてのコンポーネントが正しく設定されていればtrue</returns>
    //private bool ValidateComponents()
    //{
    //    if (animator == null)
    //    {
    //        Debug.LogError("Animator is not assigned!");
    //        return false;
    //    }

    //    if (recorder == null)
    //    {
    //        Debug.LogError("Recorder is not assigned!");
    //        return false;
    //    }

    //    if (saver == null)
    //    {
    //        Debug.LogError("Saver is not assigned!");
    //        return false;
    //    }

    //    if (evaluator == null)
    //    {
    //        Debug.LogError("Evaluator is not assigned!");
    //        return false;
    //    }

    //    if (rhythmController == null)
    //    {
    //        Debug.LogError("RhythmController is not assigned!");
    //        return false;
    //    }

    //    return true;
    //}
}


