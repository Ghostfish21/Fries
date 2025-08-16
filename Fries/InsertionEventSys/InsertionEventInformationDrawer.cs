namespace Fries.InsertionEventSys {
// Assets/Editor/InsertionEventInformationDrawer.cs
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

// 你的数据类（这里仅展示声明，实际可放在任意运行时代码处）
// public class InsertionEventInformation {
//     public Type insertedClass;
//     public string eventName;
//     public Type[] argsTypes;
//     public List<(Type listenFrom, string methodName)> listeners;
// }

    [CustomPropertyDrawer(typeof(InsertionEventInformation))]
    public class InsertionEventInformationDrawer : PropertyDrawer {
        // 样式 & 常量
        const float HPad = 4f;
        const float VPad = 2f;
        const float ColGap = 6f;
        const float TokenGap = 8f;

        static readonly Color kGrayBg = new Color(0.94f, 0.94f, 0.94f);

        static GUIStyle _tokenStyle;

        static GUIStyle TokenStyle {
            get {
                if (_tokenStyle == null) {
                    _tokenStyle = new GUIStyle(EditorStyles.label) {
                        wordWrap = false,
                        clipping = TextClipping.Overflow,
                        alignment = TextAnchor.MiddleLeft,
                        richText = false,
                        fontSize = EditorStyles.label.fontSize
                    };
                }

                return _tokenStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 准备
            EditorGUI.BeginProperty(position, label, property);
            var lineH = EditorGUIUtility.singleLineHeight;
            float y = position.y;

            // 取得实际对象（支持嵌套 / 列表 / SerializeReference）
            var info = GetTargetObjectOfProperty(property) as InsertionEventInformation;
            if (info == null) {
                EditorGUI.LabelField(position, label, new GUIContent("(null)"));
                EditorGUI.EndProperty();
                return;
            }

            // 安全取值
            string insertedName = TypeToNiceName(info.insertedClass);
            string eventName = info.eventName ?? "null";
            var args = info.argsTypes ?? Array.Empty<Type>();
            var listeners = info.listeners ?? new List<(Type listenFrom, string methodName)>();

            // ------- 第一行：Header（带 Foldout） -------
            var full = new Rect(position.x, y, position.width, lineH);
            // 左侧留出 foldout 宽度
            var foldRect = new Rect(full.x, full.y, 16f, lineH);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);

            // 三列布局
            float usableW = full.width - 16f;
            float colW = usableW / 3f;
            var col1 = new Rect(foldRect.xMax, y, colW - ColGap, lineH);
            var col2 = new Rect(col1.xMax + ColGap, y, colW - ColGap, lineH);
            var col3 = new Rect(col2.xMax + ColGap, y, usableW - (colW * 2) - (ColGap * 2), lineH);

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUI.LabelField(col1, $"{insertedName} (script)");
                EditorGUI.LabelField(col2, $"{eventName} (string)");
                EditorGUI.LabelField(col3, $"Listeners: {listeners.Count} (int)");
            }

            y += lineH + VPad;

            // ------- 灰底内容区（从第二行开始） -------
            // 先计算内容总高度以绘制背景
            int argRows = CalcArgRows(args, position.width - HPad * 2);
            float argsHeight = Mathf.Max(1, argRows) * lineH;

            float listenersHeight = 0f;
            if (property.isExpanded && listeners.Count > 0) {
                // 每个监听器一行
                listenersHeight = listeners.Count * (lineH + VPad);
            }

            float contentHeight = argsHeight + VPad + listenersHeight;
            var bgRect = new Rect(position.x, y, position.width, contentHeight + VPad);
            EditorGUI.DrawRect(bgRect, kGrayBg);

            // ------- 第二行：按 token 显示 args（自动换行） -------
            var argsStart = new Rect(position.x + HPad, y + VPad, position.width - HPad * 2, lineH);
            DrawArgsTokens(argsStart, args);

            y += argsHeight + VPad;

            // ------- 展开后：监听器列表（从第三行开始） -------
            if (property.isExpanded && listeners.Count > 0) {
                float leftW = Mathf.Round((position.width - HPad * 2) * 0.6f);
                for (int i = 0; i < listeners.Count; i++) {
                    var row = new Rect(position.x + HPad, y + VPad, position.width - HPad * 2, lineH);
                    var left = new Rect(row.x, row.y, leftW, lineH);
                    var right = new Rect(row.x + leftW + ColGap, row.y, row.width - leftW - ColGap, lineH);

                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUI.LabelField(left, $"{TypeToNiceName(listeners[i].listenFrom)} (script)");
                        EditorGUI.LabelField(right, $"{(listeners[i].methodName ?? "null")} (string)");
                    }

                    y += (lineH + VPad);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var lineH = EditorGUIUtility.singleLineHeight;
            float height = lineH + VPad; // header

            var info = GetTargetObjectOfProperty(property) as InsertionEventInformation;
            if (info == null) return height + lineH; // 最少再给一行

            var args = info.argsTypes ?? Array.Empty<Type>();
            var listeners = info.listeners ?? new List<(Type listenFrom, string methodName)>();

            // 估算可用宽度（和 OnGUI 接近）
            float width = Mathf.Max(100f, EditorGUIUtility.currentViewWidth - 40f);
            int argRows = CalcArgRows(args, width - HPad * 2);
            height += Mathf.Max(1, argRows) * lineH + VPad; // args

            if (property.isExpanded && listeners.Count > 0) {
                height += listeners.Count * (lineH + VPad);
            }
            else {
                height += VPad;
            }

            return height;
        }

        // -------- 绘制/计算 辅助 --------

        void DrawArgsTokens(Rect startRect, Type[] args) {
            var lineH = EditorGUIUtility.singleLineHeight;
            float x = startRect.x;
            float y = startRect.y;
            float maxX = startRect.x + startRect.width;

            using (new EditorGUI.DisabledScope(true)) {
                if (args == null || args.Length == 0) {
                    EditorGUI.LabelField(new Rect(x, y, startRect.width, lineH), "(no arguments)");
                    return;
                }

                for (int i = 0; i < args.Length; i++) {
                    string txt = $"{TypeToNiceName(args[i])} (script)";
                    Vector2 sz = TokenStyle.CalcSize(new GUIContent(txt));

                    if (x + sz.x > maxX) {
                        // 换行
                        x = startRect.x;
                        y += lineH;
                    }

                    var r = new Rect(x, y, sz.x, lineH);
                    EditorGUI.LabelField(r, txt, TokenStyle);
                    x += sz.x + TokenGap;
                }
            }
        }

        static int CalcArgRows(Type[] args, float width) {
            var lineH = EditorGUIUtility.singleLineHeight;
            if (args == null || args.Length == 0) return 1;

            float x = 0f;
            int rows = 1;
            foreach (var t in args) {
                string txt = $"{TypeToNiceName(t)} (script)";
                Vector2 sz = TokenStyle.CalcSize(new GUIContent(txt));
                if (x > 0f && x + sz.x > width) {
                    rows++;
                    x = 0f;
                }

                x += sz.x + TokenGap;
            }

            return Mathf.Max(1, rows);
        }

        static string TypeToNiceName(Type t) {
            if (t == null) return "null";
            if (!t.IsGenericType) return t.Name;
            string name = t.Name;
            int tick = name.IndexOf('`');
            if (tick >= 0) name = name.Substring(0, tick);
            var args = t.GetGenericArguments().Select(TypeToNiceName);
            return $"{name}<{string.Join(", ", args)}>";
        }

        // -------- SerializedProperty → 实例对象 反射工具 --------

        static object GetTargetObjectOfProperty(SerializedProperty prop) {
            if (prop == null) return null;

#if UNITY_2020_1_OR_NEWER
            // 若是 managed reference，直接取
            if (prop.propertyType == SerializedPropertyType.ManagedReference) {
                return prop.managedReferenceValue;
            }
#endif

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements) {
                if (element.Contains("[")) {
                    string elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Trim('[', ']'));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else {
                    obj = GetValue_Imp(obj, element);
                }

                if (obj == null) return null;
            }

            return obj;
        }

        static object GetValue_Imp(object source, string name) {
            if (source == null) return null;
            var type = source.GetType();

            while (type != null) {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(source);

                var p = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        static object GetValue_Imp(object source, string name, int index) {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) {
                if (!enm.MoveNext()) return null;
            }

            return enm.Current;
        }
    }
#endif

}