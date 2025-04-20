# if UNITY_EDITOR
namespace Fries.Inspector.HierarchyButtons {
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// 自定义HierarchyButton在Hierarchy面板中的显示样式
    /// </summary>
    [InitializeOnLoad]
    public class HierarchyButtonDrawer {
        // 存储所有HierarchyButton组件的GameObject的ID
        private static Dictionary<int, HierarchyButton> hierarchyButtonInstances = new();

        // 静态构造函数，在编辑器加载时调用
        static HierarchyButtonDrawer() {
            EditorApplication.hierarchyWindowItemOnGUI += onHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyChanged += onHierarchyChanged;

            // 初始化时收集所有HierarchyButton实例
            collectHierarchyButtonInstances();
        }

        // 当Hierarchy视图变化时，重新收集HierarchyButton实例
        private static void onHierarchyChanged() {
            collectHierarchyButtonInstances();
        }

        // 收集场景中所有的HierarchyButton实例
        private static void collectHierarchyButtonInstances() {
            hierarchyButtonInstances.Clear();
            HierarchyButton[] instances = UnityEngine.Object.FindObjectsOfType<HierarchyButton>();

            foreach (HierarchyButton instance in instances) {
                if (instance != null && instance.gameObject != null) {
                    int instanceID = instance.gameObject.GetInstanceID();
                    hierarchyButtonInstances[instanceID] = instance;
                }
            }
        }

        // 自定义Hierarchy项的GUI绘制
        private static void onHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            // 检查当前项是否是HierarchyButton
            if (hierarchyButtonInstances.ContainsKey(instanceID)) {
                HierarchyButton hierarchyButton = hierarchyButtonInstances[instanceID];

                if (hierarchyButton != null) {
                    // 根据响应模式绘制不同的背景
                    if (hierarchyButton.mode == HierarchyButton.ResponseMode.StaticMethod) {
                        if (hierarchyButton.targetScript != null) {
                            drawCustomBackground(selectionRect, hierarchyButton, true);
                        }
                        else {
                            drawCustomBackground(selectionRect, hierarchyButton, false);
                        }
                    }
                    else // UnityEvent模式
                    {
                        // 绘制UnityEvent模式的背景
                        drawCustomBackground(selectionRect, hierarchyButton, true);
                    }

                    // 处理鼠标事件
                    handleMouseEvents(instanceID, selectionRect, hierarchyButton);
                }
            }
        }

        // 绘制自定义背景
        private static void drawCustomBackground(Rect selectionRect, HierarchyButton hierarchyButton, bool isActive) {
            // 创建背景区域
            Rect backgroundRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.height);

            // 根据模式选择不同的颜色
            if (hierarchyButton.mode == HierarchyButton.ResponseMode.StaticMethod) {
                // 静态方法模式 - 蓝色
                GUI.color = isActive ? new Color(0.3f, 0.5f, 0.9f, 0.6f) : new Color(0.3f, 0.3f, 0.5f, 0.4f);
            }
            else {
                // UnityEvent模式 - 绿色
                GUI.color = new Color(0.3f, 0.8f, 0.4f, 0.6f);
            }

            // 绘制按钮样式背景
            GUI.Box(backgroundRect, GUIContent.none, EditorStyles.helpBox);

            // 恢复GUI颜色
            GUI.color = Color.white;
            
            // 绘制按钮图标
            // Rect iconRect = new Rect(backgroundRect.xMax - 20, backgroundRect.y + 2, 16, 16);
            // GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("d_PlayButton").image);
        }

        // 处理鼠标事件
        private static void handleMouseEvents(int instanceID, Rect selectionRect, HierarchyButton hierarchyButton) {
            Event evt = Event.current;

            if (evt.type == EventType.MouseDown && selectionRect.Contains(evt.mousePosition)) {
                // 选择对象
                Selection.instanceIDs = new int[] { instanceID };

                // 如果是左键点击，执行方法或触发事件
                if (evt.button == 0) {
                    if (hierarchyButton.mode == HierarchyButton.ResponseMode.StaticMethod) {
                        // 静态方法模式
                        if (!string.IsNullOrEmpty(hierarchyButton.selectedMethodName) &&
                            hierarchyButton.targetScript != null) {
                            // 尝试执行选择的静态方法
                            invokeSelectedMethod(hierarchyButton, evt);
                        }
                    }
                    else {
                        // UnityEvent模式
                        hierarchyButton.triggerButtonClick(evt);
                    }

                    // 消耗事件
                    evt.Use();
                }
            }
        }

        // 执行选择的静态方法
        private static void invokeSelectedMethod(HierarchyButton hierarchyButton, Event evt) {
            if (hierarchyButton.targetScript == null) return;

            Type scriptType = hierarchyButton.targetScript.GetClass();

            MethodInfo method = scriptType.GetMethod(hierarchyButton.selectedMethodName);
            try {
                // 调用静态方法，传入鼠标事件
                method.Invoke(null, new object[] { evt });
            }
            catch (Exception ex) {
                Debug.LogError($"Catch error when executing {scriptType.Name}.{(method == null ? "null" : method.Name)}: {ex.Message}");
            }
        }

        // 检查参数类型是否为鼠标事件类型
        private static bool isMouseEventParameter(Type parameterType) {
            // 检查是否为Event类型或其子类
            if (parameterType == typeof(Event) || parameterType.IsSubclassOf(typeof(Event)))
                return true;
            return false;
        }
    }

}
# endif