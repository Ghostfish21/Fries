using System;

namespace DialogueSystem {
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class EditorUpdateAttribute : Attribute {
        public EditorUpdateAttribute() {
        }
    }
}