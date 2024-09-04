using MappingAI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class ColorChangeTool : MonoBehaviour, ITool
{
    // Start is called before the first frame update
    public string ToolName { get { return this.GetType().Name; } }
    public float ToolBeginTime { get; set; }
    public List<Tuple<float, float, float>> ToolUsageTime { get; set; }
    private InputController inputController;

    public int ColorIndex = 0;
    public Image image;
    private TextTool textTool;

    private void Awake()
    {
        ToolUsageTime = new List<Tuple<float, float, float>>();
        inputController = FindAnyObjectByType<InputController>();
    }
    void Start()
    {
        image.color = Color.white;
    }
    // Update is called once per frame
    public void onValueChanged()
    {
        MaterialManager.Instance.ChangeMaterialByIndex(ColorIndex);
        inputController.ChangeHandAppearance();
        image.color = Color.white;

        if (textTool == null)
        {
            textTool = GameObject.FindAnyObjectByType<TextTool>();
        }
        if (textTool != null)
        {
            textTool.checkState();
        }
    }

    public void setToolUsageTime()
    {
        var endTime = TimeManager.instance.getTimerInSec();
        ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
    }
}
