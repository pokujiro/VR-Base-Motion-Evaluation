using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// トラッカーデータをJSON形式で保存・ロードするスクリプト
/// </summary>
public class TrackerDataSaver : MonoBehaviour
{
    [Header("Recorder Reference")]
    public TrackerDataRecorder recorder; // TrackerDataRecorderスクリプトを割り当て
    public SimpleVRIKCalibrationController simpleVRIKCalibrationController;

    [Header("保存先")]
    public string DirectoryName = "Data";

    [Header("読み込み元")]
    public string ReadDirectoryName = "Data";


    private Vector3 initialWaistPosition; // 腰の初期位置


    [System.Serializable]
    public class CalibrationData
    {
        public float scale;           // 身長スケール
        public float headScaleY;          // 身長（Y軸）
        public float pelvisScaleY;    // 腰の高さ
        public float scaleXZ;         // 横方向スケール
        public float leftArmScale;    // 左腕の長さスケール
        public float rightArmScale;   // 右腕の長さスケール
        public float leftLegScale;    // 左足の長さスケール
        public float rightLegScale;   // 右足の長さスケール
        public float calibrationHeadToWaistY; // 頭 - 腰の高さ差
        public float calibrationArmYBase; // 腰から腕の基準高さ
    }

    /// <summary>
    /// フラット化されたトラッカーデータ
    /// </summary>
    [System.Serializable]
    public class FlatTrackerData
    {
        public int frameIndex; // フレームインデックス
        public TrackerDataRecorder.TrackerData trackerData; // トラッカーデータ
    }

    /// <summary>
    /// **キャリブレーションデータを保存**
    /// </summary>
    public void SaveCalibrationData(SimpleVRIKCalibrationController calibrationController)
    {
        if (calibrationController == null)
        {
            Debug.LogError("⛔ CalibrationController が見つかりません！");
            return;
        }

        // **キャリブレーションデータをリストで取得**
        CalibrationData calibrationData = calibrationController.GetCalibrationData();

        // **データを変数に代入**
        CalibrationData data = new CalibrationData
        {
            scale = calibrationData.scale,
            headScaleY = calibrationData.headScaleY,
            pelvisScaleY = calibrationData.pelvisScaleY,
            scaleXZ = calibrationData.scaleXZ,
            leftArmScale = calibrationData.leftArmScale,
            rightArmScale = calibrationData.rightArmScale,
            leftLegScale = calibrationData.leftLegScale,
            rightLegScale = calibrationData.rightLegScale,
            calibrationHeadToWaistY = calibrationData.calibrationHeadToWaistY,
            calibrationArmYBase = calibrationData.calibrationArmYBase,
        };


        string json = JsonUtility.ToJson(data, true);

        // `Assets/Data/` ディレクトリに保存
        string directoryPath = Path.Combine(Application.dataPath, DirectoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "calibrationData.json");
        Debug.Log($"Saving file to: {filePath}");

        // string filePath = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"✅ キャリブレーションデータを保存: {filePath} \n data:{data}");
    }

    /// <summary>
    /// **キャリブレーションデータをロード**
    /// </summary>
    public CalibrationData LoadCalibrationData()
    {
        string directoryPath = Path.Combine(Application.dataPath, ReadDirectoryName);
        string filePath = Path.Combine(directoryPath, "calibrationData.json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("⚠ キャリブレーションデータが見つかりません。");
            return null;
        }

        string json = File.ReadAllText(filePath);
        CalibrationData data = JsonUtility.FromJson<CalibrationData>(json);

        Debug.Log("✅ キャリブレーションデータをロードしました:");
        Debug.Log($"  身長スケール: {data.scale}");
        Debug.Log($"  身長Y: {data.headScaleY}");
        Debug.Log($"  腰Y: {data.pelvisScaleY}");
        Debug.Log($"  横方向スケール: {data.scaleXZ}");
        Debug.Log($"  左腕スケール: {data.leftArmScale}");
        Debug.Log($"  右腕スケール: {data.rightArmScale}");
        Debug.Log($"  左足スケール: {data.leftLegScale}");
        Debug.Log($"  右足スケール: {data.rightLegScale}");
        Debug.Log($"　腕の基準位置: ArmYBase={data.calibrationArmYBase}, HeadToWaist={data.calibrationHeadToWaistY}");

        return data;
    }

    /// <summary>
    /// 記録データをJSON形式で保存
    /// </summary>
    public void SaveTrackerData()
    {
        if (recorder == null)
        {
            Debug.LogError("Recorder is not assigned!"); // Recorderが設定されていない場合のエラーログ
            return;
        }

        // 記録されたデータを取得
        var data = recorder.GetRecordedData();
        if (data == null || data.Count == 0)
        {
            Debug.LogError("No data recorded to save!"); // データが空の場合のエラーログ
            return;
        }
        // フラット化
        var flatData = new List<FlatTrackerData>();
        for (int i = 0; i < data.Count; i++)
        {
            foreach (var tracker in data[i])
            {
                flatData.Add(new FlatTrackerData { frameIndex = i, trackerData = tracker });
            }
        }

        // dataの中身の確認
        //Debug.Log($"Data contains {data.Count} frames.");
        //for (int i = 0; i < data.Count; i++)
        //{
        //    Debug.Log($"Frame {i + 1}: {data[i].Count} trackers");
        //    foreach (var tracker in data[i])
        //    {
        //        Debug.Log($"Tracker: {tracker.name}, Position: {tracker.position}, Rotation: {tracker.rotation}");
        //    }
        //}

        // JSON形式で保存
        string json = JsonUtility.ToJson(new ListWrapper<FlatTrackerData> { items = flatData }, true);  // JsonUnityではネストされているとうまくいかない。
                                                                                                        // string filePath = Path.Combine(Application.persistentDataPath, fileName);

        string directoryPath = Path.Combine(Application.dataPath, DirectoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "trackerData.json");

        Debug.Log($"Saving file to: {filePath}");

        File.WriteAllText(filePath, json);

        Debug.Log($"Tracker data saved to: {filePath}");
        // Debug.Log($"Saving JSON: {json}");

        // 保存確認
        if (File.Exists(filePath))
        {
            Debug.Log($"Tracker data successfully saved to: {filePath}");
        }
        else
        {
            Debug.LogError("Failed to save the tracker data.");
        }
    }

    /// <summary>
    /// JSON形式のトラッカーデータをロード
    /// </summary>
    /// <returns>ロードされたネストされたトラッカーデータ</returns>
    public List<List<TrackerDataRecorder.TrackerData>> LoadTrackerData()
    {
        string directoryPath = Path.Combine(Application.dataPath, ReadDirectoryName);
        string filePath = Path.Combine(directoryPath, "trackerData.json");
        Debug.Log($"ファイルあった（トラッカーサーバー）");

        // ファイルの存在を確認
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return null;
        }

        // ファイルを読み込み
        string json = File.ReadAllText(filePath);
        Debug.Log($"Loaded JSON: {json}");

        // JSONが空でないか確認
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON file is empty.");
            return null;
        }

        // デシリアライズ
        var flatData = JsonUtility.FromJson<ListWrapper<FlatTrackerData>>(json);
        if (flatData == null || flatData.items == null)
        {
            Debug.LogError("Failed to deserialize JSON or items are null.");
            return null;
        }

        // フラットデータをネスト構造に変換
        var nestedData = new List<List<TrackerDataRecorder.TrackerData>>();
        int currentFrameIndex = -1;
        List<TrackerDataRecorder.TrackerData> currentFrameData = null;

        foreach (var flatItem in flatData.items)
        {
            if (flatItem.frameIndex != currentFrameIndex)
            {
                // 新しいフレームを開始
                currentFrameIndex = flatItem.frameIndex;
                currentFrameData = new List<TrackerDataRecorder.TrackerData>();
                nestedData.Add(currentFrameData);
            }

            // フレームデータにトラッカーデータを追加
            currentFrameData.Add(flatItem.trackerData);
        }

        Debug.Log($"Successfully nested {nestedData.Count} frames.");
        return nestedData;
    }

    /// <summary>
    /// 初期位置をJSONファイルに保存
    /// </summary>
    public void SaveInitialWaistPosition()
    {
        initialWaistPosition = recorder.GetInitialPositions();
        string json = JsonUtility.ToJson(initialWaistPosition);

        string directoryPath = Path.Combine(Application.dataPath, DirectoryName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "waist_position.json");

        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// JSONファイルから初期位置を読み込む
    /// </summary>
    public Vector3 LoadInitialWaistPosition()
    {
        string directoryPath = Path.Combine(Application.dataPath, ReadDirectoryName);
        string filePath = Path.Combine(directoryPath, "waist_position.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            initialWaistPosition = JsonUtility.FromJson<Vector3>(json);
            Debug.Log($"Initial waist position loaded: {initialWaistPosition}");
        }
        else
        {
            Debug.Log("No saved waist position found. Will initialize on first recording.");
        }
        return initialWaistPosition;
    }

    /// <summary>
    /// 汎用リストラッパークラス（JSON形式でネストされたリストを保存するため）
    /// </summary>
    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> items;
    }

}

