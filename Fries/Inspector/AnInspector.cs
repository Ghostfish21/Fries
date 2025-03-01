#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Inspector.GameObjectBoxField;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Fries.Inspector {
    
    public class AnInspector : Editor {
        
        private static readonly Dictionary<MonoBehaviour, Dictionary<string, string>> dict = new();

        public static string getCachedPropertyPath(MonoBehaviour fromMono, SerializableSysObject ofObj) {
            var v = dict[fromMono];
            var v1 = v[ofObj.guid];
            return v1;
        }
        
        private SerializedObject serializedObj;

        private void traverseProperties(SerializedProperty property) {
            do {
                // 在这里你可以处理每个 property，比如判断是否有 FieldAnchorAttribute 之类的操作
                Debug.Log(property.propertyPath);

                // 如果当前属性有子属性（例如复合类型或数组），则递归遍历其子属性
                if (property.hasVisibleChildren && property.isExpanded) {
                    SerializedProperty child = property.Copy();
                    // 进入子级：这里用 Next(true) 表示进入第一个子元素
                    if (child.Next(true)) {
                        traverseProperties(child);
                    }
                }
            } while (property.Next(false)); // Next(false) 遍历同级的下一个属性
        }

        public override void OnInspectorGUI() {
            // 获取 Target Type 实例
            serializedObj = new SerializedObject(target);
            Type type = target.GetType();

            // 获取 Target 类型的所有属性
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (!dict.ContainsKey((MonoBehaviour)target)) 
                dict[(MonoBehaviour)target] = new();
            dict[(MonoBehaviour)target].Clear();
            
            // 使用 GetIterator() 获取根属性迭代器，并用 Next(true) 遍历所有层级的属性
            SerializedProperty prop1 = serializedObject.GetIterator();
            if (prop1.Next(true))  // 进入第一个子级
                traverseProperties(prop1);
            
            // 开始检测值变化
            EditorGUI.BeginChangeCheck();
            
            foreach (var field in fields) {
                SerializedProperty prop = serializedObj.FindProperty(field.Name);
                
                // 检查属性是否为布尔型
                if (field.FieldType == typeof(UnityEvent) || field.FieldType == typeof(Action)) {
                    // 检测这个属性上有没有 YureiButton Attribute
                    AButtonAttribute attr = field.GetCustomAttribute<AButtonAttribute>();
                    if (attr != null) {
                        // 如果有 YureiButton Attribute，则绘制一个按钮
                        string name = field.Name;
                        if (attr.text != null) name = attr.text;

                        if (field.FieldType == typeof(UnityEvent) && !Application.isPlaying) 
                            name = $"{name} (Require to start the game)";
                        
                        if (GUILayout.Button(name)) {
                            // 如果按钮被按下，则执行 UnityEvent
                            if (field.FieldType == typeof(UnityEvent))
                                ((UnityEvent)field.GetValue(target)).Invoke();
                            else {
                                try {
                                    ((Action)field.GetValue(target)).Invoke();
                                }
                                catch (Exception) {
                                    MethodInfo startMethod = target.GetType().GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                    if (startMethod != null && startMethod.GetParameters().Length == 0) {
                                        startMethod.Invoke(target, null);
                                        ((Action)field.GetValue(target)).Invoke();
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 绘制当前值的输入框或其他交互框
                if (field.GetCustomAttribute<IgnoreInInspectorAttribute>() == null)
                    EditorGUILayout.PropertyField(prop, true);
            }
            
            // 结束检测值变化
            EditorGUI.EndChangeCheck();
            serializedObj.ApplyModifiedProperties();
        }
    }

    
}

#endif