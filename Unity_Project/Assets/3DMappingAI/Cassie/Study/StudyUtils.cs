using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using Mapbox.Json;
using UnityEngine.UI;
using Curve;
using MappingAI;

namespace VRSketch
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

    public enum ControllerType
    {
        Vive = 0,
        Oculus = 1,
        Knuckles = 2
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
            //{ InteractionMode.Tutorial, String.Format("Tutorial: take as long as you like to explore different features (eraser, change color, redo, undo). Also, you will practice the Observation and Sketching tasks in the Tutorial.\n\nIn the Observation and Sketching tasks, you need to remember or sketch the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star) in a given time. The terrain and landmarks should be sketched in Blue and Red respectively.  \n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. \n\n After the tutorial, you will face 8 scenes (8 Observation and Sketching tasks). \n Press Thumbstick on {0} controller when you're done.\n" , undominatehand) },
            //{ InteractionMode.Observation, String.Format("Observation Task: \n You have to remember the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star). At the end of the countdown, the terrain will automatically disappear. You cannot stop the task manually. \n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. ")},
            //{ InteractionMode.Normal, String.Format("Sketching Task: \n You have to sketch the structure of the terrain and landmarks (e.g., circle, rect, cone, ring, and star). The terrain and landmarks should be sketched in Blue and Red respectively.\n\n Important: the height and location are very important for landmarks. Please try your best to represent the spatial relations in your sketch. \n\n When you're done, press Thumbstick on {0} controller to the next task.\n", undominatehand)},
            //{ InteractionMode.FreeCreation, "Free creation: sketch a random terrain." +
            //                                "\n press the Next Terrain Button on the table to change the terrain." +
            //                                "\n when you're done, press the Save Button on the table.\n" },

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

        public static bool TryLoadDataCollectionData(out StudySequenceData ssd)
        {
            ssd = new StudySequenceData();
            try
            {
                string filename = Path.Combine(Application.persistentDataPath, "data_collection.json");
                //string filename = Path.Combine(Application.persistentDataPath, "data_collection.json");
                string jsonStr = File.ReadAllText(filename);
                ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo("en-US")
                });
            }
            catch (Exception ex)
            {
                
                Debug.Log(ex.Message);
                return false;
            }
            //try
            //{
            //    string filename = Path.Combine(Application.streamingAssetsPath, "data_collection.json");
            //    //string filename = Path.Combine(Application.persistentDataPath, "data_collection.json");
            //    string jsonStr = File.ReadAllText(filename);
            //    DebugUIBuilder.instance.AddLabel(jsonStr);
            //    ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
            //    {
            //        Culture = new System.Globalization.CultureInfo("en-US")
            //    });
            //}
            //catch (Exception ex)
            //{
            //    string filename = Path.Combine(Application.persistentDataPath, "data_collection.json");
            //    string jsonStr = File.ReadAllText(filename);
            //    DebugUIBuilder.instance.AddLabel(jsonStr);
            //    ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
            //    {
            //        Culture = new System.Globalization.CultureInfo("en-US")
            //    });
            //    label = DebugUIBuilder.instance.AddLabel("read persistentDataPath" + filename);
            //    label.GetComponent<Text>().fontSize = 30;
            //    DebugUIBuilder.instance.Show();
            //    //ssd.SystemSequence = new List<int> { 1 };
            //    //ssd.InteractionModeSequence = new List<int> { 2 };
            //    //ssd.TimeLimit = new List<float> { 0 };
            //    //ssd.TerrainCoordsX = new List<float> { 0 };
            //    //ssd.TerrainCoordsY = new List<float> { 0 };
            //    //ssd.BreakTime = new List<bool> { false };
            //}
            return true;
        }

        public static bool TryLoadStudyData(out StudySequenceData ssd)
        {
            ssd = new StudySequenceData();
            try
            {
                string filename = Path.Combine(Application.persistentDataPath, "study_sequence.json");
                string jsonStr = File.ReadAllText(filename);
                ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo("en-US")
                });
            }
            catch (Exception ex)
            {
                //ssd.InteractionModeSequence = new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 2 };
                //ssd.SystemSequence = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                //ssd.BreakTime = new List<bool> { true, false, false, true, false, false, true, false, false, true };
                //ssd.TimeLimit = new List<float> { 0, 360, 360, 360, 360, 360, 360, 360, 360, 0 };
                //ssd.TerrainCoordsX = new List<float> { 46.389454f, 45.389454f, 44.389454f, 43.389454f, 42.389454f, 41.389454f, 40.389454f, 39.389454f, 38.389454f, 0 };
                //ssd.TerrainCoordsY = new List<float> { 10.396694f, 9.396694f, 8.396694f, 7.396694f, 6.396694f, 5.396694f, 4.396694f, 3.396694f, 2.396694f, 0 };

                return false;
                //return false;
            }
            //try
            //{
            //    string filename = Path.Combine(Application.streamingAssetsPath, "study_sequence.json");
            //    string jsonStr = File.ReadAllText(filename);
            //    ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
            //    {
            //        Culture = new System.Globalization.CultureInfo("en-US")
            //    });
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log(ex.Message);
            //    string filename = Path.Combine(Application.persistentDataPath, "study_sequence.json");
            //    string jsonStr = File.ReadAllText(filename);
            //    DebugUIBuilder.instance.AddLabel(jsonStr);
            //    ssd = JsonConvert.DeserializeObject<StudySequenceData>(jsonStr, new JsonSerializerSettings
            //    {
            //        Culture = new System.Globalization.CultureInfo("en-US")
            //    });

            //    //ssd.InteractionModeSequence = new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 2 };
            //    //ssd.SystemSequence = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            //    //ssd.BreakTime = new List<bool> { true, false, false, true, false, false, true, false, false, true };
            //    //ssd.TimeLimit = new List<float> { 0, 360, 360, 360, 360, 360, 360, 360, 360, 0 };
            //    //ssd.TerrainCoordsX = new List<float> { 46.389454f, 45.389454f, 44.389454f, 43.389454f, 42.389454f, 41.389454f, 40.389454f, 39.389454f, 38.389454f, 0 };
            //    //ssd.TerrainCoordsY = new List<float> { 10.396694f, 9.396694f, 8.396694f, 7.396694f, 6.396694f, 5.396694f, 4.396694f, 3.396694f, 2.396694f, 0 };

            //    return true;
            //    //return false;
            //}

            return true;
        }

        //public static bool IsRightHandedConfig()
        //{

        //    try
        //    {
        //        var filename = Path.Combine(Application.streamingAssetsPath, "dominant_hand.txt");
        //        string data = File.ReadAllText(filename);

        //        int handInt = 0;
        //        if (!Int32.TryParse(data, out handInt))
        //            //throw new Exception("Cannot open hand config file!");

        //        if (handInt == 0)
        //            return true;
        //        else
        //            return false;

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogError(ex.Message);
        //    }

        //    return true;
        //}

        public static ControllerType GetControllerType()
        {
            //try
            //{
            //    var filename = Path.Combine(Application.streamingAssetsPath, "controller_type.txt");
            //    string data = File.ReadAllText(filename);

            //    if (!Int32.TryParse(data, out int controllerInt))
            //        throw new Exception("Cannot open controller type config file!");

            //    return (ControllerType)controllerInt;

            //}
            //catch (Exception ex)
            //{
            //    Debug.LogError(ex.Message);
            //}

            return ControllerType.Oculus;
        }

    }
}