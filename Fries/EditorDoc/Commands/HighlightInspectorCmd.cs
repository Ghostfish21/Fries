using System;
using System.Linq;
# if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fries.EditorDoc.Commands {
    public static class HighlightInspectorCmd {
        public static void highlightInspector(string[] args) {
            string sceneName = args[0];
            string gameObjectPath = args[1];
            string componentType = args[2];
            string fieldName = args[3];

            UnityEngine.SceneManagement.Scene
                scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) {
                Debug.LogWarning($"Scene '{sceneName}' not found or not loaded.");
                return;
            }

            // Find the GameObject
            GameObject targetObject = findGameObjectInScene(scene, gameObjectPath);
            if (!targetObject) {
                Debug.LogWarning($"GameObject '{gameObjectPath}' not found in scene '{sceneName}'.");
                return;
            }

            // Select and ping the object
            UnityEditor.Selection.activeGameObject = targetObject;
            UnityEditor.EditorGUIUtility.PingObject(targetObject);

            // If component and field are specified, try to highlight them
            if (!string.IsNullOrEmpty(componentType)) {
                Component component = targetObject.GetComponent(componentType);
                if (component) {
                    UnityEditor.Selection.activeObject = component;
                    Debug.Log($"Selected component: {componentType} on {targetObject.name}");

                    if (string.IsNullOrEmpty(fieldName)) return;
                    Rect? rect = getFieldRect(component, fieldName);
                    if (rect != null) {
                        highlight(rect.Value.x, rect.Value.y, rect.Value.width, rect.Value.height);
                        clearHighlight(2f);
                    }
                    else Debug.LogWarning($"Field '{fieldName}' not found for highlight.");
                }
                else {
                    Debug.LogWarning($"Component '{componentType}' not found on GameObject '{gameObjectPath}'.");
                }
            }
        }

        private static GameObject findGameObjectInScene(UnityEngine.SceneManagement.Scene scene, string path) {
            string[] pathParts = path.Split('/');
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects) {
                if (root.name == pathParts[0]) {
                    GameObject current = root;
                    for (int i = 1; i < pathParts.Length; i++) {
                        Transform child = current.transform.Find(pathParts[i]);
                        if (!child)
                            return null;
                        current = child.gameObject;
                    }

                    return current;
                }
            }

            return null;
        }

        private static Rect? getFieldRect(Component component, string fieldName) {
    string displayName = GetDisplayName(component, fieldName);

    var inspType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
    if (inspType == null) return null;

    foreach (var o in Resources.FindObjectsOfTypeAll(inspType)) {
        var win = (EditorWindow)o;
        if (!win) continue;

        // 仅处理当前 Inspector 正在显示该 component 的窗口
        object tracker = null;
        var prop = inspType.GetProperty("tracker", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null) tracker = prop.GetValue(win);
        if (tracker == null) {
            var field = inspType.GetField("m_Tracker", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null) tracker = field.GetValue(win);
        }
        if (tracker == null) continue;

        var editorsProp = tracker.GetType().GetProperty("activeEditors", BindingFlags.Instance | BindingFlags.Public);
        var editors = editorsProp?.GetValue(tracker) as System.Collections.IEnumerable;
        if (editors == null) continue;

        bool containsComponent = false;
        foreach (Editor ed in editors) {
            if (!ed || ed.target != component) continue;
            containsComponent = true;
            break;
        }
        if (!containsComponent) continue;

        var root = win.rootVisualElement;
        if (root == null) continue;

        // 1) 遍历所有 PropertyField
        var propFields = root.Query<VisualElement>(className: "unity-property-field").ToList();
        foreach (var pf in propFields) {
            // 1a) 首选通过 label 文本匹配（尝试多种类名与兜底）
            var label = FindLabelOnPropertyField(pf);
            if (LabelMatches(label, displayName, fieldName)) {
                return AsLocalRect(pf.worldBound, root);
            }

            // 1b) 兜底1：用 pf.name 匹配（你日志里的 "PropertyField:sm" 通常意味着 pf.name == "sm"）
            string processedName = pf.name;
            if (pf.name.StartsWith("PropertyField:")) processedName = processedName.Substring("PropertyField:".Length);
            if (!string.IsNullOrEmpty(processedName) &&
                (string.Equals(processedName, fieldName, StringComparison.Ordinal) ||
                 string.Equals(processedName, fieldName, StringComparison.OrdinalIgnoreCase))) {
                return AsLocalRect(pf.worldBound, root);
            }

            // 1c) 兜底2：通过反射拿到绑定的 SerializedProperty，再比对 sp.name / sp.displayName
            var sp = TryGetBoundSerializedProperty(pf);
            if (sp != null) {
                if (TextMatches(sp.displayName, displayName, fieldName) ||
                    string.Equals(sp.name, fieldName, StringComparison.Ordinal) ||
                    string.Equals(sp.name, fieldName, StringComparison.OrdinalIgnoreCase)) {
                    return AsLocalRect(pf.worldBound, root);
                }
            }
        }

        // 2) 若 PropertyField 失败，再广撒网到 BaseField（有些自绘/自定义控件）
        var baseFields = root.Query<VisualElement>(className: "unity-base-field").ToList();
        foreach (var bf in baseFields) {
            var label = FindLabelOnBaseField(bf);
            if (LabelMatches(label, displayName, fieldName)) {
                return AsLocalRect(bf.worldBound, root);
            }
        }

        // 3) 最后兜底：找任意 Label/TextElement 文本命中，再向上找最近的容器
        var allText = root.Query<TextElement>().ToList();
        foreach (var te in allText) {
            if (!TextMatches(te?.text, displayName, fieldName)) continue;
            var container = FindAncestorWithClass(te, new[] { "unity-property-field", "unity-base-field" }) ?? te;
            return AsLocalRect(container.worldBound, root);
        }
    }

    return null;

    // ---------- 本地函数 ----------
    static string GetDisplayName(Component comp, string path) {
        try {
            var so = new SerializedObject(comp);
            var sp = so.FindProperty(path);
            if (sp != null) return sp.displayName;
        } catch { /* ignore */ }
        return ObjectNames.NicifyVariableName(path);
    }

    static Rect AsLocalRect(Rect world, VisualElement root) {
        world.position -= root.worldBound.position;
        return world;
    }

    static bool LabelMatches(Label label, string displayName, string rawFieldName) {
        if (label == null) return false;
        return TextMatches(label.text, displayName, rawFieldName);
    }

    static bool TextMatches(string text, string displayName, string rawFieldName) {
        if (string.IsNullOrEmpty(text)) return false;
        if (text == displayName) return true;
        if (string.Equals(text, displayName, StringComparison.OrdinalIgnoreCase)) return true;
        if (text.Trim() == displayName.Trim()) return true;
        if (text == ObjectNames.NicifyVariableName(rawFieldName)) return true;
        if (text.TrimEnd(':') == displayName.TrimEnd(':')) return true;
        return false;
    }

    static Label FindLabelOnPropertyField(VisualElement pf) {
        // 尝试多种常见类名
        return  pf.Q<Label>(className: "unity-property-field__label")
             ?? pf.Q<Label>(className: "unity-base-field__label")
             ?? pf.Q<Label>(className: "unity-label")
             ?? pf.Q<Label>(); // 兜底：第一个 Label
    }

    static Label FindLabelOnBaseField(VisualElement bf) {
        return  bf.Q<Label>(className: "unity-base-field__label")
             ?? bf.Q<Label>(className: "unity-label")
             ?? bf.Q<Label>();
    }

    static VisualElement FindAncestorWithClass(VisualElement start, string[] classNames) {
        var ve = start;
        while (ve != null) {
            foreach (var cls in classNames) {
                if (ve.ClassListContains(cls)) return ve;
            }
            ve = ve.parent;
        }
        return null;
    }

    static SerializedProperty TryGetBoundSerializedProperty(VisualElement pf) {
        // 许多版本里 PropertyField 持有一个私有 m_Property 字段
        try {
            var t = pf.GetType();
            var fi = t.GetField("m_Property", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null) {
                return fi.GetValue(pf) as SerializedProperty;
            }
            // 还有些版本有 "property" 的内部属性
            var pi = t.GetProperty("property", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (pi != null) {
                return pi.GetValue(pf) as SerializedProperty;
            }
        } catch { /* ignore */ }
        return null;
    }
}


        private static void highlight(float x, float y, float width, float height) {
            InspectorOverlayBoxes.ClearBoxes(true, true);
            InspectorOverlayBoxes.DrawBox(new Rect(x, y, width, height), new Color(1f, 0.4f, 0.1f), 3f,
                followScroll: true);
        }

        private static void clearHighlight(float delay) {
            if (delay <= 0f) {
                InspectorOverlayBoxes.ClearBoxes(true, true);
                return;
            }

            double endTime = EditorApplication.timeSinceStartup + delay;
            EditorApplication.CallbackFunction callback = null;
            callback = () => {
                if (!(EditorApplication.timeSinceStartup >= endTime)) return;
                InspectorOverlayBoxes.ClearBoxes(true, true);
                EditorApplication.update -= callback;
            };
            EditorApplication.update += callback;
        }
    }
}
# endif