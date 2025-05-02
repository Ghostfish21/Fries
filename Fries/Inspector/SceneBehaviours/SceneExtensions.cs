using System;
using UnityEngine.SceneManagement;

namespace Fries.Inspector.SceneBehaviours {
    public static class SceneExtensions {
        private static SceneSelectionProxy getProxy(Scene scene) {
            return SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'));
        }
        
        public static T getBehaviour<T>(this Scene scene) where T : SceneBehaviour {
            var v = SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'));
            if (v == null) return null;
            return v.getBehaviour<T>();
        }

        public static T[] getBehaviours<T>(this Scene scene) where T : SceneBehaviour {
            var v = SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'));
            if (v == null) return Array.Empty<T>();
            return SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'))
                .getBehaviours<T>();
        }

        public static bool hasBehaviour<T>(this Scene scene) where T : SceneBehaviour {
            var v = getProxy(scene);
            if (v == null) return false;
            return v.hasBehaviour<T>();
        }

        public static bool hasBehaviour(this Scene scene, SceneBehaviour sb) {
            var v = getProxy(scene);
            if (v == null) return false;
            return v.hasBehaviour(sb);
        }

        public static void removeBehaviour(this Scene scene, SceneBehaviour sb) {
            var v = getProxy(scene);
            if (v == null) return;
            v.removeBehaviour(sb);
        }
    }
}