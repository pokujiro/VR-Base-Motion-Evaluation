using RootMotion.FinalIK;
using UnityEngine;

public class VrikInitialPositon : MonoBehaviour
{
    public VRIK ik;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisplayInitialBodyPositions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Transform CreateDebugCube(Vector3 position, string name, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * 0.1f;
        cube.name = name;

        // 色を設定
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        return cube.transform;
    }

    void DisplayInitialBodyPositions()
    {
        if (ik == null || ik.references == null)
        {
            Debug.LogError("⛔ VRIKの参照がありません！");
            return;
        }

        Debug.Log("🛠 初期の体の位置にキューブを配置");

        // 頭・腰
        CreateDebugCube(ik.references.head.position, "Debug_Head", Color.yellow);
        Debug.Log($"🟡 Head Position: {ik.references.head.position}");

        CreateDebugCube(ik.references.pelvis.position, "Debug_Pelvis", Color.magenta);
        Debug.Log($"🟣 Pelvis Position: {ik.references.pelvis.position}");

        // 左腕
        CreateDebugCube(ik.references.leftUpperArm.position, "Debug_LeftUpperArm", Color.red);
        Debug.Log($"🔴 Left Upper Arm Position: {ik.references.leftUpperArm.position}");

        CreateDebugCube(ik.references.leftForearm.position, "Debug_LeftForearm", Color.green);
        Debug.Log($"🟢 Left Forearm Position: {ik.references.leftForearm.position}");

        CreateDebugCube(ik.references.leftHand.position, "Debug_LeftHand", Color.blue);
        Debug.Log($"🔵 Left Hand Position: {ik.references.leftHand.position}");

        // 右腕
        CreateDebugCube(ik.references.rightUpperArm.position, "Debug_RightUpperArm", Color.red);
        Debug.Log($"🔴 Right Upper Arm Position: {ik.references.rightUpperArm.position}");

        CreateDebugCube(ik.references.rightForearm.position, "Debug_RightForearm", Color.green);
        Debug.Log($"🟢 Right Forearm Position: {ik.references.rightForearm.position}");

        CreateDebugCube(ik.references.rightHand.position, "Debug_RightHand", Color.blue);
        Debug.Log($"🔵 Right Hand Position: {ik.references.rightHand.position}");

        // 足（左・右）
        CreateDebugCube(ik.references.leftFoot.position, "Debug_LeftFoot", Color.cyan);
        Debug.Log($"🔵 Left Foot Position: {ik.references.leftFoot.position}");

        CreateDebugCube(ik.references.rightFoot.position, "Debug_RightFoot", Color.cyan);
        Debug.Log($"🔵 Right Foot Position: {ik.references.rightFoot.position}");
    }

}
