namespace Fries.Inspector.GameObjectBoxField {
    using UnityEngine;
    
    # if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(GameObjectBoxes), true)]
    public class GameObjectBoxesDrawer : PropertyDrawer {
        // 用于记录当前正在拖拽的元素的索引，-1 表示没有正在拖拽
        private int draggingIndex = -1;

        private bool isDragging = false;

        // 记录拖拽开始时的鼠标位置（可用于计算偏移）
        private static Vector2 dragStartPos;

        private float draggingY;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            GameObjectBoxes gob = (GameObjectBoxes)property.getValue();
            
            // 获取 list 属性
            SerializedProperty listProp = property.FindPropertyRelative("list");

            if (listProp == null) {
                EditorGUI.LabelField(position, label.text, "Missing 'list' property");
                EditorGUI.EndProperty();
                return;
            }

            // 预留两个按钮区域，宽度均为一行高度
            float buttonWidth = EditorGUIUtility.singleLineHeight;
            Rect addRect = new Rect(position.x, position.y, buttonWidth, buttonWidth);
            Rect delRect = new Rect(position.x + buttonWidth, position.y, buttonWidth, buttonWidth);

            // 计算列表元素区域起始位置
            float listStartX = position.x + buttonWidth * 2 + 4; // 多预留一点间距
            float elementSize = EditorGUIUtility.singleLineHeight;
            float spacing = 4f;

            Event evt = Event.current;

            // 绘制添加按钮：在数组末尾添加新元素
            if (GUI.Button(addRect, "+")) {
                int newIndex = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(newIndex);
                property.serializedObject.ApplyModifiedProperties();
            }

            // 绘制删除按钮：点击时删除列表中最后一个元素（非拖拽）
            if (GUI.Button(delRect, "-") && !isDragging) {
                if (listProp.arraySize > 0) {
                    listProp.DeleteArrayElementAtIndex(listProp.arraySize - 1);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // 绘制所有列表元素（从左到右排列，不需要展开列表）
            for (int i = 0; i < listProp.arraySize; i++) {
                // 每个元素的矩形区域
                Rect elementRect = new Rect(listStartX + i * (elementSize + spacing), position.y, elementSize,
                    elementSize);
                Rect dragRect = new Rect(listStartX + i * (elementSize + spacing) - spacing/2, position.y, elementSize + spacing,
                    elementSize);

                // 如果当前元素正处于拖拽中，则在列表中绘制一个空白占位（也可绘制一个淡色背景作为提示）
                if (isDragging && i == draggingIndex) {
                    EditorGUI.DrawRect(elementRect, new Color(0.8f, 0.8f, 0.8f, 0.5f));
                }
                else {
                    // 正常绘制该元素（调用对应类型的 PropertyDrawer）
                    SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(elementRect, elementProp, GUIContent.none);
                }
                
                // 处理鼠标事件：点击开始拖拽
                if (evt.type == EventType.MouseDown && dragRect.Contains(evt.mousePosition)) {
                    draggingY = position.y;
                    draggingIndex = i;
                    dragStartPos = evt.mousePosition;
                    // 这里不立即标记为拖拽，等待 MouseDrag 事件判断
                    evt.Use();
                }
            }

            // 当鼠标拖拽时，若拖拽距离超过一定阈值则启动拖拽效果
            if (evt.type == EventType.MouseDrag && draggingIndex != -1 && draggingY == position.y) {
                // 启动拖拽状态
                isDragging = true;
                evt.Use();
            }

            // 如果处于拖拽状态，则在鼠标位置绘制拖拽的元素预览
            if (isDragging && draggingIndex != -1 && draggingY == position.y) {
                Rect ghostRect = new Rect(evt.mousePosition.x - elementSize / 2, evt.mousePosition.y - elementSize / 2,
                    elementSize, elementSize);
                // 绘制一个半透明背景
                EditorGUI.DrawRect(ghostRect, new Color(1f, 1f, 1f, 0.5f));
                // 绘制被拖拽的元素内容
                SerializedProperty draggedProp = listProp.GetArrayElementAtIndex(draggingIndex);
                EditorGUI.PropertyField(ghostRect, draggedProp, GUIContent.none);
            }

            // 处理鼠标松开事件，完成拖拽操作
            if (evt.type == EventType.MouseUp && isDragging && draggingY == position.y) {
                // 如果鼠标松开位置在减号按钮区域，则删除被拖拽的元素
                if (delRect.Contains(evt.mousePosition)) {
                    listProp.DeleteArrayElementAtIndex(draggingIndex);
                }
                // 否则如果鼠标松开位置在列表区域，则计算插入位置进行排序调整
                else if (evt.mousePosition.x >= listStartX) {
                    // 根据鼠标 x 坐标计算目标索引
                    int dropIndex =
                        Mathf.FloorToInt((evt.mousePosition.x - listStartX) / (elementSize + spacing) + 0.5f);
                    dropIndex = Mathf.Clamp(dropIndex, 0, listProp.arraySize - 1);
                    if (dropIndex != draggingIndex) {
                        // 移动数组中的元素（注意：MoveArrayElement 内部会自动处理索引调整）
                        listProp.MoveArrayElement(draggingIndex, dropIndex);
                    }
                }

                property.serializedObject.ApplyModifiedProperties();
                // 重置拖拽状态
                draggingIndex = -1;
                isDragging = false;
                evt.Use();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // 高度固定为一行
            return EditorGUIUtility.singleLineHeight;
        }
    }
    
    # endif
}