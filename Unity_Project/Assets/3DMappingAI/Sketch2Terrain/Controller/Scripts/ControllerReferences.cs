using UnityEngine;
using UnityEngine.XR;

namespace MappingAI
{
    /// <summary>Provides references of the controller</summary>
    public class ControllerReferences : MonoBehaviour
    {
        [Tooltip("The target hand (LeftHand or RightHand)")]
        public XRNode hand;

        [Tooltip("Reference to the stylus tip")]
        public Transform controllerStylusTip = default;

        [Tooltip("Reference to the controller visualization")]
        public Transform controllerVisualization = default;

        [Tooltip("Reference to the XRControllerInput script for button input")]
        public XRControllerInput xrControllerInput = default;
    }
}