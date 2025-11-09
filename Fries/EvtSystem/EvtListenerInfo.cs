using System;
using System.Collections.Generic;

namespace Fries.InsertionEventSys {
    public class EvtListenerInfo {
        public readonly string listenerName;
        public readonly float priority;

        public EvtListenerInfo(string listenerName, float priority) {
            this.listenerName = listenerName;
            this.priority = priority;
        }
        
        public bool Equals(EvtListenerInfo other) =>
            other is not null &&
            priority == other.priority &&
            StringComparer.Ordinal.Equals(listenerName, other.listenerName);

        public override bool Equals(object obj) => obj is EvtListenerInfo other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(listenerName), priority);
    }

    sealed class ListenerComparer : IComparer<EvtListenerInfo> {
        public int Compare(EvtListenerInfo x, EvtListenerInfo y) {
            if (x == null || y == null) throw new ArgumentNullException();
            // 先按 priority 降序
            int byPrio = y.priority.CompareTo(x.priority);
            if (byPrio != 0) return byPrio;
            return StringComparer.Ordinal.Compare(x.listenerName, y.listenerName);
        }
    }
}