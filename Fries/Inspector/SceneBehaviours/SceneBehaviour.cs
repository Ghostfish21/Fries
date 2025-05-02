using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fries.Inspector.SceneBehaviours {
    public class SceneBehaviour : ScriptableObject {
        // private Action _onCreate;
        // private Action _onLoad;
        // private Action _onValidate;
        // private Action _onEditorUpdate;
        //
        // private Action bindAction(Type type, string methodName, BindingFlags flags) {
        //     var mi = type.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
        //     return mi != null
        //         ? (Action) Delegate.CreateDelegate(typeof(Action), this, mi)
        //         : null;
        // }

        private static Dictionary<long, SceneBehaviour> sceneBehaviours = new();
        public static SceneBehaviour getSceneBehaviour(long timeId) {
            sceneBehaviours.TryGetValue(timeId, out var v);
            return v;
        }

        public long createTime;
        public SceneBehaviour() {
            createTime = SystemUtils.currentTimeMillis();
            while (sceneBehaviours.ContainsKey(createTime)) 
                createTime++;
            sceneBehaviours[createTime] = this;
        }

        public virtual void OnEnable() {
            sceneBehaviours[createTime] = this;
        }
    }
}