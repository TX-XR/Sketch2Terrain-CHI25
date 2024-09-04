using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace MappingAI
{
    public enum ModeType
    {
        SurfaceCalibration,
        Sketch,
    }

    public enum SurfaceType
    {
        DrawingSurface,
        DeskPivot
    }
    public enum ExperimentCondition
    {
        _2D,
        _3D,
        _AI
    }

    public enum DevelopmentMode
    {
        DataCollection,
        Experimentation,
        MaterialPreparation
    }

    public class ApplicationSettings : ScriptableObject
    {
        [Header("Mapbox")]
        [SerializeField]
        [Tooltip("Important: Set your access token!!")]
        private string mapboxAccessToken = "";

        [Header("Handness")]
        [Tooltip("The hand the stylus is attached to (LeftHand or RightHand)")]
        public XRNode primaryHand = XRNode.RightHand;
        [Header("Interface")]
        [SerializeField]
        [Tooltip("Choose between data collection, experimentation or material preparation modes")]
        private DevelopmentMode developmentMode = DevelopmentMode.DataCollection;

        [SerializeField]
        [Tooltip("Choose between 2D, 3D, AI conditions")]
        private ExperimentCondition experimentCondition = MappingAI.ExperimentCondition._3D;
        [Tooltip("The active calibration type")]
        [HideInInspector]
        public ModeType ModeType = ModeType.SurfaceCalibration;

        public string participantID = "0";

        private Transform _stylus;

        private static ApplicationSettings _instance;


        public DevelopmentMode DevelopmentMode => ApplicationSettings.Instance.developmentMode;
        public ExperimentCondition ExperimentCondition
        {
            get { return Instance.experimentCondition; }
            set { Instance.experimentCondition = value; }
        }
    

        public static ApplicationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ApplicationSettings>("ApplicationSettings");
                    _instance.ModeType = ModeType.SurfaceCalibration;
                }

                return _instance;
            }

            set { _instance = value; }
        }

        public string Mapbox_accessToken => mapboxAccessToken;
        /// <summary>References to the RightHand</summary>
        public ControllerReferences RightHand => XRInputManager.Instance.rightHand;

        /// <summary>References to the LeftHand</summary>
        public ControllerReferences LeftHand => XRInputManager.Instance.leftHand;

        /// <summary>References to the Controller Visualization</summary>
        public Transform ControllerVisualization =>
            primaryHand == XRNode.RightHand
                ? RightHand.controllerVisualization
                : LeftHand.controllerVisualization;


        /// <summary>References to the Stylus Tip</summary>
        public Transform PrimaryStylusTip =>
            primaryHand == XRNode.RightHand
                ? RightHand.controllerStylusTip
                : LeftHand.controllerStylusTip;

        public Transform SecondaryStylusTip =>
            primaryHand == XRNode.RightHand
                ? LeftHand.controllerStylusTip
                : RightHand.controllerStylusTip;

        /// <summary>References to the non-stylus controller used for button interactions</summary>
        public XRControllerInput ButtonController =>
            primaryHand == XRNode.RightHand
                ? LeftHand.xrControllerInput
                : RightHand.xrControllerInput;

        /// <summary>References to the camera attached to the stylus for Unity UI interactions</summary>
        public Transform StylusUiCamera =>
            PrimaryStylusTip.GetComponentInChildren<Camera>().transform;

        // this is for Calibrate the Surface
        public Transform GetStylus()
        {
            if (_stylus == null)
                _stylus = FindObjectOfType<Stylus>().transform;
            
            return _stylus;
        }

        private void OnValidate()
        {
            if (primaryHand != XRNode.LeftHand && primaryHand != XRNode.RightHand)
            {
                Debug.LogWarning("Selected non-hand as primary hand. Not possible, falling back to right hand", this);
                primaryHand = XRNode.RightHand;
            }
        }
    }
}