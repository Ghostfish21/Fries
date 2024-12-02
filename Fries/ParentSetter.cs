using System;
using UnityEngine;

namespace Fries {
    public class ParentSetter : MonoBehaviour {
        public Transform parent;
        
        private void Start() {
            transform.SetParent(parent);
            Destroy(this);
        }
    }
}