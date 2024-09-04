using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRSketch;

namespace MappingAI
{
    public class Commit2DStrokeCommand : MonoBehaviour, ICommandWithResult<_2DFinalStroke>
    {
        GameObject finalStrokePrefab;
        GameObject strokeObject;
        DrawingCanvas canvas;
        DrawController drawController;
        Material currentMaterial;
        public _2DFinalStroke Result { get; private set; }
        public InputStroke inputStroke { get; private set; }
        float height;
        int ID;
        public Commit2DStrokeCommand(GameObject _2DFinalStrokePrefab, DrawingCanvas canvas, InputStroke inputStroke, int ID,DrawController drawController, float height)
        {
            this.finalStrokePrefab = _2DFinalStrokePrefab;
            this.canvas = canvas;
            this.drawController = drawController;
            this.inputStroke = inputStroke;
            this.height = height;
            this.ID = ID;
            currentMaterial = MaterialManager.Instance.CurrentSelectedMaterial;
        }
        public bool Execute()
        {
            // Create final stroke game object and render the stroke
            bool FitToConstraints = false, mirror = false;
            (Curve.Curve snappedCurve,
            IntersectionConstraint[] intersections,
            MirrorPlaneConstraint[] mirrorIntersections) =
            drawController.beautifier.Beautify(
            this.inputStroke,
            FitToConstraints,
            mirror,
            out List<SerializableConstraint> appliedConstraints,
            out List<SerializableConstraint> rejectedConstraints,
            out bool planar,
            out bool onSurface,
            out bool onMirror,
            out bool closedLoop);

            strokeObject = canvas.Create(this.finalStrokePrefab, Primitive._2DStroke);
            _2DFinalStroke Stroke = strokeObject.GetComponent<_2DFinalStroke>();
            strokeObject.GetComponent<LineRenderer>().material = currentMaterial;
            Stroke.SetColorProperty(MaterialManager.Instance.GetColorProterty());
            Stroke.SetID(this.ID);
            Stroke.SetCurve(snappedCurve, closedLoop: closedLoop);
            Stroke.SaveInputSamples(this.inputStroke.GetPoints().ToArray());
            this.canvas.Add2D(Stroke);
            this.drawController.RenderStrokeBySamples(Stroke);
            this.drawController.SolidifyStroke(Stroke);
            
            //finalStroke.GetComponent<MeshRenderer>().enabled = true;
            Result = Stroke;
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
            _2DFinalStroke f = strokeObject.GetComponent<_2DFinalStroke>();
            // Get graph update
            canvas.Remove2D(f);
            f.Destroy();
        }
    }
}