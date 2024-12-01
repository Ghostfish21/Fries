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
        
        public readonly Action<object[]> action;
        public readonly object[] param;

        private ParamedAction(Action<object[]> action, params object[] param) { 
            this.action = action; 
            this.param = param;
        }
    }
    
}