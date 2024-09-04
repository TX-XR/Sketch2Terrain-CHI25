using UnityEngine;

namespace MappingAI
{
    /// <summary>Visual Feedback by a rectangle</summary>
    public class VisualFeedbackRectangle : MonoBehaviour
    {
        [SerializeField] private SurfaceCalibration surfaceCalibration;
        private GameObject _rectangle;
        private float _rectangleSize = 0.002f;

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => { _rectangle.GetComponent<MeshRenderer>().enabled = false; });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => { _rectangle.GetComponent<MeshRenderer>().enabled = false; });
        }
        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => { _rectangle.GetComponent<MeshRenderer>().enabled = false; });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => { _rectangle.GetComponent<MeshRenderer>().enabled = false; });
        }

        private void Start()
        {
            //_rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _rectangle.GetComponent<MeshRenderer>().enabled = false;
            _rectangle.GetComponent<MeshRenderer>().material.color = Color.white;
            _rectangle.transform.position = Vector3.zero;
            _rectangle.transform.rotation = Quaternion.identity;
            _rectangle.transform.localScale = Vector3.one * 0.01f;

        }

        void Update()
        {
            if (surfaceCalibration.calibrationStepsCompleted == 0) return;
            UpdateRectangle();
            
        }

        private void UpdateVisibility()
        {
            if (surfaceCalibration.calibrationStepsCompleted == 1)
            {
                _rectangle.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        private void UpdateRectangle()
        {
            _rectangle.GetComponent<MeshRenderer>().enabled = false;
            //_rectangle.transform.position = surfaceCalibration.centerPosition;
            // show VisualPrefab with height
            _rectangle.transform.position = surfaceCalibration.centerPosition;
            //_rectangle.transform.localScale = Vector3.one * _rectangleSize;
            _rectangle.transform.localPosition = new Vector3(_rectangle.transform.localPosition.x,
                _rectangle.transform.localPosition.y - 0.5f * _rectangleSize,
                _rectangle.transform.localPosition.z + 0.5f * _rectangleSize);
            _rectangle.transform.localScale = new Vector3(Mathf.Max(surfaceCalibration.width, _rectangleSize),
    _rectangle.transform.localScale.y, Mathf.Max(surfaceCalibration.height, _rectangleSize));
            _rectangle.transform.rotation = surfaceCalibration.centerRotation;
            UpdateVisibility();
            //if (surfaceCalibration.calibrationStepsCompleted == 1)
            //{
            //    _rectangle.transform.localScale = new Vector3(surfaceCalibration.width,
            //        _rectangle.transform.localScale.y, _rectangle.transform.localScale.z);
            //}

        }
    }
}