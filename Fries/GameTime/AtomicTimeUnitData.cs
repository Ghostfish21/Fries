using System;
using System.Collections.Generic;
using Fries.EventFunctions;
using Fries.InsertionEventSys;
using Fries.Pool;
using UnityEngine;

namespace Fries.GameTime {
    [CreateAssetMenu(menuName = "Fries/Atomic TimeUnit Data")]
    public class AtomicTimeUnitData : TimeUnitData {
        // 获取以秒为单位的 时间持续长度
        public float duration = 1;
        public override TimeUnit newTimeUnit(TimeManager timeManager) => new AtomicTimeUnit(timeManager, this);
    }
    
    [Serializable]
    [EvtDeclarer("OnAtomicTimeUnitExpired", typeof(TimeManager))]
    public class AtomicTimeUnit : TimeUnit {
        private AtomicTimeUnitData data;
        private float timePassed;

        public AtomicTimeUnit(TimeManager timeManager, AtomicTimeUnitData data) {
            this.timeManager = timeManager;
            this.data = data;
        }

        public override float advance(float timePassed) {
            this.timePassed += timePassed;
            if (this.timePassed < data.duration) return -1;
            this.triggerListener("OnAtomicTimeUnitExpired", timeManager);
            return this.timePassed - data.duration;
        }

        public override string formatTime(int cursor) => data.formatTime(cursor);

        public override void initData(Dictionary<int, object> data) {
            timePassed = (float)data[1];
        }

        public override Dictionary<int, object> getData() {
            Dictionary<int, object> dict = new() {
                [1] = timePassed,
                [0] = 2
            };
            return dict;
        }
    }
}