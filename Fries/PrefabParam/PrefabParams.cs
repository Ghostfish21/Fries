using System;
using System.Collections.Generic;
using Fries.Data.FastCache;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.PrefabParam {
    public class PrefabParams {
        [EvtDeclarer] public struct BeforePrefabParamsInit { }

        public static int Capacity = 1000;
        private readonly LruCache<int, List<object>> parameters = new(Capacity);

        internal void setLongTermParams(int instanceId, List<object> parameters) =>
            this.parameters.Put(instanceId, parameters);
        internal List<object> getLongTermParams(int instId) {
            bool res = this.parameters.TryGetValue(instId, out List<object> parameters);
            if (res) return parameters;
            return null;
        }

        private readonly Stack<ShortTermParams> paramRegStack = new();
        internal int shortTermParamsCount() => paramRegStack.Count;
        internal ShortTermParams peekShortTermParams() {
            if (paramRegStack.Count == 0) return null;
            return paramRegStack.Peek();
        }

        internal void pushShortTermParams(int shortTermParamsId, List<object> parameters) =>
            paramRegStack.Push(new ShortTermParams(shortTermParamsId, parameters));

        internal bool tryPopShortTermParams(int shortTermParamsId) {
            if (paramRegStack.Count == 0) return false;
            
            var tuple = paramRegStack.Peek();
            if (tuple.paramsId != shortTermParamsId) return false;
            paramRegStack.Pop();
            return true;
        }

        public static PrefabParams Singleton;
        private static int shortTermParamsIdCounter = 0;

        internal static int getShortTermParamsId() {
            int ret = shortTermParamsIdCounter;
            shortTermParamsIdCounter++;
            return ret;
        }

        [EvtListener(typeof(Events.OnEvtsysLoaded))]
        private static void init() {
            Evt.TriggerNonAlloc<BeforePrefabParamsInit>();
            Singleton = new PrefabParams();
            shortTermParamsIdCounter = 0;
        }
        
        internal static bool hasParams(int instanceId, PhaseEnum phase) {
            if (phase == PhaseEnum.Awake) return Singleton.paramRegStack.Count > 0;
            return Singleton.parameters.ContainsKey(instanceId);
        }
        
        internal static List<object> getParams(int instanceId, PhaseEnum phase) {
            if (phase == PhaseEnum.Awake) {
                if (Singleton.paramRegStack.Count == 0) return null;
                return Singleton.paramRegStack.Peek().parameters;
            }

            if (Singleton.parameters.TryGetValue(instanceId, out var list)) return list;
            return null;
        }

        internal static int tryGetParams<T>(int instanceId, PhaseEnum phase, int index, out T ret) {
            ret = default;
            if (phase == PhaseEnum.Awake) {
                if (Singleton.paramRegStack.Count == 0) return RetCodes.EPNE;
                List<object> parameters = Singleton.paramRegStack.Peek().parameters;
                if (parameters == null) return RetCodes.EBUG;
                if (index >= parameters.Count) return RetCodes.EIOoR;
                object target = parameters[index];
                if (target == null) return RetCodes.NP;
                if (target is not T finalTarget) return RetCodes.EPIT;
                ret = finalTarget;
                return RetCodes.SUCCESS;
            }

            if (!Singleton.parameters.TryGetValue(instanceId, out var list)) return RetCodes.EPNE;
            if (list == null) return RetCodes.EBUG;
            if (index >= list.Count) return RetCodes.EIOoR;
            object target1 = list[index];
            if (target1 == null) return RetCodes.NP;
            if (target1 is not T finalTarget1) return RetCodes.EPIT;
            ret = finalTarget1;
            return RetCodes.SUCCESS;
        }
    }
}