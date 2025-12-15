using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationUI : MonoBehaviour
{
    public Text evaluationText; // 評価結果を表示するUIテキスト
    public MotionEvaluator evaluator;

    void Update()
    {
        // 評価結果を取得（仮にMotionEvaluatorから取得する場合）
        string result = $"Left Hand Error: {evaluator.leftPositionError}, {evaluator.leftRotationError}\n" +
                        $"Right Hand Error: {evaluator.rightPositionError}, {evaluator.rightRotationError}";

        // テキストを更新
        evaluationText.text = result;
    }
}