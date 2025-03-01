# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Fries {
    [InitializeOnLoad]
    public static class EditorHotkeyDetector {
        static EditorHotkeyDetector() {
            // 在 SceneView 绘制 GUI 时调用 OnSceneGUI
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static AddRequest request;
        
        private static void OnSceneGUI(SceneView sceneView) {
            Event e = Event.current;
            // 检测按键按下事件，并判断是否为 F 键
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.PageDown) {
                // 包的 Git 地址，注意这里通常要加上 "git+" 前缀
                string packageUrl = "git+https://github.com/ghostfish21/Fries.git";
                // 使用 Package Manager API 添加/更新包
                request = Client.Add(packageUrl);
                // 注册 EditorApplication.update 来监控请求状态
                EditorApplication.update += progress;
            }
        }

        static void progress() {
            if (request.IsCompleted) {
                if (request.Status == StatusCode.Success) 
                    Debug.Log("Successfully updated package: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure) 
                    Debug.LogError("Failed to update package: " + request.Error.message);

                EditorApplication.update -= progress;
            }
        }
    }
}
# endif