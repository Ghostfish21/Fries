using Fries.CompCache;
using Fries.Data;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    /// <summary>
    /// Stores a local-space AABB (min/max) for a part, and provides helper placement points
    /// on the face centers based on Facing.
    /// 
    /// IMPORTANT: min/max are treated as *local space* bounds relative to this transform.
    /// </summary>
    [TypeTag]
    public class PartBounds : MonoBehaviour {
        
        [SerializeField] private Vector3 min;
        [SerializeField] private Vector3 max;

        private MeshFilter meshFilterCache;

        public Vector3 Min => min;
        public Vector3 Max => max;

        private void Awake() {
            meshFilterCache = GetComponent<MeshFilter>();
            recalcFromMesh();
        }

        private void Reset() {
            meshFilterCache = GetComponent<MeshFilter>();
            recalcFromMesh();
        }

#if UNITY_EDITOR
        private void OnValidate() => recalcFromMesh();
#endif

        private void OnDestroy() { }

        public Vector3 GetFaceCenterLocal(Facing facing) {
            Vector3 lo = Vector3.Min(min, max);
            Vector3 hi = Vector3.Max(min, max);

            float cx = (lo.x + hi.x) * 0.5f;
            float cy = (lo.y + hi.y) * 0.5f;
            float cz = (lo.z + hi.z) * 0.5f;
            
            switch (facing) {
                case Facing.north: return new Vector3(cx, cy, hi.z); // +Z
                case Facing.south: return new Vector3(cx, cy, lo.z); // -Z
                case Facing.east:  return new Vector3(hi.x, cy, cz); // +X
                case Facing.west:  return new Vector3(lo.x, cy, cz); // -X
                case Facing.up:    return new Vector3(cx, hi.y, cz); // +Y
                case Facing.down:  return new Vector3(cx, lo.y, cz); // -Y
                default:           return new Vector3(cx, cy, cz);   // center
            }
        }

        private void recalcFromMesh() {
            if (!meshFilterCache) meshFilterCache = GetComponent<MeshFilter>();
            var mf = meshFilterCache;
            if (!mf || !mf.sharedMesh) return;

            var b = mf.sharedMesh.bounds; // local-space bounds in mesh space (same as transform local for MeshFilter)
            min = b.min;
            max = b.max;
        }

        private void OnDrawGizmos() {
            // Draw the local AABB as an oriented wire cube.
            Vector3 lo = Vector3.Min(min, max);
            Vector3 hi = Vector3.Max(min, max);

            Vector3 localCenter = (lo + hi) * 0.5f;
            Vector3 localSize = hi - lo;

            Gizmos.color = Color.white;
            var old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(localCenter, localSize);
            Gizmos.matrix = old;
        }
        
        public Bounds CalcWorldAabb() {
            Vector3 lo = Vector3.Min(min, max);
            Vector3 hi = Vector3.Max(min, max);

            // 8 corners in local space
            Vector3 c000 = transform.TransformPoint(new Vector3(lo.x, lo.y, lo.z));
            Vector3 c001 = transform.TransformPoint(new Vector3(lo.x, lo.y, hi.z));
            Vector3 c010 = transform.TransformPoint(new Vector3(lo.x, hi.y, lo.z));
            Vector3 c011 = transform.TransformPoint(new Vector3(lo.x, hi.y, hi.z));
            Vector3 c100 = transform.TransformPoint(new Vector3(hi.x, lo.y, lo.z));
            Vector3 c101 = transform.TransformPoint(new Vector3(hi.x, lo.y, hi.z));
            Vector3 c110 = transform.TransformPoint(new Vector3(hi.x, hi.y, lo.z));
            Vector3 c111 = transform.TransformPoint(new Vector3(hi.x, hi.y, hi.z));

            // World-space AABB
            Bounds b = new Bounds(c000, Vector3.zero);
            b.Encapsulate(c001);
            b.Encapsulate(c010);
            b.Encapsulate(c011);
            b.Encapsulate(c100);
            b.Encapsulate(c101);
            b.Encapsulate(c110);
            b.Encapsulate(c111);
            return b;
        }
    }
}
