using System.Collections;
//using Valve.VR;
using UnityEngine;
using Unity.Profiling;
using UnityEditor;
using VRSketch;
using System;
using Mapbox.Unity.MeshGeneration.Data;
using Unity.Mathematics;

namespace MappingAI
{
    public enum UndominantGrip
    {
        Zoom,
        Grab,
        DisableAI,
        None
}
    public enum UndominantTrigger
    {
        Grab,
        Erase,
        None
    }
    public class InputController : MonoBehaviour
    {
        // SETTINGS
        [Header("Settings")]
        public bool HapticOnCollision = false;
        [SerializeField]
        private KeyCode exportSketchKey = KeyCode.X;
        [SerializeField]
        private KeyCode studyNextKey = KeyCode.Q;
        [SerializeField]
        private KeyCode skipObservation = KeyCode.E;
        [SerializeField]
        private KeyCode backToTheTaskKey = KeyCode.W;
        [SerializeField]
        private KeyCode loadSpatialAnchor = KeyCode.L;
        [SerializeField] AudioSource eraseAudioSource;
        // HANDS
        [Header("Controllers")]
        [SerializeField]
        private OVRInput.Controller primaryHandObject = OVRInput.Controller.RTouch;
        [SerializeField]
        private OVRInput.Controller secondaryHandObject = OVRInput.Controller.LTouch;

        [SerializeField]
        private GameObject primaryHandTip;
        [SerializeField]
        private GameObject SecondaryHandTip;
        // INPUTS
        //[Header("SteamVR Actions")]
        //public SteamVR_Action_Single drawAction;
        //public SteamVR_Action_Boolean addPatchAction;
        //public SteamVR_Action_Boolean eraseAction;
        //public SteamVR_Action_Boolean grabAction;
        //public SteamVR_Action_Boolean zoomAction;
        //public SteamVR_Action_Boolean toggleGridStateAction;
        //public SteamVR_Action_Boolean toggleMirror;
        //public SteamVR_Action_Boolean switchSystemAction;


        //public SteamVR_Action_Pose pose;
        //public SteamVR_Input_Sources primarySource = SteamVR_Input_Sources.RightHand;
        //public SteamVR_Input_Sources secondarySource = SteamVR_Input_Sources.LeftHand;

        // ACTION CONTROLLERS
        private DrawController drawController;
        private EraseController eraseController;
        private TextTool textTool;
        private GrabController grabController;
        private ZoomController zoomController;
        private ExportController exportController;

        // APPEARANCE CONTROLLERS
        [Header("App appearance controllers")]
        public BrushAppearance primaryHandAppearance;
        public BrushAppearance secondaryHandAppearance;
        public ZoomInteractionAppearance zoomInteractionAppearance;
        private Grid3D grid;

        // STUDY STUFF
        [Header("Study")]
        private StudyScenario scenario;
        public float IdleRecordFrequency = 2f;
        public InstructionsDisplay instructionsDisplay;
        public ControllerType controllerType = ControllerType.Oculus;
        public bool ShowInstructions = true;
        private bool InObservationMode = false;
        private EraserTool eraserTool;

        private Transform headTransform;
        private DrawingCanvas drawingCanvas;
        private TerrainManagerAsync terrainManager;
        private ParametersManager parameters = null;
        private AIModelManager aIModelManager = null;

        private Action currentAction = Action.Idle;

        private float lastRecordedIdle = 0f;

        // Study task settings
        private VRSketch.InteractionMode mode;
        private SketchSystem sketchSystem = SketchSystem.Snap;


        private bool mirrorAvailable = false;
        private bool mirroring = true;

        // Special moments during the study
        private bool isInBreakMode = false;
        private bool waitingForConfirm = false;
        private bool isEarserHot = false;
        private bool isInSketchRange = false;
        private float lastContinueInputTime;
        float tileSize = -1f;
        float strokeOffsetTimesFromBottomPlane = 0.5f;
        static ProfilerMarker s_InputLoopMarker = new ProfilerMarker("VRSketch.TreatInput");

        [SerializeField]
        private UndominantTrigger undominantTrigger = UndominantTrigger.Erase; // decide whether the Trigger button on Undominant hand is move or erase
        [SerializeField]
        private UndominantGrip undominantGrip = UndominantGrip.DisableAI; // decide whether the Grip button on Undominant hand is zoom or change color
        public bool ShowGrid = false; // decide whether the Grip button on Undominant hand is zoom or change color
        public bool Commit2Dand3DStrokes = false; // decide whether the Grip button on Undominant hand is zoom or change color

        private enum Action
        {
            Draw,
            Grab,
            Zoom,
            Idle,
            Erase
        }

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolHot, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isEarserHot = true; currentAction = Action.Erase; secondaryHandAppearance.OnEraseStart(); } }));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolCold, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isEarserHot = false; currentAction = Action.Idle; secondaryHandAppearance.OnEraseEnd(); } }));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchRangeIn, (() => { isInSketchRange = true; }));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchRangeOut, (() => { isInSketchRange = false; }));
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolHot, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isEarserHot = true; currentAction = Action.Erase; secondaryHandAppearance.OnEraseStart(); } }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolCold, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isEarserHot = false; currentAction = Action.Idle; secondaryHandAppearance.OnEraseEnd(); } }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchRangeIn, (() => { isInSketchRange = true; }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchRangeOut, (() => { isInSketchRange = false; }));
        }

        private void Start()
        {
            // Sort out handed-ness
            bool rightHanded = OculusInputManager.GetDominatehand() == OVRInput.Controller.RTouch;
            if (!rightHanded)
            {
                // Change controller mappings
                primaryHandObject = OVRInput.Controller.LTouch;
                secondaryHandObject = OVRInput.Controller.RTouch;
            }
            else
            {
                // Change controller mappings
                primaryHandObject = OVRInput.Controller.RTouch;
                secondaryHandObject = OVRInput.Controller.LTouch;
            }
            isEarserHot = false;

            headTransform = FindAnyObjectByType<OVRCameraRig>().centerEyeAnchor;
            terrainManager = ComponentManager.Instance.GetTerrainManagerAsync();
            parameters = ComponentManager.Instance.GetCASSIEParametersProvider();
            drawingCanvas = ComponentManager.Instance.GetDrawingCanvas();
            drawController = ComponentManager.Instance.GetDrawController();
            scenario = ComponentManager.Instance.GetStudyScenario();
            eraseController = ComponentManager.Instance.GetEraseController();
            grabController = ComponentManager.Instance.GetGrabController();
            zoomController = ComponentManager.Instance.GetZoomController();
            exportController = ComponentManager.Instance.GetExportController();
            aIModelManager = ComponentManager.Instance.GetAIModelManager();
            grid = ComponentManager.Instance.GetGrid3D();
            controllerType = StudyUtils.GetControllerType();
            mode = scenario.GetCurrentStep().Mode;
            StudyScenario.GetStudyStepEvent().AddListener(OnStepChange);
            if (eraserTool == null)
                eraserTool = ComponentManager.Instance.GetEraserTool();
        }

        public VRSketch.InteractionMode GetMode()
        {
            return this.mode;
        }

        // Update is called once per frame
        void Update()
        {
            // SPECIAL MODES (ignore all input)

            if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            {
                HandleSurfaceCalibrationMode();
                return;
            }

            s_InputLoopMarker.Begin();

            Vector3 primaryHandPos, secondaryHandPos, drawingPosLocal, brushNormal;
            Quaternion secondaryHandRot;
            UpdatePosRot(out primaryHandPos, out secondaryHandPos, out secondaryHandRot, out drawingPosLocal, out brushNormal);

            if (InObservationMode)
            {
                if (instructionsDisplay.CountDownFinished() || ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.MaterialPreparation || Input.GetKeyUp(skipObservation))
                {
                    BeginNormalStudy();
                    Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.ObservationTaskCold);
                    InObservationMode = false;
                }

                HandleGrab(primaryHandPos, secondaryHandPos, secondaryHandRot);
                return;
            }
            if (isInBreakMode || waitingForConfirm)
                return;

            HandleUndominatehand();

            HandleSketch(primaryHandPos, secondaryHandPos, drawingPosLocal, secondaryHandRot, brushNormal);

#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                EditorApplication.isPlaying = false;
            }
#endif

            if (FreeCreationMode() && Input.GetKeyUp(KeyCode.Escape))
            {
                // Quit app
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
            // OculusInputManager.studyNextAction() 
            // in the normal mode, if the count Down finished, go to the next study directly

            if (SwitchToNextStudy() || (NormalMode() && instructionsDisplay.CountDownFinished()))
            {
                waitingForConfirm = true;
                lastContinueInputTime = Time.time;
                StartCoroutine("WaitForConfirm");
            }

            if (Input.GetKeyUp(exportSketchKey))
            {
                exportController.ExportSketch();
            }
            //if (Input.GetKeyUp(saveStudyLogKey))
            //{
            //    Debug.Log("saving system state");
            //    SaveMidStepAndContinue();
            //}
        }

        private void HandleGrab(Vector3 primaryHandPos, Vector3 secondaryHandPos, quaternion secondaryHandRot)
        {

            // Secondary hand: grabbing

            if (currentAction.Equals(Action.Grab))
            {
                bool flag3, flag4;
                UndominatehandGrip(UndominantGrip.Grab, up: true, out flag3);
                UndominatehandTrigger(UndominantTrigger.Grab, up: true, out flag4);
                if (flag3 || flag4)
                {
                    EndGrabAction(primaryHandPos);
                }
                else
                {
                    //grabController.GrabUpdate(secondaryHandPos, secondaryHandRot);
                    grabController.GrabUpdateConstraint(secondaryHandPos, secondaryHandRot);
                    grid.OnCanvasMove(ShowGrid);
                }
            }
            else
            {
                bool flag1, flag2;
                UndominatehandGrip(UndominantGrip.Grab, up: false, out flag1);
                UndominatehandTrigger(UndominantTrigger.Grab, up: false, out flag2);
                if (flag1 || flag2)
                {
                    StartGrabAction(secondaryHandPos, secondaryHandRot);
                }
            }
            s_InputLoopMarker.End();
        }

        private void UndominatehandGrip(UndominantGrip gripState, bool up, out bool flag)
        {
            if (up)
                flag = (undominantGrip == gripState && OculusInputManager.GripButtonUp_Undominatehand());
            else
                flag = (undominantGrip == gripState && OculusInputManager.GripButtonDown_Undominatehand());
        }

        private void UndominatehandTrigger(UndominantTrigger triggerState, bool up, out bool flag)
        {
            if (up)
                flag = (undominantTrigger == triggerState && OculusInputManager.triggerButtonUp_Undominatehand());
            else
                flag = (undominantTrigger == triggerState && OculusInputManager.triggerButtonDown_Undominatehand());
        }


        private void EndGrabAction(Vector3 primaryHandPos)
        {
            currentAction = Action.Idle;
            secondaryHandAppearance.OnGrabEnd();
            grabController.OnGrabEnd();
            grid.OnTransformEnd(ShowGrid);
            // Log data
            scenario.GetCurrentStep().CanvasTransform(headTransform, primaryHandPos, drawingCanvas.transform, mirroring);
        }

        private void HandleSketch(Vector3 primaryHandPos, Vector3 secondaryHandPos, Vector3 drawingPosLocal, quaternion secondaryHandRot, Vector3 brushNormal)
        {
            RefreshGrid(primaryHandPos);
            if (Idle())
            {
                if (Time.time - lastRecordedIdle > 2f)
                {
                    lastRecordedIdle = Time.time;
                    scenario.GetCurrentStep().Idle(headTransform, primaryHandPos, drawingCanvas.transform, mirroring);
                }
                // Dominant hand: drawing, deleting, adding patch, zooming
                bool canSketch = OculusInputManager.CanSketchOrErase() && isInSketchRange && !isEarserHot;
                if (canSketch)
                {
                    // If overlap with other FinalStroke, add stroke constraint
                    if (TutorialMode() || FreeCreationModeInExperiment())
                    {
                        UpdateSketchCanvas();
                    }
                    currentAction = Action.Draw;
                    primaryHandAppearance.OnDrawStart();
                    if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
                    {
                        drawController.NewStrokeLocal(drawingPosLocal);
                    }
                    else
                    {
                        if (tileSize < 0)
                        {
                            UnityTile[] tiles = terrainManager.GetComponentsInChildren<UnityTile>();
                            float highest = float.MinValue;
                            foreach (var item in tiles)
                            {
                                if (highest < item.gameObject.transform.localPosition.x)
                                    highest = item.gameObject.transform.localPosition.x;
                            }
                            tileSize = highest;
                        }
                        drawController.NewStrokeLocal(drawingPosLocal);
                        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.MaterialPreparation && ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._2D)
                        {
                            drawController.New2DStrokeLocal();
                        }
                    }

                }
                else if (undominantGrip == UndominantGrip.Zoom && OculusInputManager.zoomActionDown())
                {
                    StartZoomAction(primaryHandPos, secondaryHandPos);
                }

                // Secondary hand: grabbing
                else if (OculusInputManager.triggerButtonDown_Undominatehand())
                {
                    switch (undominantTrigger)
                    {
                        case UndominantTrigger.Erase:
                            StartEraseAction();
                            break;
                        case UndominantTrigger.Grab:
                            StartGrabAction(secondaryHandPos, secondaryHandRot);
                            break;
                    }
                }
                else if (undominantGrip == UndominantGrip.Grab && OculusInputManager.GripButtonDown_Undominatehand())
                {
                    StartGrabAction(secondaryHandPos, secondaryHandRot);
                }
                //else if (OculusInputManager.gridStateDown() && ShowGrid)
                //{

                //    // Toggle grid state
                //    grid.ToggleGridState();
                //}
            }

            else if (Draw())
            {
                if (OculusInputManager.CanSketchOrErase() && isInSketchRange && !isEarserHot)
                {
                    // Still drawing
                    Vector3 velocity = OVRInput.GetLocalControllerVelocity(primaryHandObject);
                    float pressure = OculusInputManager.GetSketchPresure();
                    //drawController.UpdateStroke(drawingPos, primaryHandRot, velocity, pressure);
                    drawController.UpdateStrokeLocal(drawingPosLocal, primaryHandPos, brushNormal, velocity, pressure, tileSize);
                    // In the prepaer material mode, generate both 2D and 3D sketch
                    if (Commit2Dand3DStrokes)
                    {
                        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.MaterialPreparation && ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._2D)
                        {
                            var drawing2DPosLocal = drawingCanvas.transform.InverseTransformPoint(new Vector3(primaryHandPos.x, WorkspaceManager.Instance.GetBottomPlaneHeight() + strokeOffsetTimesFromBottomPlane * parameters.Current.StrokeWidth, primaryHandPos.z));
                            drawController.Update2DStrokeLocal(drawing2DPosLocal, brushNormal, velocity, pressure);
                        }
                    }

                }
                else
                {
                    if (TutorialMode())
                    {
                        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
                        {
                            MeshRenderManager.EnableChildMeshRenderers(terrainManager.GetCurrentTerrainGameObject().transform.GetChild(0));
                        }
                        else
                        {
                            MeshRenderManager.EnableChildMeshRenderers(terrainManager.transform);
                        }
                        WorkspaceManager.Instance.EnableUpperBody(false);
                        terrainManager.ActivateExampleAndLandmark(true);
                    }
                    // Commit current stroke
                    currentAction = Action.Idle;
                    primaryHandAppearance.OnDrawEnd();
                    bool success = drawController.CommitStroke(out SerializableStroke strokeData, mirror: mirroring);
                    //Log data
                    if (success)
                        scenario.GetCurrentStep().StrokeAdd(headTransform, primaryHandPos, drawingCanvas.transform, strokeData, mirroring);

                }
            }
            else if (currentAction.Equals(Action.Grab))
            {
                bool flag3, flag4;
                UndominatehandGrip(UndominantGrip.Grab, up: true, out flag3);
                UndominatehandTrigger(UndominantTrigger.Grab, up: true, out flag4);
                if (flag3 || flag4)
                {
                    EndGrabAction(primaryHandPos);
                }
                else
                {
                    //grabController.GrabUpdate(secondaryHandPos, secondaryHandRot);
                    grabController.GrabUpdateConstraint(secondaryHandPos, secondaryHandRot);
                    grid.OnCanvasMove(ShowGrid);
                }
            }
            else if (Erase())
            {   // Try delete first
                if (isEarserHot && OculusInputManager.CanSketchOrErase())
                {
                    bool deleteSuccess = eraseController.TryDelete(out InteractionType type, out int elementID, mirror: mirroring);
                    if (deleteSuccess)
                    {
                        eraseAudioSource?.Play();
                        // Log data
                        scenario.GetCurrentStep().Delete(headTransform, primaryHandPos, drawingCanvas.transform, type, elementID, mirroring);
                    }
                }
                if (undominantTrigger == UndominantTrigger.Erase && OculusInputManager.triggerButtonUp_Undominatehand())
                {
                    currentAction = Action.Idle;
                    secondaryHandAppearance.OnEraseEnd();
                    if (isEarserHot)
                    {
                        if (eraserTool == null)
                            eraserTool = ComponentManager.Instance.GetEraserTool();
                        eraserTool.onValueChanged();
                    }
                    // Log data
                    scenario.GetCurrentStep().CanvasTransform(headTransform, primaryHandPos, drawingCanvas.transform, mirroring);
                }
            }
            else if (currentAction.Equals(Action.Zoom))
            {
                if (OculusInputManager.zoomActionUp())
                {
                    currentAction = Action.Idle;
                    zoomInteractionAppearance.OnZoomEnd();
                    primaryHandAppearance.OnZoomEnd();
                    secondaryHandAppearance.OnZoomEnd();
                    if (ShowGrid)
                        grid.OnTransformEnd();
                    // Log data
                    scenario.GetCurrentStep().CanvasTransform(headTransform, primaryHandPos, drawingCanvas.transform, mirroring);
                }
                else
                {
                    float handsDistance = Vector3.Distance(primaryHandPos, secondaryHandPos);
                    bool success = zoomController.UpdateZoom(secondaryHandPos, handsDistance, out float newScale);
                    zoomInteractionAppearance.OnZoomUpdate(primaryHandPos, secondaryHandPos, success, newScale);

                }
            }

            s_InputLoopMarker.End();
        }

        private void StartZoomAction(Vector3 primaryHandPos, Vector3 secondaryHandPos)
        {
            currentAction = Action.Zoom;
            primaryHandAppearance.OnZoomStart();
            secondaryHandAppearance.OnZoomStart();
            zoomInteractionAppearance.OnZoomStart();
            grid.OnTransformStart(ShowGrid);
            float handsDistance = Vector3.Distance(primaryHandPos, secondaryHandPos);
            zoomController.StartZoom(handsDistance);
        }

        void StartEraseAction()
        {
            currentAction = Action.Erase;
            secondaryHandAppearance.OnEraseStart();
            if (eraserTool == null)
                eraserTool = ComponentManager.Instance.GetEraserTool();
            if (!isEarserHot)
                eraserTool.onValueChanged();
        }

        void StartGrabAction(Vector3 secondaryHandPos, Quaternion secondaryHandRot)
        {
            currentAction = Action.Grab;
            secondaryHandAppearance.OnGrabStart();
            grid.OnTransformStart(ShowGrid);
            grabController.GrabStart(secondaryHandPos, secondaryHandRot);
        }
        private void UpdateSketchCanvas()
        {
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
            {
                MeshRenderManager.DisableChildMeshRenderers(terrainManager.GetCurrentTerrainGameObject().transform);
                WorkspaceManager.Instance.ActivateBottomPlane(true);
                terrainManager.GetCurrentLandmarkGameObject().SetActive(false);
            }
            if (ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._2D)
            {
                WorkspaceManager.Instance.DisableUpperBody();
            }
            else
            {
                WorkspaceManager.Instance.ActivateBottomPlane(true);
            }
        }

        private bool TutorialMode()
        {
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
            {
                bool flag = this.mode == VRSketch.InteractionMode.Tutorial;
                return flag;
            }
            return false;
        }
        public bool FreeCreationModeInExperiment()
        {
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
            {
                bool flag = this.mode == VRSketch.InteractionMode.FreeCreation;
                return flag;
            }
            return false;
        }
        private void HandleSurfaceCalibrationMode()
        {
            if (OculusInputManager.ButtonOneDown(true) || OculusInputManager.ButtonOneDown(false))
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.AddCalibrationPoint);
            }
            else if (OculusInputManager.ButtonTwoDown(true) || OculusInputManager.ButtonTwoDown(false))
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SurfaceCalibrationReset);
            }
            else if (OculusInputManager.IsLoadAnchorsPrimaryIndexTrigger(true) || OculusInputManager.IsLoadAnchorsPrimaryIndexTrigger() || Input.GetKeyDown(loadSpatialAnchor))
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.TryLoadSpatialAnchor);
            }
        }

        private void HandleUndominatehand()
        {
            if (OculusInputManager.ButtonOneDown(true) || OculusInputManager.ButtonOneDown(false))
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.Undo);
            }
            if (OculusInputManager.ButtonTwoDown(true) || OculusInputManager.ButtonTwoDown(false))
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.Redo);
            }

            // if press the Grip button down, the ExecuteInference is cold. If press the Grip button is up, the ExecuteInference is hot
            bool flag3, flag4;
            UndominatehandGrip(UndominantGrip.DisableAI, up: false, out flag3);
            UndominatehandGrip(UndominantGrip.DisableAI, up: true, out flag4);
            if (flag3)
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.ExecuteInferenceCold);
                aIModelManager.CanAIModelRender(false);
            }
            else if (flag4)
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.ExecuteInferenceHot);
                aIModelManager.CanAIModelRender(true);
                aIModelManager.ExecuteInferenceAsync();
            }

        }

        public void ChangeHandAppearance()
        {
            //MaterialManager.Instance.ChangeMaterial();
            secondaryHandAppearance.SetColor(MaterialManager.Instance.CurrentSelectedMaterial.color);
            primaryHandAppearance.SetColor(MaterialManager.Instance.CurrentSelectedMaterial.color);
        }
        public void ChangeHandAppearance(Color color)
        {
            //MaterialManager.Instance.ChangeMaterial();
            secondaryHandAppearance.SetColor(color);
            primaryHandAppearance.SetColor(color);
        }
        private bool SwitchToNextStudy()
        {
            bool flag = Time.time > lastContinueInputTime + 0.5f && (Input.GetKeyUp(studyNextKey) || (!InObservationMode && OculusInputManager.NextStudy()));
            return flag;
        }

        private void UpdatePosRot(out Vector3 primaryHandPos, out Vector3 secondaryHandPos, out Quaternion secondaryHandRot, out Vector3 drawingPosLocal, out Vector3 brushNormal)
        {
            primaryHandPos = primaryHandTip.transform.position;
            Quaternion primaryHandRot = primaryHandTip.transform.rotation;

            secondaryHandPos = SecondaryHandTip.transform.position;
            secondaryHandRot = SecondaryHandTip.transform.rotation;

            // if the experimentCondition is 2D, then snap the sketch on bottom surface
            if (ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._2D)
            {
                //drawingPosLocal = canvas.transform.InverseTransformPoint(new Vector3(primaryHandPos.x, transform.position.y + parameters.Current.StrokeWidth, primaryHandPos.z));
                drawingPosLocal = drawingCanvas.transform.InverseTransformPoint(new Vector3(primaryHandPos.x, WorkspaceManager.Instance.GetBottomPlaneHeight() + strokeOffsetTimesFromBottomPlane * parameters.Current.StrokeWidth, primaryHandPos.z));
            }
            else
            {
                primaryHandPos.y = Math.Clamp(primaryHandPos.y, WorkspaceManager.Instance.GetBottomPlaneHeight() + strokeOffsetTimesFromBottomPlane * parameters.Current.StrokeWidth, float.MaxValue);
                drawingPosLocal = drawingCanvas.transform.InverseTransformPoint(primaryHandPos);
            }

            brushNormal = drawingCanvas.transform.InverseTransformDirection(primaryHandRot * new Vector3(0, 0, 1));
        }

        private void RefreshGrid(Vector3 primaryHandPos)
        {
            if (!ShowGrid)
            {
                grid.gameObject.SetActive(false);
                return;
            }
            if (grid == null)
            {
                var g = ComponentManager.Instance.GetGrid3D();
                if (g != null)
                {
                    grid = g;
                    drawingCanvas = ComponentManager.Instance.GetDrawingCanvas();
                }
            }

            grid.Refresh(primaryHandPos);
        }
        public bool Draw()
        {
            return currentAction.Equals(Action.Draw);
        }
        public bool Idle()
        {
            return currentAction.Equals(Action.Idle);
        }
        public bool Erase()
        {
            return currentAction.Equals(Action.Erase);
        }

        public UndominantTrigger GetUndominantTrigger()
        {
            return this.undominantTrigger;
        }
        public UndominantGrip GetUndominantGrip()
        {
            return this.undominantGrip;
        }
        /// <summary>
        /// disable the terrain before start sketching
        /// </summary>
        private void BeginNormalStudy()
        {

            UpdateSketchCanvas();
            if (terrainManager.GetCurrentExampleGameObject() != null)
                terrainManager.GetCurrentExampleGameObject().SetActive(false);
            // Start Countdown
            instructionsDisplay.SetCountdown(scenario.GetCurrentStep().TimeLimit);

            OnModeChange(VRSketch.InteractionMode.Normal);
            UpdateInstructions();
        }

        private void BeginObservationStudy()
        {
            if (this.mode.Equals(VRSketch.InteractionMode.Observation))
            {
                // Start Countdown
                //instructionsDisplay.SetCountdown(scenario.GetCurrentStep().TimeLimitForObservation);
                if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
                {
                    instructionsDisplay.SetCountdown(scenario.GetCurrentStep().TimeLimitForObservation);

                    //instructionsDisplay.SetCountdown(10);
                }
                else if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.MaterialPreparation) //prepare materials
                {
                    //instructionsDisplay.SetCountdown(1);
                }
                UpdateInstructions();
            }
        }

        public bool FreeCreationMode()
        {
            return this.mode.Equals(VRSketch.InteractionMode.FreeCreation);
        }

        // Break mode
        IEnumerator BreakTime()
        {
            //instructionsDisplay.SetText(
            //    "Break time\n" +
            //    "Take a break for as long as you like \n" +
            //    //"Press " + studyNextKey + " or Thumbstick Button for non-dominant hand" + " to start the next task.",
            //    "Press Thumbstick Button on " + StudyUtils.undominatehand + " controller to start the next task.",
            //    modalMode: false
            //    );            
            instructionsDisplay.SetText(
                "Break time\n" +
                "Take a break for as long as you like \n" +
                //"Press " + studyNextKey + " or Thumbstick Button for non-dominant hand" + " to start the next task.",
                "Inform the experimenter to start the next task.",
                modalMode: false
                );

            instructionsDisplay.PauseCountdown();
            // Hide everything from screen
            drawingCanvas.gameObject.SetActive(false);
            //wait for button to be pressed (wait at least 0.5s for the break)
            while (true)
            {

                if (SwitchToNextStudy())
                    break;
                yield return null;
            }

            lastContinueInputTime = Time.time;
            // Confirm end break
            isInBreakMode = false;
            drawingCanvas.gameObject.SetActive(true);
            
            bool nextStepExists = scenario.NextStep();

            if (!nextStepExists)
            {
                lastContinueInputTime = Time.time;
                isInBreakMode = true;
                StartCoroutine("FinishStudy");
            }
            //StartCoroutine("WaitForConfirmBreakEnd");
        }


        // Confirm dialog
        IEnumerator WaitForConfirm()
        {
            //instructionsDisplay.SetText(
            //"You are about to end the task.\n" +
            ////"Press " + studyNextKey + " or Thumbstick Button for non-dominant hand"+ " to confirm, \n or the " + backToTheTaskKey + " or Trigger button for non-dominant hand" + " to go back to the task.",
            //"Press Thumbstick Button on " + StudyUtils.undominatehand + " controller to confirm, \n or the Trigger Button Button on " + StudyUtils.undominatehand + " controller to go back to the task.",
            //modalMode: false
            //);
            instructionsDisplay.SetText(
                "You are about to end the task.\n" +
                //"Press " + studyNextKey + " or Thumbstick Button for non-dominant hand"+ " to confirm, \n or the " + backToTheTaskKey + " or Trigger button for non-dominant hand" + " to go back to the task.",
                "inform the experimenter to confirm, or go back to the task.",
                modalMode: false
);
            // Hide everything from screen
            drawingCanvas.gameObject.SetActive(false);

            instructionsDisplay.PauseCountdown();

            bool confirm = false;

            while (true)
            {
                if (SwitchToNextStudy())
                {
                    confirm = true;
                    break;
                }
                if (Time.time > lastContinueInputTime + 0.5f && (Input.GetKeyUp(backToTheTaskKey) || OculusInputManager.BackToTaskKey()))
                {
                    confirm = false;
                    break;
                }
                yield return null;
            }

            waitingForConfirm = false;
            drawingCanvas.gameObject.SetActive(true);

            if (confirm)
            {
                // Confirm action
                EndStep();
            }

            else
            {
                // Cancel
                CancelEndAction();
                instructionsDisplay.UnpauseCountdown(Time.time - lastContinueInputTime);
            }
        }
        IEnumerator FinishStudy()
        {
            // Display end text
            instructionsDisplay.SetText(
            "You have completed all tasks.\n" +
            "Thanks for participating in the study!\n" +
            "Press Thumbstick Button on " + StudyUtils.undominatehand + " controller to exit the application."
            );

            // Hide everything from screen
            drawingCanvas.gameObject.SetActive(false);

            while (true)
            {
                if (SwitchToNextStudy())
                    break;
                yield return null;
            }

            // Quit app
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnStepChange()
        {

            drawingCanvas.ResetPosition_Rotation_Scale();
            OnSystemChange(scenario.GetCurrentStep().System);
            OnModeChange(scenario.GetCurrentStep().Mode);
            //OnModelChange(scenario.GetCurrentStep().Model);

            // Update the terrain
            if (FreeCreationMode())
            {
                terrainManager.NextTerrain(scenario.GetCurrentStep().Zoom);
                ComponentManager.Instance.GetUIManager().updateCurrentUI_DataCollection();
            }
            else
            {
                terrainManager.LoadGivenTerrain(scenario.GetCurrentStep(), scenario.GetCurrentStep().Zoom);
                if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
                {

                    MeshRenderManager.EnableChildMeshRenderers(terrainManager.GetCurrentTerrainGameObject().transform);

                }
                else
                {
                    MeshRenderManager.EnableChildMeshRenderers(terrainManager.transform);
                }
                terrainManager.GetCurrentLandmarkGameObject().SetActive(true);
                WorkspaceManager.Instance.EnableUpperBody();
                WorkspaceManager.Instance.ActivateBottomPlane(false);
            }

            if (ObservationMode())
            {
                InObservationMode = true;
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.ObservationTaskHot);
                BeginObservationStudy();
            }
        }

        private bool ObservationMode()
        {
            return this.mode == VRSketch.InteractionMode.Observation;
        }

        private bool NormalMode()
        {
            return this.mode == VRSketch.InteractionMode.Normal;
        }

        private void OnSystemChange(SketchSystem newSystem, bool clearCanvas = true)
        {

            this.sketchSystem = newSystem;
            //primaryHandAppearance.OnModeChange(newSystem);
            // Set parameters according to system
            bool surfacing = false;
            switch (newSystem)
            {
                case SketchSystem.Baseline:
                    {
                        // Deactivate both beautification and surfacing
                        drawController.Beautification = false;
                        break;
                    }
                case SketchSystem.Snap:
                    {
                        // Activate beautification
                        drawController.Beautification = true;
                        break;
                    }
                case SketchSystem.SnapSurface:
                    {
                        // Activate both
                        drawController.Beautification = true;
                        surfacing = true;
                        break;
                    }
                default:
                    {
                        drawController.Beautification = false;
                        break;
                    }
            }

            if (clearCanvas)
                drawController.Init(surfacing);
            else
                drawController.SwitchSystem(surfacing);
        }

        private void OnModeChange(VRSketch.InteractionMode mode)
        {

            this.mode = mode;
        }

        public void UpdateInstructions()
        {
            if (!ShowInstructions)
            {
                return;
            }

            string instructions = "";
            instructions += StudyUtils.InteractionModeInstructions[this.mode];

            if(FreeCreationMode())
            {
                instructions += terrainManager.SetInstuction(terrainManager.GetIndex(), Math.Round(terrainManager.GetCurrent_Lat_lon().x, 8), Math.Round(terrainManager.GetCurrent_Lat_lon().y, 8));
                //instructions += terrainManager.SetInstuction(scenario.GetCurrentStep().TerrainName, terrainManager.indexForGivenTerrain, Math.Round(terrainManager.Current_Lat_lon.x, 8), Math.Round(terrainManager.Current_Lat_lon.y, 8));
                //instructions += terrainManager.SetInstuction(terrainManager.indexForGivenTerrain, Math.Round(terrainManager.Current_Lat_lon.x, 8), Math.Round(terrainManager.Current_Lat_lon.y, 8));

            }
            else
            {
                instructions += "\n\nScene " + (terrainManager.GetIndexForGivenTerrain() - 1) + "\n";
            }

            instructionsDisplay.SetText(instructions);
        }

        private void CancelEndAction()
        {
            UpdateInstructions();
        }
        // end current step
        private void EndStep()
        {
            // Save sketch and study information
            aIModelManager.ExecuteInferenceAsync();
            exportController.ExportSketch();
            string terrainPathName = exportController.GetTerrainPath();
            scenario.EndStep(terrainPathName);
            // Export sketch and clear canvas
            eraseController.Clear();

            if (eraserTool == null)
                eraserTool = ComponentManager.Instance.GetEraserTool();
            if (eraserTool.GetEraserState())
            {
                eraserTool.onValueChanged();
            }

            ResetHandAppearance();
            aIModelManager.ResetAIModelMesh();

            if (textTool == null)
            {
                textTool = ComponentManager.Instance.GetTextTool();

            }
            if (textTool != null)
            {
                textTool.checkState();
            }

            if (FreeCreationMode())
            {
                scenario.RedoStep();
            }
            else
            {
                // Eventual break time
                if (scenario.GetCurrentStep().BreakAfterStep)
                {
                    isInBreakMode = true;
                    lastContinueInputTime = Time.time;
                    StartCoroutine("BreakTime");
                }
                else
                {
                    bool nextStepExists = scenario.NextStep();
                    if (!nextStepExists)
                    {
                        lastContinueInputTime = Time.time;
                        isInBreakMode = true;
                        StartCoroutine("FinishStudy");
                    }
                }
            }

        }

        private void ResetHandAppearance()
        {
            MaterialManager.Instance.ResetMaterial();
            secondaryHandAppearance.SetColor(MaterialManager.Instance.CurrentSelectedMaterial.color);
            primaryHandAppearance.SetColor(MaterialManager.Instance.CurrentSelectedMaterial.color);
        }

        private void SaveMidStepAndContinue()
        {
            // Export sketch and data
            exportController.ExportSketch(scenario.GetCurrentStep().ToString()); // export formats are set in ExportController properties
            scenario.GetCurrentStep().SaveMidStepAndContinue();
        }

        // Confirm end break
        IEnumerator WaitForConfirmBreakEnd()
        {
            instructionsDisplay.SetText(
            "You are about to start the next task.\n" +
            //"Press " + studyNextKey + " or Thumbstick Button for non-dominant hand" + " to confirm.",
            "Press Thumbstick Button on " + StudyUtils.undominatehand + " controller to confirm.",
            modalMode: false
            );

            // Hide everything from screen
            drawingCanvas.gameObject.SetActive(false);


            while (true)
            {
                if (Time.time > lastContinueInputTime + 0.5f && (Input.GetKeyUp(studyNextKey) || OculusInputManager.NextStudy()))
                {
                    break;
                }
                yield return null;
            }
            // Confirm end break
            isInBreakMode = false;
            drawingCanvas.gameObject.SetActive(true);

            bool nextStepExists = scenario.NextStep();
            if (!nextStepExists)
            {
                lastContinueInputTime = Time.time;
                isInBreakMode = true;
                StartCoroutine("FinishStudy");
            }
        }
    }
}

