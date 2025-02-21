using System;
using System.IO;
using System.Linq;
# if UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
# endif
using UnityEngine;
using Fries.TaskPerformer;

namespace Fries.FbxFunctions.FbxOrientationFixer {
    # if UNITY_EDITOR
    public class OrientationFixer : EditorWindow {
        private string CloseAfterFinishKey = $"Fbx_Orientation_Fixer.{SystemUtils.projectName()}.Close_After_Finish";
        private string _folderPath;
        private bool _selectFbx;
        private bool _closeAfterFinish;
        private float _xRotate;
        private float _yRotate;
        private float _zRotate;

        private GUIStyle style;

        [MenuItem("Tools/Fries/Fbx/Orientation Fixer")]
        public static void ShowWindow() {
            GetWindow<OrientationFixer>("Orientation Fixer");
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
            style ??= new GUIStyle(EditorStyles.label) {
                richText = true,
                wordWrap = true
            };
            string combined = "";

            if (_folderPath != null) {
                string divider = "\u200B";
                string text = _folderPath;
                List<string> chars = text.Select(c => c + "").ToList();
                combined = string.Join(divider, chars);
            }

            EditorGUILayout.LabelField("<b>Locked:</b> " + combined, style);
            _closeAfterFinish = EditorGUILayout.Toggle("Close After Finish", _closeAfterFinish);
            _selectFbx = EditorGUILayout.Toggle("Selected Fbx Only", _selectFbx);

            _xRotate = EditorGUILayout.FloatField("X Rotation", _xRotate);
            _yRotate = EditorGUILayout.FloatField("Y Rotation", _yRotate);
            _zRotate = EditorGUILayout.FloatField("Z Rotation", _zRotate);

            if (GUILayout.Button("Save")) {
                EditorPrefs.SetBool(CloseAfterFinishKey, _closeAfterFinish);

                EditorApplication.RepaintProjectWindow();
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Lock Folder")) {
                if (Selection.assetGUIDs.Length == 1) {
                    string relativePath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    // 判断选中的对象是否为有效文件夹
                    if (!string.IsNullOrEmpty(relativePath) && AssetDatabase.IsValidFolder(relativePath))
                        _folderPath = getFullPath(relativePath);
                    else {
                        Debug.LogWarning("Please select a valid folder!");
                        _folderPath = "";
                    }
                }
                else {
                    Debug.LogWarning("Please only select 1 folder!");
                    _folderPath = "";
                }
            }

            if (GUILayout.Button("Rotate and Bake")) {
                if (_selectFbx) {
                    selectedFbxProcess();
                    return;
                }

                if (_folderPath == null || _folderPath.Trim() == "") {
                    Debug.LogWarning("Please lock a folder!");
                    return;
                }

                string path = $"\"{_folderPath}\"";
                if (_closeAfterFinish)
                    TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater_py"),
                        new[] { path, _xRotate + "", _yRotate + "", _zRotate + "", "1" }, true, false);
                else
                    TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater_py"),
                        new[] { path, _xRotate + "", _yRotate + "", _zRotate + "" }, true, false);
            }
        }

        private void selectedFbxProcess() {
            if (Selection.assetGUIDs.Length == 0) {
                Debug.LogWarning("Please select fbx models you want to process");
                return;
            }

            List<string> fbxPaths = new();
            foreach (var guid in Selection.assetGUIDs) {
                string childPath = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(childPath);
                if (extension.Equals(".fbx", StringComparison.OrdinalIgnoreCase))
                    fbxPaths.Add(childPath);
            }

            if (fbxPaths.Count == 0) {
                Debug.LogWarning("Please select fbx models you want to process");
                return;
            }

            System.Diagnostics.Debug.Assert(_folderPath != null, nameof(_folderPath) + " != null");
            int index = _folderPath.IndexOf("Assets", StringComparison.Ordinal);
            string assetPath = _folderPath[index..];
            string folderGuid = AssetDatabase.CreateFolder(assetPath, $"Temp_{SystemUtils.projectName()}_Selected");
            if (folderGuid == "") {
                Debug.LogWarning("Failed to create temporary folder!");
                return;
            }
            string newFolderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            AssetDatabase.Refresh();

            bool quitFlag = false;

            List<string> newFbxPaths = new();
            fbxPaths.ForEach(singlePath => {
                if (quitFlag) return;
                string fileName = Path.GetFileName(singlePath);
                string destinationPath = Path.Combine(newFolderPath, fileName).Replace("\\", "/");
                string moveResult = AssetDatabase.MoveAsset(singlePath, destinationPath);
                if (!string.IsNullOrEmpty(moveResult)) {
                    Debug.LogError($"Move file failed before bakery at file: {singlePath}, detail: " + moveResult);
                    quitFlag = true;
                    return;
                }

                newFbxPaths.Add(destinationPath);
            });
            
            AssetDatabase.Refresh();
            if (quitFlag) return;
            
            string path = $"\"{getFullPath(newFolderPath)}\"";

            void onComplete() {
                string originalFolder = assetPath;
                newFbxPaths.ForEach(singlePath => {
                    string fileName = Path.GetFileName(singlePath);
                    string destinationPath = Path.Combine(originalFolder, fileName).Replace("\\", "/");
                    string moveResult = AssetDatabase.MoveAsset(singlePath, destinationPath);
                    if (!string.IsNullOrEmpty(moveResult)) {
                        Debug.LogWarning($"Move file failed after bakery at file: {singlePath}, detail: " + moveResult);
                    }
                });
                bool deleteF = AssetDatabase.DeleteAsset(newFolderPath);
                if (!deleteF) Debug.LogWarning("Temp folder failed to delete itself!");
            }
            
            if (_closeAfterFinish)
                TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater_py"),
                    new[] { path, _xRotate + "", _yRotate + "", _zRotate + "", "1" }, true, false, onComplete);
            else
                TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater_py"),
                    new[] { path, _xRotate + "", _yRotate + "", _zRotate + "" }, true, false, onComplete);

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