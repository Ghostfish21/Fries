using System;
using UnityEngine;

namespace Fries {
    public class ParentSetter : MonoBehaviour {
        public const int AWAKE = -2;
        public const int START = -1;
        
        public Transform parent;
        public int timing = -2;
        private void Awake() {
            if (timing < AWAKE) timing = AWAKE;
            if (timing != AWAKE) return;
            transform.SetParent(parent);
            Destroy(this);
        }
        
        private void Start() {
            if (timing != START) return;
            transform.SetParent(parent);
            Destroy(this);
        }

        private int updateCount = 0;
        private void Update() {
            if (updateCount == timing) {
                transform.SetParent(parent);
                Destroy(this);
            }
            updateCount++;
        }
    }
}