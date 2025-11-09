using System;
using UnityEngine;

namespace Fries.InsertionEventSys {
    public class EvtHandle {
        public EvtInfo eventInfo;
        public EvtListenerInfo nextListener;
        public Func<EvtHandle, bool> shouldProcessEvt = _ => true;

        public bool shouldProcess(EvtHandle evtHandle) {
            if (shouldProcessEvt == null) return true;
            try {
                bool result = shouldProcessEvt(evtHandle);
                return result;
            }
            catch (Exception ex) {
                Debug.LogWarning($"EvtHandle {evtHandle} failed to process: {ex.Message}");
                return true;
            }
        }
    }
}