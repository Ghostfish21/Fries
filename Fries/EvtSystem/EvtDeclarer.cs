using System;

namespace Fries.InsertionEventSys {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EvtDeclarer : Attribute {
        public string eventName { get; }
        public Type[] argsTypes { get; }
        
        public EvtDeclarer(string eventName, params Type[] argsTypes) {
            this.eventName = eventName;
            this.argsTypes = argsTypes;
        }
    }
}