using System;

namespace Fries.InsertionEventSys {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InsertionEventDeclarer : Attribute {
        public Type type { get; }
        public string eventName { get; }
        public Type[] argsTypes { get; }
        
        public InsertionEventDeclarer(Type type, string eventName, params Type[] argsTypes) {
            this.type = type;
            this.eventName = eventName;
            this.argsTypes = argsTypes;
        }
    }
}