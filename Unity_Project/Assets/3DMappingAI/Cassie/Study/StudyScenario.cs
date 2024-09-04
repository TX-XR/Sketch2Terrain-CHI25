using MappingAI;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using VRSketch;


public class StudyScenario : MonoBehaviour
{
    private StudyStep CurrentStep { get; set; }

    private static UnityEvent setStudyStepEvent;

    //public static SetStudyStepEvent setStudyStep;

    private StudySequenceData sequenceData;
    private int stepID;
    private ParametersManager parameters = null;

    private List<int> InteractionModeSequence;
    private List<int> SystemSequence;
    private List<bool> BreakTime;
    private List<float> TimeLimit;
    private List<float> TerrainCoordsX;
    private List<float> TerrainCoordsY;
    private List<string> TerrainName;
    private List<float> Zoom;

    private ComponentManager componentManager;
    private void Start()
    {
        setStudyStepEvent = new UnityEvent();
        componentManager = FindAnyObjectByType<ComponentManager>();
        parameters = componentManager.GetCASSIEParametersProvider();
        sequenceData = new StudySequenceData();
        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.DataCollection)
            {
            sequenceData.SystemSequence = new List<int> { 1 };
            sequenceData.InteractionModeSequence = new List<int> { 3 };
            sequenceData.TimeLimit = new List<float> { 0 };
            sequenceData.TerrainCoordsX = new List<float> { 0 };
            sequenceData.TerrainCoordsY = new List<float> { 0 };
            sequenceData.BreakTime = new List<bool> { false };
            sequenceData.Zoom = new List<float> { 14 };
            sequenceData.TerrainName = new List<string> { "Random" };
        }
        else
        {
            if (ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._AI)
            {
                InteractionModeSequence = new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 3};
                SystemSequence = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                BreakTime = new List<bool> { false, false, false, false, false, false, false, false, true, false };
                TimeLimit = new List<float> { 0, 300, 300, 300, 300, 300, 300, 300, 300, 0};
                TerrainCoordsX = new List<float> { 47.12f, 27.91f, 60.53f, -8.22f, 35.362f, 35.129f, 46.43f, 46.63f, 38.231f, 0f };
                TerrainCoordsY = new List<float> { 9.29f, 98.4f, 7.000f, 117.98f, 138.73f, 78.66f, 7.96f, 7.91f, -109.82f, 0f };
                TerrainName = new List<string> { "Tutorial", "Hills: Three Parallel Rivers, China", "Fjord: Sognefjord, Norway", "Hole: Tambora Volcano, Indonesia", "Mountain: Mount Fuji, Japan", "Ridge: ", "Glacier: Aletsch, Switzerland", "Village: Lauterbrunnen, Switzerland", "Canyon: Canyonlands, USA" , "Free Creation"};
                Zoom = new List<float> { 14, 14, 12, 14, 13, 15, 14, 14, 15, 13 };
            }
            else
            {
                InteractionModeSequence = new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1 };
                SystemSequence = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                BreakTime = new List<bool> { false, false, false, false, true, false, false, false, false };
                TimeLimit = new List<float> { 0, 300, 300, 300, 300, 300, 300, 300, 300 };
                TerrainCoordsX = new List<float> { 47.12f, 27.91f, 60.53f, -8.22f, 35.362f, 35.129f, 46.43f, 46.63f, 38.231f };
                TerrainCoordsY = new List<float> { 9.29f, 98.4f, 7.000f, 117.98f, 138.73f, 78.66f, 7.96f, 7.91f, -109.82f };
                TerrainName = new List<string> { "Tutorial", "Hills: Three Parallel Rivers, China", "Fjord: Sognefjord, Norway", "Hole: Tambora Volcano, Indonesia", "Mountain: Mount Fuji, Japan", "Ridge: ", "Glacier: Aletsch, Switzerland", "Village: Lauterbrunnen, Switzerland", "Canyon: Canyonlands, USA" };
                Zoom = new List<float> { 14, 14, 12, 14, 13, 15, 14, 14, 15 };
            }

            int[] TerrainSequence;
            int.TryParse(ApplicationSettings.Instance.participantID, out int result);
            ReorderLists(result, out TerrainSequence);

            sequenceData = new StudySequenceData
            {
                InteractionModeSequence = InteractionModeSequence,
                SystemSequence = SystemSequence,
                BreakTime = BreakTime,
                TimeLimit = TimeLimit,
                TerrainCoordsX = TerrainCoordsX,
                TerrainCoordsY = TerrainCoordsY,
                TerrainName = TerrainName,
                Zoom = Zoom,
                TerrainSequence = TerrainSequence
            };
        }

        stepID = 0;
        RedoStep();
    }

    public static UnityEvent GetStudyStepEvent()
    {
        return setStudyStepEvent;
    }

    public StudyStep GetCurrentStep()
    {
        return CurrentStep;
    }

    public void ReorderLists(int inputNumber, out int[] TerrainSequence)
    {
        // Define the 8 orders
        int[][] orders = new int[][]
           {
            new int[] {1, 2, 8, 3, 7, 4, 6, 5 }, // Order 1: ABHCGDFE
            new int[] {2, 3, 1, 4, 8, 5, 7, 6 }, // Order 2: BCADHEGF
            new int[] {3, 4, 2, 5, 1, 6, 8, 7 }, // Order 3: CDBEAFHG
            new int[] {4, 5, 3, 6, 2, 7, 1, 8 }, // Order 4: DECFBGAH
            new int[] {5, 6, 4, 7, 3, 8, 2, 1 }, // Order 5: EFDGCHBA
            new int[] {6, 7, 5, 8, 4, 1, 3, 2 }, // Order 6: FGEHDACB
            new int[] {7, 8, 6, 1, 5, 2, 4, 3 }, // Order 7: GHFAEBDC
            new int[] {8, 1, 7, 2, 6, 3, 5, 4 }, // Order 8: HAGBFCED
           };

        // Get the remainder to determine the order
        int remainder = inputNumber % 8;


        TerrainSequence = orders[remainder];
        // Reorder the lists based on the selected order
        // ReorderList(InteractionModeSequence, TerrainSequence);
        // ReorderList(SystemSequence, TerrainSequence);
        // ReorderList(BreakTime, TerrainSequence);
        ReorderList(TimeLimit, TerrainSequence);
        ReorderList(TerrainCoordsX, TerrainSequence);
        ReorderList(TerrainCoordsY, TerrainSequence);
        ReorderList(TerrainName, TerrainSequence);
        ReorderList(Zoom, TerrainSequence);
    }

    private void ReorderList<T>(List<T> list, int[] order)
    {
        List<T> tempList = new List<T>(list);
        for (int i = 0; i < order.Length; i++)
        {
            list[i + 1] = tempList[order[i]];
        }
    }
    public void EndStep(string TerrainPath = "")
    {
        CurrentStep.Finish(TerrainPath);
    }

    public void RedoStep()
    {
        SketchSystem system = (SketchSystem)sequenceData.SystemSequence[stepID];
        InteractionMode mode = (InteractionMode)sequenceData.InteractionModeSequence[stepID];
        bool breakTime = sequenceData.BreakTime[stepID];
        float timeLimit = sequenceData.TimeLimit[stepID];
        Vector2 terrainCoord = new Vector2(sequenceData.TerrainCoordsX[stepID], sequenceData.TerrainCoordsY[stepID]);
        float zoom = sequenceData.Zoom[stepID];
        string terrainName = sequenceData.TerrainName[stepID];
        int[] terrainSequence = sequenceData.TerrainSequence;
        CurrentStep = new StudyStep(system, mode, breakTime, timeLimit, terrainCoord, zoom, terrainName, terrainSequence);
        // Scene settings
        setStudyStepEvent.Invoke();
    }

    public bool NextStep()
    {
        stepID++;
        if (stepID < sequenceData.SystemSequence.Count)
        {
            RedoStep();
            return true;
        }
        else
            return false;
    }
    
}
