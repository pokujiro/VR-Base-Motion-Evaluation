using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用するための名前空間

// Update()で評価結果を表示
public class EvaluationText : MonoBehaviour
{
    public TextMeshProUGUI similarityText; // TextMeshPro-Text(UI)コンポーネント
    public TextMeshProUGUI countDownText; // TextMeshPro-Text(UI)コンポーネント
    private AnimationEvaluator evaluator;

    void Start()
    {
        // AnimationEvaluatorコンポーネントを取得
        evaluator = GetComponent<AnimationEvaluator>();
    }

    // 評価結果をTextMeshProに表示
    public void CountDownTextChange(string showtext)
    {
        countDownText.text = showtext;
    }

    public void ShowEvaluateSimilarity()
    {
        if (evaluator != null)
        {
            // similarityTextに現在の一致度を設定
            similarityText.text = "Similarity: " + evaluator.similarity.ToString("F2");
        }
    }
}
