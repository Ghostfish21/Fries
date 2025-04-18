using System;

namespace Fries.Inspector.UndoRedoEvent {
    [AttributeUsage(AttributeTargets.Field)]
    public class OnUndoRedoAttribute : Attribute {
        public readonly string groupId = null;

        public OnUndoRedoAttribute() {
        }

        public OnUndoRedoAttribute(string groupId) {
            this.groupId = groupId;
        }
    }
}