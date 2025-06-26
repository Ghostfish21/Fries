using System;

namespace Fries.InsertionEventSys {
    [AttributeUsage(AttributeTargets.Method)]
    public class InsertionEventListener : Attribute {
        public readonly Type type;
        public readonly string eventName;
        
        public InsertionEventListener(Type type, string eventName) {
            this.type = type;
            this.eventName = eventName;
        }
    }
}