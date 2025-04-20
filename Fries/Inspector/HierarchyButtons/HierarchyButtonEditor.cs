namespace Fries.Inspector.HierarchyButtons {
# if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// HierarchyButton的自定义编辑器，提供脚本引用和方法选择功能
    /// </summary>
    [CustomEditor(typeof(HierarchyButton))]
    public class HierarchyButtonEditor : Editor {
        private SerializedProperty responseModeProperty;
        private SerializedProperty targetScriptProperty;
        private SerializedProperty selectedMethodNameProperty;
        private SerializedProperty targetMonoProperty;
        private SerializedProperty selectedMethodNameProperty4Mono;

        // 缓存可用的静态方法列表
        private List<MethodInfo> availableMethods = new();
        private string[] methodNames = Array.Empty<string>();
        private int selectedMethodIndex = -1;

        private void OnEnable() {
            responseModeProperty = serializedObject.FindProperty("responseMode");
            targetScriptProperty = serializedObject.FindProperty("_targetScript");
            selectedMethodNameProperty = serializedObject.FindProperty("_selectedMethodName");
            targetMonoProperty = serializedObject.FindProperty("_targetMonoBehaviour");
            selectedMethodNameProperty4Mono = serializedObject.FindProperty("_selectedMethodName4Mono");

            // 初始化方法列表
            HierarchyButton.ResponseMode responseMode =
                (HierarchyButton.ResponseMode)responseModeProperty.enumValueIndex;
            if (responseMode == HierarchyButton.ResponseMode.StaticMethod)
                updateMethodList();
            else updateMethodList(false);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            // 显示响应模式选择
            EditorGUILayout.PropertyField(responseModeProperty, new GUIContent("Response Mode", "Select button's response mode"));

            // 根据选择的响应模式显示不同的选项
            HierarchyButton.ResponseMode responseMode =
                (HierarchyButton.ResponseMode)responseModeProperty.enumValueIndex;

            if (responseMode == HierarchyButton.ResponseMode.StaticMethod) {
                drawStaticMethodMode();
            }
            else if (responseMode == HierarchyButton.ResponseMode.InstanceMethod) {
                drawUnityEventMode();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制静态方法模式的Inspector
        /// </summary>
        private void drawStaticMethodMode() {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            // 显示脚本引用字段
            EditorGUILayout.PropertyField(targetScriptProperty, new GUIContent("Target", "Script that contains target static method"));

            // 如果脚本引用发生变化
            if (EditorGUI.EndChangeCheck()) {
                // 更新方法列表
                updateMethodList();
            }

            // 如果有可用方法，显示方法选择下拉菜单
            if (methodNames.Length > 0) {
                // 查找当前选择的方法索引
                selectedMethodIndex = Array.IndexOf(methodNames, selectedMethodNameProperty.stringValue);
                if (selectedMethodIndex < 0) selectedMethodIndex = 0;

                // 显示方法选择下拉菜单
                int newSelectedIndex = EditorGUILayout.Popup("Method", selectedMethodIndex, methodNames);

                // 如果选择发生变化
                if (newSelectedIndex != selectedMethodIndex) {
                    selectedMethodIndex = newSelectedIndex;
                    selectedMethodNameProperty.stringValue = methodNames[selectedMethodIndex];
                }
            }
            else if (targetScriptProperty.objectReferenceValue != null) {
                EditorGUILayout.HelpBox("No valid static method found in the selected script.\nThe static method must has a Event parameter!", MessageType.Warning);
            }
        }

        /// <summary>
        /// 绘制UnityEvent模式的Inspector
        /// </summary>
        private void drawUnityEventMode() {
            EditorGUILayout.Space();

            // 显示脚本引用字段
            EditorGUILayout.PropertyField(targetMonoProperty, new GUIContent("Target MonoBehaviour", "Script that contains target method"));

            // 如果脚本引用发生变化
            if (EditorGUI.EndChangeCheck()) {
                // 更新方法列表
                updateMethodList(false);
            }

            // 如果有可用方法，显示方法选择下拉菜单
            if (methodNames.Length > 0) {
                // 查找当前选择的方法索引
                selectedMethodIndex = Array.IndexOf(methodNames, selectedMethodNameProperty.stringValue);
                if (selectedMethodIndex < 0) selectedMethodIndex = 0;

                // 显示方法选择下拉菜单
                int newSelectedIndex = EditorGUILayout.Popup("Method", selectedMethodIndex, methodNames);

                // 如果选择发生变化
                if (newSelectedIndex != selectedMethodIndex) {
                    selectedMethodIndex = newSelectedIndex;
                    selectedMethodNameProperty4Mono.stringValue = methodNames[selectedMethodIndex];
                }
            }
            else if (targetMonoProperty.objectReferenceValue != null) {
                EditorGUILayout.HelpBox("No valid method found in the selected script.\nThe method must has a Event parameter!", MessageType.Warning);
            }
        }

        /// <summary>
        /// 更新可用的静态方法列表
        /// </summary>
        private void updateMethodList(bool isStatic = true) {
            availableMethods.Clear();

            
            // 获取所有公共静态方法
            MethodInfo[] methods;
            if (isStatic) {
                MonoScript script = targetScriptProperty.objectReferenceValue as MonoScript;
                if (script == null) return;
                Type scriptType = script.GetClass();
                if (scriptType == null) return;
                methods = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            }
            else {
                MonoBehaviour script = targetMonoProperty.objectReferenceValue as MonoBehaviour;
                if (script == null) return;
                Type scriptType = script.GetType();
                methods = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            }

            // 筛选具有鼠标事件参数的方法
            foreach (MethodInfo method in methods) {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0 && isMouseEventParameter(parameters[0].ParameterType)) {
                    availableMethods.Add(method);
                }
            }

            // 更新方法名称数组
            methodNames = availableMethods.Select(m => m.Name).ToArray();

            if (isStatic) {
                // 如果当前选择的方法不在列表中，清空选择
                if (methodNames.Length > 0 && !methodNames.Contains(selectedMethodNameProperty.stringValue)) 
                    selectedMethodNameProperty.stringValue = methodNames[0];
                else if (methodNames.Length == 0) 
                    selectedMethodNameProperty.stringValue = "";
            }
            else {
                if (methodNames.Length > 0 && !methodNames.Contains(selectedMethodNameProperty4Mono.stringValue)) 
                    selectedMethodNameProperty4Mono.stringValue = methodNames[0];
                else if (methodNames.Length == 0)
                    selectedMethodNameProperty4Mono.stringValue = "";
            }
        }

        /// <summary>
        /// 检查参数类型是否为鼠标事件类型
        /// </summary>
        private bool isMouseEventParameter(Type parameterType) {
            // 检查是否为Event类型或其子类
            if (parameterType == typeof(Event) || parameterType.IsSubclassOf(typeof(Event)))
                return true;
            return false;
        }
    }
# endif
}