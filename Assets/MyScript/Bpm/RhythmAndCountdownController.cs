using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


//  コルーチン　１
public class RhythmAndCountdownController : MonoBehaviour
{
    [Header("Settings")]
    public float bpm = 120f; // テンポ（BPM）
    public int beat = 1;

    [Header("UI Elements")]
    public GameObject rhythmIndicatorPrefab; // リズムのインジケーターのPrefab
    public Vector3 displayPosition = new Vector3(0, 2, 5); // リズムインジケーターを表示する位置
    public Vector3 scale = new Vector3(1, 1, 1); // インジケーターのスケール

    [Header("Audio")]
    public AudioSource headBeatSound; // 頭拍音を鳴らすAudioSource

    private GameObject rhythmIndicator; // リズムを視覚化するUI（例: ImageやCircleなど）
    private double nextBeatTime; // 次のビートのタイミング
    private double beatInterval; // 1拍の時間（秒）
    public int beatCount = 0; // 現在の拍数
    private bool isPlayingRhythm = true; // リズム再生中かどうか

    void Start()
    {
        // 1拍の間隔を計算
        beatInterval = 60f / bpm;

        // インジケーターを生成し、位置を設定
        if (rhythmIndicatorPrefab != null)
        {
            rhythmIndicator = Instantiate(rhythmIndicatorPrefab, displayPosition, Quaternion.identity);
            rhythmIndicator.transform.localScale = scale; // スケールを調整
            rhythmIndicator.SetActive(false); // 初期状態で非表示
        }
        else
        {
            Debug.LogError("Rhythm Indicator Prefab is not assigned!");
        }
        // **最初のビートタイミングを補正**
        double dspTime = AudioSettings.dspTime;
        nextBeatTime = dspTime + (beatInterval - (dspTime % beatInterval));
        // リズムを再生
        StartCoroutine(ShowRhythm());
    }

    /// <summary>
    /// リズムの再生を開始
    /// </summary>
    void StartRhythm()
    {
        isPlayingRhythm = true;
        StartCoroutine(ShowRhythm());
    }

    /// <summary>
    /// リズムをテンポに合わせて視覚的に表示
    /// </summary>
    /// <summary>
    /// リズムをテンポに合わせて視覚的に表示し、頭拍で音を鳴らす
    /// </summary>
    IEnumerator ShowRhythm()
    {
        nextBeatTime = AudioSettings.dspTime + beatInterval; // 初回のビート時間を設定

        while (isPlayingRhythm)
        {
            double currentTime = AudioSettings.dspTime;

            if (currentTime >= nextBeatTime)
            {
                if (headBeatSound != null)
                {
                    headBeatSound.PlayScheduled(nextBeatTime);
                }

                if (rhythmIndicator != null)
                {
                    StartCoroutine(FlashIndicator());
                }

                // **次のビートのタイミングを確実に更新**
                nextBeatTime += beatInterval;
                beatCount++;
            }

            yield return null; // **毎フレームチェック**
        }
    }

    /// <summary>
    /// リズムのインジケーターを一瞬点灯させる
    /// </summary>
    IEnumerator FlashIndicator()
    {
        rhythmIndicator.SetActive(true);
        yield return new WaitForSeconds((float)(beatInterval / 2));
        rhythmIndicator.SetActive(false);
    }

    // 次の拍までの遅延時間を変えす。
    public double GetDelayInterval()
    {
        double currentTime = AudioSettings.dspTime;
        return nextBeatTime - currentTime;
    }


    // 1拍の時間（秒）を返す。
    public double GetBeatInterval()
    {
        return beatInterval;
    }
}
