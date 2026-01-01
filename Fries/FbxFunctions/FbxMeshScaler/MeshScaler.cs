using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.FbxFunctions.FbxId {
    # if UNITY_EDITOR
    public class MeshScaler : EditorWindow {
        private string CloseAfterFinishKey = $"Fbx_Mesh_Scaler.{SystemUtils.projectName()}.Close_After_Finish";
        private bool _closeAfterFinish;
        public Vector3 scaleFactor;

        private GUIStyle style;

        [MenuItem("Tools/Fries/Fbx/Fbx Mesh Scaler")]
        public static void ShowWindow() {
            GetWindow<MeshScaler>("Fbx Id Calculator");
        }

        private void OnEnable() {
            _closeAfterFinish = EditorPrefs.GetBool(CloseAfterFinishKey, true);
        }

        private string getFullPath(string path) {
            if (path.StartsWith("Assets"))
                return Application.dataPath + path.Substring("Assets".Length);
            return Path.GetFullPath(path);
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _closeAfterFinish = EditorGUILayout.Toggle("Close After Finish", _closeAfterFinish);
            scaleFactor = EditorGUILayout.Vector3Field("Scale Factor", scaleFactor);

            if (GUILayout.Button("Save")) {
                EditorPrefs.SetBool(CloseAfterFinishKey, _closeAfterFinish);
                EditorApplication.RepaintProjectWindow();
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Rescale")) {
                rescale();
            }
        }

        public void rescale(Action onComplete = null) {
            List<string> fbxPaths = new();
            foreach (var guid in Selection.assetGUIDs) {
                string childPath = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(childPath);
                if (extension.Equals(".fbx", StringComparison.OrdinalIgnoreCase))
                    fbxPaths.Add(childPath);
            }
                
            if (!fbxPaths.Any()) {
                Debug.LogWarning("Please select at least one fbx file!");
                return;
            }

            List<string> fullPaths = new();
            foreach (var fbxPath in fbxPaths) {
                // 获取项目根目录的绝对路径（Application.dataPath 指向 Assets 文件夹）
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                // 组合出绝对路径
                System.Diagnostics.Debug.Assert(projectRoot != null, nameof(projectRoot) + " != null");
                string fullPath = Path.Combine(projectRoot, fbxPath);
                fullPaths.Add(fullPath);
            }
            string arg = string.Join("[NEWITEM]", fullPaths);
            arg = $"\"{arg}\"";

            if (_closeAfterFinish)
                TaskPerformer.TaskPerformer.executeExe(getExePath("MeshScaler_py"),
                    new[] { arg, $"{scaleFactor.x},{scaleFactor.y},{scaleFactor.z}", "1" }, true, false, onComplete);
            else
                TaskPerformer.TaskPerformer.executeExe(getExePath("MeshScaler_py"),
                    new[] { arg, $"{scaleFactor.x},{scaleFactor.y},{scaleFactor.z}"}, true, false, onComplete);
        }

        private string getExePath(string exeName, [CallerFilePath] string filePath = "") {
            string directory = Path.GetDirectoryName(filePath);
            Debug.Assert(directory != null, nameof(directory) + " != null");
            // 拼接出 exe 的完整路径
            string exePath = Path.Combine(directory, $"{exeName}.exe");
            return $"\"{exePath}\"";
        }
    }
# endif
}