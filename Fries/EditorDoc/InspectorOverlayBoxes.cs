# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Fries.Fries.EditorDoc {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [InitializeOnLoad]
    public static class InspectorOverlayBoxes {
        // 每个 InspectorWindow 的 overlay 根节点
        private class OverlayRefs {
            public EditorWindow window;
            public VisualElement overlayRootWindowSpace; // 不随滚动
            public VisualElement overlayRootScrollSpace; // 随滚动
            public ScrollView inspectorScrollView; // 可能为空（不同版本层级差异）
            public readonly List<VisualElement> boxes = new List<VisualElement>();
        }

        private static readonly List<OverlayRefs> _overlays = new List<OverlayRefs>();

        static InspectorOverlayBoxes() {
            // 初始化&在编译或层级变化后重建
            EditorApplication.delayCall += RefreshAllInspectors;
            AssemblyReloadEvents.afterAssemblyReload += RefreshAllInspectors;
            EditorApplication.hierarchyChanged += RefreshAllInspectors;
            EditorApplication.projectChanged += RefreshAllInspectors;
            EditorApplication.update += EnsureAlive; // 保活，处理新开的 Inspector
        }

        private static void EnsureAlive() {
            // 如果开了新的 Inspector 或关闭了旧的，这里可检测并刷新
            var wins = FindInspectorWindows();
            if (wins.Count != _overlays.Count || _overlays.Any(o => !wins.Contains(o.window)))
                RefreshAllInspectors();
        }

        public static void ClearBoxes(bool windowSpace = true, bool scrollSpace = true) {
            foreach (var ov in _overlays) {
                if (windowSpace && ov.overlayRootWindowSpace != null) {
                    foreach (var b in ov.boxes.Where(b => b.parent == ov.overlayRootWindowSpace).ToList()) {
                        ov.overlayRootWindowSpace.Remove(b);
                    }
                }

                if (scrollSpace && ov.overlayRootScrollSpace != null) {
                    foreach (var b in ov.boxes.Where(b => b.parent == ov.overlayRootScrollSpace).ToList()) {
                        ov.overlayRootScrollSpace.Remove(b);
                    }
                }

                ov.boxes.RemoveAll(b => b.parent == null);
            }
        }
        
        public static void DrawBox(Rect rect, Color color, float thickness = 2f, bool followScroll = false) {
            foreach (var ov in _overlays) {
                var parent = followScroll ? ov.overlayRootScrollSpace : ov.overlayRootWindowSpace;
                if (parent == null) continue;

                var box = CreateBoxElement(rect, color, thickness);
                parent.Add(box);
                ov.boxes.Add(box);
            }
        }

        private static VisualElement CreateBoxElement(Rect r, Color borderColor, float thickness) {
            var e = new VisualElement {
                pickingMode = PickingMode.Ignore // 不拦截鼠标
            };
            e.style.position = Position.Absolute;
            e.style.left = r.x;
            e.style.top = r.y;
            e.style.width = r.width;
            e.style.height = r.height;
            e.BringToFront();
            e.style.backgroundColor = new Color(0, 0, 0, 0); // 透明背景

            e.style.borderTopWidth = thickness;
            e.style.borderBottomWidth = thickness;
            e.style.borderLeftWidth = thickness;
            e.style.borderRightWidth = thickness;

            e.style.borderTopColor = borderColor;
            e.style.borderBottomColor = borderColor;
            e.style.borderLeftColor = borderColor;
            e.style.borderRightColor = borderColor;

            // 如需半透明填充，可设置 e.style.backgroundColor = new Color(borderColor.r, borderColor.g, borderColor.b, 0.1f);

            return e;
        }

        private static void RefreshAllInspectors() {
            // 清理旧的
            foreach (var ov in _overlays) {
                TryRemoveOverlay(ov);
            }

            _overlays.Clear();

            // 重建
            foreach (var win in FindInspectorWindows()) {
                try {
                    var ov = BuildOverlayForInspector(win);
                    if (ov != null) _overlays.Add(ov);
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }
        }

        private static List<EditorWindow> FindInspectorWindows() {
            // InspectorWindow 是内部类，按名字筛
            return Resources.FindObjectsOfTypeAll<EditorWindow>()
                .Where(w => w && w.GetType().FullName == "UnityEditor.InspectorWindow")
                .ToList();
        }

        private static OverlayRefs BuildOverlayForInspector(EditorWindow win) {
            var root = win.rootVisualElement;
            if (root == null) return null;

            // 1) 不随滚动的 overlay（填满整个窗口）
            var windowOverlay = new VisualElement {
                name = "InspectorOverlay-WindowSpace",
                pickingMode = PickingMode.Ignore
            };
            windowOverlay.style.position = Position.Absolute;
            windowOverlay.style.left = 0;
            windowOverlay.style.top = 0;
            windowOverlay.style.right = 0;
            windowOverlay.style.bottom = 0;
            windowOverlay.BringToFront();
            root.Add(windowOverlay);

            // 2) 随滚动的 overlay（挂在 ScrollView 的 contentContainer 上）
            ScrollView sv = FindScrollView(root);
            VisualElement scrollOverlay = null;
            if (sv != null && sv.contentContainer != null) {
                scrollOverlay = new VisualElement {
                    name = "InspectorOverlay-ScrollSpace",
                    pickingMode = PickingMode.Ignore
                };
                scrollOverlay.style.position = Position.Absolute;
                scrollOverlay.style.left = 0;
                scrollOverlay.style.top = 0;
                scrollOverlay.style.right = 0;
                scrollOverlay.style.bottom = 0;
                scrollOverlay.BringToFront();
                sv.contentContainer.Add(scrollOverlay);
            }

            // 尺寸变化时保证覆盖层填满
            root.RegisterCallback<GeometryChangedEvent>(_ => {
                windowOverlay.style.left = 0; // 这些在绝对填充情况下可不变，但保持回调有助于版本差异
            });

            return new OverlayRefs {
                window = win,
                overlayRootWindowSpace = windowOverlay,
                overlayRootScrollSpace = scrollOverlay,
                inspectorScrollView = sv
            };
        }

        private static ScrollView FindScrollView(VisualElement root) {
            return root.Query<ScrollView>().ToList().FirstOrDefault();
        }

        private static void TryRemoveOverlay(OverlayRefs ov) {
            if (!ov.window) return;
            if (ov.overlayRootWindowSpace != null && ov.overlayRootWindowSpace.parent != null)
                ov.overlayRootWindowSpace.parent.Remove(ov.overlayRootWindowSpace);
            if (ov.overlayRootScrollSpace != null && ov.overlayRootScrollSpace.parent != null)
                ov.overlayRootScrollSpace.parent.Remove(ov.overlayRootScrollSpace);
            ov.boxes.Clear();
        }

        // ==== 示例：菜单测试 ====
        [MenuItem("Tools/Inspector Overlay/Clear Boxes")]
        private static void MenuClear() => ClearBoxes(true, true);

        [MenuItem("Tools/Inspector Overlay/Draw Demo Boxes")]
        private static void MenuDemo() {
            ClearBoxes(true, true);
            // 窗口坐标的框（固定在 Inspector 左上方 20,20）
            DrawBox(new Rect(20, 20, 200, 60), new Color(1f, 0.4f, 0.1f), 2f, followScroll: true);
            // 内容坐标的框（在内容 y=600 处；滚动时跟着走）
            DrawBox(new Rect(16, 600, 260, 80), new Color(0.2f, 0.6f, 1f), 3f, followScroll: true);
        }
    }
}
# endif