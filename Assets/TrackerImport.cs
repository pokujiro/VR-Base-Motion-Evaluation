using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    public class TrackerImport : MonoBehaviour
    {
        [SerializeField]
        private string m_TrackerName = "";
        public string TrackerName { get { return m_TrackerName; } set { m_TrackerName = value; } }

        [SerializeField]
        private InputActionReference m_IsTracked = null;
        public InputActionReference IsTracked { get { return m_IsTracked; } set { m_IsTracked = value; } }

        [SerializeField]
        private InputActionReference m_TrackingState = null;
        public InputActionReference TrackingState { get { return m_TrackingState; } set { m_TrackingState = value; } }

        [SerializeField]
        private InputActionReference m_Position = null;
        public InputActionReference Position { get { return m_Position; } set { m_Position = value; } }

        [SerializeField]
        private InputActionReference m_Rotation = null;
        public InputActionReference Rotation { get { return m_Rotation; } set { m_Rotation = value; } }

        [SerializeField]
        private InputActionReference m_Menu = null;
        public InputActionReference Menu { get { return m_Menu; } set { m_Menu = value; } }

        [SerializeField]
        private InputActionReference m_GripPress = null;
        public InputActionReference GripPress { get { return m_GripPress; } set { m_GripPress = value; } }

        [SerializeField]
        private InputActionReference m_TriggerPress = null;
        public InputActionReference TriggerPress { get { return m_TriggerPress; } set { m_TriggerPress = value; } }

        [SerializeField]
        private InputActionReference m_TrackpadPress = null;
        public InputActionReference TrackpadPress { get { return m_TrackpadPress; } set { m_TrackpadPress = value; } }

        [SerializeField]
        private InputActionReference m_TrackpadTouch = null;
        public InputActionReference TrackpadTouch { get { return m_TrackpadTouch; } set { m_TrackpadTouch = value; } }

        private Text m_Text = null;

        private void Start()
        {
            m_Text = GetComponent<Text>();
        }
    }
}
