using MappingAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace MappingAI
{
    public class CommitTextCommand : MonoBehaviour, ICommandWithResult<TextStroke>
    {
        GameObject textStrokePrefab;
        GameObject strokeObject;
        DrawingCanvas canvas;
        Curve.Curve snappedCurve;
        bool closedLoop;
        List<Sample> samples;
        DrawController drawController;
        public TextStroke Result { get; private set; }
        float textScale = 0.3f;

        public CommitTextCommand(GameObject textStrokePrefab, DrawingCanvas canvas, Curve.Curve snappedCurve, bool closedLoop, List<Sample> samples, DrawController drawController, float textScale)
        {
            this.textStrokePrefab = textStrokePrefab;
            this.canvas = canvas;
            this.snappedCurve = snappedCurve;
            this.closedLoop = closedLoop;
            this.samples = samples;
            this.drawController = drawController;
            this.textScale = textScale;


        }
        public bool Execute()
        {
            // Create final stroke game object and render the stroke
            strokeObject = canvas.Create(this.textStrokePrefab, Primitive.Text);
            TextStroke textStroke = strokeObject.GetComponent<TextStroke>();
            strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.ColorOnText;
            textStroke.SetCurve(snappedCurve, closedLoop);
            textStroke.SaveInputSamples(samples);
            textStroke.SetColorProperty(ColorProperty.Other);
            this.canvas.AddTextStroke(textStroke);
            this.drawController.RenderStroke(textStroke, textScale);
            this.drawController.SolidifyStroke(textStroke, textScale);
            //finalStroke.GetComponent<MeshRenderer>().enabled = true;
            Result = textStroke;
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
            TextStroke f = strokeObject.GetComponent<TextStroke>();
            // Get graph update
            canvas.RemoveTextStroke(f);
            f.Destroy();
        }
    }
}