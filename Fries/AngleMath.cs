using System;
using UnityEngine;

namespace Fries {
    public static class AngleMath {
        public static float clampAngle360(this float angle) {
            if (angle > 360) angle -= 360;
            else if (angle <= 0) angle += 360;
            return angle;
        }
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
            // ——1) 提取本地 Y（Yaw）——
            // 本地 Y 轴在世界空间下的方向：
            Vector3 axisYaw = qTotal * Vector3.up;
            // 在这个方向上的 twist，就是 yaw
            Quaternion qYaw = qTotal.extractTwist(axisYaw);

            // 去掉 yaw 后的剩余旋转
            Quaternion rem1 = Quaternion.Inverse(qYaw) * qTotal;

            // ——2) 提取本地 X（Pitch）——
            // 本地 X 轴，此时已经被 yaw 旋了：
            Vector3 axisPitch = qYaw * Vector3.right;
            Quaternion qPitch = rem1.extractTwist(axisPitch);

            // 去掉 pitch 后的剩余
            Quaternion rem2 = Quaternion.Inverse(qPitch) * rem1;

            // ——3) 提取本地 Z（Roll）——
            // 本地 Z 轴，此时被 yaw+pitch 旋了：
            Vector3 axisRoll = (qYaw * qPitch) * Vector3.forward;
            Quaternion qRoll = rem2.extractTwist(axisRoll);

            // 读出各自的带符号角度：φ = 2 * atan2( sin(φ/2), cos(φ/2) )
            float yaw   = 2f * Mathf.Atan2(qYaw.y,   qYaw.w);
            float pitch = 2f * Mathf.Atan2(qPitch.x, qPitch.w);
            float roll  = 2f * Mathf.Atan2(qRoll.z,  qRoll.w);

            // 注意 Unity Inspector 的顺序是 (X=pitch, Y=yaw, Z=roll)
            return new Vector3((pitch * Mathf.Rad2Deg).clampAngle360(), (yaw * Mathf.Rad2Deg).clampAngle360(),
                (roll * Mathf.Rad2Deg).clampAngle360());
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