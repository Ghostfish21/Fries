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

namespace Fries.FbxFunctions.FbxMaterialRenamer {
    # if UNITY_EDITOR
    public class MaterialRenamer : EditorWindow {
        private string CloseAfterFinishKey = $"Fbx_Material_Renamer.{SystemUtils.projectName()}.Close_After_Finish";
        private bool _closeAfterFinish;
        private string _newMatName;

        private GUIStyle style;

        [MenuItem("Tools/Fries/Fbx/Material Renamer")]
        public static void ShowWindow() {
            GetWindow<MaterialRenamer>("Material Renamer");
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
            _newMatName = EditorGUILayout.TextField("Rename as", _newMatName);

            if (GUILayout.Button("Save")) {
                EditorPrefs.SetBool(CloseAfterFinishKey, _closeAfterFinish);
                EditorApplication.RepaintProjectWindow();
                Debug.Log("Settings Saved!");
            }

            if (GUILayout.Button("Rename Material")) {
                if (Selection.assetGUIDs.Length != 1) {
                    Debug.LogWarning("Please select one material that is inside a fbx file!");
                    return;
                }
                
                Material selectedMat = Selection.activeObject as Material;
                if (selectedMat != null) {
                    // 获取材质所在的资源文件路径（如果材质嵌入在 FBX 文件中，该路径就是 FBX 文件的路径）
                    string assetPath = AssetDatabase.GetAssetPath(selectedMat); // 例如 "Assets/Models/MyModel.fbx"
                    // 获取项目根目录的绝对路径（Application.dataPath 指向 Assets 文件夹）
                    string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                    // 组合出绝对路径
                    string fullPath = Path.Combine(projectRoot, assetPath);
                    string materialName = selectedMat.name;

                    if (!fullPath.EndsWith(".fbx") && !fullPath.EndsWith(".FBX")) {
                        Debug.LogWarning("Please select one material that is inside a fbx file!");
                        return;
                    }
                    
                    if (_closeAfterFinish)
                        TaskPerformer.TaskPerformer.executeExe(getExePath("MaterialRenamer_py"),
                            new[] { fullPath, materialName, _newMatName, "1" }, true, false);
                    else
                        TaskPerformer.TaskPerformer.executeExe(getExePath("MaterialRenamer_py"),
                            new[] { fullPath, materialName, _newMatName }, true, false);
                }
                else {
                    Debug.LogWarning("Please select one material that is inside a fbx file!");
                }
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