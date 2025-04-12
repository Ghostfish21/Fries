
using System;
using UnityEngine;

namespace Fries.Data {
    public enum Axis {
        x, y, z
    }
    public static class AxisExt {
        public static float get(this Vector2 vector2, Axis axis) {
            switch (axis) {
                case Axis.x:
                    return vector2.x;
                case Axis.y:
                    return vector2.y;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static Vector2 add(this Vector2 vector2, Axis axis, float add) {
            switch (axis) {
                case Axis.x:
                    vector2.x += add;
                    return vector2;
                case Axis.y:
                    vector2.y += add;
                    return vector2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static Vector2 multiply(this Vector2 vector2, Axis axis, float by) {
            switch (axis) {
                case Axis.x:
                    vector2.x *= by;
                    return vector2;
                case Axis.y:
                    vector2.y *= by;
                    return vector2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static float get(this Vector3 vector3, Axis axis) {
            switch (axis) {
                case Axis.x:
                    return vector3.x;
                case Axis.y:
                    return vector3.y;
                case Axis.z:
                    return vector3.z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static Vector3 add(this Vector3 vector3, Axis axis, float add) {
            switch (axis) {
                case Axis.x:
                    vector3.x += add;
                    break;
                case Axis.y:
                    vector3.y += add;
                    break;
                case Axis.z:
                    vector3.z += add;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            return vector3;
        }
        
        public static Vector3 multiply(this Vector3 vector3, Axis axis, float by) {
            switch (axis) {
                case Axis.x:
                    vector3.x *= by;
                    break;
                case Axis.y:
                    vector3.y *= by;
                    break;
                case Axis.z:
                    vector3.z *= by;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
            
            return vector3;
        }
        
        public static float getPos(this Transform transform, Axis axis) {
            return transform.position.get(axis);
        }

        public static float getLocalPos(this Transform transform, Axis axis) {
            return transform.localPosition.get(axis);
        }

        public static float getEulerAngle(this Transform transform, Axis axis) {
            return transform.eulerAngles.get(axis);
        }

        public static float getLocalEulerAngle(this Transform transform, Axis axis) {
            return transform.localEulerAngles.get(axis);
        }

        public static float getScale(this Transform transform, Axis axis) {
            return transform.localScale.get(axis);
        }
    }
}