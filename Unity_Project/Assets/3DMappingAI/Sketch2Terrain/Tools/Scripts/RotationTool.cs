using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace MappingAI
{
    public class RotationTool : MonoBehaviour, ITool
    {
        public Color colorIsOn = Color.white;
        public Color colorIsOff = Color.grey;
        public float angle = 90f;
        Vector3 center;
        public string ToolName { get { return this.GetType().Name; } }
        public float ToolBeginTime { get; set; }
        public List<Tuple<float, float, float>> ToolUsageTime { get; set; }

        private bool rotationState = false;


        private Image image;
        private void Start()
        {
            ToolUsageTime = new List<Tuple<float, float, float>>();
            rotationState = false;
            image = this.GetComponentInChildren<Image>();
        }
        public LineRenderer UpdateLineRendererHeight3D(LineRenderer lineRenderer, Vector3 center)
        {
            Vector3[] allPoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(allPoints);
            Vector3[] newPositions = allPoints.Select(p =>
            {
                Vector3 rotatedPoint = RotatePointAroundYAxis(p, center, angle);
                return new Vector3(rotatedPoint.x, rotatedPoint.y, rotatedPoint.z);
            }).ToArray();
            lineRenderer.SetPositions(newPositions);
            return lineRenderer;
        }

        public static Vector3 RotatePointAroundYAxis(Vector3 point, Vector3 center, float angle)
        {
            // Translate the point so that the center becomes the origin
            Vector3 translatedPoint = point - center;

            // Rotate the point around the Y axis by the desired angle
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rotatedPoint = rotation * translatedPoint;

            // Translate the point back to its original position
            Vector3 finalPoint = rotatedPoint + center;

            // Return the final rotated point
            return finalPoint;
        }

        public Vector3 GetCenterOfGravity(LineRenderer[] lineRenderers)
        {
            Vector3 centerOfGravity = Vector3.zero;
            int totalPoints = 0;

            // Loop through each line renderer
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                // Get the positions of all the points in the line
                Vector3[] positions = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(positions);

                // Loop through each point in the line and add it to the center of gravity
                foreach (Vector3 position in positions)
                {
                    centerOfGravity += position;
                    totalPoints++;
                }
            }

            // Divide the center of gravity by the total number of points to get the average position
            centerOfGravity /= totalPoints;

            return centerOfGravity;
        }

        public void onValueChanged()
        {
            //RenderTextureManager.ResetRenderTexture();
            setToolUsageTime();
            GameObject[] lineObjects = GameObject.FindObjectsOfType<GameObject>()
        .Where(go => go.GetComponent<LineRenderer>() != null)
        .ToArray();
            LineRenderer[] lineRenderers = GameObject.FindObjectsOfType<LineRenderer>();
            center = GetCenterOfGravity(lineRenderers);

            foreach (GameObject lineObject in lineObjects)
            {
                // Get the center of the game object
                // Get the LineRenderer component attached to the game object
                LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
                // Rotate the game object based on the center point
                lineObject.transform.RotateAround(center, Vector3.up, angle);
                UpdateLineRendererHeight3D(lineRenderer, center);
            }
        }


        public void onValueChangedFor3DMappingAI()
        {
            //RenderTextureManager.ResetRenderTexture();
            setToolUsageTime();
            rotationState = !rotationState;
            
            if (rotationState)
            {
                image.color = colorIsOn;
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.RotationToolHot);
            }
            else
            {
                image.color = colorIsOff;
                setToolUsageTime();
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.RotationToolCold);
            }
        }

        public void setToolUsageTime()
        {
            ToolUsageTime.Add(Tuple.Create(TimeManager.instance.getTimerInSec(), TimeManager.instance.getTimerInSec(), 0f));
        }
    }
}