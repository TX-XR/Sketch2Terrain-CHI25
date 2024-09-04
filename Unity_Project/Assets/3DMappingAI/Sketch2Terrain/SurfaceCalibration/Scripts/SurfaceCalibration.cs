using System;
using System.Linq;
using UnityEngine;

/*
    This code was adapted from https://gitlab2.informatik.uni-wuerzburg.de/hci-development/otss-off-the-shelf-stylus and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */
namespace MappingAI
{
    public class SurfaceCalibration : MonoBehaviour
    {
        public static float defaultAspectRatio = 0.4444f;
        public static float defaultWidthOfGlass = 0.72f;
        public static float defaultHeightOfGlass = 0.4f;
        public CalibrationManagement calibrationManagement;
        private Vector3[] _calibrationPoints = new Vector3[3];

        [Header("Calibrated Values")][Tooltip("Count of the completed calibration steps")]
        public int calibrationStepsCompleted =0;

        [Header("Center Point of the rectangle")]
        public Vector3 centerPosition;
        //public Vector3 centerPositionOverall;

        public Quaternion centerRotation;

        [Header("Dimensions of the rectangle")]
        public float width;

        public float height;
        public const float defaultWidth = 1.8f;

        [Header("Corners of the rectangle")] public Vector3 firstCorner;
        public Vector3 secondCorner;
        public Vector3 thirdCorner;
        public Vector3 fourthCorner;

        [Header("Directions of the rectangle")]
        public Vector3 firstToSecondCorner;

        public Vector3 secondToThirdCorner;

        [Header("Properties of the rectangle")]
        public bool leftToRightCalibration;

        public bool topToBottomCalibration;
        public ParametersManager parameters = null;
        Transform stylusTransform;
        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.AddCalibrationPoint,
                () => AddCalibrationPoint(ApplicationSettings.Instance.GetStylus().position));
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.AddCalibrationPoint,
                () => AddCalibrationPoint(ApplicationSettings.Instance.GetStylus().position));
        }

        private void Awake()
        {
            if (parameters == null)
            {
                var p = FindAnyObjectByType<ParametersManager>();
                if (p != null)
                    parameters = p;
                else
                    return;
            }
        }
        private void Update()
        {
            //HorizontalCalibrationWithTwoPoints();
            stylusTransform = ApplicationSettings.Instance.GetStylus();
            CalibrationWithTwoPoints();
        }

        /// <summary>Adds a calibration point</summary>
        private void AddCalibrationPoint(Vector3 position)
        {
            // Update position of current sample
            _calibrationPoints[calibrationStepsCompleted] = position;
            UpdateCornerY(position.y);

            // Increase number for next calibration point
            calibrationStepsCompleted++;

            // Reset render points and calibrate the surface
            if (calibrationStepsCompleted >= 2)
            {
                calibrationStepsCompleted = 0;

                var uuids = AnchorUuidStore.Uuids.ToArray();
                if (uuids.Length > 0)
                {
                    AnchorUuidStore.Clear();
                }
                if (calibrationManagement == null)
                {
                    calibrationManagement=FindAnyObjectByType<CalibrationManagement>();
                }

                float ratio = Math.Clamp(width / defaultWidth, 0.6f, 1f);

                // make the bottom plane lower than the stroke
                centerPosition.y = centerPosition.y - 2 * parameters.Current.StrokeWidth;
                calibrationManagement.calibrate(centerPosition, centerRotation, 1);
                PlayerPrefs.SetFloat(AnchorUuidStore.RatioPref, 1);
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SurfaceCalibrationCompleted);
            }
            else
            {
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SurfaceCalibrationInProgress);
            }
        }

        // setup the height of the vritual surface
        private void UpdateCornerY(float y)
        {
            firstCorner.y = y;
            secondCorner.y = y;
            thirdCorner.y = y;
            fourthCorner.y = y;
        }

        private void CalibrationWithTwoPoints()
        {
            // Return if not in calibration process
            if (calibrationStepsCompleted == 0) return;

            // Completing Calibration Step 1: Setting the Second Calibration Point
            if (calibrationStepsCompleted == 1)
            {
                _calibrationPoints[1] = new Vector3(stylusTransform.position.x, _calibrationPoints[0].y, stylusTransform.position.z);
                firstToSecondCorner = _calibrationPoints[1] - _calibrationPoints[0];
                Vector3 horizontalDirection = Vector3.Cross(firstToSecondCorner, Vector3.up).normalized;
                width = Vector3.Distance(firstCorner, secondCorner);
                height = defaultAspectRatio * width;
                _calibrationPoints[2] = secondCorner + horizontalDirection * height;
            }

            // Set the first and the second corner
            firstCorner = _calibrationPoints[0];
            secondCorner = _calibrationPoints[1];

            // Compute the direction
            firstToSecondCorner = secondCorner - firstCorner;

            // Compute the orthogonal direction
            Vector3 secondToLastSamplePoint = _calibrationPoints[2] - firstCorner;
            float t = Vector3.Dot(secondToLastSamplePoint, firstToSecondCorner.normalized);
            Vector3 thirdSampleProjectedToLineFromFirstToSecondCorner =
                firstCorner + firstToSecondCorner.normalized * t;

            // Project third calibration's position onto horizontal line to get height
            secondToThirdCorner = _calibrationPoints[2] - thirdSampleProjectedToLineFromFirstToSecondCorner;

            // Now, get rectangle dimensions
            width = Vector3.Distance(firstCorner, secondCorner);
            height = secondToThirdCorner.magnitude;

            // Rectangle center is (halfway between first and second corner) + 0.5 * (first to second horizontal line)
            centerPosition = (firstCorner + secondCorner) / 2 + .5f * secondToThirdCorner;

            // Calculate target rotation
            var targetForward = secondCorner - firstCorner;

            // Invert target forward vector if second corner is to the right of the first corner
            var firstPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(firstCorner);
            var secondPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(secondCorner);

            // Invert forward direction if required
            leftToRightCalibration = secondPointInCameraSpace.x > firstPointInCameraSpace.x;
            if (leftToRightCalibration)
                targetForward *= -1;

            // Make plane from 3 input positions
            UnityEngine.Plane interactionPlane = new UnityEngine.Plane(secondCorner, firstCorner, _calibrationPoints[2]);
            Vector3 rectangleNormal = interactionPlane.normal;

            // Invert normal if it faces away from camera
            float dotProduct = Vector3.Dot(rectangleNormal, XRInputManager.Instance.head.position - firstCorner);
            rectangleNormal *= Mathf.Sign(dotProduct);

            if (rectangleNormal == Vector3.zero || targetForward == Vector3.zero)
                return;

            // Compute target forward vector
            targetForward = Vector3.Cross(rectangleNormal, targetForward);

            // Compute the center rotation
            centerRotation = Quaternion.LookRotation(targetForward, rectangleNormal);

            // Compute the third and fourth corner
            thirdCorner = secondCorner + secondToThirdCorner;
            fourthCorner = firstCorner + secondToThirdCorner;

            // Determine vertical calibration direction
            topToBottomCalibration = firstCorner.z > _calibrationPoints[2].z;
        }
    }
}