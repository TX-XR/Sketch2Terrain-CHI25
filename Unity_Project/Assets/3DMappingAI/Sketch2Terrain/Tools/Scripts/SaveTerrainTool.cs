using MappingAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTerrainTool : MonoBehaviour, ITool
{
    public float ToolBeginTime { get; set; }
    public string ToolName { get { return this.GetType().Name; } }

    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    private ExportController exportController;
    private StudyScenario scenario;
    private TerrainManagerAsync terrainManager;
    private ComponentManager componentManager;
    [SerializeField]
    private bool IsNextTerrain = false;

    private void Start()
    {
        componentManager = FindAnyObjectByType<ComponentManager>();
        scenario = componentManager.GetStudyScenario();
        exportController = componentManager.GetExportController();
        terrainManager = componentManager.GetTerrainManagerAsync();
    }
    public void onValueChanged()
    {
        exportController.ExportSketch();
        if (IsNextTerrain)
            terrainManager.NextTerrain(scenario.GetCurrentStep().Zoom);
    }

    public void setToolUsageTime()
    {
        ToolUsageTime.Add(Tuple.Create(TimeManager.instance.getTimerInSec(), TimeManager.instance.getTimerInSec(), 0f));
    }
}
