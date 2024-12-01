using System.Collections.Generic;
using UnityEngine;

namespace Fries.OrderedCode {
    public abstract class OrderedScript : MonoBehaviour {
        // #################### OrderedScript API ####################
        public string getName() {
            return this.GetType().Name;
        }

        // 获得这个脚本所依赖的所有前置的 脚本名称
        public virtual List<string> getDependencies() {
            return new List<string>();
        } 

        // 构造这个脚本，该方法相当于 Start
        public virtual void construct() { } 
        public virtual void update() { } 
        public virtual void fixedUpdate() { } 

        // #################### Unity 方法 ####################
        // Start 方法
        protected void Start() { 
            ScriptManager.inst().tryConstructModule(this);
        }

        protected void Update() {
            if (ScriptManager.inst().isConstructed(this)) update();
        }

        protected void FixedUpdate() {
            if (ScriptManager.inst().isConstructed(this)) fixedUpdate();
        }
    }
}