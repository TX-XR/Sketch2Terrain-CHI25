using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MappingAI
{
    /// <summary>Event Manager</summary>
    public class Sketch2TerrainEventManager : MonoBehaviour
    {
        public static string AddCalibrationPoint = "AddCalibrationPoint";
        public static string SurfaceCalibrationInProgress = "SurfaceCalibrationInProgress";
        public static string SurfaceCalibrationCompleted = "SurfaceCalibrationCompleted";
        public static string SurfaceCalibrationReset = "SurfaceCalibrationReset";
        public static string SketchStart = "SketchStart";
        public static string SketchRangeIn = "SketchRangeIn";
        public static string SketchRangeOut = "SketchRangeOut";

        public static string PassthroughStart = "PassthroughStart";
        public static string TryLoadSpatialAnchor = "TryLoadSpatialAnchor";
        public static string SpatialAnchorloaded = "SpatialAnchorloaded";

        public static string ObservationTaskHot = "ObservationTaskHot";
        public static string ObservationTaskCold = "ObservationTaskCold";

        public static string Undo = "Undo";
        public static string Redo = "Redo";

        public static string EraserToolHot = "EraserToolHot";
        public static string EraserToolCold = "EraserToolCold";

        public static string RotationToolHot = "RotationToolHot";
        public static string RotationToolCold = "RotationToolCold";

        public static string ExecuteInferenceHot = "ExecuteInferenceHot";
        public static string ExecuteInferenceCold = "ExecuteInferenceCold";
        /// <summary>Available events</summary>
        private Dictionary<string, UnityEvent> _eventDictionary = new Dictionary<string, UnityEvent>();

        /// <summary>Event Manager Instance Internal</summary>
        private static Sketch2TerrainEventManager _instance;

        /// <summary>Event Manager Instance</summary>
        public static Sketch2TerrainEventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Sketch2TerrainEventManager>();
                    _instance.Init();
                }
                return _instance;
            }
        }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        /// <summary>Initialize Event Manager</summary>
        void Init()
        {
            if (_eventDictionary == null)
            {
                _eventDictionary = new Dictionary<string, UnityEvent>();
            }
        }


        /// <summary>Adds a new event</summary>
        public static void StartListening(string eventName, UnityAction listener)
        {
            if (Instance._eventDictionary == null)
            {
                Instance.Init();
            }

            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                Instance._eventDictionary.Add(eventName, thisEvent);
            }
        }

        /// <summary>Removes an event</summary>
        public static void StopListening(string eventName, UnityAction listener)
        {
            if (_instance == null) return;
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        /// <summary>Trigger an event</summary>
        public static void TriggerEvent(string eventName)
        {
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}