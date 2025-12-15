using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションのボーンデータを記録するスクリプト（経過時間で記録管理）
/// Update（）
/// </summary>
public class AnimationRecorder : MonoBehaviour
{
    [Header(" Setting ")]
    public Animator animator; // アニメーションを再生しているAnimator
    public Animator replayAnimator;  // リプレイするアバター
    public DisplayBonesAsObjects displayBoneAsObject; // Boneの色を変更
    public BoneConnector boneConnector; // BoneConnector への参照


    [Header(" Interval ")]
    public float recordInterval = 0.1f; // 記録の間隔（秒）


    private List<BoneData> recordedData = new List<BoneData>(); // 記録されたボーンデータのリスト
    private List<BoneData> referenceRecordedData = new List<BoneData>(); // 記録されたボーンデータのリスト
    private List<FrameSimilarityData> recordedSimilarity = new List<FrameSimilarityData>(); // フレームごとの一致度を記録


    private bool isRecording = false; // 現在記録中かどうかを示すフラグ
    private float timeSinceLastRecord = 0f; // 最後に記録してからの経過時間

    [Header("初期位置")]
    private Vector3 initialWaistPosition; // 腰の初期位置
    private Vector3 initialReplayWaistPosition; // 腰の初期位置（1回目のリプレイ時）
    private bool isInitialized = false; // 初期位置が設定されたか

    [Header("閾値")]
    public float positionThreshold; // 位置誤差の閾値
    public float rotationThreshold;  // 回転誤差の閾値（度数）
    private Dictionary<HumanBodyBones, Color> boneColors = new Dictionary<HumanBodyBones, Color>();


    [Header("評価時の重み")]
    public float positionWeight; // 位置誤差の閾値
    public float rotationWeight;  // 回転誤差の閾値（度数）


    [Header("フィードバックをする")]
    [SerializeField] private bool isFeedBack = true;
    [SerializeField] private bool isFeedBackArrow = false;

    [Header("色の濃淡")]
    public float maxPositionError;  // 位置誤差の最大閾値
    public float maxRotationError;   // 回転誤差の最大閾値

    [Header("矢印")]
    [SerializeField] private GameObject arrowPrefab; // 矢印のプレハブ
    private Dictionary<HumanBodyBones, GameObject> arrows = new Dictionary<HumanBodyBones, GameObject>();　// 矢印オブジェクトを保存


    // 各ボーンごとの誤差データを記録する構造体
    [System.Serializable]
    public class BoneErrorData
    {
        public float positionError; // ポジション誤差
        public float rotationError; // 回転誤差
    }

    // フレームごとの誤差データ
    [System.Serializable]
    public class FrameSimilarityData
    {
        public int frame; // フレーム番号
        public Dictionary<HumanBodyBones, BoneErrorData> boneErrors = new Dictionary<HumanBodyBones, BoneErrorData>(); // 各ボーンごとの誤差
    }



    private List<BoneData> referenceData; // 基準データ
    private int startFrame = 0; // 記録開始時のフレーム数

    // 主要Bone に対応する　具体的な　Bone　の対応表
    private Dictionary<string, string> boneNameMapping = new Dictionary<string, string>();


    // ボーンデータの構造体
    [System.Serializable]
    public class BoneData
    {
        public string boneName; // ボーン名
        public Vector3 position; // ボーンの位置
        public Quaternion rotation; // ボーンの回転
        // public float normalizedTime; // アニメーションの正規化された時間
        public int frame; // フレーム番号（オプション）
    }

    // 主要なボーンを格納した配列
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

    private void Start()
    {
        // 基準データの記録時間をロード
        //recordingDuration = PlayerPrefs.GetFloat(RecordingDurationKey, 0f);  // PlayerPrefs.GetFloat() は保存されているデータを読み込むメソッドです
                                                                             // 第1引数 RecordingDurationKey: データを識別するためのキー
                                                                             // 第2引数 0f: データが存在しない場合のデフォルト値（この場合は0秒）


        foreach (HumanBodyBones bone in importantBones)
        {
            boneColors[bone] = Color.white; // デフォルトは白
        }

        //Debug.Log($"Loaded recording duration: {recordingDuration:F2} seconds");
    }

    public void ProcessRecording(float deltaTime)
    {
        // 記録中であり、Animatorが設定されている場合のみ記録を行う
        if (isRecording && animator != null)
        {
            // 経過時間が記録間隔を超えた場合に記録
            timeSinceLastRecord += deltaTime;

            if (timeSinceLastRecord >= recordInterval)
            {
                // Debug.Log($"Real Interval (実際のインターバル BoneRecorder) :  {timeSinceLastRecord}");
                RecordFrame(); // 各 Bone 情報の取得
                timeSinceLastRecord = 0f; // 経過時間をリセット
            }
        }
    }


    public void ProcessReplayRecording(float deltaTime)
    {
        // 記録中であり、Animatorが設定されている場合のみ記録を行う
        if (isRecording && replayAnimator != null)
        {
            // 経過時間が記録間隔を超えた場合に記録
            timeSinceLastRecord += deltaTime;

            if (timeSinceLastRecord >= recordInterval)
            {
                // Debug.Log($"Real Interval (実際のインターバル BoneRecorder (Replay)) :  {timeSinceLastRecord}");
                ReplayRecordFrame(); // 各 Bone 情報の取得
                timeSinceLastRecord = 0f; // 経過時間をリセット
            }
        }
    }

    /// <summary>
    /// 1フレーム分のボーンデータを記録し、一致度を計算する
    /// </summary>
    private void RecordFrame()
    {

        if (!isInitialized) return; // 初期位置が設定されるまで記録しない

        float totalError = 0f;
        int boneCount = 0;

        FrameSimilarityData frameData = new FrameSimilarityData
        {
            frame = Time.frameCount - startFrame
        };

        foreach (HumanBodyBones bone in importantBones)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                // 比較する対象が腰の初期位置を弾いている場合こちらを使用
                //BoneData currentBone = new BoneData
                //{
                //    boneName = boneTransform.name,
                //    position = boneTransform.position - initialWaistPosition, // 腰基準の相対位置
                //    rotation = boneTransform.rotation,
                //    frame = Time.frameCount - startFrame
                //};
                BoneData currentBone = new BoneData
                {
                    boneName = boneTransform.name,
                    position = boneTransform.position, // 腰基準の相対位置
                    rotation = boneTransform.rotation,
                    frame = Time.frameCount - startFrame
                };

                recordedData.Add(currentBone);

                float positionError = Vector3.Distance(replayAnimator.GetBoneTransform(bone).position, currentBone.position);
                //Debug.Log($"Position Error: {positionError} | RefBone: {refBone.position}, CurrentBone: {currentBone.position}");
                float rotationError = Quaternion.Angle(replayAnimator.GetBoneTransform(bone).rotation, currentBone.rotation);
                //Debug.Log($"Rotation Error: {rotationError}° | RefBone: {refBone.rotation.eulerAngles}, CurrentBone: {currentBone.rotation.eulerAngles}");

                // **誤差が閾値を超えた場合は、閾値を引いた値に調整**
                if (positionError > positionThreshold)
                {
                    positionError -= positionThreshold;
                    Debug.Log($"Position Error: {positionWeight * positionError} |realError{positionError + positionThreshold}");
                }
                else
                {
                    positionError = 0;
                }

                if (rotationError > rotationThreshold)
                {
                    rotationError -= rotationThreshold;
                    Debug.Log($"Rotation Error: {rotationWeight * rotationError} |realError{rotationError + rotationThreshold}");
                }
                else
                {
                    rotationError = 0;

                    if (isFeedBackArrow)
                    {
                        HideCorrectionArrow(bone);
                    }

                }

                // **誤差データを記録**
                frameData.boneErrors[bone] = new BoneErrorData
                {
                    positionError = positionError,
                    rotationError = rotationError,
                };
                if (isFeedBack)
                {


                    // 位置誤差と回転誤差を正規化（0.0 ～ 1.0）
                    float positionIntensity = Mathf.Clamp01(positionError / maxPositionError);
                    float rotationIntensity = Mathf.Clamp01(rotationError / maxRotationError);

                    // 初期色は白（誤差なし）
                    Color baseColor = Color.white;
                    Color targetColor = Color.white;

                    if (positionError > 0 && rotationError > 0)
                    {
                        targetColor = Color.red; // 両方の誤差がある場合
                    }
                    else if (positionError > 0)
                    {
                        targetColor = Color.blue; // 位置誤差がある場合
                    }
                    else if (rotationError > 0)
                    {
                        targetColor = Color.yellow; // 回転誤差がある場合
                    }

                    // 誤差の強度に応じて色を変化（白 → targetColor）
                    float intensity = Mathf.Max(positionIntensity, rotationIntensity);
                    boneColors[bone] = Color.Lerp(baseColor, targetColor, intensity);
                }

                // **ボーンごとに色を変更**
                displayBoneAsObject.ExchangeBoneColor(bone.ToString(), boneColors[bone]);

                // **正規化した一致度スコアを計算**
                float similarity = 1.0f - (positionWeight * positionError + (rotationError * rotationWeight)); // 回転誤差のスケール調整
                similarity = Mathf.Clamp(similarity, 0.0f, 1.0f); //Mathf.Clamp() でスコアを 0.0～1.0 の範囲に制限。

                totalError += similarity;
                boneCount++;
                //// **一致度の評価**
                //if (referenceData != null && referenceData.Count > 0)
                //{
                //    // **現在のフレームに最も近い基準データを検索**
                //    BoneData refBone = referenceData.Find(b =>
                //        b.boneName == currentBone.boneName &&
                //        Mathf.Abs(b.frame - currentBone.frame) <= 120 // 許容フレーム誤差
                //    );

                //    if (refBone != null)
                //    {
                //        float positionError = Vector3.Distance(refBone.position, currentBone.position);
                //        //Debug.Log($"Position Error: {positionError} | RefBone: {refBone.position}, CurrentBone: {currentBone.position}");
                //        float rotationError = Quaternion.Angle(refBone.rotation, currentBone.rotation);
                //        //Debug.Log($"Rotation Error: {rotationError}° | RefBone: {refBone.rotation.eulerAngles}, CurrentBone: {currentBone.rotation.eulerAngles}");

                //        // **誤差が閾値を超えた場合は、閾値を引いた値に調整**
                //        if (positionError > positionThreshold)
                //        {
                //            positionError -= positionThreshold; 
                //            Debug.Log($"Position Error: {positionWeight * positionError} |realError{positionError + positionThreshold}");
                //        }
                //        else
                //        {
                //            positionError = 0;
                //        }

                //        if (rotationError > rotationThreshold)
                //        {
                //            rotationError -= rotationThreshold;
                //            Debug.Log($"Rotation Error: {rotationWeight * rotationError} |realError{rotationError + rotationThreshold}");

                //            if (isFeedBackArrow)
                //            {
                //                // **回転の補正方向を修正**
                //                Quaternion rotationDifference = refBone.rotation * Quaternion.Inverse(currentBone.rotation);
                //                Vector3 correctionDirection = rotationDifference * Vector3.forward;

                //                // **X軸の回転誤差に対応**
                //                if (Mathf.Abs(correctionDirection.x) > Mathf.Abs(correctionDirection.z))
                //                {
                //                    correctionDirection = rotationDifference * Vector3.up;
                //                }
                //                // **矢印オブジェクトを表示**
                //                ShowCorrectionArrow(bone, boneTransform, correctionDirection);
                //            }



                //        }
                //        else
                //        {
                //            rotationError = 0;

                //            if (isFeedBackArrow)
                //            {
                //                HideCorrectionArrow(bone);
                //            }

                //        }

                //        // **誤差データを記録**
                //        frameData.boneErrors[bone] = new BoneErrorData
                //        {
                //            positionError = positionError,
                //            rotationError = rotationError,
                //        };
                //        if (isFeedBack)
                //        {


                //            // 位置誤差と回転誤差を正規化（0.0 ～ 1.0）
                //            float positionIntensity = Mathf.Clamp01(positionError / maxPositionError);
                //            float rotationIntensity = Mathf.Clamp01(rotationError / maxRotationError);

                //            // 初期色は白（誤差なし）
                //            Color baseColor = Color.white;
                //            Color targetColor = Color.white;

                //            if (positionError > 0 && rotationError > 0)
                //            {
                //                targetColor = Color.red; // 両方の誤差がある場合
                //            }
                //            else if (positionError > 0)
                //            {
                //                targetColor = Color.blue; // 位置誤差がある場合
                //            }
                //            else if (rotationError > 0)
                //            {
                //                targetColor = Color.yellow; // 回転誤差がある場合
                //            }

                //            // 誤差の強度に応じて色を変化（白 → targetColor）
                //            float intensity = Mathf.Max(positionIntensity, rotationIntensity);
                //            boneColors[bone] = Color.Lerp(baseColor, targetColor, intensity);
                //        }

                //        // **ボーンごとに色を変更**
                //        displayBoneAsObject.ExchangeBoneColor(bone.ToString(), boneColors[bone]);

                //        // **正規化した一致度スコアを計算**
                //        float similarity = 1.0f - (positionWeight*positionError + (rotationError * rotationWeight)); // 回転誤差のスケール調整
                //        similarity = Mathf.Clamp(similarity, 0.0f, 1.0f);　//Mathf.Clamp() でスコアを 0.0～1.0 の範囲に制限。

                //        totalError += similarity;
                //        boneCount++;
                //    }
                //    else
                //    {
                //        Debug.Log("基準データが見つかりません");
                //    }
                //}
            }
        }
        // **フレームごとの誤差データを記録**
        recordedSimilarity.Add(frameData);

        float finalSimilarity = totalError / boneCount;
        Debug.Log($"結果：{finalSimilarity}");


        if (isFeedBack)
        {
            if (boneConnector != null)
            {
                Dictionary<string, (float positionError, float rotationError)> errorData = new Dictionary<string, (float, float)>();

                foreach (var boneError in frameData.boneErrors)
                {
                    string boneName = boneError.Key.ToString();
                    errorData[boneName] = (boneError.Value.positionError, boneError.Value.rotationError);
                }

                boneConnector.SetBoneErrors(errorData);
                boneConnector.ConnectBones();
            }
        }
        // === BoneConnector に誤差データを渡す ===


    }

    public (float, float) GetWeight()
    {
        return (positionWeight, rotationWeight);
    }

    private void ShowCorrectionArrow(HumanBodyBones bone, Transform boneTransform, Vector3 direction)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("arrowPrefab が null のため、矢印を生成できません。");
            return;
        }

        float offsetDistance = 0.1f; // 矢印をボーンからどれだけ離すか（単位: メートル）

        if (!arrows.ContainsKey(bone))
        {
            GameObject arrow = Instantiate(arrowPrefab, boneTransform.position, Quaternion.identity);
            arrow.transform.SetParent(boneTransform, true);
            arrows[bone] = arrow;
        }

        GameObject arrowObject = arrows[bone];
        arrowObject.SetActive(true);

        // 矢印をボーンから少し離れた位置に配置
        Vector3 offsetPosition = boneTransform.position + direction.normalized * offsetDistance;
        arrowObject.transform.position = offsetPosition;
        // **X軸の回転誤差がある場合の補正**
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            arrowObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross(direction, Vector3.right));
        }
        else
        {
            arrowObject.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// 矢印を非表示にする
    /// </summary>
    private void HideCorrectionArrow(HumanBodyBones bone)
    {
        if (arrows.ContainsKey(bone))
        {
            arrows[bone].SetActive(false);
        }
    }


    /// <summary>
    /// 1フレーム分のボーンデータを記録し、一致度を計算する (Replayのみ)
    /// 
    /// </summary>
    private void ReplayRecordFrame()
    {

        if (!isInitialized) return; // 初期位置が設定されるまで記録しない

        foreach (HumanBodyBones bone in importantBones)
        {
            Transform boneTransform = replayAnimator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                BoneData currentBone = new BoneData
                {
                    boneName = boneTransform.name,
                    position = boneTransform.position - initialReplayWaistPosition, // 腰基準の相対位置
                    rotation = boneTransform.rotation,
                    frame = Time.frameCount - startFrame
                };

                referenceRecordedData.Add(currentBone);
            }
        }
    }

    /// <summary>
    /// 記録を開始する
    /// </summary>
    public void StartRecording()
    {
        recordedData.Clear(); // 以前の記録をクリア
        recordedSimilarity.Clear(); // 以前の記録をクリア

        isRecording = true; // 記録フラグをオン
        timeSinceLastRecord = 0f; // 経過時間をリセット
        startFrame = Time.frameCount; // 現在のフレームを記録開始フレームとして設定
        Transform waistTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
        initialWaistPosition = waistTransform.position; // 初期腰位置を保存
        isInitialized = true;
        Debug.Log("Recording started.");
    }

    public void StartReplayRecording()
    {
        referenceRecordedData.Clear(); // 以前の記録をクリア
        recordedSimilarity.Clear(); // 以前の記録をクリア

        isRecording = true; // 記録フラグをオン
        timeSinceLastRecord = 0f; // 経過時間をリセット
        startFrame = Time.frameCount; // 現在のフレームを記録開始フレームとして設定
        Transform waistTransform = replayAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initialReplayWaistPosition = waistTransform.position; // 初期腰位置を保存
        isInitialized = true;
        Debug.Log("Recording started.");
    }



    /// <summary>
    /// 記録を停止する
    /// </summary>
    public void StopRecording()
    {
        isRecording = false; // 記録フラグをオフ
    }

    /// <summary>
    /// 基準データを読み込む
    /// </summary>
    /// <param name="reference"></param>
    public void SetReferenceData(List<BoneData> reference)
    {
        // 基準データを読み込む
        referenceData = reference;
    }

    /// <summary>
    /// 記録された一致度データを取得する
    /// </summary>
    public List<FrameSimilarityData> GetSimilarityData()
    {
        return recordedSimilarity;
    }

    /// <summary>
    /// 記録されたデータを取得する
    /// </summary>
    /// <returns>記録されたボーンデータのリスト</returns>
    public List<BoneData> GetReferenceRecordedData()
    {
        return referenceRecordedData; // 記録されたデータを返す
    }

    public List<BoneData> GetRecordedData()
    {
        return recordedData;
    }

    /// <summary>
    /// 外部でマッピングが使えるようにする
    /// </summary>
    /// <returns>主要ボーンに対しての具体的なボーンのマッピング</returns>
    public Dictionary<string, string> GetBoneNameMapping()
    {
        return boneNameMapping;
    }
}
