using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class TrackerDebugVisualizer : MonoBehaviour
{
    public float sphereRadius = 0.05f;
    private GameObject[] debugSpheres;
    private GUIStyle debugLabelStyle;
    private bool isInitialized = false;

    // トラッカーの特定に使用するIDを追加
    private InputDevice[] trackerDevices;
    private string[] trackerNames = {
        "HTCViveTrackerOpenXR",
        "HTCViveTrackerOpenXR1",
        "HTCViveTrackerOpenXR8"

    };

    void Start()
    {
        InitializeDebugObjects();
        InitializeTrackers();
    }

    private void InitializeTrackers()
    {
        trackerDevices = new InputDevice[trackerNames.Length];
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        Debug.Log($"=== 検出されたデバイス数: {devices.Count} ===");

        foreach (var device in devices)
        {
            Debug.Log($"デバイス名: {device.name}");
            Debug.Log($"特性: {device.characteristics}");
            Debug.Log("----------------------------");
        }
    }

    private void OnDeviceConnected(InputDevice device)
    {
        Debug.Log($"Device connected: {device.name}");
        for (int i = 0; i < trackerNames.Length; i++)
        {
            if (device.name == trackerNames[i])
            {
                trackerDevices[i] = device;
            }
        }
    }

    private void OnDeviceDisconnected(InputDevice device)
    {
        Debug.Log($"Device disconnected: {device.name}");
        for (int i = 0; i < trackerDevices.Length; i++)
        {
            if (trackerDevices[i].name == device.name)
            {
                trackerDevices[i] = default(InputDevice);
            }
        }
    }

    private void InitializeDebugObjects()
    {
        if (isInitialized) return;

        debugLabelStyle = new GUIStyle();
        debugLabelStyle.normal.textColor = Color.white;
        debugLabelStyle.fontSize = 14;
        debugLabelStyle.fontStyle = FontStyle.Bold;

        debugSpheres = new GameObject[trackerNames.Length];
        for (int i = 0; i < trackerNames.Length; i++)
        {
            debugSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSpheres[i].transform.localScale = Vector3.one * sphereRadius;

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.HSVToRGB(i * 0.1f, 1, 1);
            debugSpheres[i].GetComponent<Renderer>().material = mat;
            debugSpheres[i].name = "DebugSphere_" + trackerNames[i];
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
        UpdateTrackerPositions();
    }

    private void UpdateTrackerPositions()
    {
        for (int i = 0; i < trackerDevices.Length; i++)
        {
            if (trackerDevices[i].isValid)
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                bool hasPos = trackerDevices[i].TryGetFeatureValue(CommonUsages.devicePosition, out position);
                bool hasRot = trackerDevices[i].TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);

                if (hasPos && debugSpheres[i] != null)
                {
                    debugSpheres[i].transform.position = position;
                    Debug.Log($"Tracker {trackerNames[i]} position: {position}");
                }
                if (hasRot && debugSpheres[i] != null)
                {
                    debugSpheres[i].transform.rotation = rotation;
                }
            }
        }
    }

    void OnGUI()
    {
        if (!isInitialized || !Application.isPlaying) return;

        try
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 400));
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Tracker Debug Information", debugLabelStyle);

                for (int i = 0; i < trackerDevices.Length; i++)
                {
                    if (debugSpheres[i] != null)
                    {
                        string status = trackerDevices[i].isValid ? "Connected" : "Disconnected";
                        GUILayout.Label($"{trackerNames[i]}: {status}", debugLabelStyle);
                        if (trackerDevices[i].isValid)
                        {
                            GUILayout.Label($"Position: {debugSpheres[i].transform.position.ToString("F3")}", debugLabelStyle);
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnGUI: {e.Message}");
        }
    }

    void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;

        for (int i = 0; i < debugSpheres?.Length; i++)
        {
            if (debugSpheres[i] != null)
            {
                Destroy(debugSpheres[i]);
            }
        }
    }
}