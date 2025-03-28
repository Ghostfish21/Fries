
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.EditorEvents {
    public class EditorAppUtils {
        public static bool isEditor() {
            # if UNITY_EDITOR
            return true;
            # endif
            return false;
        }
        
        public static double timeSinceStartUp() {
            # if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
            # endif
            return -1;
        }
    }
}