using System;
using System.Linq;
using UnityEngine;
using static Mapbox.Map.Tile;

namespace MappingAI
{
    /// <summary>The surface calibration procedure: 3ViSuAl</summary>
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
                //ApplicationSettings.Instance.GetDeskPivot().CalibratePositionAndRotation(centerPosition, centerRotation);
                //ApplicationSettings.Instance.GetDeskPivot().CalibrateXYZ(centerPosition, centerRotation, width / 1.8f);
                //ApplicationSettings.Instance.GetDeskPivot().Calibrate(centerPosition, centerRotation, width / 1.8f, width / 1.8f);
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SurfaceCalibrationCompleted);
                // set the surface and deskpovit's position after calibrationStepsCompleteds, casue the table model is 1.8 x 0.8
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
            // 如果不在校准过程中，则返回 (Return if not in calibration process)
            if (calibrationStepsCompleted == 0) return;

            // 完成校准步骤1：设置第二个校准点
            if (calibrationStepsCompleted == 1)
            {
                //_calibrationPoints[1] = new Vector3(transform.position.x, _calibrationPoints[0].y, _calibrationPoints[0].z);
                _calibrationPoints[1] = new Vector3(stylusTransform.position.x, _calibrationPoints[0].y, stylusTransform.position.z);
                //_calibrationPoints[2] = ApplicationSettings.Instance.GetStylus().position + Vector3.up * 0.002f;
                firstToSecondCorner = _calibrationPoints[1] - _calibrationPoints[0];
                Vector3 horizontalDirection = Vector3.Cross(firstToSecondCorner, Vector3.up).normalized;
                width = Vector3.Distance(firstCorner, secondCorner);
                height = defaultAspectRatio * width;
                _calibrationPoints[2] = secondCorner + horizontalDirection * height;
            }

            // 设置第一个和第二个角 (Set the first and the second corner)
            firstCorner = _calibrationPoints[0];
            secondCorner = _calibrationPoints[1];

            // 计算方向 (Compute the direction)
            firstToSecondCorner = secondCorner - firstCorner;

            // 计算正交方向 (Compute the orthogonal direction)
            Vector3 secondToLastSamplePoint = _calibrationPoints[2] - firstCorner;
            float t = Vector3.Dot(secondToLastSamplePoint, firstToSecondCorner.normalized);
            Vector3 thirdSampleProjectedToLineFromFirstToSecondCorner =
                firstCorner + firstToSecondCorner.normalized * t;

            // 将第三次校准的位置投影到水平线上以获取高度 (Project third calibration's position onto horizontal line to get height)
            secondToThirdCorner = _calibrationPoints[2] - thirdSampleProjectedToLineFromFirstToSecondCorner;

            // 现在，获取矩形尺寸 (Now, get rectangle dimensions)
            width = Vector3.Distance(firstCorner, secondCorner);
            height = secondToThirdCorner.magnitude;

            // 矩形中心是（第一和第二角之间的一半）+ 0.5 *（第一到第二水平线） (Rectangle center is (halfway between first and second corner) + 0.5 * (first to second horizontal line))
            centerPosition = (firstCorner + secondCorner) / 2 + .5f * secondToThirdCorner;

            // 计算目标旋转 (Calculate target rotation)
            var targetForward = secondCorner - firstCorner;

            // 如果第二个角在第一个角的右边，反转目标前向矢量 (Invert target forward vector if second corner is to the right of the first corner)
            var firstPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(firstCorner);
            var secondPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(secondCorner);

            // 如果需要，反转前向方向 (Invert forward direction if required)
            leftToRightCalibration = secondPointInCameraSpace.x > firstPointInCameraSpace.x;
            if (leftToRightCalibration)
                targetForward *= -1;

            // 从3个输入位置制作平面 (Make plane from 3 input positions)
            Plane interactionPlane = new Plane(secondCorner, firstCorner, _calibrationPoints[2]);
            Vector3 rectangleNormal = interactionPlane.normal;

            // 如果法线远离相机，反转法线 (Invert normal if it faces away from camera)
            float dotProduct = Vector3.Dot(rectangleNormal, XRInputManager.Instance.head.position - firstCorner);
            rectangleNormal *= Mathf.Sign(dotProduct);

            if (rectangleNormal == Vector3.zero || targetForward == Vector3.zero)
                return;

            // 计算目标前向矢量 (Compute target forward vector)
            targetForward = Vector3.Cross(rectangleNormal, targetForward);

            // 计算中心旋转 (Compute the center rotation)
            centerRotation = Quaternion.LookRotation(targetForward, rectangleNormal);

            // 计算第三和第四角 (Compute the third and fourth corner)
            thirdCorner = secondCorner + secondToThirdCorner;
            fourthCorner = firstCorner + secondToThirdCorner;

            // 确定垂直校准方向 (Determine vertical calibration direction)
            topToBottomCalibration = firstCorner.z > _calibrationPoints[2].z;
        }

        private void CalibrationWithThreePoints()
        {
            // 如果不在校准过程中，则返回 (Return if not in calibration process)
            if (calibrationStepsCompleted == 0) return;

            // 完成校准步骤1：设置第一个和第二个校准点 (Finished Calibration Step 1: Set the first and second calibration point)
            if (calibrationStepsCompleted == 1)
            {
                _calibrationPoints[1] = stylusTransform.position;
                _calibrationPoints[2] = stylusTransform.position - Vector3.up * 0.002f;
            }

            // 完成校准步骤2：设置第三个校准点 (Finished Calibration Step 2: Set the third calibration point)
            if (calibrationStepsCompleted == 2)
            {
                _calibrationPoints[2] = stylusTransform.position;
            }

            // 设置第一个和第二个角 (Set the first and the second corner)
            firstCorner = _calibrationPoints[0];
            secondCorner = _calibrationPoints[1];

            // 计算方向 (Compute the direction)
            firstToSecondCorner = secondCorner - firstCorner;

            // 计算正交方向 (Compute the orthogonal direction)
            Vector3 secondToLastSamplePoint = _calibrationPoints[2] - firstCorner;
            float t = Vector3.Dot(secondToLastSamplePoint, firstToSecondCorner.normalized);
            Vector3 thirdSampleProjectedToLineFromFirstToSecondCorner =
                firstCorner + firstToSecondCorner.normalized * t;

            // 将第三次校准的位置投影到水平线上以获取高度 (Project third calibration's position onto horizontal line to get height)
            secondToThirdCorner = _calibrationPoints[2] - thirdSampleProjectedToLineFromFirstToSecondCorner;

            // 现在，获取矩形尺寸 (Now, get rectangle dimensions)
            width = Vector3.Distance(firstCorner, secondCorner);
            height = secondToThirdCorner.magnitude;

            // 矩形中心是（第一和第二角之间的一半）+ 0.5 *（第一到第二水平线） (Rectangle center is (halfway between first and second corner) + 0.5 * (first to second horizontal line))
            centerPosition = (firstCorner + secondCorner) / 2 + .5f * secondToThirdCorner;

            // 计算目标旋转 (Calculate target rotation)
            var targetForward = firstCorner - secondCorner;

            // 如果第二个角在第一个角的右边，反转目标前向矢量 (Invert target forward vector if second corner is to the right of the first corner)
            var firstPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(firstCorner);
            var secondPointInCameraSpace = XRInputManager.Instance.head.InverseTransformPoint(secondCorner);

            // 如果需要，反转前向方向 (Invert forward direction if required)
            leftToRightCalibration = secondPointInCameraSpace.x > firstPointInCameraSpace.x;
            if (leftToRightCalibration)
                targetForward *= -1;

            // 从3个输入位置制作平面 (Make plane from 3 input positions)
            Plane interactionPlane = new Plane(firstCorner, secondCorner, _calibrationPoints[2]);
            Vector3 rectangleNormal = interactionPlane.normal;

            // 如果法线远离相机，反转法线 (Invert normal if it faces away from camera)
            float dotProduct = Vector3.Dot(rectangleNormal, XRInputManager.Instance.head.position - firstCorner);
            rectangleNormal *= Mathf.Sign(dotProduct);

            if (rectangleNormal == Vector3.zero || targetForward == Vector3.zero)
                return;

            // 计算目标前向矢量 (Compute target forward vector)
            targetForward = Vector3.Cross(rectangleNormal, targetForward);

            // 计算中心旋转 (Compute the center rotation)
            centerRotation = Quaternion.LookRotation(targetForward, rectangleNormal);

            // 计算第三和第四角 (Compute the third and fourth corner)
            thirdCorner = secondCorner + secondToThirdCorner;
            fourthCorner = firstCorner + secondToThirdCorner;

            // 确定垂直校准方向 (Determine vertical calibration direction)
            topToBottomCalibration = firstCorner.y > _calibrationPoints[2].y;
        }
    }
}