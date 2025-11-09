using System;

namespace Fries.InsertionEventSys {
    [AttributeUsage(AttributeTargets.Method)]
    public class EvtListener : Attribute {
        public readonly Type type;
        public readonly string eventName;
        public readonly float priority;
        
        public EvtListener(Type type, string eventName, float priority = 0) {
            this.type = type;
            this.eventName = eventName;
            this.priority = priority;
        }
    }
}