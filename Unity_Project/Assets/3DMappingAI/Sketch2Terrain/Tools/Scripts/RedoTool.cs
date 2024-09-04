using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MappingAI;
using System;
using UnityEngine.UI;
public class RedoTool : MonoBehaviour, ITool
{
    // Start is called before the first frame update
    public string ToolName { get { return this.GetType().Name; } }
    public float ToolBeginTime { get; set; }
    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }
    public Image image;
    private void Awake()
    {
        ToolUsageTime = new List<Tuple<float, float, float>>();
        image.color = Color.gray;
    }

    // Update is called once per frame
    public void setToolUsageTime()
    {
        var endTime = TimeManager.instance.getTimerInSec();
        ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
    }

    public void onValueChanged()
    {
        Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.Redo);
    }
}
