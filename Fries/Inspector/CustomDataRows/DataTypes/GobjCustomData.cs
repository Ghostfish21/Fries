using System;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class GobjCustomData : CustomData {
        public string getDisplayName() {
            return "GameObject";
        }

        public Type getType() {
            return typeof(GameObjectWrapper);
        }

        public object getDefaultValue() {
            return new GameObjectWrapper();
        }
    }
}