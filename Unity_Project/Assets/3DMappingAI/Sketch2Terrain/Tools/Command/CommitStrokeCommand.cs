using Mapbox.Directions;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Mapbox.Map.Tile;



namespace MappingAI
{
    public class CommitStrokeCommand: MonoBehaviour, ICommandWithResult<FinalStroke>
    {
        GameObject finalStrokePrefab;
        GameObject strokeObject;
        DrawingCanvas canvas;
        int finalStrokeID;
        Curve.Curve snappedCurve;
        bool closedLoop;
        Vector3[] samples;
        DrawController drawController;
        IntersectionConstraint[] intersections;
        float snapToExistingNodeThreshold;
        float mergeConstraintsThreshold;
        Material currentMaterial;
        ColorProperty currentColorProperty;
        public FinalStroke Result { get; private set; }

        public CommitStrokeCommand(GameObject finalStrokePrefab, DrawingCanvas canvas, int finalStrokeID, Curve.Curve snappedCurve, bool closedLoop, Vector3[] samples, IntersectionConstraint[] intersections, float snapToExistingNodeThreshold, float mergeConstraintsThreshold, DrawController drawController)
        {
            this.finalStrokePrefab = finalStrokePrefab;
            this.canvas = canvas;
            this.finalStrokeID = finalStrokeID;
            this.snappedCurve = snappedCurve;
            this.closedLoop = closedLoop;
            this.samples = samples;
            this.intersections = intersections;
            this.snapToExistingNodeThreshold = snapToExistingNodeThreshold;
            this.mergeConstraintsThreshold = mergeConstraintsThreshold;
            this.drawController = drawController;
            currentMaterial = MaterialManager.Instance.CurrentSelectedMaterial;
            currentColorProperty = MaterialManager.Instance.GetColorProterty();
        }
        public bool Execute()
        {
            // Create final stroke game object and render the stroke
            strokeObject = canvas.Create(this.finalStrokePrefab, Primitive.Stroke);
            FinalStroke finalStroke = strokeObject.GetComponent<FinalStroke>();
            strokeObject.GetComponent<LineRenderer>().material = currentMaterial;
            finalStroke.SetID(finalStrokeID);
            finalStroke.SetCurve(snappedCurve, closedLoop: closedLoop);
            finalStroke.SaveInputSamples(samples);
            finalStroke.SetColorProperty(currentColorProperty);

            foreach (var intersection in intersections)
            {
                // OLD STROKE: Create or fetch node and create new segments if needed
                //Debug.Log("intersection param = " + intersection.NewCurveData.t.ToString("F6"));
                INode node = intersection.IntersectedStroke.AddIntersectionOldStroke(intersection.OldCurveData, snapToExistingNodeThreshold);

                // NEW STROKE: Insert node and create new segments if needed
                finalStroke.AddIntersectionNewStroke(node, intersection.NewCurveData, mergeConstraintsThreshold);
                Debug.Log("[GRAPH UPDATE] added node with " + node.IncidentCount + " neighbors");
            }
            if (finalStroke.GetColorProperty() == ColorProperty.Terrain)
                this.canvas.AddTerrainStroke(finalStroke);
            else if (finalStroke.GetColorProperty() == ColorProperty.Landmark)
                this.canvas.AddLandmarkStroke(finalStroke);
            this.drawController.RenderStroke(finalStroke);
            this.drawController.SolidifyStroke(finalStroke);
            //finalStroke.GetComponent<MeshRenderer>().enabled = true;

            Result = finalStroke;
            return true;
        }


        public void Redo()
        {
            Execute();
        }

        public void Undo()
        {
            if (strokeObject == null)
            {
                return;
            }
            FinalStroke finalStroke = strokeObject.GetComponent<FinalStroke>();
            currentMaterial = strokeObject.GetComponent<LineRenderer>().material;
            currentColorProperty = finalStroke.GetColorProperty();
            // Get graph update
            if (finalStroke.GetColorProperty() == ColorProperty.Terrain)
            {
                
                this.canvas.RemoveTerrainStroke(finalStroke);

            }
            else if (finalStroke.GetColorProperty() == ColorProperty.Landmark)
                this.canvas.RemoveLandmarkStroke(finalStroke);
            finalStroke.Destroy();
        }
    }
}