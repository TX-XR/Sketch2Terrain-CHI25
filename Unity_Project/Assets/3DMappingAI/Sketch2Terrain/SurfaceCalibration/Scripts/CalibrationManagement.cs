
using System.Linq;
using UnityEngine;

namespace MappingAI
{
    public class CalibrationManagement : MonoBehaviour
    {
        private GameObject UI;
        [SerializeField] private GameObject SketchSpace;
        [SerializeField] private GameObject AnchorPref;
        private GameObject Anchor;
        public bool isSurfaceCalibrationCompleted = false;
        public InstructionsDisplay instructionsDisplay;
        public InputController inputController;
        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationReset, surfaceCalibrationReset);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SpatialAnchorloaded, spatialAnchorLoaded);
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationReset, surfaceCalibrationReset);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SpatialAnchorloaded, spatialAnchorLoaded);
        }
        private void Start()
        {
            UI = ComponentManager.Instance.GetUIManager().getCurrentUI();
            deActivateUI();
        }

        private void surfaceCalibrationCompleted()
        {
            Anchor = Instantiate(AnchorPref, transform.position, transform.rotation, transform);
            activateUI();

        }

        private void spatialAnchorLoaded()
        {
            // if SpatialAnchorLoaded, directly go to the sketch mode
            activateUI();
        }

        private void activateUI()
        {
            isSurfaceCalibrationCompleted = true;
            MeshRenderManager.EnableChildMeshRenderers(SketchSpace.transform);
            MeshRenderManager.EnableChildLineRenderers(SketchSpace.transform);
            UI.SetActive(true);
            // change the mode type
            if (ApplicationSettings.Instance.ModeType == ModeType.SurfaceCalibration)
            {
                ApplicationSettings.Instance.ModeType = ModeType.Sketch;
                inputController.UpdateInstructions();
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SketchStart);
            }
        }

        private void deActivateUI()
        {
            isSurfaceCalibrationCompleted = false;
            MeshRenderManager.DisableChildMeshRenderers(SketchSpace.transform);
            MeshRenderManager.DisableChildLineRenderers(SketchSpace.transform);
            UI.SetActive(false);
        }

        private void surfaceCalibrationReset()
        {
            if (Anchor != null)
                Destroy(Anchor);

            var uuids = AnchorUuidStore.Uuids.ToArray();
            if (uuids.Length > 0)
            {
                AnchorUuidStore.Clear();
            }
            if (gameObject.TryGetComponent<OVRSpatialAnchor>(out var component))
            {
                Destroy(component);
            }
            deActivateUI();

        }
        public void calibrate(Vector3 centerPosition, Quaternion centerRotation, float ratio)
        {
            transform.position = centerPosition;
            transform.rotation = centerRotation;
            transform.localScale = new Vector3(ratio, ratio, ratio);
        }
    }
}

