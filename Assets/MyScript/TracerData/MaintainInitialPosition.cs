using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// スクリプト内で１８０（y軸）回転している
/// インスペクターで再生位置は決める。
/// </summary>

public class MaintainInitialPosition : MonoBehaviour
{
    public Transform tracker; // トラッカーのTransform
    private Vector3 initialTrackerPosition;
    private Quaternion initialTrackerRotation;
    private Vector3 initialObjectPosition;
    private Quaternion initialObjectRotation;
    private bool isInitialized = false;
    private int frameCounter = 0;
    private int initializationDelayFrames = 5; // 5フレーム後に初期位置を記録

    [SerializeField] private InputActionReference CaliblationButton; // Stopボタン入力

    void Start()
    {
        initialObjectPosition = transform.position;
        Debug.Log($"初期位置{initialObjectPosition}");
    }

    void Update()
    {
        if (!isInitialized)
        {
            frameCounter++;
            if (frameCounter >= initializationDelayFrames && tracker.position != Vector3.zero)
            {
                InitializePosition();
            }
            return;
        }

        if (CaliblationButton.action.WasPressedThisFrame())
        {
            TrackerInitializePosition();
            Debug.Log(" Calliblation ");
        }

        // １８０回転しても座標系の軸の向きは変わっていない？
        Vector3 targetPosition = tracker.position;
        Vector3 targetDistance = targetPosition - initialTrackerPosition;
        transform.position = new Vector3(targetDistance.x, targetDistance.y, -targetDistance.z) + initialObjectPosition + initialTrackerPosition;

        Quaternion targetRotation = tracker.rotation;

        Quaternion yRotation = Quaternion.Euler(0, 180, 0);

        transform.rotation = yRotation * new Quaternion(targetRotation.x, -targetRotation.y, -targetRotation.z, targetRotation.w);

    }

    void InitializePosition()
    {
        initialTrackerPosition = new Vector3(tracker.position.x, tracker.position.y, -tracker.position.z);
        initialTrackerRotation = tracker.rotation;
        initialObjectPosition = transform.position;
        initialObjectRotation = transform.rotation;
        isInitialized = true;
        Debug.Log($"Initialized after {frameCounter} frames.");
    }

    void TrackerInitializePosition()
    {
        initialTrackerPosition = tracker.position;
        initialTrackerRotation = tracker.rotation;
        isInitialized = true;
        Debug.Log($"Initialized after {frameCounter} frames.");
    }
}


