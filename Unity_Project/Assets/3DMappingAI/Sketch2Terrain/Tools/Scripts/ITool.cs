using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITool
{

    public float ToolBeginTime { get; set; }
    //public List<GameObject> gameObjects { get; set; }
    public string ToolName { get { return this.GetType().Name; } }
    /// <summary>
    /// The time when user begin using tools
    /// The time when user end using tools
    /// The duration time of using tools
    /// </summary>
    List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    public void onValueChanged();
    public void setToolUsageTime();

    //public void ActiveButtons(bool flag)
    //{
    //    if (gameObjects.Count > 0)
    //    {
    //        for (int i = 0; i < gameObjects.Count; i++)
    //        {
    //            GameObject gameObject = gameObjects[i];
    //            gameObject.SetActive(flag);
    //        }
    //    }
    //}

}
public interface IToolButton
{

    float ToolBeginTime { get; set; }
    string ToolName { get; }
    /// <summary>
    /// The time when user begin using tools
    /// The time when user end using tools
    /// The duration time of using tools
    /// </summary>
    List<Tuple<float, float, float>> ToolUsageTime { get; set; }

    public void onBtnSelected();
    public void onBtnUnselected();
    public void setToolUsageTime();



}