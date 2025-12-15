using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayAsObject : MonoBehaviour
{
    public Animator animator; // アバターのAnimatorコンポーネント
    public GameObject bonePrefab; // ボーンを表示するためのPrefab（例: 小さな球）
    public Material boneMaterial; // ボーンのマテリアル（変更用）
    public Material lineMaterial; // 線を描画するためのマテリアル

    // private List<GameObject> boneObjects = new List<GameObject>(); // 生成したボーンオブジェクト
    public Dictionary<string, GameObject> boneObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, LineRenderer> lineRenderers = new Dictionary<string, LineRenderer>();

    void Start()
    {
        if (animator == null || bonePrefab == null || lineMaterial == null || boneMaterial == null)
        {
            Debug.LogError("Animator, Bone Prefab, Line Material, or Bone Material is not assigned!");
            return;
        }

        Debug.Log("Starting bone visualization...");

        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) continue;

            Transform boneTransform = animator.GetBoneTransform(bone);

            if (boneTransform != null)
            {
                // ボーンの位置にPrefabを生成
                GameObject boneObject = Instantiate(bonePrefab, boneTransform.position, Quaternion.identity, boneTransform);
                // Debug.Log($" オブジェクトの位置　{boneTransform.position}");
                boneObject.transform.SetParent(null, true); // 第二引数 'true' でワールド座標を維持

                boneObject.name = bone.ToString();
                boneObjects[boneObject.name] = boneObject;

                // ボーンのマテリアルを設定
                MeshRenderer renderer = boneObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(boneMaterial); // インスタンス化して個別変更可能に
                }

                // 特定のボーン（例: Hips）を無視
                if (bone == HumanBodyBones.Hips) continue;

                // 親が存在する場合に線を描画
                if (boneTransform.parent != null)
                {
                    // LineRendererを追加して線をつなぐ
                    GameObject lineObject = new GameObject($"{bone}_Line");
                    LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                    lineRenderer.material = new Material(lineMaterial); // 個別のマテリアルを設定
                    lineRenderer.startWidth = 0.02f;
                    lineRenderer.endWidth = 0.02f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, boneTransform.position);
                    lineRenderer.SetPosition(1, boneTransform.parent.position);
                    lineRenderers[boneObject.name] = lineRenderer;

                    // 更新用スクリプトを追加して線を追従
                    ReplayBoneLineUpdater updater = lineObject.AddComponent<ReplayBoneLineUpdater>();
                    updater.startTransform = boneTransform;
                    updater.endTransform = boneTransform.parent;
                }
            }
            else
            {
                Debug.LogWarning($"Bone not found: {bone}");
            }
        }

        Debug.Log("Bone visualizer with lines initialized.");
    }

    /// <summary>
    /// **特定のボーンの色を変更**
    /// </summary>
    public void ExchangeBoneColor(string boneName, Color newColor)
    {
        if (boneObjects.ContainsKey(boneName))
        {
            MeshRenderer renderer = boneObjects[boneName].GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }

        if (lineRenderers.ContainsKey(boneName))
        {
            LineRenderer lineRenderer = lineRenderers[boneName];
            lineRenderer.startColor = newColor;
            lineRenderer.endColor = newColor;
            if (lineRenderer.material != null)
            {
                lineRenderer.material.color = newColor;
            }
        }
    }

    void Update()
    {
        foreach (var bone in boneObjects)
        {
            if (animator != null)
            {
                Transform boneTransform = animator.GetBoneTransform((HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), bone.Key));
                if (boneTransform != null)
                {
                    bone.Value.transform.position = boneTransform.position;
                }
            }
        }
    }

    /// <summary>
    /// ボーンとラインを非表示にする
    /// </summary>
    public void HideBonesAndLines()
    {
        foreach (var bone in boneObjects.Values)
        {
            if (bone != null)
            {
                bone.SetActive(false);
            }
        }

        foreach (var line in lineRenderers.Values)
        {
            if (line != null && line.gameObject != null)
            {
                line.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ボーンとラインを表示する
    /// </summary>
    public void ShowBonesAndLines()
    {
        foreach (var bone in boneObjects.Values)
        {
            if (bone != null)
            {
                bone.SetActive(true);
            }
        }

        foreach (var line in lineRenderers.Values)
        {
            if (line != null && line.gameObject != null)
            {
                line.gameObject.SetActive(true);
            }
        }
    }


}



// LineRendererをボーンに追従させるスクリプト
public class ReplayBoneLineUpdater : MonoBehaviour
{
    public DisplayBonesAsObjects displayBonesAsObjects;
    public Transform startTransform; // 線の始点
    public Transform endTransform;   // 線の終点
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lineRenderer != null && startTransform != null && endTransform != null)
        {
            lineRenderer.SetPosition(0, startTransform.position);
            lineRenderer.SetPosition(1, endTransform.position);
        }

        
    }
}
