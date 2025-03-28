
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.EditorEvents {
    public class EditorAppUtils {
        public static double timeSinceStartUp() {
            # if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
            # endif
            return -1;
        }
    }
}