# if UNITY_EDITOR
using System.Collections.Generic;
using Fries.Inspector.EditorEvents;
using UnityEditor;
using UnityEngine;

namespace Seagull.Interior_04E.Inspector {
    [InitializeOnLoad]
    public class MaterialHiderManager {
        private static GameObject selectingGobj;
        
        static MaterialHiderManager() {
            EditorApplication.update += () => {
                if (Selection.activeGameObject == selectingGobj) return;
                selectionChange(selectingGobj, Selection.activeGameObject);
                selectingGobj = Selection.activeGameObject;
            };
        }

        public static void selectionChange(GameObject old, GameObject neu) {
            if (old != null) {
                var hider = getHider(old);
                hider?.update(false);
            }
            if (neu != null) {
                var hider = getHider(neu);
                hider?.update(true);
            }
        }

        public static MaterialHider getHider(GameObject gameObject) {
            return gameObject.GetComponent<MaterialHider>();
        }
    }
}
# endif