using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Oculus.Interaction.OptionalAttribute;

namespace MappingAI
{
    public class ToolManager : MonoBehaviour
    {
        public List<GameObject> buttons = new List<GameObject>();
        GameObject inferenceButtons;
        GameObject previousInferenceButtons;

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.ObservationTaskHot, (() => { ActiveButtons(false); }));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.ObservationTaskCold, (() =>{ ActiveButtons(true); }));
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.ObservationTaskHot, (() => { ActiveButtons(false); }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.ObservationTaskCold, (() => { ActiveButtons(true); }));
        }

        private void Start()
        {
            if (ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._AI)
            {
                UpdateInferenceButton(false);
            }
        }

        private void UpdateInferenceButton(bool flag)
        {
            //if (!flag)
            //{
            //    if (FindAnyObjectByType<InferenceTool>() == null)
            //    {
            //        inferenceButtons = previousInferenceButtons;
            //    }
            //    else
            //    {
            //        inferenceButtons = FindAnyObjectByType<InferenceTool>().gameObject;
            //    }
            //    previousInferenceButtons = inferenceButtons;
            //    inferenceButtons.SetActive(flag);
            //}
            //else
            //{
            //    previousInferenceButtons.SetActive(flag);
            //}
            
        }

        public void ActiveButtons(bool flag)
        {
            if (buttons.Count > 0)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    GameObject gameObject = buttons[i];
                    gameObject.SetActive(flag);   
                }
                if (ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._AI)
                {
                    UpdateInferenceButton(flag);
                }
            }
        }
    }
}

