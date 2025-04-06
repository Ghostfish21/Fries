namespace Fries.Inspector.ComponentWrapper {
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ComponentWrapper))]
    public class MyCustomComponentDrawer : PropertyDrawer {
        // 计算属性绘制所需的总高度
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            // 如果展开，则增加 component 的详细高度
            if (property.isExpanded) {
                SerializedProperty compProp = property.FindPropertyRelative("component");
                height += EditorGUI.GetPropertyHeight(compProp, label, true);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 开始绘制属性
            EditorGUI.BeginProperty(position, label, property);

            // 绘制带折叠箭头的标签行
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            // 如果展开，则绘制 component 字段的详细信息
            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                SerializedProperty compProp = property.FindPropertyRelative("component");
                Rect compRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                    position.width, EditorGUI.GetPropertyHeight(compProp, label, true));
                EditorGUI.PropertyField(compRect, compProp, true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}