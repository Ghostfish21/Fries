using System;

namespace Fries.TaskPerformer {
    
    public struct ParamedAction : IEquatable<ParamedAction> {

        /// <summary>
        /// Get a paramed action with the action and the param
        /// </summary>
        public static ParamedAction pa(Action<object[]> action, params object[] param) => new(action, param);
        /// <summary>
        /// Get a paramed action with the action
        /// </summary>
        public static ParamedAction pa(Action action) => new ParamedAction(_ => action(), null);
        
        public static implicit operator ParamedAction((Action<object[]> action, object[] parameters) tuple) {
            return new ParamedAction(tuple.action, tuple.parameters);
        }

        // 隐式转换运算符：将无参数的 Action 转换为 ParamedAction
        public static implicit operator ParamedAction(Action action) {
            return pa(action);
        }
        
        public Action<object[]> action;
        public object[] param;
        
        public TaskHandle taskHandle;

        private ParamedAction(Action<object[]> action, params object[] param) { 
            this.action = action; 
            this.param = param;
            this.taskHandle = null;
        }

        public bool Equals(ParamedAction other) {
            return Equals(action, other.action) && Equals(param, other.param) && Equals(taskHandle, other.taskHandle);
        }

        public override bool Equals(object obj) {
            return obj is ParamedAction other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(action, param, taskHandle);
        }
    }
    
}