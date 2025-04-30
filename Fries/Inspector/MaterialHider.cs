using UnityEngine;

namespace Fries.Inspector {
    [ExecuteInEditMode]
    public class MaterialHider : MonoBehaviour {
        public bool hided = true;

        private void OnValidate() {
            foreach (var material in gameObject.GetComponent<Renderer>().sharedMaterials) {
                if (hided) material.hideFlags |= HideFlags.HideInInspector;
                else material.hideFlags &= ~HideFlags.HideInInspector;
            }
        }

        public void update(bool isHided) {
            if (!hided) return;
            
            foreach (var material in gameObject.GetComponent<Renderer>().sharedMaterials) {
                if (isHided) material.hideFlags |= HideFlags.HideInInspector;
                else material.hideFlags &= ~HideFlags.HideInInspector;
            }
        }
    }
}