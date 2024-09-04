using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

namespace MappingAI
{
    public class TextTool : MonoBehaviour, ITool
    {
        // Start is called before the first frame update
        public DrawController drawController;
        public bool createText = false;
        public Image image;
        public string ToolName { get { return this.GetType().Name; } }
        public float ToolBeginTime { get; set; }
        public List<Tuple<float, float, float>> ToolUsageTime { get; set; }
        private InputController inputController;
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
            createText = !createText;
            image.color = createText ? Color.white : image.color = Color.white;
            if (createText)
                inputController.ChangeHandAppearance(Color.white);
            drawController.SetCreateTextFlag(createText);
        }

        public void checkState()
        {
            if (createText) { onValueChanged(); }
        }

        public void setToolUsageTime()
        {
            var endTime = TimeManager.instance.getTimerInSec();
            ToolUsageTime.Add(Tuple.Create(ToolBeginTime, endTime, endTime - ToolBeginTime));
        }
    }

}
