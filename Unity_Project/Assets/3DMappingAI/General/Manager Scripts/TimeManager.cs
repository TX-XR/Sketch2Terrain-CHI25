using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MappingAI
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager instance;
        public float Timer;
        private float TimerInSecond;
        private float TimerForSketch;
        public List<Tuple< float,float,float>> SkrokeCreationTime;
        public float SkrokeBeginTime;
        public float HeightAdjustBeginTime;
        public bool updateTimerForSketch;
        public int upLayerNum;
        public int downLayerNum;
        public List<Tuple<string, float, float, float>> HeightAdjustList;

        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
            Timer = 0.0f;
            TimerForSketch = 0.0f;
            upLayerNum = 0;
            downLayerNum = 0;
            updateTimerForSketch = false;
            SkrokeCreationTime = new List<Tuple<float, float, float>>();
            HeightAdjustList = new List<Tuple<string, float, float, float>>();
        }

        void Update()
        {
            if (updateTimerForSketch)
                TimerForSketch += Time.deltaTime;
            Timer += Time.deltaTime;        
        }
        public void setSkrokeCreationTime()
        {
            if (SkrokeBeginTime!=0)
            {
                SkrokeCreationTime.Add(Tuple.Create(SkrokeBeginTime, getTimerInSec(), getTimerInSec() - SkrokeBeginTime));            
            }
            else
            {
                SkrokeCreationTime.Add(Tuple.Create(getTimerInSec(), getTimerInSec(), 0f));
            }
            SkrokeBeginTime = 0;
        }
        public void setSkrokeBeginTimeInSec()
        {
            SkrokeBeginTime = getTimerInSec();
        }
        public void updateHeightAdjustTimes(bool up)
        {
            if (up)
            {
                upLayerNum += 1;
                
            }
            else
            {
                downLayerNum += 1;
            }
            HeightAdjustBeginTime = getTimerInSec();

        }

        public void setHeightAdjustList(bool up)
        {
            if (up)
            {
                HeightAdjustList.Add(Tuple.Create("upup", HeightAdjustBeginTime, getTimerInSec(), getTimerInSec() - HeightAdjustBeginTime));
            }
            else
            {
                HeightAdjustList.Add(Tuple.Create("down", HeightAdjustBeginTime, getTimerInSec(), getTimerInSec() - HeightAdjustBeginTime));
            }
        }
        public float getTimerInSec()
        {
            TimerInSecond = Mathf.Round(Timer);
            return TimerInSecond;
        }

        public float getTimerForSketchInSec()
        {
            TimerInSecond = Mathf.Round(TimerForSketch);
            return TimerInSecond;
        }

        public float getTimerForSketchPercentage()
        {
            TimerInSecond =100 * Mathf.Round(TimerForSketch) / Mathf.Round(Timer);
            return TimerInSecond;
        }
    }
}