using System;
using UnityEngine;

namespace Fries {
    public static class AngleMath {
        // 获取 Transform 在世界坐标系下计算的 偏航角（Yaw）
        // 该计算返回 Atan2(end.z, end.x) 的结果 的 (0, 360] 的值
        public static float getWorldYaw(this Transform transform) {
            Vector3 endPoint = transform.TransformPoint(1, 0, 0);
            float f = Mathf.Atan2(endPoint.z, endPoint.x) * Mathf.Rad2Deg;
            if (f > 360) f -= 360;
            else if (f <= 0) f += 360;
            return f;
        }
    }
}