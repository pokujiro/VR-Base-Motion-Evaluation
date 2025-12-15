using UnityEngine;
using UnityEngine.InputSystem;


public class MoveACube : MonoBehaviour
{

    //[SerializeField] private InputActionAsset ActionAsset;  // 大本の　InputAction

    //[SerializeField] private InputActionReference JoyStitckR;   // ある具体的な操作（ジョイコンなど）
    //[SerializeField] private InputActionReference RightControllerPosition;

    [Header("Action Button")]
    [SerializeField] private InputActionReference ExampleButton;   // ある具体的な操作（ジョイコンなど）
    //[SerializeField] private InputActionReference SecondaryButton;

    // Declare the ActionAsset and InputActionReference
    // Remember to enable the InputActionAsset before using it.
    // After the InputActionAsset is enabled, the input data, which is given by the InputActionReference from the action it is associated with, can be used in the script.
    //private void OnEnable()
    //{
    //    if (ActionAsset != null)
    //    {
    //        ActionAsset.Enable();
    //    }
    //}

    // Using InputActionReference to get the input value
    void Update()
    {
        // Now, by using ReadValue<TValue>, the value of InputActionReference can be used in the code.
        // The thumbstick of our controller returns a 2D value, which is held by a Vector2. By giving Translate() the value, we can move the GameObject this script is attached on.
        //transform.Translate(JoyStitckR.action.ReadValue<Vector2>() * Time.deltaTime);

        //// 移動　（右手のコントローラ）
        //Vector3 controllerPosition = RightControllerPosition.action.ReadValue<Vector3>();
        //transform.position = controllerPosition;

        if (ExampleButton.action.WasPressedThisFrame())
        {
            Debug.Log(" Pressed ! ");
        }
    }
}