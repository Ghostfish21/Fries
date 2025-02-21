namespace Fries.FbxFunctions.FbxSnapshots {
    # if UNITY_EDITOR
    using UnityEditor;
    # endif
    using UnityEngine;

    public enum StandardView {
        Front,  // 正视图
        Back,   // 反视图
        Left,   // 左侧视图
        Right,  // 右侧视图
        Top,    // 顶视图
        Bottom  // 底视图
    }
    
    public static class SceneRotationUtil {
        # if UNITY_EDITOR
        /// <summary>
        /// 将传入的 SceneView 调整到指定标准视角
        /// </summary>
        /// <param name="sceneView">目标 SceneView</param>
        /// <param name="view">标准视角类型</param>
        public static void setSceneViewStandardView(SceneView sceneView, StandardView view) {
            if (sceneView == null)
                return;

            Quaternion rotation = Quaternion.identity;
            switch(view) {
                case StandardView.Front:
                    // 正视图：从物体正面看（通常从 Z 负方向看物体）
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case StandardView.Back:
                    // 反视图
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case StandardView.Left:
                    // 左侧视图
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case StandardView.Right:
                    // 右侧视图
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case StandardView.Top:
                    // 顶视图
                    rotation = Quaternion.Euler(90, 0, 0);
                    break;
                case StandardView.Bottom:
                    // 底视图
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
            }

            // 使用 LookAtDirect 方法设置摄像机的旋转角度，同时保持当前 pivot 和 size
            sceneView.LookAtDirect(sceneView.pivot, rotation, sceneView.size);
            sceneView.Repaint();
        }
        # endif
    }

}