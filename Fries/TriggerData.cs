using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries {
    [RequireComponent(typeof(Collider))]
    public class TriggerData : MonoBehaviour {
        public List<GameObject> objects = new();
        
        private void OnTriggerEnter(Collider other) {
            objects.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other) {
            objects.Remove(other.gameObject);
        }
    }
}