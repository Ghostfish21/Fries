// using Fries.Fries.EditorDoc;
// using UnityEngine;
//
// namespace HelpSystem.DataModels {
//     [System.Serializable]
//     public class NavigateToSceneObjectCommand : Command {
//         public string sceneName;
//         public string gameObjectPath; // Hierarchy path like "Parent/Child/Target"
//         public string componentType;
//         public string fieldName;
//
//         public override void Execute() {
// #if UNITY_EDITOR
//             // Find the scene
//             UnityEngine.SceneManagement.Scene
//                 scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
//             if (!scene.IsValid()) {
//                 Debug.LogWarning($"Scene '{sceneName}' not found or not loaded.");
//                 return;
//             }
//
//             // Find the GameObject
//             GameObject targetObject = FindGameObjectInScene(scene, gameObjectPath);
//             if (!targetObject) {
//                 Debug.LogWarning($"GameObject '{gameObjectPath}' not found in scene '{sceneName}'.");
//                 return;
//             }
//
//             // Select and ping the object
//             UnityEditor.Selection.activeGameObject = targetObject;
//             UnityEditor.EditorGUIUtility.PingObject(targetObject);
//
//             // If component and field are specified, try to highlight them
//             if (!string.IsNullOrEmpty(componentType)) {
//                 Component component = targetObject.GetComponent(componentType);
//                 if (component != null) {
//                     UnityEditor.Selection.activeObject = component;
//                     Debug.Log($"Selected component: {componentType} on {targetObject.name}");
//
//                     if (!string.IsNullOrEmpty(fieldName)) {
//                         Debug.Log($"Field to focus: {fieldName}");
//                         HighlightableEditor.Highlight(component, fieldName, 2.0f); // Highlight for 2 seconds
//                     }
//                 }
//                 else {
//                     Debug.LogWarning($"Component '{componentType}' not found on GameObject '{gameObjectPath}'.");
//                 }
//             }
// #endif
//         }
//
//         private GameObject FindGameObjectInScene(UnityEngine.SceneManagement.Scene scene, string path) {
//             string[] pathParts = path.Split('/');
//             GameObject[] rootObjects = scene.GetRootGameObjects();
//
//             foreach (GameObject root in rootObjects) {
//                 if (root.name == pathParts[0]) {
//                     GameObject current = root;
//                     for (int i = 1; i < pathParts.Length; i++) {
//                         Transform child = current.transform.Find(pathParts[i]);
//                         if (child == null)
//                             return null;
//                         current = child.gameObject;
//                     }
//
//                     return current;
//                 }
//             }
//
//             return null;
//         }
//     }
//
//     [System.Serializable]
//     public class OpenWindowCommand : Command {
//         public string windowTypeName; // e.g., "UnityEditor.ConsoleWindow"
//
//         public override void Execute() {
// #if UNITY_EDITOR
//             try {
//                 System.Type windowType = System.Type.GetType(windowTypeName);
//                 if (windowType != null) {
//                     UnityEditor.EditorWindow.GetWindow(windowType);
//                     Debug.Log($"Opened window: {windowTypeName}");
//                 }
//                 else {
//                     Debug.LogWarning($"Window type '{windowTypeName}' not found.");
//                 }
//             }
//             catch (System.Exception e) {
//                 Debug.LogError($"Failed to open window '{windowTypeName}': {e.Message}");
//             }
// #endif
//         }
//     }
//
//     [System.Serializable]
//     public class ExecuteMenuItemCommand : Command {
//         public string menuItemPath; // e.g., "Window/General/Console"
//
//         public override void Execute() {
// #if UNITY_EDITOR
//             try {
//                 UnityEditor.EditorApplication.ExecuteMenuItem(menuItemPath);
//                 Debug.Log($"Executed menu item: {menuItemPath}");
//             }
//             catch (System.Exception e) {
//                 Debug.LogError($"Failed to execute menu item '{menuItemPath}': {e.Message}");
//             }
// #endif
//         }
//     }
// }