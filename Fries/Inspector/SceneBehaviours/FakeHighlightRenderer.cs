namespace Fries.Inspector.SceneBehaviours {
    # if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [InitializeOnLoad]
    public static class FakeHighlightRenderer {
        public static int currentInstanceId = -1;
        private static Rect? currentDrawRect;
        public static void clear() {
            currentInstanceId = -1;
            currentDrawRect = null;
        }
        
        // 定义一个带透明度的金黄色
        static readonly Color k_GoldTransparent = new Color(1f, 0.95f, 0f, 0.5f);

        static FakeHighlightRenderer() {
            // 注册回调
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            if (currentInstanceId == -1) return;
            if (instanceID != currentInstanceId) return;
            
            // 仅在 Repaint 事件时绘制，保证不会干扰其他 GUI 事件
            if (Event.current.type != EventType.Repaint)
                return;

            // 如果这个行对应的对象被选中了
            if (Selection.instanceIDs.Contains(instanceID)) {
                // 可选：让背景延伸到整行左右边缘
                Rect v = selectionRect;
                v.xMin = 0;
                currentDrawRect = v;
            }

            if (currentDrawRect != null) {
                var v = currentDrawRect.Value;
                v.xMax = EditorGUIUtility.currentViewWidth;
                EditorGUI.DrawRect(v, k_GoldTransparent);
            }
        }
    }
    # endif
}