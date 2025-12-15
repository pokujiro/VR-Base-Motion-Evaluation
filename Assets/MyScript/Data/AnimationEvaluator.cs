using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using static AnimationRecorder;
using System.Collections;
using System.Linq;
using TMPro;

public class AnimationEvaluator : MonoBehaviour
{
    [Header(" Settings ")]
    public Animator animator; // リアルタイムアニメーションのAnimator
    public AnimationRecorder recorder; // AnimationRecorderを利用してマッピングを取得
    public AnimationDataSaver animationDataSaver;
    public AnimationRecorder animationRecorder;

    [Header(" reference data bone List ")]
    public List<AnimationRecorder.BoneData> referenceData; // 基準データ

    private List<FrameSimilarityData> similarityData; // 一致度データ

    [Header(" result ")]
    public float similarity; // 一致度

    private Dictionary<string, string> boneNameMapping; // ボーン名のマッピング
    private Dictionary<HumanBodyBones, string> japanesebBoneNameMapping = new Dictionary<HumanBodyBones, string>()
{
    { HumanBodyBones.Hips, "腰" },
    { HumanBodyBones.Spine, "背骨" },
    { HumanBodyBones.Chest, "胸" },
    { HumanBodyBones.Neck, "首" },
    { HumanBodyBones.Head, "頭" },
    { HumanBodyBones.LeftUpperArm, "左上腕" },
    { HumanBodyBones.RightUpperArm, "右上腕" },
    { HumanBodyBones.LeftLowerArm, "左前腕" },
    { HumanBodyBones.RightLowerArm, "右前腕" },
    { HumanBodyBones.LeftHand, "左手" },
    { HumanBodyBones.RightHand, "右手" },
    { HumanBodyBones.LeftUpperLeg, "左太腿" },
    { HumanBodyBones.RightUpperLeg, "右太腿" },
    { HumanBodyBones.LeftLowerLeg, "左膝下" },
    { HumanBodyBones.RightLowerLeg, "右膝下" },
    { HumanBodyBones.LeftFoot, "左足" },
    { HumanBodyBones.RightFoot, "右足" }
};

    // 主要なボーンリスト（AnimationRecorderと一致させる）
    private readonly HumanBodyBones[] importantBones = new HumanBodyBones[]
    {
        HumanBodyBones.Hips,
        HumanBodyBones.Spine,
        HumanBodyBones.Chest,
        HumanBodyBones.Neck,
        HumanBodyBones.Head,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.LeftHand,
        HumanBodyBones.RightHand,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.RightFoot
    };

    void Start()
    {
        // コンポーネントの確認
        if (!ValidateComponents()) return;
        boneNameMapping = recorder.GetBoneNameMapping();
        Debug.Log("Bone name mapping loaded.");
    }

    /// <summary>
    /// 各フレームごとのボーンの一致度を評価（各ボーンごとの誤差を出力）
    /// </summary>
    public void EvaluateSimilarity(List<FrameSimilarityData> frameSimilarity)
    {
        similarityData = frameSimilarity;

        (float positionWeight, float rotationWeight) = animationRecorder.GetWeight();

        // 計算開始時間を記録
        float startTimeEmu = Time.realtimeSinceStartup;

        // 集計用のリスト（全フレーム・全ボーンの誤差データ）
        List<float> positionErrors = new List<float>();
        List<float> rotationErrors = new List<float>();

        // 各ボーンごとのエラーデータを格納する辞書
        Dictionary<HumanBodyBones, List<BoneErrorData>> boneErrorRecords = new Dictionary<HumanBodyBones, List<BoneErrorData>>();

        foreach (HumanBodyBones bone in importantBones)
        {
            boneErrorRecords[bone] = new List<BoneErrorData>();
        }

        // 各フレームのデータを処理
        foreach (var frameData in similarityData)
        {
            //Debug.Log($"[Frame {frameData.frame}]");

            foreach (var boneError in frameData.boneErrors)
            {
                positionErrors.Add(boneError.Value.positionError);
                rotationErrors.Add(boneError.Value.rotationError);

                // ボーンごとのエラーを記録
                boneErrorRecords[boneError.Key].Add(boneError.Value);

                // 各ボーンの誤差をログ出力
                // Debug.Log($"{boneError.Key}: PosErr = {boneError.Value.positionError:F4}, RotErr = {boneError.Value.rotationError:F4}");
            }
        }

        // **統計計算（ポジション誤差）**
        float avgPositionError = positionErrors.Average();
        float maxPositionError = positionErrors.Max();
        float minPositionError = positionErrors.Min();
        float stdDevPosition = Mathf.Sqrt(positionErrors.Average(p => Mathf.Pow(p - avgPositionError, 2)));

        // **統計計算（回転誤差）**
        float avgRotationError = rotationErrors.Average();
        float maxRotationError = rotationErrors.Max();
        float minRotationError = rotationErrors.Min();
        float stdDevRotation = Mathf.Sqrt(rotationErrors.Average(r => Mathf.Pow(r - avgRotationError, 2)));

        // **ボーンごとの誤差統計**
        Debug.Log($"=== Per-Bone Error Statistics ===");
        foreach (var bone in importantBones)
        {
            if (boneErrorRecords[bone].Count > 0)
            {
                float boneAvgPosErr = boneErrorRecords[bone].Average(e => e.positionError);
                float boneAvgRotErr = boneErrorRecords[bone].Average(e => e.rotationError);
                // Debug.Log($"{bone}: AvgPosErr = {boneAvgPosErr:F4}, AvgRotErr = {boneAvgRotErr:F4}");
            }
        }
        // **UI にボーン誤差を表示**
        UpdateBoneErrorUI(boneErrorRecords);

        // **最終的なスコア (Similarity) の計算**
        similarity = 1.0f - (avgPositionError * positionWeight + avgRotationError * rotationWeight);
        similarity = Mathf.Clamp(similarity, 0.0f, 1.0f);

        Debug.Log($"=== Final Similarity Report ===\n" +
                  $" - Position Error: Avg = {avgPositionError:F4}, Max = {maxPositionError:F4}, Min = {minPositionError:F4}, StdDev = {stdDevPosition:F4}\n" +
                  $" - Rotation Error: Avg = {avgRotationError:F4}, Max = {maxRotationError:F4}, Min = {minRotationError:F4}, StdDev = {stdDevRotation:F4}\n" +
                  $" - Final Similarity Score: {similarity:F4}");

        // UI に表示
        UpdateSimilarityUI(avgPositionError, maxPositionError, minPositionError, stdDevPosition,
                           avgRotationError, maxRotationError, minRotationError, stdDevRotation, similarity);


        // 計算時間を記録
        float endTimeEmu = Time.realtimeSinceStartup;
        float elapsedTimeEmu = (endTimeEmu - startTimeEmu) * 1000; // ミリ秒単位
        Debug.Log($"Time to calculate similarity: {elapsedTimeEmu:F2} ms");
    }

    public TextMeshProUGUI boneErrorText; // ボーンの誤差データ表示用 UI
    public TextMeshProUGUI similarityText; // UI テキスト


    private void UpdateBoneErrorUI(Dictionary<HumanBodyBones, List<BoneErrorData>> boneErrorRecords)
    {
        if (boneErrorText == null) return;

        float positionErrorThreshold = 0.15f; // 位置誤差の閾値
        float rotationErrorThreshold = 45f;   // 回転誤差の閾値

        string uiText = "<size=18><b>=== ボーン誤差統計 ===</b></size>\n";

        foreach (var bone in boneErrorRecords.Keys)
        {
            if (boneErrorRecords[bone].Count > 0)
            {
                float boneAvgPosErr = boneErrorRecords[bone].Average(e => e.positionError);
                float boneAvgRotErr = boneErrorRecords[bone].Average(e => e.rotationError);

                // **ボーンの日本語名を取得**
                string boneName = japanesebBoneNameMapping.ContainsKey(bone) ? japanesebBoneNameMapping[bone] : bone.ToString();

                // **誤差の判定**
                bool isPositionErrorHigh = boneAvgPosErr > positionErrorThreshold;
                bool isRotationErrorHigh = boneAvgRotErr > rotationErrorThreshold;

                // **ボーン名の色判定（どちらかの誤差が大きければ赤くする）**
                string boneColorTag = (isPositionErrorHigh || isRotationErrorHigh) ? "<color=red>" : "<color=#90EE90>";

                // **誤差の値ごとの色判定**
                string posErrColorTag = isPositionErrorHigh ? "<color=red>" : "<color=#90EE90>"; //ライトぐr－ン
                string rotErrColorTag = isRotationErrorHigh ? "<color=red>" : "<color=#90EE90>";

                // **UIテキストに反映**
                uiText += $"{boneColorTag}{boneName}</color>: " +
                          $"位置誤差 = {posErrColorTag}{boneAvgPosErr:F3}</color>, " +
                          $"回転誤差 = {rotErrColorTag}{boneAvgRotErr:F3}</color>\n";
            }
        }

        boneErrorText.text = uiText;
        Debug.Log($"{uiText}");
    }



    private void UpdateSimilarityUI(float avgPositionError, float maxPositionError, float minPositionError, float stdDevPosition,
                                    float avgRotationError, float maxRotationError, float minRotationError, float stdDevRotation,
                                    float similarity)
    {
        if (similarityText != null)
        {
            similarityText.text = $"=== Final Similarity Report ===\n" +
                                  $" - P_Error:\n   Avg = {avgPositionError:F3}, Max = {maxPositionError:F3}, Min = {minPositionError:F3}\n" +
                                  $" - R_Error:\n   Avg = {avgRotationError:F3}, Max = {maxRotationError:F3}, Min = {minRotationError:F3}\n" +
                                  $" - Final Similarity Score: {similarity:F3}";
        }
    }

    private void FinalizeSimilarityReport(float avgPositionError, float maxPositionError, float minPositionError, float stdDevPosition,
                                          float avgRotationError, float maxRotationError, float minRotationError, float stdDevRotation,
                                          float similarity)
    {
        Debug.Log($"=== Final Similarity Report ===\n" +
                  $" - Position Error: Avg = {avgPositionError:F4}, Max = {maxPositionError:F4}, Min = {minPositionError:F4}, StdDev = {stdDevPosition:F4}\n" +
                  $" - Rotation Error: Avg = {avgRotationError:F4}, Max = {maxRotationError:F4}, Min = {minRotationError:F4}, StdDev = {stdDevRotation:F4}\n" +
                  $" - Final Similarity Score: {similarity:F4}");

        // UI に表示
        UpdateSimilarityUI(avgPositionError, maxPositionError, minPositionError, stdDevPosition,
                           avgRotationError, maxRotationError, minRotationError, stdDevRotation, similarity);
    }



    /// <summary>
    /// 一致度を評価
    /// </summary>
    public void OldEvaluateSimilarity()
    {
        // 処理開始時刻を記録
        float startTime = Time.realtimeSinceStartup;

        // JSONファイルから基準データを読み込む
        referenceData = animationDataSaver.LoadReferenceData();


        // 処理終了時刻を記録
        float endTime = Time.realtimeSinceStartup;

        // 経過時間を計算してログに出力
        float elapsedTime = (endTime - startTime) * 1000; // ミリ秒単位
        Debug.Log($"Time to load and parse reference data: {elapsedTime:F2} ms");

        // 一致度の計算処理を開始
        float startTimeEmu = Time.realtimeSinceStartup;
        if (referenceData == null || referenceData.Count == 0)
        {
            Debug.LogError("Reference data is not set or empty."); // 基準データが設定されていない場合
            return;
        }

        float totalError = 0f;

        // 閾値の設定
        float positionThreshold = 0.05f;  // 5cm
        float rotationThreshold = 10f;    // 10度
        int validComparisons = 0;  // 有効な比較数をカウント

        // 現在のアニメーションデータを取得
        var currentData = recorder.GetRecordedData();
        
        if (currentData == null || currentData.Count == 0)
        {
            Debug.LogError("No recorded data found for evaluation.");
            return;
        }

        if (referenceData == null || referenceData.Count == 0)
        {
            Debug.LogError("Reference data is not set or empty.");
            return;
        }

        for (int i = 0; i < referenceData.Count; i++)
        {
            var refBone = referenceData[i];

            // 現在のデータから同じフレームのデータを検索
            var currentBone = currentData.Find(b =>
                b.boneName == refBone.boneName &&
                Mathf.Abs(b.frame - refBone.frame) <= 120 // 許容フレーム誤差
            );

            // 比較データが不足している場合にスキップ
            if (currentBone == null)
            {
                Debug.LogWarning($"Skipped bone: {refBone.boneName}. No matching data found within the allowed range.");
                continue;
            }

            // 位置誤差を計算
            float positionError = Vector3.Distance(refBone.position, currentBone.position);
            Debug.Log($"Position 現在{refBone.position} 基準{currentBone.position}");
            if (positionError < positionThreshold)
            {
                Debug.Log($"閾値誤差 Position　{positionError}");
                positionError = 0f; // 閾値未満の誤差を無視

            }

            // 回転誤差を計算
            float rotationError = Quaternion.Angle(refBone.rotation, currentBone.rotation);
            if (rotationError < rotationThreshold)
            {
                rotationError = 0f; // 閾値未満の誤差を無視
                Debug.Log($"閾値誤差 Rotation　{rotationError}");
            }

            // エラーを累積（回転の影響を調整）
            totalError += positionError + (rotationError * 0.1f);

            // 有効な比較回数をカウント
            validComparisons++;

            Debug.Log($"Bone: {refBone.boneName}, PositionError: {positionError:F4}, RotationError: {rotationError:F2}");
        }

        // 全体の一致度を計算
        if (validComparisons > 0)
        {
            similarity = 1.0f - (totalError / validComparisons); // 有効な比較数で割る
        }
        else
        {
            similarity = 0f; // 比較データがなければ0
        }

        // 処理終了時刻を記録
        float endTimeEmu = Time.realtimeSinceStartup;

        // 経過時間を計算してログに出力
        float elapsedTimeEmu = (endTimeEmu - startTimeEmu) * 1000; // ミリ秒単位
        Debug.Log($"Time to calculate similarity: {elapsedTimeEmu:F2} ms");
    }
    private bool ValidateComponents()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return false;
        }

        if (recorder == null)
        {
            Debug.LogError("Recorder is not assigned!");
            return false;
        }
        if (animationDataSaver  == null)
        {
            Debug.LogError("animationDataSavar is not assigned!");
            return false;
        }
        return true;
    }

    [Serializable]
    public class ListWrapper<T>
    {
        public List<T> bones;
    }
}


