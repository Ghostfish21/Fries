using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Fries.FbxFunctions.ModelRecognition {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class ModelFingerprint : MonoBehaviour {
        public string FingerPrint;
        public Vector3 fingerPrintVector { get; private set; }
        
        private Vector3[] fingerprint;
        private bool fingerprintFound = false;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        
        public void Reset() {
            if (!meshFilter) {
                meshFilter = GetComponent<MeshFilter>();
                meshRenderer = GetComponent<MeshRenderer>();
            }

            findFingerprint();
            calcFingerprint();
        }

        private void OnValidate() {
            Reset();
        }

        private void calcFingerprint() {
            try {
                float dist120 = (fingerprint[1] - fingerprint[0]).magnitude;
                float dist220 = (fingerprint[2] - fingerprint[0]).magnitude;
                float dist320 = (fingerprint[3] - fingerprint[0]).magnitude;
                FingerPrint = $"{dist120 / dist220},{dist220 / dist320},{dist320 / dist120}";
                fingerPrintVector = new Vector3(dist120 / dist220, dist220 / dist320, dist320 / dist120);
            }
            catch (Exception) {
                Debug.LogError("The script is not initialized yet, please run reset first!");
            }
        }

        // 在模型中查找指纹
        public void findFingerprint() {
            if (meshFilter == null || meshRenderer == null) {
                Debug.LogError("MeshFilter or MeshRenderer is null");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null) {
                Debug.LogError("Unable to get mesh, it is null");
                return;
            }

            // 获取顶点和三角形
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = transform.TransformPoint(vertices[i]);
            // 构建边的连接关系
            int[] triangles = mesh.triangles;
            Dictionary<int, List<int>> vertexConnections = buildVertexConnections(vertices, triangles);

            // 查找孤立顶点
            List<int> isolatedVertexIndices = new();
            for (int i = 0; i < vertices.Length; i++) {
                if (!vertexConnections.ContainsKey(i))
                    isolatedVertexIndices.Add(i);
                else if (vertexConnections[i].Count == 0)
                    isolatedVertexIndices.Add(i);
            }

            if (isolatedVertexIndices.Count != 4) {
                Debug.LogError("The mesh doesn't have exactly 4 isolated vertices, could not recreate fingerprint!");
                return;
            }

            // 推算中心点坐标
            int centerVertexIndex = -1;
            // 我们需要找到一个点，使得它与其他三个点的连线互相垂直
            foreach (int potentialCenterIndex in isolatedVertexIndices) {
                // 获取除了潜在中心点外的其他三个点
                List<int> otherVertices = isolatedVertexIndices
                    .Where(idx => idx != potentialCenterIndex)
                    .ToList();

                if (otherVertices.Count != 3) continue; // 确保有且只有三个其他点

                // 计算从潜在中心点到其他三个点的向量
                Vector3 v1 = vertices[otherVertices[0]] - vertices[potentialCenterIndex];
                Vector3 v2 = vertices[otherVertices[1]] - vertices[potentialCenterIndex];
                Vector3 v3 = vertices[otherVertices[2]] - vertices[potentialCenterIndex];

                // 归一化向量
                v1.Normalize();
                v2.Normalize();
                v3.Normalize();

                // 检查这三个向量是否互相垂直（点积接近于0）
                // 由于浮点数精度问题，使用一个小的阈值
                float threshold = 0.1f;
                float dot12 = Mathf.Abs(Vector3.Dot(v1, v2));
                float dot13 = Mathf.Abs(Vector3.Dot(v1, v3));
                float dot23 = Mathf.Abs(Vector3.Dot(v2, v3));

                // 如果三个向量互相垂直，则找到了中心点
                if (dot12 < threshold && dot13 < threshold && dot23 < threshold) {
                    centerVertexIndex = potentialCenterIndex;
                    break;
                }
            }

            if (centerVertexIndex == -1) {
                Debug.LogError("Center Point is not found, could not recreate fingerprint!");
                return;
            }

            // 创建指纹
            Vector3 center = vertices[centerVertexIndex];
            List<Vector3> endpoints = new List<Vector3>();
            for (int i = 0; i < isolatedVertexIndices.Count; i++) {
                if (centerVertexIndex == isolatedVertexIndices[i]) continue;
                endpoints.Add(vertices[isolatedVertexIndices[i]]);
            }

            // 根据a, b, c的值确定哪个点是X, Y, Z基准点
            Vector3[] fingerprint = new Vector3[4];
            fingerprint[0] = center; // 中心点

            // 计算三个端点到中心点的距离
            float[] distances = new float[3];
            for (int i = 0; i < 3; i++) {
                distances[i] = Vector3.Distance(endpoints[i], center);
            }

            float c(float o, float ct) {
                if (o == ct) return 1000000;
                return o;
            }

            float c2(float o, float ct1, float ct2) {
                if (o == ct1 || o == ct2) return 1000000;
                return o;
            }

            float shortest = Mathf.Min(distances[0], Mathf.Min(distances[1], distances[2]));
            int estIndex = -1;
            for (int i = 0; i < distances.Length; i++) {
                if (distances[i] == shortest) estIndex = i;
            }

            float sndShortest = Mathf.Min(c(distances[0], shortest),
                Mathf.Min(c(distances[1], shortest),
                    c(distances[2], shortest)));
            int sndIndex = -1;
            for (int i = 0; i < distances.Length; i++) {
                if (distances[i] == sndShortest) sndIndex = i;
            }

            float thirdShortest = Mathf.Min(c2(distances[0], shortest, sndShortest),
                Mathf.Min(c2(distances[1], shortest, sndShortest),
                    c2(distances[2], shortest, sndShortest)));
            int trdIndex = -1;
            for (int i = 0; i < distances.Length; i++) {
                if (distances[i] == thirdShortest) trdIndex = i;
            }

            // 按照距离分配X, Y, Z基准点
            fingerprint[1] = endpoints[estIndex];
            fingerprint[2] = endpoints[sndIndex];
            fingerprint[3] = endpoints[trdIndex];

            fingerprintFound = true;
            this.fingerprint = fingerprint;
        }

        // 构建顶点的连接关系
        private Dictionary<int, List<int>> buildVertexConnections(Vector3[] vertices, int[] triangles) {
            Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();

            void AddConnection(int v1, int v2) {
                if (!connections[v1].Contains(v2)) connections[v1].Add(v2);
                if (!connections[v2].Contains(v1)) connections[v2].Add(v1);
            }

            // 初始化连接列表
            for (int i = 0; i < vertices.Length; i++) {
                connections[i] = new List<int>();
            }

            // 从三角形中提取边
            for (int i = 0; i < triangles.Length; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                // 添加边的连接关系（避免重复添加）
                AddConnection(v1, v2);
                AddConnection(v2, v3);
                AddConnection(v3, v1);
            }

            return connections;
        }

        // 重建变换参数
        public Vector3[] recreateTransform(ModelFingerprint distFingerprint) {
            Vector3[] dstFingerprint = distFingerprint.fingerprint;
            if (fingerprint == null || dstFingerprint == null ||
                fingerprint.Length != 4 || dstFingerprint.Length != 4) {
                Debug.LogError("Invalid fingerprint format!");
                return null;
            }

            // 提取源指纹和目标指纹的中心点和基准点
            Vector3 srcCenter = fingerprint[0];
            Vector3 srcX = fingerprint[1];
            Vector3 srcY = fingerprint[2];
            Vector3 srcZ = fingerprint[3];

            Vector3 dstCenter = dstFingerprint[0];
            Vector3 dstX = dstFingerprint[1];
            Vector3 dstY = dstFingerprint[2];
            Vector3 dstZ = dstFingerprint[3];

            // 计算源指纹的边长
            float srcLengthX = Vector3.Distance(srcCenter, srcX);
            float srcLengthY = Vector3.Distance(srcCenter, srcY);
            float srcLengthZ = Vector3.Distance(srcCenter, srcZ);

            // 计算目标指纹的边长
            float dstLengthX = Vector3.Distance(dstCenter, dstX);
            float dstLengthY = Vector3.Distance(dstCenter, dstY);
            float dstLengthZ = Vector3.Distance(dstCenter, dstZ);

            // 计算比例关系，检查是否为同一指纹
            float ratioXY_src = srcLengthX / srcLengthY;
            float ratioYZ_src = srcLengthY / srcLengthZ;
            float ratioZX_src = srcLengthZ / srcLengthX;

            float ratioXY_dst = dstLengthX / dstLengthY;
            float ratioYZ_dst = dstLengthY / dstLengthZ;
            float ratioZX_dst = dstLengthZ / dstLengthX;

            // 检查比例是否匹配（考虑浮点误差）
            float tolerance = 0.01f;
            if (Mathf.Abs(ratioXY_src - ratioXY_dst) > tolerance ||
                Mathf.Abs(ratioYZ_src - ratioYZ_dst) > tolerance ||
                Mathf.Abs(ratioZX_src - ratioZX_dst) > tolerance) {
                Debug.LogWarning("Fingerprint not match!");
                return null;
            }

            // 计算变换参数
            // 1. 平移 - 中心点的位移
            Vector3 translation = dstCenter - srcCenter;

            // 2. 缩放 - 使用边长的平均比例
            float scaleRatio = (dstLengthX / srcLengthX + dstLengthY / srcLengthY + dstLengthZ / srcLengthZ) / 3f;

            Vector3[] src = new[] { srcCenter, srcX, srcY, srcZ };
            Vector3[] dist = new[] { dstCenter, dstX, dstY, dstZ };
            Vector3 eulerAngles =
                (computeRotationEuler(src, dist) * Quaternion.Euler(transform.eulerAngles)).eulerAngles -
                transform.eulerAngles;

            // 返回变换参数：平移、旋转、缩放
            Vector3[] transformParams = new Vector3[3];
            transformParams[0] = translation;
            transformParams[1] = eulerAngles;
            transformParams[2] = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            return transformParams;
        }

        public static Quaternion computeRotationEuler(Vector3[] srcPoints, Vector3[] dstPoints) {
            if (srcPoints.Length != 4 || dstPoints.Length != 4)
                throw new System.ArgumentException("点数组长度必须为4");

            // 1) 提取中心和两个基准向量
            Vector3 srcCenter = srcPoints[0];
            Vector3 dstCenter = dstPoints[0];

            Vector3 v1_src = srcPoints[1] - srcCenter;
            Vector3 v2_src = srcPoints[2] - srcCenter;

            Vector3 v1_dst = dstPoints[1] - dstCenter;
            Vector3 v2_dst = dstPoints[2] - dstCenter;

            // 2) 第一步：将 v1_src 对齐到 v1_dst
            Quaternion q1 = Quaternion.FromToRotation(v1_src, v1_dst);

            // 3) 把第一步的旋转应用到 v2_src 上
            Vector3 v2_src_rot = q1 * v2_src;

            // 4) 第二步：将旋转后的 v2_src_rot 对齐到 v2_dst
            Quaternion q2 = Quaternion.FromToRotation(v2_src_rot, v2_dst);

            // 5) 合成旋转：先 q1 再 q2
            Quaternion qTotal = q2 * q1;
            return qTotal;
        }

        // 用于在场景中可视化指纹
        private void OnDrawGizmos() {
            if (!fingerprintFound || fingerprint == null || fingerprint.Length != 4)
                return;

            // 绘制中心点
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(fingerprint[0], 0.05f);

            // 绘制X基准点和连接线
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(fingerprint[1], 0.03f);
            Gizmos.DrawLine(fingerprint[0], fingerprint[1]);

            // 绘制Y基准点和连接线
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(fingerprint[2], 0.03f);
            Gizmos.DrawLine(fingerprint[0], fingerprint[2]);

            // 绘制Z基准点和连接线
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(fingerprint[3], 0.03f);
            Gizmos.DrawLine(fingerprint[0], fingerprint[3]);
        }
    }
}