# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Fries {
    [InitializeOnLoad]
    public static class EditorHotkeyDetector {

        private static AddRequest request;

        [Shortcut("Fries/Update", KeyCode.F12, ShortcutModifiers.Action)]
        public static void create() {
            // 包的 Git 地址，注意这里通常要加上 "git+" 前缀
            string packageUrl = "git+https://github.com/ghostfish21/Fries.git";
            // 使用 Package Manager API 添加/更新包
            request = Client.Add(packageUrl);
            // 注册 EditorApplication.update 来监控请求状态
            EditorApplication.update += progress;
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