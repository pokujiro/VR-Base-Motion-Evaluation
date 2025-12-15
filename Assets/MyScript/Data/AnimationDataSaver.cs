using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static TrackerDataSaver;
using System;

/// <summary>
/// 記録されたアニメーションデータをJSON形式で保存するスクリプト
/// </summary>
public class AnimationDataSaver : MonoBehaviour
{
    [Header("Recorder Reference")]
    public AnimationRecorder recorder; // AnimationRecorderスクリプトを割り当て

    [Header("保存先")]
    public string DirectoryName = "Data";

    [Header("読み込み元")]
    public string ReadDirectoryName = "Data";



    /// <summary>
    /// 記録データをJSON形式で保存
    /// </summary>
    public void SaveReferenceData()
    {
        if (recorder == null)
        {
            Debug.LogError("Recorder is not assigned!"); // Recorderが設定されていない場合のエラーログ
            return;
        }

        // 記録されたデータを取得
        var data = recorder.GetReferenceRecordedData();
        if (data == null || data.Count == 0)
        {
            Debug.LogError("No data recorded to save!"); // データが空の場合のエラーログ
            return;
        }

        // JSONファイルとして保存
        string json = JsonUtility.ToJson(new ListWrapper<AnimationRecorder.BoneData> { bones = data }, true);
        try
        {
            // `Assets/Data/` ディレクトリに保存
            string directoryPath = Path.Combine(Application.dataPath, DirectoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, "referenceAnimationWithTracker.json");
            // ファイルを書き込む
            File.WriteAllText(filePath, json);
            Debug.Log($"File saved successfully to: {filePath}");
        }
        catch (UnauthorizedAccessException e)
        {
            Debug.LogError($"Access denied: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"I/O error: {e.Message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An error occurred: {e.Message}");
        }

        //// 保存の確認
        //if (File.Exists(filePath))
        //{
        //    Debug.Log($"Reference data successfully saved to: {filePath}");
        //}
        //else
        //{
        //    Debug.LogError("Failed to save the reference data.");
        //}
    }

    /// <summary>
    /// JSON形式のBoneデータをロード
    /// </summary>
    /// <returns>ロードされたネストされたトラッカーデータ</returns>
    public List<AnimationRecorder.BoneData> LoadReferenceData()
    {
        string directoryPath = Path.Combine(Application.dataPath, ReadDirectoryName);
        string filePath = Path.Combine(directoryPath, "referenceAnimationWithTracker.json");
        //Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");
        //Debug.Log($"Load　filePath: {filePath}");

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

        var loadedData = JsonUtility.FromJson<ListWrapper<AnimationRecorder.BoneData>>(json).bones;
        if (loadedData != null && loadedData.Count > 0)
        {
            Debug.Log("Reference data (Bone) loaded successfully.");
        }
        else
        {
            Debug.LogError("Loaded reference data (Bone) is empty."); // データが空の場合のエラーログ
        }

        return loadedData;
    }

    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> bones; // ボーンデータのリスト
    }
}
