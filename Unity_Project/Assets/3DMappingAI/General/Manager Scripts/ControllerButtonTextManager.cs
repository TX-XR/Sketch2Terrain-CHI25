using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace MappingAI
{

    
    /// <summary>Updates text hints on the controller</summary>
    public class ControllerButtonTextManager : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private TMP_Text LeftHandThumbstick;
        [SerializeField] private TMP_Text LeftHandButton1;
        [SerializeField] private TMP_Text LeftHandButton2;
        [SerializeField] private TMP_Text LeftHandTrigger;
        [SerializeField] private TMP_Text LeftHandGrip;

        [SerializeField] private TMP_Text RightHandThumbstick;
        [SerializeField] private TMP_Text RightHandButton1;
        [SerializeField] private TMP_Text RightHandButton2;
        [SerializeField] private TMP_Text RightHandTrigger;
        [SerializeField] private TMP_Text RightHandGrip;

        public MeshRenderer secondaryHandAppearance;

        private TMP_Text PrimaryHandThumbstick;
        private TMP_Text PrimaryHandButton1;
        private TMP_Text PrimaryHandButton2;
        private TMP_Text PrimaryHandTrigger;
        private TMP_Text PrimaryHandGrip;

        private TMP_Text SecondaryHandThumbstick;
        private TMP_Text SecondaryHandButton1;
        private TMP_Text SecondaryHandButton2;
        private TMP_Text SecondaryHandTrigger;
        private TMP_Text SecondaryHandGrip;

        private string buttonTextXandA;
        private string buttonTextYandB;
        private InputController inputController;

        private void OnEnable()
        {
            ApplicationSettings.Instance.ButtonController.OnPrimary2DAxisPress.AddListener(() =>
                UpdateTextColor(LeftHandThumbstick, RightHandThumbstick, Color.red));

            // buttonXandA
            ApplicationSettings.Instance.ButtonController.OnPrimaryButtonPress.AddListener(() =>
                UpdateTextColor(LeftHandButton1, RightHandButton1, Color.red));

            // buttonYandB
            ApplicationSettings.Instance.ButtonController.OnSecondaryButtonPress.AddListener(() =>
                UpdateTextColor(LeftHandButton2, RightHandButton2, Color.red));

            // buttonTrigger
            ApplicationSettings.Instance.ButtonController.OnTriggerPress.AddListener(() =>
    UpdateTextColor(LeftHandTrigger, RightHandTrigger, Color.red));

            // buttonTrigger
            ApplicationSettings.Instance.ButtonController.OnGripPress.AddListener(() =>
    UpdateTextColor(LeftHandGrip, RightHandGrip, Color.red));

            ApplicationSettings.Instance.ButtonController.OnPrimaryButtonRelease.AddListener(() =>
                UpdateTextColor(LeftHandButton1, RightHandButton1, Color.white));
            ApplicationSettings.Instance.ButtonController.OnSecondaryButtonRelease.AddListener(() =>
                UpdateTextColor(LeftHandButton2, RightHandButton2, Color.white));
            ApplicationSettings.Instance.ButtonController.OnTriggerRelease.AddListener(() =>
UpdateTextColor(LeftHandTrigger, RightHandTrigger, Color.white));

            ApplicationSettings.Instance.ButtonController.OnGripRelease.AddListener(() =>
UpdateTextColor(LeftHandGrip, RightHandGrip, Color.white));

            // Register for the calibration type changed event to update the mode text field
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchStart, UpdateText);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationInProgress, ()=> { SecondaryHandTrigger.enabled = false; });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.RotationToolHot, () => { SecondaryHandTrigger.SetText("Trigger:\nRotation"); });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.RotationToolCold, () => { SecondaryHandTrigger.SetText("Trigger:\nMove"); });
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchStart, UpdateText);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationInProgress, () => { SecondaryHandTrigger.enabled = false; });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.RotationToolHot, () => { SecondaryHandTrigger.SetText("Trigger:\nRotation"); });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.RotationToolCold, () => { SecondaryHandTrigger.SetText("Trigger:\nMove"); });
        }
        private void Awake()
        {
           if (ApplicationSettings.Instance.primaryHand == XRNode.RightHand)
            {
                PrimaryHandThumbstick = RightHandThumbstick;
                PrimaryHandButton1 = RightHandButton1;
                PrimaryHandButton2 = RightHandButton2;
                PrimaryHandTrigger = RightHandTrigger;
                PrimaryHandGrip = RightHandGrip;

                SecondaryHandThumbstick = LeftHandThumbstick;
                SecondaryHandButton1 = LeftHandButton1;
                SecondaryHandButton2 = LeftHandButton2;
                SecondaryHandTrigger = LeftHandTrigger;
                SecondaryHandGrip = LeftHandGrip;
            }
            else
            {
                PrimaryHandThumbstick = LeftHandThumbstick;
                PrimaryHandButton1 = LeftHandButton1;
                PrimaryHandButton2 = LeftHandButton2;
                PrimaryHandTrigger = LeftHandTrigger;
                PrimaryHandGrip = LeftHandGrip;

                SecondaryHandThumbstick = RightHandThumbstick;
                SecondaryHandButton1 = RightHandButton1;
                SecondaryHandButton2 = RightHandButton2;
                SecondaryHandTrigger = RightHandTrigger;
                SecondaryHandGrip = RightHandGrip;
            }

            //layerManager = FindObjectOfType<LayerManager>().GetComponent<LayerManager>();
            inputController = FindAnyObjectByType<InputController>();
        }
        private void Start()
        {
            if (ApplicationSettings.Instance.primaryHand == XRNode.LeftHand)
            {
                buttonTextXandA = "A:";
                buttonTextYandB = "B:";
            }
            else
            {
                buttonTextXandA = "X:";
                buttonTextYandB = "Y:";
            }
            UpdateText();
        }


        private void Update()
        {
            CheckControllerState();
        }

        private void CheckControllerState()
        {
            // righ thand is pen, left hand is controller
            if (ApplicationSettings.Instance.primaryHand == XRNode.RightHand)
            {
                // primaryHand
                if (!OculusInputManager.GetRightControllerState())
                {
                    if (PrimaryHandManager.getVisibilityState())
                        PrimaryHandManager.changeVisibility(false);
                }
                else
                {
                    if (!PrimaryHandManager.getVisibilityState())
                        PrimaryHandManager.changeVisibility(true);
                }
                // secondary hand
                if (!OculusInputManager.GetLeftControllerState() && (LeftHandThumbstick.enabled || LeftHandButton1.enabled || LeftHandButton2.enabled || LeftHandTrigger.enabled || LeftHandGrip.enabled))
                {
                    LeftHandThumbstick.enabled = false;
                    LeftHandButton1.enabled = false;
                    LeftHandButton2.enabled = false;
                    LeftHandTrigger.enabled = false;
                    LeftHandGrip.enabled = false;
                    secondaryHandAppearance.enabled = false;

                }
                else
                {
                    if (OculusInputManager.GetLeftControllerState() && !(LeftHandThumbstick.enabled || LeftHandButton1.enabled || LeftHandButton2.enabled || LeftHandTrigger.enabled || LeftHandGrip.enabled))
                    {
                        UpdateText();
                    }
                }
            }
            else
            {    // primaryHand
                if (!OculusInputManager.GetLeftControllerState())
                {
                    if (PrimaryHandManager.getVisibilityState())
                        PrimaryHandManager.changeVisibility(false);
                }
                else
                {
                    if (!PrimaryHandManager.getVisibilityState())
                        PrimaryHandManager.changeVisibility(true);
                }

                // secondary hand
                if (!OculusInputManager.GetRightControllerState() && (RightHandThumbstick.enabled || RightHandButton1.enabled || RightHandButton2.enabled || RightHandTrigger.enabled || RightHandGrip.enabled))
                {
                    RightHandThumbstick.enabled = false;
                    RightHandButton1.enabled = false;
                    RightHandButton2.enabled = false;
                    RightHandTrigger.enabled = false;
                    RightHandGrip.enabled = false;
                    secondaryHandAppearance.enabled = false;
                }
                else
                {
                    if (OculusInputManager.GetRightControllerState() && !(RightHandThumbstick.enabled || RightHandButton1.enabled || RightHandButton2.enabled || RightHandTrigger.enabled || RightHandGrip.enabled))
                    {
                        UpdateText();
                    }
                }
            }
                
            
        }

        private void UpdateText()
        {
            switch (ApplicationSettings.Instance.ModeType)
            {
                case ModeType.SurfaceCalibration:
                    SecondaryHandThumbstick.enabled = true;
                    SecondaryHandButton1.enabled = true;
                    SecondaryHandButton2.enabled = true;
                    SecondaryHandTrigger.enabled = false;
                    SecondaryHandGrip.enabled = false;

                    SecondaryHandThumbstick.SetText("Thumbstick\nSurface Calibration");
                    SecondaryHandButton1.SetText(buttonTextXandA + "Calibrate");
                    SecondaryHandButton2.SetText(buttonTextYandB + "Reset");

                    if (AnchorUuidStore.Uuids.ToArray().Length > 0 && ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
                    {
                        SecondaryHandTrigger.enabled = true;
                        SecondaryHandTrigger.SetText("Trigger:\nLoad Environment");
                    }

                    PrimaryHandThumbstick.enabled = false;
                    PrimaryHandButton1.enabled = false;
                    PrimaryHandButton2.enabled = false;
                    PrimaryHandTrigger.enabled = false;
                    PrimaryHandGrip.enabled = false;
                    break;
                case ModeType.Sketch:
                    SecondaryHandThumbstick.enabled = true;
                    SecondaryHandButton1.enabled = true;
                    SecondaryHandButton2.enabled = true;
                    SecondaryHandTrigger.enabled = true;
                    SecondaryHandGrip.enabled = true;

                    SecondaryHandThumbstick.SetText("Thumbstick\nPress: Next Task");
                    SecondaryHandButton1.SetText(buttonTextXandA+"Undo");
                    SecondaryHandButton2.SetText(buttonTextYandB+"Redo");
                    //SecondaryHandTrigger.SetText("Trigger:\nMove");
                    //SecondaryHandGrip.SetText("Grip:\nZoom");
                    switch (inputController.GetUndominantTrigger())
                    {
                        case UndominantTrigger.Grab:
                            SecondaryHandTrigger.SetText("Trigger:\nAdjust height");
                            break;
                        case UndominantTrigger.Erase:
                            SecondaryHandTrigger.SetText("Trigger:\nEraser");
                            break;
                        case UndominantTrigger.None:
                            SecondaryHandTrigger.SetText("");
                            break;
                    }

                    switch (inputController.GetUndominantGrip())
                    {
                        case UndominantGrip.Grab:
                            SecondaryHandGrip.SetText("Grip:\nAdjust height");
                            break;
                        case UndominantGrip.Zoom:
                            SecondaryHandGrip.SetText("Grip:\nZoom");
                            break;
                        case UndominantGrip.DisableAI:
                            SecondaryHandGrip.SetText("Disable AI");
                            break;
                        case UndominantGrip.None:
                            SecondaryHandGrip.SetText("");
                            break;
                    }
                    break;
            }
        }
        private void UpdateTextColor(TMP_Text button1, TMP_Text button2, Color color)
        {
            button1.color = color;
            button2.color = color;
        }
    }
}