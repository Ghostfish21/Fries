
# if UNITY_EDITOR
namespace Fries.Inspector.ComponentWrapper {
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ComponentWrapper))]
    public class MyCustomComponentDrawer : PropertyDrawer {
        private Editor cachedEditor;
        
        // 计算整个绘制区域所需的总高度
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight; // 折叠行高度
            if (property.isExpanded) {
                SerializedProperty compProp = property.FindPropertyRelative("component");
                Component comp = compProp.objectReferenceValue as Component;
                if (comp != null) {
                    // // 创建序列化对象，用于遍历并计算组件内部各属性的高度
                    // SerializedObject serializedComp = new SerializedObject(comp);
                    // SerializedProperty prop = serializedComp.GetIterator();
                    // if (prop.NextVisible(true)) {
                    //     do {
                    //         // 跳过 m_Script 字段（不可修改）
                    //         if (prop.name == "m_Script") continue;
                    //         height += EditorGUI.GetPropertyHeight(prop, true)
                    //                   + EditorGUIUtility.standardVerticalSpacing;
                    //     } while (prop.NextVisible(false));
                    // }
                }
                else {
                    // 如果组件为空，则为显示可拖入的对象框预留一行高度
                    height += EditorGUIUtility.singleLineHeight;
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            // 绘制折叠箭头及标签行
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                SerializedProperty compProp = property.FindPropertyRelative("component");
                Component comp = compProp.objectReferenceValue as Component;
                float yOffset = position.y + EditorGUIUtility.singleLineHeight;
                if (comp != null) {
                    // 如果 component 不为空，创建其序列化对象并绘制内部所有可见属性
                    SerializedObject serializedComp = new SerializedObject(comp);
                    serializedComp.Update();
                    
                    // 创建或更新缓存的Editor
                    if (cachedEditor == null || cachedEditor.target != compProp.objectReferenceValue) 
                        Editor.CreateCachedEditor(compProp.objectReferenceValue, null, ref cachedEditor);
                    Rect compRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, 100);
                    cachedEditor.OnInspectorGUI();
                    
                    serializedComp.ApplyModifiedProperties();
                }
                else {
                    // 如果 component 为空，则显示一个对象拖拽框供用户赋值
                    Rect objectFieldRect = new Rect(position.x, yOffset, position.width,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(objectFieldRect, compProp, new GUIContent("Component"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
# endif