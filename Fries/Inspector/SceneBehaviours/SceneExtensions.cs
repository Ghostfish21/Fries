using UnityEngine.SceneManagement;

namespace Fries.Inspector.SceneBehaviours {
    public static class SceneExtensions {
        private static SceneSelectionProxy getProxy(Scene scene) {
            return SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'));
        }
        
        public static T getBehaviour<T>(this Scene scene) where T : SceneBehaviour {
            return SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6')).getBehaviour<T>();
        }

        public static T[] getBehaviours<T>(this Scene scene) where T : SceneBehaviour {
            return SceneBehaviourData.getProxy(scene.path.Replace('\\', '\u00a6').Replace('/', '\u00a6'))
                .getBehaviours<T>();
        }

        public static bool hasBehaviour<T>(this Scene scene) where T : SceneBehaviour {
            return getProxy(scene).hasBehaviour<T>();
        }

        public static bool hasBehaviour(this Scene scene, SceneBehaviour sb) {
            return getProxy(scene).hasBehaviour(sb);
        }

        public static void removeBehaviour(this Scene scene, SceneBehaviour sb) {
            getProxy(scene).removeBehaviour(sb);
        }
    }
}