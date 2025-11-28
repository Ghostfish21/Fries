using System;
using UnityEngine;

namespace Fries.InsertionEventSys {
    public class EvtHandle {
        public EvtInfo eventInfo;
        public EvtListenerInfo nextListener;
        private string shouldProcessRegisterAssemblyFullname;
        private Func<EvtHandle, bool> shouldProcessEvt = _ => true;
        public void setShouldProcess(Func<EvtHandle, bool> shouldProcessEvt) {
            if (shouldProcessEvt.Method.DeclaringType == null) {
                Debug.LogWarning("You can't register a Process Check method without declaring type!");
                return;
            }
            shouldProcessRegisterAssemblyFullname = shouldProcessEvt.Method.DeclaringType.Assembly.FullName;

            this.shouldProcessEvt = shouldProcessEvt;
        }

        public bool shouldProcess() {
            if (shouldProcessEvt == null) return true;
            try {
                if (!nextListener.canBeExternallyCancelled) return true;
                if (!nextListener.isFriendlyAssembly(shouldProcessRegisterAssemblyFullname)) return true;
                
                bool result = shouldProcessEvt(this);
                return result;
            }
            catch (Exception ex) {
                Debug.LogWarning($"EvtHandle {this} failed to process: {ex.Message}");
                return true;
            }
        }
    }
}