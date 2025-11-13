using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.CoroutineExpr {
    public class CoroutineScheduler : MonoBehaviour {
        private List<CoroutineHandle> runningCoroutines = new();
        private readonly object lockObj = new();

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        private CoroutineHandle start(IEnumerator routine) {
            CoroutineHandle handle;
            lock (lockObj) {
                handle = new CoroutineHandle(routine);
                runningCoroutines.Add(handle);
                handle.setStatus(CoroutineHandle.RUNNING);
            }
            return handle;
        }

        private void suspend(CoroutineHandle handle) {
            lock (lockObj) {
                if (!handle.isSuspendable()) return;
                handle.setStatus(CoroutineHandle.SUSPENDED);
                runningCoroutines.Remove(handle);
            }
        }

        private void resume(CoroutineHandle handle) {
            lock (lockObj) {
                if (!handle.isRegistrable()) return;
                handle.setStatus(CoroutineHandle.RUNNING);
                runningCoroutines.Add(handle);
            }
        }
        
        private void Update() {
            lock (lockObj) {
                for (int i = runningCoroutines.Count-1; i >= 0; i--) {
                    CoroutineHandle handle = runningCoroutines[i];
                    try {
                        Func<bool> isReady = null;
                        if (handle.routine.Current != null) {
                            object y = handle.routine.Current;
                            if (y is Func<bool> predicate) isReady = predicate;
                            else Debug.LogWarning("Coroutine predicate must be a function, skipping...");
                        }
                        if (isReady != null && !isReady()) continue;
                    
                        if (handle.routine.MoveNext()) continue;
                        runningCoroutines.RemoveAt(i); 
                        handle.setStatus(CoroutineHandle.COMPLETED);
                    }
                    catch (Exception ex) {
                        runningCoroutines.RemoveAt(i);
                        handle.fault = ex;
                        handle.setStatus(CoroutineHandle.FAULTED);
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        
    }
}