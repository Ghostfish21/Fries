using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries {
    public class TransformData {
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;
    }
    
    public class TransformGroup : MonoBehaviour {
        private Dictionary<Transform, TransformData> data;
        public List<Transform> transforms;

        private void OnValidate() {
            transforms?.ForEach(t => {
                if (t == null) return;
                if (!data.ContainsKey(t)) {
                    data[t] = new TransformData {
                        position = t.position,
                        eulerAngles = t.eulerAngles,
                        scale = t.localScale
                    };
                }

                t.position = data[t].position + transform.localPosition;
                t.eulerAngles = data[t].eulerAngles + transform.eulerAngles;
                t.localScale = data[t].scale + transform.localScale;
            });
        }
    }
}