using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Inspector.TypeDrawer;
using Fries.Pool;
using UnityEngine;

namespace Fries.GameTime {
    [CreateAssetMenu(menuName = "Fries/Infinite TimeUnit Data")]
    public class InfiniteTimeUnitData : ScriptableObject {
        [SerializeField] private int defaultLoopCount;
        [SerializeField] private TimeUnitData entry;

        public InfiniteTimeUnit newTimeUnit(TimeManager timeManager) => new(timeManager, this);
        public TimeUnitData get => entry;
        public int getLoopCount => defaultLoopCount;
    }
    
    [Serializable]
    public class InfiniteTimeUnit : TimeUnit {
        private InfiniteTimeUnitData data;
        private TimeUnit current;
        
        private int loopCount;

        public override string formatTime(int _) {
            return current.formatTime(loopCount);
        }

        public InfiniteTimeUnit(TimeManager timeManager, InfiniteTimeUnitData data) {
            this.timeManager = timeManager;
            this.data = data;
            loopCount = data.getLoopCount;
            current = data.get.newTimeUnit(timeManager);
        } 

        public override float advance(float timePassed) {
            float isDone = current.advance(timePassed);
            if (isDone == -1) return -1;
            
            while (isDone != -1) {
                loopCount++;
                current = data.get.newTimeUnit(timeManager);
                isDone = current.advance(isDone);
            }
            return -1;
        }
        
        public override void initData(Dictionary<int, object> data) {
            int index = (int)data[0];
            loopCount = (int)data[index];
            
            current = this.data.get.newTimeUnit(timeManager);
            data[0] = index - 1;
            current.initData(data);
        }
        
        public override Dictionary<int, object> getData() {
            var data = current.getData();
            data[(int)data[0]] = loopCount;
            return data;
        }
    }
}