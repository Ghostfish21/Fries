﻿using System;
using System.Linq;

namespace Fries.Inspector.SceneBehaviours {
    # if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;

    public static class ProjectWindowHelper {
        public static string GetCurrentProjectBrowserFolder() {
            // 调用隐藏的 ProjectWindowUtil.TryGetActiveFolderPath(out string path)
            var pwType = typeof(ProjectWindowUtil);
            var method = pwType.GetMethod(
                "TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            object[] args = { null };
            bool found = (bool)method.Invoke(null, args);
            var folder = args[0] as string;
            return found && !string.IsNullOrEmpty(folder) ? folder : "Assets";
        }
        
        public static void ShowFolderByReflection(string folderPath) {
            // 获取 ProjectBrowser 实例
            var pbType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var all = Resources
                .FindObjectsOfTypeAll(pbType)
                .OfType<EditorWindow>()
                .FirstOrDefault(w => w.titleContent.text == "Project");

            EditorWindow pb = all != null 
                ? all 
                : EditorWindow.GetWindow(pbType, false, "Project", true);

            // 寻找内部方法：ShowFolderContentsAtPath 或 ShowFolderContents
            var method = pbType.GetMethod("ShowFolderContentsAtPath", BindingFlags.Instance | BindingFlags.NonPublic)
                         ?? pbType.GetMethod("ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);

            if (method == null) {
                Debug.LogError("Unable to obtain ProjectBrowser.ShowFolderContentsAtPath method!");
                return;
            }

            var getInstanceIDMethod = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID",
                BindingFlags.Static | BindingFlags.NonPublic);
            int instanceID = (int)getInstanceIDMethod.Invoke(null, new object[] { folderPath });
            try {
                method.Invoke(pb, new object[] { instanceID, true });
            }
            catch (Exception e) {
                // ignored
            }
        }
    }
    # endif
}