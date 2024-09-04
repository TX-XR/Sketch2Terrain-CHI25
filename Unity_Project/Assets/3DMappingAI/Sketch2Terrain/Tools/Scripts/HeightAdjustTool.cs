using MappingAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class HeightAdjustTool : MonoBehaviour, ITool
{
    // Start is called before the first frame update
    public string ToolName { get { return this.GetType().Name; } }
    public float ToolBeginTime { get; set; }
    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    public bool IsIncrease = false;
    public float HeightDifference = 0.05f;
    DrawingCanvas canvas { get; set; }
    UIManager UIManager { get; set; }
    Vector3 startPosition;

    private void OnEnable()
    {
        Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchStart, () => { startPosition = canvas.transform.position; });
    }

    private void OnDisable()
    {
        Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchStart, () => { startPosition = canvas.transform.position; });
    }

    private void Awake()
    {
        ToolUsageTime = new List<Tuple<float, float, float>>();
        canvas = FindAnyObjectByType<DrawingCanvas>();
        UIManager = FindAnyObjectByType<UIManager>();
    }

    // Update is called once per frame
    public void setToolUsageTime()
    {
        var endTime = TimeManager.instance.getTimerInSec();
        ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
    }

    public void onValueChanged()
    {
        float heightDifference = IsIncrease? this.HeightDifference: -this.HeightDifference;
        //Vector3 projectedPositionDifference = Vector3.ProjectOnPlane(positionDifference, Vector3.up);

        // Update object's position on the x-z plane
        Vector3 newPosition = canvas.transform.position;
        newPosition.y += heightDifference; // only adjust y
        if (newPosition.y <= startPosition.y)
        {
            canvas.transform.position = startPosition;
            UIManager.getCurrentUI().transform.position = new Vector3(UIManager.getCurrentUI().transform.position.x, startPosition.y, UIManager.getCurrentUI().transform.position.z) ;
        }
        else
        {
            canvas.transform.position = newPosition;
            UIManager.getCurrentUI().transform.position = new Vector3(UIManager.getCurrentUI().transform.position.x, newPosition.y, UIManager.getCurrentUI().transform.position.z);
        }
            
    }
}
