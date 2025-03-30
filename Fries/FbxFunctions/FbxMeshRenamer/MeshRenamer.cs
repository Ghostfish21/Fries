using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
# if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.FbxFunctions.FbxMeshRenamer {
    # if UNITY_EDITOR
    public class MeshRenamer : EditorWindow {
        private string CloseAfterFinishKey = $"Fbx_Mesh_Renamer.{SystemUtils.projectName()}.Close_After_Finish";
        private bool _closeAfterFinish;

        private GUIStyle style;

        [MenuItem("Tools/Fries/Fbx/Mesh Renamer")]
        public static void ShowWindow() {
            GetWindow<MeshRenamer>("Mesh Renamer");
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

            if (GUILayout.Button("Save")) {
                EditorPrefs.SetBool(CloseAfterFinishKey, _closeAfterFinish);
                EditorApplication.RepaintProjectWindow();
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Rename Mesh")) {
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
                    TaskPerformer.TaskPerformer.executeExe(getExePath("MeshRenamer_py"),
                        new[] { arg, "1" }, true, false);
                else
                    TaskPerformer.TaskPerformer.executeExe(getExePath("MeshRenamer_py"),
                        new[] { arg }, true, false);
            }
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