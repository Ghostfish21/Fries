using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    public class PersistObjectLoadOperation {
        public List<Action> actions = new();
        private bool isDone;
        
        public void Close() {
            if (isDone) return;
            isDone = true;
            foreach (var action in actions) {
                try {
                    action();
                }
                catch (Exception e) {
                    Debug.LogError($"Caught exception when closing persist object load operation! Exception: {e}");
                }
            }
        }
    }
}