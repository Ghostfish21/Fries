
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.EditorEvents {
    public class EditorAppUtils {
        public static bool isEditor() {
            # if UNITY_EDITOR
            return true;
            # else
            return false;
            # endif
        }
        
        public static double timeSinceStartUp() {
            # if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
            # else
            return -1;
            # endif
        }

        public static string openFilePanel(string title, string directory, string extension) {
            # if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
            # else
            return null;
            # endif
        }

        public static bool displayDialog(string title, string message, string ok, string cancel = null) {
            # if UNITY_EDITOR
            if (cancel == null)
                return EditorUtility.DisplayDialog(title, message, ok);
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
            # else
            return false;
            # endif
        }
    }
}