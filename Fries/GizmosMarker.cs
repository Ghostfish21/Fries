using System;
using UnityEngine;

namespace Fries {
    public class GizmosMarker : TestMonoBehaviour {
        public float radiu = 0.1f;
        private void OnDrawGizmos() {
            Gizmos.DrawSphere(transform.position, radiu);
        }
    }
}