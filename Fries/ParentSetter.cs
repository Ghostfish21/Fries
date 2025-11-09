using System;
using UnityEngine;

namespace Fries {
    public class ParentSetter : MonoBehaviour {
        public Transform parent;
        private void Awake() {
            transform.SetParent(parent);
        }
    }
}