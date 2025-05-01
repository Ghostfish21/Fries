using System.Collections.Generic;

namespace Fries.Inspector.SceneBehaviours {
    # if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;

    public static class InspectorUtil {
        public static void showInInspector(Object obj) {
            // 取到 UnityEditor.InspectorWindow 类型
            var asm = typeof(EditorWindow).Assembly;
            var inspType = asm.GetType("UnityEditor.InspectorWindow");
            if (inspType == null) {
                Debug.LogError("没找到 InspectorWindow 类型，Unity 版本可能不支持此方法。");
                return;
            }
            var window = EditorWindow.GetWindow(inspType);

            // 找到内部的 SetTargets（或 SetObjects）方法
            var setLocked = inspType.GetMethod("SetObjectsLocked", BindingFlags.Instance | BindingFlags.NonPublic);
            if (setLocked == null) {
                Debug.LogError("Unable to find setLocked method, current Unity version doesn't support the operation");
                return;
            }
            var list = new List<Object> { obj };
            setLocked.Invoke(window, new object[] { list });

            // 强制重绘
            var inspectorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorType == null)
                return;
            var inspectors = Resources.FindObjectsOfTypeAll(inspectorType);
            foreach (var o in inspectors) {
                var inspector = (EditorWindow)o;
                // 对根 VisualElement 标记脏，UI Toolkit 会在下一帧重绘它
                inspector.rootVisualElement.MarkDirtyRepaint();
            }

            EditorApplication.delayCall += UnityEditorInternal.InternalEditorUtility.RepaintAllViews;
        }

        public static void cancelLock() {
            var asm = typeof(EditorWindow).Assembly;
            var inspType = asm.GetType("UnityEditor.InspectorWindow");
            if (inspType == null) {
                Debug.LogError("Unable to find InspectorWindow Type, current Unity version doesn't support the operation");
                return;
            }

            var window = EditorWindow.GetWindow(inspType);
            PropertyInfo propLocked = inspType.GetProperty("isLocked",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            propLocked.SetValue(window, false, null);
        }
    }
    # endif
}