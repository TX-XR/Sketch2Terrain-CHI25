using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MappingAI
{
    public class EraserTool : MonoBehaviour, ITool
    {
        private Color colorIsOn=Color.blue;
        private Color colorIsOff = Color.grey;
        public bool isOn = false;
        private Image image;
        public List<GameObject> gameObjects = new List<GameObject>();
        public string ToolName { get { return this.GetType().Name;} }
        public float ToolBeginTime { get; set; }
        public List<Tuple<float, float, float>> ToolUsageTime { get; set; }
        private AIModelManager aIModelManager;
        bool State = false;



        private void Start()
        {
            image = this.GetComponentInChildren<Image>();
            image.color = colorIsOff;
            ToolUsageTime = new List<Tuple<float, float, float>>();
            aIModelManager = ComponentManager.Instance.GetAIModelManager();
        }

        public void onValueChanged()
        {
            isOn = !isOn;
            if (isOn)
            {
                State = true;
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.EraserToolHot);
                image.color = colorIsOn;
                ToolBeginTime = TimeManager.instance.getTimerInSec();
                ActiveButtons(false);
                aIModelManager.CanAIModelRender(false);
            }
            else
            {
                State = false;
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.EraserToolCold);
                image.color = colorIsOff;
                setToolUsageTime();
                ActiveButtons(true);
                aIModelManager.CanAIModelRender(true);
                aIModelManager.ExecuteInferenceAsync();
            }
        }

        public bool GetEraserState()
        {
            return State;
        }
        public void setToolUsageTime()
        {
            var endTime = TimeManager.instance.getTimerInSec();
            ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
        }

        public void ActiveButtons(bool flag)
        {
            if (gameObjects.Count > 0)
            {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GameObject gameObject = gameObjects[i];
                    gameObject.SetActive(flag);
                }
            }
        }
    }
}
