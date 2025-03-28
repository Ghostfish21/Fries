using System;

namespace Fries.Inspector.EditorEvents {
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class EditorUpdateAttribute : Attribute {
        public EditorUpdateAttribute() {
        }
    }
}