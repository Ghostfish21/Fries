using System.Collections.Generic;
using Fries.InsertionEventSys;
using UnityEngine;

namespace Fries.GameTime {
    public class TimeManager : MonoBehaviour {
        [SerializeField] private InfiniteTimeUnitData infiniteTimeData;
        private InfiniteTimeUnit infiniteTimeUnit;
        private bool isStarted;
        
        public string formatedTime => infiniteTimeUnit.formatTime(0);

        public void start(InfiniteTimeUnitData infiniteTimeData = null, Dictionary<int, object> data = null) {
            if (infiniteTimeData) this.infiniteTimeData = infiniteTimeData;
            if (!this.infiniteTimeData) {
                Debug.LogError("Unable to start the Time Manager, because there is no infinite time data found!");
                return;
            }
            
            infiniteTimeUnit = this.infiniteTimeData.newTimeUnit(this);
            if (data != null) infiniteTimeUnit.initData(data);
            isStarted = true;
        }

        private void Update() {
            if (!isStarted) return;
            infiniteTimeUnit.advance(Time.deltaTime);
        }

        public Dictionary<int, object> getData() {
            return infiniteTimeUnit.getData();
        }
    }
}