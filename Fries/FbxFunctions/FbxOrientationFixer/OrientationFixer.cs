using System;
using System.IO;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;
using Fries.TaskPerformer;

namespace Fries.FbxFunctions.FbxOrientationFixer {
    # if UNITY_EDITOR
    public class OrientationFixer : EditorWindow {
        private string CloseAfterFinishKey = $"Fbx_Orientation_Fixer.{SystemUtils.projectName()}.Close_After_Finish";
        private bool _closeAfterFinish;
        private float _xRotate;
        private float _yRotate;
        private float _zRotate;
        
        [MenuItem("Tools/Fries/Fbx/Orientation Fixer")]
        public static void ShowWindow() {
            GetWindow<OrientationFixer>("Orientation Fixer");
        }

        private void OnEnable() {
            _closeAfterFinish = EditorPrefs.GetBool(CloseAfterFinishKey, true);
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _closeAfterFinish = EditorGUILayout.Toggle("Close After Finish", _closeAfterFinish);
            _xRotate = EditorGUILayout.FloatField("X Rotation", _xRotate);
            _yRotate = EditorGUILayout.FloatField("Y Rotation", _yRotate);
            _zRotate = EditorGUILayout.FloatField("Z Rotation", _zRotate);
            
            if (GUILayout.Button("Save")) {
                EditorPrefs.SetBool(CloseAfterFinishKey, _closeAfterFinish);

                EditorApplication.RepaintProjectWindow();
                Debug.Log("Settings Saved!");
            }
            
            if (GUILayout.Button("Fix Rotation")) {
                // 检查是否只选中一个对象
                if (Selection.assetGUIDs.Length == 1) {
                    string relativePath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    // 判断选中的对象是否为有效文件夹
                    if (!string.IsNullOrEmpty(relativePath) && AssetDatabase.IsValidFolder(relativePath)) {
                        string absolutePath = "";
                        // 如果路径以 "Assets" 开头，则用 Application.dataPath 替换 Assets 部分
                        if (relativePath.StartsWith("Assets")) 
                            absolutePath = Application.dataPath + relativePath.Substring("Assets".Length);
                        else absolutePath = Path.GetFullPath(relativePath);
                        absolutePath = $"\"{absolutePath}\"";
                        if (_closeAfterFinish) 
                            TaskPerformer.TaskPerformer.executeExe(getExePath("ZeroRotation"), new []{absolutePath, "1"}, true, false);
                        else TaskPerformer.TaskPerformer.executeExe(getExePath("ZeroRotation"), new []{absolutePath}, true, false);
                    }
                    else Debug.LogWarning("Please select a valid folder!");
                }
                else Debug.LogWarning("Please only select 1 folder!");
            }
            
            if (GUILayout.Button("Rotate and Bake")) {
                // 检查是否只选中一个对象
                if (Selection.assetGUIDs.Length == 1) {
                    string relativePath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    // 判断选中的对象是否为有效文件夹
                    if (!string.IsNullOrEmpty(relativePath) && AssetDatabase.IsValidFolder(relativePath)) {
                        string absolutePath = "";
                        // 如果路径以 "Assets" 开头，则用 Application.dataPath 替换 Assets 部分
                        if (relativePath.StartsWith("Assets")) 
                            absolutePath = Application.dataPath + relativePath.Substring("Assets".Length);
                        else absolutePath = Path.GetFullPath(relativePath);
                        absolutePath = $"\"{absolutePath}\"";
                        if (_closeAfterFinish) 
                            TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater"), new []{absolutePath, _xRotate+"", _yRotate+"", _zRotate+"", "1"}, true, false);
                        else TaskPerformer.TaskPerformer.executeExe(getExePath("Rotater"), new []{absolutePath, _xRotate+"", _yRotate+"", _zRotate+""}, true, false);
                    }
                    else Debug.LogWarning("Please select a valid folder!");
                }
                else Debug.LogWarning("Please only select 1 folder!");
            }
        }

        private string getExePath(string exeName) {
            // 获取当前脚本对应的 MonoScript 对象
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = System.IO.Path.GetDirectoryName(assemblyPath);
            // 拼出 exe 的完整路径
            System.Diagnostics.Debug.Assert(directory != null, nameof(directory) + " != null");
            string exePath = Path.Combine(directory, $"{exeName}.exe");
            return $"\"{exePath}\"";
        }
    }
    # endif
}