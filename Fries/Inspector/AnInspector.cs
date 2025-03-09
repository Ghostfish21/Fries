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

        private void traverseProperties(SerializedProperty prop) {
                // 处理当前属性（例如检查 FieldAnchorAttribute 并记录 guid 与 propertyPath）
                if (!prop.hasAnnotation(typeof(FieldAnchorAttribute))) return;
                processProperty(prop);

                // 如果当前属性是数组（排除字符串数组），则遍历数组内的每个元素
                if (prop.isArray && prop.propertyType != SerializedPropertyType.String) {
                    for (int i = 0; i < prop.arraySize; i++) {
                        SerializedProperty element = prop.GetArrayElementAtIndex(i);
                        // 对数组元素递归调用，注意这里用 Copy() 防止迭代器干扰
                        traverseProperties(element.Copy());
                    }
                }
                // 否则，如果属性具有可见子属性，则递归遍历这些子属性
                else if (prop.hasVisibleChildren) {
                    SerializedProperty child = prop.Copy();
                    if (child.Next(true)) {
                        do {
                            traverseProperties(child.Copy());
                        } while (child.Next(false));
                    }
                }
        }

        private void processProperty(SerializedProperty prop) {
            // 注意：对于数组中的元素，其 property.name 可能为 "Element 0" 等，
            // 这时通过 target.GetType() 获取 FieldInfo 可能无法获取到，需根据具体情况调整
            FieldInfo field = target.GetType().GetField(prop.name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return;
            if (field.GetCustomAttribute<FieldAnchorAttribute>() == null)
                return;
            SerializableSysObject value = prop.getValue();
            if (value == null) return;
            dict[(MonoBehaviour)target][value.guid] = prop.propertyPath;
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
            if (prop1.Next(true)) {
                do {
                    traverseProperties(prop1);
                } while (prop1.Next(false));
            }
            
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