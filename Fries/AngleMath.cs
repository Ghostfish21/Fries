using System;
using UnityEngine;

namespace Fries {
    public static class AngleMath {
        // 获取 Transform 在世界坐标系下计算的 偏航角（Yaw）
        // 该计算返回 Atan2(end.z, end.x) 的结果 的 (0, 360] 的值
        public static float getWorldYaw(this Transform transform) {
            Vector3 endPoint = transform.TransformPoint(1, 0, 0);
            endPoint -= transform.position;
            float f = Mathf.Atan2(endPoint.z, endPoint.x) * Mathf.Rad2Deg;
            if (f > 360) f -= 360;
            else if (f <= 0) f += 360;
            return f;
        }
        
        // 请传入 本地坐标为 1，0，0 的基准点的世界坐标
        // 其余行为和上述方法一致
        public static float getWorldYaw(this Transform transform, Vector3 coordinateWorld) {
            Vector3 endPoint = coordinateWorld;
            endPoint -= transform.position;
            float f = Mathf.Atan2(endPoint.z, endPoint.x) * Mathf.Rad2Deg;
            if (f > 360) f -= 360;
            else if (f <= 0) f += 360;
            return f;
        }
        
        // 从 q 中提取绕指定轴的 twist 分量（轴必须已归一化）
        private static Quaternion extractTwist(this Quaternion q, Vector3 axis) {
            // 确保 axis 单位化
            axis.Normalize();

            // 四元数的向量部分
            Vector3 qVec = new Vector3(q.x, q.y, q.z);

            // 在 axis 上的投影长度
            float proj = Vector3.Dot(qVec, axis);

            // 构造未归一化的 twist 四元数 (w 保留原值，向量部分只保留投影)
            Quaternion twist = new Quaternion(
                axis.x * proj,
                axis.y * proj,
                axis.z * proj,
                q.w
            );
            return twist.normalized;
        }

        // 把任意 Quaternion 分解成 Extrinsic X→Y→Z 顺序的欧拉角 (a,b,c)
        // 返回 Vector3(a, b, c)，分别对应绕 X、Y、Z 的世界坐标下的旋转角度
        public static Vector3 toEulerExtrinsicXYZ(this Quaternion qTotal) {
            // 1) 提取绕 Z 的扭转
            Quaternion qz = qTotal.extractTwist(Vector3.forward);
            // 2) 去除 Z，剩余 r1
            Quaternion r1 = Quaternion.Inverse(qz) * qTotal;

            // 3) 从 r1 中提取绕 Y 的扭转
            Quaternion qy = r1.extractTwist(Vector3.up);
            // 4) 去除 Y，剩余 r2
            Quaternion r2 = Quaternion.Inverse(qy) * r1;

            // 5) 从 r2 中提取绕 X 的扭转
            Quaternion qx = r2.extractTwist(Vector3.right);

            // 6) 读出每个 twist 四元数的角度：φ = 2 * atan2( sin(φ/2), cos(φ/2) )
            float a = 2f * Mathf.Atan2(qx.x, qx.w); // 绕 X 的角度
            float b = 2f * Mathf.Atan2(qy.y, qy.w); // 绕 Y 的角度
            float c = 2f * Mathf.Atan2(qz.z, qz.w); // 绕 Z 的角度

            return new Vector3(a * Mathf.Rad2Deg, b * Mathf.Rad2Deg, c * Mathf.Rad2Deg);
        }

        /// 根据 Extrinsic X→Y→Z 的欧拉角 (a,b,c) 重构 Quaternion
        /// 参数均以弧度为单位
        public static Quaternion fromEulerExtrinsicXYZ(this Vector3 eulerExtrinsicXYZ) {
            // 各轴扭转四元数
            Quaternion qx = new Quaternion(
                Mathf.Sin(eulerExtrinsicXYZ.x * 0.5f), 0f, 0f,
                Mathf.Cos(eulerExtrinsicXYZ.x * 0.5f)
            );
            Quaternion qy = new Quaternion(
                0f, Mathf.Sin(eulerExtrinsicXYZ.y * 0.5f), 0f,
                Mathf.Cos(eulerExtrinsicXYZ.y * 0.5f)
            );
            Quaternion qz = new Quaternion(
                0f, 0f, Mathf.Sin(eulerExtrinsicXYZ.z * 0.5f),
                Mathf.Cos(eulerExtrinsicXYZ.z * 0.5f)
            );

            // Extrinsic X→Y→Z 顺序（右乘先执行 X，再 Y，再 Z）
            return qz * qy * qx;
        }
    }
}