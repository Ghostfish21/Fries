using System;

namespace Fries.TaskPerformer {
    
    public struct ParamedAction {

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
        
        public readonly Action<object[]> action;
        public readonly object[] param;

        private ParamedAction(Action<object[]> action, params object[] param) { 
            this.action = action; 
            this.param = param;
        }
    }
    
}