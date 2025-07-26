using UnityEngine;

namespace Fries.Fries.EditorDoc.Commands {
    public class HighlightInspectorCmd {
        public static void highlightInspector(string[] args) {
            string sceneName = args[0];
            string gameObjectPath = args[1];
            string componentType = args[2];
            string fieldName = args[3];

            UnityEngine.SceneManagement.Scene
                scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) {
                Debug.LogWarning($"Scene '{sceneName}' not found or not loaded.");
                return;
            }

            // Find the GameObject
            GameObject targetObject = findGameObjectInScene(scene, gameObjectPath);
            if (!targetObject) {
                Debug.LogWarning($"GameObject '{gameObjectPath}' not found in scene '{sceneName}'.");
                return;
            }

            // Select and ping the object
            UnityEditor.Selection.activeGameObject = targetObject;
            UnityEditor.EditorGUIUtility.PingObject(targetObject);

            // If component and field are specified, try to highlight them
            if (!string.IsNullOrEmpty(componentType)) {
                Component component = targetObject.GetComponent(componentType);
                if (component) {
                    UnityEditor.Selection.activeObject = component;
                    Debug.Log($"Selected component: {componentType} on {targetObject.name}");

                    if (!string.IsNullOrEmpty(fieldName)) {
                        // TODO 在这里补全对 highlight 方法的调用
                        // TODO 在这里补全对 clearHighlight 方法的调用
                    }
                }
                else {
                    Debug.LogWarning($"Component '{componentType}' not found on GameObject '{gameObjectPath}'.");
                }
            }
        }

        private static GameObject findGameObjectInScene(UnityEngine.SceneManagement.Scene scene, string path) {
            string[] pathParts = path.Split('/');
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects) {
                if (root.name == pathParts[0]) {
                    GameObject current = root;
                    for (int i = 1; i < pathParts.Length; i++) {
                        Transform child = current.transform.Find(pathParts[i]);
                        if (!child)
                            return null;
                        current = child.gameObject;
                    }

                    return current;
                }
            }

            return null;
        }
        
        private static void highlight(float x, float y, float width, float height) {
            InspectorOverlayBoxes.ClearBoxes(true, true);
            InspectorOverlayBoxes.DrawBox(new Rect(20, 20, 200, 60), new Color(1f, 0.4f, 0.1f), 3f, followScroll: true);
        }
        
        private static void clearHighlight(float delay) {
            // TODO 在这里补全 delay 的生效
            InspectorOverlayBoxes.ClearBoxes(true, true);
        }
    }
}