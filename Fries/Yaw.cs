using System;
using UnityEngine;

namespace Fries {
    public class Yaw : MonoBehaviour {
        public float yaw { get; private set; } = 0;

        private void Update() {
            Vector3 endPoint = transform.TransformPoint(1, 0, 0);
            float f = Mathf.Atan2(endPoint.z, endPoint.x) * Mathf.Rad2Deg;
            if (f > 360) f -= 360;
            else if (f <= 0) f += 360;
            yaw = f;
        }
    }
}