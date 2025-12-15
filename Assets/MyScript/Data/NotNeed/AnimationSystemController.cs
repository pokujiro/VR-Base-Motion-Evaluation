using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// key input system

public class AnimationSystemController : MonoBehaviour
{

    //  キー入力で制御　データの保存など


    public Animator animator; // アニメーションを再生するアバター
    public Text similarityText; // 評価スコアを表示するUI

    private AnimationRecorder recorder;
    private AnimationDataSaver saver;
    private AnimationEvaluator evaluator;

    void Start()
    {
        // 各スクリプトの参照を取得
        recorder = animator.GetComponent<AnimationRecorder>();
        saver = animator.GetComponent<AnimationDataSaver>();
        evaluator = animator.GetComponent<AnimationEvaluator>();
    }
}
