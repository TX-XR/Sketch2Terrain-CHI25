using Meta.WitAi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace MappingAI
{
    /// <summary>
    /// Demonstrates loading existing spatial anchors from storage.
    /// </summary>
    /// <remarks>
    /// Loading existing anchors involves two asynchronous methods:
    /// 1. Call <see cref="OVRSpatialAnchor.LoadUnboundAnchorsAsync"/>
    /// 2. For each unbound anchor you wish to localize, invoke <see cref="OVRSpatialAnchor.UnboundAnchor.Localize"/>.
    /// 3. Once localized, your callback will receive an <see cref="OVRSpatialAnchor.UnboundAnchor"/>. Instantiate an
    /// <see cref="OVRSpatialAnchor"/> component and bind it to the `UnboundAnchor` by calling
    /// <see cref="OVRSpatialAnchor.UnboundAnchor.BindTo"/>.
    /// </remarks>
    public class SpatialAnchorLoader : MonoBehaviour
    {
        //[SerializeField]
        //public OVRSpatialAnchor _anchorPrefab;
        //[SerializeField] private OVRSpatialAnchor _anchorPrefab;
        public CalibrationManagement calibrationManagement;

        Action<bool, OVRSpatialAnchor.UnboundAnchor> _onAnchorLocalized;

        readonly List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.TryLoadSpatialAnchor,
                () =>
                {
                    if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
                    {
                        LoadAnchorsByUuid();
                    }
                }
                );
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.TryLoadSpatialAnchor,
                () =>
                {
                    if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
                    {
                        LoadAnchorsByUuid();
                    }
                }
                );
        }


        private void Awake()
        {
            _onAnchorLocalized = OnLocalized;
        }

        public void LoadAnchorsByUuid()
        {
            var uuids = AnchorUuidStore.Uuids.ToArray();
            if (uuids.Length == 0)
            {
                return;
            }
            OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, _unboundAnchors)
                .ContinueWith(result =>
                {
                    if (result.Success)
                    {
                        ProcessUnboundAnchors(result.Value);
                    }
                });
        }
      

        private void ProcessUnboundAnchors(IReadOnlyList<OVRSpatialAnchor.UnboundAnchor> unboundAnchors)
        {
            foreach (var anchor in unboundAnchors)
            {
                if (anchor.Localized)
                {
                    _onAnchorLocalized(true, anchor);
                }
                else if (!anchor.Localizing)
                {
                    anchor.LocalizeAsync().ContinueWith(_onAnchorLocalized, anchor);
                }
            }
        }

        private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
        {
            if (!success)
            {
                return;
            }

            var pose = unboundAnchor.Pose;
            //CalibrateUI(pose);
            OVRSpatialAnchor spatialAnchor = CalibrateUI(pose);
            unboundAnchor.BindTo(spatialAnchor);

        }

        private OVRSpatialAnchor CalibrateUI(Pose pose)
        {
            calibrationManagement.transform.position = pose.position;
            calibrationManagement.transform.rotation = pose.rotation;
            calibrationManagement.calibrate(pose.position, pose.rotation, PlayerPrefs.GetFloat(AnchorUuidStore.RatioPref));
            OVRSpatialAnchor spatialAnchor;
            if (!calibrationManagement.gameObject.TryGetComponent<OVRSpatialAnchor>(out spatialAnchor))
            {
                spatialAnchor = calibrationManagement.gameObject.AddComponent<OVRSpatialAnchor>();
            }
            Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SpatialAnchorloaded);
            return spatialAnchor;
        }
    }
}