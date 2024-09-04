using UnityEngine;
using UnityEngine.XR;

/// <summary>Triggers button based events for distance interaction with a surface</summary>
namespace MappingAI
{
    public class XRInputManager: MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the head")]
        public Transform head;

        [Tooltip("Reference to the RightHand")]
        public ControllerReferences rightHand;

        [Tooltip("Reference to the LeftHand")] 
        public ControllerReferences leftHand;

        public static XRInputManager _instance;

        /// <summary>Handles the singleton instance</summary>
        public static XRInputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<XRInputManager>();

                    if (_instance == null)
                    {
                        GameObject container = new GameObject("XRInputManager");
                        _instance = container.AddComponent<XRInputManager>();
                    }
                }

                return _instance;
            }
        }

        /// <summary>Returns whether the interaction button is pressed</summary>
        public bool IsInteractionButtonPressed()
        {
            return ApplicationSettings.Instance.ButtonController.triggerButton;
        }
    }
}