using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Pool;
using UnityEngine;

namespace Fries.GameTime {
    public abstract class TimeUnitData : ScriptableObject {
        public abstract TimeUnit newTimeUnit(TimeManager timeManager);
        
        [SerializeField] [SerializeReference] 
        internal TimeFormatterSelector timeFormatterSelector = new();
        [SerializeField] internal string format;
        private string defaultFormatter(int cursor) => format.Replace("?", cursor + 1 + "");
        private string primitiveFormatter(int cursor) => cursor + "";
        
        private Func<int, string> formatterCache;
        public string formatTime(int cursor) {
            if (formatterCache == null) {
                if (!string.IsNullOrEmpty(format)) formatterCache = defaultFormatter;
                else {
                    MethodInfo mi = timeFormatterSelector.getSelectedMethod();
                    if (mi == null) formatterCache = primitiveFormatter;
                    else formatterCache = (Func<int, string>)Delegate.CreateDelegate(typeof(Func<int, string>), mi);
                }
            }
            return formatterCache(cursor);
        }
    }

    public abstract class TimeUnit {
        protected TimeManager timeManager;
        public abstract float advance(float timePassed);
        public abstract string formatTime(int cursor);
        public abstract void initData(Dictionary<int, object> data);
        public abstract Dictionary<int, object> getData();
    }
}