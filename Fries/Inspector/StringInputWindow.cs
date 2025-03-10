# if UNITY_EDITOR
using Fries.Inspector.GameObjectBoxField;
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector {
    public class StringInputWindow : EditorWindow {
        private bool sizeFlag = false;
        private int controlID;
        private SerializedProperty property;
        private PickerData data;

        // 输入框内容，初始值为原有的字符串值
        private string inputText = "";

        /// <summary>
        /// 初始化窗口
        /// </summary>
        /// <param name="controlID">选择器 ID</param>
        /// <param name="property">目标 SerializedProperty（类型为 string）</param>
        /// <param name="data">相关 PickerData 数据</param>
        public void Init(int controlID, SerializedProperty property, PickerData data) {
            this.controlID = controlID;
            this.property = property;
            this.data = data;
            // 从 property 获取原有的字符串值
            inputText = "";
            if (property.boxedValue != null)
                inputText = property.getValue().get<string>();
        }

        private void OnGUI() {
            // 显示输入框，预填原有的字符串值
            int lineCount = Mathf.Max(1, inputText.Split('\n').Length);
            float textAreaHeight = EditorGUIUtility.singleLineHeight * lineCount + 10;
            inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(textAreaHeight));

            // 提交按钮
            if (GUILayout.Button("Submit")) {
                property.managedReferenceValue = new StringSso(inputText);
                property.serializedObject.ApplyModifiedProperties();
                Close();
            }
            
            
                // 这里可以使用 GUILayoutUtility.GetLastRect() 或者其他方法来估算内容尺寸
                // 例如，这里取最后一个布局元素的 Rect，并加上一些额外的 margin
                Rect contentRect = GUILayoutUtility.GetLastRect();
                // 如果内容 Rect 过小，设置一个最小尺寸
                float width = Mathf.Max(300, contentRect.x + contentRect.width + 20);
                float height = contentRect.y + EditorGUIUtility.singleLineHeight;
                // 设置窗口最小和最大尺寸为同样的值，达到固定窗口大小的目的
                this.minSize = new Vector2(width, height);
                this.maxSize = new Vector2(width, height);
                sizeFlag = true;
            
        }
    }
}
# endif