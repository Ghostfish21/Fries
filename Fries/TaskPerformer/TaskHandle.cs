﻿using System;
using System.Collections.Generic;

namespace Fries.TaskPerformer {
    public class TaskHandle {
        public bool isCancelled { get; internal set; }
        public bool isExecuted { get; internal set; }
        public Action onComplete;
        public int executedTime { get; internal set; }
        public Dictionary<string, object> data { get; internal set; } = new();

        public void cancel() {
            isCancelled = true;
        }
    }
}