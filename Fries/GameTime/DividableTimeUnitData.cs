using System;
using System.Collections.Generic;
using Fries.Pool;
using UnityEngine;

namespace Fries.GameTime {
    [CreateAssetMenu(menuName = "Fries/Dividable TimeUnit Data")]
    public class DividableTimeUnitData : TimeUnitData {
        [SerializeField] private List<TimeUnitData> sequence = new();
        public override TimeUnit newTimeUnit(TimeManager timeManager) => new DividableTimeUnit(timeManager, this);
        public TimeUnitData get(int index) => sequence[index];
        public int length => sequence.Count;
    }
    
    [Serializable]
    public class DividableTimeUnit : TimeUnit {
        private DividableTimeUnitData data;
        private TimeUnit current;
        private int cursor = 0;

        public DividableTimeUnit(TimeManager timeManager, DividableTimeUnitData data) {
            this.timeManager = timeManager;
            this.data = data;
            current = data.get(cursor).newTimeUnit(timeManager);
        }

        public override float advance(float timePassed) {
            float isDone = current.advance(timePassed);
            if (isDone == -1) return -1;
            
            while (isDone != -1) {
                cursor++;
                if (cursor == data.length) return isDone;
                current = data.get(cursor).newTimeUnit(timeManager);
                isDone = current.advance(isDone);
            }
            return -1;
        }

        public override string formatTime(int cursor) => 
            data.formatTime(cursor) + current.formatTime(this.cursor);

        public override void initData(Dictionary<int, object> data) {
            int index = (int)data[0];
            cursor = (int)data[index];
            current = this.data.get(cursor).newTimeUnit(timeManager);
            data[0] = index - 1;
            current.initData(data);
        }

        public override Dictionary<int, object> getData() {
            var data = current.getData();
            data[(int)data[0]] = cursor;
            data[0] = (int)data[0] + 1;
            return data;
        }
    }
}