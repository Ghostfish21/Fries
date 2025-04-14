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

        public static FacingParams getCustomFacingParams() {
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
        public static Facing getFacing(this Transform transform) {
            var v = getDetailedFacing(transform, FacingParams.defaultFacingParams);
            if (v.isLookingForward) return v.yawFacing;
            return v.pitchFacing;
        }
        
        public static Facing getFacing(this Transform transform, FacingParams param) {
            var v = getDetailedFacing(transform, param);
            if (v.isLookingForward) return v.yawFacing;
            return v.pitchFacing;
        }
        
        public static Facing getRawFacing(this Transform transform) {
            var v = getDetailedFacing(transform, FacingParams.defaultFacingParams);
            return FacingExt.combine(v.yawFacing, v.pitchFacing, v.isLookingForward);
        }

        public static Facing getRawFacing(this Transform transform, FacingParams param) {
            var v = getDetailedFacing(transform, param);
            return FacingExt.combine(v.yawFacing, v.pitchFacing, v.isLookingForward);
        }

        public static (Facing yawFacing, Facing pitchFacing, bool isLookingForward) getDetailedFacing(this Transform transform, FacingParams param) {
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles = eulerAngles.add(param.yawAxis, param.yawOffset).multiply(param.yawAxis, param.yawScale)
                .add(param.pitchAxis, param.pitchOffset).multiply(param.pitchAxis, param.pitchScale);
            
            bool isNeither = false;
            float horizontal = Mathf.Repeat(eulerAngles.get(param.yawAxis), 360);
            float vertical = Mathf.Repeat(eulerAngles.get(param.pitchAxis), 360);

            if (Mathf.Abs(vertical - 180) <= param.neitherRange) isNeither = true;
            
            Facing horizontalFacing = Facing.none;
            if (horizontal is > 315f and <= 360f) horizontalFacing = Facing.north;
            else if (horizontal is >= 0f and <= 45f) horizontalFacing = Facing.north;
            else if (horizontal is > 45f and <= 135f) horizontalFacing = Facing.east;
            else if (horizontal is > 135f and <= 225f) horizontalFacing = Facing.south;
            else if (horizontal is > 225f and <= 315f) horizontalFacing = Facing.west;
            
            Facing verticalFacing = Facing.none;
            if (vertical <= 180) verticalFacing = Facing.up;
            else verticalFacing = Facing.down;

            return (horizontalFacing, verticalFacing, isNeither);
        }
    }
}