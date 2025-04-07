# if UNITY_EDITOR
using Fries.Inspector.GameObjectBoxField;
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector {
    public class StringInputWindow : EditorWindow {
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
                inputText = property.getSsoValue().get<string>();
        }

        private void OnGUI() {
            GUIStyle textAreaStyle = EditorStyles.textArea;
            // 先将文本按换行符分割，计算每一行的宽度，取最大值
            string[] lines = inputText.Split('\n');
            float maxLineWidth = 0;
            foreach (string line in lines) {
                Vector2 lineSize = textAreaStyle.CalcSize(new GUIContent(line));
                if (lineSize.x > maxLineWidth)
                    maxLineWidth = lineSize.x;
            }
            // 为宽度添加一点边距
            float horizontalMargin = 20f;
            float textAreaWidth = maxLineWidth + horizontalMargin;
            textAreaWidth = Mathf.Max(200, textAreaWidth);
            textAreaWidth = Mathf.Min(1000, textAreaWidth);
            // 根据计算出的宽度获取合适的高度
            // 注意这里 CalcHeight 会根据文本在给定宽度下的换行情况返回需要的高度
            float textAreaHeight = textAreaStyle.CalcHeight(new GUIContent(inputText), textAreaWidth) + 10f;
            // 绘制 TextArea，并使用计算出的宽度和高度
            inputText = EditorGUILayout.TextArea(inputText, textAreaStyle, GUILayout.Width(textAreaWidth), GUILayout.Height(textAreaHeight));

            // 提交按钮
            if (GUILayout.Button("Submit")) {
                property.managedReferenceValue = new StringSso(inputText);
                property.serializedObject.ApplyModifiedProperties();
                Close();
            }
            
            if (Event.current.type == EventType.Repaint) {
                // 这里可以使用 GUILayoutUtility.GetLastRect() 或者其他方法来估算内容尺寸
                // 例如，这里取最后一个布局元素的 Rect，并加上一些额外的 margin
                Rect contentRect = GUILayoutUtility.GetLastRect();
                // 如果内容 Rect 过小，设置一个最小尺寸
                float width = textAreaWidth + 5;
                float height = contentRect.y + EditorGUIUtility.singleLineHeight;
                // 设置窗口最小和最大尺寸为同样的值，达到固定窗口大小的目的
                this.minSize = new Vector2(width, height);
                this.maxSize = new Vector2(width, height);
            }
        }
    }
}
# endif