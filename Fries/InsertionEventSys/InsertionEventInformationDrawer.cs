namespace Fries.InsertionEventSys {
#if UNITY_EDITOR
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(InsertionEventInformation))]
    public class InsertionEventInformationDrawer : PropertyDrawer {
        private static readonly float Line = EditorGUIUtility.singleLineHeight;
        private static readonly float VPad = EditorGUIUtility.standardVerticalSpacing;
        private static readonly float FoldW = 14f; // 折叠三角宽度
        private static readonly float HPad = 6f; // 水平内边距
        private static readonly Color ArgBg = new Color(0.92f, 0.92f, 0.92f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 读取运行时对象（反射），只读展示
            var info = GetObjectFromProperty(property) as InsertionEventInformation;
            if (info == null) {
                EditorGUI.HelpBox(position, "无法读取 InsertionEventInformation（对象为 null 或未被序列化容器持有）。", MessageType.Info);
                return;
            }

            // 预计算
            var listeners = info.listeners ?? new List<(Type listenFrom, string methodName)>();
            var args = info.argsTypes ?? Array.Empty<Type>();
            string className = NicifyTypeName(info.insertedClass);
            string eventName = info.eventName ?? "<null>";

            // 行 rect 游标
            var r = new Rect(position.x, position.y, position.width, Line);

            // ===== 第一行：Foldout + insertedClass + eventName + Listeners: count =====
            // 折叠按钮可点击；其它区域禁用（只读）
            var foldRect = new Rect(r.x, r.y, FoldW, r.height);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);

            // 三列布局（40% / 40% / 20%）
            float x = r.x + FoldW + HPad;
            float usableW = r.width - FoldW - HPad;
            float col1 = Mathf.Floor(usableW * 0.40f);
            float col2 = Mathf.Floor(usableW * 0.40f);
            float col3 = usableW - col1 - col2;

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUI.LabelField(new Rect(x, r.y, col1, r.height), new GUIContent(className), EditorStyles.label);
                x += col1 + HPad;

                EditorGUI.LabelField(new Rect(x, r.y, col2, r.height), new GUIContent(eventName), EditorStyles.label);
                x += col2 + HPad;

                EditorGUI.LabelField(new Rect(x, r.y, col3, r.height), new GUIContent($"Listeners: {listeners.Count}"),
                    EditorStyles.label);
            }

            // ===== 第二行：灰底参数类型，自动换行 =====
            if (args.Length > 0) {
                r.y += Line + VPad;

                int argLines = CalcArgLineCount(args, position.width - (FoldW + 2 * HPad));
                float argsH = argLines * Line + (argLines - 1) * VPad;

                // 背景
                var bgRect = new Rect(position.x, r.y, position.width, argsH);
                EditorGUI.DrawRect(bgRect, ArgBg);

                // 内容（只读）
                using (new EditorGUI.DisabledScope(true)) {
                    float ax = position.x + HPad;
                    float ay = r.y;
                    float maxW = position.width - 2 * HPad;

                    foreach (var t in args) {
                        string s = NicifyTypeName(t);
                        var size = EditorStyles.miniLabel.CalcSize(new GUIContent(s));
                        float needW = size.x;

                        if (ax + needW > position.x + maxW) {
                            // 换行
                            ax = position.x + HPad;
                            ay += Line + VPad;
                        }

                        EditorGUI.LabelField(new Rect(ax, ay, needW, Line), s, EditorStyles.miniLabel);
                        ax += needW + HPad;
                    }
                }

                r.y += argsH; // 游标下移到参数块末尾
            }

            // ===== 展开后：逐行显示 listener =====
            if (property.isExpanded && listeners.Count > 0) {
                for (int i = 0; i < listeners.Count; i++) {
                    r.y += VPad;
                    r.y += Line * (i == 0 ? 1 : 0); // 第一条从下一标准行开始

                    var (fromType, method) = listeners[i];
                    string fromName = NicifyTypeName(fromType);
                    string methodName = method ?? "<null>";

                    float lx = position.x + FoldW + HPad;
                    float uW = position.width - FoldW - 2 * HPad;
                    float c1 = Mathf.Floor(uW * 0.5f);
                    float c2 = uW - c1;

                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUI.LabelField(new Rect(lx, r.y, c1, Line), fromName, EditorStyles.label);
                        lx += c1 + HPad;
                        EditorGUI.LabelField(new Rect(lx, r.y, c2, Line), methodName, EditorStyles.label);
                    }

                    r.y += Line;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float h = Line; // 第一行

            if (GetObjectFromProperty(property) is InsertionEventInformation info) {
                var args = info.argsTypes ?? Array.Empty<Type>();
                if (args.Length > 0) {
                    float usableW = Mathf.Max(60f, EditorGUIUtility.currentViewWidth - 40f);
                    int lines = CalcArgLineCount(args, usableW - (FoldW + 2 * HPad));
                    h += VPad + lines * Line + (lines - 1) * VPad;
                }

                if (property.isExpanded && info.listeners != null && info.listeners.Count > 0) {
                    // 每个 listener 一行
                    h += VPad + info.listeners.Count * (Line + VPad);
                }
            }

            return h;
        }

        // ========== 工具函数 ==========

        static int CalcArgLineCount(Type[] args, float availableWidth) {
            if (args == null || args.Length == 0) return 0;

            int lines = 1;
            float x = 0f;
            foreach (var t in args) {
                string s = NicifyTypeName(t);
                float w = EditorStyles.miniLabel.CalcSize(new GUIContent(s)).x;

                if (x == 0f) {
                    x = w;
                    continue;
                }

                if (x + HPad + w > availableWidth) {
                    lines++;
                    x = w;
                }
                else {
                    x += HPad + w;
                }
            }

            return Mathf.Max(1, lines);
        }

        static string NicifyTypeName(Type t) {
            if (t == null) return "<None>";
            if (t.IsGenericType) {
                var def = t.Name;
                int tick = def.IndexOf('`');
                if (tick >= 0) def = def.Substring(0, tick);
                var args = t.GetGenericArguments();
                var parts = new string[args.Length];
                for (int i = 0; i < args.Length; i++) parts[i] = NicifyTypeName(args[i]);
                return $"{def}<{string.Join(", ", parts)}>";
            }

            if (t.IsArray) {
                return $"{NicifyTypeName(t.GetElementType())}[]";
            }

            return t.Name;
        }

        // 通过 SerializedProperty.propertyPath 反射到真实对象实例
        static object GetObjectFromProperty(SerializedProperty prop) {
            if (prop == null) return null;
            object obj = prop.serializedObject.targetObject;
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var el in elements) {
                if (el.Contains("[")) {
                    var name = el.Substring(0, el.IndexOf("[", StringComparison.Ordinal));
                    int index = Convert.ToInt32(el.Substring(el.IndexOf("[", StringComparison.Ordinal)).Trim('[', ']'));
                    obj = GetIndexedValue(obj, name, index);
                }
                else {
                    obj = GetMemberValue(obj, el);
                }

                if (obj == null) return null;
            }

            return obj;
        }

        static object GetMemberValue(object source, string name) {
            if (source == null) return null;
            var type = source.GetType();
            // 字段
            var f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) return f.GetValue(source);
            // 属性
            var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null) return p.GetValue(source, null);
            return null;
        }

        static object GetIndexedValue(object source, string name, int index) {
            var enumerable = GetMemberValue(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var e = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) {
                if (!e.MoveNext()) return null;
            }

            return e.Current;
        }
    }
#endif

}