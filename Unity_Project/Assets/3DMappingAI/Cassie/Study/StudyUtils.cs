using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MappingAI
{
    [Serializable]
    public struct StudySequenceData
    {
        public List<int> SystemSequence;
        public List<int> InteractionModeSequence;
        public List<bool> BreakTime;
        public List<float> TimeLimit;
        public List<string> TerrainName;
        public List<float> TerrainCoordsX;
        public List<float> TerrainCoordsY;
        public List<float> Zoom;
        public int[] TerrainSequence;

    }

    public enum SketchSystem
    {
        Baseline = 0,
        Snap = 1,
        SnapSurface = 2
    }

    public enum SketchModel
    {
        Mouse = 0,
        Lamp = 1,
        Plane = 2,
        Shoe = 3,
    }

    public enum InteractionMode
    {
        Tutorial = 0,
        Observation = 1,
        Normal = 2,
        FreeCreation = 3,
    }

    public enum InteractionType
    {
        Idle,
        StrokeAdd,
        StrokeDelete,
        SurfaceAdd,
        SurfaceDelete,
        CanvasTransform
    }


    public static class StudyUtils
    {
        public static string undominatehand = OculusInputManager.GetUndominatehand() == OVRInput.Controller.LTouch? "left":"right";

        public static Dictionary<SketchModel, Plane> MirrorModelMapping = new Dictionary<SketchModel, Plane>
        {
            { SketchModel.Mouse, new Plane(Vector3.right, new Vector3(-0.125f, 0.125f, 0.125f)) },
            { SketchModel.Plane, new Plane(Vector3.forward, new Vector3(0.125f, 0.125f, 0f)) },
            { SketchModel.Lamp, new Plane(Vector3.forward, new Vector3(0.125f, 0.125f, 0f)) },
            { SketchModel.Shoe, new Plane(Vector3.right, new Vector3(0f, 0.125f, 0.125f)) },
        };

        public static Dictionary<InteractionMode, string> InteractionModeInstructions = new Dictionary<InteractionMode, string>
        {
            { InteractionMode.Tutorial, String.Format("Tutorial: take as long as you like to explore different features (eraser, change color, text). Also, you will practice the Observation and Sketching tasks in the Tutorial.\n\nIn the Observation and Sketching tasks, you need to remember or sketch the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star) in a given time. The terrain and landmarks should be sketched in Blue and Red respectively.  \n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. \n\n After the tutorial, you will face 8 scenes (8 Observation and Sketching tasks). \n Inform the experimenter when you're done.\n" , undominatehand) },
            { InteractionMode.Observation, String.Format("Observation Task: \n You have to remember the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star). At the end of the countdown, the terrain will automatically disappear. You cannot stop the task manually. \n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. ")},
            { InteractionMode.Normal, String.Format("Sketching Task: \n You have to sketch the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star). The terrain and landmarks should be sketched in Blue and Red respectively.\n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. \n\n When you're done, inform the experimenter to the next task.\n", undominatehand)},
            { InteractionMode.FreeCreation, "Free creation: sketch a random terrain." +
                                            "\n press the Next Terrain Button on the table to change the terrain." +
                                            "\n when you're done, press the Save Button on the table.\n" },
        };

        public static Dictionary<SketchSystem, string> SystemInstructions = new Dictionary<SketchSystem, string>
        {
            { SketchSystem.Baseline, "Freehand" },
            { SketchSystem.Snap, "Snap" },
            { SketchSystem.SnapSurface, "Patch" }
        };

        public static Dictionary<SketchModel, string> ModelNames = new Dictionary<SketchModel, string>
        {
            { SketchModel.Mouse, "Computer mouse" },
            { SketchModel.Lamp, "Desk lamp" },
            { SketchModel.Plane, "Airplane" },
            { SketchModel.Shoe, "Running shoe" },
        };
    }
}