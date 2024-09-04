using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MappingAI
{


    // Stores data for a given study step
    [System.Serializable]
    public class StudyStep
    {
        public SketchSystem System { get; }
        public SketchModel Model { get; }
        public InteractionMode Mode { get; } // Tutorial or normal
        public bool BreakAfterStep { get; }
        public float TimeLimitForObservation { get; }
        public float TimeLimit { get; }
        public float Zoom { get; }
        public string TerrainName { get; }
        public Vector2 terrainCoord { get; }
        public int[] terrainSequence { get; }

        //private SessionData sessionData;
        private List<SystemState> systemStates;
        private List<SerializableStroke> sketchedStrokes;
        private List<SerializablePatch> createdPatches;

        public StudyStep(SketchSystem system, InteractionMode interactionMode, bool breakTime, float timeLimit, Vector2 terrainCoord, float zoom, string terrainName, int[] terrainSequence)
        {
            System = system;
            Mode = interactionMode;
            systemStates = new List<SystemState>();
            sketchedStrokes = new List<SerializableStroke>();
            createdPatches = new List<SerializablePatch>();
            BreakAfterStep = breakTime;
            this.TimeLimit = timeLimit;
            this.terrainCoord = terrainCoord;
            this.Zoom = zoom;
            this.TerrainName = terrainName;
            this.TimeLimitForObservation = 120;
            this.terrainSequence = terrainSequence;
        }

        public override string ToString()
        {
            string timestamp = (DateTime.Now).ToString("yyyyMMddHHmmss");
            return "Study_" + timestamp + "_" + (int)Mode + "_" + (int)System;
        }


        public void Idle(Transform headTransform, Vector3 primaryHandPos, Transform canvasTransform, bool mirroring)
        {
            SystemState idleState = new SystemState(
                InteractionType.Idle,
                -1,
                mirroring,
                headTransform.position,
                headTransform.rotation,
                primaryHandPos,
                canvasTransform.position,
                canvasTransform.rotation,
                canvasTransform.localScale.x
                );

            systemStates.Add(idleState);
        }

        public void CanvasTransform(Transform headTransform, Vector3 primaryHandPos, Transform canvasTransform, bool mirroring)
        {
            SystemState transformState = new SystemState(
                InteractionType.CanvasTransform,
                -1,
                mirroring,
                headTransform.position,
                headTransform.rotation,
                primaryHandPos,
                canvasTransform.position,
                canvasTransform.rotation,
                canvasTransform.localScale.x
                );

            systemStates.Add(transformState);
        }

        public void StrokeAdd(Transform headTransform, Vector3 primaryHandPos, Transform canvasTransform, SerializableStroke stroke, bool mirroring)
        {
            SystemState strokeAddState = new SystemState(
                InteractionType.StrokeAdd,
                stroke.id,
                mirroring,
                headTransform.position,
                headTransform.rotation,
                primaryHandPos,
                canvasTransform.position,
                canvasTransform.rotation,
                canvasTransform.localScale.x
                );

            sketchedStrokes.Add(stroke);

            systemStates.Add(strokeAddState);

        }

        public void SurfaceAdd(Transform headTransform, Vector3 primaryHandPos, Transform canvasTransform, SerializablePatch patch, bool mirroring)
        {
            SystemState strokeAddState = new SystemState(
                InteractionType.SurfaceAdd,
                patch.id,
                mirroring,
                headTransform.position,
                headTransform.rotation,
                primaryHandPos,
                canvasTransform.position,
                canvasTransform.rotation,
                canvasTransform.localScale.x
                );

            createdPatches.Add(patch);

            systemStates.Add(strokeAddState);

        }

        public void Delete(Transform headTransform, Vector3 primaryHandPos, Transform canvasTransform, InteractionType type, int id, bool mirroring)
        {
            SystemState strokeDelState = new SystemState(
                type,
                id,
                mirroring,
                headTransform.position,
                headTransform.rotation,
                primaryHandPos,
                canvasTransform.position,
                canvasTransform.rotation,
                canvasTransform.localScale.x
                );

            systemStates.Add(strokeDelState);
        }

        // Log data
        public void Finish(string TerrainPath = "")
        {
            // Store all data
            SessionData sessionData = new SessionData(System, Mode, systemStates, sketchedStrokes, createdPatches);
            Debug.Log("[STUDY DATA] saved " + systemStates.Count + " states, " + sketchedStrokes.Count + " strokes, " + createdPatches.Count + " patches.");
            StudyLog.SaveData(sessionData, ToString(), TerrainPath);
        }

        public void SaveMidStepAndContinue()
        {
            // Store all data
            SessionData sessionData = new SessionData(System, Mode, systemStates, sketchedStrokes, createdPatches);
            Debug.Log("[STUDY DATA] saved " + systemStates.Count + " states, " + sketchedStrokes.Count + " strokes, " + createdPatches.Count + " patches.");
            StudyLog.SaveData(sessionData, ToString());

            // Reinitialize states, strokes and patches lists
            systemStates = new List<SystemState>();
            sketchedStrokes = new List<SerializableStroke>();
            createdPatches = new List<SerializablePatch>();
        }
    }
}