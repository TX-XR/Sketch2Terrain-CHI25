using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MappingAI
{
    public class InferenceTool : MonoBehaviour, ITool
    {
        // Start is called before the first frame update
        public bool CanInference = false;
        public Image image;
        public string ToolName { get { return this.GetType().Name; } }
        public float ToolBeginTime { get; set; }
        public List<Tuple<float, float, float>> ToolUsageTime { get; set; }
        public GameObject AIModelContainerParent;

        private AIModelManager aIModelManager;
        private void Start()
        {
            aIModelManager = ComponentManager.Instance.GetAIModelManager();

            ToolUsageTime = new List<Tuple<float, float, float>>();
            Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.ExecuteInferenceHot);
            image.color = Color.blue;
        }

        // Update is called once per frame
        public void onValueChanged()
        {
            CanInference = !CanInference;
            image.color = CanInference ? Color.blue: image.color = Color.white;
            if (CanInference)
            {
                AIModelContainerParent.transform.localPosition = Vector3.zero;
            }
            else
            {
                AIModelContainerParent.transform.localPosition = new Vector3(-0.45f, 0, 0);
            }
            //if (CanInference)
            //{
            //    aIModelManager.ExecuteInferenceAsync(true);
            //    EventManagerVR.TriggerEvent(EventManagerVR.ExecuteInferenceHot);
            //    AIGeneratedModelContainer.enabled = true;
            //}
            //else
            //{
            //    AIGeneratedModelContainer.enabled = false;
            //    EventManagerVR.TriggerEvent(EventManagerVR.ExecuteInferenceCold);
            //}
        }

        public void setToolUsageTime()
        {
            var endTime = TimeManager.instance.getTimerInSec();
            ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
        }
    }

}
