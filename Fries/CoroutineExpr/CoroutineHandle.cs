using System;
using System.Collections;

namespace Fries.CoroutineExpr {
    public class CoroutineHandle {
        public IEnumerator routine { get; private set; }
        private bool isComplete;
        public Exception fault;

        public CoroutineHandle(IEnumerator routine) {
            this.routine = routine;
        }

        public const int NEW = 0;
        public const int RUNNING = 1;
        public const int SUSPENDED = 2;
        public const int COMPLETED = 3;
        public const int FAULTED = 4;
        private int status = NEW;

        public bool isRegistrable() {
            if (status is NEW or SUSPENDED) return true;
            return false;
        }
        public bool isSuspendable() {
            if (status is RUNNING) return true;
            return false;
        }

        public void setStatus(int status) {
            this.status = status;
        }
    }
}