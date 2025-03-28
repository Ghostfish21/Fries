
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

        public static string openFilePanel(string title, string directory, string extension) {
            # if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
            # endif
            return null;
        }

        public static bool displayDialog(string title, string message, string ok, string cancel = null) {
            # if UNITY_EDITOR
            if (cancel == null)
                return EditorUtility.DisplayDialog(title, message, ok);
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
            # endif
            return false;
        }
    }
}