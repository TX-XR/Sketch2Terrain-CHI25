using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MappingAI;

public class EndTool : MonoBehaviour, ITool
{
    public float ToolBeginTime { get; set; }

    public string ToolName { get { return this.GetType().Name; } }

    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    private void Awake()
    {
        ToolUsageTime = new List<Tuple<float, float, float>>();
    }
    public void onValueChanged()
    {
        OculusInputManager.SetVibration();
        ToolBeginTime = TimeManager.instance.getTimerInSec();
        setToolUsageTime();
        EndScene();
    }
    public void EndScene()
    {
        //var objExportTool =  FindFirstObjectByType<ObjExportTool>();
        //objExportTool.OnQuit();
        Application.Quit();
    }
    public void setToolUsageTime()
    {
        var endTime = TimeManager.instance.getTimerInSec();

        ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
    }
}
