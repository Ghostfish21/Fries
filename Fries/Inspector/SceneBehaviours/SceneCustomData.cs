using System;
using UnityEngine;

namespace Fries.Inspector.SceneBehaviours {
    public class SceneCustomData : SceneBehaviour {
        public bool test;
        public SceneCustomData() : base() {
            
        }

        public override void OnEnable() {
            base.OnEnable();
        }
    }
}