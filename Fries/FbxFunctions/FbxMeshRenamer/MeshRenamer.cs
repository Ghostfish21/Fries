using System.IO;
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
                if (Selection.assetGUIDs.Length != 1) {
                    Debug.LogWarning("Please select one fbx file!");
                    return;
                }

                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!path.EndsWith(".fbx") && !path.EndsWith(".FBX")) {
                    Debug.LogWarning("Please select one fbx file!");
                    return;
                }

                // 获取项目根目录的绝对路径（Application.dataPath 指向 Assets 文件夹）
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                // 组合出绝对路径
                System.Diagnostics.Debug.Assert(projectRoot != null, nameof(projectRoot) + " != null");
                string fullPath = Path.Combine(projectRoot, path);
                fullPath = $"\"{fullPath}\"";

                if (_closeAfterFinish)
                    TaskPerformer.TaskPerformer.executeExe(getExePath("MeshRenamer_py"),
                        new[] { fullPath, "1" }, true, false);
                else
                    TaskPerformer.TaskPerformer.executeExe(getExePath("MeshRenamer_py"),
                        new[] { fullPath }, true, false);
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