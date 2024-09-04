using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MappingAI
{
    public class PassthroughManager : MonoBehaviour
    {
        public OVRPassthroughLayer passthroughLayer;
        OVRCameraRig ovrCameraRig;
        Camera centerCamera;
        // Start is called before the first frame update
        void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationReset, () => HidePassthroughLayerState(false));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => HidePassthroughLayerState(true));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => HidePassthroughLayerState(true));
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationReset, () => HidePassthroughLayerState(false));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => HidePassthroughLayerState(true));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => HidePassthroughLayerState(true));
        }

        private void Start()
        {
            passthroughLayer.hidden = false;
            // Set camera background to transparent
            ovrCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
            centerCamera = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
            centerCamera.clearFlags = CameraClearFlags.SolidColor;
            centerCamera.backgroundColor = Color.clear;
        }

        private void HidePassthroughLayerState(bool state)
        {
            passthroughLayer.hidden = state;
            if (state)
            {
                passthroughLayer.hidden = false;
                //centerCamera.backgroundColor = Color.gray;
                centerCamera.backgroundColor = new Color(0, 0, 0, 0.9f);
            }
            else {
                centerCamera.backgroundColor = Color.clear; 
            }
        }
    }
}
