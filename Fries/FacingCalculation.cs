using Fries.Data;
using UnityEngine;

namespace Fries {
    public class FacingParams {
        public static FacingParams defaultFacingParams = new() {
            yawAxis = Axis.y,
            pitchAxis = Axis.x,
            pitchOffset = 0,
            yawOffset = 0,
            yawScale = 1,
            pitchScale = 1,
            neitherRange = 45
        };

        public static FacingParams GetCustomFacingParams() {
            return new FacingParams {
                yawAxis = Axis.y,
                pitchAxis = Axis.x,
                pitchOffset = 0,
                yawOffset = 0,
                yawScale = 1,
                pitchScale = 1,
                neitherRange = 45
            };
        }

        public Axis yawAxis;
        public float yawOffset;
        public float yawScale;
        
        public Axis pitchAxis;
        public float pitchOffset;
        public float pitchScale;

        public float neitherRange;
    }
    
    public static class FacingCalculation {
        public static Facing GetFacing(this Transform transform) {
            var v = GetDetailedFacing(transform, FacingParams.defaultFacingParams);
            if (v.isLookingForward) return v.yawFacing;
            return v.pitchFacing;
        }
        public static Facing GetFacing(this Transform transform, out Facing horizontalFacing) {
            var v = GetDetailedFacing(transform, FacingParams.defaultFacingParams);
            horizontalFacing = v.yawFacing;
            if (v.isLookingForward) return v.yawFacing;
            return v.pitchFacing;
        }
        
        public static Facing GetFacing(this Transform transform, FacingParams param) {
            var v = GetDetailedFacing(transform, param);
            if (v.isLookingForward) return v.yawFacing;
            return v.pitchFacing;
        }
        
        public static Facing GetRawFacing(this Transform transform) {
            var v = GetDetailedFacing(transform, FacingParams.defaultFacingParams);
            return FacingExt.Combine(v.yawFacing, v.pitchFacing, v.isLookingForward);
        }

        public static Facing GetRawFacing(this Transform transform, FacingParams param) {
            var v = GetDetailedFacing(transform, param);
            return FacingExt.Combine(v.yawFacing, v.pitchFacing, v.isLookingForward);
        }

        static Vector3 AxisDir(Axis a) => a switch {
            Axis.x => Vector3.right,
            Axis.y => Vector3.up,
            _      => Vector3.forward
        };

        public static (Facing yawFacing, Facing pitchFacing, bool isLookingForward)
            GetDetailedFacing(this Transform transform, FacingParams param) {

            static Vector3 AxisDir(Axis a) => a switch {
                Axis.x => Vector3.right,
                Axis.y => Vector3.up,
                _      => Vector3.forward
            };

            Vector3 upAxis = AxisDir(param.yawAxis);
            Vector3 fwd = transform.forward.normalized;

            // ----- pitch（稳定：用 forward 的“仰角”）-----
            // elevation: [-90, 90]，0 = 水平看向前方
            float pitch = Mathf.Asin(Mathf.Clamp(Vector3.Dot(fwd, upAxis), -1f, 1f)) * Mathf.Rad2Deg;
            pitch = (pitch + param.pitchOffset) * param.pitchScale;

            bool isLookingForward = Mathf.Abs(pitch) <= param.neitherRange;
            Facing pitchFacing = pitch >= 0f ? Facing.up : Facing.down;

            // ----- yaw（稳定：把 forward 投影到“地平面”后算方位角）-----
            Vector3 flat = Vector3.ProjectOnPlane(fwd, upAxis);
            float yaw = 0f;

            if (flat.sqrMagnitude > 1e-8f) {
                flat.Normalize();

                // 让 yaw=0 对齐世界 +Z（跟你原来 eulerAngles.y 的 0° 一致）
                Vector3 refFwd = AxisDir(Axis.z);
                if (Mathf.Abs(Vector3.Dot(refFwd, upAxis)) > 0.999f) refFwd = AxisDir(Axis.x);
                refFwd = Vector3.ProjectOnPlane(refFwd, upAxis).normalized;

                yaw = Vector3.SignedAngle(refFwd, flat, upAxis);
            }

            yaw = Mathf.Repeat((yaw + param.yawOffset) * param.yawScale, 360f);

            Facing yawFacing;
            if (yaw > 315f || yaw <= 45f) yawFacing = Facing.north;
            else if (yaw <= 135f)        yawFacing = Facing.east;
            else if (yaw <= 225f)        yawFacing = Facing.south;
            else                         yawFacing = Facing.west;

            return (yawFacing, pitchFacing, isLookingForward);
        }

    }
}