using System.Collections.Generic;
using UnityEngine;

namespace HelpSystem.DataModels {
    [CreateAssetMenu(fileName = "New Post", menuName = "Editor Doc/Post", order = 1)]
    public class Post : ScriptableObject {
        public string title;
        public List<TextAsset> pages = new();
    }

//     [System.Serializable]
//     public class GoToPageCommand : Command {
//         public int pageIndex;
//
//         public override void Execute() {
//             Debug.Log($"Go to page: {pageIndex}");
//         }
//     }
//
//     [System.Serializable]
//     public class NavigateToUnityObjectCommand : Command {
//         public Object targetObject;
//
//         public override void Execute() {
// #if UNITY_EDITOR
//             if (targetObject) {
//                 UnityEditor.Selection.activeObject = targetObject;
//                 UnityEditor.EditorGUIUtility.PingObject(targetObject);
//             }
//             else {
//                 Debug.LogWarning("Target object is null for navigation command.");
//             }
// #endif
//         }
//     }
//
//     [System.Serializable]
//     public class NavigateToProjectFileCommand : Command {
//         public string assetPath;
//
//         public override void Execute() {
// #if UNITY_EDITOR
//             if (!string.IsNullOrEmpty(assetPath)) {
//                 Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
//                 if (asset != null) {
//                     UnityEditor.Selection.activeObject = asset;
//                     UnityEditor.EditorGUIUtility.PingObject(asset);
//                 }
//                 else {
//                     Debug.LogWarning($"Asset not found at path: {assetPath}");
//                 }
//             }
//             else {
//                 Debug.LogWarning("Asset path is empty for navigation command.");
//             }
// #endif
//         }
//     }
}

