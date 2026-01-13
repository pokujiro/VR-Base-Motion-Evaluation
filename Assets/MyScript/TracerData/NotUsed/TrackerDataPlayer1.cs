using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
///  コルーチン　PlayBack関数で
/// </summary>
public class TrackerDataPlayer1 : MonoBehaviour
{
    [Header("Playback Settings (Objects to be moved)")] // 動かす対象のオブジェクト
    [SerializeField] private Transform headTransform; // 頭のトラッカーのTransform　
    [SerializeField] private Transform leftHandTransform; // 左手のトラッカーのTransform
    [SerializeField] private Transform rightHandTransform; // 右手のトラッカーのTransform
    [SerializeField] private Transform leftFootTransform; // 左足のトラッカーのTransform
    [SerializeField] private Transform rightFootTransform; // 右足のトラッカーのTransform
    [SerializeField] private Transform waistTransform; // 腰のトラッカーのTransform

    [SerializeField] private float playbackInterval = 0.1f; // データを再生する間隔（秒）　レコーダーのインターバルと同じでなければならない
    private List<List<TrackerDataRecorder.TrackerData>> recordedData; // 記録されたトラッカーデータ
    private bool isPlaying = false; // 再生中かどうかを示すフラグ

    /// <summary>
    /// 記録されたデータを設定
    /// </summary>
    /// <param name="data">記録されたトラッカーデータのリスト</param>
    public void SetRecordedData(List<List<TrackerDataRecorder.TrackerData>> data)
    {
        recordedData = data; // 記録データをセット
    }

    /// <summary>
    /// データの再生を開始
    /// </summary>
    public void StartPlayback()
    {
        // 再生データが存在しない場合はエラーを表示
        if (recordedData == null || recordedData.Count == 0)
        {
            Debug.LogError("No data to play."); // データがない場合のエラーログ
            return;
        }

        // 再生が開始されていない場合のみ再生を開始
        if (!isPlaying)
        {
            isPlaying = true; // 再生フラグを設定
            StartCoroutine(Playback()); // 再生コルーチンを開始
        }
    }

    /// <summary>
    /// データの再生を停止
    /// </summary>
    public void StopPlayback()
    {
        isPlaying = false; // 再生フラグを解除
        StopAllCoroutines(); // 再生中のコルーチンを全て停止

        // すべてのコルーチンを停止するので　リズムと合わせたときにエラーが起きるかもしれない
        //
        //
        //
    }

    /// <summary>
    /// 記録データを順番に再生するコルーチン
    /// </summary>
    private IEnumerator Playback()
    {
        foreach (var frameData in recordedData) // フレームごとのデータをループ
        {
            if (!isPlaying) yield break; // 再生フラグが解除されていたら終了

            foreach (var tracker in frameData) // 各トラッカーのデータを適用
            {
                ApplyTrackerData(tracker); // トラッカーのデータを適用
            }

            yield return new WaitForSeconds(playbackInterval); // 再生間隔を待機
        }

        Debug.Log("Playback finished."); // 再生完了ログ
        isPlaying = false; // 再生フラグを解除
    }

    /// <summary>
    /// トラッカーデータを適用してTransformを更新
    /// </summary>
    /// <param name="trackerData">トラッカーの位置と回転データ</param>
    private void ApplyTrackerData(TrackerDataRecorder.TrackerData trackerData)
    {
        switch (trackerData.name) // トラッカー名に応じて適用先を選択
        {
            case "Head": // 頭のトラッカー
                headTransform.position = trackerData.position;
                headTransform.rotation = trackerData.rotation;
                break;
            case "LeftHand": // 左手のトラッカー
                leftHandTransform.position = trackerData.position;
                leftHandTransform.rotation = trackerData.rotation;
                break;
            case "RightHand": // 右手のトラッカー
                rightHandTransform.position = trackerData.position;
                rightHandTransform.rotation = trackerData.rotation;
                break;
            case "LeftFoot": // 左足のトラッカー
                leftFootTransform.position = trackerData.position;
                leftFootTransform.rotation = trackerData.rotation;
                break;
            case "RightFoot": // 右足のトラッカー
                rightFootTransform.position = trackerData.position;
                rightFootTransform.rotation = trackerData.rotation;
                break;
            case "Waist": // 腰のトラッカー
                waistTransform.position = trackerData.position;
                waistTransform.rotation = trackerData.rotation;
                break;
        }
    }
}
