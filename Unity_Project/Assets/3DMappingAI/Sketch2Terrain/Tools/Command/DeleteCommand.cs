using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.TextCore.Text;

namespace MappingAI
{

    public class DeleteCommand : MonoBehaviour, ICommandWithResult<FinalStroke>
    {
        FinalStroke finalStroke;
        TextStroke textStroke;
        _2DFinalStroke _2DFinalStroke;
        DrawingCanvas canvas;
        GameObject strokeObject;
        DrawController drawController;
        private int finalStrokeID;
        private bool closedLoop;
        private Vector3[] samples;
        private List<Sample> textSamples;
        private Curve.Curve snappedCurve;
        GameObject StrokePrefab;
        public FinalStroke Result { get; private set; }
        float scale;
        Primitive strokeType;
        ColorProperty strokeProperty;
        public DeleteCommand(DrawingCanvas canvas, GameObject StrokeGameObject, GameObject finalStrokePrefab, GameObject _2DFinalStrokePrefab, GameObject textStrokePrefab, float scale)
        {
            this.canvas = canvas;
            this.strokeObject = StrokeGameObject;
            drawController = FindAnyObjectByType<DrawController>();
            // Store all the import properties from final stroke
            this.scale = scale;
            if (this.strokeObject.TryGetComponent<FinalStroke>(out this.finalStroke))
            {
                this.StrokePrefab = finalStrokePrefab;
                strokeType = Primitive.Stroke;
                finalStrokeID = this.finalStroke.ID;
                snappedCurve = this.finalStroke.Curve;
                closedLoop = this.finalStroke._closedLoop;
                samples = this.finalStroke.inputSamples;
                strokeProperty = this.finalStroke.GetColorProperty();
            } 
            else if (this.strokeObject.TryGetComponent<TextStroke>(out this.textStroke))
            {
                this.StrokePrefab = textStrokePrefab;
                strokeType = Primitive.Text;
                snappedCurve = this.textStroke.Curve;
                closedLoop = this.textStroke._closedLoop;
                textSamples = this.textStroke.Samples;
                strokeProperty = this.textStroke.GetColorProperty();
            }
            else if (this.strokeObject.TryGetComponent<_2DFinalStroke>(out this._2DFinalStroke))
            {
                this.StrokePrefab = _2DFinalStrokePrefab;
                strokeType = Primitive._2DStroke;
                finalStrokeID = this._2DFinalStroke.ID;
                snappedCurve = this._2DFinalStroke.Curve;
                closedLoop = this._2DFinalStroke._closedLoop;
                samples = this._2DFinalStroke.inputSamples;
                strokeProperty = this.finalStroke.GetColorProperty();
            }
        }

        public void Redo()
        {
            Execute();
        }

        public void Undo()
        {
            this.strokeObject = canvas.Create(this.StrokePrefab, strokeType);
            if (strokeType == Primitive.Stroke)
            {
                this.strokeObject.GetComponent<LineRenderer>().material = MaterialManager.Instance.GetMaterialByIndex((int)strokeProperty);
                this.finalStroke = this.strokeObject.GetComponent<FinalStroke>();
                updateFinalStrokeAttributes();
            }
            else if (strokeType == Primitive.Text)
            {
                this.textStroke = this.strokeObject.GetComponent<TextStroke>();
                this.strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.ColorOnText;
                updateTextStrokeAttributes();
            }
            else if (strokeType == Primitive._2DStroke)
            {
                this.strokeObject.GetComponent<LineRenderer>().material = MaterialManager.Instance.GetMaterialByIndex((int)strokeProperty);
                this._2DFinalStroke = this.strokeObject.GetComponent<_2DFinalStroke>();
                update2DFinaltrokeAttributes();
            }
        }

        private void updateTextStrokeAttributes()
        {
            this.textStroke.SetCurve(snappedCurve, closedLoop);
            this.textStroke.SaveInputSamples(textSamples);
            this.textStroke.SetColorProperty(strokeProperty);
            this.canvas.TextStrokes.Add(this.textStroke);
            this.drawController.RenderStroke(textStroke, this.scale);
            this.drawController.SolidifyStroke(textStroke, this.scale);
        }

        private void update2DFinaltrokeAttributes()
        {
            this._2DFinalStroke.SetID(finalStrokeID);
            this._2DFinalStroke.SetCurve(snappedCurve, closedLoop);
            this._2DFinalStroke.SaveInputSamples(samples);
            this._2DFinalStroke.SetColorProperty(strokeProperty);
            this.canvas._2DStrokes.Add(this._2DFinalStroke);
            this.drawController.RenderStroke(_2DFinalStroke);
            this.drawController.SolidifyStroke(_2DFinalStroke);
        }

        private void updateFinalStrokeAttributes()
        {
            this.finalStroke.SetID(finalStrokeID);
            this.finalStroke.SetCurve(snappedCurve, closedLoop: closedLoop);
            this.finalStroke.SaveInputSamples(samples);
            this.finalStroke.SetColorProperty(strokeProperty);
            this.canvas.TerrainStrokes.Add(this.finalStroke);
            this.drawController.RenderStroke(finalStroke);
            this.drawController.SolidifyStroke(finalStroke);
        }

        public bool Execute()
        {
            if (strokeType == Primitive.Stroke)
            {
                canvas.TerrainStrokes.Remove(this.finalStroke);
                this.finalStroke.Destroy();
                // Get graph update 
                canvas.GraphUpdate();
            }
            else if (strokeType == Primitive.Text)
            {
                canvas.TextStrokes.Remove(this.textStroke);
                this.textStroke.Destroy();
            }
            else if (strokeType == Primitive._2DStroke)
            {
                canvas._2DStrokes.Remove(this._2DFinalStroke);
                this._2DFinalStroke.Destroy();
            }

            return true;
        }
    }
}