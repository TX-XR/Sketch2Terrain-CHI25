using MappingAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TerrainChangeManagerAsync : MonoBehaviour, ITool
{
    public float ToolBeginTime { get; set; }

    public string ToolName { get { return this.GetType().Name; } }

    private TerrainManagerAsync terrainManagerAsync;
    private StudyScenario scenario;
    private EraseController eraseController;
    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    public bool isNext = false;
    private Image image;
    private DrawingCanvas canvas;
    private AIModelManager aIModelManager;
    static System.Random random = new System.Random();

    private void Start()
    {
        //terrainManager = FindAnyObjectByType<TerrainManager>();
        aIModelManager = ComponentManager.Instance.GetAIModelManager();
        terrainManagerAsync = ComponentManager.Instance.GetTerrainManagerAsync();
        scenario = ComponentManager.Instance.GetStudyScenario();
        canvas = ComponentManager.Instance.GetDrawingCanvas();
        eraseController = ComponentManager.Instance.GetEraseController();
        ToolUsageTime = new List<Tuple<float, float, float>>();
        image = GetComponentInChildren<Image>();
        if (image != null & !isNext)
        {
            image.color = Color.gray;
        }
    }
    private void Update()
    {

        if (terrainManagerAsync.GetIndex() > 0 && image.color == Color.gray)
        {
            image.color = Color.white;
        }
        else if (terrainManagerAsync.GetIndex() == 0 && image.color == Color.white && !isNext)
        {
            image.color = Color.gray;
        }
    }

    public void onValueChanged()
    {
        if (isNext)
        {
            canvas.ResetPosition_Rotation_Scale();
            //terrainManagerAsync.NextTerrain(terrainManagerAsync.defaultZoom);
            int randomZoom = random.Next(-1, 2);
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.DataCollection)
            {
                terrainManagerAsync.NextTerrain(scenario.GetCurrentStep().Zoom + randomZoom);
            }

            else
                terrainManagerAsync.NextTerrain(scenario.GetCurrentStep().Zoom + randomZoom);
            eraseController.Clear();
            aIModelManager.ResetAIModelMesh();
        }
        else
        {
            if (terrainManagerAsync.GetIndex() >= 1)
            {
                canvas.ResetPosition_Rotation_Scale();
                terrainManagerAsync.PrevioustTerrain();
                eraseController.Clear();
                aIModelManager.ResetAIModelMesh();

            }    
        }
    }

    public void setToolUsageTime()
    {
        var endTime = TimeManager.instance.getTimerInSec();

        ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
    }
}
