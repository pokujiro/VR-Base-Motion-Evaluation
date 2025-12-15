using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RhythmController : MonoBehaviour
{
    public GameObject rhythmIndicatorPrefab; // リズムのインジケーターのPrefab
    public float beatInterval = 1.0f; // リズムの間隔（秒）
    public Vector3 displayPosition = new Vector3(0, 2, 5); // リズムインジケーターを表示する位置
    public Vector3 scale = new Vector3(1, 1, 1); // インジケーターのスケール



    private GameObject rhythmIndicator; // インジケーターのインスタンス
    private bool isRhythmPlaying = true; // リズムの再生フラグ

    void Start()
    {
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

        // リズムを再生
        StartCoroutine(PlayRhythm());
    }

    IEnumerator PlayRhythm()
    {
        while (isRhythmPlaying)
        {
            if (rhythmIndicator != null)
            {
                rhythmIndicator.SetActive(true); // インジケーターを表示
                yield return new WaitForSeconds(beatInterval / 2); // 点灯時間

                rhythmIndicator.SetActive(false); // インジケーターを非表示
                yield return new WaitForSeconds(beatInterval / 2); // 消灯時間
            }
        }
    }

    public void StopRhythm()
    {
        isRhythmPlaying = false; // リズムを停止
        if (rhythmIndicator != null)
            rhythmIndicator.SetActive(false); // インジケーターを非表示
    }

    public void StartRhythm()
    {
        if (!isRhythmPlaying)
        {
            isRhythmPlaying = true;
            StartCoroutine(PlayRhythm()); // リズムを再開
        }
    }
}

